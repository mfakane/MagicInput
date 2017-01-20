namespace MagicInput.Input
{
	public class KeyData
	{
		public KeyPhysicalDevice PhysicalDevice { get; }
		public bool? IsDown { get; }
		public int? ScanCode { get; }
		public MouseButtons MouseButtons { get; }
		public int? WheelDelta { get; }
		public int? X { get; }
		public int? Y { get; }
		public bool? IsAbsolute { get; }

		public KeyData(KeyPhysicalDevice physicalDevice, bool isDown, int scanCode)
		{
			PhysicalDevice = physicalDevice;
			IsDown = isDown;
			ScanCode = scanCode;
		}

		public KeyData(KeyPhysicalDevice physicalDevice, int x, int y, bool isAbsolute, MouseButtons mouseButtons, int? wheelDelta)
		{
			PhysicalDevice = physicalDevice;
			X = x;
			Y = y;
			IsAbsolute = isAbsolute;
			MouseButtons = mouseButtons;
			WheelDelta = wheelDelta;
		}

		public override string ToString() =>
			IsDown.HasValue
				? $"IsDown: {IsDown}, ScanCode: {ScanCode}"
				: $"X: {X}, Y: {Y}, IsAbsolute: {IsAbsolute}, MouseButtons: {MouseButtons}, WheelDelta: {WheelDelta}";
	}
}
