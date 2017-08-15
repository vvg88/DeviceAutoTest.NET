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
using NeuroSoft.EEG.EEGMontageMaker;
using System.ComponentModel;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for EEG5ImpedancesControl.xaml
    /// </summary>
    public partial class EEG5SocketsControl : UserControl, INotifyPropertyChanged
    {
        public EEG5SocketsControl(EEG5Device device, RangedValue<double> electrodeRanges)
        {
            InitializeComponent();
            DataContext = this;
            this.device = device;
            this.electrodeRanges = electrodeRanges;
            EEG5LedsPanel.Device = this.device;
            testingResults = new bool[EEG5LedsPanel.LedsColors.Count - 1];
            Impedances = new List<ImpedanceInfo>();
        }

        #region Fields

        private const uint polyChannelsNum = 4;

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
        /// Берет имя из перечисления EEG5LedsStates и возвращает соответствующее ему имя из
        /// коллекции Impedances.
        /// </summary>
        private string CurrentElectrodeName 
        { 
            get 
            {
                string nameFromEnum = EEG5Scripts.SortLedNames(Enum.GetNames(typeof(EEG5LedsStates)), (EEG5LedsPanel.isNS5 == Visibility.Visible), false)[currentElectrodeIndx];
                if (nameFromEnum.Contains("ECG"))
                    return Properties.Resource1.ECG;
                else
                    if (nameFromEnum.Contains("_PLUS"))
                        return nameFromEnum.Substring(0, 2) + "+";
                    else
                        if (nameFromEnum.Contains("_MINUS"))
                            return nameFromEnum.Substring(0, 2) + "-";
                        else
                            if (nameFromEnum.Contains("ZERO"))
                                return Properties.Resource1.GND;
                            else
                                if (nameFromEnum.Contains("REF"))
                                    return "Ref";
                                else
                                    return nameFromEnum;
            } 
        }
        /// <summary>
        /// Массив с результатами проверок всех электродов
        /// </summary>
        private bool[] testingResults;
        /// <summary>
        /// Флаг проверки разъемов земли
        /// </summary>
        private bool gndsAreTesting = true;
        /// <summary>
        /// Счетчик проверок разъемов земли прибора
        /// </summary>
        private int gndsTestsCounter;
        /// <summary>
        /// Флаг валидности сопротивления одного из контактов земли
        /// </summary>
        private bool isGndImpedanceValid = false;

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
            ((device as IDeviceImpedance) as IDeviceImpedance).Start();
            device.DeviceImpedance += new DeviceImpedanceDelegate(device_DeviceImpedance);
            gndsAreTesting = true;
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
            if (currentElectrodeIndx <= Impedances.Count - 1 && measureNumber <= 30)
            {
                if (gndsAreTesting)
                {
                    ImpedanceInfo impInfoItem = Impedances.FirstOrDefault(impInfo => impInfo.Title == "Ref");
                    if (gndsAreTesting && measureNumber == 0)
                        TestingCommand = String.Format(Properties.Resource1.ConnectRefTo, gndsTestsCounter + 1);
                    CurrentImpedance = String.Format(Properties.Resource1.ImpedanceOfIs, "Ref", Math.Round(impInfoItem.Resistance.Value * 1e-3, 3));
                    ImpedanceValid = impInfoItem.Resistance.IsValidValue;
                    if (ImpedanceValid)
                    {
                        if (!isGndImpedanceValid)
                        {
                            gndsTestsCounter++;
                            isGndImpedanceValid = true;
                            measureNumber = 0;
                        }
                        if (gndsTestsCounter == 5)
                        {
                            gndsAreTesting = false;
                            testingResults[Impedances.IndexOf(Impedances.FirstOrDefault(impInfo => impInfo.Title == Properties.Resource1.GND))] = true;
                        }
                    }
                    else
                    {
                        if (isGndImpedanceValid)
                            isGndImpedanceValid = false;
                        measureNumber++;
                    }
                }
                else
                {
                    ImpedanceInfo impInfoItem = Impedances.FirstOrDefault(impInfo => impInfo.Title == CurrentElectrodeName);
                    if (currentElectrodeIndx == 0 && measureNumber == 0)
                        TestingCommand = String.Format(Properties.Resource1.ConnectElectrode + Properties.Resource1.ElectrodeToDeviceGnd, CurrentElectrodeName);
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
            if (CurrentElectrodeName != Properties.Resource1.ECG)
                currentElectrodeIndx++;
            else
                currentElectrodeIndx += 2;
            if (currentElectrodeIndx == (EEG5LedsPanel.isNS5 == Visibility.Visible ? 45 : 45 - 11))
                return;
            string suffix = CurrentElectrodeName == Properties.Resource1.ECG ? Properties.Resource1.S_PluralEnd : "";
            TestingCommand = String.Format(Properties.Resource1.ConnectElectrode + suffix + Properties.Resource1.ElectrodeToDeviceGnd, CurrentElectrodeName);
        }

        /// <summary>
        /// Проводит индикацию импеданса на светодиодном поле в программе
        /// </summary>
        private void IndicateImpedances()
        {
            string electrodeName;
            foreach (EEGLedColor ledColor in EEG5LedsPanel.LedsColors.Values)
            {
                if (ledColor.LedName.Contains("ECG"))
                    electrodeName = Properties.Resource1.ECG;
                else
                    if (ledColor.LedName.Contains("_PLUS"))
                        electrodeName = ledColor.LedName.Substring(0, 2) + "+";
                    else
                        if (ledColor.LedName.Contains("_MINUS"))
                            electrodeName = ledColor.LedName.Substring(0, 2) + "-";
                        else
                            if (ledColor.LedName.Contains("ZERO"))
                                electrodeName = Properties.Resource1.GND;
                            else
                                if (ledColor.LedName.Contains("REF"))
                                    electrodeName = "Ref";
                                else
                                    electrodeName = ledColor.LedName;
                if (Impedances.FirstOrDefault(impInfo => impInfo.Title == electrodeName).Resistance.IsValidValue)
                    ledColor.LedBrush = Brushes.LightGreen;
                else
                    ledColor.LedBrush = Brushes.Red;
            }
            EEG5LedsPanel.SetLedLights();
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
            foreach (EEGLedColor ledColor in EEG5LedsPanel.LedsColors.Values)
                ledColor.LedBrush = Brushes.Transparent;
            (device as IDeviceImpedance).Stop();
            device.SetWorkMode(EEGWorkMode.EEGTransmit);
            device.DeviceImpedance -= new DeviceImpedanceDelegate(device_DeviceImpedance);
        }

        /// <summary>
        /// Инициализирует коллекцию импедансов
        /// </summary>
        /// <param name="impedances"> Инициализируемая коллекция </param>
        /// <param name="montage"> Текущий монтаж </param>
        private void LoadImpedancesCollection(List<ImpedanceInfo> impedances, MontageHead montage)
        {
            for (int i = 0; i < montage.Electrods.Length; i++)
            {
                if (montage.Electrods[i].Impedance != ImpedanceStatus.None)
                    impedances.Add(new ImpedanceInfo(impedances.Count + 1, montage.Electrods[i].Name, electrodeRanges.MinValue, electrodeRanges.MaxValue));
            }
            if (EEG5LedsPanel.isNS5 == Visibility.Visible)
            {
                for (int i = 0; i < montage.ExtraEEGChannels.Length; i++)
                {
                    impedances.Add(new ImpedanceInfo(impedances.Count + i, montage.ExtraEEGChannels[i].Name,
                                                                               electrodeRanges.MinValue, electrodeRanges.MaxValue));
                }
            }
            for (int i = 0; i < polyChannelsNum; i++)      // было montage.PolyChannels.Length, потом оно увеличилось до 8 и задал жестко polyChannelsNum = 4
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
            if (EEG5LedsPanel.isNS5 == Visibility.Visible)
            {
                for (int i = 0; i < montage.ExtraEEGChannels.Length; i++)
                {
                    Impedances[indx++].Resistance.Value = montage.ExtraEEGChannels[i].IpmedanceValue * 1e3;
                }
            }
            for (int i = 0; i < polyChannelsNum; i++)      // было montage.PolyChannels.Length, потом оно увеличилось до 8 и задал жестко polyChannelsNum = 4
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

        private EEG5Device device;

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
