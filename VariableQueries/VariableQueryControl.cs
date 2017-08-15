using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Контрол, запрашивающий значение переменной
    /// </summary>
    [System.ComponentModel.DesignTimeVisible(false)]
    public class VariableQueryControl : ContentControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="variable"></param>
        public VariableQueryControl(DATVariable variable)
        {
            Variable = variable;
        }

        private DATVariable variable;

        /// <summary>
        /// Ссылка на переменную
        /// </summary>
        public DATVariable Variable
        {
            get { return variable; }
            set
            {
                if (variable != value)
                {
                    variable = value;
                    OnPropertyChanged("Variable");
                }
            }
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Событие на изменение свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Уведомление об изменении свойства (все объекты представления привязаные к этому свойству автоматически обновят себя)
        /// </summary>
        /// <param name="propertyName">Имя свойства принимающего новое значение</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static VariableQueryControl CreateVariableQuery(DATVariable variable)
        {
            VariableQueryControl instance = null;
            switch (variable.TestVariableType)
            {
                case DATVariableType.Boolean:
                    instance = new BoolVariableQuery(variable);
                    break;
                case DATVariableType.Double:
                    instance = new DoubleVariableQuery(variable);
                    break;
                case DATVariableType.Integer:
                    instance = new IntegerVariableQuery(variable);
                    break;
                case DATVariableType.String:
                    instance = new StringVariableQuery(variable);
                    break;
            }
            return instance;
        }
    }
}
