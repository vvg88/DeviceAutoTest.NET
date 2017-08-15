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
    public class ReadDataViewModel : DATBaseViewModel
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="device"></param>
        /// <param name="deviceAmplifierSate"></param>
        public ReadDataViewModel(ScriptEnvironment environment, NeuroMEPMicro device, NeuroMEPMicroAmplifierState deviceAmplifierState)
        {
            Environment = environment;
            SetDeviceInfo(device, deviceAmplifierState);
            // Частота отсчётов
            double samplingRate = deviceAmplifierState.SampleFrequencyVP;
            //Фильтры
            //ReFilters = new Filter[ChannelsCount];
            //Filters = new Filter[ChannelsCount];

            for (int i = 0; i < ChannelsCount; i++)
            {
                //ReFilters[i] = new IIRFilterRejector(50, 40, (float)SamplesRateInSamplesPerSecond);
                var filterInfo = new FilterInfo(i.ToString(), samplingRate);
                filterInfo.FilterChanged += new RoutedEventHandler(filterInfo_FilterChanged);
                FiltersInfo.Add(filterInfo);
                filterInfo.ApplyFilterSettings();
                //Filters[i] = FiltersInfo[i].CreateFilter();
            }
            Device.ReceiveData += new ReceiveDataDelegate(DeviceReceiveDataHandler);
            TickIntervalsProcessing = new RangedValue<int>(0, 0, 0);
        }

        #region Properties

        internal int ChannelsCount
        {
            get
            {
                return device.GetChannelsCount();
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

        private NeuroMEPMicro device;
        /// <summary>
        /// Device
        /// </summary>
        public NeuroMEPMicro Device
        {
            get { return device; }
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
        public float SamplesRateInSamplesPerSecond
        {
            get
            {
                if (Device == null)
                {
                    return 0;
                }
                return (Device.GetDeviceState().GetState(typeof(NeuroMEPMicroAmplifierState)) as NeuroMEPMicroAmplifierState).SampleFrequencyVP;
            }
            set
            {
                Device.SetSampleFrequencyVP(value);
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
                    Device.SetFiltersRe(value, false);
                    for (int i = 0; i < Device.Filters.Length; i++)
                    {
                        for (int j = 0; j < Device.Filters[i].Length; j++)
                        {
                            if ((Device.Filters[i][j] is NeuroSoft.MathLib.Filters.IIRFilterRejector))
                                Device.Filters[i][j].Active = enablePowerRejector;
                            else
                            {
                                Device.Filters[i][j].Active = FiltersInfo[i].IsEnabled;
                            }
                        }
                    }
                    OnPropertyChanged("EnablePowerRejector");
                }
            }
        }

        ///// <summary>
        ///// Фильтры сетевой помехи.
        ///// </summary>
        //private Filter[] ReFilters;
        ///// <summary>
        ///// Фильтры частот
        ///// </summary>
        //private Filter[] Filters;

        private ObservableCollection<FilterInfo> filtersInfo = new ObservableCollection<FilterInfo>();

        /// <summary>
        /// Информация о фильтрах
        /// </summary>
        public ObservableCollection<FilterInfo> FiltersInfo
        {
            get { return filtersInfo; }
        }

        private ObservableCollection<DataStatistics> statisticsCollection = new ObservableCollection<DataStatistics>();

        /// <summary>
        /// Коллекция со статистикой 
        /// </summary>
        public ObservableCollection<DataStatistics> StatisticsCollection
        {
            get { return statisticsCollection; }
        }

        private ObservableCollection<ChannelInfo> channelsInfo = new ObservableCollection<ChannelInfo>();

        /// <summary>
        /// Настройки каналов
        /// </summary>
        public ObservableCollection<ChannelInfo> ChannelsInfo
        {
            get { return channelsInfo; }
        }

        /// <summary>
        /// Числп тактов таймера, необходимое для корректной обработки сигнала
        /// </summary>
        public RangedValue<int> TickIntervalsProcessing { get; set; }

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
            Device.SetFilterHi(index, (float)filterInfo.MaxFreq);
            Device.SetFilterLo(index, (float)filterInfo.MinFreq);
            for (int i = 0; i < Device.Filters[index].Length; i++)
            {
                if (!(Device.Filters[index][i] is NeuroSoft.MathLib.Filters.IIRFilterRejector))
                    Device.Filters[index][i].Active = filterInfo.IsEnabled;
            }
            //lock (this)
            //{
            //    Filters[index] = filterInfo.CreateFilter();
            //}
        }

        /// <summary>
        /// Установка начальных значений
        /// </summary>
        /// <param name="device"></param>
        /// <param name="deviceAmplifierState"></param>
        private void SetDeviceInfo(NeuroMEPMicro device, NeuroMEPMicroAmplifierState deviceAmplifierState)
        {
            Device = device;
            DeviceAmplifierState = deviceAmplifierState;

            samplesSum = new double[ChannelsCount];
            samplesSumSqr = new double[ChannelsCount];
            maxSamples = new float[ChannelsCount];
            minSamples = new float[ChannelsCount];

            for (int i = 0; i < ChannelsCount; i++)
            {
                string name = i.ToString();
                StatisticsCollection.Add(new DataStatistics(name));
                ChannelsInfo.Add(new ChannelInfo(i, Device, deviceAmplifierState));
            }
        }

        #region ReadData

        protected bool isReading = false;
        /// <summary>
        /// Начать чтение данных
        /// </summary>
        public virtual void StartReadData()
        {
            if (isReading)
                return;
            isReading = true;

            Device.BeginTransmit();
            //Device.BeginTransmitVP(); // При использовании этого метода не останавливался прием данных, так как не устанавливался флаг Transmitting
        }

        /// <summary>
        /// Остановить чтение данных
        /// </summary>
        public virtual void StopReadData()
        {            
            ResetDataStatistics();
            Device.StopTransmit();
            //Device.AmplifierStopAdc();
            isReading = false;
        }

        protected int samples = 0;
        protected double[] samplesSum;
        protected double[] samplesSumSqr;
        protected float[] maxSamples;
        protected float[] minSamples;

        //private void ReadSamplesStimulsData(Hardware.Devices.SamplesStimulsDataEventArgs e, object sender)
        //{
        //    for (int i = 0; i < ChannelsCount; i++)
        //    {
        //        if (EnablePowerRejector)
        //        {
        //            ReFilters[i].DoFilter(e.ChannelsSamples[i], e.ChannelsSamples[i], 0, 0, e.SamplesCount);
        //        }
        //        if (FiltersInfo[i].IsEnabled)
        //        {
        //            Filters[i].DoFilter(e.ChannelsSamples[i], e.ChannelsSamples[i], 0, 0, e.SamplesCount);
        //        }
        //    }            
        //    OnReadSamplesStimulsData(e, sender);
        //    OnRecieveData(e);
        //}

        protected virtual void DeviceReceiveDataHandler(object sender, ReceiveDataArgs e)
        {
            //OnReadSamplesStimulsData(e, sender);
            for (int i = 0; i < ChannelsCount; i++)
            {
                //if (EnablePowerRejector)
                //{
                //    ReFilters[i].DoFilter(e.Data[i], e.Data[i], 0, 0, e.Data[i].Length);
                //}
                //if (FiltersInfo[i].IsEnabled)
                //{
                //    Filters[i].DoFilter(e.Data[i], e.Data[i], 0, 0, e.Data[i].Length);
                //}
            }
            OnReadSamplesStimulsData(e, this);
            OnRecieveData(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        protected virtual void OnReadSamplesStimulsData(ReceiveDataArgs e, object sender)
        {
            AppendDataStatistics(e);
        }

        private void OnRecieveData(ReceiveDataArgs e)
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
        protected void AppendDataStatistics(ReceiveDataArgs e)
        {
            int maxIndex = 0;
            int minIndex = 0;
            for (int i = 0; i < DeviceAmplifierState.GetChannelsCount(); i++)
            {
                samplesSum[i] += NeuroSoft.MathLib.Basic.SumArray_IPP(e.Data[i]);
                float[] sqrSamples = new float[e.Data[i].Length];
                NeuroSoft.MathLib.Basic.SqrOnArray_IPP(e.Data[i], sqrSamples);
                samplesSumSqr[i] += NeuroSoft.MathLib.Basic.SumArray_IPP(sqrSamples);
                
                List<float> iSamples = new List<float>(e.Data[i]);
                NeuroSoft.MathLib.Basic.CalcMinMax(iSamples, out minIndex, out maxIndex);
                float max = e.Data[i][maxIndex];
                float min = e.Data[i][minIndex];
                if (max > maxSamples[i])
                {
                    maxSamples[i] = max;
                }
                if (min < minSamples[i])
                {
                    minSamples[i] = min;
                }
            }
            samples += e.Data[0].Length / 2 + e.Data[1].Length / 2;
        }

        /// <summary>
        /// Обновление статистики
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateStatistic()
        {
            TickIntervalsProcessing.Value++;
            for (int i = 0; i < ChannelsCount; i++)
            {
                double averange = samplesSum[i] / samples;
                if (samples > 0)
                {
                    StatisticsCollection[i].UpdateStatistics(averange, minSamples[i], maxSamples[i], Math.Sqrt(samplesSumSqr[i] / samples - averange * averange));
                }
                else
                {
                    StatisticsCollection[i].UpdateStatistics(double.NaN, double.NaN, double.NaN, double.NaN);
                }
            }
            if (!TickIntervalsProcessing.IsValidValue)
            {
                samples = 0;
                TickIntervalsProcessing.Value = 0;
                Array.Clear(samplesSum, 0, ChannelsCount);
                Array.Clear(samplesSumSqr, 0, ChannelsCount);
                ResetMinMax();
            }
        }

        /// <summary>
        /// Сброс статистики
        /// </summary>
        protected void ResetDataStatistics()
        {
            for (int i = 0; i < DeviceAmplifierState.GetChannelsCount(); i++)
            {
                if (maxSamples != null && minSamples != null)
                {
                    maxSamples[i] = float.MinValue;
                    minSamples[i] = float.MaxValue;
                }
            }
            samples = 0;
        }
        /// <summary>
        /// Сброс максимальных и минимальных значений
        /// </summary>
        private void ResetMinMax()
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                maxSamples[i] = float.MinValue;
                minSamples[i] = float.MaxValue;
            }
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
    public delegate void RecieveDataHandler(object sender, ReceiveDataArgs e);

    /// <summary>
    /// Настройки канала
    /// </summary>
    public class ChannelInfo : DATBaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingFreq"></param>
        public ChannelInfo(int channelNum, NeuroMEPMicro device, NeuroMEPMicroAmplifierState deviceAmplifierState)
        {
            ChannelNumber = channelNum;
            this.device = device;
            //this.deviceAmplifierState = deviceAmplifierState;
        }

        private NeuroMEPMicro device;
        //private NeuroMEPMicroAmplifierState deviceAmplifierState;
        private int channelNumber;

        /// <summary>
        /// Имя канала
        /// </summary>
        public int ChannelNumber
        {
            get { return channelNumber; }
            private set { channelNumber = value; }
        }

        private List<float> highFreqPassBands;
        /// <summary>
        /// 
        /// </summary>
        public List<float> HighFreqPassBands
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
        public float SelectedHighFreq
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
                // Фильтры НЧ в девайсе НейроМВП-Микро не устанавливаются
                if (device == null || HighFreqPassBands.Count == 0)
                    return;
                device.SetFilterHiVP(channelNumber, value);
                device.SetChannelsUsing();
                OnPropertyChanged("SelectedHiFreq");
            }
        }

        private List<float> lowFreqPassBands = null;
        /// <summary>
        /// 
        /// </summary>
        public List<float> LowFreqPassBands
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
        public float SelectedLowFreq
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
                device.SetFilterLoVP(channelNumber, value);
                device.SetChannelsUsing();
                OnPropertyChanged("SelectedLowFreq");
            }
        }

        private List<float> channelSupportedRanges;

        /// <summary>
        /// 
        /// </summary>
        public List<float> ChannelSupportedRanges
        {
            get
            {
                if (channelSupportedRanges == null)
                {
                    //channelSupportedRanges = Capabilities.GetChannelSupportedRange(ChannelNumber).ToList();
                    float[] suppurtedRanges = { 0.0002f, 0.0005f, 0.001f, 0.002f, 0.005f, 0.01f, 0.02f, 0.05f, 0.1f };
                    channelSupportedRanges = suppurtedRanges.ToList();
                }
                return channelSupportedRanges;
            }
        }

        /// <summary>
        /// Индекс диапазона входного сигнала
        /// </summary>
        public float SelectedChannelRange
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
                device.SetGainFactorVP(channelNumber, (PolyChannelsGainFactor)channelSupportedRanges.IndexOf(value));
                device.SetChannelsUsing();
                OnPropertyChanged("SelectedChannelRange");
            }
        }

        /// <summary>
        /// Признак зануления канала
        /// </summary>
        public bool ChannelIsNull
        {
            get
            {
                if (device == null)
                {
                    return false;
                }
                return device.GetChannelState(channelNumber).Enabled;
            }
            set
            {
                device.SetChannelEnable(channelNumber, value);
                device.SetChannelsUsing();
                OnPropertyChanged("ChannelIsNull");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FilterInfo : DATBaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingFreq"></param>
        public FilterInfo(string channelName, double samplingFreq)
        {
            SamplingFreq = samplingFreq;
            ChannelName = channelName;
        }

        private string channelName;
        /// <summary>
        /// Имя канала
        /// </summary>
        public string ChannelName
        {
            get { return channelName; }
            set
            {
                if (channelName != value)
                {
                    channelName = value;
                    OnPropertyChanged("ChannelName");
                }
            }
        }


        private bool isEnabled = false;
        /// <summary>
        /// Включен ли фильтр
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged("IsEnabled");
                    OnFilterChanged();
                }
            }
        }
        private double minFreq = 0;
        /// <summary>
        /// Нижняя частота среза
        /// </summary>
        public double MinFreq
        {
            get { return minFreq; }
            set
            {
                if (minFreq != value)
                {
                    if (value >= MaxFreq)
                        return;
                    minFreq = value;
                    OnPropertyChanged("MinFreq");
                    OnFilterChanged();
                }
            }
        }

        private double maxFreq = 100;
        /// <summary>
        /// Верхняя частота среза
        /// </summary>
        public double MaxFreq
        {
            get { return maxFreq; }
            set
            {
                if (maxFreq != value)
                {
                    if (value <= MinFreq || value >= MaxFreqValue)
                        return;
                    maxFreq = value;
                    OnPropertyChanged("MaxFreq");
                    OnFilterChanged();
                }
            }
        }

        /// <summary>
        /// Ограничение сверху значения верхней частоты среза
        /// </summary>
        public double MaxFreqValue
        {
            get
            {
                return 0.5 * SamplingFreq - 1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal void SetMinMaxFreq(double? minFreq, double? maxFreq)
        {
            if (minFreq.HasValue)
            {
                this.minFreq = minFreq.Value;
            }
            if (maxFreq.HasValue)
            {
                this.maxFreq = maxFreq.Value;
            }
            OnPropertyChanged("MinFreq");
            OnPropertyChanged("MaxFreq");
            OnFilterChanged();
        }

        private double samplingFreq;

        /// <summary>
        /// Частота квантования
        /// </summary>
        public double SamplingFreq
        {
            get { return samplingFreq; }
            private set
            {
                if (samplingFreq != value)
                {
                    samplingFreq = value;
                }
            }
        }

        /// <summary>
        /// Событие изменения фильтра
        /// </summary>
        public event RoutedEventHandler FilterChanged;

        private void OnFilterChanged()
        {
            if (FilterChanged != null)
            {
                FilterChanged(this, new RoutedEventArgs());
            }
        }
        /// <summary>
        /// Применяет настройки фильтров. Передает их в класс NeuroMEPMicro
        /// </summary>
        public void ApplyFilterSettings()
        {
            if (FilterChanged != null)
            {
                OnFilterChanged();
            }
        }

        /// <summary>
        /// Метод создания фильтра на основе данных этого класса
        /// </summary>
        /// <returns></returns>
        //public Filter CreateFilter()
        //{
        //    double[][] coeff;

        //    if (MinFreq == 0)
        //        coeff = ButterworthIIRF_Designing.GetCoefficients_1_Standard(
        //            FilterType.LF,
        //            4,
        //            1,
        //            SamplingFreq,
        //            MaxFreq);
        //    else
        //        coeff = ButterworthIIRF_Designing.GetCoefficients_1_Standard(
        //            FilterType.Band,
        //            4,
        //            1,
        //            SamplingFreq,
        //            MinFreq,
        //            MaxFreq);

        //    Filter filter = new IIRFilter(coeff[0], coeff[1]);
        //    filter.Active = true;
        //    filter.Reset();
        //    return filter;
        //}
    }
}
