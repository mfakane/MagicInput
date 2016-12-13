using System.Windows;
using System.Windows.Interactivity;
using TriggerAction = System.Windows.Interactivity.TriggerAction;
using TriggerBase = System.Windows.Interactivity.TriggerBase;

namespace MagicInput.Views
{
	class StyleInteraction
	{
		public static readonly DependencyProperty TriggersProperty = DependencyProperty.RegisterAttached("Triggers", typeof(StyleTriggers), typeof(StyleInteraction), new PropertyMetadata((d, e) =>
		{
			if (e.OldValue == e.NewValue)
				return;

			var target = Interaction.GetTriggers(d);

			if (e.NewValue != null)
				foreach (var i in (StyleTriggers)e.NewValue)
				{
					var trigger = (TriggerBase)i.Clone();

					foreach (var j in i.Actions)
						trigger.Actions.Add((TriggerAction)j.Clone());

					target.Add(trigger);
				}
		}));

		public static StyleTriggers GetTriggers(DependencyObject obj)
		{
			return (StyleTriggers)obj.GetValue(TriggersProperty);
		}

		public static void SetTriggers(DependencyObject obj, StyleTriggers value)
		{
			obj.SetValue(TriggersProperty, value);
		}
	}

	class StyleTriggers : FreezableCollection<TriggerBase>
	{
		protected override Freezable CreateInstanceCore()
		{
			return new StyleTriggers();
		}
	}
}
