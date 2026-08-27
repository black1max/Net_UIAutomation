using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetUiAutomationTool.Models;
using NetUiAutomationTool.Services;

namespace NetUiAutomationTool.Tests
{
    [TestClass]
    public class UiAutomationIntegrationTests
    {
        private Process? _targetProcess;
        private string _sampleAppExe = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            var baseDir = AppContext.BaseDirectory;
            var sampleExePath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\src\SampleWinFormsApp\bin\Debug\net10.0-windows\SampleWinFormsApp.exe"));

            if (!File.Exists(sampleExePath))
            {
                sampleExePath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\src\SampleWinFormsApp\bin\Debug\net10.0-windows\SampleWinFormsApp.exe"));
            }

            _sampleAppExe = sampleExePath;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_targetProcess != null && !_targetProcess.HasExited)
            {
                try { _targetProcess.Kill(); } catch { }
                _targetProcess.Dispose();
            }
        }

        [TestMethod]
        public async Task TestWinFormsAutomationDirectAndScenarioAsync()
        {
            if (!File.Exists(_sampleAppExe))
            {
                Assert.Inconclusive($"Sample WinForms app not found at: {_sampleAppExe}");
                return;
            }

            // 1. サンプルWinFormsアプリを起動
            _targetProcess = Process.Start(_sampleAppExe);
            Assert.IsNotNull(_targetProcess, "プロセス起動に失敗しました");
            Thread.Sleep(1500);

            using var service = new AutomationService();
            var mainWindow = service.GetMainWindow(_targetProcess.Id);
            Assert.IsNotNull(mainWindow, "メインウィンドウの取得に失敗しました");

            // 2. UIツリーの探索
            var tree = service.BuildTree(mainWindow, 5);
            Assert.IsNotNull(tree, "UIツリー構築に失敗しました");
            Assert.IsTrue(tree.Children.Count > 0, "ツリーの子要素が取得できませんでした");

            // 3. テキストボックスの検索とテキスト設定 (ValuePattern / Direct Action)
            var txtUsername = service.FindElement(mainWindow, ElementLocatorType.AutomationId, "txtUsername");
            Assert.IsNotNull(txtUsername, "txtUsername が見つかりません");
            bool textSet = service.SetText(txtUsername, "AutomatedTester");
            Assert.IsTrue(textSet, "テキスト設定に失敗しました");
            string currentText = service.GetText(txtUsername);
            Assert.AreEqual("AutomatedTester", currentText, "設定されたテキストが一致しません");

            // 4. チェックボックスのトグル操作 (TogglePattern)
            var chkAdmin = service.FindElement(mainWindow, ElementLocatorType.AutomationId, "chkIsAdmin");
            Assert.IsNotNull(chkAdmin, "chkIsAdmin が見つかりません");
            bool uncheck = service.SetCheckState(chkAdmin, false);
            Assert.IsTrue(uncheck, "チェックOFF設定に失敗しました");

            // 5. ボタンクリック (InvokePattern)
            var btnSubmit = service.FindElement(mainWindow, ElementLocatorType.AutomationId, "btnSubmit");
            Assert.IsNotNull(btnSubmit, "btnSubmit が見つかりません");
            bool clicked = service.PerformClick(btnSubmit);
            Assert.IsTrue(clicked, "ボタンクリックに失敗しました");

            // 6. 実行ログテキストボックスの検証
            Thread.Sleep(500);
            var txtLog = service.FindElement(mainWindow, ElementLocatorType.AutomationId, "txtLog");
            Assert.IsNotNull(txtLog, "txtLog が見つかりません");
            string logContent = service.GetText(txtLog);
            Assert.IsTrue(logContent.Contains("AutomatedTester"), $"ログに登録内容が含まれていません: {logContent}");

            // 7. シナリオ実行エンジンのテスト
            var scenarioStep1 = new AutomationStep
            {
                StepNumber = 1,
                ActionType = StepActionType.SetText,
                LocatorType = ElementLocatorType.AutomationId,
                LocatorValue = "txtUsername",
                ActionValue = "ScenarioUser99",
                WaitMilliseconds = 100
            };

            var scenarioStep2 = new AutomationStep
            {
                StepNumber = 2,
                ActionType = StepActionType.Click,
                LocatorType = ElementLocatorType.AutomationId,
                LocatorValue = "btnSubmit",
                WaitMilliseconds = 100
            };

            bool step1Ok = await service.ExecuteStepAsync(mainWindow, scenarioStep1);
            Assert.IsTrue(step1Ok, $"Step 1 failed: {scenarioStep1.Status}");

            bool step2Ok = await service.ExecuteStepAsync(mainWindow, scenarioStep2);
            Assert.IsTrue(step2Ok, $"Step 2 failed: {scenarioStep2.Status}");

            // ログに ScenarioUser99 が反映されたか検証
            logContent = service.GetText(txtLog);
            Assert.IsTrue(logContent.Contains("ScenarioUser99"), "シナリオ実行の結果がログに反映されていません");
        }
    }
}
