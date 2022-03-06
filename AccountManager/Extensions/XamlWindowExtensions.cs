using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Extensions
{
    public static class XamlWindowExtensions
    {
		public static void SetDimensionsAndCenter(this Microsoft.UI.Xaml.Window window, int width, int height)
		{
			// Get desktop window so we can get its height
			var desktopWindow = PInvoke.User32.GetDesktopWindow();
			var desiredHeight = height;
			var desiredWidth = width;
			// Get the desktop window as a rectangle
			PInvoke.User32.GetWindowRect(desktopWindow, out PInvoke.RECT rect);
			// Get the handle of the MAUI app
			IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
			// Set its position to the center of the screen and height/width to the desired values
			PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
			(Int32)(rect.right / 2 - (desiredWidth / 2)), (Int32)(rect.bottom / 2 - (desiredHeight / 2)), (Int32)desiredWidth, (Int32)desiredHeight,
			PInvoke.User32.SetWindowPosFlags.SWP_SHOWWINDOW);
		}
	}
}
