using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using NetUiAutomationTool.Models;
using NetUiAutomationTool.Services;
using NetUiAutomationTool.Views;

namespace NetUiAutomationTool.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly AutomationService _automationService;
        private readonly DispatcherTimer _spyTimer;
        private AutomationElement? _currentMainWindow;
        private CancellationTokenSource? _treeCts;

        [ObservableProperty]
        private ObservableCollection<TargetProcessInfo> _processes = new();

        [ObservableProperty]
        private TargetProcessInfo? _selectedProcess;

        [ObservableProperty]
        private string _processFilter = string.Empty;

        [ObservableProperty]
        private ObservableCollection<AutomationElementNode> _elementTree = new();

        [ObservableProperty]
        private AutomationElementNode? _selectedNode;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = "準備完了";

        [ObservableProperty]
        private string _actionInputText = string.Empty;

        [ObservableProperty]
        private string _generatedCode = string.Empty;

        [ObservableProperty]
        private bool _isSpyMode;

        [ObservableProperty]
        private string _spyStatusText = "スパイモード: OFF (Ctrl+F12で取得)";

        [ObservableProperty]
        private ObservableCollection<string> _actionLogs = new();

        [ObservableProperty]
        private ObservableCollection<AutomationStep> _scenarioSteps = new();

        [ObservableProperty]
        private AutomationStep? _selectedScenarioStep;

        [ObservableProperty]
        private bool _useUia3 = true;

        public MainViewModel()
        {
            _automationService = new AutomationService();

            _spyTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _spyTimer.Tick += SpyTimer_Tick;

            RefreshProcesses();
        }

        partial void OnIsSpyModeChanged(bool value)
        {
            if (value)
            {
                _spyTimer?.Start();
                SpyStatusText = "スパイモード: ON (マウスホバーで探索中)";
                AddLog("スパイモードを開始しました。ターゲット画面上の要素にマウスを乗せてください。");
            }
            else
            {
                _spyTimer?.Stop();
                HighlightOverlay.HideHighlight();
                SpyStatusText = "スパイモード: OFF (Ctrl+F12で取得)";
                AddLog("スパイモードを停止しました。");
            }
        }

        partial void OnUseUia3Changed(bool value)
        {
            _automationService.UseUia3 = value;
            AddLog($"UI Automation バージョン変更: {(value ? "UIA3 (推奨)" : "UIA2")}");
        }

        partial void OnProcessFilterChanged(string value)
        {
            ApplyProcessFilter();
        }

        partial void OnSelectedNodeChanged(AutomationElementNode? value)
        {
            if (value != null)
            {
                try
                {
                    if (value.BoundingRectangle.Width > 0 && value.BoundingRectangle.Height > 0)
                    {
                        HighlightOverlay.ShowHighlight(value.BoundingRectangle, 1500);
                    }
                }
                catch { }

                // デフォルトの入力テキストを設定（現在の値があれば）
                if (value.ControlType == ControlType.Edit || value.ControlType == ControlType.Document)
                {
                    ActionInputText = _automationService.GetText(value.Element);
                }
                else if (value.ControlType == ControlType.ComboBox)
                {
                    ActionInputText = value.Name;
                }

                UpdateGeneratedSnippet(value);
            }
        }

        private void AddLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
                ActionLogs.Insert(0, entry);
                StatusMessage = message;
            });
        }

        // ==================== プロセス管理 ====================

        [RelayCommand]
        public void RefreshProcesses()
        {
            var list = _automationService.GetRunningProcesses();
            _allProcesses = list;
            ApplyProcessFilter();
            AddLog($"プロセス一覧を更新しました ({list.Count} 件)");
        }

        private List<TargetProcessInfo> _allProcesses = new();

        private void ApplyProcessFilter()
        {
            Processes.Clear();
            var filtered = string.IsNullOrWhiteSpace(ProcessFilter)
                ? _allProcesses
                : _allProcesses.Where(p => p.ProcessName.Contains(ProcessFilter, StringComparison.OrdinalIgnoreCase) ||
                                           p.MainWindowTitle.Contains(ProcessFilter, StringComparison.OrdinalIgnoreCase));

            foreach (var p in filtered)
            {
                Processes.Add(p);
            }
        }

        [RelayCommand]
        public async Task AttachProcessAsync()
        {
            if (SelectedProcess == null)
            {
                MessageBox.Show("対象のプロセスを選択してください。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsBusy = true;
            StatusMessage = $"プロセス '{SelectedProcess.ProcessName}' に接続中...";

            try
            {
                await Task.Run(() =>
                {
                    _currentMainWindow = _automationService.GetMainWindow(SelectedProcess.Id);
                });

                if (_currentMainWindow == null)
                {
                    AddLog($"エラー: プロセス {SelectedProcess.Id} のメインウィンドウを取得できませんでした。");
                    MessageBox.Show("メインウィンドウの取得に失敗しました。管理者権限が必要か、ウィンドウが最小化されていないか確認してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AddLog($"プロセス '{SelectedProcess.ProcessName}' (PID: {SelectedProcess.Id}) に接続しました。");
                await LoadElementTreeAsync();
            }
            catch (Exception ex)
            {
                AddLog($"接続エラー: {ex.Message}");
                MessageBox.Show($"接続中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ==================== UIツリー探索 ====================

        [RelayCommand]
        public async Task LoadElementTreeAsync()
        {
            if (_currentMainWindow == null) return;

            IsBusy = true;
            StatusMessage = "UIツリーを解析・構築中...";
            _treeCts?.Cancel();
            _treeCts = new CancellationTokenSource();

            try
            {
                var token = _treeCts.Token;
                AutomationElementNode? rootNode = null;

                await Task.Run(() =>
                {
                    rootNode = _automationService.BuildTree(_currentMainWindow, 7, token);
                }, token);

                ElementTree.Clear();
                if (rootNode != null)
                {
                    ElementTree.Add(rootNode);
                    AddLog($"UIツリーを構築しました (ルート: {rootNode.DisplayText})");
                }
            }
            catch (OperationCanceledException)
            {
                AddLog("UIツリーの読み込みがキャンセルされました。");
            }
            catch (Exception ex)
            {
                AddLog($"UIツリー構築エラー: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ==================== スパイモード ====================

        [RelayCommand]
        public void ToggleSpyMode()
        {
            IsSpyMode = !IsSpyMode;
        }

        private System.Drawing.Point _lastPoint;
        private void SpyTimer_Tick(object? sender, EventArgs e)
        {
            if (!IsSpyMode) return;

            try
            {
                var p = System.Windows.Forms.Cursor.Position;
                if (Math.Abs(p.X - _lastPoint.X) < 5 && Math.Abs(p.Y - _lastPoint.Y) < 5) return;
                _lastPoint = p;

                var elem = _automationService.GetElementFromPoint(p);
                if (elem != null)
                {
                    var rect = elem.BoundingRectangle;
                    HighlightOverlay.ShowHighlight(rect, 400);
                    StatusMessage = $"[SPY] {elem.ControlType}: \"{elem.Name}\" (ID: {elem.AutomationId})";
                }
            }
            catch { }
        }

        [RelayCommand]
        public void CaptureCurrentCursorElement()
        {
            try
            {
                var p = System.Windows.Forms.Cursor.Position;
                var elem = _automationService.GetElementFromPoint(p);
                if (elem != null)
                {
                    var node = new AutomationElementNode(elem);
                    SelectedNode = node;
                    HighlightOverlay.ShowHighlight(node.BoundingRectangle, 2000);
                    AddLog($"要素をキャプチャしました: {node.DisplayText}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"キャプチャエラー: {ex.Message}");
            }
        }

        // ==================== コントロール直接操作 ====================

        [RelayCommand]
        public async Task ExecuteClickAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);
            
            bool success = await Task.Run(() => _automationService.PerformClick(SelectedNode.Element));
            AddLog(success ? $"クリック成功: {SelectedNode.DisplayText}" : $"クリック失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteDoubleClickAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);
            
            bool success = await Task.Run(() => _automationService.PerformDoubleClick(SelectedNode.Element));
            AddLog(success ? $"ダブルクリック成功: {SelectedNode.DisplayText}" : $"ダブルクリック失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteSetTextAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);

            bool success = await Task.Run(() => _automationService.SetText(SelectedNode.Element, ActionInputText));
            AddLog(success ? $"テキスト設定成功: \"{ActionInputText}\" -> {SelectedNode.DisplayText}" : $"テキスト設定失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteAppendTextAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);

            bool success = await Task.Run(() => _automationService.AppendText(SelectedNode.Element, ActionInputText));
            AddLog(success ? $"テキスト追加成功: \"{ActionInputText}\" -> {SelectedNode.DisplayText}" : $"テキスト追加失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteClearTextAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);

            bool success = await Task.Run(() => _automationService.SetText(SelectedNode.Element, ""));
            AddLog(success ? $"テキストクリア成功: {SelectedNode.DisplayText}" : $"テキストクリア失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteCheckAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);

            bool success = await Task.Run(() => _automationService.SetCheckState(SelectedNode.Element, true));
            AddLog(success ? $"チェックON成功: {SelectedNode.DisplayText}" : $"チェックON失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteUncheckAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);

            bool success = await Task.Run(() => _automationService.SetCheckState(SelectedNode.Element, false));
            AddLog(success ? $"チェックOFF成功: {SelectedNode.DisplayText}" : $"チェックOFF失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteToggleAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);

            bool success = await Task.Run(() => _automationService.PerformClick(SelectedNode.Element));
            AddLog(success ? $"トグル/クリック成功: {SelectedNode.DisplayText}" : $"トグル失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteSelectComboAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);

            bool success = await Task.Run(() => _automationService.SelectComboItem(SelectedNode.Element, ActionInputText));
            AddLog(success ? $"項目選択成功: \"{ActionInputText}\" -> {SelectedNode.DisplayText}" : $"項目選択失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public async Task ExecuteFocusAsync()
        {
            if (SelectedNode == null) return;
            HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 800);

            bool success = await Task.Run(() => _automationService.FocusElement(SelectedNode.Element));
            AddLog(success ? $"フォーカス成功: {SelectedNode.DisplayText}" : $"フォーカス失敗: {SelectedNode.DisplayText}");
        }

        [RelayCommand]
        public void HighlightSelectedElement()
        {
            if (SelectedNode != null)
            {
                HighlightOverlay.ShowHighlight(SelectedNode.BoundingRectangle, 2000);
            }
        }

        // ==================== シナリオ自動化 ====================

        [RelayCommand]
        public void AddSelectedNodeToScenario(string actionTypeStr)
        {
            if (SelectedNode == null)
            {
                MessageBox.Show("追加するUI要素を選択してください。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!Enum.TryParse<StepActionType>(actionTypeStr, out var actionType))
            {
                actionType = StepActionType.Click;
            }

            var locatorType = !string.IsNullOrEmpty(SelectedNode.AutomationId)
                ? ElementLocatorType.AutomationId
                : (!string.IsNullOrEmpty(SelectedNode.Name) ? ElementLocatorType.Name : ElementLocatorType.ClassName);

            var locatorVal = !string.IsNullOrEmpty(SelectedNode.AutomationId)
                ? SelectedNode.AutomationId
                : (!string.IsNullOrEmpty(SelectedNode.Name) ? SelectedNode.Name : SelectedNode.ClassName);

            var step = new AutomationStep
            {
                StepNumber = ScenarioSteps.Count + 1,
                ActionType = actionType,
                LocatorType = locatorType,
                LocatorValue = locatorVal,
                ActionValue = ActionInputText,
                WaitMilliseconds = 500,
                Description = $"{actionType} on {SelectedNode.ControlTypeName} '{SelectedNode.Name}'"
            };

            ScenarioSteps.Add(step);
            AddLog($"シナリオにステップ追加: {step.Summary}");
            UpdateGeneratedScenarioCode();
        }

        [RelayCommand]
        public void AddWaitStep()
        {
            var step = new AutomationStep
            {
                StepNumber = ScenarioSteps.Count + 1,
                ActionType = StepActionType.Wait,
                WaitMilliseconds = 1000,
                Description = "Wait 1000ms"
            };
            ScenarioSteps.Add(step);
            UpdateGeneratedScenarioCode();
        }

        [RelayCommand]
        public void RemoveStep(AutomationStep? step)
        {
            if (step != null && ScenarioSteps.Contains(step))
            {
                ScenarioSteps.Remove(step);
                RenumberSteps();
                UpdateGeneratedScenarioCode();
            }
        }

        [RelayCommand]
        public void ClearSteps()
        {
            ScenarioSteps.Clear();
            UpdateGeneratedScenarioCode();
            AddLog("シナリオの全ステップをクリアしました");
        }

        private void RenumberSteps()
        {
            for (int i = 0; i < ScenarioSteps.Count; i++)
            {
                ScenarioSteps[i].StepNumber = i + 1;
            }
        }

        [RelayCommand]
        public async Task RunScenarioAsync()
        {
            if (_currentMainWindow == null)
            {
                MessageBox.Show("ターゲットプロセスにアタッチしてください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ScenarioSteps.Count == 0)
            {
                MessageBox.Show("実行するシナリオステップがありません。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsBusy = true;
            AddLog($"シナリオ自動実行を開始します (全 {ScenarioSteps.Count} ステップ)...");

            int successCount = 0;
            for (int i = 0; i < ScenarioSteps.Count; i++)
            {
                var step = ScenarioSteps[i];
                SelectedScenarioStep = step;
                StatusMessage = $"実行中 [{i + 1}/{ScenarioSteps.Count}]: {step.Summary}";

                bool ok = await _automationService.ExecuteStepAsync(_currentMainWindow, step);
                if (ok)
                {
                    successCount++;
                    AddLog($"[OK] Step {i + 1}: {step.Summary} -> {step.Status}");
                }
                else
                {
                    AddLog($"[NG] Step {i + 1}: {step.Summary} -> {step.Status}");
                    var result = MessageBox.Show($"ステップ {i + 1} の実行に失敗しました:\n{step.Status}\n\nシナリオの実行を継続しますか？", "ステップエラー", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        break;
                    }
                }
            }

            IsBusy = false;
            AddLog($"シナリオ実行完了: 成功 {successCount}/{ScenarioSteps.Count}");
            MessageBox.Show($"シナリオの実行が完了しました。\n成功: {successCount}/{ScenarioSteps.Count}", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ==================== C# コード生成 ====================

        private void UpdateGeneratedSnippet(AutomationElementNode node)
        {
            GeneratedCode = CodeGeneratorService.GenerateElementActionCode(node, StepActionType.Click, ActionInputText);
        }

        public void UpdateGeneratedScenarioCode()
        {
            string procName = SelectedProcess?.ProcessName ?? "TargetApp";
            GeneratedCode = CodeGeneratorService.GenerateScenarioCode(procName, ScenarioSteps);
        }

        [RelayCommand]
        public void CopyGeneratedCode()
        {
            if (!string.IsNullOrEmpty(GeneratedCode))
            {
                Clipboard.SetText(GeneratedCode);
                AddLog("生成された C# コードをクリップボードにコピーしました。");
            }
        }

        [RelayCommand]
        public void SaveScenario()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Scenario (*.json)|*.json|All files (*.*)|*.*",
                FileName = "automation_scenario.json"
            };
            if (dlg.ShowDialog() == true)
            {
                var json = JsonSerializer.Serialize(ScenarioSteps, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dlg.FileName, json);
                AddLog($"シナリオを保存しました: {dlg.FileName}");
            }
        }

        [RelayCommand]
        public void LoadScenario()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Scenario (*.json)|*.json|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                var json = File.ReadAllText(dlg.FileName);
                var steps = JsonSerializer.Deserialize<List<AutomationStep>>(json);
                if (steps != null)
                {
                    ScenarioSteps.Clear();
                    foreach (var s in steps) ScenarioSteps.Add(s);
                    RenumberSteps();
                    UpdateGeneratedScenarioCode();
                    AddLog($"シナリオを読み込みました ({steps.Count} 件): {dlg.FileName}");
                }
            }
        }

        [RelayCommand]
        public void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        [RelayCommand]
        public void ShowAbout()
        {
            MessageBox.Show(
                "WinForms / .NET UI Automation Studio\n" +
                "バージョン: 1.0.0 (.NET 10 & FlaUI)\n\n" +
                "WinForms / WPF / Win32 / UWP アプリケーションを調査・操作・自動化し、C# (FlaUI) のテスト自動化コードを生成する開発者向けツールです。",
                "バージョン情報",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void Dispose()
        {
            _spyTimer?.Stop();
            _automationService?.Dispose();
        }
    }
}
