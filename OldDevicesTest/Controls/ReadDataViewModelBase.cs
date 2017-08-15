using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using NeuroSoft.DeviceAutoTest.ScriptExecution;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    public class ReadDataViewModelBase : DATBaseViewModel
    {
        public ReadDataViewModelBase(ScriptEnvironment environment, NSDevice device)
        {
            Environment = environment;
            Device = device;
            Device.ReceiveData += new ReceiveDataDelegate(DeviceReceiveDataHandler);
            TickIntervalsProcessing = new RangedValue<int>(0, 0, 0);
            TickInterval = new TimeSpan(0, 0, 1);
        }

        #region Properties
        /// <summary>
        /// Количество каналов в устройстве
        /// </summary>
        public virtual int ChannelsCount
        {
            get
            {
                return device.GetChannelsCount();
            }
            protected set { }
        }

        private ObservableCollection<ChannelInfoBase> channelsInfo = new ObservableCollection<ChannelInfoBase>();
        /// <summary>
        /// Настройки каналов
        /// </summary>
        public ObservableCollection<ChannelInfoBase> ChannelsInfo
        {
            get { return channelsInfo; }
        }

        private MonitoringPlotter dataMonitoringPlotter;
        /// <summary>
        /// Плоттер
        /// </summary>
        public MonitoringPlotter DataMonitoringPlotter 
        {
            get
            {
                return dataMonitoringPlotter;
            }
            set
            {
                if (dataMonitoringPlotter != value)
                {
                    dataMonitoringPlotter = value;
                    InitPlotter();
                }
            }
        }

        protected NSDevice device;
        /// <summary>
        /// Ссылка на устройство
        /// </summary>
        public virtual NSDevice Device
        {
            get { return device; }
            private set { device = value; }
        }

        protected bool enablePowerRejector = false;
        /// <summary>
        /// Включить фильтр сетевой помехи
        /// </summary>
        public virtual bool EnablePowerRejector
        {
            get 
            {
                for (int i = 0; i < ChannelsCount; i++)
                {
                    if (!Device.GetChannelState(i).FilterReEnabled)
                        return false;
                }
                return true; 
            }
            set
            {
                if (enablePowerRejector != value)
                {
                    enablePowerRejector = value;
                    OnPropertyChanged("EnablePowerRejector");
                }
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

        /// <summary>
        /// Флаг видимости кнопок установки фильтров. Сделано по просьбе наладчика не усложнять контрол
        /// </summary>
        public Visibility FiltersButtonsUsed
        {
            get { return (Device is EEG4Device) || (Device is EEG5Device) ? Visibility.Collapsed : Visibility.Visible; }
        }
        

        protected ObservableCollection<FilterInfo> filtersInfo = new ObservableCollection<FilterInfo>();
        /// <summary>
        /// Информация о программных фильтрах
        /// </summary>
        public ObservableCollection<FilterInfo> FiltersInfo
        {
            get { return filtersInfo; }
        }

        /// <summary>
        /// Частота дискретизации
        /// </summary>
        public virtual float SamplesRateInSamplesPerSecond { get; set; }

        protected ObservableCollection<DataStatistics> statisticsCollection = new ObservableCollection<DataStatistics>();
        /// <summary>
        /// Коллекция со статистикой 
        /// </summary>
        public ObservableCollection<DataStatistics> StatisticsCollection
        {
            get { return statisticsCollection; }
        }

        /// <summary>
        /// Частота обновления статистики
        /// </summary>
        public TimeSpan TickInterval { get; private set; }

        /// <summary>
        /// Число тактов таймера, необходимое для корректной обработки сигнала
        /// </summary>
        public RangedValue<int> TickIntervalsProcessing { get; set; }

        #endregion

        #region Fields

        protected bool isReading = false;

        protected int samples = 0;
        protected double[] samplesSum;
        protected double[] samplesSumSqr;
        protected float[] maxSamples;
        protected float[] minSamples;
        // Вводятся для хранения размахов сигнала, пока следующий период будет рассчитываться
        protected float[] maxSamplesSaved;
        protected float[] minSamplesSaved;

        #endregion

        #region Methods

        /// <summary>
        /// Пополнение статистики новыми данными
        /// </summary>
        /// <param name="e"></param>
        protected virtual void AppendDataStatistics(ReceiveDataArgs e)
        {
            int maxIndex = 0;
            int minIndex = 0;
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (e.Data[i] != null)
                {
                    samplesSum[i] += MathLib.Basic.SumArray_IPP(e.Data[i]);
                    float[] sqrSamples = new float[e.Data[i].Length];
                    MathLib.Basic.SqrOnArray_IPP(e.Data[i], sqrSamples);
                    samplesSumSqr[i] += MathLib.Basic.SumArray_IPP(sqrSamples);

                    List<float> iSamples = new List<float>(e.Data[i]);
                    MathLib.Basic.CalcMinMax(iSamples, out minIndex, out maxIndex);
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
                    if (i == 0)
                        samples += e.Data[i].Length;
                }
            }
        }

        /// <summary>
        /// Обработчик получения данных с устройства
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void DeviceReceiveDataHandler(object sender, ReceiveDataArgs e)
        {
            OnReadSamplesStimulsData(e, this);
            OnRecieveData(e);
        }
        /// <summary>
        /// Проводит инициализацию плоттера по умолчанию
        /// </summary>
        protected virtual void InitPlotter()
        {
            NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[] channelParams = new NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[ChannelsCount];
            for (int i = 0; i < ChannelsCount; i++)
            {
                string name = i.ToString();
                channelParams[i] = new Hardware.Devices.Controls.DataPlotter.ChannelParams(name, 30, 10, 1);
            }
            DataMonitoringPlotter.ChannelsCount = ChannelsCount;
            DataMonitoringPlotter.InitChannels(channelParams);
            DataMonitoringPlotter.SamplesRateInSamplesPerSecond = SamplesRateInSamplesPerSecond;
        }

        /// <summary>
        /// Вызывает обновления статистики
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        protected virtual void OnReadSamplesStimulsData(ReceiveDataArgs e, object sender)
        {
            AppendDataStatistics(e);
        }
        /// <summary>
        /// Вызывает событие о получении данных
        /// </summary>
        /// <param name="e"></param>
        private void OnRecieveData(ReceiveDataArgs e)
        {
            if (RecieveData != null)
            {
                RecieveData(this, e);
            }
        }

        /// <summary>
        /// Сброс статистики
        /// </summary>
        protected void ResetDataStatistics()
        {
            for (int i = 0; i < ChannelsCount; i++)
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
        protected void ResetMinMax()
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
        protected virtual void SetDeviceInfo()
        {
            samplesSum = new double[ChannelsCount];
            samplesSumSqr = new double[ChannelsCount];
            maxSamples = new float[ChannelsCount];
            minSamples = new float[ChannelsCount];
            maxSamplesSaved = new float[ChannelsCount];
            minSamplesSaved = new float[ChannelsCount];
        }

        /// <summary>
        /// Применяет настройки
        /// </summary>
        public virtual void SetSavedSettings(ReadDataSettings settings)
        {
            //if (savedSettings == null)
            //    return;
            //var settings = savedSettings;

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
                if (settings.SampleFrequency != 80000.0)
                {
                    SamplesRateInSamplesPerSecond = (float)settings.SampleFrequency;
                }

                if (settings.YScale != null)
                {
                    DataMonitoringPlotter.SelectedYScaleItem = settings.YScale;
                }

                if (settings.XScale != null)
                {
                    DataMonitoringPlotter.SelectedXScaleItem = settings.XScale;
                }
                if (settings.TickInterval != null)
                {
                    TickInterval = settings.TickInterval.Value;
                }
                if (settings.TickIntervalsProcessing != null)
                {
                    TickIntervalsProcessing = new RangedValue<int>(0, 0, settings.TickIntervalsProcessing.Value);
                }
            }
        }

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
            if (isReading)
            {
                ResetDataStatistics();
                Device.StopTransmit();
                //Device.ReceiveData -= new ReceiveDataDelegate(DeviceReceiveDataHandler);
                isReading = false;
            }
        }

        /// <summary>
        /// Обновление статистики
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void UpdateStatistic()
        {
            if (TickIntervalsProcessing != null)
                TickIntervalsProcessing.Value++;
            for (int i = 0; i < ChannelsCount; i++)
            {
                double averange = samplesSum[i] / samples;
                if (samples > 0)
                {
                    // Отображается размах, рассчитаныый на предыдущем интервале TickIntervalsProcessing. Если частота выше 1 Гц, то отображается актуальный размах.
                    // Если ниже, то размах отображается с запаздыванием на TickIntervalsProcessing.
                    if (minSamples[i] < minSamplesSaved[i] || !TickIntervalsProcessing.IsValidValue)
                        minSamplesSaved[i] = minSamples[i];
                    if (maxSamples[i] > maxSamplesSaved[i] || !TickIntervalsProcessing.IsValidValue)
                        maxSamplesSaved[i] = maxSamples[i];
                    StatisticsCollection[i].UpdateStatistics(averange, minSamplesSaved[i], maxSamplesSaved[i], Math.Sqrt(samplesSumSqr[i] / samples - averange * averange));
                }
                else
                {
                    StatisticsCollection[i].UpdateStatistics(double.NaN, double.NaN, double.NaN, double.NaN);
                }
            }
            if (TickIntervalsProcessing != null && !TickIntervalsProcessing.IsValidValue)
            {
                samples = 0;
                TickIntervalsProcessing.Value = 0;
                Array.Clear(samplesSum, 0, ChannelsCount);
                Array.Clear(samplesSumSqr, 0, ChannelsCount);
                ResetMinMax();
            }
        }

        #endregion

        /// <summary>
        /// Событие о передаче данных
        /// </summary>
        public event RecieveDataHandler RecieveData;
    }

    /// <summary>
    /// Делегат обработчика событий о получении данных
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RecieveDataHandler(object sender, ReceiveDataArgs e);

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
                IsEnabledChanged = true;
                if (isEnabled != value)
                {
                    isEnabled = value;
                    //IsEnabledChanged = true;
                    OnPropertyChanged("IsEnabled");
                    OnFilterChanged();
                }
            }
        }
        /// <summary>
        /// Флаг, что событие об изменении фильтра вызвано изменением его разрегения
        /// </summary>
        public bool IsEnabledChanged { get; private set; }

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
                    IsEnabledChanged = false;
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
                    IsEnabledChanged = false;
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
            IsEnabledChanged = false;
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

    public class ChannelInfoBase : DATBaseViewModel
    {
        public ChannelInfoBase(int channelNum, NSDevice device)
        {
            ChannelNumber = channelNum;
            this.device = device;
        }

        #region Fields
        /// <summary>
        /// Ссылка на экземпляр устройства
        /// </summary>
        protected NSDevice device;

        #endregion

        #region Properties

        protected int channelNumber;
        /// <summary>
        /// Номер канала
        /// </summary>
        public virtual int ChannelNumber
        {
            get { return channelNumber; }
            private set { channelNumber = value; }
        }
        /// <summary>
        /// Поддерживаемые ФНЧ
        /// </summary>
        public virtual List<float> HighFreqPassBands { get; set; }
        /// <summary>
        /// Выбранная частота ФНЧ
        /// </summary>
        public virtual float SelectedHighFreq { get; set; }
        /// <summary>
        /// Поддерживаемые ФВЧ
        /// </summary>
        public virtual List<float> LowFreqPassBands { get; set; }
        /// <summary>
        /// Выбранная частота ФВЧ
        /// </summary>
        public virtual float SelectedLowFreq { get; set; }
        /// <summary>
        /// Поддерживаемые диапазоны усиления каналов
        /// </summary>
        public virtual List<float> ChannelSupportedRanges { get; set; }
        /// <summary>
        /// Диапазон канала усиления
        /// </summary>
        public virtual float SelectedChannelRange { get; set; }
        /// <summary>
        /// Признак использования канала
        /// </summary>
        public virtual bool ChannelIsNull
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
                //(device as NeuroMEPMicro).SetChannelsUsing();
                OnPropertyChanged("ChannelIsNull");
            }
        }

        #endregion
    }
}
