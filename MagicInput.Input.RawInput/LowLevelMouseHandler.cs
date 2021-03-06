﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	class LowLevelMouseHandler : IDisposable
	{
		delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

		[SuppressUnmanagedCodeSecurity, DllImport("user32", SetLastError = true)]
		static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);
		[SuppressUnmanagedCodeSecurity, DllImport("user32")]
		static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", SetLastError = true)]
		static extern bool UnhookWindowsHookEx(IntPtr hhk);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", SetLastError = true)]
		static extern bool GetCursorPos(out Int32Point lpPoint);
		[SuppressUnmanagedCodeSecurity, DllImport("user32", SetLastError = true)]
		static extern bool SetCursorPos(int x, int y);

		const int WM_INPUT_DEVICE_CHANGE = 0xFE;
		const int WM_INPUT = 0xFF;
		const int WH_MOUSE_LL = 14;

		[StructLayout(LayoutKind.Sequential)]
		struct Int32Point
		{
			public int X;
			public int Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct LowLevelMouseParameters
		{
			public int X;
			public int Y;
			public short MouseDataLow;
			public short MouseDataHigh;
			public EventInjectedFlags Flags;
			public int Timestamp;
			public IntPtr ExtraInformation;
		}

		[Flags]
		enum EventInjectedFlags
		{
			None,
			Injected,
			LowerIntegrityLevelInjected
		}

		enum MouseMessage
		{
			MouseMove = 0x0200,
			LeftButtonDown = 0x0201,
			LeftButtonUp = 0x0202,
			RightButtonDown = 0x0204,
			RightButtonUp = 0x0205,
			MiddleButtonDown = 0x0207,
			MiddleButtonUp = 0x0208,
			MouseWheel = 0x020A,
			XButtonDown = 0x020B,
			XButtonUp = 0x020C,
			MouseHorizontalWheel = 0x020E
		}

		readonly IntPtr hHook;
		readonly HookProc hookDelegate;
		readonly Thread readThread;
		readonly BlockingCollection<(TaskCompletionSource<RawInputData> result, Func<RawInputData, bool> matches)> readRequest = new BlockingCollection<(TaskCompletionSource<RawInputData> result, Func<RawInputData, bool> matches)>();
		static Int32Point? prevPos;

		public event EventHandler<RawInputEventArgs> PreviewInput;
		public event EventHandler<RawInputEventArgs> Input;

		public LowLevelMouseHandler()
		{
			hHook = SetWindowsHookEx(WH_MOUSE_LL, hookDelegate = HookHandler, IntPtr.Zero, 0);

			if (hHook == IntPtr.Zero)
				throw new Win32Exception();

			readThread = new Thread(ReadThread) { IsBackground = true };
			readThread.Start();
		}

		int HookHandler(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode < 0)
				return CallNextHookEx(hHook, nCode, wParam, lParam);

			var msg = (MouseMessage)wParam.ToInt32();
			var data = Marshal.PtrToStructure<LowLevelMouseParameters>(lParam);

			if (data.Flags == EventInjectedFlags.None)
			{
				var rid = ReadInput(5, i =>
					i.Type == RawInputDeviceType.Mouse &&
					i.DeviceHandle != IntPtr.Zero &&
					FlagMatches(i.Mouse, msg, data));

				if (rid != null)
				{
					var e = new RawInputEventArgs(rid);

					OnPreviewInput(e);

					if (!e.Handled && OnInput(rid))
					{
						if (msg == MouseMessage.MouseMove && prevPos is Int32Point pt)
							SetCursorPos(pt.X, pt.Y);

						return 1;
					}
				}
			}

			{
				GetCursorPos(out var pt);
				prevPos = pt;
			}

			return CallNextHookEx(hHook, nCode, wParam, lParam);
		}

		protected virtual void OnPreviewInput(RawInputEventArgs e) =>
			PreviewInput?.Invoke(this, e);

		protected virtual void OnInput(RawInputEventArgs e) =>
			Input?.Invoke(this, e);

		bool OnInput(RawInputData input)
		{
			var e = new RawInputEventArgs(input);

			OnInput(e);

			return e.Handled;
		}

		void ReadThread()
		{
			using (var f = new ReceiverWindow())
			{
				RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, f.Handle);

				try
				{
					var buffer = new List<(DateTime timeout, RawInputData rid)>();
					var newBuffer = new List<(DateTime timeout, RawInputData rid)>();

					while (true)
					{
						buffer.AddRange(RawInputData.GetBufferedData().Select(i => (DateTime.Now.AddMilliseconds(10), i)));

						foreach (var i in buffer)
							if (i.timeout > DateTime.Now)
							{
								var items = new List<(TaskCompletionSource<RawInputData> result, Func<RawInputData, bool> matches)>();

								while (readRequest.TryTake(out var item))
								{
									if (item.matches(i.rid))
										item.result.TrySetResult(i.rid);
									else
										newBuffer.Add(i);

									if (item.result.Task.IsCanceled)
										items.Add(item);
								}

								foreach (var j in items)
									readRequest.Add(j);
							}

						(buffer, newBuffer) = (newBuffer, buffer);
						newBuffer.Clear();
						Thread.Sleep(1);
					}
				}
				finally
				{
					RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);
				}
			}
		}

		RawInputData ReadInput(int millisecondsTimeout, Func<RawInputData, bool> matches)
		{
			using (var cts = new CancellationTokenSource())
			{
				var tcs = new TaskCompletionSource<RawInputData>();

				cts.Token.Register(() => tcs.TrySetCanceled());
				cts.CancelAfter(millisecondsTimeout);
				readRequest.Add((tcs, matches));

				return tcs.Task.ContinueWith(t => t.IsCanceled ? null : t.Result).Result;
			}
		}

		static bool FlagMatches(RawMouse raw, MouseMessage msg, LowLevelMouseParameters data)
		{
			switch (msg)
			{
				case MouseMessage.MouseMove:
					return raw.LastX != 0
						|| raw.LastY != 0;
				case MouseMessage.LeftButtonDown:
					return (raw.Buttons & RawMouseButtonFlags.LeftButtonDown) != 0;
				case MouseMessage.LeftButtonUp:
					return (raw.Buttons & RawMouseButtonFlags.LeftButtonUp) != 0;
				case MouseMessage.RightButtonDown:
					return (raw.Buttons & RawMouseButtonFlags.RightButtonDown) != 0;
				case MouseMessage.RightButtonUp:
					return (raw.Buttons & RawMouseButtonFlags.RightButtonUp) != 0;
				case MouseMessage.MiddleButtonDown:
					return (raw.Buttons & RawMouseButtonFlags.MiddleButtonDown) != 0;
				case MouseMessage.MiddleButtonUp:
					return (raw.Buttons & RawMouseButtonFlags.MiddleButtonUp) != 0;
				case MouseMessage.MouseWheel:
					return (raw.Buttons & RawMouseButtonFlags.MouseWheel) != 0
						&& raw.ButtonData == data.MouseDataHigh;
				case MouseMessage.XButtonDown:
					return (raw.Buttons & (data.MouseDataHigh == 1 ? RawMouseButtonFlags.Button4Down : RawMouseButtonFlags.Button5Down)) != 0;
				case MouseMessage.XButtonUp:
					return (raw.Buttons & (data.MouseDataHigh == 1 ? RawMouseButtonFlags.Button4Up : RawMouseButtonFlags.Button5Up)) != 0;
				case MouseMessage.MouseHorizontalWheel:
					return (raw.Buttons & RawMouseButtonFlags.MouseHorizontalWheel) != 0
						&& raw.ButtonData == data.MouseDataHigh;
				default:
					return false;
			}
		}

		~LowLevelMouseHandler() =>
			Dispose();

		public void Dispose()
		{
			UnhookWindowsHookEx(hHook);
			readThread.Abort();
			GC.SuppressFinalize(this);
		}

		sealed class ReceiverWindow : NativeWindow, IDisposable
		{
			public ReceiverWindow()
			{
				CreateHandle(new CreateParams
				{
					X = 0,
					Y = 0,
					Width = 0,
					Height = 0,
					Style = 0x800000
				});
			}

			~ReceiverWindow() =>
				Dispose();

			public void Dispose()
			{
				DestroyHandle();
				GC.SuppressFinalize(this);
			}
		}
	}
}
