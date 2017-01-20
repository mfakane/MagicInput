using System;
using System.Collections.Generic;
using System.Linq;
using Linearstar.Core.RawInput;
using MagicInput.Input.Behaviors;

namespace MagicInput.Input.RawInput
{
	public class RawKeyPhysicalDevice : KeyPhysicalDevice
	{
		RawInputDevice rawDevice;
		RawInputDevice RawDevice
		{
			get => rawDevice ?? (rawDevice = RawInputDevice.GetDevices().FirstOrDefault(i => i.DevicePath == DevicePath));
			set => rawDevice = value;
		}

		IntPtr? Handle => rawDevice?.Handle;

		public string DevicePath { get; private set; }

		public override string Name => RawDevice == null ? null : RawDevice.ProductName ?? $"VID_{RawDevice.VendorId:X4}&PID_{RawDevice.ProductId:X4}";
		public override string ManufacturerName => RawDevice?.ManufacturerName;
		public override string Description => DevicePath;
		public override bool IsConnected => RawDevice?.IsConnected ?? false;

		public override KeyDeviceKind Kind =>
			RawDevice?.DeviceType == RawInputDeviceType.Mouse ? KeyDeviceKind.Mouse :
			RawDevice?.DeviceType == RawInputDeviceType.Keyboard ? KeyDeviceKind.Keyboard : KeyDeviceKind.Unknown;

		public RawKeyPhysicalDevice()
		{
		}

		public RawKeyPhysicalDevice(RawInputDevice rawDevice)
			: this() =>
			DevicePath = rawDevice.DevicePath;

		public static bool IsSupported(RawInputDevice rawDevice)
		{
			switch (rawDevice.DeviceType)
			{
				case RawInputDeviceType.Mouse:
				case RawInputDeviceType.Keyboard:
					return true;
				default:
					return false;
			}
		}

		public override KeyDevice CreateDevice() =>
			RawDevice?.DeviceType == RawInputDeviceType.Mouse
				? new KeyDevice(this, new[]
				{
					new RawKeyInput(RawKeyMouseButtons.Left),
					new RawKeyInput(RawKeyMouseButtons.Right),
					new RawKeyInput(RawKeyMouseButtons.Middle),
					new RawKeyInput(RawKeyMouseButtons.X1),
					new RawKeyInput(RawKeyMouseButtons.X2),
					new RawKeyInput(RawKeyMouseButtons.WheelUp),
					new RawKeyInput(RawKeyMouseButtons.WheelDown),
					new RawKeyInput(RawKeyMouseButtons.HorizontalWheelLeft),
					new RawKeyInput(RawKeyMouseButtons.HorizontalWheelRight),
				})
				: base.CreateDevice();

		public override IDisposable RegisterDevice() =>
			new RawKeyDeviceHandle(this);

		public override IDisposable RegisterPreview(Action<KeyInputEventArgs> previewKeyInput) =>
			new RawKeyDeviceHandle(this, previewKeyInput);

		public override bool Equals(KeyPhysicalDevice other) =>
			other is RawKeyPhysicalDevice rkpd && rkpd.DevicePath == DevicePath;

		public override int GetHashCode() =>
			typeof(KeyPhysicalDevice).GetHashCode() ^ DevicePath.GetHashCode();

		public override string ToString() =>
			$"{Name}{(ManufacturerName == null ? null : ", " + ManufacturerName)}";

		class RawKeyDeviceHandle : IDisposable
		{
			static readonly object inputHookLock = new object();
			static readonly HashSet<RawKeyDeviceHandle> devices = new HashSet<RawKeyDeviceHandle>();
			static readonly HashSet<KeyBehavior> activeBehaviors = new HashSet<KeyBehavior>();
			static LowLevelMouseHandler mouseServer;
			static InputHookServer hookServer;

			RawKeyPhysicalDevice PhysicalDevice { get; }
			Action<KeyInputEventArgs> PreviewKeyInput { get; }

			public RawKeyDeviceHandle(RawKeyPhysicalDevice physicalDevice, Action<KeyInputEventArgs> previewKeyInput = null)
			{
				PreviewKeyInput = previewKeyInput;
				PhysicalDevice = physicalDevice;
				Register(this);
			}

			public void Dispose() =>
				Unregister(this);

			static void Register(RawKeyDeviceHandle device)
			{
				if (!devices.Any())
					StartHookServer();

				devices.Add(device);
			}

			static void Unregister(RawKeyDeviceHandle device)
			{
				devices.Remove(device);

				if (devices.Any()) return;

				StopHookServer();

				foreach (var i in activeBehaviors)
					i.DoKeyUp(null);

				activeBehaviors.Clear();
			}

			static void StartHookServer()
			{
				if (hookServer != null)
					return;

				mouseServer = new LowLevelMouseHandler();
				hookServer = new InputHookServer();
				mouseServer.PreviewInput += PreviewInputHandler;
				mouseServer.Input += (sender, e) =>
				{
					var rid = e.Input;
					var buttons = rid.Mouse.Buttons;

					e.Handled = OnKeyDown(rid);
					OnKeyUp(rid);
				};

				hookServer.DeviceConnected += (sender, e) =>
				{
					foreach (var i in devices)
						if (i.PhysicalDevice.DevicePath == e.Device.DevicePath)
							i.PhysicalDevice.RawDevice = e.Device;
				};
				hookServer.DeviceDisconnected += (sender, e) =>
				{
					foreach (var i in devices)
						if (i.PhysicalDevice.Handle is IntPtr handle &&
							handle == e.Handle)
							i.PhysicalDevice.RawDevice = null;
				};
				hookServer.PreviewInput += PreviewInputHandler;
				hookServer.Input += (sender, e) =>
				{
					var rid = e.Input;

					e.Handled = OnKeyDown(rid);
					OnKeyUp(rid);
				};

				bool OnKeyDown(RawInputData rid)
				{
					var handled = false;

					foreach (var i in devices.Where(i => i.PhysicalDevice.Device.DeviceSet != null)
											 .Select(i => i.PhysicalDevice.Device.DeviceSet)
											 .Select(i => i.CurrentProfile.CurrentKeyMap)
											 .SelectMany(i => i.Behaviors)
											 .Concat(activeBehaviors)
											 .Distinct()
											 .Where(i => i.Device?.PhysicalDevice is RawKeyPhysicalDevice rkpd
													  && rkpd.IsConnected
													  && rkpd.DevicePath == rid.Device?.DevicePath
													  && i.Key is RawKeyInput rki
													  && rki.IsMatch(rid, false)))
					{
						activeBehaviors.Add(i);
						handled = i.DoKeyDown(ToData(i.Device.PhysicalDevice, rid)) || handled;
					}

					return handled;
				}

				void OnKeyUp(RawInputData rid)
				{
					foreach (var i in activeBehaviors.Where(i => i.Device.PhysicalDevice is RawKeyPhysicalDevice rkpd
															  && rkpd.IsConnected
															  && rkpd.DevicePath == rid.Device?.DevicePath
															  && i.Key is RawKeyInput rki
															  && rki.IsMatch(rid, true))
													 .ToArray())
					{
						i.DoKeyUp(ToData(i.Device.PhysicalDevice, rid));
						activeBehaviors.Remove(i);
					}
				}

				void PreviewInputHandler(object sender, RawInputEventArgs e)
				{
					var rid = e.Input;

					foreach (var i in devices)
						if (i.PreviewKeyInput != null &&
							i.PhysicalDevice.DevicePath == rid.Device?.DevicePath)
						{
							var key = RawKeyInput.FromRawInputData(rid);

							if (key == null)
								continue;

							var e2 = new KeyInputEventArgs(i.PhysicalDevice, key);

							i.PreviewKeyInput(e2);
							e.Handled = e.Handled || e2.Handled;
						}
				}
			}

			static KeyData ToData(KeyPhysicalDevice physicalDevice, RawInputData rid)
			{
				switch (rid.Type)
				{
					case RawInputDeviceType.Mouse:
						return new KeyData
						(
							physicalDevice,
							rid.Mouse.LastX,
							rid.Mouse.LastY,
							(rid.Mouse.Flags & RawMouseFlags.MoveAbsolute) != 0,
							rid.Mouse.Buttons.ToMouseButtons(),
							rid.Mouse.ButtonData
						);
					case RawInputDeviceType.Keyboard:
						return new KeyData(physicalDevice, (rid.Keyboard.Flags & RawKeyboardFlags.Up) == 0, rid.Keyboard.ScanCode);
					default:
						return null;
				}
			}

			static void StopHookServer()
			{
				hookServer?.Dispose();
				mouseServer?.Dispose();
				hookServer = null;
				mouseServer = null;
			}
		}
	}
}
