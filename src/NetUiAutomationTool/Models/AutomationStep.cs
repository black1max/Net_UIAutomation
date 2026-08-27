using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetUiAutomationTool.Models
{
    public enum StepActionType
    {
        Click,
        DoubleClick,
        SetText,
        AppendText,
        ClearText,
        Check,
        Uncheck,
        Toggle,
        SelectComboItem,
        Focus,
        Wait,
        AssertTextEquals,
        AssertTextContains,
        SendKeys
    }

    public enum ElementLocatorType
    {
        AutomationId,
        Name,
        ClassName,
        ControlTypeAndName,
        XPath
    }

    public class AutomationStep : ObservableObject
    {
        private int _stepNumber;
        private StepActionType _actionType = StepActionType.Click;
        private ElementLocatorType _locatorType = ElementLocatorType.AutomationId;
        private string _locatorValue = string.Empty;
        private string _actionValue = string.Empty;
        private int _waitMilliseconds = 500;
        private string _description = string.Empty;
        private string _status = "未実行";
        private bool _isSuccess = true;

        public int StepNumber
        {
            get => _stepNumber;
            set => SetProperty(ref _stepNumber, value);
        }

        public StepActionType ActionType
        {
            get => _actionType;
            set => SetProperty(ref _actionType, value);
        }

        public ElementLocatorType LocatorType
        {
            get => _locatorType;
            set => SetProperty(ref _locatorType, value);
        }

        public string LocatorValue
        {
            get => _locatorValue;
            set => SetProperty(ref _locatorValue, value);
        }

        public string ActionValue
        {
            get => _actionValue;
            set => SetProperty(ref _actionValue, value);
        }

        public int WaitMilliseconds
        {
            get => _waitMilliseconds;
            set => SetProperty(ref _waitMilliseconds, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        [JsonIgnore]
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        [JsonIgnore]
        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        public string Summary
        {
            get
            {
                if (ActionType == StepActionType.Wait)
                {
                    return $"Wait {WaitMilliseconds}ms";
                }
                return $"{ActionType}: [{LocatorType}={LocatorValue}] {(string.IsNullOrEmpty(ActionValue) ? "" : $"-> '{ActionValue}'")}";
            }
        }
    }
}
