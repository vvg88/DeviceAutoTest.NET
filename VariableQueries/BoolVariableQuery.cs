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
    public class BoolVariableQuery : VariableQueryControl
    {
        static BoolVariableQuery()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BoolVariableQuery), new FrameworkPropertyMetadata(typeof(BoolVariableQuery)));
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="variable"></param>
        public BoolVariableQuery(DATVariable variable) : base(variable) { }
    }
}
