using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MagicInput.Views
{
	class ItemDragDropBehavior : Behavior<Selector>
	{
		public static readonly DependencyProperty ModelCollectionProperty = DependencyProperty.Register("ModelCollection", typeof(IList), typeof(ItemDragDropBehavior), new PropertyMetadata());

		Point? mouseDownPosition;

		public Orientation Orientation { get; set; } = Orientation.Vertical;

		public IList ModelCollection
		{
			get { return (IList)GetValue(ModelCollectionProperty); }
			set { SetValue(ModelCollectionProperty, value); }
		}

		protected override void OnAttached()
		{
			AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
			AssociatedObject.PreviewMouseMove += AssociatedObject_PreviewMouseMove;
			AssociatedObject.PreviewMouseUp += AssociatedObject_PreviewMouseUp;
			base.OnAttached();
		}

		protected override void OnDetaching()
		{
			AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
			AssociatedObject.PreviewMouseMove -= AssociatedObject_PreviewMouseMove;
			AssociatedObject.PreviewMouseUp -= AssociatedObject_PreviewMouseUp;
			base.OnDetaching();
		}

		void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed ||
				AssociatedObject.SelectedIndex == -1)
				return;

			mouseDownPosition = e.GetPosition(AssociatedObject);
		}

		void AssociatedObject_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed ||
				!(mouseDownPosition is Point pt) ||
				(pt - e.GetPosition(AssociatedObject)).LengthSquared < new Vector(SystemParameters.MinimumHorizontalDragDistance, SystemParameters.MinimumVerticalDragDistance).LengthSquared)
				return;

			var allowDrop = AssociatedObject.AllowDrop;
			var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
			var adorner = new DragAdorner(AssociatedObject, Orientation, AssociatedObject.ItemContainerGenerator.ContainerFromIndex(0).GetSelfAndAncestors().OfType<Panel>().First());

			layer.Add(adorner);
			AssociatedObject.QueryContinueDrag += AssociatedObject_QueryContinueDrag;
			AssociatedObject.DragOver += AssociatedObject_DragOver;
			AssociatedObject.Drop += AssociatedObject_Drop;
			AssociatedObject.AllowDrop = true;

			DragDrop.DoDragDrop(AssociatedObject, new SelectedItemsData(AssociatedObject), DragDropEffects.Move);

			AssociatedObject.AllowDrop = allowDrop;
			AssociatedObject.QueryContinueDrag -= AssociatedObject_QueryContinueDrag;
			AssociatedObject.DragOver -= AssociatedObject_DragOver;
			AssociatedObject.Drop -= AssociatedObject_Drop;
			layer.Remove(adorner);

			void AssociatedObject_QueryContinueDrag(object sender2, QueryContinueDragEventArgs e2)
			{
				var hitItem = GetHitItem();

				if (hitItem.container != null)
				{
					var isInsertAfter = IsInsertAfter(hitItem.container);
					var adornerPos = hitItem.container.TranslatePoint
					(
						new Point
						(
							isInsertAfter ? hitItem.container.ActualWidth : 0,
							isInsertAfter ? hitItem.container.ActualHeight : 0
						),
						AssociatedObject
					);

					adorner.Visibility = Visibility.Visible;

					if (Orientation == Orientation.Horizontal)
						adorner.X = adornerPos.X;
					else
						adorner.Y = adornerPos.Y;
				}
				else
					adorner.Visibility = Visibility.Collapsed;
			}

			void AssociatedObject_DragOver(object sender2, DragEventArgs e2)
			{
				if (e2.Data.GetData(typeof(SelectedItemsData)) is SelectedItemsData data &&
					data.TargetObject == AssociatedObject)
					e2.Effects = DragDropEffects.Move;
			}

			void AssociatedObject_Drop(object sender2, DragEventArgs e2)
			{
				if (!(e2.Data.GetData(typeof(SelectedItemsData)) is SelectedItemsData data) ||
					data.TargetObject != AssociatedObject)
					return;

				var items = ModelCollection ?? AssociatedObject.ItemsSource as IList ?? AssociatedObject.Items;
				var hitItem = GetHitItem();

				if (hitItem.container != null &&
					data.SelectedIndices.Contains(hitItem.index))
					return;

				var selectedItems = data.SelectedIndices.Select(i => items[i]).ToArray();

				AssociatedObject.SelectedIndex = hitItem.index;

				foreach (var i in data.SelectedIndices.Reverse())
				{
					items.RemoveAt(i);

					if (hitItem.index > i)
						hitItem.index--;
				}

				var targetIndex = hitItem.container == null
					? items.Count
					: hitItem.index + (IsInsertAfter(hitItem.container) ? 1 : 0);

				foreach (var i in selectedItems.Reverse())
					items.Insert(targetIndex, i);

				AssociatedObject.SelectedIndex = targetIndex;
			}

			(int index, FrameworkElement container) GetHitItem() =>
				Enumerable.Range(0, AssociatedObject.Items.Count)
						  .Select(i => (index: i, container: (FrameworkElement)AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i)))
						  .Where(i => i.container != null)
						  .FirstOrDefault(i => VisualTreeHelper.HitTest(i.container, CursorInfo.GetPosition(i.container)) != null);

			bool IsInsertAfter(FrameworkElement container) =>
				Orientation == Orientation.Horizontal
					? CursorInfo.GetPosition(container).X > container.ActualWidth / 2
					: CursorInfo.GetPosition(container).Y > container.ActualHeight / 2;
		}

		void AssociatedObject_PreviewMouseUp(object sender, MouseButtonEventArgs e) =>
			mouseDownPosition = null;

		class SelectedItemsData
		{
			public Selector TargetObject { get; }
			public IReadOnlyList<int> SelectedIndices { get; }

			public SelectedItemsData(Selector targetObject)
			{
				TargetObject = targetObject;
				SelectedIndices = new[] { targetObject.SelectedIndex };
			}
		}

		static class CursorInfo
		{
			[SuppressUnmanagedCodeSecurity, DllImport("user32")]
			static extern void GetCursorPos(out Int32Point pt);

			struct Int32Point
			{
				public int X;
				public int Y;
			}

			public static Point GetPosition(Visual v)
			{
				GetCursorPos(out var p);

				return v.PointFromScreen(new Point(p.X, p.Y));
			}
		}

		class DragAdorner : Adorner
		{
			readonly UIElement visual;
			double x;
			double y;

			public double X
			{
				get => x;
				set
				{
					x = value;

					if (Parent is AdornerLayer parent)
						parent.Update(AdornedElement);
				}
			}

			public double Y
			{
				get => y;
				set
				{
					y = value;

					if (Parent is AdornerLayer parent)
						parent.Update(AdornedElement);
				}
			}

			public DragAdorner(FrameworkElement adornedElement, Orientation orientation, Panel itemsPanel)
				: base(adornedElement)
			{
				const int size = 16;
				var isVertical = orientation == Orientation.Vertical;

				visual = new Border
				{
					Width = isVertical ? adornedElement.ActualWidth : size,
					Height = !isVertical ? adornedElement.ActualHeight : size,
					Margin = new Thickness(isVertical ? 0 : -size / 2, isVertical ? -size / 2 : 0, 0, 0),
					Child = new Grid
					{
						ColumnDefinitions =
						{
							new ColumnDefinition { Width = GridLength.Auto },
							new ColumnDefinition(),
							new ColumnDefinition { Width = GridLength.Auto },
						},
						LayoutTransform = isVertical ? null : new RotateTransform(90),
						Children =
						{
							new Line
							{
								Stroke = SystemColors.ControlTextBrush,
								StrokeThickness = size,
								StrokeEndLineCap = PenLineCap.Triangle,
								Width = size / 2,
								X1 = -size / 2,
								X2 = 0,
								Y1 = size / 2,
								Y2 = size / 2,
								ClipToBounds = true,
							},
							SetGridColumnSpan(new Line
							{
								X2 = 1,
								Y1 = size / 2,
								Y2 = size / 2,
								Stretch = Stretch.Fill,
								StrokeThickness = 2,
								Stroke = SystemColors.ControlTextBrush,
							}, 3),
							SetGridColumn(new Line
							{
								Stroke = SystemColors.ControlTextBrush,
								StrokeThickness = size,
								StrokeStartLineCap = PenLineCap.Triangle,
								Width = size / 2,
								X1 = size / 2,
								X2 = size,
								Y1 = size / 2,
								Y2 = size / 2,
								ClipToBounds = true,
							}, 2),
						},
					},
				};

				T SetGridColumn<T>(T element, int column)
					where T : UIElement
				{
					Grid.SetColumn(element, column);

					return element;
				}

				T SetGridColumnSpan<T>(T element, int column)
					where T : UIElement
				{
					Grid.SetColumnSpan(element, column);

					return element;
				}
			}

			protected override int VisualChildrenCount => 1;
			protected override Visual GetVisualChild(int index) => visual;

			protected override Size MeasureOverride(Size constraint)
			{
				visual.Measure(constraint);

				return visual.DesiredSize;
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				visual.Arrange(new Rect(visual.DesiredSize));

				return finalSize;
			}

			public override GeneralTransform GetDesiredTransform(GeneralTransform transform) =>
				new GeneralTransformGroup
				{
					Children =
					{
						base.GetDesiredTransform(transform),
						new TranslateTransform(x, y),
					},
				};
		}
	}
}
