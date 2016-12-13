using System.Collections.Generic;
using System.Collections.ObjectModel;
using Livet.Messaging;
using MagicInput.Input.Behaviors;
using MagicInput.Input.Behaviors.Actions;

namespace MagicInput.ViewModels
{
	class SequenceBehaviorViewModel : KeyBehaviorViewModel
	{
		SequenceBehavior SequenceBehavior => (SequenceBehavior)Behavior;

		public SequenceBehaviorViewModel(KeyMapViewModel keyMap, SequenceBehavior behavior)
			: base(keyMap, behavior)
		{
		}

		void AddAction(IList<SequenceAction> actions, SequenceAction action)
		{
			var vm = new SequenceActionViewModel(this, action);

			if (Messenger.GetResponse(new TransitionMessage(vm, nameof(SequenceActionViewModel))).Response ?? false)
				actions.Add(vm.Action);
		}

		void EditAction(IList<SequenceAction> actions, SequenceAction action)
		{
			var vm = new SequenceActionViewModel(this, action.Clone());

			if (Messenger.GetResponse(new TransitionMessage(vm, nameof(SequenceActionViewModel))).Response ?? false)
				if (actions.Contains(action))
					actions[actions.IndexOf(action)] = vm.Action;
				else
					actions.Add(vm.Action);
		}

		void RemoveAction(IList<SequenceAction> actions, SequenceAction action) =>
			actions.Remove(action);

		public void EditKeyDownAction(SequenceAction action) =>
			EditAction(SequenceBehavior.KeyDownAction, action);

		public void EditKeyHoldAction(SequenceAction action) =>
			EditAction(SequenceBehavior.KeyHoldAction, action);

		public void EditKeyUpAction(SequenceAction action) =>
			EditAction(SequenceBehavior.KeyUpAction, action);

		public void RemoveKeyDownAction(SequenceAction action) =>
			SequenceBehavior.KeyDownAction.Remove(action);

		public void RemoveKeyHoldAction(SequenceAction action) =>
			SequenceBehavior.KeyHoldAction.Remove(action);

		public void RemoveKeyUpAction(SequenceAction action) =>
			SequenceBehavior.KeyUpAction.Remove(action);

		public void AddKeyStrokeAction(ObservableCollection<SequenceAction> actions) =>
			AddAction(actions, new KeyStrokeAction());

		public void AddMouseButtonAction(ObservableCollection<SequenceAction> actions) =>
			AddAction(actions, new MouseButtonAction());

		public void AddTextInputAction(ObservableCollection<SequenceAction> actions) =>
			AddAction(actions, new TextInputAction());

		public void AddCopyToClipboardAction(ObservableCollection<SequenceAction> actions) =>
			AddAction(actions, new CopyToClipboardAction());

		public void AddOpenFileAction(ObservableCollection<SequenceAction> actions) =>
			AddAction(actions, new OpenFileAction());

		public void AddWaitAction(ObservableCollection<SequenceAction> actions) =>
			AddAction(actions, new WaitAction());
	}
}