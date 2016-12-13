using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Livet;
using MagicInput.Input;
using MsgPack.Serialization;

namespace MagicInput.Models
{
	public class MainModel : NotifyObject, IDisposable
	{
		IDisposable deviceHandle;
		readonly CancellationTokenSource cts = new CancellationTokenSource();
		readonly KeyFactoryHost host;
		static readonly string startupPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		static readonly string settingsPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".dat");

		[MessagePackIgnore]
		public IntPtr CurrentActiveWindow
		{
			get => GetValue<IntPtr>();
			private set => SetValue(value);
		}

		public int SwitchInterval
		{
			get => GetValue<int>();
			set => SetValue(value);
		}

		[MessagePackIgnore]
		public bool IsSuspended
		{
			get => GetValue<bool>();
			private set => SetValue(value);
		}

		[MessagePackIgnore]
		public IList<IKeyProvider> Providers => host.Providers;
		public ObservableCollection<KeyDeviceSet> DeviceSets { get; } = new ObservableCollection<KeyDeviceSet>();

		public MainModel()
		{
			SwitchInterval = 100;
			host = new KeyFactoryHost(Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "MagicInput.Input.*.dll")
											   .Select(Assembly.LoadFrom));
		}

		public void Load(DependencyObject obj)
		{
			ApplyChanges();
			Task.Run(() => MainProcessAsync(cts.Token), cts.Token);
		}

		public async Task MainProcessAsync(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				var hWnd = NativeMethods.GetForegroundWindow();

				if (hWnd != CurrentActiveWindow)
				{
					CurrentActiveWindow = hWnd;
					IsSuspended = hWnd.GetWindowProcessId() == Process.GetCurrentProcess().Id;

					if (!IsSuspended)
						foreach (var i in DeviceSets)
							i.ActiveWindowChanged(hWnd.GetWindowProcessName(), hWnd.GetWindowText());
				}

				await Task.Delay(SwitchInterval, ct);
			}
		}

		public void ApplyChanges()
		{
			deviceHandle?.Dispose();
			deviceHandle = CreateHandle();
		}

		IDisposable CreateHandle() =>
			new LivetCompositeDisposable(DeviceSets.SelectMany(i => i.Devices).Select(i => i.RegisterDevice()));

		static MessagePackSerializer<MainModel> CreateSerializer()
		{
			var ctx = new SerializationContext
			{
				SerializationMethod = SerializationMethod.Map,
			};

			ctx.ResolveSerializer += (sender, e) =>
			{
				if (e.TargetType.IsConstructedGenericType && e.TargetType.GetGenericTypeDefinition() == typeof(TreeCollection<,>))
				{
					var builtType = typeof(TreeCollection<,>.CustomMessagePackSerializer).MakeGenericType(e.TargetType.GetGenericArguments());

					typeof(ResolveSerializerEventArgs).GetMethod("SetSerializer").MakeGenericMethod(e.TargetType).Invoke(e, new[] { Activator.CreateInstance(builtType, e.Context, e.PolymorphismSchema) });
				}
			};

			return ctx.GetSerializer<MainModel>();
		}

		public static MainModel LoadFromSettings() =>
			File.Exists(settingsPath) ? CreateSerializer().UnpackSingleObject(File.ReadAllBytes(settingsPath)) : null;

		public void Save() =>
			File.WriteAllBytes(settingsPath, CreateSerializer().PackSingleObject(this));

		public void Dispose()
		{
			cts.Cancel();
			cts.Dispose();
			deviceHandle?.Dispose();
			host.Dispose();
		}
	}
}
