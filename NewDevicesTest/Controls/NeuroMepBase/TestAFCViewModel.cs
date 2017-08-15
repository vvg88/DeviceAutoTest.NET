using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.Hardware.Devices.Base;
using System.Collections.ObjectModel;
using System.Windows;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using System.Windows.Threading;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.NewDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{

    /// <summary>
    /// 
    /// </summary>
    public class TestAFCViewModel : ReadDataViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="device"></param>
        /// <param name="amplifierCapabilities"></param>
        public TestAFCViewModel(ScriptEnvironment environment, NeuroMepBase device, AmplifierCapabilities amplifierCapabilities, int lowFreqPassBandIndex, float signalAmpl)
            : base(environment, device, amplifierCapabilities)
        {            
            SignalAmpl = signalAmpl;
            LowFreqPassBandIndex = lowFreqPassBandIndex;
        }

        #region Properties
       
        private float signalAmpl;
        /// <summary>
        /// Амплитуда генерируемого сигнала
        /// </summary>
        public float SignalAmpl
        {
            get { return signalAmpl; }
            private set { signalAmpl = value; }
        }

        private int lowFreqPassBandIndex;
        /// <summary>
        /// Индекс нижней границы частоты пропускания
        /// </summary>
        public int LowFreqPassBandIndex
        {
            get { return lowFreqPassBandIndex; }
            private set { lowFreqPassBandIndex = value; }
        }       

        private ObservableCollection<AFCItemsRow> rows = new ObservableCollection<AFCItemsRow>();
        /// <summary>
        /// Список строк (по частотам)
        /// </summary>
        public ObservableCollection<AFCItemsRow> Rows
        {
            get { return rows; }
        }

        private AFCProcessingInfo processingItemInfo;
        /// <summary>
        /// Информация об обрабатываемых частоте и диапазоне
        /// </summary>
        internal AFCProcessingInfo ProcessingItemInfo
        {
            get { return processingItemInfo; }
            private set 
            {
                if (processingItemInfo != null)
                {
                    processingItemInfo.Stop();
                }
                processingItemInfo = value;
            }
        }

        private bool isTesting;
        /// <summary>
        /// Признак обработки данных
        /// </summary>
        public bool IsTesting
        {
            get { return isTesting; }
            private set
            {
                if (isTesting != value)
                {
                    isTesting = value;
                    OnPropertyChanged("IsTesting");
                }
            }
        }

        private bool settingParams = false;
        /// <summary>
        /// Признак обработки данных
        /// </summary>
        public bool SettingParams
        {
            get { return settingParams; }
            private set
            {
                if (settingParams != value)
                {
                    settingParams = value;
                    OnPropertyChanged("SettingParams");
                }
            }
        }
        

        private string testingStatus;
        /// <summary>
        /// Текстовое описание состояния обработки
        /// </summary>
        public string TestingStatus
        {
            get { return testingStatus; }
            set
            {
                if (testingStatus != value)
                {
                    testingStatus = value;
                    OnPropertyChanged("TestingStatus");
                }
            }
        }

        private AFCItemsRow currentRow;
        /// <summary>
        /// Текущая строка (частота)
        /// </summary>
        public AFCItemsRow CurrentRow
        {
            get { return currentRow; }
            set
            {
                if (currentRow != value)
                {
                    currentRow = value;
                    OnPropertyChanged("CurrentRow");
                }
            }
        }
        #endregion

        #region Methods

        #region Processing

        private bool processToEnd = false;

        /// <summary>
        /// Определение размаха сигнала для заданных частоты и диапазона
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="rangeIndex"></param>
        /// <param name="toEnd">Продолжать ли опредление размаха для остальных частот и диапазонов</param>
        public void StartProcess(float freq, int rangeIndex, bool toEnd)
        {
            try
            {                
                SettingParams = true;
                processToEnd = toEnd;
                ProcessingItemInfo = new AFCProcessingInfo(this, freq, rangeIndex);
                SetStandParams(freq, rangeIndex);
                StartReadData();
                ResetDataStatistics();
                ProcessingItemInfo.Start();
                CurrentRow = Rows.LastOrDefault(r => r.Frequency == freq);
                TestingStatus = string.Format(Properties.Resources.TestAFCStart, freq, AmplifierCapabilities.GetChannelSupportedRange(0)[rangeIndex]);
                IsTesting = true;
            }
            finally
            {
                SettingParams = false;                
            }
        }

        private void SetStandParams(float freq, int rangeIndex)
        {            
            StandOperations.SetGeneratorState(Environment, SignalAmpl, freq);
            for (int i = 0; i < AmplifierCapabilities.ChannelsCount; i++)
            {
                Device.AmplifierSetChannelRangeIndex(i, rangeIndex);
                Device.AmplifierSetChannelLowFreqPassBandIndex(i, LowFreqPassBandIndex);
            }
        }

        private void TestProcessItem()
        {
            if (ProcessingItemInfo == null)
                return;
            lock (ProcessingItemInfo)
            {                
                if (ProcessingItemInfo.Timeouted) //заканчиваем определение размаха по таймауту
                {
                    EndProcessItem();
                }
                else if (samples >= ProcessingItemInfo.SamplesLimit)
                {
                    if (UpdateAFCItems())
                    {
                        EndProcessItem();
                    }
                }
            }
        }

        private bool UpdateAFCItems()
        {
            if (ProcessingItemInfo == null)
                return false;
            bool hasInvalid = false;
            foreach (var item in ProcessingItemInfo.ProcessingItems)
            {
                item.SetSwingValue(maxSamples[item.Channel] - minSamples[item.Channel]);
                if (!item.Swing.IsValidValue)
                {
                    hasInvalid = true;
                }
            }
            ResetDataStatistics();
            return !hasInvalid;
        }

        private void EndProcessItem()
        {
            if (ProcessingItemInfo == null)
                return;
            bool success = ProcessingItemInfo.SwingIsValid;
            float nextFreq = ProcessingItemInfo.NextFreq;
            int nextRange = ProcessingItemInfo.NextRangeIndex;
            bool hasNext = ProcessingItemInfo.HasNext;
            ProcessingItemInfo = null;
            if (processToEnd && success && hasNext)
            {
                StartProcess(nextFreq, nextRange, processToEnd);
            }
            else
            {
                StopProcess(Properties.Resources.TestAFCFinished);
                Environment.DoAutoTest();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
        public void StopProcess(string reason)
        {            
            IsTesting = false;
            TestingStatus = reason;
            ProcessingItemInfo = null;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                StopReadData();
            }));
        }
        #endregion

        #region ReadData

        /// <summary>
        /// 
        /// </summary>
        public override void StopReadData()
        {            
            ProcessingItemInfo = null;
            base.StopReadData();
        }

        protected override void OnReadSamplesStimulsData(SamplesStimulsDataEventArgs e, object sender)
        {
            if (ProcessingItemInfo == null || !ProcessingItemInfo.Started || !ProcessingItemInfo.DelayIsOut)
            {
                if (samples > 0)
                {
                    ResetDataStatistics();
                }
                return;
            }
            base.OnReadSamplesStimulsData(e, sender);
            TestProcessItem();            
        }
        
        #endregion

        /// <summary>
        /// Остановить обработку данных или повторить обработку в зависимости от текущего состояния
        /// </summary>
        public void StopOrStart()
        {
            if (SettingParams)
                return;
            if (IsTesting)
            {
                StopProcess(Properties.Resources.TestAFCAborted);
            }
            else
            {
                foreach (var row in Rows)
                {
                    foreach (var item in row.ItemsList)
                    {
                        item.SetSwingValue(double.NaN);
                    }
                }
                if (Rows.Count > 0)
                {
                    StartProcess(Rows[0].Frequency, Rows[0].ItemsList[0].RangeIndex, true);
                }
            }
        }
        #endregion        
    }
    

    /// <summary>
    /// 
    /// </summary>
    public class AFCItem : DATBaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="rangeIndex"></param>
        /// <param name="channel"></param>
        /// <param name="minSwing"></param>
        /// <param name="maxSwing"></param>
        public AFCItem(float freq, int rangeIndex, int channel, double minSwing, double maxSwing)
        {
            Frequency = freq;
            RangeIndex = rangeIndex;
            Channel = channel;
            swing.MinValue = minSwing;
            swing.MaxValue = maxSwing;
        }

        #region Properties
        private int rangeIndex;
        /// <summary>
        /// Индекс диапазона
        /// </summary>
        public int RangeIndex
        {
            get { return rangeIndex; }
            private set
            {
                rangeIndex = value;
                OnPropertyChanged("RangeIndex");
            }
        }

        private int channel;
        /// <summary>
        /// Канал
        /// </summary>
        public int Channel
        {
            get { return channel; }
            private set
            {
                channel = value;
                OnPropertyChanged("Channel");
            }
        }

        private float frequency;
        /// <summary>
        /// Частота
        /// </summary>
        public float Frequency
        {
            get { return frequency; }
            private set
            {
                frequency = value;
                float periodTime = 1 / Frequency; //Время одного периода (с)
                float averageTime = 2 * periodTime; //Время, необходимое для усреднения (минимум два периода)
                if (averageTime < 0.1f)
                {
                    averageTime = 0.1f;
                }
                SamplesLimit = Convert.ToInt32(averageTime * 100000/*Частота квантования*/);
                OnPropertyChanged("Frequency");
            }
        }

        private int samplesLimit = -1;
        /// <summary>
        /// Количество отсчетов для усреднения
        /// </summary>
        public int SamplesLimit
        {
            get { return samplesLimit; }
            private set { samplesLimit = value; }
        }
        private RangedValue<double> swing = new RangedValue<double>(double.NaN, 0, double.MaxValue);
        /// <summary>
        /// Размах
        /// </summary>
        public RangedValue<double> Swing
        {
            get { return swing; }
            set
            {
                if (swing != value)
                {
                    swing = value;
                    OnPropertyChanged("Swing");
                }
            }
        }

        /// <summary>
        /// Установить значение размаха с уведомлением о возможном изменении состояния ячейки
        /// </summary>
        /// <param name="swing"></param>
        public void SetSwingValue(double swing)
        {
            Swing.Value = swing;
            OnPropertyChanged("SwingIsValid");
        }


        /// <summary>
        /// 
        /// </summary>
        public bool? SwingIsValid
        {
            get
            {
                if (double.IsNaN(Swing.Value))
                    return null;
                return Swing.IsValidValue;
            }
        }

        private bool isProcessing = false;

        /// <summary>
        /// Признак активности процесса вычисления размаха
        /// </summary>
        public bool IsProcessing
        {
            get { return isProcessing; }
            internal set
            {
                isProcessing = value;
                OnPropertyChanged("IsProcessing");
            }
        }
        
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class AFCItemsRow : DATBaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="freq"></param>
        /// <param name="minSweep"></param>
        /// <param name="maxSweep"></param>
        public AFCItemsRow(int channel, float freq, double minSweep, double maxSweep)
        {
            Frequency = freq;
            Channel = channel;
            UpdateRangeItems(minSweep, maxSweep);
        }

        #region Properties
        private float frequency;
        /// <summary>
        /// Частота
        /// </summary>
        public float Frequency
        {
            get { return frequency; }
            private set
            {
                frequency = value;
                OnPropertyChanged("Frequency");
            }
        }

        private int channel;
        /// <summary>
        /// Канал
        /// </summary>
        public int Channel
        {
            get { return channel; }
            private set
            {
                channel = value;
                OnPropertyChanged("Channel");
            }
        }

        /// <summary>
        /// Заголовок строки
        /// </summary>
        public string Header
        {
            get
            {
                string freqStr = Frequency < 1000 ? string.Format("{0:0.#}", Frequency) + " Hz" : string.Format("{0:0.#}", Frequency/1000f) + " kHz";
                return "Freq: " + string.Format("{0:0.#}", Frequency) + " Hz" + "\n"
                     + "Channel: " + Channel;
            }
        }

        private AFCItem range0Item;
        /// <summary>
        /// Диапазон с индексом 0
        /// </summary>
        public AFCItem Range0Item
        {
            get { return range0Item; }
            private set
            {
                range0Item = value;
                OnPropertyChanged("Range0Item");
            }
        }

        private AFCItem range1Item;
        /// <summary>
        /// Диапазон с индексом 1
        /// </summary>
        public AFCItem Range1Item
        {
            get { return range1Item; }
            private set
            {
                range1Item = value;
                OnPropertyChanged("Range1Item");
            }
        }

        private AFCItem range2Item;
        /// <summary>
        /// Диапазон с индексом 2
        /// </summary>
        public AFCItem Range2Item
        {
            get { return range2Item; }
            private set
            {
                range2Item = value;
                OnPropertyChanged("Range2Item");
            }
        }

        private AFCItem range3Item;
        /// <summary>
        /// Диапазон с индексом 3
        /// </summary>
        public AFCItem Range3Item
        {
            get { return range3Item; }
            private set
            {
                range3Item = value;
                OnPropertyChanged("Range3Item");
            }
        }

        private List<AFCItem> itemsList = null;
        internal List<AFCItem> ItemsList
        {
            get
            {
                if (itemsList == null)
                {
                    itemsList = new List<AFCItem>();
                    itemsList.Add(Range0Item);
                    itemsList.Add(Range1Item);
                    itemsList.Add(Range2Item);
                    itemsList.Add(Range3Item);
                    return itemsList;
                }
                return itemsList;
            }
        }

        /// <summary>
        /// Значение размаха для протокола
        /// </summary>
        public double ResultSwing
        {
            get
            {
                return (from item in ItemsList select item.Swing.Value).Max();
            }
        }

        /// <summary>
        /// Признак успешности выполнения теста для данной строки
        /// </summary>
        public bool SuccessTest
        {
            get 
            {
                foreach (var item in ItemsList)
                {
                    if (item.SwingIsValid != true)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion

        private void UpdateRangeItems(double minSweep, double maxSweep)
        {
            Range0Item = new AFCItem(Frequency, 0, Channel, minSweep, maxSweep);
            Range1Item = new AFCItem(Frequency, 1, Channel, minSweep, maxSweep);
            Range2Item = new AFCItem(Frequency, 2, Channel, minSweep, maxSweep);
            Range3Item = new AFCItem(Frequency, 3, Channel, minSweep, maxSweep);
        }
    }

    internal class AFCProcessingInfo : TestSignalInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="rangeIndex"></param>
        public AFCProcessingInfo(TestAFCViewModel AFCViewModel, float freq, int rangeIndex)
            : base(freq)
        {
            this.AFCViewModel = AFCViewModel;
            RangeIndex = rangeIndex;            
            InitProcessingItems(afcViewModel.Rows);
            FindNext();
        }

        private TestAFCViewModel afcViewModel;

        /// <summary>
        /// 
        /// </summary>
        public TestAFCViewModel AFCViewModel
        {
            get { return afcViewModel; }
            private set 
            { 
                afcViewModel = value;                
            }
        }

        private int rangeIndex;
        /// <summary>
        /// Индекс диапазона
        /// </summary>
        public int RangeIndex
        {
            get { return rangeIndex; }
            private set
            {
                rangeIndex = value;                
            }
        }       

        #region NextItem
        private float nextFreq;
        /// <summary>
        /// Следующая частота для определения размаха
        /// </summary>
        public float NextFreq
        {
            get { return nextFreq; }
            private set { nextFreq = value; }
        }

        private int nextRangeIndex;
        /// <summary>
        /// Следующий индекс диапазона для определения размаха
        /// </summary>
        public int NextRangeIndex
        {
            get { return nextRangeIndex; }
            private set { nextRangeIndex = value; }
        }

        private bool hasNext;
        /// <summary>
        /// Признак наличия частот и диапазонов для дальнейшего определения размаха
        /// </summary>
        public bool HasNext
        {
            get { return hasNext; }
            private set { hasNext = value; }
        }

        private void FindNext()
        {
            HasNext = false;
            if (ProcessingItems.Count == 0)
                return;
            AFCItemsRow row = AFCViewModel.Rows.FirstOrDefault(r => r.Frequency == Frequency);
            if (row == null)
                return;
            var item = row.ItemsList.FirstOrDefault(i => i.RangeIndex == RangeIndex);
            if (item == null)
                return;
            int itemIndex = row.ItemsList.IndexOf(item);
            if (itemIndex < row.ItemsList.Count - 1)
            {
                NextFreq = Frequency;
                NextRangeIndex = row.ItemsList[itemIndex + 1].RangeIndex;
                HasNext = true;                
            }
            else
            {
                int rowIndex = AFCViewModel.Rows.IndexOf(row);
                for (int i = rowIndex + 1; i < AFCViewModel.Rows.Count; i++)
                {
                    if (AFCViewModel.Rows[i].Frequency != Frequency)
                    {
                        NextFreq = AFCViewModel.Rows[i].Frequency;
                        NextRangeIndex = 0;
                        HasNext = true; 
                        break;
                    }
                }
            }
        }
        #endregion        

        /// <summary>
        /// 
        /// </summary>
        public bool SwingIsValid
        {
            get
            {
                foreach (var item in ProcessingItems)
                {
                    if (item.SwingIsValid != true)
                    {
                        return false;
                    }
                }
                return true; 
            }
        }

        private List<AFCItem> processingItems = new List<AFCItem>();

        /// <summary>
        /// Список 
        /// </summary>
        public List<AFCItem> ProcessingItems
        {
            get { return processingItems; }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rows"></param>
        private void InitProcessingItems(IEnumerable<AFCItemsRow> rows)
        {
            ClearProcessingItems();            
            foreach (var row in rows)
            {
                if (row.Frequency == Frequency)
                {
                    foreach (var item in row.ItemsList)
                    {
                        if (item.RangeIndex == RangeIndex)
                        {
                            ProcessingItems.Add(item);
                        }
                    }
                }
            }
            foreach (var item in ProcessingItems)
            {
                item.IsProcessing = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClearProcessingItems()
        {
            foreach (var item in ProcessingItems)
            {
                item.IsProcessing = false;
            }
            ProcessingItems.Clear();
        }
        

        /// <summary>
        /// 
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            ClearProcessingItems();
        }
    }

    internal class TestSignalInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="freq"></param>
        public TestSignalInfo(float freq)
        {
            Frequency = freq;
        }

        private float frequency;
        /// <summary>
        /// Частота
        /// </summary>
        public float Frequency
        {
            get { return frequency; }
            protected set
            {
                frequency = value;
                float periodTime = 1 / Frequency; //Время одного периода (с)
                float averageTime = 2 * periodTime; //Время, необходимое для усреднения (минимум два периода)
                if (averageTime < 0.1f)
                {
                    averageTime = 0.1f;
                }
                SamplesLimit = Convert.ToInt32(averageTime * 100000/*Частота квантования*/);
            }
        }

        private uint startDelay = 1000;

        /// <summary>
        /// Задержка старта чтения данных для определения размаха (мс)
        /// </summary>
        public uint StartDelay
        {
            get { return startDelay; }
            set { startDelay = value; }
        }

        private uint timeout = 20000;

        /// <summary>
        /// Таймаут определения размаха (мс)
        /// </summary>
        public uint Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        private int samplesLimit = -1;
        /// <summary>
        /// Количество отсчетов для усреднения
        /// </summary>
        public int SamplesLimit
        {
            get { return samplesLimit; }
            private set { samplesLimit = value; }
        }

        private long timeLimit = -1;
        /// <summary>
        /// Время (в тиках) окончания обработки по таймауту
        /// </summary>
        public long TimeLimit
        {
            get { return timeLimit; }
            private set { timeLimit = value; }
        }

        private bool started = false;

        /// <summary>
        /// 
        /// </summary>
        public bool Started
        {
            get { return started; }
            private set { started = value; }
        }

        private long startTimeTicks;

        /// <summary>
        /// Время начала анализа данных
        /// </summary>
        public long StartTimeTicks
        {
            get { return startTimeTicks; }
            protected set { startTimeTicks = value; }
        }

        /// <summary>
        /// Признак превышения таймаута
        /// </summary>
        public bool Timeouted
        {
            get
            {
                return DateTime.UtcNow.Ticks > TimeLimit;
            }
        }

        private bool delayIsOut = false;
        /// <summary>
        /// Признак превышения таймаута
        /// </summary>
        public bool DelayIsOut
        {
            get
            {
                if (!delayIsOut)
                {
                    delayIsOut = DateTime.UtcNow.Ticks > StartTimeTicks;
                }
                return delayIsOut;
            }
        }

        /// <summary>
        /// Начало определения размаха
        /// </summary>
        public virtual void Start()
        {
            if (started)
                return;
            Started = true;
            delayIsOut = false;
            long now = DateTime.UtcNow.Ticks;
            long delay = TimeSpan.FromMilliseconds(StartDelay).Ticks;
            long timeout = TimeSpan.FromMilliseconds(Timeout).Ticks;
            StartTimeTicks = now + delay;
            TimeLimit = StartTimeTicks + timeout;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Stop()
        {
            Started = false;            
        }
    }
}
