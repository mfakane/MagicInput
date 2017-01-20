using System;
using System.ComponentModel;
using System.Linq;

namespace MagicInput.Input.Behaviors
{
	[DisplayName("キーマップ切り替え")]
	public class SwitchKeyMapBehavior : KeyBehavior
	{
		public Guid TargetKeyMapGuid { get; set; }

		public SwitchKeyMapBehavior()
		{
		}

		public SwitchKeyMapBehavior(KeyInput key)
			: this() =>
			Key = key;

		protected override bool OnKeyDown(KeyData data)
		{
			var keyMap = KeyMap.Profile.KeyMaps.FirstOrDefault(i => i.Guid == TargetKeyMapGuid);

			if (keyMap != null)
			{
				KeyMap.Profile.PushKeyMap(keyMap);

				return true;
			}
			else
				return false;
		}

		protected override bool OnKeyRepeat(KeyData data) => true;

		protected override void OnKeyUp(KeyData data) =>
			KeyMap.Profile.PopKeyMap();

		public override string ToString() =>
			"キーマップ: " + KeyMap?.Profile.KeyMaps.FirstOrDefault(i => i.Guid == TargetKeyMapGuid)?.Name;
	}
}
