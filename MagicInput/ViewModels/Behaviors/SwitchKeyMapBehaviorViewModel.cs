using System.Collections.Generic;
using System.Linq;
using MagicInput.Input;
using MagicInput.Input.Behaviors;

namespace MagicInput.ViewModels
{
	public class SwitchKeyMapBehaviorViewModel : KeyBehaviorViewModel
	{
		public SwitchKeyMapBehavior SwitchKeyMapBehavior => (SwitchKeyMapBehavior)Behavior;
		public IList<KeyMap> KeyMaps => SwitchKeyMapBehavior.KeyMap.Profile.KeyMaps.Where(i => i != SwitchKeyMapBehavior.KeyMap).ToArray();

		public SwitchKeyMapBehaviorViewModel(KeyMapViewModel keyMap, SwitchKeyMapBehavior behavior)
			: base(keyMap, behavior)
		{
		}
	}
}
