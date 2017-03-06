using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Linearstar.Core.RawInput;
using MagicInput.Input.Behaviors;
using MsgPack.Serialization;

namespace MagicInput.Input.RawInput
{
	public class RawMouseInput : RawKeyInputBase, IEquatable<RawMouseInput>
	{
		DateTime lastInput;

		public override string DisplayCode => MouseButton.ToString();
		public override string ShortName =>
			MouseButton == RawMouseButtons.Move ? "P" : new string(MouseButton.ToString().Where(i => char.IsUpper(i) || char.IsDigit(i)).ToArray()).TrimStart('H');

		public RawMouseButtons MouseButton { get; set; }

		[MessagePackIgnore]
		public override IList<Type> SupportedTypes => new[]
		{
			typeof(FallbackBehavior),
			typeof(MoveWithSequenceBehavior),
		};

		public RawMouseInput()
		{
		}

		public RawMouseInput(RawMouseButtons mouseButton) =>
			MouseButton = mouseButton;

		public static RawMouseInput FromRawInputData(RawInputData rid)
		{
			if (rid.Type != RawInputDeviceType.Mouse) return null;

			var button = rid.Mouse.Buttons.GetDownRawMouseButton(rid.Mouse.ButtonData, true);

			return button != RawMouseButtons.None ? new RawMouseInput
			{
				MouseButton = button,
			} : null;
		}

		public override bool IsMatch(RawInputData rid, bool isUp)
		{
			if (rid.Type != RawInputDeviceType.Mouse) return false;

			if (isUp)
				return (rid.Mouse.Buttons.ToRawMouseButton(rid.Mouse.ButtonData) & MouseButton) != 0
					&& (rid.Mouse.Buttons.GetDownRawMouseButton(rid.Mouse.ButtonData) & MouseButton) == 0;
			else
				return (rid.Mouse.Buttons.GetDownRawMouseButton(rid.Mouse.ButtonData) & MouseButton) != 0
					|| (rid.Mouse.LastX != 0 || rid.Mouse.LastY != 0) && (MouseButton & RawMouseButtons.Move) != 0;
		}

		public override void OnKeyDown(Action doKeyUp)
		{
			if ((MouseButton & RawMouseButtons.Move
							 | RawMouseButtons.WheelUp
							 | RawMouseButtons.WheelDown
							 | RawMouseButtons.HorizontalWheelLeft
							 | RawMouseButtons.HorizontalWheelRight) == RawMouseButtons.None)
				return;

			Task.Run(async () =>
			{
				const int timeout = 50;
				var time = DateTime.Now;

				await Task.Delay(timeout).ConfigureAwait(false);

				if (lastInput <= time)
					doKeyUp();
			});
		}

		public override string ToString() =>
			MouseButton.ToString();

		public bool Equals(RawMouseInput other) =>
			(other?.DeviceGuid == Guid.Empty || DeviceGuid == Guid.Empty || other?.DeviceGuid == DeviceGuid) &&
			other?.MouseButton == MouseButton;

		public override bool Equals(object obj) =>
			obj is RawMouseInput rmi && Equals(rmi);

		public override int GetHashCode() =>
			typeof(RawMouseInput).GetHashCode() ^ MouseButton.GetHashCode();
	}

	[Flags]
	public enum RawMouseButtons
	{
		None,
		Move,
		Left,
		Right = 4,
		Middle = 8,
		X1 = 16,
		X2 = 32,
		WheelUp = 64,
		WheelDown = 128,
		HorizontalWheelLeft = 256,
		HorizontalWheelRight = 512,
	}
}
