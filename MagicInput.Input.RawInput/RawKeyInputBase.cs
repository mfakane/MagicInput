using System;
using Linearstar.Core.RawInput;

namespace MagicInput.Input.RawInput
{
	public abstract class RawKeyInputBase : KeyInput
	{
		public abstract bool IsMatch(RawInputData rid, bool isUp);

		public virtual void OnKeyDown(Action doKeyUp)
		{
		}
	}
}
