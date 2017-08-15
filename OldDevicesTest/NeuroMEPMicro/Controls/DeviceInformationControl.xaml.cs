using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for DeviceInformationControl.xaml
    /// </summary>
    public partial class DeviceInformationControl : UserControl
    {
        public DeviceInformationControl(ScriptEnvironment environment)
        {
            InitializeComponent();
            DataContext = this;
            InitDeviceInfoCollection(environment);
        }
        /// <summary>
        /// Коллекция элементов с информацией о приборе
        /// </summary>
        private ObservableCollection<DeviceInfoItem> deviceInfoItems = new ObservableCollection<DeviceInfoItem>();
        /// <summary>
        /// Свойство для получения доступа к коллекции с информацией о приборе
        /// </summary>
        public ObservableCollection<DeviceInfoItem> DeviceInfoItems
        {
            get { return deviceInfoItems; }
        }

        /// <summary>
        /// Инициализирует список с пунктами информации о приборе
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        private void InitDeviceInfoCollection(ScriptEnvironment environment)
        {
            NeuroMEPMicro device = environment.Device as NeuroMEPMicro;
            if (device != null)
            {
                deviceInfoItems.Add(new DeviceInfoItem("Имя устройства", device.DeviceInformation.DeviceName));
                deviceInfoItems.Add(new DeviceInfoItem("Серийный номер прибора", int.Parse(device.GetDeviceState().SerialNo), int.Parse(environment.DeviceSerialNumber) - 1, 
                                    int.Parse(environment.DeviceSerialNumber) + 1));
                deviceInfoItems.Add(new DeviceInfoItem("Версия прибора", (int)device.DeviceInformation.DeviceVersia, 0, 100));
                deviceInfoItems.Add(new DeviceInfoItem("Неисправность в устройстве", (int)device.DeviceInformation.DeviceAlarm));
                deviceInfoItems.Add(new DeviceInfoItem("Код опорного напряжения +2,5В", (int)device.DeviceInformation.Opornoe, 24400, 24800));
                deviceInfoItems.Add(new DeviceInfoItem("Код тестового сигнала измерения импеданса +", (int)device.DeviceInformation.U_low_plus, 2500, 4500));
                deviceInfoItems.Add(new DeviceInfoItem("Код тестового сигнала измерения импеданса -", (int)device.DeviceInformation.U_low_minus, -4500, -2500));
                deviceInfoItems.Add(new DeviceInfoItem("Код тестового сигнала калибровочного сигнала +", (int)device.DeviceInformation.U_high_plus, -500, +500));
                deviceInfoItems.Add(new DeviceInfoItem("Код тестового сигнала калибровочного сигнала -", (int)device.DeviceInformation.U_high_minus, -500, +500));
                deviceInfoItems.Add(new DeviceInfoItem("Информация о модуле стимулятора", (int)device.DeviceInformation.info_stim));
                deviceInfoItems.Add(new DeviceInfoItem("Информация о модуле клавиатуры", (int)device.DeviceInformation.info_klav));
                deviceInfoItems.Add(new DeviceInfoItem("Версия модуля клавиатуры", (byte)device.DeviceInformation.versia_klav));
                deviceInfoItems.Add(new DeviceInfoItem("Состояние кнопок модуля клавиатуры", (int)device.DeviceInformation.klav_work));
                deviceInfoItems.Add(new DeviceInfoItem("Состояние кнопок или педалей на разъеме XS1", (int)device.DeviceInformation.info_ra0));
                deviceInfoItems.Add(new DeviceInfoItem("Состояние кнопок или педалей на разъеме XS2", (int)device.DeviceInformation.info_ra1));
            }
        }

        /// <summary>
        /// Проверяет, соответствуют ли значения требованиям. Если не соответствует, возвращает сообщение об ошибке
        /// </summary>
        /// <param name="strinErroreMessage"> Сообщение об ошибке </param>
        /// <returns> Результат оценки значений </returns>
        public bool? CheckDeviceInformationValid(out string strinErroreMessage)
        {
            foreach (DeviceInfoItem devInfItem in deviceInfoItems)
            {
                if (!devInfItem.IsValid)
                {
                    strinErroreMessage = "Значение '" + devInfItem.ParamName + "' не соответствует требованиям.";
                    return null;
                }
            }
            strinErroreMessage = "";
            return true;
        }
    }

    /// <summary>
    /// Класс для элементов информации о приборе
    /// </summary>
    public class DeviceInfoItem : INotifyPropertyChanged
    {
        #region Конструкторы

        /// <summary>
        /// Конструктор для параметров со строковым значением
        /// </summary>
        /// <param name="paramName"> Имя параметра </param>
        /// <param name="paramValue"> Значение параметра </param>
        public DeviceInfoItem(string paramName, string paramValue)
        {
            ParamName = paramName;
            ParamValue = paramValue;
            IsValid = true;
        }
        /// <summary>
        /// Конструктор для значений, которые имеют ограниченное значение
        /// </summary>
        /// <param name="paramName"> Имя параметра </param>
        /// <param name="paramValue"> Значение параметра </param>
        /// <param name="minValue"> Минимальное значение </param>
        /// <param name="maxValue"> Максимальное значение </param>
        public DeviceInfoItem(string paramName, int paramValue, int minValue, int maxValue)
        {
            ParamName = paramName;
            ParamValue = paramValue.ToString();
            RangedValue<int> rangeParamValue = new RangedValue<int>(paramValue, minValue, maxValue);
            if (!rangeParamValue.IsValidValue)
                IsValid = false;
            else
                IsValid = true;
        }
        /// <summary>
        /// Конструктор для параметров со строковым значением
        /// </summary>
        /// <param name="paramName"> Имя параметра </param>
        /// <param name="paramValue"> Значение параметра </param>
        public DeviceInfoItem(string paramName, int paramValue)
        {
            ParamName = paramName;
            ParamValue = paramValue.ToString("X");
            IsValid = true;
        }

        #endregion

        /// <summary>
        /// Имя параметра информации о приборе
        /// </summary>
        private string paramName;
        /// <summary>
        /// Имя параметра информации о приборе
        /// </summary>
        public string ParamName 
        {
            get { return paramName; }
            private set
            {
                paramName = value;
                //OnPropertyChanged("ParamName");
            }
        }
        /// <summary>
        /// Строковое значение параметра информации о приборе
        /// </summary>
        private string paramValue;
        /// <summary>
        /// Строковое значение параметра информации о приборе
        /// </summary>
        public string ParamValue 
        {
            get { return paramValue; }
            private set
            {
                paramValue = value;
                //OnPropertyChanged("ParamValue");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private bool isValid;

        public bool IsValid
        {
            get { return isValid; }
            private set
            {
                isValid = value;
                OnPropertyChanged("IsValid");
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
    }
}
