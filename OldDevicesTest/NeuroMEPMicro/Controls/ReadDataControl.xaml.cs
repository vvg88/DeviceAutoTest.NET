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
//using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.MathLib.Filters;
//using NeuroSoft.Hardware.Devices;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.Devices;
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for ReadStimulsControl.xaml
    /// </summary>
    public partial class ReadDataControl : UserControl, IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public ReadDataControl(ReadDataViewModel viewModel)
        {
            InitializeComponent();
            //DataContext = ;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            //SetDeviceInfo(device, amplifierCapabilities);
            this.ViewModel = viewModel;

            // Фильтры
            //ReFilters = new Filter[ChannelsCount];
            //Filters = new Filter[ChannelsCount];
            for (int i = 0; i < ChannelsCount; i++)
            {
                //ReFilters[i] = new IIRFilterRejector(50, 40, (float)viewModel.SamplesRateInSamplesPerSecond);
                //var filterInfo = new FilterInfo(viewModel.StatisticsCollection[i].ChannelName, viewModel.SamplesRateInSamplesPerSecond);
                //viewModel.FiltersInfo[i].FilterChanged += new RoutedEventHandler(filterInfo_FilterChanged);
                //viewModel.FiltersInfo.Add(filterInfo);
                //Filters[i] = viewModel.FiltersInfo[i].CreateFilter();
            }
            MonitoringPlotter.SelectedYScaleItem = MonitoringPlotter.YScaleItems[12];
            timer.Start();
        }

        private ReadDataViewModel viewModel;

        public ReadDataViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                if (viewModel != value)
                {
                    if (viewModel != null)
                    {
                        viewModel.RecieveData -= new RecieveDataHandler(ViewModel_RecieveData);
                    }
                    viewModel = value;
                    MonitoringPlotter.AmplifierCapabilities = new AmplifierCapabilities(true, viewModel.ChannelsCount, null, null, null, null, null, 0, null, false, viewModel.ChannelsCount * 2);//ViewModel.AmplifierCapabilities;
                    MonitoringPlotter.SamplesRateInSamplesPerSecond = ViewModel.SamplesRateInSamplesPerSecond;
                    DataContext = ViewModel;
                    ViewModel.RecieveData += new RecieveDataHandler(ViewModel_RecieveData);
                    //ViewModel.StartReadData();
                    //timer.Start();
                    //OnPropertyChanged("ViewModel");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="amplifierCapabilities"></param>
        //public void SetDeviceInfo(NeuroMepBase device, AmplifierCapabilities amplifierCapabilities)
        //{
        //    Device = device;
        //    AmplifierCapabilities = amplifierCapabilities;

        //    StatisticsCollection.Clear();

        //    for (int i = 0; i < ChannelsCount; i++)
        //    {
        //        string name = i.ToString();
        //        StatisticsCollection.Add(new DataStatistics(name));
        //    }
            
        //    samplesSum = new double[ChannelsCount];
        //    samplesSumSqr = new double[ChannelsCount];
        //    maxSamples = new float[ChannelsCount];
        //    minSamples = new float[ChannelsCount];

        //    ResetMinMax();
        //    AdcModeDescription adcModeDescription = AmplifierCapabilities.SupportedAdcMode[device.AmplifierAdcModeIndex];
        //    MonitoringPlotter.SamplesRateInSamplesPerSecond = adcModeDescription.Frequence;

        //    // Частота отсчётов
        //    double samplingRate = adcModeDescription.Frequence;            
 
        //    //Фильтры
        //    ReFilters = new Filter[ChannelsCount];
        //    Filters = new Filter[ChannelsCount];

        //    for (int i = 0; i < ChannelsCount; i++)
        //    {
        //        ReFilters[i] = new IIRFilterRejector(50, 40, (float)samplingRate);

        //        var filterInfo = new FilterInfo(StatisticsCollection[i].ChannelName, samplingRate);
        //        filterInfo.FilterChanged += new RoutedEventHandler(filterInfo_FilterChanged);
        //        FiltersInfo.Add(filterInfo);
        //        Filters[i] = filterInfo.CreateFilter();

        //        ChannelsInfo.Add(new ChannelInfo(i, Device, AmplifierCapabilities));
        //    }

        //    SetSavedSettings();
        //}

        //void filterInfo_FilterChanged(object sender, RoutedEventArgs e)
        //{                        
        //    FilterInfo filterInfo = sender as FilterInfo;            
        //    int index = viewModel.FiltersInfo.IndexOf(filterInfo);
        //    lock (this)
        //    {
        //        Filters[index] = filterInfo.CreateFilter();
        //    }
        //}

        #region Properties

        private DispatcherTimer timer = new DispatcherTimer();
        private NeuroMEPMicro Device;

        internal int ChannelsCount
        {
            get 
            {
                return viewModel.ChannelsCount; 
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

        //private ObservableCollection<FilterInfo> filtersInfo = new ObservableCollection<FilterInfo>();

        ///// <summary>
        ///// Информация о фильтрах
        ///// </summary>
        //public ObservableCollection<FilterInfo> FiltersInfo
        //{
        //    get { return filtersInfo; } 
        //}

        //private ObservableCollection<ChannelInfo> channelsInfo = new ObservableCollection<ChannelInfo>();

        ///// <summary>
        ///// Настройки каналов
        ///// </summary>
        //public ObservableCollection<ChannelInfo> ChannelsInfo
        //{
        //    get { return channelsInfo; }
        //}

        /*private AmplifierCapabilities amplifierCapabilities;     
        private AmplifierCapabilities AmplifierCapabilities            
        {
            get { return amplifierCapabilities; }
            set
            {
                amplifierCapabilities = value;
                MonitoringPlotter.AmplifierCapabilities = amplifierCapabilities;
            }
        }*/
                

        /*private ObservableCollection<DataStatistics> statisticsCollection = new ObservableCollection<DataStatistics>();

        /// <summary>
        /// Коллекция со статистикой 
        /// </summary>
        public ObservableCollection<DataStatistics> StatisticsCollection
        {
            get { return statisticsCollection; }
        }*/

        #endregion

        #region ChannelsSettings

        private ReadDataSettings savedSettings = null;

        /// <summary>
        /// Применение установок для контрола (фильтры аппаратные и программные, коэффициенты усиления, пределы значений сигналов)
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
                    viewModel.StatisticsCollection[i].Swing.MaxValue = settings.SwingRange.Max.Value;
                }
                if (settings.SwingRange.Min.HasValue)
                {
                    viewModel.StatisticsCollection[i].Swing.MinValue = settings.SwingRange.Min.Value;
                }

                if (settings.AverageRange.Max.HasValue)
                {
                    viewModel.StatisticsCollection[i].Average.MaxValue = settings.AverageRange.Max.Value;
                }
                if (settings.AverageRange.Min.HasValue)
                {
                    viewModel.StatisticsCollection[i].Average.MinValue = settings.AverageRange.Min.Value;
                }
                
                if (settings.RMSRange.Max.HasValue)
                {
                    viewModel.StatisticsCollection[i].RMS.MaxValue = settings.RMSRange.Max.Value;
                }
                if (settings.RMSRange.Min.HasValue)
                {
                    viewModel.StatisticsCollection[i].RMS.MinValue = settings.RMSRange.Min.Value;
                }

                if (settings.RangeIndex != null)
                {
                    viewModel.ChannelsInfo[i].SelectedChannelRange = viewModel.ChannelsInfo[i].ChannelSupportedRanges[settings.RangeIndex.Value];
                }
            }

            if (settings.MinFreqFilter.HasValue || settings.MaxFreqFilter.HasValue)
            {
                for (int i = 0; i < ChannelsCount; i++)
                {
                    if (settings.MinFreqFilter.HasValue)
                    {
                        if (viewModel.ChannelsInfo[i].LowFreqPassBands.Contains((float)settings.MinFreqFilter.Value))
                        {
                            viewModel.ChannelsInfo[i].SelectedLowFreq = (float)settings.MinFreqFilter.Value;
                            viewModel.FiltersInfo[i].IsEnabled = false;
                            viewModel.FiltersInfo[i].ApplyFilterSettings();
                        }
                        else
                        {
                            if (settings.MinFreqFilter.HasValue && settings.MaxFreqFilter.HasValue)
                            {
                                viewModel.FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, settings.MaxFreqFilter);
                                viewModel.FiltersInfo[i].IsEnabled = true;
                                continue;
                            }
                            else
                            {
                                viewModel.FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, 20000f);
                                viewModel.FiltersInfo[i].IsEnabled = true;
                            }
                        }
                    }
                    if (settings.MaxFreqFilter.HasValue)
                    {
                        if (viewModel.ChannelsInfo[i].HighFreqPassBands.Contains((float)settings.MaxFreqFilter.Value))
                        {
                            viewModel.ChannelsInfo[i].SelectedHighFreq = (float)settings.MaxFreqFilter.Value;
                            viewModel.FiltersInfo[i].IsEnabled = false;
                            viewModel.FiltersInfo[i].ApplyFilterSettings();
                        }
                        else
                        {
                            if (!settings.MinFreqFilter.HasValue)
                                viewModel.FiltersInfo[i].SetMinMaxFreq(0.0, settings.MaxFreqFilter);
                            else
                                viewModel.FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, settings.MaxFreqFilter);
                            viewModel.FiltersInfo[i].IsEnabled = true;
                        }
                    }
                }
                //if (settings.MinFreqFilter.HasValue || settings.MaxFreqFilter.HasValue)
                //{
                //    SetIsEnabledFilters(true);
                //}
            }

            if (settings.TickInterval != null)
            {
                timer.Interval = settings.TickInterval.Value;
            }

            if (settings.TickIntervalsProcessing != null)
            {
                viewModel.TickIntervalsProcessing = new RangedValue<int>(0, 0, settings.TickIntervalsProcessing.Value);
            }

            if (settings.SampleFrequency != 80000.0)
            {
                viewModel.SamplesRateInSamplesPerSecond = (float)settings.SampleFrequency;
            }
        }

        #endregion

        #region Filters

        //private bool enablePowerRejector = false;

        ///// <summary>
        ///// Включить фильтр сетевой помехи
        ///// </summary>
        //public bool EnablePowerRejector
        //{
        //    get { return enablePowerRejector; }
        //    set 
        //    {
        //        if (enablePowerRejector != value)
        //        {
        //            enablePowerRejector = value;                    
        //            OnPropertyChanged("EnablePowerRejector");
        //        }
        //    }
        //}

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
            foreach (var filterInfo in viewModel.FiltersInfo)
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
            foreach (var filterInfo in viewModel.FiltersInfo)
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
        //public bool StartReadData()
        //{
        //    if (Device == null || AmplifierCapabilities == null)
        //    {
        //        return false;
        //    }
        //    samples = 0;
        //    Array.Clear(samplesSum, 0, ChannelsCount);
        //    timer.Start();
        //    Device.AmplifierStartAdc(ReadSamplesStimulsData, this);
        //    return true;
        //}

        //private int samples = 0;
        //private double[] samplesSum;
        //private double[] samplesSumSqr;
        //private float[] maxSamples;
        //private float[] minSamples;

        /*private void ReadSamplesStimulsData(Hardware.Devices.SamplesStimulsDataEventArgs e, object sender)
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
        }*/

        void timer_Tick(object sender, EventArgs e)
        {
            if (MonitoringPlotter.Paused)
                return;
            viewModel.UpdateStatistic();
        }

        //private void ResetMinMax()
        //{
        //    for (int i = 0; i < ChannelsCount; i++)
        //    {
        //        maxSamples[i] = float.MinValue;
        //        minSamples[i] = float.MaxValue;
        //    }
        //}
        /*
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
        }*/

        void ViewModel_RecieveData(object sender, ReceiveDataArgs e)
        {
            SamplesStimulsDataEventArgs DataEventArgs = new SamplesStimulsDataEventArgs();
            DataEventArgs.ChannelsSamples = e.Data;
            DataEventArgs.SamplesCount = e.Data[1].Length;
            if (e.Points != null && e.Points.Count != 0)
            {
                DataEventArgs.Stimuls = new StimulusInfo[e.Points.Count];
                for (int i = 0; i < e.Points.Count; i++)
                {
                    DataEventArgs.Stimuls[i] = new StimulusInfo(e.Points[i].Time, 1);
                }
            }
            //for (int i = 0; i < ChannelsCount; i++)
            //{
            //    //if (viewModel.EnablePowerRejector)
            //    //{
            //    //    viewModel.ReFilters[i].DoFilter(DataEventArgs.ChannelsSamples[i], DataEventArgs.ChannelsSamples[i], 0, 0, DataEventArgs.SamplesCount);
            //    //}
            //    //if (viewModel.FiltersInfo[i].IsEnabled)
            //    //{
            //    //    Filters[i].DoFilter(DataEventArgs.ChannelsSamples[i], DataEventArgs.ChannelsSamples[i], 0, 0, DataEventArgs.SamplesCount);
            //    //}
            //}
            MonitoringPlotter.WriteData(DataEventArgs);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            viewModel.StopReadData();
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

        private TimeSpan? tickInterval = null;

        /// <summary>
        /// Временной интервал обновления отображения данных
        /// </summary>
        public TimeSpan? TickInterval
        {
            get { return tickInterval; }
            set { tickInterval = value; }
        }

        /// <summary>
        /// Количество тиков таймера, необходимое для обработки сигнала. 
        /// Необходимо устанавливать для сигналов частотой менее 2 Гц
        /// </summary>
        public int? TickIntervalsProcessing { get; set; }

        private int? rangeIndex = null;

        /// <summary>
        /// Индекс диапазона усиления
        /// </summary>
        public int? RangeIndex
        {
            get { return rangeIndex; }
            set { rangeIndex = value; }
        }

        private double sampleFrequency = 80000.0;
        /// <summary>
        /// Частота дискретизации
        /// </summary>
        public double SampleFrequency
        {
            get { return sampleFrequency; }
            set { sampleFrequency = value; }
        }
    }
}
