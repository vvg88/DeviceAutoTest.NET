using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuroSoft.DeviceAutoTest.Controls
{
    public interface IAutoAppendDataProvider
    {
        string GetAppendText(string textPattern, string firstMatch);
    }
}
