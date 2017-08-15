using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using System.Collections.ObjectModel;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    public class TestInputRangesViewModel : TestInputRangesViewModelBase
    {
        public TestInputRangesViewModel(ScriptEnvironment environment, NeuroMEPMicro device, NeuroMEPMicroAmplifierState deviceAmplifierState)
            : base (environment, device/*, deviceAmplifierState*/)
        {
            SetDeviceInfo(device, deviceAmplifierState);
            currentTestInfo = new TestSignalInfo(/*environment,*/ deviceAmplifierState.SampleFrequencyVP, 300.0);
            InputRangeTests = new ObservableCollection<TestInputRangeInfo>();
        }

        #region Properties

        //private ObservableCollection<TestInputRangeInfo> inputRangeTests;
        ///// <summary>
        ///// Массив с информацией о текущих тестах
        ///// </summary>
        //public ObservableCollection<TestInputRangeInfo> InputRangeTests
        //{
        //    get { return inputRangeTests; }
        //    set { inputRangeTests = value; }
        //}

        /// <summary>
        /// Включить фильтр сетевой помехи
        /// </summary>
        public override bool EnablePowerRejector
        {
            get { return enablePowerRejector; }
            set
            {
                if (enablePowerRejector != value)
                {
                    enablePowerRejector = value;
                    (Device as NeuroMEPMicro).SetFiltersRe(value, false);
                    for (int i = 0; i < (Device as NeuroMEPMicro).Filters.Length; i++)
                    {
                        for (int j = 0; j < (Device as NeuroMEPMicro).Filters[i].Length; j++)
                        {
                            if (((Device as NeuroMEPMicro).Filters[i][j] is NeuroSoft.MathLib.Filters.IIRFilterRejector))
                                (Device as NeuroMEPMicro).Filters[i][j].Active = enablePowerRejector;
                            else
                            {
                                //(Device as NeuroMEPMicro).Filters[i][j].Active = FiltersInfo[i].IsEnabled;
                            }
                        }
                    }
                    OnPropertyChanged("EnablePowerRejector");
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
                if (Device == null)
                {
                    return 0;
                }
                return ((Device as NeuroMEPMicro).GetDeviceState().GetState(typeof(NeuroMEPMicroAmplifierState)) as NeuroMEPMicroAmplifierState).SampleFrequencyVP;
            }
            set
            {
                (Device as NeuroMEPMicro).SetSampleFrequencyVP(value);
            }
        }

        //private bool isTesting;
        ///// <summary>
        ///// Признак обработки данных
        ///// </summary>
        //public bool IsTesting
        //{
        //    get { return isTesting; }
        //    private set
        //    {
        //        if (isTesting != value)
        //        {
        //            isTesting = value;
        //            OnPropertyChanged("IsTesting");
        //        }
        //    }
        //}

        //private bool? testingResult;
        ///// <summary>
        ///// Признак успешного проведения тестирования
        ///// </summary>
        //public bool? TestingResult
        //{
        //    get { return testingResult; }
        //    private set 
        //    {
        //        testingResult = value;
        //        OnPropertyChanged("TestingResult");
        //    }
        //}
        ///// <summary>
        ///// Результат последнего проходимого теста
        ///// (Необходимо для подсветки строк зеленым цветом)
        ///// </summary>
        //public bool LastTestResult
        //{
        //    get { return InputRangeTests[currentTestNumber - 1].IsValidValues; }
        //}
        

        //private bool settingParams = false;
        ///// <summary>
        ///// Признак обработки данных
        ///// </summary>
        //public bool SettingParams
        //{
        //    get { return settingParams; }
        //    private set
        //    {
        //        if (settingParams != value)
        //        {
        //            settingParams = value;
        //            OnPropertyChanged("SettingParams");
        //        }
        //    }
        //}

        //private string testingStatus;
        ///// <summary>
        ///// Текстовое описание состояния обработки
        ///// </summary>
        //public string TestingStatus
        //{
        //    get { return testingStatus; }
        //    set
        //    {
        //        if (testingStatus != value)
        //        {
        //            testingStatus = value;
        //            OnPropertyChanged("TestingStatus");
        //        }
        //    }
        //}

        //private bool testWasLaunched;
        ///// <summary>
        ///// Признак, что тест был запущен, но завершился неуспешно
        ///// </summary>
        //public bool TestWasLaunched
        //{
        //    get { return testWasLaunched; }
        //}

        ///// <summary>
        ///// информация о текущем тесте (время до таймаута и проч.)
        ///// </summary>
        //private TestSignalInfo currentTestInfo;
        ///// <summary>
        ///// Номер текущего теста
        ///// </summary>
        //private int currentTestNumber = 0;
        //public int CurrentTestNumber
        //{ 
        //    get { return currentTestNumber; }
        //}
        
        #endregion

        #region Methods

        /// <summary>
        /// Запуск чтения данных
        /// </summary>
        public override void StartReadData()
        {
            //if (!testWasLaunched)
            //    currentTestNumber = 0;
            //TestingResult = null;
            //testWasLaunched = true;
            //DataMonitoringPlotter.SelectedXScaleItem = DataMonitoringPlotter.XScaleItems[13];
            //StartNewTest();
            
            // Отключаем программные фильтры в классе NeuroMEPMicro и устанавливаем частоту среза ФНЧ на максимум
            for (int i = 0; i < (device as NeuroMEPMicro).Filters.Length; i++)
            {
                for (int j = 0; j < (device as NeuroMEPMicro).Filters[i].Length; j++)
                    if ((device as NeuroMEPMicro).Filters[i][j].Active)
                        (device as NeuroMEPMicro).Filters[i][j].Active = false;
            }
            for (int i = 0; i < ChannelsCount; i++)
            {
                (device as NeuroMEPMicro).SetFilterHiVP(i, 10000.0f);
                (device as NeuroMEPMicro).SetFilterLoVP(i, 0.5f);
            }
            base.StartReadData();
        }

        /// <summary>
        /// Установка начальных значений
        /// </summary>
        /// <param name="device"></param>
        /// <param name="deviceAmplifierState"></param>
        protected void SetDeviceInfo(NeuroMEPMicro device, NeuroMEPMicroAmplifierState deviceAmplifierState)
        {
            //MepDevice = device;
            //DeviceAmplifierState = deviceAmplifierState;
            base.SetDeviceInfo();
            for (int i = 0; i < ChannelsCount; i++)
            {
                string name = i.ToString();
                StatisticsCollection.Add(new DataStatistics(name));
                ChannelsInfo.Add(new ChannelInfo(i, Device as NeuroMEPMicro, deviceAmplifierState));
            }
        }

        ///// <summary>
        ///// Обработчик события получения данных с прибора
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //protected override void DeviceReceiveDataHandler(object sender, ReceiveDataArgs e)
        //{
        //    base.DeviceReceiveDataHandler(sender, e);
        //    TestExecution();
        //}

        ///// <summary>
        ///// Запускает новый тест
        ///// </summary>
        //private void StartNewTest()
        //{
        //    currentTestInfo.Start();
        //    float signalAmpl = 0.0002f;
        //    int inputGainRange;
        //    DataMonitoringPlotter.SelectedYScaleItem = DataMonitoringPlotter.YScaleItems[9];

        //    if (currentTestNumber == 0)
        //        signalAmpl *= 0.8f;
        //    if (currentTestNumber > 2 && currentTestNumber <= 6)
        //    {
        //        signalAmpl = 0.001f;
        //        DataMonitoringPlotter.SelectedYScaleItem = DataMonitoringPlotter.YScaleItems[11];
        //        if (currentTestNumber == 3)
        //            signalAmpl *= 0.9f;
        //    }
        //    if (currentTestNumber > 6 && currentTestNumber <= 10)
        //    {
        //        signalAmpl = 0.01f;
        //        DataMonitoringPlotter.SelectedYScaleItem = DataMonitoringPlotter.YScaleItems[14];
        //        if (currentTestNumber == 7)
        //            signalAmpl *= 0.9f;
        //    }
        //    if (currentTestNumber > 10)
        //    {
        //        signalAmpl = (float)(0.1 * 0.9);
        //        DataMonitoringPlotter.SelectedYScaleItem = DataMonitoringPlotter.YScaleItems[17];
        //    }

        //    if (currentTestNumber < 3)
        //        inputGainRange = currentTestNumber;
        //    else
        //        if (currentTestNumber < 7)
        //            inputGainRange = currentTestNumber - 1;
        //        else
        //            if (currentTestNumber < 11)
        //                inputGainRange = currentTestNumber - 2;
        //            else
        //                inputGainRange = currentTestNumber - 3;

        //    InputRangeTests.Add(new TestInputRangeInfo(ChannelsInfo[0].ChannelSupportedRanges[inputGainRange], Math.Round(signalAmpl / 4, 6), Math.Round((signalAmpl) * 0.95, 6), Math.Round((signalAmpl) * 1.05, 6)));
        //    InputRangeTests[currentTestNumber].SetHardwareParams(this, inputGainRange);
        //    ResetDataStatistics();
        //    TestingStatus = string.Format(Properties.Resources.TestInputRangeStart, InputRangeTests[currentTestNumber].SwingString, InputRangeTests[currentTestNumber].Range);
        //    currentTestInfo.Start();
        //    IsTesting = true;
        //}

        ///// <summary>
        ///// Проверяет, нормально ли там все померялось
        ///// </summary>
        //private void TestExecution()
        //{
        //    if (currentTestInfo == null)
        //        return;
        //    lock (currentTestInfo)
        //    {
        //        // Проверить корректность поступающих данных
        //        if (CheckDataCorrect())
        //        {
        //            // Если таймеры не запущены, то запустить их
        //            if (!currentTestInfo.Started)
        //                currentTestInfo.Start();
        //            if (currentTestInfo.Timeouted) //заканчиваем определение размаха по таймауту
        //            {
        //                EndTestExecution();
        //            }
        //            else if (samples >= currentTestInfo.SamplesLimit)
        //            {
        //                if (UpdateCurrentTestInfo() && currentTestInfo.DelayIsOut)
        //                {
        //                    EndTestExecution();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // Если поступающие данные некорректны, то остановить задержки, если они были запущены.
        //            if (currentTestInfo.Started)
        //                currentTestInfo.Stop();
        //        }

        //    }
        //}

        ///// <summary>
        ///// Метод проводит оценку поступающих данных (отличен размах от нуля или нет)
        ///// </summary>
        ///// <returns> Результат проверки </returns>
        //private bool CheckDataCorrect()
        //{
        //    for (int i = 0; i < ChannelsCount; i++)
        //    {
        //        if (maxSamples[i] < float.MaxValue && minSamples[i] > float.MinValue)
        //        {
        //            if (maxSamples[i] - minSamples[i] == 0)
        //                return false;
        //        }
        //        else
        //            return false;
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// Метод обновлния значений размаха сигнала
        ///// </summary>
        ///// <returns> Данные соответствуют требованиям </returns>
        //private bool UpdateCurrentTestInfo()
        //{
        //    if (currentTestInfo == null)
        //        return false;
        //    bool hasInvalid = false;
        //    InputRangeTests[currentTestNumber].ChannelsSwings[0].Value = maxSamples[0] - minSamples[0];
        //    InputRangeTests[currentTestNumber].ChannelsSwings[1].Value = maxSamples[1] - minSamples[1];
        //    hasInvalid = !(InputRangeTests[currentTestNumber].ChannelsSwings[0].IsValidValue && InputRangeTests[currentTestNumber].ChannelsSwings[1].IsValidValue);
        //    InputRangeTests[currentTestNumber].IsValidValues = !hasInvalid;
        //    ResetDataStatistics();
        //    return !hasInvalid;
        //}

        ///// <summary>
        ///// Заврешение выполнения текущего теста
        ///// </summary>
        //private void EndTestExecution()
        //{
        //    if (currentTestNumber < 11 && !currentTestInfo.Timeouted)
        //    {
        //        currentTestNumber++;
        //        StartNewTest();
        //    }
        //    else
        //    {
        //        StopTesting();
        //    }
        //    currentTestInfo.Stop();
        //}

        //private void StopTesting(bool wasStopped = false)
        //{
        //    IsTesting = false;
        //    if (wasStopped)
        //    {
        //        TestingStatus = "Проверка принудительно остановлена.";
        //    }
        //    else
        //    {
        //        TestingResult = InputRangeTests[currentTestNumber].IsValidValues;
        //        TestingStatus = String.Format(Properties.Resources.TestInputRangeStop, TestingResult == true ? "успешно" : "с ошибками");
        //        if (TestingResult.Value && currentTestNumber == 11)
        //            testWasLaunched = false;
        //    }
        //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        //    {
        //        StopReadData();
        //    }));
        //}
        ///// <summary>
        ///// Остановка процесса тестирования
        ///// </summary>
        ///// <param name="wasStopped"> Флаг того, что тест был остановлен принудительно </param>
        //public void StopTestProcess(bool wasStopped = false)
        //{
        //    StopTesting(wasStopped);
        //    currentTestInfo.Stop();
        //}

        #endregion
    }

    

    public class TestSignalInfo
    {
        public TestSignalInfo(/*ScriptEnvironment environment,*/ double sampleFreq, double signalFreq)
        {
            this.sampleFreq = sampleFreq;
            SignalFreq = signalFreq;
            //this.environment = environment;
        }

        /// <summary>
        /// Частота дискретизации сигнала
        /// </summary>
        private double sampleFreq;

        //private ScriptEnvironment environment;

        /// <summary>
        /// Частота сигнала
        /// </summary>
        private double signalFreq;

        public double SignalFreq
        {
            get { return signalFreq; }
            protected set
            {
                signalFreq = value;
                double periodTime = 1 / signalFreq;    //Время одного периода (с)
                double averageTime = 2 * periodTime;   //Время, необходимое для усреднения (минимум два периода)
                if (averageTime < 0.1f)
                {
                    averageTime = 0.1f;
                }
                SamplesLimit = Convert.ToInt32(averageTime * sampleFreq);
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

        private bool started = false;

        /// <summary>
        /// Признак выполнения тестирования
        /// </summary>
        public bool Started
        {
            get { return started; }
            private set { started = value; }
        }

        private long startDelay = 5000;
        /// <summary>
        /// Задержка в процессе тестирования для отображения результатов измерения
        /// </summary>
        public long StartDelay 
        {
            get { return startDelay; }
        }

        private uint timeout = 25000;

        /// <summary>
        /// Таймаут определения размаха (мс)
        /// </summary>
        public uint Timeout
        {
            get { return timeout; }
            set { timeout = value; }
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

        private bool delayIsOut;

        public bool DelayIsOut 
        {
            get
            {
                if (Started == true)
                    return DateTime.UtcNow.Ticks > (TimeLimit - TimeSpan.FromMilliseconds(Timeout).Ticks);
                return delayIsOut;
            }
            private set { delayIsOut = value; } 
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

        /// <summary>
        /// Начало определения размаха
        /// </summary>
        public virtual void Start()
        {
            if (started)
                return;
            Started = true;
            DelayIsOut = false;
            long timeNow = DateTime.UtcNow.Ticks;
            long delay = TimeSpan.FromMilliseconds(StartDelay).Ticks;
            long timeout = TimeSpan.FromMilliseconds(Timeout).Ticks;
            TimeLimit = timeNow + delay + timeout;
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
