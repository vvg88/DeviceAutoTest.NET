using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.Hardware.Devices;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.NewDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class TestInputResistanceViewModel : ReadDataViewModel
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="device"></param>
        /// <param name="amplifierCapabilities"></param>
        public TestInputResistanceViewModel(ScriptEnvironment environment, NeuroMepBase device, AmplifierCapabilities amplifierCapabilities)
            : base(environment, device, amplifierCapabilities)
        {
            for (int i = 0; i < amplifierCapabilities.ChannelsCount; i++)
            {
                Rows.Add(new InputResistanceInfo(i, 3));
                int maxRange = amplifierCapabilities.GetChannelSupportedRange(i).Count - 1;
                if (maxRange > 0)
                    Device.AmplifierSetChannelRangeIndex(i, amplifierCapabilities.GetChannelSupportedRange(i).Count - 1);
                if (amplifierCapabilities.GetChannelSupportedLowFreqPassBand(i).Count > 0)
                    Device.AmplifierSetChannelLowFreqPassBandIndex(i, 0);
            }
        }

        #region Properties

        private ObservableCollection<InputResistanceInfo> rows = new ObservableCollection<InputResistanceInfo>();        
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<InputResistanceInfo> Rows
        {
            get { return rows; }
        }


        private TestSignalInfo testInfo;
        /// <summary>
        /// Информация о процессе тестирования
        /// </summary>
        internal TestSignalInfo TestInfo
        {
            get { return testInfo; }
            private set
            {
                if (testInfo != null)
                {
                    testInfo.Stop();
                }
                testInfo = value;
            }
        }

        private bool FreqIs2Hz;

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

        #endregion
        /// <summary>
        /// 
        /// </summary>
        public void StartTest()
        {
            if (IsTesting)
                return;
            foreach (var row in Rows)
            {
                row.Reset();
            }                        
            StartReadData();
            SwitchFreq(true);
        }

        private void SwitchFreq(bool switchTo2Hz)
        {
            foreach (var row in Rows)
            {
                row.FreqIs2Hz = switchTo2Hz;
            }
            float freq = switchTo2Hz ? 2 : 2000;
            TestInfo = new TestSignalInfo(freq);
            FreqIs2Hz = switchTo2Hz;
            EnablePowerRejector = true;
            if (FreqIs2Hz)
            {
                SetFilters(0, 20);
            }
            else
            {
                //SetFilters(200, 20000);
                SetFilters(1000, 3000);
            }
            SetStandParams(freq);
            ResetDataStatistics();
            TestInfo.Start();
        }

        private void SetFilters(float minFreq, float maxFreq)
        {
            foreach (var filter in FiltersInfo)
            {
                filter.IsEnabled = true;
                if (minFreq > filter.MaxFreq)
                {
                    filter.MaxFreq = maxFreq;
                    filter.MinFreq = minFreq;
                }
                else
                {
                    filter.MinFreq = minFreq;
                    filter.MaxFreq = maxFreq;
                }
            }          
        }
        private void SetStandParams(float freq)
        {
            Environment.OpenStand();
            var stand = Environment.Stand;
            if (stand == null)
                return;            
            StandOperations.SetGeneratorState(Environment, 0.5f, freq);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        protected override void OnReadSamplesStimulsData(SamplesStimulsDataEventArgs e, object sender)
        {
            if (TestInfo == null || !TestInfo.Started || !TestInfo.DelayIsOut)
            {
                if (samples > 0)
                {
                    ResetDataStatistics();
                }
                return;
            }
            if (!FreqIs2Hz)//для 2 кГц сделаем "поправку" (несовершенство стенда)
            {
                for (int i = 0; i < ChannelsCount; i++)
                {
                    NeuroSoft.MathLib.Basic.MulConst_IPP(e.ChannelsSamples[i], 0.8f);
                }
            }
            //добавим новые отсчеты в статистику
            base.OnReadSamplesStimulsData(e, sender);
            //проверим, не стабилизировался ли размах
            TestCurrentSwing();
        }

        private void TestCurrentSwing()
        {
            lock (TestInfo)
            {
                if (TestInfo == null)
                    return;
                if (TestInfo.Timeouted) //заканчиваем определение размаха по таймауту
                {
                    EndTestSwing();
                }
                else if (samples >= TestInfo.SamplesLimit)
                {
                    if (UpdateSwingInfo())
                    {
                        EndTestSwing();
                    }
                }
            }
        }

        private bool UpdateSwingInfo()
        {
            if (TestInfo == null)
                return false;
            bool hasInvalid = false;
            for (int i = 0; i < AmplifierCapabilities.ChannelsCount; i++)
            {
                var row = Rows[i];
                row.AppendSwing(maxSamples[i] - minSamples[i]);
                if (!row.CurrentSwingIsStable)
                {
                    hasInvalid = true;
                }
            }
            ResetDataStatistics();
            return !hasInvalid;
        }

        private void EndTestSwing()
        {
            if (TestInfo == null)
                return;
            bool swingIsStable = true;
            foreach (var row in Rows)
            {
                if (!row.CurrentSwingIsStable)
                {
                    swingIsStable = false;
                    break;
                }
            }
            TestInfo = null;
            if (swingIsStable && FreqIs2Hz)
            {
                SwitchFreq(false);
            }
            else
            {
                StopTest();
                Environment.DoAutoTest();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
        public void StopTest()
        {           
            IsTesting = false;
            TestInfo = null;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                StopReadData();
            }));
            CalcResistanceAndCapacity();
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopStart()
        {
            if (IsTesting)
            {
                StopTest();
            }
            else
            {
                StartTest();
            }
        }

        /// <summary>
        /// Расчет сопротивления и емкости
        /// </summary>
        private void CalcResistanceAndCapacity()
        {
            foreach (var row in Rows)
            {
                row.CalcResistanceAndCapacity();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class InputResistanceInfo : DATBaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stabilizationSwingLimit"></param>
        public InputResistanceInfo(int channel, int stabilizationSwingLimit)
        {
            StabilizationSwingLimit = stabilizationSwingLimit;
            Channel = channel;
            Reset();
        }

        #region Properties               
        private int StabilizationSwingLimit;

        private StableableSwing swing2Hz = null;
        /// <summary>
        /// Размах сигнала при частоте 2 Гц
        /// </summary>
        public StableableSwing Swing2HzInfo
        {
            get { return swing2Hz; }
        }

        private StableableSwing swing2000Hz = null;
        /// <summary>
        /// Размах сигнала при частоте 2 КГц
        /// </summary>
        public StableableSwing Swing2000HzInfo
        {
            get { return swing2000Hz; }
        }

        private RangedValue<double> capacity = null;

        /// <summary>
        /// Ёмкость (Ф)
        /// </summary>
        public RangedValue<double> Capacity
        {
            get { return capacity; }
            private set 
            {
                capacity = value;
                OnPropertyChanged("Capacity");
            }
        }

        /// <summary>
        /// Допустима ли текущая емкость
        /// </summary>
        public bool? CapacityIsValid
        {
            get
            {
                if (double.IsNaN(Capacity.Value))
                    return null;
                return Capacity.IsValidValue;
            }
        }

        private RangedValue<double> resistance = null;

        /// <summary>
        /// Сопротивление (Ом)
        /// </summary>
        public RangedValue<double> Resistance
        {
            get { return resistance; }
            private set
            {
                resistance = value;
                OnPropertyChanged("Resistance");
            }
        }

        /// <summary>
        /// Допустимо ли текущее сопротивление
        /// </summary>
        public bool? ResistanceIsValid
        {
            get
            {
                if (double.IsNaN(Resistance.Value))
                    return null;
                return Resistance.IsValidValue;
            }
        }

        private bool freqIs2Hz = true;
        /// <summary>
        /// Признак расчета размаха для частоты 2 Гц
        /// </summary>
        public bool FreqIs2Hz
        {
            get { return freqIs2Hz; }
            set 
            { 
                freqIs2Hz = value;
                OnPropertyChanged("FreqIs2Hz");
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
                return "Channel: " + Channel;
            }
        }

        /// <summary>
        /// Признак стабильности сигнала на текущей частоте
        /// </summary>
        public bool CurrentSwingIsStable
        {
            get
            {
                if (FreqIs2Hz)
                {
                    return Swing2HzInfo.IsStable;
                }
                else
                {
                    return Swing2000HzInfo.IsStable;
                }
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="swingValue"></param>
        public void AppendSwing(double swingValue)
        {
            if (FreqIs2Hz)
            {
                Swing2HzInfo.UpdateSwing(swingValue);
            }
            else
            {
                Swing2000HzInfo.UpdateSwing(swingValue);
            }
        }
       
        internal void Reset()
        {
            swing2Hz = new StableableSwing(StabilizationSwingLimit);
            OnPropertyChanged("Swing2HzInfo");
            swing2000Hz = new StableableSwing(StabilizationSwingLimit);
            OnPropertyChanged("Swing2000HzInfo");
            Resistance = new RangedValue<double>(double.NaN, 1.8e+9, 2.2e+9);
            OnPropertyChanged("Resistance");
            OnPropertyChanged("ResistanceIsValid");
            Capacity = new RangedValue<double>(double.NaN, 13e-12, 22e-12);
            OnPropertyChanged("Capacity");
            OnPropertyChanged("CapacityIsValid");
        }

        /// <summary>
        /// Расчет сопротивления и емкости
        /// </summary>
        internal void CalcResistanceAndCapacity()
        {
            if (!Swing2HzInfo.IsStable || !Swing2000HzInfo.IsStable)
            {
                return;
            }

            double R = 200e3d;//Резистор 200 КОм
            double V = 1; //Сигнал размахом 1 В
            double C = 22e-9d; //Конденсатор 22 нФ
            double F1 = 2d; //Частота 2 Гц
            double F2 = 2000d; // Частота 2 КГц

            double Cg = 1 / (R * R) + System.Math.Pow(2 * System.Math.PI * C * F1, 2);
            double Cd = 1 / (R * R) + System.Math.Pow(2 * System.Math.PI * C * F2, 2);

            double Ce = System.Math.Pow(2 * System.Math.PI * F1, 2);
            double Ca = Ce - System.Math.Pow(2 * System.Math.PI * F2, 2);
            
            double u1 = Swing2HzInfo.SwingValue;
            double u2 = Swing2000HzInfo.SwingValue;

            double Cb = System.Math.Pow(V - u1, 2);
            double Cv = System.Math.Pow(V - u2, 2);

            double Cgb = Cg / Cb;
            double Cdv = Cd / Cv;
            double Cea = Ce / Ca;

            Resistance.Value = 1 / System.Math.Sqrt(u1 * u1 * Cgb - (Cea * (u1 * u1 * Cgb - u2 * u2 * Cdv)));
            OnPropertyChanged("Resistance");
            OnPropertyChanged("ResistanceIsValid");
            Capacity.Value = System.Math.Sqrt((u1 * u1 * Cgb - u2 * u2 * Cdv) / Ca);
            OnPropertyChanged("Capacity");
            OnPropertyChanged("CapacityIsValid");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StableableSwing : DATBaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stabilizationLimit"></param>
        /// <param name="stableMaxError"></param>
        public StableableSwing(int stabilizationLimit, double stableMaxError = 0.05d)
        {
            StabilizationLimit = stabilizationLimit;
            StableMaxError = stableMaxError;
        }
        private List<double> SwingHistory = new List<double>();
        /// <summary>
        /// Максимальная относительная погрешность при стабилизации сигнала
        /// </summary>
        private double StableMaxError = 0.05;        
        private int stabilizationLimit;

        /// <summary>
        /// Максимальная относительная погрешность при стабилизации сигнала
        /// </summary>
        public int StabilizationLimit
        {
            get { return stabilizationLimit; }
            private set
            { 
                stabilizationLimit = value;
            }
        }

        private double swingValue = double.NaN;
        /// <summary>
        /// Текущий размах сигнала (в вольтах)
        /// </summary>
        public double SwingValue
        {
            get { return swingValue; }
            private set
            {
                swingValue = value;
                OnPropertyChanged("SwingValue");
            }
        }

        private bool isStable = false;
        /// <summary>
        /// Признак стабилизации сигнала
        /// </summary>
        public bool IsStable
        {
            get { return isStable; }
            private set
            {
                isStable = value;
                OnPropertyChanged("IsStable");
            }
        }

        /// <summary>
        /// Обновление значения размаха сигнала
        /// </summary>
        /// <param name="newSwing"></param>
        public void UpdateSwing(double newSwing)
        {
            SwingValue = newSwing;
            
            IsStable = IsStableInternal;
            if (SwingHistory.Count > 0 && SwingHistory.Count == StabilizationLimit)
            {
                SwingHistory.RemoveAt(0);
            }
            SwingHistory.Add(SwingValue);
        }

        private bool IsStableInternal
        {
            get
            {
                if (SwingHistory.Count < StabilizationLimit)
                {
                    return false;
                }
                double absoluteError = SwingValue * StableMaxError;
                if (SwingHistory.Max() > SwingValue + absoluteError || SwingHistory.Min() < SwingValue - absoluteError)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
