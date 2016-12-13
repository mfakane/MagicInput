using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Livet;
using MagicInput.Input.Behaviors;

namespace MagicInput.ViewModels
{
	public class KeyBehaviorViewModel : ViewModelBase
	{
		readonly Dictionary<Type, KeyBehavior> behaviorInstances = new Dictionary<Type, KeyBehavior>();

		public KeyMapViewModel KeyMap { get; }

		public KeyBehavior Behavior
		{
			get => GetValue<KeyBehavior>();
			private set => SetValue(value);
		}

		public KeyBehaviorViewModel(KeyMapViewModel keyMap, KeyBehavior behavior)
		{
			KeyMap = keyMap;
			Behavior = behavior;
			behaviorInstances[behavior.GetType()] = behavior;
		}

		public static KeyBehaviorViewModel FromModel(KeyMapViewModel keyMap, KeyBehavior behavior)
		{
			switch (behavior)
			{
				case SequenceBehavior sequenceBehavior:
					return new SequenceBehaviorViewModel(keyMap, sequenceBehavior);
				case SwitchKeyMapBehavior switchKeyMapBehavior:
					return new SwitchKeyMapBehaviorViewModel(keyMap, switchKeyMapBehavior);
				default:
					return new KeyBehaviorViewModel(keyMap, behavior);
			}
		}

		public Type BehaviorType
		{
			get => Behavior?.GetType();
			set
			{
				if (value == BehaviorType)
					return;

				if (!behaviorInstances.ContainsKey(value))
					behaviorInstances[value] = (KeyBehavior)Activator.CreateInstance(value, Behavior.Key);

				var behaviors = KeyMap.KeyMap.Behaviors;
				var idx = behaviors.IndexOf(Behavior);

				behaviors[idx] = Behavior = behaviorInstances[value];
			}
		}

		public void Load(DependencyObject obj)
		{
			if (DesignerProperties.GetIsInDesignMode(obj))
				return;

			CompositeDisposable.Add(Behavior.Device.PhysicalDevice.RegisterPreview(e => DispatcherHelper.UIDispatcher.InvokeAsync(() =>
			{
				if ((Behavior.Device?.PhysicalDevice.Equals(e.PhysicalDevice) ?? false) && e.Key == Behavior.Key)
					KeyMap.SelectedBehavior = this;
			})));
		}
	}
}
