# WinForms / .NET UI Automation Studio (.NET 10 WPF)

.NET 10 (C#) と WPF で構築された、外部の .NET (.NET Framework / WinForms / WPF 等) アプリケーションのUI要素を調査・操作・自動化するための高機能GUIツールです。

---

## 🌟 主な機能

1. **プロセス選択 & アタッチ**
   - 起動中のプロセス一覧をリアルタイム取得・絞り込み検索
   - ワンクリックでターゲットプロセス・メインウィンドウに接続
   - テスト用サンプルWinFormsアプリの起動ボタンを内蔵

2. **UI Automation ツリー探索 (Inspector)**
   - ターゲットウィンドウ内のUI要素をツリー階層構造（Hierarchy）で展開表示
   - コントロールタイプ（Button, Edit, ComboBox, CheckBox等）、AutomationId、Name、ClassName、Handle、BoundingRectangle、対応パターンをインスペクト
   - **画面オーバーレイ・ハイライト表示**: 選択中のUI要素を画面上で赤枠強調表示

3. **リアルタイム スパイ機能 (Spy Mode)**
   - スパイモード有効化時、マウスカーソルを乗せた画面上のコントロールを自動検知してハイライト
   - グローバルホットキー (`Ctrl + F12`) でカーソル下の要素を即座にキャプチャ・選択

4. **コントロール直接操作 (Direct Actions)**
   - 🖱️ **クリック / Invoke**: ボタンの押下 (`InvokePattern` / `LegacyIAccessiblePattern` / クリック)
   - 🖱️🖱️ **ダブルクリック**: リスト行やアイコンのダブルクリック
   - ✍️ **テキスト設定・追加・クリア**: テキストボックスへの値設定 (`ValuePattern` / `TextPattern` / キーストローク)
   - ☑️ **チェックON / OFF / トグル**: チェックボックスやラジオボタンの状態制御 (`TogglePattern`)
   - 🔽 **コンボボックス選択**: ドロップダウンの展開と項目選択 (`SelectionPattern` / `ExpandCollapsePattern`)
   - 🎯 **フォーカス**: コントロールへのフォーカス移動

5. **自動化シナリオビルダー (Scenario Runner)**
   - 要素の操作手順（検索 ➔ 入力 ➔ クリック ➔ 待機等）をGUI上で組み立て
   - ワンクリックでシナリオを自動実行
   - シナリオの JSON 保存・読み込み対応

6. **C# 自動化コード生成 (CodeGen)**
   - 選択した要素の操作コード、または作成したシナリオ全体のスクリプトを `FlaUI.UIA3` の C# コードとしてリアルタイム生成
   - クリップボードにワンクリックでコピーして自身のプロジェクトにすぐ組み込み可能

---

## 📁 プロジェクト構成

```
c:\TEMP\Net_UIAutomation\
├── NetUiAutomationTool.slnx             # ソリューションファイル (.NET 10)
├── src/
│   ├── NetUiAutomationTool/             # メインGUIツール (WPF / .NET 10)
│   │   ├── Models/                      # AutomationElementNode, AutomationStep, TargetProcessInfo
│   │   ├── Services/                    # AutomationService, HighlightOverlay, CodeGeneratorService
│   │   ├── ViewModels/                  # MainViewModel
│   │   ├── Views/                       # MainWindow.xaml, ObjectToVisibilityConverter
│   │   └── Styles/                      # ModernStyles.xaml
│   └── SampleWinFormsApp/               # テスト用 WinForms アプリ (.NET 10)
│       └── MainForm.cs                  # 各種WinForms標準コントロール搭載画面
└── tests/
    └── NetUiAutomationTool.Tests/       # UI Automation統合テスト (MSTest)
```

---

## 🚀 起動方法

### GUIツールの起動
```bash
dotnet run --project src/NetUiAutomationTool/NetUiAutomationTool.csproj
```

### テスト用WinFormsアプリの起動 (単体)
```bash
dotnet run --project src/SampleWinFormsApp/SampleWinFormsApp.csproj
```

### 統合テストの実行
```bash
dotnet test
```
