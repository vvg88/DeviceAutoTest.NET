
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.WPFComponents.ScalableWindows;
using System.Windows;
using NeuroSoft.Hardware.Common;
using NeuroSoft.Hardware.Devices.Base;

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class StimulatorSettings<T> : UserControl, INotifyPropertyChanged, IDisposable where T : IStimulator
    {
        /// <summary>
        /// Конструктор для дизайнера
        /// </summary>
        /// <param name="device"></param>
        public StimulatorSettings()
        {
            DataContext = this;            
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="device"></param>
        public StimulatorSettings(INsDevice device)
        {
            Device = device;
            DataContext = this;
        }

        void device_DeviceMalfunction(object sender, int malfunctionsCode, bool isCritical, string message)
        {
            OnPropertyChanged("StimulationIsRun");
        }

        void DeviceMalfunction(AsyncActionArgs<DeviceErrorInfo> args)
        {
            device_DeviceMalfunction(Device, args.Data.Descriptor.ErrorCode, args.Data.Descriptor.IsCritical,
                args.Data.Descriptor.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        protected INsDevice Device;
        /// <summary>
        /// 
        /// </summary>
        protected T _stimulator;

        /// <summary>
        /// Экземпляр стимулятора
        /// </summary>
        public T Stimulator
        {
            get
            {
                if (_stimulator == null && Device != null)
                {
                    _stimulator = Device.FindModule<T>();
                    OnPropertyChanged("Stimulator");
                }
                return _stimulator;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool StimulationIsRun
        {
            get
            {
                if (Stimulator != null)
                    return Stimulator.StimulationIsRun;
                return false;
            }
        }

        private uint numOfStimuls = 0;
        /// <summary>
        /// Количество стимулов
        /// </summary>
        public uint NumOfStimuls
        {
            get { return numOfStimuls; }
            set
            {
                numOfStimuls = value;
                OnPropertyChanged("NumOfStimuls");
            }
        }

        /// <summary>
        /// Признак того, что стимулы были зафиксированы в потоке данных
        /// </summary>
        public bool StimulsCaptured
        {
            get { return NumOfStimuls > 0; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void StartStimulation(bool CaptureSyncImp = false, int stimulusCount = 0)
        {
            if (Stimulator == null)
                return;
            if (!OnStartStimulation())
                return;
            if (!Stimulator.ModuleIsOn)
                Stimulator.ModuleOn(DeviceMalfunction);
            if (Stimulator.StimulationIsRun)
            {
                Stimulator.StimulationStop();
            }
            if (!Stimulator.ModuleIsOn)
            {
                NSMessageBox.Show(Properties.Resources.CantEnableStimulator, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (stimulusCount > 0)
                Stimulator.StimulationStart(stimulusCount); //StimulationBegin(stimulusCount);
            else
                Stimulator.StimulationStart(); //StimulationBegin();
            if (Device is NeuroMepBase && CaptureSyncImp)
            {
                if ((Device as NeuroMepBase).AmplifierAdcIsRun)
                {
                    (Device as NeuroMepBase).AmplifierStopAdc();
                }
                if (!(Device as NeuroMepBase).Amplifier.ModuleIsOn)
                    (Device as NeuroMepBase).AmplifierModuleOn(DeviceMalfunction);
                (Device as NeuroMepBase).AmplifierMonitoringStart(DataCallBack, DeviceMalfunction);
            }
            OnPropertyChanged("StimulationIsRun");
        }
        /// <summary>
        /// Действия перед стартом стимуляции
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnStartStimulation()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopStimulation()
        {
            if (Stimulator == null)
                return;
            if (Device is NeuroMepBase)
            {
                if ((Device as NeuroMepBase).AmplifierAdcIsRun)
                {
                    (Device as NeuroMepBase).AmplifierStopAdc();
                    (Device as NeuroMepBase).AmplifierPowerOff();
                }
            }
            Stimulator.StimulationStop();
            Stimulator.ModuleOff();
        }
        /// <summary>
        /// Функция обратного вызова, которая считает стимулы
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        void DataCallBack(AsyncActionArgs<AmplifierMonitoringDataOld> data)
        {
            NumOfStimuls += (uint)data.Data.Data.StimulsCount;
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
        public virtual void Dispose()
        {
            DataContext = null;
            StopStimulation();
            Device = null;
            _stimulator = default(T);            
        }
    }
}
