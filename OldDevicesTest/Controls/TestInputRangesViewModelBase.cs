using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.Devices;
using System.ComponentModel;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    public class TestInputRangesViewModelBase : ReadDataViewModelBase, IDisposable
    {
        public TestInputRangesViewModelBase(ScriptEnvironment environment, NSDevice device)
            : base(environment, device)
        {
            ChannelsCount = device.GetChannelsCount();
            
            maxSampSum = new float[channelsCount];
            minSampSum = new float[channelsCount];
            for (int i = 0; i < channelsCount; i++)
                maxSampSum[i] = minSampSum[i] = 0;
        }

        #region Свойства

        //private Visibility[] channelsVisibility = new Visibility[4];
        ///// <summary>
        ///// Количество тестируемых каналов
        ///// </summary>
        //public Visibility[] ChannelsVisibility
        //{
        //    get { return channelsVisibility; }
        //    private set
        //    {
        //        channelsVisibility = value;
        //        OnPropertyChanged("ChannelsVisibility");
        //    }
        //}

        private int channelsCount;
        /// <summary>
        /// Число каналов в устройстве
        /// </summary>
        public override int ChannelsCount
        {
            //get { return channelsCount; }
            protected set 
            { 
                channelsCount = value;
                OnPropertyChanged("ChannelsCount");
            }
        }
        

        protected ObservableCollection<TestInputRangeInfo> inputRangeTests;
        /// <summary>
        /// Массив с информацией о текущих тестах
        /// </summary>
        public ObservableCollection<TestInputRangeInfo> InputRangeTests
        {
            get { return inputRangeTests; }
            set { inputRangeTests = value; }
        }

        //private int channelsNumber;
        ///// <summary>
        ///// Число каналов
        ///// </summary>
        //public int ChannelsNumber
        //{
        //    get { return channelsNumber; }
        //    set
        //    {
        //        channelsNumber = value;
        //        OnPropertyChanged("ChannelsNumber");
        //    }
        //}
        

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

        private bool? testingResult;
        /// <summary>
        /// Признак успешного проведения тестирования
        /// </summary>
        public bool? TestingResult
        {
            get { return testingResult; }
            private set
            {
                testingResult = value;
                OnPropertyChanged("TestingResult");
            }
        }

        protected bool settingParams = false;
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

        protected string testingStatus;
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

        private bool testWasLaunched;
        /// <summary>
        /// Признак, что тест был запущен, но завершился неуспешно
        /// </summary>
        public bool TestWasLaunched
        {
            get { return testWasLaunched; }
        }

        /// <summary>
        /// информация о текущем тесте (время до таймаута и проч.)
        /// </summary>
        protected TestSignalInfo currentTestInfo;
        /// <summary>
        /// Номер текущего теста
        /// </summary>
        protected int currentTestNumber = 0;
        public int CurrentTestNumber
        {
            get { return currentTestNumber; }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Обработчик события получения данных с прибора
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void DeviceReceiveDataHandler(object sender, ReceiveDataArgs e)
        {
            base.DeviceReceiveDataHandler(sender, e);
            TestExecution();
        }

        public override float SamplesRateInSamplesPerSecond
        {
            get
            {
                return (device.GetDeviceState().GetState(typeof(NeuroMEPMicroAmplifierState))as NeuroMEPMicroAmplifierState).SampleFrequencyVP ;
            }
        }

        private int sampsPerPeriod = 0; // число отсчетов за период сигнала
        private float[] maxSampSum;     // сумма максимумов сигнала
        private float[] minSampSum;     // сумма минимумов сигнала

        /// <summary>
        /// Обработка статистики
        /// </summary>
        /// <param name="e"></param>
        protected override void AppendDataStatistics(ReceiveDataArgs e)
        {
            int maxIndex = 0;
            int minIndex = 0;
            int periodsQuant = 0, sampsRest = 0; // кол-во периодов и кол-во оставшихся отсчетов, не попавших в целое число периодов
            
            //Размах вычисляется на трех периодах сигнала и усредняется на промежутке между обновлениями данных
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (e.Data[i] != null)
                {
                    ArraySegment<float> sampsWholePeriods, sampThreePeriods;
                    //maxSum = 0;
                    //minSum = 0;
                    periodsQuant = e.Data[i].Length / (sampsPerPeriod * 3); // определить число периодов
                    sampsRest = e.Data[i].Length % (sampsPerPeriod * 3);    // определить число отсчетов, не попавших в целое число периодов
                    sampsWholePeriods = new ArraySegment<float>(e.Data[i], 0, (int)(e.Data[i].Length - (sampsRest != 0 ? sampsRest : 0))); // выбрать отсчеты, попадающие в целое число периодов

                    // Определить максимум и минимум на трех периодах сигнала и сложить их
                    for (int j = 0; j < periodsQuant; j++)
                    {
                        sampThreePeriods = new ArraySegment<float>(sampsWholePeriods.Array, j * sampsPerPeriod * 3, sampsPerPeriod * 3); // выбрать отсчеты, соответствующие трем периодам сигнала
                        List<float> iSamples = new List<float>(sampThreePeriods.Array);
                        NeuroSoft.MathLib.Basic.CalcMinMax(iSamples, out minIndex, out maxIndex);
                        maxSampSum[i] += e.Data[i][maxIndex];
                        minSampSum[i] += e.Data[i][minIndex];
                    }
                    
                    if (i == 0)
                         samples += sampsWholePeriods.Count;

                    // При количестве отсчетов, соответствующем обновлению данных, вычислить средние значения для максимума и минимума сигнала
                    if (samples >= currentTestInfo.SamplesLimit)
                    {
                        maxSamples[i] = maxSampSum[i] / (samples / sampsPerPeriod / 3);
                        minSamples[i] = minSampSum[i] / (samples / sampsPerPeriod / 3);
                    }
                }
            }
        }

        public void Dispose()
        {
            Device.ReceiveData -= new ReceiveDataDelegate(DeviceReceiveDataHandler);
        }

        /// <summary>
        /// Запуск чтения данных
        /// </summary>
        public override void StartReadData()
        {
            Device.ReceiveData -= new ReceiveDataDelegate(DeviceReceiveDataHandler);
            Device.ReceiveData += new ReceiveDataDelegate(DeviceReceiveDataHandler);
            if (!testWasLaunched)
                currentTestNumber = 0;
            TestingResult = null;
            testWasLaunched = true;
            sampsPerPeriod = (int)(SamplesRateInSamplesPerSecond / currentTestInfo.SignalFreq);
            DataMonitoringPlotter.SelectedXScaleItem = DataMonitoringPlotter.XScaleItems[13];
            StartNewTest();
            base.StartReadData();
        }

        /// <summary>
        /// Запускает новый тест
        /// </summary>
        protected virtual void StartNewTest()
        {
            //currentTestInfo.Start();
            float signalAmpl = 0.0002f;
            int inputGainRange;
            DataMonitoringPlotter.SelectedYScaleItem = DataMonitoringPlotter.YScaleItems[9];

            if (currentTestNumber == 0)
                signalAmpl *= 0.8f;
            if (currentTestNumber > 2 && currentTestNumber <= 6)
            {
                signalAmpl = 0.001f;
                DataMonitoringPlotter.SelectedYScaleItem = DataMonitoringPlotter.YScaleItems[11];
                //if (currentTestNumber == 3)
                //    signalAmpl *= 0.9f;
            }
            if (currentTestNumber > 6 && currentTestNumber <= 10)
            {
                signalAmpl = 0.01f;
                DataMonitoringPlotter.SelectedYScaleItem = DataMonitoringPlotter.YScaleItems[14];
                //if (currentTestNumber == 7)
                //    signalAmpl *= 0.9f;
            }
            if (currentTestNumber > 10)
            {
                signalAmpl = (float)(0.1 * 0.9);
                DataMonitoringPlotter.SelectedYScaleItem = DataMonitoringPlotter.YScaleItems[17];
            }

            if (currentTestNumber < 3)
                inputGainRange = currentTestNumber;
            else
                if (currentTestNumber < 7)
                    inputGainRange = currentTestNumber - 1;
                else
                    if (currentTestNumber < 11)
                        inputGainRange = currentTestNumber - 2;
                    else
                        inputGainRange = currentTestNumber - 3;

            if (Device is NeuroMEPMicro)
                InputRangeTests.Add(new TestInputRangeInfo(ChannelsInfo[0].ChannelSupportedRanges[inputGainRange],
                                    Math.Round(signalAmpl / 4, 6), Math.Round((signalAmpl) * 0.95, 6), Math.Round((signalAmpl) * 1.05, 6)));
            else
            {
                if (Device is EEG5Device || (Device is EEG4Device && (EEG4Scripts.GetDeviceType(Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.NS_4P
                                                                      || EEG4Scripts.GetDeviceType(Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.NS_4EP
                                                                      || EEG4Scripts.GetDeviceType(Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4)))
                    InputRangeTests.Add(new TestInputRangeInfo(ChannelsInfo[0].ChannelSupportedRanges[inputGainRange],
                                        Math.Round(signalAmpl / 4, 6), Math.Round((signalAmpl) * 0.95, 6), Math.Round((signalAmpl) * 1.05, 6), 4));
                else
                    InputRangeTests.Add(new TestInputRangeInfo(ChannelsInfo[0].ChannelSupportedRanges[inputGainRange],
                                        Math.Round(signalAmpl / 4, 6), Math.Round((signalAmpl) * 0.95, 6), Math.Round((signalAmpl) * 1.05, 6), 1));
            }
            InputRangeTests[currentTestNumber].SetHardwareParams(this, inputGainRange);
            ResetDataStatistics();
            TestingStatus = string.Format(Properties.Resources.TestInputRangeStart, InputRangeTests[currentTestNumber].SwingString, InputRangeTests[currentTestNumber].Range);
            //currentTestInfo.Start();
            IsTesting = true;
        }

        /// <summary>
        /// Проверяет, нормально ли там все померялось
        /// </summary>
        private void TestExecution()
        {
            if (currentTestInfo == null)
                return;
            lock (currentTestInfo)
            {
                // Проверить корректность поступающих данных
                if (CheckDataCorrect())
                {
                    // Если таймеры не запущены, то запустить их
                    if (!currentTestInfo.Started)
                        currentTestInfo.Start();
                    if (currentTestInfo.Timeouted) //заканчиваем определение размаха по таймауту
                    {
                        EndTestExecution();
                    }
                    else if (samples >= currentTestInfo.SamplesLimit)
                    {
                        if (UpdateCurrentTestInfo() && currentTestInfo.DelayIsOut)
                        {
                            EndTestExecution();
                        }
                    }
                }
                else
                {
                    // Если поступающие данные некорректны, то остановить задержки, если они были запущены.
                    if (currentTestInfo.Started)
                        currentTestInfo.Stop();
                }

            }
        }

        /// <summary>
        /// Метод проводит оценку поступающих данных (отличен размах от нуля или нет)
        /// </summary>
        /// <returns> Результат проверки </returns>
        private bool CheckDataCorrect()
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (maxSamples[i] < float.MaxValue && minSamples[i] > float.MinValue)
                {
                    if (maxSamples[i] - minSamples[i] == 0)
                        return false;
                }
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Метод обновлния значений размаха сигнала
        /// </summary>
        /// <returns> Данные соответствуют требованиям </returns>
        private bool UpdateCurrentTestInfo()
        {
            if (currentTestInfo == null)
                return false;
            bool hasInvalid = false;
            for (int i = 0; i < ChannelsCount; i++)
            {
                InputRangeTests[currentTestNumber].ChannelsSwings[i].Value = maxSamples[i] - minSamples[i]; // определить размах сигнала
                hasInvalid |= !InputRangeTests[currentTestNumber].ChannelsSwings[i].IsValidValue;

                if (samples >= currentTestInfo.SamplesLimit)
                    maxSampSum[i] = minSampSum[i] = 0; // обнулить суммы максимумов и минимумов
            }
            
            //InputRangeTests[currentTestNumber].ChannelsSwings[1].Value = maxSamples[1] - minSamples[1];
            //hasInvalid = !(InputRangeTests[currentTestNumber].ChannelsSwings[0].IsValidValue && InputRangeTests[currentTestNumber].ChannelsSwings[1].IsValidValue);
            InputRangeTests[currentTestNumber].IsValidValues = !hasInvalid;
            ResetDataStatistics();
            return !hasInvalid;
        }

        /// <summary>
        /// Заврешение выполнения текущего теста
        /// </summary>
        private void EndTestExecution()
        {
            if (currentTestNumber < 11 && !currentTestInfo.Timeouted)
            {
                currentTestNumber++;
                StartNewTest();
            }
            else
            {
                StopTesting();
            }
            currentTestInfo.Stop();
        }

        /// <summary>
        /// Останавливает тестирование
        /// </summary>
        /// <param name="wasStopped"> Признак принудительной остановки тестирования </param>
        private void StopTesting(bool wasStopped = false)
        {
            IsTesting = false;
            if (wasStopped)
            {
                TestingStatus = Properties.Resource1.TestWasStoppedForcibly;
            }
            else
            {
                TestingResult = InputRangeTests[currentTestNumber].IsValidValues;
                TestingStatus = String.Format(Properties.Resources.TestInputRangeStop, TestingResult == true ? Properties.Resource1.Successful : Properties.Resource1.WithMistakes);
                if (TestingResult.Value && currentTestNumber == 11)
                    testWasLaunched = false;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                StopReadData();
            }));
        }
        /// <summary>
        /// Остановка процесса тестирования
        /// </summary>
        /// <param name="wasStopped"> Флаг того, что тест был остановлен принудительно </param>
        public void StopTestProcess(bool wasStopped = false)
        {
            StopTesting(wasStopped);
            currentTestInfo.Stop();
        }

        #endregion
    }

    /// <summary>
    /// Класс с информацией о тестируемом диапазоне
    /// </summary>
    public class TestInputRangeInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="range"></param>
        /// <param name="swing"></param>
        /// <param name="minSwing"></param>
        /// <param name="maxSwing"></param>
        /// <param name="chanCount"></param>
        public TestInputRangeInfo(float range, double swing, double minSwing, double maxSwing, int chanCount = 2)
        {
            this.Range = range * 1e3 > 1 ? (Math.Round(range * 1e3)).ToString() + " мВ" : (Math.Round(range * 1e6)).ToString() + " мкВ";
            this.Swing = swing;
            //for (int i = 0; i < 4; i++)
            //{
            //    if (i < chanCount)
            //        ChannelsVisibility[i] = Visibility.Visible;
            //    else
            //        ChannelsVisibility[i] = Visibility.Collapsed;
            //}
            ChannelsSwings = new RangedValue<double>[chanCount];
            for (int i = 0; i < chanCount; i++)
            {
                this.ChannelsSwings[i] = new RangedValue<double>(0, minSwing, maxSwing);
                //this.ChannelTwoSwing = new RangedValue<double>(0, minSwing, maxSwing);
            }
        }

        private string range;
        /// <summary>
        /// Диапазон усиления
        /// </summary>
        public string Range
        {
            get { return range; }
            set
            {
                range = value;
                OnPropertyChanged("Range");
            }
        }

        private double swing;
        /// <summary>
        /// Размах подаваемого сигнала в мВ
        /// </summary>
        public double Swing
        {
            get { return swing; }
            set
            {
                swing = value;
                OnPropertyChanged("Swing");
            }
        }

        public string SwingString
        {
            get { return swing * 4 * 1e3 >= 1 ? (Math.Round(swing * 4 * 1e3)).ToString() + " мВ" : (Math.Round(swing * 4 * 1e6)).ToString() + " мкВ"; }
        }

        private RangedValue<double>[] channelsSwings;
        /// <summary>
        /// Размахи сигналов в каналах
        /// </summary>
        public RangedValue<double>[] ChannelsSwings
        {
            get { return channelsSwings; }
            set
            {
                channelsSwings = value;
                OnPropertyChanged("ChannelOneSwing");
            }
        }

        //private Visibility[] channelsVisibility = new Visibility[4];
        ///// <summary>
        ///// Количество тестируемых каналов
        ///// </summary>
        //public Visibility[] ChannelsVisibility 
        //{
        //    get { return channelsVisibility; }
        //    private set
        //    {
        //        channelsVisibility = value;
        //        OnPropertyChanged("ChannelsVisibility");
        //    }
        //}

        //private RangedValue<double> channelTwoSwings;
        ///// <summary>
        ///// Размахи сигналов в каналах
        ///// </summary>
        //public RangedValue<double> ChannelTwoSwing
        //{
        //    get { return channelTwoSwings; }
        //    set 
        //    { 
        //        channelTwoSwings = value;
        //        OnPropertyChanged("ChannelTwoSwing");
        //    }
        //}

        private bool isValidValues;
        /// <summary>
        /// Флаг, что результирующие значения сигнала размаха верные
        /// </summary>
        public bool IsValidValues
        {
            get { return isValidValues; }
            internal set
            {
                isValidValues = value;
                OnPropertyChanged("IsValidValues");
            }
        }

        /// <summary>
        /// Установка параметров аппаратуры
        /// </summary>
        /// <param name="genAmpl"> Амплитуда сигнала генератора</param>
        /// <param name="gainRangeIndx"> Индекс диапазона усиления </param>
        /// <param name="viewModel"> Модель представления чтения данных </param>
        public virtual void SetHardwareParams(ReadDataViewModelBase viewModel, int gainRangeIndx, float signalFreq = 300.0f)
        {
            StandOperations.SetGeneratorState(viewModel.Environment, (float)swing, signalFreq);

            if (viewModel.Device is NeuroMEPMicro)
                StandOperations.SetDifferentialSignalCommutatorStateMEPMicro(viewModel.Environment);
            else
            {
                if (viewModel.Device is EEG5Device || (viewModel.Device is EEG4Device && (EEG4Scripts.GetDeviceType(viewModel.Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.NS_4P
                                                                                          || EEG4Scripts.GetDeviceType(viewModel.Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.NS_4EP
                                                                                          || EEG4Scripts.GetDeviceType(viewModel.Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4)))
                    StandOperationsEEG.SetDifferetialVpSignal(viewModel.Environment);
                else
                    StandOperationsEEG.EEG4SetDifferetialVpSignal(viewModel.Environment);
            }
            var device = (viewModel.Environment.Device as NSDevice);
            for (int i = 0; i < device.GetChannelsCount(); i++)
            {
                viewModel.ChannelsInfo[i].SelectedChannelRange = viewModel.ChannelsInfo[i].ChannelSupportedRanges[gainRangeIndx];
            }
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
}
