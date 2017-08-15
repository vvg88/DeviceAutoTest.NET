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
using System.Collections.ObjectModel;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using System.Windows.Threading;
using NeuroSoft.Common;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for ElectrodeImpedancesControl.xaml
    /// </summary>
    public partial class ElectrodeImpedancesControl : UserControl, INotifyPropertyChanged
    {
        public ElectrodeImpedancesControl(ScriptEnvironment environment, NeuroMEPMicro device, double minImpValues, double maxImpValues)
        {
            InitializeComponent();
            this.device = device;
            ChannelsCount = device.GetChannelsCount();
            Environment = environment;
            Impedances.Add(new ImpedanceInfo(0, "Ground"));
            for (int i = 0; i < channelsCount; i++)
            {
                Impedances.Add(new ImpedanceInfo(i * 2 + 1, "Электрод " + (i + 1) + "+", minImpValues, maxImpValues));
                Impedances.Add(new ImpedanceInfo(i * 2 + 2, "Электрод " + (i + 1) + "-", minImpValues, maxImpValues));
            }
            SavedImpedances.Add(new ImpedanceInfo(0, "Ground"));
            for (int i = 0; i < channelsCount; i++)
            {
                SavedImpedances.Add(new ImpedanceInfo(i * 2 + 1, "Электрод " + (i + 1) + "+", minImpValues, maxImpValues));
                SavedImpedances.Add(new ImpedanceInfo(i * 2 + 2, "Электрод " + (i + 1) + "-", minImpValues, maxImpValues));
            }
            DataContext = this;
        }

        #region Properties

        private List<ImpedanceInfo> impedances = new List<ImpedanceInfo>();

        /// <summary>
        /// Коллекция импедансов
        /// </summary>
        public List<ImpedanceInfo> Impedances
        {
            get { return impedances; }
        }

        private ObservableCollection<ImpedanceInfo> savedImpedances = new ObservableCollection<ImpedanceInfo>();

        public ObservableCollection<ImpedanceInfo> SavedImpedances
        {
            get { return savedImpedances; }
        }
        
        private NeuroMEPMicro device;
        /// <summary>
        /// 
        /// </summary>
        public NeuroMEPMicro Device
        {
            get { return device; }
            private set { device = value; }
        }

        private ScriptEnvironment environment;
        /// <summary>
        /// 
        /// </summary>
        public ScriptEnvironment Environment
        {
            get { return environment; }
            private set { environment = value; }
        }

        private int channelsCount;
        /// <summary>
        /// 
        /// </summary>
        public int ChannelsCount
        {
            get { return channelsCount; }
            private set { channelsCount = value; }
        }
        
        private bool started = false;
        private int currentElectrodeIndex = -1;

        /// <summary>
        /// Индекс тестируемого в данный момент электрода
        /// </summary>
        public int CurrentElectrodeIndex
        {
            get { return currentElectrodeIndex; }
            private set { currentElectrodeIndex = value; }
        }

        internal ImpedanceInfo CurrentImpedanceInfo
        {
            get
            {
                if (CurrentElectrodeIndex < 0 || CurrentElectrodeIndex > SavedImpedances.Count - 1)
                    return null;
                return SavedImpedances[CurrentElectrodeIndex];                  
            }
        }
        private bool startTestSolderingPending = false;

        private Range<double> testImpedanceRange = new Range<double>(9000, 11000);
        /// <summary>
        /// Область допустимых значений импеданса на проверяемом электроде
        /// </summary>
        public Range<double> TestImpedanceRange
        {
            get { return testImpedanceRange; }
            private set { testImpedanceRange = value; }
        }

        private bool testing = false;
        /// <summary>
        /// Признак процесса тестирования
        /// </summary>
        public bool Testing
        {
            get { return testing; }
            set
            {
                if (testing != value)
                {
                    testing = value;
                    OnPropertyChanged("Testing");
                    OnPropertyChanged("TestingResult");
                }
            }
        }

        private string testingStatus;
        /// <summary>
        /// Статус процесса тестирования
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

        /// <summary>
        /// Результат тестирования
        /// </summary>
        public bool? TestingResult
        {
            get
            {
                if (Testing)
                    return null;
                for (int i = 1; i < SavedImpedances.Count; i++)
                {
                    var impedanceInfo = SavedImpedances[i];
                    if (impedanceInfo.IsValid != true)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion

        #region Methods
                
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if (started)
                return;            
            if (Device != null)
            {
                System.Windows.Forms.UserControl userImpControl = (Device as IDeviceImpedance).GetControl();
                Device.DeviceImpedance += new DeviceImpedanceDelegate(device_AmplifierImpedanceEventNew);
                Device.SetWorkMode(NeuroMEPMicroWorkMode.ImpedanceTransmit);
                Device.BeginTransmitImpedance();
                started = true;
            }
        }

        private int testImpedanceLimit = 20;
        private int testImpedanceCount = 0;

        void device_AmplifierImpedanceEventNew(object sender, DeviceImpedanceArgs e)
        {            
            if (startTestSolderingPending || !testing)
                return;
            UpdateImpedaces(e.State);
            testImpedanceCount++;
            var impedanceInfo = CurrentImpedanceInfo;
            if (impedanceInfo == null)
            {
                FinishTesting();
                return;
            }
            //bool isValid = impedanceInfo.Resistance.Value >= TestImpedanceRange.Min && impedanceInfo.Resistance.Value <= TestImpedanceRange.Max;
            bool isValid = impedanceInfo.Resistance.IsValidValue;
            if (isValid && testImpedanceCount > 10)
            {
                impedanceInfo.IsValid = isValid;
                //SavedImpedances[currentElectrodeIndex].Resistance.Value = Impedances[currentElectrodeIndex].Resistance.Value;
                CurrentElectrodeIndex++;
                StartTestElectrode();
            }
            else 
                if (testImpedanceCount > testImpedanceLimit)
                {
                    impedanceInfo.IsValid = false;
                    FinishTesting();
                }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (!started)
                return;
            var device = Device;
            Dispatcher.BeginInvoke(new Action(()=>
            {
                if (device != null)
                {
                    device.StopTransmitImpedance();
                    device.SetWorkMode(NeuroMEPMicroWorkMode.VPTransmit);
                    device.DeviceImpedance -= new DeviceImpedanceDelegate(device_AmplifierImpedanceEventNew);
                    //device.AmplifierPowerOff();
                    started = false;                    
                }
            }));            
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Stop();
            Device = null;
            ClearSolderingEventHandlers();
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (ImpedanceInfo impInfo in SavedImpedances)
            {
                impInfo.Resistance.Value = 0.0;
            }
            StartTesting();
        }

        public void StartTesting()
        {
            if (Testing)
                return;
            foreach (var impedance in Impedances)
            {
                impedance.IsValid = null;
            }
            Testing = true;
            TestingStatus = string.Format("Подготовка к тестированию...");
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                Start();
                CurrentElectrodeIndex = 1;
                StartTestElectrode();
            }));
        }

        private void StartTestElectrode()
        {
            if (CurrentElectrodeIndex > ChannelsCount * 2 || CurrentImpedanceInfo == null)
            {
                FinishTesting();
                return;
            }
            testImpedanceCount = 0;
            sumOfImpedances = 0.0;
            TestingStatus = string.Format("Тестирование электрода: {0}", CurrentImpedanceInfo.Title);
            startTestSolderingPending = true;
            if (StartTestSoldering != null)
            {
                StartTestSoldering(this, CurrentElectrodeIndex);
            }
            startTestSolderingPending = false;
        }

        private void FinishTesting()
        {
            if (!testing)
                return;
            Testing = false;
            CurrentElectrodeIndex = -1;
            Stop();
            TestingStatus = string.Format("Тестирование завершено. Тест {0}пройден.", TestingResult == true ? "" : "не ");
            Environment.DoAutoTest();
        }

        /// <summary>
        /// Сумма значений импедансов для вычисления среднего значения
        /// </summary>
        private double sumOfImpedances = 0.0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="impedances"></param>
        public void UpdateImpedaces(NSDeviceImpedanceState impedanceState)
        {
            var impedances = (impedanceState as NeuroMEPAmplifierEEGImpedanceState).ImpedanceChannelsStates;
            if (impedances == null)
                return;
            Impedances[0].Resistance.Value = ((impedances[0].Ground + impedances[1].Ground) / 2) * 1e3;
            SavedImpedances[0] = Impedances[0];
            foreach (var impedance in impedances)
            {
                Impedances[(int)impedance.ChannelNum * 2 + 1].Resistance.Value = impedance.BipolarPlus * 1e3;
                Impedances[(int)impedance.ChannelNum * 2 + 2].Resistance.Value = impedance.BipolarMinus * 1e3;
            }
            if (testImpedanceCount != 0)
            {
                sumOfImpedances += Impedances[currentElectrodeIndex].Resistance.Value;
                SavedImpedances[currentElectrodeIndex].Resistance.Value = sumOfImpedances / testImpedanceCount;
            }
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        public event StartTestSolderingEventHandler StartTestSoldering;
        private void ClearSolderingEventHandlers()
        {
            if (StartTestSoldering != null)
            {
                Delegate[] delegateList = StartTestSoldering.GetInvocationList();
                foreach (StartTestSolderingEventHandler oldHandler in delegateList)
                {
                    StartTestSoldering -= oldHandler;
                }
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="electrode"></param>
    public delegate void StartTestSolderingEventHandler(object sender, int electrode);

    public class ImpedanceInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="title"></param>
        /// <param name="measureFreq"></param>
        /// <param name="minResistance"></param>
        /// <param name="maxResistance"></param>
        /// <param name="minCapacity"></param>
        /// <param name="maxCapacity"></param>
        public ImpedanceInfo(int index, string title, double minResistance, double maxResistance)
        {
            resistance = new RangedValue<double>(0, minResistance, maxResistance);
            Title = title;
            Index = index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="title"></param>
        /// <param name="measureFreq"></param>
        public ImpedanceInfo(int index, string title)
        {
            resistance = new RangedValue<double>(0, 0, 0) { IgnoreRange = true };
            Title = title;
            Index = index;
        }

        //private double measureFreq;
        ///// <summary>
        ///// Частота на которой измеряется импеданс электродов. Если меньше нуля, то измерение импеденса не поддерживается.
        ///// </summary>
        //public double MeasureFreq
        //{
        //    get { return measureFreq; }
        //    private set { measureFreq = value; }
        //}

        private int index;
        /// <summary>
        /// Индекс импеданса
        /// </summary>
        public int Index
        {
            get { return index; }
            private set { index = value; }
        }

        private string title;
        /// <summary>
        /// Текстовое описание
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        //private Complex impedance;
        ///// <summary>
        ///// Значение импеданса
        ///// </summary>
        //public Complex Impedance
        //{
        //    get { return impedance; }
        //    set
        //    {
        //        if (IsValid.HasValue)
        //            return;
        //        impedance = value;
        //        UpdateCapacity();
        //        UpdateResistance();
        //        OnPropertyChanged("Impedance");
        //        OnPropertyChanged("Phi");
        //        OnPropertyChanged("Real");
        //        OnPropertyChanged("Abs");
        //        OnPropertyChanged("Image");
        //    }
        //}

        //private RangedValue<double> capacity;
        ///// <summary>
        ///// Емкость
        ///// </summary>
        //public RangedValue<double> Capacity
        //{
        //    get
        //    {
        //        return capacity;
        //    }
        //}

        //private void UpdateCapacity()
        //{
        //    double temp = Impedance.Real * Impedance.Real + Impedance.Imaginary * Impedance.Imaginary;
        //    double k = 1e12 / (2 * System.Math.PI * MeasureFreq);
        //    Capacity.Value = Impedance.Imaginary * k / temp;
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public double Phi
        //{
        //    get
        //    {
        //        return Impedance.Phase * 180 / System.Math.PI;
        //    }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public double Abs
        //{
        //    get
        //    {
        //        return Impedance.Magnitude;
        //    }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public double Image
        //{
        //    get
        //    {
        //        return Impedance.Imaginary;
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public double Real
        //{
        //    get
        //    {
        //        return Impedance.Real;
        //    }
        //}

        private RangedValue<double> resistance = null;
        /// <summary>
        /// Значение сопротивления электрода
        /// </summary>
        public RangedValue<double> Resistance
        {
            get
            {
                return resistance;
            }
        }

        private bool? isValid = null;

        /// <summary>
        /// 
        /// </summary>
        public bool? IsValid
        {
            get { return isValid; }
            set
            {
                if (isValid != value)
                {
                    isValid = value;
                    OnPropertyChanged("IsValid");
                }
            }
        }

        //private void UpdateResistance()
        //{
        //    double temp = Impedance.Real * Impedance.Real + Impedance.Imaginary * Impedance.Imaginary;
        //    Resistance.Value = temp / Impedance.Real;
        //}

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
