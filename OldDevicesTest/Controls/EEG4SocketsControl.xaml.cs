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
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.ComponentModel;
using NeuroSoft.EEG.EEGMontageMaker;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for EEG4SocketsControl.xaml
    /// </summary>
    public partial class EEG4SocketsControl : UserControl, INotifyPropertyChanged
    {
        public EEG4SocketsControl(ScriptEnvironment environment, RangedValue<double> electrodeRanges)
        {
            InitializeComponent();
            DataContext = this;
            this.environment = environment;
            this.device = environment.Device as EEG4Device;
            this.electrodeRanges = electrodeRanges;
            EEG4LedsPanel.Environment = environment;
            testingResults = new bool[EEG4LedsPanel.LedsColors.Count - 1];
            Impedances = new List<ImpedanceInfo>();
            sortedLedNames = EEG4Scripts.SortLedNames(environment, Enum.GetNames(typeof(EEG4LedsStates)), false);
        }

        #region Fields

        private RangedValue<double> electrodeRanges;
        /// <summary>
        /// Коллекция импедансов
        /// </summary>
        private List<ImpedanceInfo> Impedances;
        /// <summary>
        /// Номер проверяемого электрода
        /// </summary>
        private int currentElectrodeIndx = 0;
        /// <summary>
        /// Номер измерения
        /// </summary>
        private int measureNumber = 0;
        /// <summary>
        /// Имя проверяемого электрода из перечисления EEG5LedsStates
        /// Берет имя из перечисления EEG4LedsStates и возвращает соответствующее ему имя из
        /// коллекции Impedances.
        /// </summary>
        private string CurrentElectrodeName
        {
            get
            {
                string nameFromEnum = sortedLedNames[currentElectrodeIndx];
                if (nameFromEnum.Contains("_P"))
                    return nameFromEnum.Substring(0, 2) + "+";
                else
                    if (nameFromEnum.Contains("_M"))
                        return nameFromEnum.Substring(0, 2) + "-";
                    else
                        if (nameFromEnum.Contains("ZERO"))
                            return Properties.Resource1.GND;
                        else
                            return nameFromEnum;
            }
        }
        /// <summary>
        /// Массив с результатами проверок всех электродов
        /// </summary>
        private bool[] testingResults;
        
        /// <summary>
        /// Переменная окружения скрипта
        /// </summary>
        private ScriptEnvironment environment;

        /// <summary>
        /// Максимальный индекс проверяемого электрода в зависимости от тестируемого устройства
        /// </summary>
        private int MaxElectrodeIndx
        {
            get
            {
                switch (EEG4Scripts.GetDeviceType(environment))
                {
                    case EEG4Scripts.NeuronSpectrumTypes.NS_1:
                        return 12;
                    case EEG4Scripts.NeuronSpectrumTypes.NS_2:
                        return 20;
                    case EEG4Scripts.NeuronSpectrumTypes.NS_3:
                        return 23;
                    case EEG4Scripts.NeuronSpectrumTypes.NS_4:
                        return 25;
                    case EEG4Scripts.NeuronSpectrumTypes.NS_4P:
                    case EEG4Scripts.NeuronSpectrumTypes.NS_4EP:
                        return 31;
                    default:
                        return 0;
                }
            }
        }

        private string[] sortedLedNames;
        #endregion

        #region Properties

        private string testingCommand;
        /// <summary>
        /// Команда, что делать наладчику
        /// </summary>
        public string TestingCommand
        {
            get { return testingCommand; }
            set
            {
                testingCommand = value;
                OnPropertyChanged("TestingCommand");
            }
        }

        private string currentImpedance;
        /// <summary>
        /// Отборажает сопротивление проверяемого электрода
        /// </summary>
        public string CurrentImpedance
        {
            get { return currentImpedance; }
            set
            {
                currentImpedance = value;
                OnPropertyChanged("CurrentImpedance");
            }
        }

        private bool? testingResult;
        /// <summary>
        /// Результат тестирования
        /// </summary>
        public bool? TestingResult
        {
            get { return testingResult; }
            set
            {
                testingResult = value;
                OnPropertyChanged("TestingResult");
            }
        }

        private bool impedanceValid;
        /// <summary>
        /// Результат текущей проверки электрода
        /// </summary>
        public bool ImpedanceValid
        {
            get { return impedanceValid; }
            set
            {
                impedanceValid = value;
                OnPropertyChanged("ImpedanceValid");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Запуск измерения импеданса
        /// </summary>
        public void Start()
        {
            TestingCommand = Properties.Resource1.TestPreparing;
            device.DeviceImpedance += new DeviceImpedanceDelegate(device_DeviceImpedance);
            (device as IDeviceImpedance).Start();
        }

        /// <summary>
        /// Обработчи события измерения импеданса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void device_DeviceImpedance(object sender, DeviceImpedanceArgs e)
        {
            MontageHead currentMontage = (e.State as EEG4ImpedanceState).Impedance.Montage;
            if (Impedances.Count == 0)
            {
                LoadImpedancesCollection(Impedances, currentMontage);
            }
            UpdateImpedances(currentMontage);
            IndicateImpedances();
            ExecuteTest();
        }

        private void ExecuteTest()
        {
            if (currentElectrodeIndx <= MaxElectrodeIndx - 1 && measureNumber <= 30)
            {
                ImpedanceInfo impInfoItem = Impedances.FirstOrDefault(impInfo => impInfo.Title == CurrentElectrodeName);
                if (currentElectrodeIndx == 0 && measureNumber == 0)
                    TestingCommand = String.Format(Properties.Resource1.ConnectElectrode + " " + Properties.Resource1.ElectrodeToDeviceGnd, CurrentElectrodeName);
                CurrentImpedance = String.Format(Properties.Resource1.ImpedanceOfIs, CurrentElectrodeName, Math.Round(impInfoItem.Resistance.Value * 1e-3, 3));
                ImpedanceValid = impInfoItem.Resistance.IsValidValue;
                if (ImpedanceValid)
                {
                    testingResults[Impedances.IndexOf(impInfoItem)] = true;
                    ChangeElectrode();
                    measureNumber = 0;
                }
                else
                    measureNumber++;
            }
            else
            {
                FinishTesting();
            }
        }

        /// <summary>
        /// Изменяет текущий проверяемый электрод
        /// </summary>
        private void ChangeElectrode()
        {
            currentElectrodeIndx++;
            if (currentElectrodeIndx == MaxElectrodeIndx)
                return;
            string suffix = CurrentElectrodeName == Properties.Resource1.ECG ? Properties.Resource1.S_PluralEnd : "";
            TestingCommand = String.Format(Properties.Resource1.ConnectElectrode + suffix + " " + Properties.Resource1.ElectrodeToDeviceGnd, CurrentElectrodeName);
        }

        /// <summary>
        /// Проводит индикацию импеданса на светодиодном поле в программе
        /// </summary>
        private void IndicateImpedances()
        {
            string electrodeName;
            foreach (EEG4LedColor ledColor in EEG4LedsPanel.LedsColors.Values)
            {
                if (EEG4LedsPanel.IsNS4P == System.Windows.Visibility.Visible)
                {
                    if (ledColor.LedName.Contains("_P"))
                        electrodeName = (ledColor.LedName.Substring(0, 2) + "+");
                    else
                        if (ledColor.LedName.Contains("_M"))
                            electrodeName = (ledColor.LedName.Substring(0, 2) + "-");
                        else
                            if (ledColor.LedName.Contains("ZERO"))
                                electrodeName = Properties.Resource1.GND;
                            else
                                electrodeName = ledColor.LedName;
                }
                else
                {
                    if (ledColor.LedName.Contains("_P") && !ledColor.LedName.Contains("E4"))
                        electrodeName = ledColor.LedName.Substring(0, 1) + "1+";
                    else
                        if (ledColor.LedName.Contains("_M"))
                            electrodeName = ledColor.LedName.Substring(0, 1) + "1-";
                        else
                            if (ledColor.LedName.Contains("E4_P"))
                                electrodeName = Properties.Resource1.GND;
                            else
                                electrodeName = ledColor.LedName;
                }
                if (Impedances.FirstOrDefault(impInfo => impInfo.Title == electrodeName).Resistance.IsValidValue)
                    ledColor.LedBrush = Brushes.LightGreen;
                else
                    ledColor.LedBrush = Brushes.Red;
            }
            EEG4LedsPanel.SetLedLights();
        }
        /// <summary>
        /// Останавливает тестирование
        /// </summary>
        private void FinishTesting()
        {
            foreach (bool testResult in testingResults)
            {
                if (!testResult)
                {
                    TestingResult = testResult;
                    break;
                }
                TestingResult = true;
            }
            TestingCommand = Properties.Resource1.TestingFinished + " " + (TestingResult.Value ? Properties.Resource1.Successful + "." : Properties.Resource1.WithMistakes + ".");
            CurrentImpedance = "";
            foreach (EEG4LedColor ledColor in EEG4LedsPanel.LedsColors.Values)
                ledColor.LedBrush = Brushes.Transparent;
            (device as IDeviceImpedance).Stop();
            device.DeviceImpedance -= new DeviceImpedanceDelegate(device_DeviceImpedance);
        }

        /// <summary>
        /// Инициализирует коллекцию импедансов
        /// </summary>
        /// <param name="impedances"> Инициализируемая коллекция </param>
        /// <param name="montage"> Текущий монтаж </param>
        private void LoadImpedancesCollection(List<ImpedanceInfo> impedances, MontageHead montage)
        {
            var sortedLedNames = EEG4Scripts.SortLedNames(environment, Enum.GetNames(typeof(EEG4LedsStates)), true);
            for (int i = 0; i < montage.Electrods.Length; i++)
            {
                if (montage.Electrods[i].Impedance != ImpedanceStatus.None && sortedLedNames.Contains(montage.Electrods[i].Name))
                    impedances.Add(new ImpedanceInfo(impedances.Count + 1, montage.Electrods[i].Name, electrodeRanges.MinValue, electrodeRanges.MaxValue));
            }
            for (int i = 0; i < montage.PolyChannels.Length; i++)
            {
                impedances.Add(new ImpedanceInfo(impedances.Count + i * 2, montage.PolyChannels[i].Name + montage.PolyChannels[i].Plus.Name,
                                                                           electrodeRanges.MinValue, electrodeRanges.MaxValue));
                impedances.Add(new ImpedanceInfo(impedances.Count + 1 + i * 2, montage.PolyChannels[i].Name + montage.PolyChannels[i].Minus.Name,
                                                                           electrodeRanges.MinValue, electrodeRanges.MaxValue));
            }
            for (int i = 0; i < montage.OtheChannels.Length; i++)
            {
                if (montage.OtheChannels[i].Electrod.Impedance != ImpedanceStatus.None)
                {
                    impedances.Add(new ImpedanceInfo(impedances.Count + i, montage.OtheChannels[i].Name,
                                                                                   electrodeRanges.MinValue, electrodeRanges.MaxValue));
                }
            }
        }

        /// <summary>
        /// Обновляет значение импедансов в коллекции
        /// </summary>
        /// <param name="montage"></param>
        private void UpdateImpedances(MontageHead montage)
        {
            int indx = 0;
            for (int i = 0; i < montage.Electrods.Length; i++)
            {
                if (montage.Electrods[i].Name == Impedances[indx].Title)
                    Impedances[indx++].Resistance.Value = montage.Electrods[i].IpmedanceValue * 1e3;
            }
            for (int i = 0; i < montage.PolyChannels.Length; i++)
            {
                Impedances[indx++].Resistance.Value = montage.PolyChannels[i].Plus.IpmedanceValue * 1e3;
                Impedances[indx++].Resistance.Value = montage.PolyChannels[i].Minus.IpmedanceValue * 1e3;
            }
            for (int i = 0; i < montage.OtheChannels.Length; i++)
            {
                if (indx < Impedances.Count && montage.OtheChannels[i].Name == Impedances[indx].Title)
                    Impedances[indx++].Resistance.Value = montage.OtheChannels[i].Electrod.IpmedanceValue * 1e3;
            }
        }

        #endregion

        private EEG4Device device;

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
}
