using System.Collections.Generic;
using Livet;
using Livet.Messaging;
using MagicInput.Input;
using MagicInput.Models;

namespace MagicInput.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		public MainModel Model { get; }
		public IReadOnlyList<KeyDeviceSetViewModel> DeviceSets { get; }

		public KeyProfileViewModel SelectedProfile
		{
			get => GetValue<KeyProfileViewModel>();
			set => SetValue(value);
		}

		public MainViewModel()
			: this(MainModel.LoadFromSettings() ?? new MainModel())
		{
		}

		public MainViewModel(MainModel model)
		{
			Model = model;
			DeviceSets = RegisterDisposable(ViewModelHelper.CreateReadOnlyDispatcherCollection(Model.DeviceSets, i => new KeyDeviceSetViewModel(this, i), DispatcherHelper.UIDispatcher));
		}

		protected override void Dispose(bool disposing)
		{
			Model.Save();
			base.Dispose(disposing);
		}

		public void AddDeviceSet()
		{
			var vm = new KeyDeviceSetSettingsViewModel(this, new KeyDeviceSet("デバイス セット")
			{
				Profiles =
				{
					new KeyProfile("プロファイル 1")
					{
						KeyMaps =
						{
							new KeyMap("デフォルト"),
						}
					},
				},
			});

			if (Messenger.GetResponse(new TransitionMessage(vm, nameof(KeyDeviceSetSettingsViewModel))).Response ?? false)
			{
				Model.DeviceSets.Add(vm.DeviceSet);
				Model.ApplyChanges();
			}
		}

		public void RemoveDeviceSet(KeyDeviceSetViewModel deviceSet)
		{
			Model.DeviceSets.Remove(deviceSet.DeviceSet);
			Model.ApplyChanges();
		}

		public void Show() =>
			Messenger.Raise(new InteractionMessage(nameof(Show)));

		public void Close() =>
			Messenger.Raise(new InteractionMessage(nameof(Close)));
	}
}
