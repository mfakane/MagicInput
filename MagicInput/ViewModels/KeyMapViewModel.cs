using Livet;
using MagicInput.Input;

namespace MagicInput.ViewModels
{
	public class KeyMapViewModel : ViewModelBase
	{
		public KeyProfileViewModel Profile { get; }
		public KeyMap KeyMap { get; }
		public ReadOnlyDispatcherCollection<KeyBehaviorViewModel> Behaviors { get; }

		public string Name
		{
			get => KeyMap.Name;
			set
			{
				KeyMap.Name = value;
				RaisePropertyChanged();
			}
		}

		public bool IsChangingName
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}

		public KeyBehaviorViewModel SelectedBehavior
		{
			get => GetValue<KeyBehaviorViewModel>();
			set => SetValue(value);
		}

		public KeyMapViewModel(KeyProfileViewModel profile, KeyMap keyMap)
		{
			Profile = profile;
			KeyMap = keyMap;
			Behaviors = ViewModelHelper.CreateReadOnlyDispatcherCollection(keyMap.Behaviors, i => KeyBehaviorViewModel.FromModel(this, i), DispatcherHelper.UIDispatcher);
		}
	}
}
