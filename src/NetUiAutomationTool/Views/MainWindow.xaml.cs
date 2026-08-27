using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using NetUiAutomationTool.Models;
using NetUiAutomationTool.ViewModels;

namespace NetUiAutomationTool.Views
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_F12 = 0x7B;

        private HwndSource? _source;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source?.AddHook(HwndHook);

            // Ctrl + F12 でカーソル下の要素をスパイキャプチャ
            RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CONTROL, VK_F12);
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
            _source?.RemoveHook(HwndHook);

            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.CaptureCurrentCursorElementCommand.Execute(null);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel vm && e.NewValue is AutomationElementNode node)
            {
                vm.SelectedNode = node;
            }
        }
    }
}
