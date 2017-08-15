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
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices.Bluetooth;
using InTheHand.Net.Sockets;
using System.Timers;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Scripts;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Controls
{
    /// <summary>
    /// Логика взаимодействия для MonitoringData.xaml
    /// </summary>
    public partial class MonitoringData : UserControl, INotifyPropertyChanged, IDisposable
    {
        ScriptEnvironment environment;
        /// <summary>
        // класс для Bluetooth устройств
        /// </summary>
        public static BluetoothDevice Device8EXv2012;
        public static PolySpectrum8EX Device8EX;
        private static Timer aTimer;
        /// <summary>
        /// Количество каналов в устройстве
        /// </summary>
        static int ChannelsCount;
        /// <summary>
        /// данные для отображения
        /// </summary>
        public static float[][] channelData = new float[9][];
        /// <summary>
        /// состояние первых отсчетов в пакете данных
        /// </summary>
        public static float[] firstSampleState = new float[10];
        SamplesEventsCallbackArgs ECG_data = new SamplesEventsCallbackArgs();
        SamplesStimulsDataEventArgs ECG_data1 = new SamplesStimulsDataEventArgs();
        /// <summary>
        /// базовая частота квантования
        /// </summary>
        public static Int32 DataRate = 1000;
        /// <summary>
        /// делитель базовой частоты
        /// </summary>
        uint FreqDivider = 1;
        /// <summary>
        /// делитель базовой частоты для канала дыхания
        /// </summary>
        public static uint FreqDividerBr = 20;

        public MonitoringData(ScriptEnvironment environment, MonitoringType type)
        {
            InitializeComponent();
            DataContext = this;
            this.environment = environment;
            nameDevice.Content = SelectBTDevices.nameBTDevice;
            if (monitoringPlotter == null)
            {
                monitoringPlotter = new MonitoringPlotter();
            }
            // Create a timer with a 1 second interval.
            aTimer = new Timer(1000);
            aTimer.Enabled = false;
            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(aTimer_Elapsed);
            Device8EXv2012 = new BluetoothDevice();
            Device8EX = new PolySpectrum8EX();
            // отключаем программные фильтры
            Device8EX.RejectorMode = PSDeviceRejectorMode.None;
            Device8EX.LFMode = PSDeviceLFMode.None;
            SamplesRateInSamplesPerSecond = DataRate;
            SelectedFreqBr = FreqScaleBr[FreqScaleBr.Length - 1];
            InitPlotter();
            for (int i = 0; i < ChannelsCount; i++)
            {
//                channelData[i] = new float[65536];
                firstSampleState[i] = 0;
            }
            ProtocolPS8EXv2012.FilterBr();
            SetStatistic(type);
        }

        /// <summary>
        /// Обработка события Timer (1 сек)
        /// </summary>
        public void aTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            if (Device8EXv2012 != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    dataRateView.Content = ProtocolPS8EXv2012.ByteToSecond.ToString() + " b/s";
                    ProtocolPS8EXv2012.ByteToSecond = 0;
                }));  
            }
            if (monitoringPlotter != null)
            {
                if (!monitoringPlotter.Paused) UpdateStatistic();
            }
        }

        private void openCloseBTDevice_Click(object sender, RoutedEventArgs e)
        {
            if (SelectBTDevices.nameBTDevice != null)
            {
                try
                {
                    if (SelectBTDevices.nameBTDevice.IndexOf(PolySpectrum8EX_2012.DeviceNameSign) > -1)
                    {
                        if (!Device8EXv2012.IsOpened)
                        {
                            this.Cursor = Cursors.Wait;
                            Device8EXv2012.PinCode = "1234";
                            Device8EXv2012.Open(SelectBTDevices.devices[SelectBTDevices.numSelectedDevice]);
                            Device8EXv2012.DeviceData += new BluetoothDataHandler(ReadRecieveDataPS8EXv2012);
                            ProtocolPS8EXv2012.BeginTransmit();
                            aTimer.Enabled = true;
                            nameDevice.Foreground = Brushes.Green;
                            nameDevice.FontWeight = FontWeights.Bold;
                            monitoringPlotter.Height = 600;
                        }
                        else
                        {
                            ProtocolPS8EXv2012.StopTransmit();
                            Device8EXv2012.Close();
                            //Device8EXv2012 = null;
                            nameDevice.Foreground = Brushes.Black;
                            nameDevice.FontWeight = FontWeights.Normal;
                        }
                        openCloseBTDevice.Content = Device8EXv2012.IsOpened == false ? "OpenBTDevice" : "CloseBTDevice";
                        this.Cursor = Cursors.Arrow;
                        return;
                    }
                    if (SelectBTDevices.nameBTDevice.IndexOf(PolySpectrum8EX.DeviceNameSign) > -1)
                    {
                        if (!Device8EX.Active)
                        {
                            this.Cursor = Cursors.Wait;
                            // Определяем наличие прибора
                            string serialNo = PolySpectrum8EX.ExtractSerial(SelectBTDevices.nameBTDevice);
                            Device8EX.BTAddress = SelectBTDevices.addressBTDevice;
                            Device8EX.Open();
                            Device8EX.ReceiveData += new ReceiveDataDelegate(Device8EX_ReceiveData);
                            Device8EX.BeginTransmit();
                            aTimer.Enabled = true;
                            nameDevice.Foreground = Brushes.Green;
                            nameDevice.FontWeight = FontWeights.Bold;
                            monitoringPlotter.Height = 600;
                        }
                        else
                        {
                            Device8EX.StopTransmit();
                            Device8EX.Close();
                            //Device8EX = null;
                            nameDevice.Foreground = Brushes.Black;
                            nameDevice.FontWeight = FontWeights.Normal;
                        }
                        openCloseBTDevice.Content = Device8EX.Active == false ? "OpenBTDevice" : "CloseBTDevice";
                        this.Cursor = Cursors.Arrow;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    //Device8EXv2012 = null;
                    //Device8EX = null;
                    nameDevice.Foreground = Brushes.Red;
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
            this.Cursor = Cursors.Arrow;
        }

        public static bool onOfBr;
        /// <summary>
        /// Событие о передаче данных
        /// </summary>
        void ReadRecieveDataPS8EXv2012(object sender, byte[] buffer, int count)
        {
            onOfBr = (bool)checkBoxBr.IsChecked ? true : false;
            ProtocolPS8EXv2012.ReadDeviceData(buffer, count);
            if (ProtocolPS8EXv2012.NumFramesInCurPacket != 0)
            {
                ECG_data.ChannelsSamplesBuffers = channelData;
                ECG_data.ChannelsSamplesCount = null;
                ECG_data.FirstSampleNumber = (ulong)ProtocolPS8EXv2012.NumFirstADCFrame;
                ECG_data.SamplesCount = ProtocolPS8EXv2012.NumFramesInCurPacket;
                ECG_data.EventsBuffer = null;
                ECG_data.EventsCount = 0;
                ECG_data.FirstSampleState = firstSampleState;
                ECG_data.SynchronizationNumber = -1;
                monitoringPlotter.WriteData(ECG_data);
                AppendDataStatistics(ECG_data);
            }
        }

        void Device8EX_ReceiveData(object sender, ReceiveDataArgs e)
        {
            ECG_data1.ChannelsSamples = e.Data;
            ECG_data1.SamplesCount = e.Data[0].Length;
            ECG_data1.Stimuls = null;
            ECG_data1.StimulsCount = 0;
            ECG_data1.SynchronizationNumber = -1;
            if (ECG_data1.SamplesCount != 0)
            {
                monitoringPlotter.WriteData(ECG_data1);
                AppendDataStatistics(e);
            }
        }

        #region Statistic

        protected ObservableCollection<DataStatistics> statisticsCollection = new ObservableCollection<DataStatistics>();
        /// <summary>
        /// Коллекция со статистикой 
        /// </summary>
        public ObservableCollection<DataStatistics> StatisticsCollection
        {
            get { return statisticsCollection; }
        }

        protected double[] statValueMin;
        protected double[] statValueMax;
        protected double[] arithmeticMean;
        protected double[] statValueRMS;
        protected int statSamplesCount = 0;   // счетчик отсчетов для вычисления статистики
        protected double[] samplesSum;
        protected double[] samplesSumSqr;

        /// <summary>
        /// Пополнение статистики новыми данными
        /// </summary>
        /// <param name="data"></param>
        protected void AppendDataStatistics(SamplesEventsCallbackArgs data)
        {
            int maxIndex = 0;   // индекс массива на котором достигнут максимум
            int minIndex = 0;   // индекс массива на котором достигнут минимум
            int channelIndex;

            for (channelIndex = 0; channelIndex < ChannelsCount; channelIndex++)
            {
                // суммируем данные
                samplesSum[channelIndex] += NeuroSoft.MathLib.Basic.SumArray_IPP(data.ChannelsSamplesBuffers[channelIndex], 0, data.SamplesCount);
                // поиск максимального и минимального элемента массива
                NeuroSoft.MathLib.Basic.CalcMinMax_IPP(data.ChannelsSamplesBuffers[channelIndex], 0, data.SamplesCount, out minIndex, out maxIndex);
                if (statValueMax[channelIndex] < data.ChannelsSamplesBuffers[channelIndex][maxIndex])
                {
                    statValueMax[channelIndex] = data.ChannelsSamplesBuffers[channelIndex][maxIndex];
                }
                if (statValueMin[channelIndex] > data.ChannelsSamplesBuffers[channelIndex][minIndex])
                {
                    statValueMin[channelIndex] = data.ChannelsSamplesBuffers[channelIndex][minIndex];
                }
                // взятие квадрата от всех элементов массива
                float[] sqrDest = new float[data.SamplesCount]; // массив приёмник
                NeuroSoft.MathLib.Basic.SqrOnArray_IPP(data.ChannelsSamplesBuffers[channelIndex], 0, sqrDest, 0, data.SamplesCount);
                samplesSumSqr[channelIndex] += NeuroSoft.MathLib.Basic.SumArray_IPP(sqrDest);
            }
            statSamplesCount += data.SamplesCount;
        }

        /// <summary>
        /// Пополнение статистики новыми данными
        /// </summary>
        /// <param name="e"></param>
        protected void AppendDataStatistics(ReceiveDataArgs e)
        {
            int maxIndex = 0;   // индекс массива на котором достигнут максимум
            int minIndex = 0;   // индекс массива на котором достигнут минимум
            int channelIndex;
            for (channelIndex = 0; channelIndex < ChannelsCount; channelIndex++)
            {
                if (e.Data[channelIndex] != null)
                {
                    // суммируем данные
                    samplesSum[channelIndex] += NeuroSoft.MathLib.Basic.SumArray_IPP(e.Data[channelIndex], 0, e.Data[channelIndex].Length);
                    // поиск максимального и минимального элемента массива
                    NeuroSoft.MathLib.Basic.CalcMinMax_IPP(e.Data[channelIndex], 0, e.Data[channelIndex].Length, out minIndex, out maxIndex);
                    if (statValueMax[channelIndex] < e.Data[channelIndex][maxIndex])
                    {
                        statValueMax[channelIndex] = e.Data[channelIndex][maxIndex];
                    }
                    if (statValueMin[channelIndex] > e.Data[channelIndex][minIndex])
                    {
                        statValueMin[channelIndex] = e.Data[channelIndex][minIndex];
                    }
                    // взятие квадрата от всех элементов массива
                    float[] sqrDest = new float[e.Data[channelIndex].Length]; // массив приёмник
                    NeuroSoft.MathLib.Basic.SqrOnArray_IPP(e.Data[channelIndex], 0, sqrDest, 0, e.Data[channelIndex].Length);
                    samplesSumSqr[channelIndex] += NeuroSoft.MathLib.Basic.SumArray_IPP(sqrDest);
                }
            }
            statSamplesCount += e.Data[0].Length;
        }

        void SetStatistic(MonitoringType type)
        {
            samplesSum = new double[ChannelsCount];
            statValueMax = new double[ChannelsCount];
            statValueMin = new double[ChannelsCount];
            samplesSumSqr = new double[ChannelsCount];
            ResetMinMax();
            StatisticsCollection.Clear();
            for (int i = 0; i < ChannelsCount; i++)
            {
                StatisticsCollection.Add(new DataStatistics(channelsNames[i]));
                if (type == MonitoringType.NOISE_DATA)
                {
                    StatisticsCollection[i].Swing.MinValue = 0.0f;
                    StatisticsCollection[i].Swing.MaxValue = 20e-6f;
                    monitoringPlotter.SelectedXScaleItem = monitoringPlotter.XScaleItems[9];
                    monitoringPlotter.SelectedYScaleItem = monitoringPlotter.YScaleItems[7];
                }
                if (type == MonitoringType.ECG_DATA)
                {
                    StatisticsCollection[i].Swing.MinValue = 19.8e-3f;
                    StatisticsCollection[i].Swing.MaxValue = 20.2e-3f;
                    monitoringPlotter.SelectedXScaleItem = monitoringPlotter.XScaleItems[10];
                    monitoringPlotter.SelectedYScaleItem = monitoringPlotter.YScaleItems[15];
                }
                if (type == MonitoringType.CMRR_DATA)
                {
                    StatisticsCollection[i].Swing.MinValue = 0.0f;
                    StatisticsCollection[i].Swing.MaxValue = 2.5e-3f;
                    monitoringPlotter.SelectedXScaleItem = monitoringPlotter.XScaleItems[10];
                    monitoringPlotter.SelectedYScaleItem = monitoringPlotter.YScaleItems[14];
                }
                if (type == MonitoringType.BR_DATA)
                {
                    StatisticsCollection[i].Swing.MinValue = 4.0e-3f;
                    StatisticsCollection[i].Swing.MaxValue = 9.0e-3f;
                }
            }
        }

        /// <summary>
        /// Сброс максимальных и минимальных значений
        /// </summary>
        protected void ResetMinMax()
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                statValueMax[i] = float.MinValue;
                statValueMin[i] = float.MaxValue;
            }
        }

        /// <summary>
        /// Обновление статистики
        /// </summary>
        public virtual void UpdateStatistic()
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (statSamplesCount > 0)
                {
                    double averange = samplesSum[i] / statSamplesCount;
                    StatisticsCollection[i].UpdateStatistics(averange, statValueMin[i], statValueMax[i], Math.Sqrt(samplesSumSqr[i] / statSamplesCount - averange * averange));
                }
            }
            statSamplesCount = 0;
            Array.Clear(samplesSum, 0, ChannelsCount);
            Array.Clear(samplesSumSqr, 0, ChannelsCount);
            ResetMinMax();
        }

        #endregion

        /// <summary>
        /// Имена каналов
        /// </summary>
        public string[] channelsNames = new string[] { "L", "F", "C1", "C2", "C3", "C4", "C5", "C6", "BR", "T" };

        /// <summary>
        /// Инициализация плоттера
        /// </summary>
        protected virtual void InitPlotter()
        {
            SetupDataPlotter(SelectBTDevices.nameBTDevice);
            monitoringPlotter.Height = 30;
            monitoringPlotter.ChannelsCount = ChannelsCount;
            monitoringPlotter.SamplesRateInSamplesPerSecond = SamplesRateInSamplesPerSecond;
            comboBoxFreqBr.Visibility = Visibility.Hidden;
        }

        public void SetupDataPlotter(string s)
        {
            int i;
            float interval = 15.0f;
            float poz = interval;
            if (s.IndexOf(PolySpectrum8EX_2012.DeviceNameSign) > -1)
            {
                FreqDivider = 1;
                FreqDividerBr = 20;
                ChannelsCount = ProtocolPS8EXv2012.myChannelsCount;
                NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[] channelParams = new NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[ChannelsCount];
                for (i = 0; i < ChannelsCount - 1; i++)
                {
                    channelParams[i] = new Hardware.Devices.Controls.DataPlotter.ChannelParams(channelsNames[i], poz, 1.0f, FreqDivider);
                    poz += interval;
                }
                channelParams[i] = new Hardware.Devices.Controls.DataPlotter.ChannelParams(channelsNames[i], poz, 1.0f, FreqDividerBr);
                monitoringPlotter.InitChannels(channelParams, (int)interval);
                return;
            }
            if (s.IndexOf(PolySpectrum8EX.DeviceNameSign) > -1)
            {
                FreqDivider = 1;
                FreqDividerBr = 1;
                Device8EX.MonitoringMode = PSDeviceMonitoringMode.DirectChannels;
                ChannelsCount = Device8EX.GetChannelsCount();
                NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[] channelParams = new NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[ChannelsCount];
                for (i = 0; i < ChannelsCount - 2; i++)
                {
                    channelParams[i] = new Hardware.Devices.Controls.DataPlotter.ChannelParams(channelsNames[i], poz, 1.0f, FreqDivider);
                    poz += interval;
                }
                channelParams[i] = new Hardware.Devices.Controls.DataPlotter.ChannelParams(channelsNames[i], poz, 1.0f, FreqDividerBr);
                i++;
                channelParams[i] = new Hardware.Devices.Controls.DataPlotter.ChannelParams(channelsNames[i], poz, 1.0f, FreqDivider);
                monitoringPlotter.InitChannels(channelParams, (int)interval);
                return;
            }
        }
        /// <summary>
        /// Частота дискретизации
        /// </summary>
        public virtual float SamplesRateInSamplesPerSecond { get; set; }

        private ScaleItem[] freqScaleBr = null;
        public ScaleItem[] FreqScaleBr
        {
            get
            {
                if (freqScaleBr == null)
                {
                    freqScaleBr = new ScaleItem[] { new ScaleItem(0.05f, "0.05 Гц"), new ScaleItem(0.5f, "0.5 Гц"), new ScaleItem(6.0f, "6.0 Гц") };
                }
                return freqScaleBr;
            }
        }

        private ScaleItem selectedFreqBr;
        public ScaleItem SelectedFreqBr
        {
            get { return selectedFreqBr; }
            set
            {
                if (selectedFreqBr != value)
                {
                    selectedFreqBr = value;
                    OnPropertyChanged("SelectedFreqBr");
                }
            }
        }

        /// <summary>
        ////Меняем частоту канала дыхания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxFreqBr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((bool)checkBoxBr.IsChecked)
            {
                if (selectedFreqBr.Scale == 0.05f)
                {
                    monitoringPlotter.SelectedXScaleItem = monitoringPlotter.XScaleItems[4];
                    monitoringPlotter.SelectedYScaleItem = monitoringPlotter.YScaleItems[13];
                    aTimer.Interval = 15000;
                }
               if (selectedFreqBr.Scale == 0.5f)
                {
                    monitoringPlotter.SelectedXScaleItem = monitoringPlotter.XScaleItems[6];
                    monitoringPlotter.SelectedYScaleItem = monitoringPlotter.YScaleItems[14];
                    aTimer.Interval = 2000;
                }
                if (selectedFreqBr.Scale == 6.0f)
                {
                    monitoringPlotter.SelectedXScaleItem = monitoringPlotter.XScaleItems[8];
                    monitoringPlotter.SelectedYScaleItem = monitoringPlotter.YScaleItems[13];
                    aTimer.Interval = 1000;
                }
                UniversalTestStand stand = environment.Stand;
                stand.ResetCommutatorState();
                StandOperations.SetGeneratorState(environment, 0.25f, selectedFreqBr.Scale);
                NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Scripts.StandOperation.SetBreathChannel(environment, true);
            }
        }

        private void checkBoxBr_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxFreqBr.Visibility = Visibility.Visible;
            SelectedFreqBr = FreqScaleBr[1];
        }

        private void checkBoxBr_Unchecked(object sender, RoutedEventArgs e)
        {
            comboBoxFreqBr.Visibility = Visibility.Hidden;
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

        public void Dispose()
        {
            monitoringPlotter.Height = 30;
            aTimer = null;
            Device8EX = null;
            Device8EXv2012 = null;
            monitoringPlotter = null;
        }

        // сброс изолинии
        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            Device8EX.ResetFilters();           
        }
    }
}
