using System;
using System.Collections.Generic;
using NetUiAutomationTool.Models;

namespace NetUiAutomationTool.ViewModels
{
    public static class MainViewModelHelper
    {
        public static IEnumerable<StepActionType> ActionTypes =>
            (StepActionType[])Enum.GetValues(typeof(StepActionType));

        public static IEnumerable<ElementLocatorType> LocatorTypes =>
            (ElementLocatorType[])Enum.GetValues(typeof(ElementLocatorType));
    }
}
