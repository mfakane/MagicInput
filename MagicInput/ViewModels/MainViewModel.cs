using System.Collections.Generic;
using System.Linq;
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

		public KeyDeviceSetViewModel SelectedDeviceSet
		{
			get => GetValue<KeyDeviceSetViewModel>();
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
			SelectedDeviceSet = DeviceSets.FirstOrDefault();
		}

		protected override void Dispose(bool disposing)
		{
			Model.Save();
			base.Dispose(disposing);
		}

		public void ApplyChanges()
		{
			Model.ApplyChanges();

			foreach (var i in DeviceSets.SelectMany(i => i.Profiles).SelectMany(i => i.KeyMaps).SelectMany(i => i.Behaviors))
				i.StartPreview();
		}

		public void Stop()
		{
			Model.Stop();

			foreach (var i in DeviceSets.SelectMany(i => i.Profiles).SelectMany(i => i.KeyMaps).SelectMany(i => i.Behaviors))
				i.StopPreview();
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
				SelectedDeviceSet = DeviceSets.Last();
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
