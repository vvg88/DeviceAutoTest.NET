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
using NeuroSoft.Devices;
using System.Collections.ObjectModel;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.EEG.EEGMontageMaker;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for EEGPolarisationControl.xaml
    /// </summary>
    public partial class EEGPolarisationControl : UserControl
    {
        public EEGPolarisationControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public EEGPolarisationControl(EEG5Device device, RangedValue<double> polarisationRanges)
        {
            InitializeComponent();
            DataContext = this;
            this.device = device;
            this.polarisationRanges = polarisationRanges;
            //this.gndRange = gndRange;
            //this.isEEGImpedance = isEEGImpedance;
            //\\System.Windows.Forms.UserControl userImpControl = (device as IDeviceImpedance).GetControl();
            //if (!isImpedanceHandlerExt)
                (this.device as EEG5Device).DeviceImpedance += new DeviceImpedanceDelegate(device_DeviceImpedance);
            (this.device as EEG5Device).SetWorkMode(EEGWorkMode.ImpedanceTransmit);
            //InitImpedancesCollection(device.GetMontage());
            device.BeginTransmitImpedance();
            started = true;
        }

        #region Properties

        private ObservableCollection<PolarisationInfo> polarisations = new ObservableCollection<PolarisationInfo>();
        /// <summary>
        /// Коллекция сохраненных импедансов
        /// </summary>
        public ObservableCollection<PolarisationInfo> Polarisations
        {
            get { return polarisations; }
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
        /// Соответствие значений напряжения поляризации требованиям
        /// </summary>
        public bool ValidPolarisations
        {
            get
            {
                foreach (PolarisationInfo impInf in Polarisations)
                {
                    if (impInf.Polarisation.IsValidValue != true)
                    {
                        CommonScripts.ShowError(String.Format(Properties.Resource1.ElectrodePolarisationInvalidMessage, impInf.Title));
                        return false;
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// Соответствие разницы значений напряжения поляризации требованиям
        /// </summary>
        public bool ValidPolarisDiff
        {
            get
            {
                foreach (PolarisationInfo polarInfo in Polarisations)
                {
                    if (polarInfo.IsBigDifference)
                    {
                        CommonScripts.ShowError(String.Format(Properties.Resource1.ElectrodePolarDiffInvalidMessage, polarInfo.Title));
                        return false;
                    }
                }
                return true;
            }
        }

        private RangedValue<double> polarisationRanges;
        private bool started = false;

        #endregion

        #region Methods
        /// <summary>
        /// Останов чтения поляризации электродов
        /// </summary>
        public void Stop()
        {
            if (!started)
                return;
            var device = Device;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (device != null)
                {
                    if (device is EEG5Device)
                    {
                        (device as EEG5Device).StopTransmitImpedance();
                        (device as EEG5Device).SetWorkMode(EEGWorkMode.EEGTransmit);
                        (device as EEG5Device).DeviceImpedance -= new DeviceImpedanceDelegate(device_DeviceImpedance);
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
            
            int indx = 0;
            if (Polarisations.Count == 0)
                InitPolarisationsCollection(currentMontage);
            for (int i = 0; i < currentMontage.Electrods.Length; i++)
            {
                if (currentMontage.Electrods[i].Name == Polarisations[indx].Title)
                    Polarisations[indx++].Polarisation.Value = currentMontage.Electrods[i].PolarizationValue;
            }
            if (Device is EEG5Device && !((Device as EEG5Device).GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP)
            {
                for (int i = 0; i < currentMontage.ExtraEEGChannels.Length; i++)
                {
                    Polarisations[indx++].Polarisation.Value = currentMontage.ExtraEEGChannels[i].PolarizationValue;
                }
            }
            //for (int i = 0; i < currentMontage.PolyChannels.Length; i++)
            //{
            //    Polarisations[indx++].Polarisation.Value = currentMontage.PolyChannels[i].Plus.PolarizationValue;
            //    Polarisations[indx++].Polarisation.Value = currentMontage.PolyChannels[i].Minus.PolarizationValue;
            //}
            for (int i = 0; i < currentMontage.OtheChannels.Length; i++)
            {
                if (indx < Polarisations.Count && currentMontage.OtheChannels[i].Name == Polarisations[indx].Title)
                    Polarisations[indx++].Polarisation.Value = currentMontage.OtheChannels[i].Electrod.PolarizationValue;
            }
            CalcPolarisDiff();
        }

        /// <summary>
        /// Определяет разницу между значениями напряжения поляризации в массиве и устанавливает признак, что разница выше требования
        /// </summary>
        private void CalcPolarisDiff()
        {
            foreach (PolarisationInfo polarInfo in Polarisations)
            {
                foreach (PolarisationInfo polarisationInfo in Polarisations)
                {
                    if (polarisationInfo.Title != polarInfo.Title)
                    {
                        if (polarInfo.Polarisation.Value - polarisationInfo.Polarisation.Value > 2)
                        {
                            polarisationInfo.IsBigDifference = true;
                            //break;
                        }
                        else
                            polarisationInfo.IsBigDifference = false;
                    }
                }
            }
        }

        /// <summary>
        /// Инициализирует коллекцию с измерениями поляризации электродов
        /// </summary>
        /// <param name="currentMontage"></param>
        private void InitPolarisationsCollection(MontageHead currentMontage)
        {
            for (int i = 0; i < currentMontage.Electrods.Length; i++)
            {
                if (currentMontage.Electrods[i].Impedance != ImpedanceStatus.None)
                    Polarisations.Add(new PolarisationInfo(Polarisations.Count + 1, /*"Электрод " + */currentMontage.Electrods[i].Name, polarisationRanges.MinValue, polarisationRanges.MaxValue));
            }
            if (Device is EEG5Device && !((Device as EEG5Device).GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP)
            {
                for (int i = 0; i < currentMontage.ExtraEEGChannels.Length; i++)
                {
                    Polarisations.Add(new PolarisationInfo(Polarisations.Count + i, /*"Электрод " + */currentMontage.ExtraEEGChannels[i].Name,
                                                                               polarisationRanges.MinValue, polarisationRanges.MaxValue));
                }
            }
            //for (int i = 0; i < currentMontage.PolyChannels.Length; i++)
            //{
            //    Polarisations.Add(new PolarisationInfo(Polarisations.Count + i * 2, /*"Электрод " + */currentMontage.PolyChannels[i].Name + currentMontage.PolyChannels[i].Plus.Name,
            //                                     polarisationRanges.MinValue, polarisationRanges.MaxValue));
            //    Polarisations.Add(new PolarisationInfo(Polarisations.Count + 1 + i * 2, /*"Электрод " + */currentMontage.PolyChannels[i].Name + currentMontage.PolyChannels[i].Minus.Name,
            //                                     polarisationRanges.MinValue, polarisationRanges.MaxValue));
            //}
            for (int i = 0; i < currentMontage.OtheChannels.Length; i++)
            {
                if (currentMontage.OtheChannels[i].Electrod.Impedance != ImpedanceStatus.None)
                {
                    if (currentMontage.OtheChannels[i].Name == Properties.Resource1.ECG)
                        Polarisations.Add(new PolarisationInfo(Polarisations.Count + i, /*"Электрод " + */currentMontage.OtheChannels[i].Name,
                                                         polarisationRanges.MinValue, polarisationRanges.MaxValue));
                    else
                        Polarisations.Add(new PolarisationInfo(Polarisations.Count + i, /*"Электрод " + */currentMontage.OtheChannels[i].Name));
                }
            }
            //for (int i = 0; i < currentMontage.OtheChannels.Length; i++)
            //{
            //    if (currentMontage.OtheChannels[i].Electrod.Impedance != ImpedanceStatus.None)
            //    {
            //        if (currentMontage.OtheChannels[i].Name == Properties.Resource1.GND)
            //            Impedances.Add(new ImpedanceInfo(Impedances.Count + i, /*"Электрод " + */currentMontage.OtheChannels[i].Name));
            //    }
            //}
        }

        #endregion
    }

    /// <summary>
    /// Класс, содержащий информацию о поляризации электродов
    /// </summary>
    public class PolarisationInfo : INotifyPropertyChanged
    {
        public PolarisationInfo(int index, string title, double minPolaris, double maxPolaris)
        {
            polarisation = new RangedValue<double>(0, minPolaris, maxPolaris);
            Title = title;
            Index = index;
        }

        public PolarisationInfo(int index, string title)
        {
            polarisation = new RangedValue<double>(0, 0, 0) { IgnoreRange = true };
            Title = title;
            Index = index;
        }

        private int index;
        /// <summary>
        /// Индекс электрода
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

        private RangedValue<double> polarisation = null;
        /// <summary>
        /// Значение сопротивления электрода
        /// </summary>
        public RangedValue<double> Polarisation
        {
            get
            {
                return polarisation;
            }
        }

        private bool? isValid = null;

        /// <summary>
        /// Валидность значения поляризации
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

        private bool isBigDifference = false;
        /// <summary>
        /// Признак того, что напряжение поляризации данного электрода отличается более чем на 2 мВ от напряжения поляризации другого электрода
        /// </summary>
        public bool IsBigDifference
        {
            get { return isBigDifference; }
            set 
            {
                if (isBigDifference != value && !Polarisation.IgnoreRange)
                {
                    isBigDifference = value;
                    OnPropertyChanged("IsBigDifference");
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
