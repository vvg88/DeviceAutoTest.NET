using System.Collections.Generic;
using NeuroSoft.DeviceAutoTest.Controls;

namespace NeuroSoft.DeviceAutoTest.Controls
{
    public interface IAutoCompleteDataProvider
    {
        IEnumerable<AutoCompleteItem> GetItems(string textPattern);
    }
}
