using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.MathLib.Filters;
using System.Windows;
using System.Collections.ObjectModel;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.Hardware.Common;
using NeuroSoft.WPFComponents.ScalableWindows;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    public class ReadDataViewModel : DATBaseViewModel
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="device"></param>
        /// <param name="amplifierCapabilities"></param>
        public ReadDataViewModel(ScriptEnvironment environment, NeuroMepBase device, AmplifierCapabilities amplifierCapabilities)
        {
            Environment = environment;
            Device = device;
            AmplifierCapabilities = amplifierCapabilities;

            AdcModeDescription adcModeDescription = AmplifierCapabilities.SupportedAdcMode[device.AmplifierAdcModeIndex];            

            // Частота отсчётов
            double samplingRate = adcModeDescription.Frequence;

            //Фильтры
            ReFilters = new Filter[ChannelsCount];
            Filters = new Filter[ChannelsCount];

            for (int i = 0; i < ChannelsCount; i++)
            {
                ReFilters[i] = new IIRFilterRejector(50, 40, (float)samplingRate);
                ReFilters[i].Active = true;

                var filterInfo = new FilterInfo(i.ToString(), samplingRate);
                filterInfo.FilterChanged += new RoutedEventHandler(filterInfo_FilterChanged);
                FiltersInfo.Add(filterInfo);
                Filters[i] = filterInfo.CreateFilter();
            }
        }

        #region Properties

        internal int ChannelsCount
        {
            get
            {
                return AmplifierCapabilities.ChannelsCount;
            }
        }

        private ScriptEnvironment environment;
        /// <summary>
        /// Environment
        /// </summary>
        public ScriptEnvironment Environment
        {
            get { return environment; }
            private set { environment = value; }
        }

        private NeuroMepBase device;
        /// <summary>
        /// Device
        /// </summary>
        public NeuroMepBase Device
        {
            get { return device; }
            private set { device = value; }
        }

        private AmplifierCapabilities amplifierCapabilities;

        /// <summary>
        /// AmplifierCapabilities
        /// </summary>
        public AmplifierCapabilities AmplifierCapabilities
        {
            get { return amplifierCapabilities; }
            private set
            {
                amplifierCapabilities = value;
                if (amplifierCapabilities != null)
                {
                    maxSamples = new float[amplifierCapabilities.ChannelsCount];
                    minSamples = new float[amplifierCapabilities.ChannelsCount];
                    ResetDataStatistics();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float SamplesRateInSamplesPerSecond
        {
            get
            {
                if (AmplifierCapabilities == null || Device == null)
                {
                    return 0;
                }
                return AmplifierCapabilities.SupportedAdcMode[Device.AmplifierAdcModeIndex].Frequence;
            }
        }

        private bool enablePowerRejector = false;

        /// <summary>
        /// Включить фильтр сетевой помехи
        /// </summary>
        public bool EnablePowerRejector
        {
            get { return enablePowerRejector; }
            set
            {
                if (enablePowerRejector != value)
                {
                    enablePowerRejector = value;
                    OnPropertyChanged("EnablePowerRejector");
                }
            }
        }

        /// <summary>
        /// Фильтры сетевой помехи.
        /// </summary>
        private Filter[] ReFilters;
        /// <summary>
        /// Фильтры частот
        /// </summary>
        private Filter[] Filters;

        private ObservableCollection<FilterInfo> filtersInfo = new ObservableCollection<FilterInfo>();

        /// <summary>
        /// Информация о фильтрах
        /// </summary>
        public ObservableCollection<FilterInfo> FiltersInfo
        {
            get { return filtersInfo; }
        }

        #endregion

        #region Methods

        void filterInfo_FilterChanged(object sender, RoutedEventArgs e)
        {
            FilterInfo filterInfo = sender as FilterInfo;
            int index = FiltersInfo.IndexOf(filterInfo);
            lock (this)
            {
                Filters[index] = filterInfo.CreateFilter();
            }
        }

        #region ReadData
        private bool isReading = false;
        /// <summary>
        /// Начать чтение данных
        /// </summary>
        public virtual void StartReadData()
        {
            if (isReading)
                return;
            isReading = true;
            Device.AmplifierMonitoringStart(ReadSamplesStimulsData, ReadSamplesErrHandler);
        }

        /// <summary>
        /// Остановить чтение данных
        /// </summary>
        public virtual void StopReadData()
        {            
            ResetDataStatistics();
            if (Device.AmplifierAdcIsRun)
                Device.AmplifierStopAdc();
            isReading = false;
        }

        protected int samples = 0;
        protected float[] maxSamples;
        protected float[] minSamples;
        
        private void ReadSamplesStimulsData(Hardware.Devices.SamplesStimulsDataEventArgs e, object sender)
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (EnablePowerRejector)
                {
                    ReFilters[i].DoFilter(e.ChannelsSamples[i], e.ChannelsSamples[i], 0, 0, e.SamplesCount);
                }
                if (FiltersInfo[i].IsEnabled)
                {
                    Filters[i].DoFilter(e.ChannelsSamples[i], e.ChannelsSamples[i], 0, 0, e.SamplesCount);
                }
            }            
            OnReadSamplesStimulsData(e, sender);
            OnRecieveData(e);
        }

        private void ReadSamplesStimulsData(AsyncActionArgs<AmplifierMonitoringDataOld> args)
        {
            var e = args.Data.Data;
            ReadSamplesStimulsData(e, this);
        }

        void ReadSamplesErrHandler(AsyncActionArgs<DeviceErrorInfo> args)
        {
            NSMessageBox.Show(this, "При чтении данных возникла ошибка:" + args.Data.Descriptor.Message,
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        protected virtual void OnReadSamplesStimulsData(Hardware.Devices.SamplesStimulsDataEventArgs e, object sender)
        {
            AppendDataStatistics(e);  
        }

        private void OnRecieveData(Hardware.Devices.SamplesStimulsDataEventArgs e)
        {
            if (RecieveData != null)
            {
                RecieveData(this, e);
            }
        }

        /// <summary>
        /// Пополнение статистики новыми данными
        /// </summary>
        /// <param name="e"></param>
        protected void AppendDataStatistics(Hardware.Devices.SamplesStimulsDataEventArgs e)
        {
            int maxIndex = 0;
            int minIndex = 0;
            for (int i = 0; i < AmplifierCapabilities.ChannelsCount; i++)
            {
                List<float> iSamples = new List<float>(e.ChannelsSamples[i]);
                NeuroSoft.MathLib.Basic.CalcMinMax(iSamples, out minIndex, out maxIndex);
                float max = e.ChannelsSamples[i][maxIndex];
                float min = e.ChannelsSamples[i][minIndex];
                if (max > maxSamples[i])
                {
                    maxSamples[i] = max;
                }
                if (min < minSamples[i])
                {
                    minSamples[i] = min;
                }
            }
            samples += e.SamplesCount;
        }

        /// <summary>
        /// Сброс статистики
        /// </summary>
        protected void ResetDataStatistics()
        {
            for (int i = 0; i < AmplifierCapabilities.ChannelsCount; i++)
            {
                maxSamples[i] = float.MinValue;
                minSamples[i] = float.MaxValue;
            }
            samples = 0;
        }
        #endregion

        #endregion
        /// <summary>
        /// 
        /// </summary>
        public event RecieveDataHandler RecieveData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RecieveDataHandler(object sender, Hardware.Devices.SamplesStimulsDataEventArgs e);
}
