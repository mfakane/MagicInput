using System.Threading;

namespace MagicInput.Input.Behaviors.Actions
{
	public class WaitAction : SequenceAction
	{
		public int MillisecondsTimeout { get; set; }

		public override void DoAction() =>
			Thread.Sleep(MillisecondsTimeout);

		public override string ToString() =>
			$"待機 {MillisecondsTimeout}ms";
	}
}
