using System;
using System.Collections.ObjectModel;
using System.Drawing;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace NetUiAutomationTool.Models
{
    public class AutomationElementNode
    {
        public AutomationElement Element { get; }
        
        public string Name { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public ControlType ControlType { get; set; }
        public string ControlTypeName { get; set; } = string.Empty;
        public string HelpText { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsOffscreen { get; set; }
        public Rectangle BoundingRectangle { get; set; }
        public int ProcessId { get; set; }
        public IntPtr NativeWindowHandle { get; set; }
        public string PatternsSummary { get; set; } = string.Empty;

        public ObservableCollection<AutomationElementNode> Children { get; set; } = new();

        public string DisplayText
        {
            get
            {
                var type = ControlTypeName;
                if (string.IsNullOrEmpty(type)) type = "Element";

                string namePart = string.IsNullOrEmpty(Name) ? "" : $" \"{Name}\"";
                string idPart = string.IsNullOrEmpty(AutomationId) ? "" : $" [ID: {AutomationId}]";
                return $"{type}{namePart}{idPart}";
            }
        }

        public string DetailedInfo
        {
            get
            {
                return $"ControlType: {ControlTypeName}\n" +
                       $"Name: {Name}\n" +
                       $"AutomationId: {AutomationId}\n" +
                       $"ClassName: {ClassName}\n" +
                       $"Handle: 0x{NativeWindowHandle.ToInt64():X}\n" +
                       $"Enabled: {IsEnabled}, Offscreen: {IsOffscreen}\n" +
                       $"Bounds: {BoundingRectangle}\n" +
                       $"Supported Patterns: {PatternsSummary}";
            }
        }

        public AutomationElementNode(AutomationElement element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            LoadProperties();
        }

        private void LoadProperties()
        {
            try { Name = Element.Properties.Name.ValueOrDefault ?? string.Empty; } catch { }
            try { AutomationId = Element.Properties.AutomationId.ValueOrDefault ?? string.Empty; } catch { }
            try { ClassName = Element.Properties.ClassName.ValueOrDefault ?? string.Empty; } catch { }
            try 
            { 
                ControlType = Element.Properties.ControlType.ValueOrDefault;
                ControlTypeName = ControlType.ToString();
            } 
            catch { ControlTypeName = "Unknown"; }

            try { HelpText = Element.Properties.HelpText.ValueOrDefault ?? string.Empty; } catch { }
            try { ItemType = Element.Properties.ItemType.ValueOrDefault ?? string.Empty; } catch { }
            try { IsEnabled = Element.Properties.IsEnabled.ValueOrDefault; } catch { }
            try { IsOffscreen = Element.Properties.IsOffscreen.ValueOrDefault; } catch { }
            try { BoundingRectangle = Element.Properties.BoundingRectangle.ValueOrDefault; } catch { }
            try { ProcessId = Element.Properties.ProcessId.ValueOrDefault; } catch { }
            try { NativeWindowHandle = Element.Properties.NativeWindowHandle.ValueOrDefault; } catch { }

            try
            {
                var patterns = Element.GetSupportedPatternsDirect();
                PatternsSummary = patterns != null && patterns.Length > 0
                    ? string.Join(", ", Array.ConvertAll(patterns, p => p.Name.Replace("Pattern", "")))
                    : "None";
            }
            catch
            {
                PatternsSummary = "N/A";
            }
        }
    }
}
