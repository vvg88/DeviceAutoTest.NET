using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.ComponentModel;
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
        public ReadDataControl(ReadDataViewModelBase viewModel)
        {
            InitializeComponent();
            timer.Interval = viewModel.TickInterval;
            timer.Tick += new EventHandler(timer_Tick);
            this.ViewModel = viewModel;

            EventsCounter = 0;
            timer.Start();
        }

        private ReadDataViewModelBase viewModel;

        public ReadDataViewModelBase ViewModel
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
                    viewModel.DataMonitoringPlotter = MonitoringPlotter;
                    DataContext = ViewModel;
                    ViewModel.RecieveData += new RecieveDataHandler(ViewModel_RecieveData);
                }
            }
        }

        #region Properties

        private DispatcherTimer timer = new DispatcherTimer();

        internal int ChannelsCount
        {
            get 
            {
                return viewModel.ChannelsCount; 
            }
        }

        /// <summary>
        /// Счетчик событий
        /// </summary>
        public int EventsCounter { get; private set; }

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
            viewModel.SetSavedSettings(settings);
        }

        

        #endregion

        #region Filters
        
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
        
        void timer_Tick(object sender, EventArgs e)
        {
            if (MonitoringPlotter.Paused)
                return;
            viewModel.UpdateStatistic();
        }
        
        void ViewModel_RecieveData(object sender, ReceiveDataArgs e)
        {
            SamplesStimulsDataEventArgs DataEventArgs = new SamplesStimulsDataEventArgs();
            DataEventArgs.ChannelsSamples = e.Data;
            DataEventArgs.SamplesCount = e.Data[0].Length;
            if (e.Points != null && e.Points.Count != 0)
            {
                DataEventArgs.Stimuls = new StimulusInfo[e.Points.Count];
                DataEventArgs.StimulsCount = e.Points.Count;
                EventsCounter += e.Points.Count;
                for (int i = 0; i < e.Points.Count; i++)
                {
                    if (e.Points[i].Time * viewModel.SamplesRateInSamplesPerSecond >= e.Data[0].Length)
                        DataEventArgs.Stimuls[i] = new StimulusInfo(0.01, 0);
                    else
                        DataEventArgs.Stimuls[i] = new StimulusInfo(e.Points[i].Time, 0);
                }
            }
            MonitoringPlotter.WriteData(DataEventArgs);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            viewModel.StopReadData();
            viewModel.RecieveData -= new RecieveDataHandler(ViewModel_RecieveData);
            timer.Stop();
            MonitoringPlotter.Dispose();
            DataContext = null;
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

        private ScaleItem yScale;
        /// <summary>
        /// Масштаб по вертикали
        /// </summary>
        public ScaleItem YScale
        {
            get { return yScale; }
            set { yScale = value; }
        }

        private ScaleItem xScale;
        /// <summary>
        /// Масштаб по горизонтали
        /// </summary>
        public ScaleItem XScale
        {
            get { return xScale; }
            set { xScale = value; }
        }
    }
}
