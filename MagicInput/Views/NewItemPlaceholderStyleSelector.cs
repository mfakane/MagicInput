using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MagicInput.Views
{
	class NewItemPlaceholderStyleSelector : StyleSelector
	{
		public Style ItemStyle { get; set; }
		public Style PlaceholderStyle { get; set; }

		public override Style SelectStyle(object item, DependencyObject container)
		{
			if (item == CollectionView.NewItemPlaceholder)
				return PlaceholderStyle;
			else
				return ItemStyle;
		}
	}
}
