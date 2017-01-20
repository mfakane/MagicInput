using System.ComponentModel;
using System.Linq;

namespace MagicInput.Input.Behaviors
{
	[DisplayName("フォールバック")]
	public class FallbackBehavior : KeyBehavior
	{
		public FallbackBehavior()
		{
		}

		public FallbackBehavior(KeyInput key)
			: base(key)
		{
		}

		protected override bool OnKeyDown(KeyData data) =>
			GetPreviousKeyMapKey()?.DoKeyDown(data) ?? false;

		protected override bool OnKeyRepeat(KeyData data) =>
			GetPreviousKeyMapKey()?.DoKeyDown(data) ?? false;

		protected override void OnKeyUp(KeyData data) =>
			GetPreviousKeyMapKey()?.DoKeyUp(data);

		KeyBehavior GetPreviousKeyMapKey()
		{
			var profile = KeyMap.Profile;

			if (!profile.PreviousKeyMaps.Any()) return null;

			var guid = profile.PreviousKeyMaps.Pop();

			try
			{
				var rt = profile.KeyMaps.FirstOrDefault(i => i.Guid == guid && i.Guid != KeyMap.Guid)?.Behaviors.FirstOrDefault(i => i.Key == Key);

				if (rt is FallbackBehavior innerFallback)
					rt = innerFallback.GetPreviousKeyMapKey() ?? rt;

				return rt;
			}
			finally
			{
				profile.PreviousKeyMaps.Push(guid);
			}
		}
	}
}
