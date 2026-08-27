using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetUiAutomationTool.Services
{
    public class HighlightOverlay
    {
        private static Window? _overlayWindow;
        private static Border? _highlightBorder;
        private static CancellationTokenSource? _cts;

        public static void ShowHighlight(System.Drawing.Rectangle rect, int durationMs = 2000)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            Application.Current?.Dispatcher?.Invoke(() =>
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var token = _cts.Token;

                if (_overlayWindow == null)
                {
                    _highlightBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 255, 30, 30)),
                        BorderThickness = new Thickness(3),
                        Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 255, 50, 50)),
                        CornerRadius = new CornerRadius(3)
                    };

                    var canvas = new Canvas();
                    canvas.Children.Add(_highlightBorder);

                    _overlayWindow = new Window
                    {
                        WindowStyle = WindowStyle.None,
                        AllowsTransparency = true,
                        Background = System.Windows.Media.Brushes.Transparent,
                        Topmost = true,
                        ShowInTaskbar = false,
                        IsHitTestVisible = false,
                        Left = 0,
                        Top = 0,
                        Width = SystemParameters.VirtualScreenWidth,
                        Height = SystemParameters.VirtualScreenHeight,
                        Content = canvas
                    };
                }

                if (!_overlayWindow.IsVisible)
                {
                    _overlayWindow.Show();
                }

                Canvas.SetLeft(_highlightBorder, rect.Left - 2);
                Canvas.SetTop(_highlightBorder, rect.Top - 2);
                _highlightBorder!.Width = rect.Width + 4;
                _highlightBorder!.Height = rect.Height + 4;
                _highlightBorder.Opacity = 1.0;

                if (durationMs > 0)
                {
                    Task.Delay(durationMs, token).ContinueWith(t =>
                    {
                        if (!t.IsCanceled)
                        {
                            Application.Current?.Dispatcher?.Invoke(HideHighlight);
                        }
                    }, TaskScheduler.Default);
                }
            });
        }

        public static void HideHighlight()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (_overlayWindow != null && _overlayWindow.IsVisible)
                {
                    _overlayWindow.Hide();
                }
            });
        }
    }
}
