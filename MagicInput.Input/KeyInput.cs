using System;
using System.Collections.Generic;
using MagicInput.Input.Behaviors;
using MsgPack.Serialization;

namespace MagicInput.Input
{
	[MessagePackRuntimeType]
	public abstract class KeyInput
	{
		public Guid DeviceGuid { get; set; }
		public abstract string DisplayCode { get; }
		public abstract string ShortName { get; }

		[MessagePackIgnore]
		public virtual IList<Type> SupportedTypes => new[]
		{
			typeof(FallbackBehavior),
			typeof(SequenceBehavior),
			typeof(SwitchKeyMapBehavior),
		};

		public KeyInput()
		{
		}

		public static bool operator ==(KeyInput a, KeyInput b) =>
			a?.Equals(b) ?? ReferenceEquals(b, null);

		public static bool operator !=(KeyInput a, KeyInput b) =>
			!(a == b);

		public abstract override int GetHashCode();
		public abstract override bool Equals(object obj);
	}
}