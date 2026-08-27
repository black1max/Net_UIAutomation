using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiagnosticsDebug = System.Diagnostics.Debug;
using Process = System.Diagnostics.Process;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using FlaUI.UIA2;
using NetUiAutomationTool.Models;

namespace NetUiAutomationTool.Services
{
    public class AutomationService : IDisposable
    {
        private AutomationBase _automation;
        private bool _useUia3 = true;

        public bool UseUia3
        {
            get => _useUia3;
            set
            {
                if (_useUia3 != value)
                {
                    _useUia3 = value;
                    ReinitializeAutomation();
                }
            }
        }

        public AutomationService()
        {
            _automation = new UIA3Automation();
        }

        private void ReinitializeAutomation()
        {
            _automation?.Dispose();
            _automation = _useUia3 ? new UIA3Automation() : new UIA2Automation();
        }

        public List<TargetProcessInfo> GetRunningProcesses()
        {
            var list = new List<TargetProcessInfo>();
            var processes = Process.GetProcesses();

            foreach (var p in processes)
            {
                try
                {
                    if (p.MainWindowHandle != IntPtr.Zero && !string.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        list.Add(TargetProcessInfo.FromProcess(p));
                    }
                }
                catch
                {
                    // アクセス拒否など
                }
            }

            return list.OrderBy(p => p.ProcessName).ToList();
        }

        public AutomationElement? GetMainWindow(int processId)
        {
            try
            {
                var app = FlaUI.Core.Application.Attach(processId);
                return app.GetMainWindow(_automation);
            }
            catch (Exception ex)
            {
                DiagnosticsDebug.WriteLine($"Failed to get main window: {ex.Message}");
                return null;
            }
        }

        public AutomationElement? GetDesktopElement()
        {
            try
            {
                return _automation.GetDesktop();
            }
            catch
            {
                return null;
            }
        }

        public AutomationElementNode BuildTree(AutomationElement rootElement, int maxDepth = 6, CancellationToken ct = default)
        {
            var rootNode = new AutomationElementNode(rootElement);
            PopulateChildren(rootNode, 1, maxDepth, ct);
            return rootNode;
        }

        private void PopulateChildren(AutomationElementNode parentNode, int currentDepth, int maxDepth, CancellationToken ct)
        {
            if (currentDepth > maxDepth || ct.IsCancellationRequested) return;

            try
            {
                var children = parentNode.Element.FindAllChildren();
                foreach (var child in children)
                {
                    if (ct.IsCancellationRequested) break;

                    try
                    {
                        var childNode = new AutomationElementNode(child);
                        parentNode.Children.Add(childNode);
                        PopulateChildren(childNode, currentDepth + 1, maxDepth, ct);
                    }
                    catch
                    {
                        // COMエラー等の要素スキップ
                    }
                }
            }
            catch
            {
                // FindAllChildren 失敗時
            }
        }

        public AutomationElement? GetElementFromPoint(Point screenPoint)
        {
            try
            {
                return _automation.FromPoint(screenPoint);
            }
            catch
            {
                return null;
            }
        }

        // ==================== コントロール直接操作 ====================

        public bool PerformClick(AutomationElement element)
        {
            if (element == null) return false;

            try
            {
                // InvokePattern
                if (element.Patterns.Invoke.IsSupported)
                {
                    element.Patterns.Invoke.Pattern.Invoke();
                    return true;
                }

                // TogglePattern
                if (element.Patterns.Toggle.IsSupported)
                {
                    element.Patterns.Toggle.Pattern.Toggle();
                    return true;
                }

                // SelectionItemPattern
                if (element.Patterns.SelectionItem.IsSupported)
                {
                    element.Patterns.SelectionItem.Pattern.Select();
                    return true;
                }

                // LegacyIAccessiblePattern
                if (element.Patterns.LegacyIAccessible.IsSupported)
                {
                    element.Patterns.LegacyIAccessible.Pattern.DoDefaultAction();
                    return true;
                }

                // マウスクリックにフォールバック
                element.Click();
                return true;
            }
            catch (Exception ex)
            {
                DiagnosticsDebug.WriteLine($"Click failed: {ex.Message}");
                try
                {
                    element.Click();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool PerformDoubleClick(AutomationElement element)
        {
            if (element == null) return false;
            try
            {
                element.DoubleClick();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetText(AutomationElement element, string text)
        {
            if (element == null) return false;

            try
            {
                // ValuePattern
                if (element.Patterns.Value.IsSupported)
                {
                    element.Patterns.Value.Pattern.SetValue(text);
                    return true;
                }

                // LegacyIAccessiblePattern
                if (element.Patterns.LegacyIAccessible.IsSupported)
                {
                    element.Patterns.LegacyIAccessible.Pattern.SetValue(text);
                    return true;
                }

                // TextBoxコントロールヘルパー
                var tb = element.AsTextBox();
                if (tb != null)
                {
                    tb.Text = text;
                    return true;
                }

                // キー入力フォールバック
                element.Focus();
                Keyboard.Type(text);
                return true;
            }
            catch (Exception ex)
            {
                DiagnosticsDebug.WriteLine($"SetText failed: {ex.Message}");
                try
                {
                    element.Focus();
                    Keyboard.Type(text);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool AppendText(AutomationElement element, string text)
        {
            if (element == null) return false;
            try
            {
                string currentText = GetText(element);
                return SetText(element, currentText + text);
            }
            catch
            {
                return false;
            }
        }

        public string GetText(AutomationElement element)
        {
            if (element == null) return string.Empty;

            try
            {
                if (element.Patterns.Value.IsSupported)
                {
                    return element.Patterns.Value.Pattern.Value.ValueOrDefault ?? string.Empty;
                }

                if (element.Patterns.Text.IsSupported)
                {
                    return element.Patterns.Text.Pattern.DocumentRange.GetText(10000);
                }

                if (element.Patterns.LegacyIAccessible.IsSupported)
                {
                    return element.Patterns.LegacyIAccessible.Pattern.Value.ValueOrDefault ?? string.Empty;
                }

                return element.Name ?? string.Empty;
            }
            catch
            {
                return element.Name ?? string.Empty;
            }
        }

        public bool SetCheckState(AutomationElement element, bool isChecked)
        {
            if (element == null) return false;

            try
            {
                if (element.Patterns.Toggle.IsSupported)
                {
                    var toggle = element.Patterns.Toggle.Pattern;
                    var current = toggle.ToggleState.ValueOrDefault;
                    if ((isChecked && current != ToggleState.On) || (!isChecked && current != ToggleState.Off))
                    {
                        toggle.Toggle();
                    }
                    return true;
                }

                var cb = element.AsCheckBox();
                if (cb != null)
                {
                    cb.IsChecked = isChecked;
                    return true;
                }

                element.Click();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SelectComboItem(AutomationElement element, string itemText)
        {
            if (element == null) return false;

            try
            {
                var combo = element.AsComboBox();
                if (combo != null)
                {
                    combo.Expand();
                    Thread.Sleep(100);
                    var item = combo.Items.FirstOrDefault(i => string.Equals(i.Name, itemText, StringComparison.OrdinalIgnoreCase) || (i.Text != null && i.Text.Contains(itemText)));
                    if (item != null)
                    {
                        item.Select();
                        combo.Collapse();
                        return true;
                    }
                    combo.Collapse();
                }

                // 子要素探索による選択
                var targetItem = element.FindFirstDescendant(cf => cf.ByName(itemText));
                if (targetItem != null && targetItem.Patterns.SelectionItem.IsSupported)
                {
                    targetItem.Patterns.SelectionItem.Pattern.Select();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool FocusElement(AutomationElement element)
        {
            if (element == null) return false;
            try
            {
                element.Focus();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== セレクタによる検索 ====================

        public AutomationElement? FindElement(AutomationElement root, ElementLocatorType locatorType, string value)
        {
            if (root == null || string.IsNullOrEmpty(value)) return null;

            try
            {
                var cf = _automation.ConditionFactory;
                return locatorType switch
                {
                    ElementLocatorType.AutomationId => root.FindFirstDescendant(cf.ByAutomationId(value)),
                    ElementLocatorType.Name => root.FindFirstDescendant(cf.ByName(value)),
                    ElementLocatorType.ClassName => root.FindFirstDescendant(cf.ByClassName(value)),
                    ElementLocatorType.XPath => root.FindFirstByXPath(value),
                    _ => root.FindFirstDescendant(cf.ByAutomationId(value)) ?? root.FindFirstDescendant(cf.ByName(value))
                };
            }
            catch
            {
                return null;
            }
        }

        // ==================== シナリオステップ実行 ====================

        public async Task<bool> ExecuteStepAsync(AutomationElement root, AutomationStep step)
        {
            if (step.ActionType == StepActionType.Wait)
            {
                step.Status = $"待機中 ({step.WaitMilliseconds}ms)...";
                await Task.Delay(step.WaitMilliseconds);
                step.Status = "完了";
                step.IsSuccess = true;
                return true;
            }

            step.Status = $"要素検索中: {step.LocatorType}={step.LocatorValue}";
            var element = await Task.Run(() => FindElement(root, step.LocatorType, step.LocatorValue));

            if (element == null)
            {
                step.Status = "エラー: 要素が見つかりませんでした";
                step.IsSuccess = false;
                return false;
            }

            // ハイライト表示
            try
            {
                var bounds = element.BoundingRectangle;
                HighlightOverlay.ShowHighlight(bounds, 800);
            }
            catch { }

            bool success = false;
            string message = "";

            await Task.Run(() =>
            {
                try
                {
                    switch (step.ActionType)
                    {
                        case StepActionType.Click:
                            success = PerformClick(element);
                            message = success ? "クリック成功" : "クリック失敗";
                            break;

                        case StepActionType.DoubleClick:
                            success = PerformDoubleClick(element);
                            message = success ? "ダブルクリック成功" : "ダブルクリック失敗";
                            break;

                        case StepActionType.SetText:
                            success = SetText(element, step.ActionValue);
                            message = success ? $"テキスト設定成功: '{step.ActionValue}'" : "テキスト設定失敗";
                            break;

                        case StepActionType.AppendText:
                            success = AppendText(element, step.ActionValue);
                            message = success ? $"テキスト追加成功: '{step.ActionValue}'" : "テキスト追加失敗";
                            break;

                        case StepActionType.ClearText:
                            success = SetText(element, "");
                            message = success ? "テキストクリア成功" : "テキストクリア失敗";
                            break;

                        case StepActionType.Check:
                            success = SetCheckState(element, true);
                            message = success ? "チェック成功" : "チェック失敗";
                            break;

                        case StepActionType.Uncheck:
                            success = SetCheckState(element, false);
                            message = success ? "チェック解除成功" : "チェック解除失敗";
                            break;

                        case StepActionType.Toggle:
                            if (element.Patterns.Toggle.IsSupported)
                            {
                                element.Patterns.Toggle.Pattern.Toggle();
                                success = true;
                                message = "トグル切り替え成功";
                            }
                            else
                            {
                                success = PerformClick(element);
                                message = "クリック実行";
                            }
                            break;

                        case StepActionType.SelectComboItem:
                            success = SelectComboItem(element, step.ActionValue);
                            message = success ? $"コンボ項目選択成功: '{step.ActionValue}'" : "コンボ項目選択失敗";
                            break;

                        case StepActionType.Focus:
                            success = FocusElement(element);
                            message = success ? "フォーカス設定成功" : "フォーカス設定失敗";
                            break;

                        case StepActionType.AssertTextEquals:
                            var curText1 = GetText(element);
                            success = string.Equals(curText1, step.ActionValue, StringComparison.Ordinal);
                            message = success ? $"検証一致: '{curText1}'" : $"検証不一致: 実際='{curText1}', 期待='{step.ActionValue}'";
                            break;

                        case StepActionType.AssertTextContains:
                            var curText2 = GetText(element);
                            success = curText2.Contains(step.ActionValue);
                            message = success ? $"検証含有人致: '{curText2}'" : $"検証未含有: 実際='{curText2}', 期待含有='{step.ActionValue}'";
                            break;

                        case StepActionType.SendKeys:
                            FocusElement(element);
                            Keyboard.Type(step.ActionValue);
                            success = true;
                            message = $"キー送信成功: '{step.ActionValue}'";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    message = $"例外発生: {ex.Message}";
                }
            });

            step.IsSuccess = success;
            step.Status = message;

            if (step.WaitMilliseconds > 0)
            {
                await Task.Delay(step.WaitMilliseconds);
            }

            return success;
        }

        public void Dispose()
        {
            _automation?.Dispose();
        }
    }
}
