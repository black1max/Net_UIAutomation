using System;
using System.Collections.Generic;
using System.Text;
using NetUiAutomationTool.Models;

namespace NetUiAutomationTool.Services
{
    public static class CodeGeneratorService
    {
        public static string GenerateElementActionCode(AutomationElementNode node, StepActionType action, string actionValue)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// ターゲット要素の検索と操作コード (FlaUI / .NET)");
            sb.AppendLine("using FlaUI.Core;");
            sb.AppendLine("using FlaUI.Core.AutomationElements;");
            sb.AppendLine("using FlaUI.UIA3;");
            sb.AppendLine();
            sb.AppendLine("// 1. 初期化とターゲットウィンドウ取得");
            sb.AppendLine("using var automation = new UIA3Automation();");
            sb.AppendLine($"var app = Application.Attach({node.ProcessId}); // または Application.Attach(\"プロセス名\")");
            sb.AppendLine("var mainWindow = app.GetMainWindow(automation);");
            sb.AppendLine();

            string findCode = "";
            if (!string.IsNullOrEmpty(node.AutomationId))
            {
                findCode = $"mainWindow.FindFirstDescendant(cf => cf.ByAutomationId(\"{node.AutomationId}\"))";
            }
            else if (!string.IsNullOrEmpty(node.Name))
            {
                findCode = $"mainWindow.FindFirstDescendant(cf => cf.ByName(\"{node.Name}\"))";
            }
            else if (!string.IsNullOrEmpty(node.ClassName))
            {
                findCode = $"mainWindow.FindFirstDescendant(cf => cf.ByClassName(\"{node.ClassName}\"))";
            }
            else
            {
                findCode = $"mainWindow.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.{node.ControlType}))";
            }

            sb.AppendLine($"// 2. コントロール取得 ({node.ControlTypeName}: {node.Name})");
            sb.AppendLine($"var targetElement = {findCode};");
            sb.AppendLine("if (targetElement == null) throw new Exception(\"Target element not found!\");");
            sb.AppendLine();

            sb.AppendLine("// 3. 操作実行");
            switch (action)
            {
                case StepActionType.Click:
                    sb.AppendLine("if (targetElement.Patterns.Invoke.IsSupported)");
                    sb.AppendLine("    targetElement.Patterns.Invoke.Pattern.Invoke();");
                    sb.AppendLine("else");
                    sb.AppendLine("    targetElement.Click();");
                    break;
                case StepActionType.SetText:
                    sb.AppendLine($"targetElement.AsTextBox().Text = \"{actionValue}\";");
                    break;
                case StepActionType.Check:
                    sb.AppendLine("targetElement.AsCheckBox().IsChecked = true;");
                    break;
                case StepActionType.Uncheck:
                    sb.AppendLine("targetElement.AsCheckBox().IsChecked = false;");
                    break;
                case StepActionType.Toggle:
                    sb.AppendLine("targetElement.Patterns.Toggle.Pattern.Toggle();");
                    break;
                case StepActionType.SelectComboItem:
                    sb.AppendLine($"targetElement.AsComboBox().Select(\"{actionValue}\");");
                    break;
                default:
                    sb.AppendLine("targetElement.Click();");
                    break;
            }

            return sb.ToString();
        }

        public static string GenerateScenarioCode(string processName, IEnumerable<AutomationStep> steps)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// ===========================================================================");
            sb.AppendLine("// WinForms / .NET 自動化シナリオ実行スクリプト (FlaUI C#)");
            sb.AppendLine("// ===========================================================================");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("using FlaUI.Core;");
            sb.AppendLine("using FlaUI.Core.AutomationElements;");
            sb.AppendLine("using FlaUI.UIA3;");
            sb.AppendLine();
            sb.AppendLine("namespace AutomationScript");
            sb.AppendLine("{");
            sb.AppendLine("    public class Program");
            sb.AppendLine("    {");
            sb.AppendLine("        public static void Main()");
            sb.AppendLine("        {");
            sb.AppendLine("            using var automation = new UIA3Automation();");
            sb.AppendLine($"            Console.WriteLine(\"Targetプロセス '{processName}' に接続中...\");");
            sb.AppendLine($"            var app = Application.Attach(\"{processName}\");");
            sb.AppendLine("            var mainWindow = app.GetMainWindow(automation);");
            sb.AppendLine("            if (mainWindow == null) throw new Exception(\"メインウィンドウの取得に失敗しました\");");
            sb.AppendLine();

            int index = 1;
            foreach (var step in steps)
            {
                sb.AppendLine($"            // Step {index++}: {step.Summary}");
                if (step.ActionType == StepActionType.Wait)
                {
                    sb.AppendLine($"            Thread.Sleep({step.WaitMilliseconds});");
                    sb.AppendLine();
                    continue;
                }

                string findQuery = step.LocatorType switch
                {
                    ElementLocatorType.AutomationId => $"cf => cf.ByAutomationId(\"{step.LocatorValue}\")",
                    ElementLocatorType.Name => $"cf => cf.ByName(\"{step.LocatorValue}\")",
                    ElementLocatorType.ClassName => $"cf => cf.ByClassName(\"{step.LocatorValue}\")",
                    _ => $"cf => cf.ByAutomationId(\"{step.LocatorValue}\")"
                };

                string elemVar = $"element{index}";
                sb.AppendLine($"            var {elemVar} = mainWindow.FindFirstDescendant({findQuery});");
                sb.AppendLine($"            if ({elemVar} == null) throw new Exception(\"要素が見つかりません: {step.LocatorValue}\");");

                switch (step.ActionType)
                {
                    case StepActionType.Click:
                        sb.AppendLine($"            if ({elemVar}.Patterns.Invoke.IsSupported) {elemVar}.Patterns.Invoke.Pattern.Invoke(); else {elemVar}.Click();");
                        break;
                    case StepActionType.SetText:
                        sb.AppendLine($"            if ({elemVar}.Patterns.Value.IsSupported) {elemVar}.Patterns.Value.Pattern.SetValue(\"{step.ActionValue}\"); else {elemVar}.AsTextBox().Text = \"{step.ActionValue}\";");
                        break;
                    case StepActionType.AppendText:
                        sb.AppendLine($"            var txt = {elemVar}.Patterns.Value.Pattern.Value.ValueOrDefault ?? \"\";");
                        sb.AppendLine($"            {elemVar}.Patterns.Value.Pattern.SetValue(txt + \"{step.ActionValue}\");");
                        break;
                    case StepActionType.ClearText:
                        sb.AppendLine($"            {elemVar}.Patterns.Value.Pattern.SetValue(\"\");");
                        break;
                    case StepActionType.Check:
                        sb.AppendLine($"            {elemVar}.AsCheckBox().IsChecked = true;");
                        break;
                    case StepActionType.Uncheck:
                        sb.AppendLine($"            {elemVar}.AsCheckBox().IsChecked = false;");
                        break;
                    case StepActionType.Toggle:
                        sb.AppendLine($"            {elemVar}.Patterns.Toggle.Pattern.Toggle();");
                        break;
                    case StepActionType.SelectComboItem:
                        sb.AppendLine($"            {elemVar}.AsComboBox().Select(\"{step.ActionValue}\");");
                        break;
                    case StepActionType.Focus:
                        sb.AppendLine($"            {elemVar}.Focus();");
                        break;
                }

                if (step.WaitMilliseconds > 0)
                {
                    sb.AppendLine($"            Thread.Sleep({step.WaitMilliseconds});");
                }
                sb.AppendLine();
            }

            sb.AppendLine("            Console.WriteLine(\"自動化シナリオの実行が完了しました。\");");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
