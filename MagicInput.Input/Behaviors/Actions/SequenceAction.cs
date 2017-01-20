using System.ComponentModel;
using MsgPack.Serialization;

namespace MagicInput.Input.Behaviors.Actions
{
	[MessagePackRuntimeType]
	public abstract class SequenceAction : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[MessagePackIgnore]
		public KeyBehavior Behavior { get; set; }

		protected void OnPropertyChanged(PropertyChangedEventArgs e) =>
			PropertyChanged?.Invoke(this, e);

		public abstract void DoAction();

		public virtual SequenceAction Clone()
		{
			var ctx = new SerializationContext
			{
				SerializationMethod = SerializationMethod.Map,
			};
			var serialzer = ctx.GetSerializer<SequenceAction>();

			return serialzer.UnpackSingleObject(serialzer.PackSingleObject(this));
		}
	}
}