using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MsgPack.Serialization;
using MsgPack.Serialization.CollectionSerializers;

namespace MagicInput.Input
{
	public class TreeCollection<T, TParent> : ObservableCollection<T>
	{
		readonly Func<TParent> parent;
		readonly Func<T, TParent> getParent;
		readonly Action<T, TParent> setParent;

		TreeCollection()
		{
		}

		public TreeCollection(TParent parent, Func<T, TParent> getParent, Action<T, TParent> setParent)
			: this(() => parent, getParent, setParent)
		{
		}

		public TreeCollection(Func<TParent> parent, Func<T, TParent> getParent, Action<T, TParent> setParent)
		{
			this.parent = parent;
			this.getParent = getParent;
			this.setParent = setParent;
		}

		public TreeCollection(Func<TParent> parent, Func<T, TParent> getParent, Action<T, TParent> setParent, IEnumerable<T> collection)
			: this(parent, getParent, setParent)
		{
			if (collection == null) return;

			foreach (var i in collection)
				Add(i);
		}

		protected override void InsertItem(int index, T item)
		{
			setParent?.Invoke(item, parent());

			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			setParent?.Invoke(this[index], default(TParent));
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, T item)
		{
			if (setParent != null)
			{
				setParent(this[index], default(TParent));
				setParent(item, parent());
			}

			base.SetItem(index, item);
		}

		protected override void ClearItems()
		{
			if (setParent != null)
				foreach (var i in this)
					setParent(i, default(TParent));

			base.ClearItems();
		}

		public class CustomMessagePackSerializer : CollectionMessagePackSerializer<TreeCollection<T, TParent>, T>
		{
			public CustomMessagePackSerializer(SerializationContext ownerContext, PolymorphismSchema schema)
				: base(ownerContext, schema)
			{
			}

			public CustomMessagePackSerializer(SerializationContext ownerContext, PolymorphismSchema schema, SerializerCapabilities capabilities)
				: base(ownerContext, schema, capabilities)
			{
			}

			protected override TreeCollection<T, TParent> CreateInstance(int initialCapacity) =>
				new TreeCollection<T, TParent>();
		}
	}
}
