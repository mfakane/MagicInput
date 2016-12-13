using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MagicInput.Input.Behaviors.Actions;

namespace MagicInput.Input.Behaviors
{
	[DisplayName("シーケンス")]
	public class SequenceBehavior : KeyBehavior
	{
		Task onKeyDown;
		readonly object onKeyDownLock = new object();

		int _holdDuration;
		public int HoldDuration
		{
			get => _holdDuration;
			set
			{
				if (_holdDuration != (_holdDuration = value))
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(HoldDuration)));
			}
		}

		public IList<SequenceAction> KeyDownAction { get; }
		public IList<SequenceAction> KeyHoldAction { get; }
		public IList<SequenceAction> KeyUpAction { get; }

		public SequenceBehavior()
		{
			KeyDownAction = new TreeCollection<SequenceAction, SequenceBehavior>(this, i => i.Behavior, (i, parent) => i.Behavior = parent);
			KeyHoldAction = new TreeCollection<SequenceAction, SequenceBehavior>(this, i => i.Behavior, (i, parent) => i.Behavior = parent);
			KeyUpAction = new TreeCollection<SequenceAction, SequenceBehavior>(this, i => i.Behavior, (i, parent) => i.Behavior = parent);
		}

		public SequenceBehavior(KeyInput key)
			: this() =>
			Key = key;

		protected override bool OnKeyDown()
		{
			lock (onKeyDownLock)
			{
				onKeyDown = Task.Run(() =>
				{
					foreach (var i in KeyDownAction)
						i.DoAction();
				});

				if (HoldDuration > 0)
					onKeyDown = onKeyDown.ContinueWith(async t =>
					{
						await Task.Delay(HoldDuration);

						if (IsDown)
							foreach (var i in KeyHoldAction)
								i.DoAction();
					});
			}

			return true;
		}

		protected override bool OnKeyRepeat()
		{
			if (HoldDuration > 0) return true;

			lock (onKeyDownLock)
				onKeyDown = onKeyDown?.ContinueWith(t =>
				{
					foreach (var i in KeyHoldAction)
						i.DoAction();
				});

			return true;
		}

		protected override void OnKeyUp()
		{
			lock (onKeyDownLock)
				onKeyDown = onKeyDown?.ContinueWith(t =>
				{
					foreach (var i in KeyUpAction)
						i.DoAction();
				});
		}
	}
}
