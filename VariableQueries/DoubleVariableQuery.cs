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
    public class DoubleVariableQuery : VariableQueryControl
    {
        static DoubleVariableQuery()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DoubleVariableQuery), new FrameworkPropertyMetadata(typeof(DoubleVariableQuery)));
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="variable"></param>
        public DoubleVariableQuery(DATVariable variable) : base(variable) { }
    }
}
