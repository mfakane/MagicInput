using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MagicInput
{
	[SuppressUnmanagedCodeSecurity]
	static class NativeMethods
	{
		[DllImport("user32")]
		public static extern IntPtr GetForegroundWindow();
		[DllImport("user32")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
		[DllImport("user32")]
		public static extern int GetWindowTextLength(IntPtr hWnd);
		[DllImport("user32")]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		public static string GetWindowText(this IntPtr hWnd)
		{
			var sb = new StringBuilder(GetWindowTextLength(hWnd) + 1);

			GetWindowText(hWnd, sb, sb.Capacity);

			return sb.ToString();
		}

		public static int GetWindowProcessId(this IntPtr hWnd)
		{
			GetWindowThreadProcessId(hWnd, out var pid);

			return pid;
		}

		public static string GetWindowProcessName(this IntPtr hWnd)
		{
			var pid = hWnd.GetWindowProcessId();

			if (pid <= 0)
				return null;

			using (var p = Process.GetProcessById(pid))
				return p.ProcessName;
		}
	}
}
