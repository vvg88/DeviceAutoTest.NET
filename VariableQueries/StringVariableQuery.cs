using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Контрол, запрашивающий значение переменной
    /// </summary>
    [System.ComponentModel.DesignTimeVisible(false)]
    public class StringVariableQuery : VariableQueryControl
    {
        static StringVariableQuery()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StringVariableQuery), new FrameworkPropertyMetadata(typeof(StringVariableQuery)));
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="variable"></param>
        public StringVariableQuery(DATVariable variable) : base(variable) { }
    }
}
