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
using System.ComponentModel;
using NeuroSoft.Hardware.Devices;
using System.Numerics;
using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.Hardware.Common;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for ImpedancesSolderingControl.xaml
    /// </summary>
    public partial class ImpedancesSolderingControl : UserControl, IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public ImpedancesSolderingControl(ScriptEnvironment environment, NeuroMepBase device, double measureFreq, int channelsCount)
        {
            InitializeComponent();
            Device = device;
            ChannelsCount = channelsCount;
            Environment = environment;
            ImpedancesView.Impedances.Add(new ImpedanceInfo(0, "Ground", measureFreq));
            for (int i = 0; i < channelsCount; i++)
            {
                ImpedancesView.Impedances.Add(new ImpedanceInfo(i * 2 + 1, "Ch" + (i+1) + "+", measureFreq));
                ImpedancesView.Impedances.Add(new ImpedanceInfo(i * 2 + 2, "Ch" + (i+1) + "-", measureFreq));                
            }
            DataContext = this;
        }

        #region Properties
        private NeuroMepBase device;
        /// <summary>
        /// 
        /// </summary>
        public NeuroMepBase Device
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
                if (CurrentElectrodeIndex < 0 || CurrentElectrodeIndex > ImpedancesView.Impedances.Count - 1)
                    return null;
                return ImpedancesView.Impedances[CurrentElectrodeIndex];                  
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
                for (int i = 1; i < ImpedancesView.Impedances.Count; i++)
                {
                    var impedanceInfo = ImpedancesView.Impedances[i];
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
                if (!Device.Amplifier.ModuleIsOn)
                    Device.AmplifierModuleOn(errinfo => CommonScripts.ShowError(errinfo.Data.Descriptor.Message));
                Device.AmplifierImpedanceStart(device_AmplifierImpedanceEventNew, errInfo => CommonScripts.ShowError(errInfo.Data.Descriptor.Message));
                started = true;
            }
        }

        private int testImpedanceLimit = 20;
        private int testImpedanceCount = 0;

        void device_AmplifierImpedanceEventNew(AsyncActionArgs<AmplifierImpedanceComplexData> e)
        {            
            if (startTestSolderingPending || !testing)
                return;
            ImpedancesView.UpdateImpedaces(e.Data.Data.Select(imp => imp.Value));
            testImpedanceCount++;
            var impedanceInfo = CurrentImpedanceInfo;
            if (impedanceInfo == null)
            {
                FinishTesting();
                return;
            }
            bool isValid = impedanceInfo.Abs >= TestImpedanceRange.Min && impedanceInfo.Abs <= TestImpedanceRange.Max;
            if (isValid)
            {
                impedanceInfo.IsValid = isValid;
                CurrentElectrodeIndex++;
                StartTestElectrode();
            }
            else if (testImpedanceCount > testImpedanceLimit)
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
                    device.AmplifierImpedanceStop();
                    device.AmplifierPowerOff();
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
            StartTesting();
        }

        public void StartTesting()
        {
            if (Testing)
                return;
            foreach (var impedance in ImpedancesView.Impedances)
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
}
