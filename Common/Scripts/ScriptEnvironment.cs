using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Hardware.Devices;
using System.Windows.Threading;
using System.Windows.Controls;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.WPFComponents.ScalableWindows;
using System.Windows;
using NeuroSoft.DeviceAutoTest.Common;

namespace NeuroSoft.DeviceAutoTest.ScriptExecution
{
    /// <summary>
    /// Данные об окружении скрипта.
    /// </summary>
    public class ScriptEnvironment : IDisposable
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="deviceSerialNumber"></param>
        public ScriptEnvironment(string deviceSerialNumber, bool debug = false)
        {
            DeviceSerialNumber = deviceSerialNumber;
            isDebug = debug;
            try
            {
                ExecutablePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);
            }
            catch { }
        }

        private Dictionary<string, object> data = new Dictionary<string, object>();
        /// <summary>
        /// Данные окружения.
        /// </summary>
        public Dictionary<string, object> Data
        {
            get { return data; }            
        }

        /// <summary>
        /// Доступ к объекту окружения по ключу
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                EnsureKey(key);
                return Data[key];
            }
            set
            {
                EnsureKey(key);
                Data[key] = value;
            }
        }

        private void EnsureKey(string key)
        {
            if (!Data.ContainsKey(key))
            {
                Data.Add(key, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public object CreateItem(string key, object defaultValue)
        {
            if (!Data.ContainsKey(key))
            {
                Data.Add(key, defaultValue);
            }
            return Data[key];
        }

        #region Stand
        /// <summary>
        /// Открыть стенд тестирования и запомнить его в окружении 
        /// </summary>        
        /// <returns></returns>
        public void OpenStand()
        {
            if (stand == null || !stand.DeviceOpened)
            {                
                string[] serialNumbers = UniversalTestStand.RefreshAndGetSerialNumbers();
                if (serialNumbers.Length > 0)
                {
                    bool result = UniversalTestStand.RefreshAndOpen(serialNumbers[0], out stand);
                }
            }
        }

        /// <summary>
        /// Закрыть стенд
        /// </summary>        
        /// <returns></returns>
        public void CloseStand()
        {
            if (stand != null && stand.DeviceOpened)
            {
                stand.Close();
            }
        }

        private UniversalTestStand stand = null;
        /// <summary>
        /// Объект стенда
        /// </summary>
        /// <returns></returns>
        public UniversalTestStand Stand
        {
            get
            {
                return stand;
            }
            private set
            {
                stand = value;
            }
        }

        #endregion

        #region Device
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceWrapper"></param>
        public void InitDevice(INsDeviceWrapper deviceWrapper)
        {
            this.DeviceWrapper = deviceWrapper;
        }

        private INsDeviceWrapper deviceWrapper = null;

        /// <summary>
        /// Обертка девайса
        /// </summary>
        public INsDeviceWrapper DeviceWrapper
        {
            get { return deviceWrapper; }
            private set { deviceWrapper = value; }
        }
        /// <summary>
        /// Объект устройства
        /// </summary>
        /// <returns></returns>
        public object Device
        {
            get
            {
                if (DeviceWrapper != null)
                {
                    return DeviceWrapper.Device;
                }
                return null;
            } 
        }

        /// <summary>
        /// Закрыть устройство
        /// </summary>        
        /// <returns></returns>
        public void CloseDevice()
        {
            if (DeviceWrapper != null)
            {
                DeviceWrapper.Close();
                DeviceWrapper = null;
            }
        }


        private string deviceSerialNumber;

        /// <summary>
        /// Серийный номер текущего устройства
        /// </summary>
        public string DeviceSerialNumber
        {
            get { return deviceSerialNumber; }
            private set { deviceSerialNumber = value; }
        }

        #endregion

        #region TempProperties
        private bool isDebug = false;
        /// <summary>
        /// Признак режима отладки
        /// </summary>
        public bool IsDebug
        {
            get { return isDebug; }
        }

        private bool testWasCorrected = false;
        /// <summary>
        /// Признак внесения исправлений в результате выполнения скрипта
        /// </summary>
        public bool TestWasCorrected
        {
            get { return testWasCorrected; }
            set { testWasCorrected = value; }
        }

        private bool isAutoTesting = false;
        /// <summary>
        /// Признак процесса автоматического тестирования
        /// </summary>
        public bool IsAutoTesting
        {
            get { return isAutoTesting; }
            set { isAutoTesting = value; }
        }

        private string overrideComments = null;
        /// <summary>
        /// Значение переопределенного комметария при выполнении скрипта
        /// </summary>
        public string OverrideComments
        {
            get { return overrideComments; }
            set { overrideComments = value; }
        }

        /// <summary>
        /// Сброс значений временных переменных
        /// </summary>
        public void ResetTempProps()
        {
            //TestWasCorrected = false;
            OverrideComments = null;
        }

        /// <summary>
        /// Возвращает ContentPresenter по заданному ключу
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ContentPresenter GetPresenter(string key)
        {
            if (string.IsNullOrEmpty(key) || !Data.ContainsKey(key))
                return null;
            return Data[key] as ContentPresenter;
        }

        #endregion

        #region Supply Current

        private SupplyCurrentIndicator supplyCurrent;
        /// <summary>
        /// 
        /// </summary>
        public SupplyCurrentIndicator SupplyCurrent
        {
            get
            {
                if (supplyCurrent == null)
                {
                    supplyCurrent = new SupplyCurrentIndicator(this);
                }
                return supplyCurrent;
            }
        }
       
        /// <summary>
        /// Установка диапазона допустимых значений тока потребления
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void SetSupplyCurrentMinMax(double min, double max)
        {
            SupplyCurrent.SupplyCurrentMin = min;
            SupplyCurrent.SupplyCurrentMax = max;
        }

        /// <summary>
        /// Начать чтение тока потребления
        /// </summary>
        public void StartReadSupplyCurrent()
        {
            OpenStand();
            SupplyCurrent.Start();
        }

        /// <summary>
        /// Остановить чтение тока потребления
        /// </summary>        
        public void StopReadSupplyCurrent()
        {
            SupplyCurrent.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="correntTestOnError">Пометить тест как требующий исправлений в случае ошибки</param>
        /// <returns></returns>
        public bool TestSupplyCurrent(bool correntTestOnError = false)
        {            
            if (!SupplyCurrent.IsValidCurrent)
            {
                NSMessageBox.Show(string.Format("Недопустимое значение ({0} мА) тока потребления. Внесите исправления и запустите тест заново.", string.Format("{0:0.##}", supplyCurrent.LastSupplyCurrent)), NeuroSoft.DeviceAutoTest.Common.Properties.Resources.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                if (correntTestOnError)
                    TestWasCorrected = true;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="supplyCurrentValue">Значение тока потребления</param>
        /// <param name="correntTestOnError">Пометить тест как требующий исправлений в случае ошибки</param>
        /// <returns></returns>
        public bool TestSupplyCurrent(out double supplyCurrentValue, bool correntTestOnError = true)
        {
            supplyCurrentValue = SupplyCurrent.LastSupplyCurrent;
            return TestSupplyCurrent(correntTestOnError);
        }

        #endregion

        #region Other 
        private string executablePath;
        /// <summary>
        /// Абсолютный путь к папке с исполняемой программой
        /// </summary>
        public string ExecutablePath
        {
            get { return executablePath; }
            set { executablePath = value; }
        }

        /// <summary>
        /// Событие, уведомляющее о том, что тест в режиме автоматического тестирования готов к выполнению
        /// </summary>
        public event RoutedEventHandler AutoTestComplete;
        /// <summary>
        /// Уведомление об выполнении теста в процессе автоматического тестирования
        /// </summary>
        public void DoAutoTest()
        {
            DoAutoTest(AutoTestAction.Execute);            
        }
        /// <summary>
        /// Уведомление об выполнении теста в процессе автоматического тестирования
        /// </summary>
        public void DoAutoTest(AutoTestAction action)
        {
            if (AutoTestComplete != null)
                AutoTestComplete(action, new RoutedEventArgs());
        }
        #endregion
        #region Dispose
        /// <summary>
        /// Удаление объекта по ключу
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (Data.ContainsKey(key))
            {
                return Data.Remove(key);
            }
            return false;
        }

        /// <summary>
        /// Удаление объекта по ключу
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void RemoveAndDispose(string key)
        {
            if (Data.ContainsKey(key))
            {
                IDisposable disposable = Data[key] as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                Data.Remove(key);
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            foreach (var key in Data.Keys)
            {
                object dataItem = Data[key];
                if (dataItem is IDisposable)
                {
                    (dataItem as IDisposable).Dispose();
                }
                else if (dataItem is DispatcherTimer)
                {
                    (dataItem as DispatcherTimer).Stop();
                }
            }
            Data.Clear();
            if (supplyCurrent != null)
            {
                SupplyCurrent.Dispose();
            }
            CloseDevices();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseDevices()
        {        
            if (Device != null)
            {
                CloseDevice();
            }
            if (Stand != null)
            {
                Stand.Dispose();
            }
        }
        #endregion
    }    

    /// <summary>
    /// Класс-оболочка для работы с девайсами
    /// </summary>
    public interface INsDeviceWrapper
    {
        /// <summary>
        /// Ссылка на объект девайса
        /// </summary>
        object Device
        {
            get;            
        }

        /// <summary>
        /// Метод закрытия девайса
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Расшириение интерфейса INsDevice
    /// </summary>
    public static class INsDeviceExtentions
    {
        /// <summary>
        /// Поиск модуля по типу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindModule<T>(this INsDevice device) where T : IModule
        {            
            object foundModule = device.Modules.FirstOrDefault(m => typeof(T).IsAssignableFrom(m.GetType()));
            return foundModule != null ? (T)foundModule : default(T);
        }
    }
}
