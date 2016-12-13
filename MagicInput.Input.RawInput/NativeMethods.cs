using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MagicInput.Input.RawInput
{
	[SuppressUnmanagedCodeSecurity]
	static class NativeMethods
	{
		[DllImport("user32")]
		public static extern int GetKeyNameText(uint lParam, StringBuilder lpString, int nSize);
	}
}
