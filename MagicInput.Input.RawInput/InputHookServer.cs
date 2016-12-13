using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Linearstar.Core.RawInput;
using Linearstar.Core.SendInput;

namespace MagicInput.Input.RawInput
{
	class InputHookServer : NativeWindow, IDisposable
	{
		[SuppressUnmanagedCodeSecurity, DllImport("user32", SetLastError = true)]
		static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint message, int action, IntPtr pChangeFilterStruct);
		[SuppressUnmanagedCodeSecurity, DllImport("MagicHook")]
		static extern bool LoadHook(IntPtr hServerWnd, bool installMouseHook, bool installKeyboardHook);
		[SuppressUnmanagedCodeSecurity, DllImport("MagicHook")]
		static extern bool UnloadHook();

		const int MSGFLT_ALLOW = 1;
		const int WM_INPUT = 0xFF;
		const int WM_APP = 0x8000;
		const int WM_APP_MOUSE = WM_APP + 1;
		const int WM_APP_KEYBOARD = WM_APP + 2;

		readonly List<(int expires, RawInputData data)> keyboardQueue = new List<(int expires, RawInputData data)>();

		public event EventHandler<RawInputEventArgs> PreviewInput;
		public event EventHandler<RawInputEventArgs> Input;

		public InputHookServer()
		{
			CreateHandle(new CreateParams
			{
				X = 0,
				Y = 0,
				Width = 0,
				Height = 0,
				Style = 0x800000,
			});
			ChangeWindowMessageFilterEx(Handle, WM_APP_KEYBOARD, MSGFLT_ALLOW, IntPtr.Zero);

			RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink, Handle);
			LoadHook(Handle, false, true);
		}

		protected virtual void OnPreviewInput(RawInputEventArgs e) =>
			PreviewInput?.Invoke(this, e);

		protected virtual void OnInput(RawInputEventArgs e) =>
			Input?.Invoke(this, e);

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WM_INPUT:
					{
						var rid = RawInputData.FromHandle(m.LParam);
						var e = new RawInputEventArgs(rid);

						OnPreviewInput(e);

						switch (rid.Type)
						{
							case RawInputDeviceType.Keyboard:
								if (!e.Handled)
									keyboardQueue.Add((Environment.TickCount + 25, rid));

								break;
						}

						break;
					}
				case WM_APP_MOUSE:
					m.Result = IntPtr.Zero;

					return;
				case WM_APP_KEYBOARD:
					var key = (Keys)m.WParam.ToInt32();
					var param = new KeyMessageParameters(m.LParam);
					var record = GetMatchingRecord(param);

					if (record != null)
					{
						var e = new RawInputEventArgs(record);

						OnInput(e);

						m.Result = e.Handled ? new IntPtr(1) : IntPtr.Zero;
					}

					return;

					RawInputData GetMatchingRecord(KeyMessageParameters parameters)
					{
						for (int i = 0; i < keyboardQueue.Count; i++)
						{
							var item = keyboardQueue[i];

							if (Environment.TickCount > item.expires)
							{
								keyboardQueue.RemoveAt(i);
								i--;

								continue;
							}

							if (item.data.Type != RawInputDeviceType.Keyboard)
								continue;
							else if (item.data.Keyboard.ScanCode != parameters.ScanCode)
								continue;
							else if (((item.data.Keyboard.Flags & RawKeyboardFlags.Up) != 0) != parameters.IsKeyUp)
								continue;

							return item.data;
						}

						return null;
					}
			}

			base.WndProc(ref m);
		}

		~InputHookServer() =>
			Dispose();

		public void Dispose()
		{
			RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);
			UnloadHook();
			DestroyHandle();
			GC.SuppressFinalize(this);
		}
	}
}
