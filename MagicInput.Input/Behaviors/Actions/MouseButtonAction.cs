using System;
using Linearstar.Core.SendInput;

namespace MagicInput.Input.Behaviors.Actions
{
	public class MouseButtonAction : SequenceAction
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int MouseButton { get; set; }
		public int WheelDelta { get; set; } = 120;
		public MouseButtonActionKind Kind { get; set; }

		public override void DoAction()
		{
			var mouse = new SendMouse();

			switch (Kind)
			{
				case MouseButtonActionKind.MoveRelative:
					mouse.Flags = SendMouseFlags.Move;
					mouse.X = X;
					mouse.Y = Y;

					break;
				case MouseButtonActionKind.MoveAbsolute:
					mouse.Flags = SendMouseFlags.Move | SendMouseFlags.Absolute;
					mouse.X = X;
					mouse.Y = Y;

					break;
				case MouseButtonActionKind.Down:
				case MouseButtonActionKind.DownUp:
					mouse.Flags = MouseButtonToDown(MouseButton);

					if (MouseButton > 2)
						mouse.MouseData = MouseButton - 2;

					break;
				case MouseButtonActionKind.Up:
					mouse.Flags = MouseButtonToUp(MouseButton);

					if (MouseButton > 2)
						mouse.MouseData = MouseButton - 2;

					break;
				case MouseButtonActionKind.Wheel:
				case MouseButtonActionKind.HorizontalWheel:
					mouse.Flags = Kind == MouseButtonActionKind.Wheel ? SendMouseFlags.MouseWheel : SendMouseFlags.MouseHorizontalWheel;
					mouse.MouseData = WheelDelta;

					break;
			}

			SendInput.Send(mouse);

			if (Kind == MouseButtonActionKind.DownUp)
			{
				mouse.Flags = MouseButtonToDown(MouseButton);
				SendInput.Send(mouse);
			}

			SendMouseFlags MouseButtonToDown(int mouseButton)
			{
				switch (mouseButton)
				{
					case 0: return SendMouseFlags.LeftButtonDown;
					case 1: return SendMouseFlags.RightButtonDown;
					case 2: return SendMouseFlags.MiddleButtonDown;
					default: return SendMouseFlags.XButtonDown;
				}
			}

			SendMouseFlags MouseButtonToUp(int mouseButton)
			{
				switch (mouseButton)
				{
					case 0: return SendMouseFlags.LeftButtonUp;
					case 1: return SendMouseFlags.RightButtonUp;
					case 2: return SendMouseFlags.MiddleButtonUp;
					default: return SendMouseFlags.XButtonUp;
				}
			}
		}

		public override string ToString()
		{
			switch (Kind)
			{
				case MouseButtonActionKind.Down:
				case MouseButtonActionKind.Up:
				case MouseButtonActionKind.DownUp:
					return $"マウス {(MouseButton > 2 ? "X" + (MouseButton - 2) : new[] { "左ボタン", "右ボタン", "中ボタン" }[MouseButton])} {Kind}";
				case MouseButtonActionKind.Wheel:
					return $"マウス ホイール {WheelDelta}";
				case MouseButtonActionKind.HorizontalWheel:
					return $"マウス 横ホイール {WheelDelta}";
				case MouseButtonActionKind.MoveAbsolute:
					return $"マウス 移動 絶対 {X}, {Y}";
				case MouseButtonActionKind.MoveRelative:
					return $"マウス 移動 相対 {X}, {Y}";
				default:
					throw new InvalidOperationException();
			}
		}
	}

	public enum MouseButtonActionKind
	{
		Down,
		Up,
		DownUp,
		Wheel,
		HorizontalWheel,
		MoveAbsolute,
		MoveRelative,
	}
}
