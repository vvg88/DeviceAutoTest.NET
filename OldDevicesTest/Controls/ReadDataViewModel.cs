using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
//using NeuroSoft.Hardware.Devices;
//using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.MathLib.Filters;
using System.Windows;
using System.Collections.ObjectModel;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.Devices;
using System.Windows.Threading;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    public class ReadDataViewModel : ReadDataViewModelBase
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="device"></param>
        /// <param name="deviceAmplifierSate"></param>
        public ReadDataViewModel(ScriptEnvironment environment, NSDevice device)
            : base(environment, device)
        {
            //Environment = environment;
            SetDeviceInfo(device as NeuroMEPMicro, (NeuroMEPMicroAmplifierState)(device as NeuroMEPMicro).GetDeviceState().GetState(typeof(NeuroMEPMicroAmplifierState)));
            // Частота отсчётов
            double samplingRate = deviceAmplifierState.SampleFrequencyVP;

            for (int i = 0; i < ChannelsCount; i++)
            {
                var filterInfo = new FilterInfo(i.ToString(), samplingRate);
                filterInfo.FilterChanged += new RoutedEventHandler(filterInfo_FilterChanged);
                FiltersInfo.Add(filterInfo);
                filterInfo.ApplyFilterSettings();
            }
            //TickIntervalsProcessing = new RangedValue<int>(0, 0, 0);
        }

        #region Properties

        //private ScriptEnvironment environment;
        ///// <summary>
        ///// Environment
        ///// </summary>
        //public ScriptEnvironment Environment
        //{
        //    get { return environment; }
        //    private set { environment = value; }
        //}

        //private NeuroMEPMicro mepDevice;
        /// <summary>
        /// Device
        /// </summary>
        public NeuroMEPMicro MepDevice
        {
            get { return device as NeuroMEPMicro; }
            private set { device = value; }
        }

        private NeuroMEPMicroAmplifierState deviceAmplifierState;

        /// <summary>
        /// AmplifierCapabilities
        /// </summary>
        public NeuroMEPMicroAmplifierState DeviceAmplifierState
        {
            get { return deviceAmplifierState; }
            private set
            {
                deviceAmplifierState = value;
                if (deviceAmplifierState != null)
                {
                    ResetDataStatistics();
                }
            }
        }

        /// <summary>
        /// Частота дискретизации
        /// </summary>
        public override float SamplesRateInSamplesPerSecond
        {
            get
            {
                if (MepDevice == null)
                {
                    return 0;
                }
                return (MepDevice.GetDeviceState().GetState(typeof(NeuroMEPMicroAmplifierState)) as NeuroMEPMicroAmplifierState).SampleFrequencyVP;
            }
            set
            {
                MepDevice.SetSampleFrequencyVP(value);
            }
        }

        //private bool enablePowerRejector = false;

        /// <summary>
        /// Включить фильтр сетевой помехи
        /// </summary>
        public override bool EnablePowerRejector
        {
            get { return base.EnablePowerRejector; }
            set
            {
                //if (enablePowerRejector != value)
                //{
                //    enablePowerRejector = value;
                    MepDevice.SetFiltersRe(value, false);
                    for (int i = 0; i < MepDevice.Filters.Length; i++)
                    {
                        for (int j = 0; j < MepDevice.Filters[i].Length; j++)
                        {
                            if ((MepDevice.Filters[i][j] is NeuroSoft.MathLib.Filters.IIRFilterRejector))
                                MepDevice.Filters[i][j].Active = value;
                            else
                            {
                                MepDevice.Filters[i][j].Active = FiltersInfo[i].IsEnabled;
                            }
                        }
                    }
                    base.EnablePowerRejector = value;
                //}
            }
        }

        //private ObservableCollection<ChannelInfo> channelsInfo = new ObservableCollection<ChannelInfo>();
        ///// <summary>
        ///// Настройки каналов
        ///// </summary>
        //public ObservableCollection<ChannelInfo> ChannelsInfo
        //{
        //    get { return channelsInfo; }
        //}

        ///// <summary>
        ///// Число тактов таймера, необходимое для корректной обработки сигнала
        ///// </summary>
        //public RangedValue<int> TickIntervalsProcessing { get; set; }

        /// <summary>
        /// Плоттер
        /// </summary>
        //public MonitoringPlotter DataMonitoringPlotter { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Обработчик события изменения фильтров
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void filterInfo_FilterChanged(object sender, RoutedEventArgs e)
        {
            FilterInfo filterInfo = sender as FilterInfo;
            int index = FiltersInfo.IndexOf(filterInfo);
            MepDevice.SetFilterHi(index, (float)filterInfo.MaxFreq);
            MepDevice.SetFilterLo(index, (float)filterInfo.MinFreq);
            for (int i = 0; i < MepDevice.Filters[index].Length; i++)
            {
                if (!(MepDevice.Filters[index][i] is NeuroSoft.MathLib.Filters.IIRFilterRejector))
                    MepDevice.Filters[index][i].Active = filterInfo.IsEnabled;
            }
        }

        /// <summary>
        /// Установка начальных значений
        /// </summary>
        /// <param name="device"></param>
        /// <param name="deviceAmplifierState"></param>
        protected void SetDeviceInfo(NeuroMEPMicro device, NeuroMEPMicroAmplifierState deviceAmplifierState)
        {
            MepDevice = device;
            DeviceAmplifierState = deviceAmplifierState;
            base.SetDeviceInfo();
            for (int i = 0; i < ChannelsCount; i++)
            {
                string name = i.ToString();
                StatisticsCollection.Add(new DataStatistics(name));
                ChannelsInfo.Add(new ChannelInfo(i, MepDevice, deviceAmplifierState));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void SetSavedSettings(ReadDataSettings settings)
        {
            //if (savedSettings == null)
            //    return;
            //var settings = savedSettings;
            base.SetSavedSettings(settings);
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (settings.RangeIndex != null)
                {
                    ChannelsInfo[i].SelectedChannelRange = ChannelsInfo[i].ChannelSupportedRanges[settings.RangeIndex.Value];
                }
            }

            if (settings.MinFreqFilter.HasValue || settings.MaxFreqFilter.HasValue)
            {
                for (int i = 0; i < ChannelsCount; i++)
                {
                    if (settings.MinFreqFilter.HasValue)
                    {
                        if (ChannelsInfo[i].LowFreqPassBands.Contains((float)settings.MinFreqFilter.Value))
                        {
                            ChannelsInfo[i].SelectedLowFreq = (float)settings.MinFreqFilter.Value;
                            FiltersInfo[i].IsEnabled = false;
                            FiltersInfo[i].ApplyFilterSettings();
                        }
                        else
                        {
                            if (settings.MinFreqFilter.HasValue && settings.MaxFreqFilter.HasValue)
                            {
                                FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, settings.MaxFreqFilter);
                                FiltersInfo[i].IsEnabled = true;
                                continue;
                            }
                            else
                            {
                                FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, 20000f);
                                FiltersInfo[i].IsEnabled = true;
                            }
                        }
                    }
                    if (settings.MaxFreqFilter.HasValue)
                    {
                        if (ChannelsInfo[i].HighFreqPassBands.Contains((float)settings.MaxFreqFilter.Value))
                        {
                            ChannelsInfo[i].SelectedHighFreq = (float)settings.MaxFreqFilter.Value;
                            FiltersInfo[i].IsEnabled = false;
                            FiltersInfo[i].ApplyFilterSettings();
                        }
                        else
                        {
                            if (!settings.MinFreqFilter.HasValue)
                                FiltersInfo[i].SetMinMaxFreq(0.0, settings.MaxFreqFilter);
                            else
                                FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, settings.MaxFreqFilter);
                            FiltersInfo[i].IsEnabled = true;
                        }
                    }
                }
                //if (settings.MinFreqFilter.HasValue || settings.MaxFreqFilter.HasValue)
                //{
                //    SetIsEnabledFilters(true);
                //}
            }
        }

        #region ReadData

        ///// <summary>
        ///// Обновление статистики
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //public override void UpdateStatistic()
        //{
        //    TickIntervalsProcessing.Value++;
        //    base.UpdateStatistic();
        //    //for (int i = 0; i < ChannelsCount; i++)
        //    //{
        //    //    double averange = samplesSum[i] / samples;
        //    //    if (samples > 0)
        //    //    {
        //    //        StatisticsCollection[i].UpdateStatistics(averange, minSamples[i], maxSamples[i], Math.Sqrt(samplesSumSqr[i] / samples - averange * averange));
        //    //    }
        //    //    else
        //    //    {
        //    //        StatisticsCollection[i].UpdateStatistics(double.NaN, double.NaN, double.NaN, double.NaN);
        //    //    }
        //    //}
        //    if (!TickIntervalsProcessing.IsValidValue)
        //    {
        //        samples = 0;
        //        TickIntervalsProcessing.Value = 0;
        //        Array.Clear(samplesSum, 0, ChannelsCount);
        //        Array.Clear(samplesSumSqr, 0, ChannelsCount);
        //        ResetMinMax();
        //    }
        //}

        #endregion

        #endregion
        ///// <summary>
        ///// 
        ///// </summary>
        //public event RecieveDataHandler RecieveData;
    }

    

    /// <summary>
    /// Настройки канала
    /// </summary>
    public class ChannelInfo : ChannelInfoBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingFreq"></param>
        public ChannelInfo(int channelNum, NeuroMEPMicro device, NeuroMEPMicroAmplifierState deviceAmplifierState)
            : base(channelNum, device)
        {
            //this.device = device;
            //this.deviceAmplifierState = deviceAmplifierState;
        }

        //private NeuroMEPMicro device;
        //private NeuroMEPMicroAmplifierState deviceAmplifierState;
        

        private List<float> highFreqPassBands;
        /// <summary>
        /// 
        /// </summary>
        public override List<float> HighFreqPassBands
        {
            get
            {
                if (highFreqPassBands == null)
                {
                    float[] highFreqsPass = { 10000.0f, 250.0f };
                    highFreqPassBands = highFreqsPass.ToList();
                }
                return highFreqPassBands;
            }
        }

        /// <summary>
        /// Индекс верхней частоты полосы пропускания сигнала
        /// </summary>
        public override float SelectedHighFreq
        {
            get
            {
                if (device == null || HighFreqPassBands.Count == 0)
                {
                    return -1;
                }
                return HighFreqPassBands[(int)(device.GetDeviceState().GetState(typeof(NeuroMEPMicroAmplifierState)) as NeuroMEPMicroAmplifierState).FiltersHiVP[channelNumber]]; 
            }
            set
            {
                if (device == null || HighFreqPassBands.Count == 0)
                    return;
                (device as NeuroMEPMicro).SetFilterHiVP(channelNumber, value);
                (device as NeuroMEPMicro).SetChannelsUsing();
                OnPropertyChanged("SelectedHiFreq");
            }
        }

        private List<float> lowFreqPassBands = null;
        /// <summary>
        /// 
        /// </summary>
        public override List<float> LowFreqPassBands
        {
            get
            {
                if (lowFreqPassBands == null)
                {
                    float[] lowFreqsPass = { 0.05f, 0.5f, 160f};
                    lowFreqPassBands = lowFreqsPass.ToList();
                }
                return lowFreqPassBands;
            }
        }

        /// <summary>
        /// Ниняя частота полосы пропускания сигнала
        /// </summary>
        public override float SelectedLowFreq
        {
            get
            {
                if (device == null || LowFreqPassBands.Count == 0)
                {
                    return -1;
                }
                return LowFreqPassBands[(int)(device.GetDeviceState().GetState(typeof(NeuroMEPMicroAmplifierState)) as NeuroMEPMicroAmplifierState).FiltersLoVP[channelNumber]];
            }
            set
            {
                if (device == null || LowFreqPassBands.Count == 0)
                    return;
                (device as NeuroMEPMicro).SetFilterLoVP(channelNumber, value);
                (device as NeuroMEPMicro).SetChannelsUsing();
                OnPropertyChanged("SelectedLowFreq");
            }
        }

        private List<float> channelSupportedRanges;

        /// <summary>
        /// 
        /// </summary>
        public override List<float> ChannelSupportedRanges
        {
            get
            {
                if (channelSupportedRanges == null)
                {
                    float[] suppurtedRanges = { 0.0002f, 0.0005f, 0.001f, 0.002f, 0.005f, 0.01f, 0.02f, 0.05f, 0.1f };
                    channelSupportedRanges = suppurtedRanges.ToList();
                }
                return channelSupportedRanges;
            }
        }

        /// <summary>
        /// Индекс диапазона входного сигнала
        /// </summary>
        public override float SelectedChannelRange
        {
            get
            {
                if (device == null || ChannelSupportedRanges.Count == 0)
                {
                    return -1;
                }
                return device.GetChannelState(channelNumber).InputRange;
            }
            set
            {
                if (device == null || ChannelSupportedRanges.Count == 0)
                    return;
                (device as NeuroMEPMicro).SetGainFactorVP(channelNumber, (PolyChannelsGainFactor)channelSupportedRanges.IndexOf(value));
                (device as NeuroMEPMicro).SetChannelsUsing();
                OnPropertyChanged("SelectedChannelRange");
            }
        }

        /// <summary>
        /// Признак зануления канала
        /// </summary>
        //public override bool ChannelIsNull
        //{
        //    get
        //    {
        //        if (device == null)
        //        {
        //            return false;
        //        }
        //        return device.GetChannelState(channelNumber).Enabled;
        //    }
        //    set
        //    {
        //        device.SetChannelEnable(channelNumber, value);
        //        (device as NeuroMEPMicro).SetChannelsUsing();
        //        OnPropertyChanged("ChannelIsNull");
        //    }
        //}
    }
}
