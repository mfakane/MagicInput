using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Livet.Commands;

namespace MagicInput.Views
{
	[MarkupExtensionReturnType(typeof(ICommand))]
	class MethodCommand : MarkupExtension
	{
		public string MethodName { get; }

		public MethodCommand(string methodName) =>
			MethodName = methodName;

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var targetInfo = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

			if (!(targetInfo.TargetObject is FrameworkElement targetObject))
				return this;

			var command = new ListenerCommand<object>(p =>
			{
				var dataContext = targetObject.DataContext;
				var info = ReslovePath(dataContext, MethodName);

				if (info.memberInfo is MethodInfo mi)
					mi.Invoke(info.target, mi.GetParameters().Length == 0 ? null : new[] { p });
			}, () =>
			{
				var dataContext = targetObject.DataContext;
				var info = ReslovePath(dataContext, MethodName, "Can");

				if (info.memberInfo is PropertyInfo pi)
					return (bool)pi.GetValue(dataContext);
				else
					return true;
			});

			targetObject.DataContextChanged += (sender, e) =>
			{
				if (e.OldValue is INotifyPropertyChanged oldContext)
					oldContext.PropertyChanged -= DataContext_PropertyChanged;

				if (e.NewValue is INotifyPropertyChanged newContext)
					newContext.PropertyChanged += DataContext_PropertyChanged;
			};

			return command;

			void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == "Can" + MethodName.Split('.').Last())
					command.RaiseCanExecuteChanged();
			}
		}

		(object target, MemberInfo memberInfo) ReslovePath(object target, string path, string lastPrefix = null)
		{
			if (target == null)
				return (null, null);

			var sl = path.Split('.');

			foreach (var i in sl.Take(sl.Length - 1))
			{
				var type = target?.GetType();

				if (type?.GetProperty(i) is PropertyInfo pi)
					target = pi.GetValue(target);
				else
					return (null, null);
			}

			return target == null ? (null, null) : (target, target.GetType().GetMember(lastPrefix + sl.Last()).FirstOrDefault());
		}
	}
}
