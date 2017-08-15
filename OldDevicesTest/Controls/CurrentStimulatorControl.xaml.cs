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
using NeuroSoft.Devices;
using System.Collections.ObjectModel;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for CurrentStimulatorControl.xaml
    /// </summary>
    public partial class CurrentStimulatorControl : UserControl, INotifyPropertyChanged
    {
        public CurrentStimulatorControl(ICurrentStimulator currentStimulator, NSDevice device)
        {
            InitializeComponent();
            CurrentStimulator = currentStimulator;

            foreach (StimulusPolarity stimPolar in CurrentStimulator.GetAvailablePolarities())
                StimulsPolarity.Add(stimPolar.ToString());
            StimulationModes.Add(Enum.GetNames(typeof(StimulationModeEnum))[0]);
            StimulForms.Add(Enum.GetNames(typeof(CurrentStimulusForm))[0]);
            //foreach (string stimMode in Enum.GetNames(typeof(StimulationModeEnum)))
            //    StimulationModes.Add(stimMode);
            //foreach (string stimForm in Enum.GetNames(typeof(CurrentStimulusForm)))
            //    StimulForms.Add(stimForm);

            Device = device;

            Duration = CurrentStimulator.GetStimulusDuration() * 1000;
            Period = 1.0 / CurrentStimulator.GetFrequency();
            Amplitude = CurrentStimulator.GetStimulusAmplitude() * 1000;
            CurrentStimPolarity = CurrentStimulator.GetPolarity().ToString();
            CurrentStimulationMode = CurrentStimulator.GetStimulationMode().ToString();
            CurrentStimulForm = CurrentStimulator.GetStimulusForm().ToString();
            DataContext = this;
        }
        /// <summary>
        /// Экземпляр интерфейса токового тсимулятора
        /// </summary>
        private ICurrentStimulator CurrentStimulator;
        /// <summary>
        /// Экземпляр устройства
        /// </summary>
        private NSDevice Device;

        //private double duration;
        /// <summary>
        /// Длительность стимула стимулятора (мс)
        /// </summary>
        public double Duration
        {
            get { return CurrentStimulator.GetStimulusDuration() * 1000.0; }
            set
            {
                CurrentStimulator.SetStimulusDuration((float)(value / 1000.0));
                OnPropertyChanged("Duration");
            }
        }

        //private double period;
        /// <summary>
        /// Период стимуляции (с)
        /// </summary>
        public double Period
        {
            get { return 1.0 / CurrentStimulator.GetFrequency(); }
            set
            {
                CurrentStimulator.SetFrequency((float)(1.0 / value));
                OnPropertyChanged("Period");
            }
        }

        //private double amplitude;
        /// <summary>
        /// Амплитуда стимулов (мА)
        /// </summary>
        public double Amplitude
        {
            get { return CurrentStimulator.GetStimulusAmplitude() * 1000; }
            set
            {
                CurrentStimulator.SetStimulusAmplitude((float)(value / 1000.0));
                OnPropertyChanged("Amplitude");
            }
        }

        private ObservableCollection<string> stimulsPolarity = new ObservableCollection<string>();
        /// <summary>
        /// Возможные полярности стимула
        /// </summary>
        public ObservableCollection<string> StimulsPolarity
        {
            get { return stimulsPolarity; }
        }
        /// <summary>
        /// Текущая полярность стимула
        /// </summary>
        //private StimulusPolarity currentStimPolarity;
        /// <summary>
        /// Свойство привязки для текущей полярности импульса
        /// </summary>
        public string CurrentStimPolarity
        {
            get { return CurrentStimulator.GetPolarity().ToString(); }
            set
            {
                CurrentStimulator.SetPolarity((StimulusPolarity)Enum.Parse(typeof(StimulusPolarity), value));
                OnPropertyChanged("CurrentStimPolarity");
            }
        }

        private ObservableCollection<string> stimulationModes = new ObservableCollection<string>();
        /// <summary>
        /// Возможные типы стимула
        /// </summary>
        public ObservableCollection<string> StimulationModes
        {
            get { return stimulationModes; }
        }
        /// <summary>
        /// Текущий тип стимула
        /// </summary>
        //private StimulationModeEnum currentStimulationMode;
        /// <summary>
        /// Свойство привязки для текущего режима стимуляции
        /// </summary>
        public string CurrentStimulationMode
        {
            get { return CurrentStimulator.GetStimulationMode().ToString(); }
            set
            {
                CurrentStimulator.SetStimulationMode((StimulationModeEnum)Enum.Parse(typeof(StimulationModeEnum), value));
                OnPropertyChanged("CurrentStimulationMode");
            }
        }

        private ObservableCollection<string> stimulForms = new ObservableCollection<string>();
        /// <summary>
        /// Возможные формы стимула
        /// </summary>
        public ObservableCollection<string> StimulForms
        {
            get { return stimulForms; }
        }
        /// <summary>
        /// Текущая форма стимула
        /// </summary>
        //private CurrentStimulusForm currentStimForm;

        public string CurrentStimulForm
        {
            get { return CurrentStimulator.GetStimulusForm().ToString(); }
            set
            {
                CurrentStimulator.SetStimulusForm((CurrentStimulusForm)Enum.Parse(typeof(CurrentStimulusForm), value));
                OnPropertyChanged("CurrentStimulForm");
            }
        }

        private bool stimulationIsValid = false;
        /// <summary>
        /// Признак запуска стимуляции
        /// </summary>
        public bool StimulationIsValid
        {
            get { return stimulationIsValid; }
            set
            {
                stimulationIsValid = value;
                //if (value)
                    OnPropertyChanged("StimulationIsValid");
            }
        }
        /// <summary>
        /// Строки с описанием запуска стимуляции
        /// </summary>
        //private string[] StimulationRunning = {"Стимуляция отсутствует", "Стимуляция запущена"};
        /// <summary>
        /// Строки с описанием состояния стимулов
        /// </summary>
        //private string[] StimulStates = { "Нормальный стимул", "Параметры стимула не соответствуют заданным", "Обрыв электрода", "Строка с реальными параметрами стимула", "Короткое замыкание" };

        private string stimulationState;
        /// <summary>
        /// Строка, возвращающая состояние стимуляции
        /// </summary>
        public string StimulationState
        {
            get { return stimulationState; }
            set 
            { 
                stimulationState = value;
                OnPropertyChanged("StimulationState");
            }
        }
        /// <summary>
        /// Запускает стимуляцию
        /// </summary>
        public void StartStimulation()
        {
            Device.DeviceError += new DeviceErrorDelegate(CurrentStimError);
            if (Device is NeuroMEPMicro)
                (Device as NeuroMEPMicro).SetWorkMode(NeuroMEPMicroWorkMode.Kalibrovka);
            if (Device is EEG5Device)
                (Device as EEG5Device).SetWorkMode(EEGWorkMode.Kalibrovka);
            Device.BeginTransmit();
            CurrentStimulator.SetEnabled(true);
            CurrentStimulator.StartStimulation();
            StimulationState = "Стимуляция запущена.";
            StimulationIsValid = true;
        }
        /// <summary>
        /// Останавливает стимуляцию
        /// </summary>
        public void StopStimulation()
        {
            Device.StopTransmit();
            CurrentStimulator.StopStimulation();
            CurrentStimulator.SetEnabled(false);
            Device.DeviceError -= new DeviceErrorDelegate(CurrentStimError);
        }

        /// <summary>
        /// Обработчик сообщений об ошибках с устройства
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="errorArgs"></param>
        private void CurrentStimError(object sender, DeviceErrorArgs errorArgs)
        {
            if (errorArgs.Error > 1000 && errorArgs.Error <= 1005)
            {
                StimulationState = StimulationState.Remove(StimulationState.LastIndexOf('.')) + "." + errorArgs.Text.Substring(errorArgs.Text.LastIndexOf('\n'));
                if (errorArgs.Error == 1002 || errorArgs.Error == 1003 || errorArgs.Error == 1005)
                    StimulationIsValid = false;
                else
                    StimulationIsValid = true;
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    ApplySettingsAndStartStimulation();
        //}

        /// <summary>
        /// Применяет настройки и запускает стимуляцию
        /// </summary>
        //public void ApplySettingsAndStartStimulation()
        //{
        //    CurrentStimulator.SetFrequency((float)(1.0 / Period));
        //    CurrentStimulator.SetStimulusDuration((float)(Duration / 1000.0));
        //    CurrentStimulator.SetStimulusAmplitude((float)(Amplitude / 1000.0));
        //    CurrentStimulator.SetStimulationMode(currentStimulationMode);
        //    CurrentStimulator.SetStimulusForm(currentStimForm);
        //    CurrentStimulator.SetPolarity(currentStimPolarity);
        //    CurrentStimulator.SetEnabled(true);
        //    CurrentStimulator.StartStimulation();
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
