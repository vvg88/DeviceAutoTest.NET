using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.MathLib.Filters;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.Hardware.Common;
using NeuroSoft.WPFComponents.ScalableWindows;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for ReadStimulsControl.xaml
    /// </summary>
    public partial class ReadDataControl : UserControl, IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public ReadDataControl(NeuroMepBase device, AmplifierCapabilities amplifierCapabilities)
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            SetDeviceInfo(device, amplifierCapabilities);
            MonitoringPlotter.SelectedYScaleItem = MonitoringPlotter.YScaleItems[12];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="amplifierCapabilities"></param>
        public void SetDeviceInfo(NeuroMepBase device, AmplifierCapabilities amplifierCapabilities)
        {
            Device = device;
            AmplifierCapabilities = amplifierCapabilities;

            StatisticsCollection.Clear();

            for (int i = 0; i < ChannelsCount; i++)
            {
                string name = i.ToString();
                StatisticsCollection.Add(new DataStatistics(name));
            }
            
            samplesSum = new double[ChannelsCount];
            samplesSumSqr = new double[ChannelsCount];
            maxSamples = new float[ChannelsCount];
            minSamples = new float[ChannelsCount];

            ResetMinMax();
            AdcModeDescription adcModeDescription = AmplifierCapabilities.SupportedAdcMode[device.AmplifierAdcModeIndex];
            MonitoringPlotter.SamplesRateInSamplesPerSecond = adcModeDescription.Frequence;

            // Частота отсчётов
            double samplingRate = adcModeDescription.Frequence;            
 
            //Фильтры
            ReFilters = new Filter[ChannelsCount];
            Filters = new Filter[ChannelsCount];

            for (int i = 0; i < ChannelsCount; i++)
            {
                ReFilters[i] = new IIRFilterRejector(50, 40, (float)samplingRate);

                var filterInfo = new FilterInfo(StatisticsCollection[i].ChannelName, samplingRate);
                filterInfo.FilterChanged += new RoutedEventHandler(filterInfo_FilterChanged);
                FiltersInfo.Add(filterInfo);
                Filters[i] = filterInfo.CreateFilter();

                ChannelsInfo.Add(new ChannelInfo(i, Device, AmplifierCapabilities));
            }

            SetSavedSettings();
        }

        void filterInfo_FilterChanged(object sender, RoutedEventArgs e)
        {                        
            FilterInfo filterInfo = sender as FilterInfo;            
            int index = FiltersInfo.IndexOf(filterInfo);
            lock (this)
            {
                Filters[index] = filterInfo.CreateFilter();
            }
        }

        #region Properties

        private DispatcherTimer timer = new DispatcherTimer();
        private NeuroMepBase Device;

        internal int ChannelsCount
        {
            get 
            {
                return AmplifierCapabilities.ChannelsCount; 
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

        private ObservableCollection<ChannelInfo> channelsInfo = new ObservableCollection<ChannelInfo>();

        /// <summary>
        /// Настройки каналов
        /// </summary>
        public ObservableCollection<ChannelInfo> ChannelsInfo
        {
            get { return channelsInfo; }
        }

        private AmplifierCapabilities amplifierCapabilities;     
        private AmplifierCapabilities AmplifierCapabilities            
        {
            get { return amplifierCapabilities; }
            set
            {
                amplifierCapabilities = value;
                MonitoringPlotter.AmplifierCapabilities = amplifierCapabilities;
            }
        }
                

        private ObservableCollection<DataStatistics> statisticsCollection = new ObservableCollection<DataStatistics>();

        /// <summary>
        /// Коллекция со статистикой 
        /// </summary>
        public ObservableCollection<DataStatistics> StatisticsCollection
        {
            get { return statisticsCollection; }
        }

        #endregion

        #region ChannelsSettings

        private ReadDataSettings savedSettings = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public void SetSettings(ReadDataSettings settings)
        {
            savedSettings = settings;
            SetSavedSettings();
        }

        private void SetSavedSettings()
        {
            if (savedSettings == null)
                return;
            var settings = savedSettings;

            for (int i = 0; i < ChannelsCount; i++)
            {
                if (settings.SwingRange.Max.HasValue)
                {
                    StatisticsCollection[i].Swing.MaxValue = settings.SwingRange.Max.Value;
                }
                if (settings.SwingRange.Min.HasValue)
                {
                    StatisticsCollection[i].Swing.MinValue = settings.SwingRange.Min.Value;
                }

                if (settings.AverageRange.Max.HasValue)
                {
                    StatisticsCollection[i].Average.MaxValue = settings.AverageRange.Max.Value;
                }
                if (settings.AverageRange.Min.HasValue)
                {
                    StatisticsCollection[i].Average.MinValue = settings.AverageRange.Min.Value;
                }


                if (settings.RMSRange.Max.HasValue)
                {
                    StatisticsCollection[i].RMS.MaxValue = settings.RMSRange.Max.Value;
                }
                if (settings.RMSRange.Min.HasValue)
                {
                    StatisticsCollection[i].RMS.MinValue = settings.RMSRange.Min.Value;
                }

                if (settings.RangeIndex != null)
                {
                    Device.AmplifierSetChannelRangeIndex(i, settings.RangeIndex.Value);
                }
            }

            if (settings.MinFreqFilter.HasValue || settings.MaxFreqFilter.HasValue)
            {
                foreach (var filterInfo in FiltersInfo)
                {
                    filterInfo.SetMinMaxFreq(settings.MinFreqFilter, settings.MaxFreqFilter);
                }
                if (settings.MinFreqFilter.HasValue && settings.MaxFreqFilter.HasValue)
                {
                    SetIsEnabledFilters(true);
                }
            }
            if (settings.EnablePowerRejector.HasValue)
            {
                EnablePowerRejector = settings.EnablePowerRejector.Value;
            }
            if (settings.TickInterval != null)
            {
                timer.Interval = settings.TickInterval.Value;
            }
            if (settings.XScaleIndex != null && settings.XScaleIndex.Value > -1 && settings.XScaleIndex.Value < MonitoringPlotter.XScaleItems.Count())
            {
                MonitoringPlotter.SelectedXScaleItem = MonitoringPlotter.XScaleItems[settings.XScaleIndex.Value];
            }
            if (settings.YScaleIndex != null && settings.YScaleIndex.Value > -1 && settings.YScaleIndex.Value < MonitoringPlotter.YScaleItems.Count())
            {
                MonitoringPlotter.SelectedYScaleItem = MonitoringPlotter.YScaleItems[settings.YScaleIndex.Value];
            }
        }

        #endregion

        #region Filters


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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledFilters(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isEnabled"></param>
        public void SetIsEnabledFilters(bool isEnabled)
        {
            foreach (var filterInfo in FiltersInfo)
            {
                filterInfo.IsEnabled = isEnabled;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minFreq"></param>
        /// <param name="maxFreq"></param>
        public void SetMinMaxFreqFilters(double minFreq, double maxFreq)
        {            
            foreach (var filterInfo in FiltersInfo)
            {
                filterInfo.SetMinMaxFreq(minFreq, maxFreq);
            }
            SetIsEnabledFilters(true);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SetMinMaxFreqFilters(1, 100);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SetMinMaxFreqFilters(20, 10000);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SetMinMaxFreqFilters(0, 20);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            SetMinMaxFreqFilters(10, 500);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            SetMinMaxFreqFilters(200, 20000);
        }
        #endregion

        #region ReadData

        /// <summary>
        /// 
        /// </summary>
        public bool StartReadData()
        {
            if (Device == null || AmplifierCapabilities == null)
            {
                return false;
            }
            samples = 0;
            Array.Clear(samplesSum, 0, ChannelsCount);
            timer.Start();
            //Device.AmplifierStartAdc(ReadSamplesStimulsData, null/*this*/);
            Device.AmplifierMonitoringStart(ReadSamplesStimulsData, ReadSamplesErrHandler);
            return true;
        }

        private int samples = 0;
        private double[] samplesSum;
        private double[] samplesSumSqr;
        private float[] maxSamples;
        private float[] minSamples;

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
            int maxIndex = 0;
            int minIndex = 0;
            for (int i = 0; i < ChannelsCount; i++)
            {
                samplesSum[i] += NeuroSoft.MathLib.Basic.SumArray_IPP(e.ChannelsSamples[i]);
                float[] sqrSamples = new float[e.SamplesCount];
                NeuroSoft.MathLib.Basic.SqrOnArray_IPP(e.ChannelsSamples[i], sqrSamples);
                samplesSumSqr[i] += NeuroSoft.MathLib.Basic.SumArray_IPP(sqrSamples);                

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
            MonitoringPlotter.WriteData(e);
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

        void timer_Tick(object sender, EventArgs e)
        {
            if (MonitoringPlotter.Paused)
                return;
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
            samples = 0;
            Array.Clear(samplesSum, 0, ChannelsCount);
            Array.Clear(samplesSumSqr, 0, ChannelsCount);
            ResetMinMax();
        }

        private void ResetMinMax()
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                maxSamples[i] = float.MinValue;
                minSamples[i] = float.MaxValue;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopReadData()
        {
            timer.Stop();
            if (Device != null)
            {
                Device.AmplifierStopAdc();
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            StopReadData();
            MonitoringPlotter.Dispose();
            Device = null;
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

    /// <summary>
    /// 
    /// </summary>
    public class ReadDataSettings
    {
        private Range<double?> averageRange = new Range<double?>(null, null);
        /// <summary>
        /// Ограничение среднего значения
        /// </summary>
        public Range<double?> AverageRange
        {
            get { return averageRange; }
        }

        private Range<double?> swingRange = new Range<double?>(null, null);
        /// <summary>
        /// Ограничение размаха
        /// </summary>
        public Range<double?> SwingRange
        {
            get { return swingRange; }
        }


        private Range<double?> rmsRange = new Range<double?>(null, null);
        /// <summary>
        /// Ограничение размаха
        /// </summary>
        public Range<double?> RMSRange
        {
            get { return rmsRange; }
        }

        private double? minFreqFilter = null;

        /// <summary>
        /// Нижняя граница программного фильтра
        /// </summary>
        public double? MinFreqFilter
        {
            get { return minFreqFilter; }
            set { minFreqFilter = value; }
        }

        private double? maxFreqFilter = null;
        /// <summary>
        /// Верхняя граница программного фильтра
        /// </summary>
        public double? MaxFreqFilter
        {
            get { return maxFreqFilter; }
            set { maxFreqFilter = value; }
        }

        private bool? enablePowerRejector = null;
        /// <summary>
        /// Включить сетевой фильтр
        /// </summary>
        public bool? EnablePowerRejector
        {
            get { return enablePowerRejector; }
            set { enablePowerRejector = value; }
        }

        private TimeSpan? tickInterval = null;

        /// <summary>
        /// Временной интервал обработки данных
        /// </summary>
        public TimeSpan? TickInterval
        {
            get { return tickInterval; }
            set { tickInterval = value; }
        }

        private int? rangeIndex = null;

        /// <summary>
        /// Индекс диапазона усиления
        /// </summary>
        public int? RangeIndex
        {
            get { return rangeIndex; }
            set { rangeIndex = value; }
        }

        private int? xScaleIndex;
        /// <summary>
        /// Индекс масштаба по оси абсцисс (из значений MonitoringPlotter.XScaleItems)
        /// </summary>
        public int? XScaleIndex
        {
            get { return xScaleIndex; }
            set { xScaleIndex = value; }
        }

        private int? yScaleIndex;
        /// <summary>
        /// Индекс масштаба по оси абсцисс (из значений MonitoringPlotter.YScaleItems)
        /// </summary>
        public int? YScaleIndex
        {
            get { return yScaleIndex; }
            set { yScaleIndex = value; }
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
        /// Метод создания фильтра на основе данных этого класса
        /// </summary>
        /// <returns></returns>
        public Filter CreateFilter()
        {
            double[][] coeff;

            if (MinFreq == 0)
                coeff = ButterworthIIRF_Designing.GetCoefficients_1_Standard(
                    FilterType.LF,
                    4,
                    1,
                    SamplingFreq,
                    MaxFreq);
            else
                coeff = ButterworthIIRF_Designing.GetCoefficients_1_Standard(
                    FilterType.Band,
                    4,
                    1,
                    SamplingFreq,
                    MinFreq,
                    MaxFreq);

            Filter filter = new IIRFilter(coeff[0], coeff[1]);
            filter.Active = true;
            filter.Reset();
            return filter;
        }
    }

    /// <summary>
    /// Настройки канала
    /// </summary>
    public class ChannelInfo : DATBaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingFreq"></param>
        public ChannelInfo(int channelNum, NeuroMepBase device, AmplifierCapabilities capabilities)
        {
            ChannelNumber = channelNum;
            Device = device;
            Capabilities = capabilities;
        }

        private NeuroMepBase Device;
        private AmplifierCapabilities Capabilities;
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
                    highFreqPassBands = Capabilities.GetChannelSupportedHiFreqPassBand(ChannelNumber).ToList();
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
                if (Device == null || HighFreqPassBands.Count == 0)
                {
                    return -1;
                }
                return HighFreqPassBands[Device.AmplifierGetChannelHiFreqPassBandIndex(ChannelNumber)]; 
            }
            set 
            {
                if (Device == null || HighFreqPassBands.Count == 0)
                    return;
                Device.AmplifierSetChannelHiFreqPassBandIndex(ChannelNumber, HighFreqPassBands.IndexOf(value));
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
                    lowFreqPassBands = Capabilities.GetChannelSupportedLowFreqPassBand(ChannelNumber).ToList();
                }
                return lowFreqPassBands;
            }
        }

        /// <summary>
        /// Индекс нижней частоты полосы пропускания сигнала
        /// </summary>
        public float SelectedLowFreq
        {
            get
            {
                if (Device == null || LowFreqPassBands.Count == 0)
                {
                    return -1;
                }
                return LowFreqPassBands[Device.AmplifierGetChannelLowFreqPassBandIndex(ChannelNumber)];
            }
            set 
            {
                if (Device == null || LowFreqPassBands.Count == 0)
                    return;
                Device.AmplifierSetChannelLowFreqPassBandIndex(ChannelNumber, LowFreqPassBands.IndexOf(value));
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
                    channelSupportedRanges = Capabilities.GetChannelSupportedRange(ChannelNumber).ToList();
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
                if (Device == null || ChannelSupportedRanges.Count == 0)
                {
                    return -1;
                }
                return ChannelSupportedRanges[Device.AmplifierGetChannelRangeIndex(ChannelNumber)];
            }
            set
            {                
                if (Device == null || ChannelSupportedRanges.Count == 0)
                    return;
                Device.AmplifierSetChannelRangeIndex(ChannelNumber, ChannelSupportedRanges.IndexOf(value));
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
                if (Device == null)
                {
                    return false;
                }
                return Device.AmplifierGetChannelNull(ChannelNumber);
            }
            set
            {
                Device.AmplifierSetChannelNull(ChannelNumber, value);
                OnPropertyChanged("ChannelIsNull");
            }
        }
    }
}
