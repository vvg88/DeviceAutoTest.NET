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
    public class IntegerVariableQuery : VariableQueryControl
    {
        static IntegerVariableQuery()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IntegerVariableQuery), new FrameworkPropertyMetadata(typeof(IntegerVariableQuery)));
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="variable"></param>
        public IntegerVariableQuery(DATVariable variable) : base(variable) { }
    }
}
