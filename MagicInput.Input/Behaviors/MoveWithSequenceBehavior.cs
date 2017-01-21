using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MagicInput.Input.Behaviors.Actions;

namespace MagicInput.Input.Behaviors
{
	[DisplayName("シーケンスと移動")]
	public class MoveWithSequenceBehavior : KeyBehavior
	{
		Task onKeyDown;
		readonly object onKeyDownLock = new object();
		Point startPosition;

		public IList<SequenceAction> KeyDownAction { get; }
		public IList<SequenceAction> KeyUpAction { get; }

		public MoveWithSequenceBehavior()
		{
			KeyDownAction = new TreeCollection<SequenceAction, MoveWithSequenceBehavior>(this, i => (MoveWithSequenceBehavior)i.Behavior, (i, parent) => i.Behavior = parent);
			KeyUpAction = new TreeCollection<SequenceAction, MoveWithSequenceBehavior>(this, i => (MoveWithSequenceBehavior)i.Behavior, (i, parent) => i.Behavior = parent);
		}

		public MoveWithSequenceBehavior(KeyInput key)
			: this() =>
			Key = key;

		protected override bool OnKeyDown(KeyData data)
		{
			lock (onKeyDownLock)
				onKeyDown = Task.Run(() =>
				{
					startPosition = Cursor.Position;

					foreach (var i in KeyDownAction)
						i.DoAction();
				});

			return true;
		}

		protected override bool OnKeyRepeat(KeyData data) =>
			false;

		protected override void OnKeyUp(KeyData data)
		{
			lock (onKeyDownLock)
				onKeyDown = onKeyDown?.ContinueWith(t =>
				{
					foreach (var i in KeyUpAction)
						i.DoAction();

					Cursor.Position = startPosition;
				});
		}
	}
}
