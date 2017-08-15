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
using System.ComponentModel;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using System.Collections.ObjectModel;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.EEG.EEGMontageMaker;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;
using NeuroSoft.DeviceAutoTest.ScriptExecution;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for NSDevicesImpedanceControl.xaml
    /// </summary>
    public partial class NSDevicesImpedanceControl : UserControl, IDisposable
    {
        public NSDevicesImpedanceControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        /// <summary>
        /// Конструктор для измерения импедансов энцефалографа
        /// </summary>
        /// <param name="device"> Ссылка на экземпляр утсройства </param>
        /// <param name="electrodeRanges"> Диапазон сопротивлений электродов </param>
        /// <param name="isEEGImpedance"> Измерение импедансов ведется по отведениям ЭЭГ</param>
        public NSDevicesImpedanceControl(EEG5Device device, RangedValue<double> electrodeRanges, bool isEEGImpedance, bool isImpedanceHandlerExt = false)
        {
            InitializeComponent();
            DataContext = this;
            this.device = device;
            this.electrodeRanges = electrodeRanges;
            this.isEEGImpedance = isEEGImpedance;
            if (!isImpedanceHandlerExt)
                (this.device as EEG5Device).DeviceImpedance += new DeviceImpedanceDelegate(device_DeviceImpedance);
        }

        /// <summary>
        /// Конструктор для измерения импедансов энцефалографов НС-1...НС-4
        /// </summary>
        /// <param name="device"> Ссылка на экземпляр утсройства </param>
        /// <param name="electrodeRanges"> Диапазон сопротивлений электродов </param>
        /// <param name="isEEGImpedance"> Измерение импедансов ведется по отведениям ЭЭГ</param>
        /// <param name="isImpedanceHandlerExt"> Флаг использования внешнего обработчика событий </param>
        public NSDevicesImpedanceControl(ScriptEnvironment environment, RangedValue<double> electrodeRanges, bool isEEGImpedance, bool isImpedanceHandlerExt = false)
        {
            InitializeComponent();
            DataContext = this;
            this.environment = environment;
            this.device = this.environment.Device as EEG4Device;
            this.electrodeRanges = electrodeRanges;
            this.isEEGImpedance = isEEGImpedance;
            if (!isImpedanceHandlerExt)
                (this.device as EEG4Device).DeviceImpedance += new DeviceImpedanceDelegate(device_DeviceImpedance);
        }

        /// <summary>
        /// Конструктор для измерения импедансов НейроМЕП-Микро
        /// </summary>
        /// <param name="device"> Ссылка на экземпляр утсройства </param>
        /// <param name="electrodeRanges"> Диапазон сопротивлений электродов </param>
        public NSDevicesImpedanceControl(NeuroMEPMicro device, RangedValue<double> electrodeRanges, DeviceImpedanceDelegate MEPimpCallBack)
        {
            InitializeComponent();
            DataContext = this;
            this.device = device;
            this.electrodeRanges = electrodeRanges;
            Impedances.Add(new ImpedanceInfo(0, "Ground"));
            for (int i = 0; i < (this.device as NeuroMEPMicro).GetChannelsCount(); i++)
            {
                Impedances.Add(new ImpedanceInfo(i * 2 + 1, Properties.Resource1.Electrode + " " + (i + 1) + "+", this.electrodeRanges.MinValue, this.electrodeRanges.MaxValue));
                Impedances.Add(new ImpedanceInfo(i * 2 + 2, Properties.Resource1.Electrode + " " + (i + 1) + "-", this.electrodeRanges.MinValue, this.electrodeRanges.MaxValue));
            }
            foreach (ImpedanceInfo impInfo in Impedances)
                SavedImpedances.Add(impInfo);
            if (MEPimpCallBack != null)
                (this.device as NeuroMEPMicro).DeviceImpedance += new DeviceImpedanceDelegate(MEPimpCallBack);
            else
                (this.device as NeuroMEPMicro).DeviceImpedance += new DeviceImpedanceDelegate(device_AmplifierImpedanceEventNew);
        }

        private const uint polyChannelsNum = 4;
        private RangedValue<double> electrodeRanges;
        private bool isEEGImpedance;
        private ScriptEnvironment environment;

        private List<ImpedanceInfo> impedances = new List<ImpedanceInfo>();
        /// <summary>
        /// Коллекция импедансов
        /// </summary>
        public List<ImpedanceInfo> Impedances
        {
            get { return impedances; }
        }

        private ObservableCollection<ImpedanceInfo> savedImpedances = new ObservableCollection<ImpedanceInfo>();
        /// <summary>
        /// Коллекция сохраненных импедансов
        /// </summary>
        public ObservableCollection<ImpedanceInfo> SavedImpedances
        {
            get { return savedImpedances; }
        }

        private NSDevice device;
        /// <summary>
        /// Ссылка на экземпляр устройства
        /// </summary>
        public NSDevice Device
        {
            get { return device; }
            protected set { device = value; }
        }
        /// <summary>
        /// Соответствие импедансов требованиям
        /// </summary>
        public bool ValidImpedances
        {
            get
            {
                foreach (ImpedanceInfo impInf in SavedImpedances)
                {
                    if (impInf.Resistance.IsValidValue != true)
                    {
                        CommonScripts.ShowError(String.Format(Properties.Resource1.ElectrodeImpInvalidMessage, impInf.Title));
                        return false;
                    }
                }
                return true;
            }
        }

        private bool started = false;

        /// <summary>
        /// Запуск процесса считывания импеданса
        /// </summary>
        public void Start()
        {
            if (device is NeuroMEPMicro)
            {
                (this.device as NeuroMEPMicro).SetWorkMode(NeuroMEPMicroWorkMode.ImpedanceTransmit);
                (this.device as NeuroMEPMicro).BeginTransmitImpedance();
            }
            if (device is EEG5Device)
            {
                ((this.device as EEG5Device) as IDeviceImpedance).Start();
            }
            if (device is EEG4Device)
            {
                ((this.device as EEG4Device) as IDeviceImpedance).Start();
            }
            started = true;
        }
        /// <summary>
        /// Запуск процесса считывания импеданса с указанием обработчика
        /// </summary>
        /// <param name="MEPimpCallBack"></param>
        public void Start(DeviceImpedanceDelegate ExternCallBack, NSDevice device)
        {
            if (Device == null)
            {
                this.device = device;
            }
            if (Device is NeuroMEPMicro)
                (device as NeuroMEPMicro).DeviceImpedance += new DeviceImpedanceDelegate(ExternCallBack);
            if (Device is EEG5Device)
            {
                (device as EEG5Device).DeviceImpedance += new DeviceImpedanceDelegate(ExternCallBack);
            }
            if (Device is EEG4Device && EEG4Scripts.GetDeviceType(Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4)
                (device as EEG4Device).DeviceImpedance += new DeviceImpedanceDelegate(ExternCallBack);

            ExternImpedanceHandler = ExternCallBack;
            Start();
        }

        /// <summary>
        /// Делегат внешнего обработчика событий измерения импеданса
        /// </summary>
        private DeviceImpedanceDelegate ExternImpedanceHandler;

        public void Stop()
        {
            if (!started)
                return;
            var device = Device;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (device != null)
                {
                    if (device is NeuroMEPMicro)
                    {
                        (device as NeuroMEPMicro).StopTransmitImpedance();
                        (device as NeuroMEPMicro).SetWorkMode(NeuroMEPMicroWorkMode.VPTransmit);
                        if (ExternImpedanceHandler != null)
                            (device as NeuroMEPMicro).DeviceImpedance -= new DeviceImpedanceDelegate(ExternImpedanceHandler);
                        else
                            (device as NeuroMEPMicro).DeviceImpedance -= new DeviceImpedanceDelegate(device_AmplifierImpedanceEventNew);
                        started = false;
                    }
                    if (device is EEG5Device)
                    {
                        ((device as EEG5Device) as IDeviceImpedance).Stop();
                        (device as EEG5Device).SetWorkMode(EEGWorkMode.EEGTransmit);
                        if (ExternImpedanceHandler != null)
                            (device as EEG5Device).DeviceImpedance -= new DeviceImpedanceDelegate(ExternImpedanceHandler);
                        else
                            (device as EEG5Device).DeviceImpedance -= new DeviceImpedanceDelegate(device_DeviceImpedance);
                        started = false;
                    }
                    if (device is EEG4Device)
                    {
                        ((device as EEG4Device) as IDeviceImpedance).Stop();
                        (device as EEG4Device).SetWorkMode(EEGWorkMode.EEGTransmit);
                        if (ExternImpedanceHandler != null)
                            (device as EEG4Device).DeviceImpedance -= new DeviceImpedanceDelegate(ExternImpedanceHandler);
                        else
                            (device as EEG4Device).DeviceImpedance -= new DeviceImpedanceDelegate(device_DeviceImpedance);
                        started = false;
                    }
                }
            })); 
        }

        /// <summary>
        /// Обработчик события измерения импеданса от энцефалографа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void device_DeviceImpedance(object sender, DeviceImpedanceArgs e)
        {
            MontageHead currentMontage = (e.State as EEG4ImpedanceState).Impedance.Montage;
            if (Impedances.Count == 0)
            {
                InitImpedancesCollection(currentMontage);
            }
            int indx = 0;
            if (isEEGImpedance)
            {
                for (int i = 0; i < currentMontage.Electrods.Length; i++)
                {
                    if (currentMontage.Electrods[i].Name == SavedImpedances[indx].Title)
                        SavedImpedances[indx++].Resistance.Value = currentMontage.Electrods[i].IpmedanceValue * 1e3;
                }
                if (Device is EEG5Device && !((Device as EEG5Device).GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP)
                {
                    for (int i = 0; i < currentMontage.ExtraEEGChannels.Length; i++)
                    {
                        SavedImpedances[indx++].Resistance.Value = currentMontage.ExtraEEGChannels[i].IpmedanceValue * 1e3;
                    }
                }
                for (int i = 0; i < currentMontage.OtheChannels.Length; i++)
                {
                    if (indx < SavedImpedances.Count && currentMontage.OtheChannels[i].Name == SavedImpedances[indx].Title)
                        SavedImpedances[indx++].Resistance.Value = currentMontage.OtheChannels[i].Electrod.IpmedanceValue * 1e3;
                }
            }
            else
            {
                for (int i = 0; i < polyChannelsNum; i++)   // было montage.PolyChannels.Length, потом оно увеличилось до 8 и задал жестко polyChannelsNum = 4
                {
                    SavedImpedances[indx++].Resistance.Value = currentMontage.PolyChannels[i].Plus.IpmedanceValue * 1e3;
                    SavedImpedances[indx++].Resistance.Value = currentMontage.PolyChannels[i].Minus.IpmedanceValue * 1e3;
                    if (Device is EEG4Device)
                        if (EEG4Scripts.GetDeviceType(Device as EEG4Device) != EEG4Scripts.NeuronSpectrumTypes.NS_4P
                            && EEG4Scripts.GetDeviceType(Device as EEG4Device) != EEG4Scripts.NeuronSpectrumTypes.NS_4EP
                            && EEG4Scripts.GetDeviceType(Device as EEG4Device) != EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4)
                            break;
                }
                for (int i = 0; i < currentMontage.OtheChannels.Length; i++)
                {
                    if (indx < SavedImpedances.Count && currentMontage.OtheChannels[i].Name == SavedImpedances[indx].Title)
                        SavedImpedances[indx++].Resistance.Value = currentMontage.OtheChannels[i].Electrod.IpmedanceValue * 1e3;
                }
            }
        }

        /// <summary>
        /// Обработчик события измерения импеданса для НейроМВП-Микро.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void device_AmplifierImpedanceEventNew(object sender, DeviceImpedanceArgs e)
        {
            var impedances = (e.State as NeuroMEPAmplifierEEGImpedanceState).ImpedanceChannelsStates;
            if (impedances == null)
                return;
            Impedances[0].Resistance.Value = ((impedances[0].Ground + impedances[0].Ground) / 2) * 1e3;
            foreach (var impedance in impedances)
            {
                Impedances[(int)impedance.ChannelNum * 2 + 1].Resistance.Value = impedance.BipolarPlus * 1e3;
                Impedances[(int)impedance.ChannelNum * 2 + 2].Resistance.Value = impedance.BipolarMinus * 1e3;
            }
            foreach (ImpedanceInfo impInfo in Impedances)
                SavedImpedances[impInfo.Index].Resistance.Value = impInfo.Resistance.Value;
        }

        /// <summary>
        /// Инициализирует коллекцию с импедансами
        /// </summary>
        /// <param name="currentMontage"> Текущий монтаж электродов </param>
        private void InitImpedancesCollection(MontageHead currentMontage)
        {
            if (isEEGImpedance)
            {
                for (int i = 0; i < currentMontage.Electrods.Length; i++)
                {
                    if (Device is EEG4Device && EEG4Scripts.GetDeviceType(Device as EEG4Device) != EEG4Scripts.NeuronSpectrumTypes.NS_4P &&
                        EEG4Scripts.GetDeviceType(Device as EEG4Device) != EEG4Scripts.NeuronSpectrumTypes.NS_4EP)
                    {
                        if (EEG4Scripts.SortLedNames(environment, Enum.GetNames(typeof(EEG4LedsStates)), true).Contains(currentMontage.Electrods[i].Name))
                            Impedances.Add(new ImpedanceInfo(Impedances.Count + 1, currentMontage.Electrods[i].Name, electrodeRanges.MinValue, electrodeRanges.MaxValue));
                    }
                    else
                        if (currentMontage.Electrods[i].Impedance != ImpedanceStatus.None)
                            Impedances.Add(new ImpedanceInfo(Impedances.Count + 1, currentMontage.Electrods[i].Name, electrodeRanges.MinValue, electrodeRanges.MaxValue));
                }
                if (Device is EEG5Device && !((Device as EEG5Device).GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP)
                {
                    for (int i = 0; i < currentMontage.ExtraEEGChannels.Length; i++)
                    {
                        Impedances.Add(new ImpedanceInfo(Impedances.Count + i, currentMontage.ExtraEEGChannels[i].Name,
                                                                                   electrodeRanges.MinValue, electrodeRanges.MaxValue));
                    }
                }
                for (int i = 0; i < currentMontage.OtheChannels.Length; i++)
                {
                    if (currentMontage.OtheChannels[i].Electrod.Impedance != ImpedanceStatus.None)
                    {
                        if (currentMontage.OtheChannels[i].Name == Properties.Resource1.ECG)
                            Impedances.Add(new ImpedanceInfo(Impedances.Count + i, currentMontage.OtheChannels[i].Name,
                                                             electrodeRanges.MinValue, electrodeRanges.MaxValue));
                        else
                            Impedances.Add(new ImpedanceInfo(Impedances.Count + i, currentMontage.OtheChannels[i].Name));
                    }
                }
            }
            else
            {
                for (int i = 0; i < polyChannelsNum; i++)   // было montage.PolyChannels.Length, потом оно увеличилось до 8 и задал жестко polyChannelsNum = 4
                {
                    Impedances.Add(new ImpedanceInfo(Impedances.Count + i * 2, currentMontage.PolyChannels[i].Name + currentMontage.PolyChannels[i].Plus.Name,
                                                     electrodeRanges.MinValue, electrodeRanges.MaxValue));
                    Impedances.Add(new ImpedanceInfo(Impedances.Count + 1 + i * 2, currentMontage.PolyChannels[i].Name + currentMontage.PolyChannels[i].Minus.Name,
                                                     electrodeRanges.MinValue, electrodeRanges.MaxValue));
                    if (Device is EEG4Device)
                        if (EEG4Scripts.GetDeviceType(Device as EEG4Device) != EEG4Scripts.NeuronSpectrumTypes.NS_4P
                            && EEG4Scripts.GetDeviceType(Device as EEG4Device) != EEG4Scripts.NeuronSpectrumTypes.NS_4EP
                            && EEG4Scripts.GetDeviceType(Device as EEG4Device) != EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4)
                            break;
                }
                for (int i = 0; i < currentMontage.OtheChannels.Length; i++)
                {
                    if (currentMontage.OtheChannels[i].Electrod.Impedance != ImpedanceStatus.None)
                    {
                        if (currentMontage.OtheChannels[i].Name == Properties.Resource1.GND)
                            Impedances.Add(new ImpedanceInfo(Impedances.Count + i, /*"Электрод " + */currentMontage.OtheChannels[i].Name));
                    }
                }
            }
            foreach (ImpedanceInfo impInfo in Impedances)
                SavedImpedances.Add(impInfo);
        }

        public void Dispose()
        {

        }
    }

    /// <summary>
    /// Класс, содержащий информацию об импедансе
    /// </summary>
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
            resistance = new RangedValue<double>(double.NaN, minResistance, maxResistance);
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
