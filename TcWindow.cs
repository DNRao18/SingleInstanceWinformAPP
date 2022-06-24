using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SingleInstanceWinformAPP
{
  public class TcWindow
  {
    private static readonly IntPtr HWND_TOP = new IntPtr(0);
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_MDICHILD = 64;
    private const int WS_EX_APPWINDOW = 262144;
    private const int WS_VISIBLE = 268435456;
    private const int WS_CHILD = 1073741824;
    private const int WS_CLIPSIBLINGS = 67108864;
    private const int SWP_NOACTIVATE = 16;
    private const uint SWP_NOSIZE = 1;
    private const int SRCCOPY = 13369376;
    private IntPtr mHandle;
    private int mProcessId;
    private string mWindowName;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EnumWindows(
      TcWindow.EnumThreadWindowsCallback callback,
      IntPtr extraData);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
      IntPtr hWnd,
      IntPtr hWndInsertAfter,
      int X,
      int Y,
      int cx,
      int cy,
      uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int IsIconic(IntPtr hWnd);

    [DllImport("GDI32.dll")]
    private static extern bool BitBlt(
      int hdcDest,
      int nXDest,
      int nYDest,
      int nWidth,
      int nHeight,
      int hdcSrc,
      int nXSrc,
      int nYSrc,
      int dwRop);

    [DllImport("GDI32.dll")]
    private static extern int CreateCompatibleBitmap(int hdc, int nWidth, int nHeight);

    [DllImport("GDI32.dll")]
    private static extern int CreateCompatibleDC(int hdc);

    [DllImport("GDI32.dll")]
    private static extern bool DeleteDC(int hdc);

    [DllImport("GDI32.dll")]
    private static extern bool DeleteObject(int hObject);

    [DllImport("GDI32.dll")]
    private static extern int GetDeviceCaps(int hdc, int nIndex);

    [DllImport("GDI32.dll")]
    private static extern int SelectObject(int hdc, int hgdiobj);

    [DllImport("User32.dll")]
    private static extern int GetWindowDC(int hWnd);

    [DllImport("User32.dll")]
    private static extern int ReleaseDC(int hWnd, int hDC);

    public static Image CaptureWindow(IntPtr hwnd, int width, int height)
    {
      int windowDc = TcWindow.GetWindowDC(hwnd.ToInt32());
      int compatibleDc = TcWindow.CreateCompatibleDC(windowDc);
      int compatibleBitmap = TcWindow.CreateCompatibleBitmap(windowDc, width, height);
      int hgdiobj = TcWindow.SelectObject(compatibleDc, compatibleBitmap);
      TcWindow.BitBlt(compatibleDc, 0, 0, width, height, windowDc, 0, 0, 13369376);
      TcWindow.SelectObject(compatibleDc, hgdiobj);
      TcWindow.DeleteDC(compatibleDc);
      TcWindow.ReleaseDC(hwnd.ToInt32(), windowDc);
      Image image = (Image) typeof (Image).InvokeMember("FromHbitmap", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, (Binder) null, (object) null, new object[1]
      {
        (object) new IntPtr(compatibleBitmap)
      });
      TcWindow.DeleteObject(compatibleBitmap);
      return image;
    }

    private TcWindow(int processId, string windowName)
    {
      this.mHandle = IntPtr.Zero;
      this.mProcessId = processId;
      this.mWindowName = windowName;
    }

    private bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
    {
      HandleRef handleRef = new HandleRef((object) this, handle);
      int processId;
      TcWindow.GetWindowThreadProcessId(handleRef, out processId);
      if (processId == this.mProcessId)
      {
        StringBuilder lpString = new StringBuilder(100);
        TcWindow.GetWindowText(handleRef, lpString, 100);
        if (lpString.ToString() == this.mWindowName)
        {
          this.mHandle = handle;
          return false;
        }
      }
      return true;
    }

    private IntPtr FindWindow()
    {
      TcWindow.EnumWindows(new TcWindow.EnumThreadWindowsCallback(this.EnumWindowsCallback), IntPtr.Zero);
      return this.mHandle;
    }

    public static IntPtr FindWindow(int processId, string windowName)
    {
      return new TcWindow(processId, windowName).FindWindow();
    }

    public static IntPtr SetWindowParent(IntPtr child, IntPtr parent)
    {
      return TcWindow.SetParent(child, parent);
    }

    public static void SetWindowPosition(IntPtr hwnd, int x, int y)
    {
      TcWindow.SetWindowPos(hwnd, TcWindow.HWND_TOP, x, y, 0, 0, 17U);
    }

    public static void SetWindowVisible(IntPtr hwnd, bool visible)
    {
      TcWindow.ShowWindow(hwnd, visible ? 5 : 0);
    }

    public static void MinimizeWindow(IntPtr hwnd, bool minimized)
    {
      TcWindow.ShowWindow(hwnd, minimized ? 6 : 9);
    }

    public static void CloseWindow(IntPtr ptr)
    {
      TcWindow.PostMessage(ptr, 16, IntPtr.Zero, IntPtr.Zero);
    }

    public static void SetWindowStyle(IntPtr hWnd, bool exStyle, int style)
    {
      TcWindow.SetWindowLong(hWnd, exStyle ? -20 : -16, style);
    }

    public static int GetWindowStyle(IntPtr hWnd, bool exStyle)
    {
      return TcWindow.GetWindowLong(hWnd, exStyle ? -20 : -16);
    }

    public static int SetMdiStyle(IntPtr ptr)
    {
      int dwNewLong = (TcWindow.GetWindowLong(ptr, -20) | 64) & -262145;
      return TcWindow.SetWindowLong(ptr, -20, dwNewLong);
    }

    public static int SetChildStyle(IntPtr ptr, bool b)
    {
      int windowLong = TcWindow.GetWindowLong(ptr, -16);
      int dwNewLong = !b ? windowLong & -1140850689 : 1140850688;
      return TcWindow.SetWindowLong(ptr, -16, dwNewLong);
    }

    private delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);
  }
}
