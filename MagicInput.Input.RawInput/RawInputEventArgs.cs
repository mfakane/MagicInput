using System;
using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	class RawInputEventArgs : EventArgs
	{
		public bool Handled { get; set; }
		public RawInputData Input { get; }

		public RawInputEventArgs(RawInputData input) =>
			Input = input;
	}
}
