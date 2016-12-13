using System;
using System.Collections.Generic;
using MagicInput.Input.Behaviors;
using MagicInput.Input.Behaviors.Actions;

namespace MagicInput.ViewModels
{
	class SequenceActionViewModel : ViewModelBase
	{
		readonly Dictionary<Type, SequenceAction> actions = new Dictionary<Type, SequenceAction>();

		public SequenceBehaviorViewModel Behavior { get; }

		public SequenceAction Action
		{
			get => GetValue<SequenceAction>();
			private set
			{
				actions[value.GetType()] = SetValue(value);
				value.Behavior = (SequenceBehavior)Behavior.Behavior;
				RaisePropertyChanged(nameof(ActionType));
			}
		}

		public Type ActionType
		{
			get => Action?.GetType();
			set => Action = actions.ContainsKey(value)
				? actions[value]
				: actions[value] = (SequenceAction)Activator.CreateInstance(value);
		}

		public SequenceActionViewModel(SequenceBehaviorViewModel behavior, SequenceAction action)
		{
			Behavior = behavior;
			Action = action;
		}
	}
}
