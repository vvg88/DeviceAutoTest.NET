using System;
using System.Windows;
using System.Windows.Controls;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using System.Windows.Threading;
using System.ComponentModel;
using NeuroSoft.EEG.EEGMontageMaker;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for ElectrodeImpedancesControl.xaml
    /// </summary>
    public partial class NSDeviceSolderingControl : UserControl, INotifyPropertyChanged
    {
        public NSDeviceSolderingControl(ScriptEnvironment environment, NSDevice device, double minImpValues, double maxImpValues)
        {
            InitializeComponent();
            Device = device;
            ChannelsCount = device.GetChannelsCount();
            Environment = environment;
            NSDevicesImpedancesControl.Impedances.Add(new ImpedanceInfo(0, "Ground"));
            for (int i = 0; i < ChannelsCount; i++)
            {
                NSDevicesImpedancesControl.Impedances.Add(new ImpedanceInfo(i * 2 + 1, "Электрод " + (i + 1) + "+", minImpValues, maxImpValues));
                NSDevicesImpedancesControl.Impedances.Add(new ImpedanceInfo(i * 2 + 2, "Электрод " + (i + 1) + "-", minImpValues, maxImpValues));
            }
            foreach (ImpedanceInfo impInfo in NSDevicesImpedancesControl.Impedances)
            {
                if (impInfo.Index == 0)
                    NSDevicesImpedancesControl.SavedImpedances.Add(new ImpedanceInfo(impInfo.Index, impInfo.Title));
                else
                    NSDevicesImpedancesControl.SavedImpedances.Add(new ImpedanceInfo(impInfo.Index, impInfo.Title, impInfo.Resistance.MinValue, impInfo.Resistance.MaxValue));
            }
            DataContext = this;
        }

        #region Properties

        private const uint polyChannelsNum = 4;

        private NSDevice device;

        public NSDevice Device
        {
            get { return device; }
            set { device = value; }
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
            get 
            {
                int chanCount = 0;
                if (device is NeuroMEPMicro)
                    chanCount = device.GetChannelsCount();
                if ((device is EEG5Device)
                    || (device is EEG4Device && EEG4Scripts.GetDeviceType(device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4))
                    chanCount = 4;
                return chanCount; 
            }
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
                if (CurrentElectrodeIndex < 0 || CurrentElectrodeIndex > NSDevicesImpedancesControl.SavedImpedances.Count - 1)
                    return null;
                return NSDevicesImpedancesControl.SavedImpedances[CurrentElectrodeIndex];                  
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
                for (int i = 1; i < NSDevicesImpedancesControl.SavedImpedances.Count; i++)
                {
                    var impedanceInfo = NSDevicesImpedancesControl.SavedImpedances[i];
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
        /// Запуск проверки распайки входных разъемов
        /// </summary>
        private void Start()
        {
            if (started)
                return;            
            if (NSDevicesImpedancesControl != null)
            {
                if (device is NeuroMEPMicro)
                    NSDevicesImpedancesControl.Start(device_AmplifierImpedanceEventNew, device);
                if (device is EEG5Device)
                    NSDevicesImpedancesControl.Start(device_AmplifierImpedanceEventNew, device);
                if (device is EEG4Device && EEG4Scripts.GetDeviceType(device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4)
                    NSDevicesImpedancesControl.Start(device_AmplifierImpedanceEventNew, device);
                started = true;
            }
        }

        private int testImpedanceLimit = 20;
        private int testImpedanceCount = 0;

        /// <summary>
        /// Обработчик события измерения импедансов от НейроМВП-Микро
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            bool isValid = impedanceInfo.Resistance.IsValidValue;
            if (isValid && testImpedanceCount > 3)
            {
                impedanceInfo.IsValid = isValid;
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
        /// Останов проверки распайки входных разъемов
        /// </summary>
        public void Stop()
        {
            if (!started)
                return;
            if (NSDevicesImpedancesControl != null)
            {
                NSDevicesImpedancesControl.Stop();
                started = false;
            }        
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
            foreach (ImpedanceInfo impInfo in NSDevicesImpedancesControl.SavedImpedances)
            {
                impInfo.Resistance.Value = double.NaN/*0.0*/;
            }
            StartTesting();
        }

        public void StartTesting()
        {
            if (Testing)
                return;
            foreach (var impedance in NSDevicesImpedancesControl.Impedances)
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
            StartTestSoldering?.Invoke(this, CurrentElectrodeIndex);
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
        /// 
        /// </summary>
        /// <param name="impedances"></param>
        public void UpdateImpedaces(NSDeviceImpedanceState impedanceState)
        {
            if (Device is NeuroMEPMicro)
            {
                var impedances = (impedanceState as NeuroMEPAmplifierEEGImpedanceState).ImpedanceChannelsStates;
                if (impedances == null)
                    return;
                NSDevicesImpedancesControl.Impedances[0].Resistance.Value = ((impedances[0].Ground + impedances[0].Ground) / 2) * 1e3;
                NSDevicesImpedancesControl.SavedImpedances[0] = NSDevicesImpedancesControl.Impedances[0];
                foreach (var impedance in impedances)
                {
                    NSDevicesImpedancesControl.Impedances[(int)impedance.ChannelNum * 2 + 1].Resistance.Value = impedance.BipolarPlus * 1e3;
                    NSDevicesImpedancesControl.Impedances[(int)impedance.ChannelNum * 2 + 2].Resistance.Value = impedance.BipolarMinus * 1e3;
                }
                NSDevicesImpedancesControl.SavedImpedances[currentElectrodeIndex].Resistance.Value = NSDevicesImpedancesControl.Impedances[currentElectrodeIndex].Resistance.Value;
            }
            if (Device is EEG5Device)
            {
                MontageHead currentMontage = (impedanceState as EEG4ImpedanceState).Impedance.Montage;
                int indx = 0;
                for (int i = 0; i < currentMontage.OtheChannels.Length; i++)
                {
                    if (indx < NSDevicesImpedancesControl.Impedances.Count && currentMontage.OtheChannels[i].Name == "Земля")
                    {
                        NSDevicesImpedancesControl.Impedances[indx].Resistance.Value = currentMontage.OtheChannels[i].Electrod.IpmedanceValue * 1e3;
                        NSDevicesImpedancesControl.SavedImpedances[indx].Resistance.Value = NSDevicesImpedancesControl.Impedances[indx++].Resistance.Value;
                    }
                }
                for (int i = 0; i < polyChannelsNum; i++)
                {
                    NSDevicesImpedancesControl.Impedances[indx++].Resistance.Value = currentMontage.PolyChannels[i].Plus.IpmedanceValue * 1e3;
                    NSDevicesImpedancesControl.Impedances[indx++].Resistance.Value = currentMontage.PolyChannels[i].Minus.IpmedanceValue * 1e3;
                }
                NSDevicesImpedancesControl.SavedImpedances[currentElectrodeIndex].Resistance.Value = NSDevicesImpedancesControl.Impedances[currentElectrodeIndex].Resistance.Value;
            }
            if (device is EEG4Device && EEG4Scripts.GetDeviceType(device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4)
            {
                MontageHead currentMontage = (impedanceState as EEG4ImpedanceState).Impedance.Montage;
                int indx = 0;
                NSDevicesImpedancesControl.Impedances[indx++].Resistance.Value = currentMontage.OtheChannels[0].Electrod.IpmedanceValue;
                for (int i = 0; i < currentMontage.PolyChannels.Length; i++)
                {
                    NSDevicesImpedancesControl.Impedances[indx++].Resistance.Value = currentMontage.PolyChannels[i].Plus.IpmedanceValue * 1e3;
                    NSDevicesImpedancesControl.Impedances[indx++].Resistance.Value = currentMontage.PolyChannels[i].Minus.IpmedanceValue * 1e3;
                }
                NSDevicesImpedancesControl.SavedImpedances[currentElectrodeIndex].Resistance.Value = NSDevicesImpedancesControl.Impedances[currentElectrodeIndex].Resistance.Value;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
