using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace NetUiAutomationTool.Models
{
    public class TargetProcessInfo
    {
        public int Id { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string MainWindowTitle { get; set; } = string.Empty;
        public IntPtr MainWindowHandle { get; set; }
        public string DisplayText => $"[{Id}] {ProcessName} - {MainWindowTitle}";

        public static TargetProcessInfo FromProcess(Process p)
        {
            return new TargetProcessInfo
            {
                Id = p.Id,
                ProcessName = p.ProcessName,
                MainWindowTitle = p.MainWindowTitle,
                MainWindowHandle = p.MainWindowHandle
            };
        }
    }
}
