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
using System.Collections.ObjectModel;
using NeuroSoft.Devices;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for PhonoStimulatorControl.xaml
    /// </summary>
    public partial class PhonoStimulatorControl : UserControl, INotifyPropertyChanged
    {
        public PhonoStimulatorControl(IAudioStimulator phonoStimulator)
        {
            InitializeComponent();
            PhonoStimulator = phonoStimulator;

            //foreach (string stimType in Enum.GetNames(typeof(StimulusPolarity)))
            foreach (var stimPolarity in phonoStimulator.GetAvailablePolarities()/*Enum.GetNames(typeof(StimulusPolarity))*/)
                StimulsPolarity.Add(stimPolarity.ToString());
            foreach (string stimType in Enum.GetNames(typeof(AudioStimulusType)))
                StimulTypes.Add(stimType);

            Duration = PhonoStimulator.GetStimulusDuration();
            Period = 1.0 / PhonoStimulator.GetFrequency();
            Amplitude = PhonoStimulator.GetStimulusAmplitude();
            FrequencyTone = PhonoStimulator.GetStimulusTone();
            CurrentStimPolarity = PhonoStimulator.GetPolarity().ToString();
            CurrentAudioStimType = PhonoStimulator.GetStimulusType().ToString()/*AudioStimulusType.Meander.ToString()*/;
            PhonoStimulator.SetEnabled(true);
            PhonoStimulator.StartStimulation();
            StimulationIsRun = true;
            DataContext = this;
        }

        private IAudioStimulator PhonoStimulator;

        //private double duration;
        /// <summary>
        /// Длительность стимула стимулятора
        /// </summary>
        public double Duration
        {
            get { return PhonoStimulator.GetStimulusDuration() * 1000.0; }
            set 
            { 
                //duration = value;
                PhonoStimulator.SetStimulusDuration((float)(value / 1000.0));
                OnPropertyChanged("Duration");
            }
        }

        //private double period;
        /// <summary>
        /// Период стимуляции
        /// </summary>
        public double Period
        {
            get { return 1.0 / PhonoStimulator.GetFrequency(); }
            set 
            { 
                //period = value;
                PhonoStimulator.SetFrequency((float)(1.0 / value));
                OnPropertyChanged("Period");
            }
        }

        //private double amplitude;
        /// <summary>
        /// Амплитуда стимулов
        /// </summary>
        public double Amplitude
        {
            get { return PhonoStimulator.GetStimulusAmplitude(); }
            set 
            { 
                //amplitude = value;
                PhonoStimulator.SetStimulusAmplitude((float)value);
                OnPropertyChanged("Amplitude");
            }
        }

        //private double frequencyTone;
        /// <summary>
        /// Частота тона стимула
        /// </summary>
        public double FrequencyTone
        {
            get { return PhonoStimulator.GetStimulusTone(); }
            set 
            { 
                //frequencyTone = value;
                PhonoStimulator.SetStimulusTone((float)value);
                OnPropertyChanged("FrequencyTone");
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

        public string CurrentStimPolarity
        {
            get { return PhonoStimulator.GetPolarity().ToString(); }
            set
            {
                //currentStimPolarity = (StimulusPolarity)Enum.Parse(typeof(StimulusPolarity), value);
                PhonoStimulator.SetPolarity((StimulusPolarity)Enum.Parse(typeof(StimulusPolarity), value));
                OnPropertyChanged("CurrentStimPolarity");
            }
        }

        private ObservableCollection<string> stimulTypes = new ObservableCollection<string>();
        /// <summary>
        /// Возможные типы стимула
        /// </summary>
        public ObservableCollection<string> StimulTypes
        {
            get { return stimulTypes; }
        }
        /// <summary>
        /// Текущий тип стимула
        /// </summary>
        //private AudioStimulusType currentAudioStimType;

        public string CurrentAudioStimType
        {
            get { return PhonoStimulator.GetStimulusType().ToString(); }
            set
            {
                //currentAudioStimType = (AudioStimulusType)Enum.Parse(typeof(AudioStimulusType), value);
                PhonoStimulator.SetStimulusType((AudioStimulusType)Enum.Parse(typeof(AudioStimulusType), value));
                OnPropertyChanged("CurrentAudioStimType");
            }
        }

        private bool leftSide;
        /// <summary>
        /// Левая сторона стимуляции
        /// </summary>
        public bool LeftSide
        {
            get { return leftSide; }
            set
            {
                if (leftSide != value)
                {
                    leftSide = value;
                    OnPropertyChanged("LeftSide");
                    if (value)
                    {
                        PhonoStimulator.SetStimulusSide(StimulusSide.Left);
                        RightSide = false;
                        BothSides = false;
                    }
                }
            }
        }
        private bool rightSide;
        /// <summary>
        /// Правая сторона стимуляции
        /// </summary>
        public bool RightSide
        {
            get { return rightSide; }
            set
            {
                if (rightSide != value)
                {
                    rightSide = value;
                    OnPropertyChanged("RightSide");
                    if (value)
                    {
                        PhonoStimulator.SetStimulusSide(StimulusSide.Right);
                        LeftSide = false;
                        BothSides = false;
                    }
                }
            }
        }
        private bool bothSides = true;
        /// <summary>
        /// Обе стороны стимуляции
        /// </summary>
        public bool BothSides
        {
            get { return bothSides; }
            set
            {
                if (bothSides != value)
                {
                    bothSides = value;
                    OnPropertyChanged("BothSides");
                    OnPropertyChanged("NotBothSides");
                    if (value)
                    {
                        PhonoStimulator.SetStimulusSide(StimulusSide.Both);
                        NoiseGain = 0.0f;
                        RightSide = false;
                        LeftSide = false;
                        IsNoise = false;
                    }
                }
            }
        }

        /// <summary>
        /// Признак, что выбрана правая или левая сторона стимуляции. Небходимо для установки шума в каналах, т.к. у НС-5 шум включается только
        /// в том канале, который сейчас не выбран. 
        /// </summary>
        public bool NotBothSides
        {
            get { return !BothSides; }
        }

        /// <summary>
        /// Флаг доступности включения шума
        /// </summary>
        public bool IsNoiseAvailable
        {
            get { return (PhonoStimulator is EEG5Device); }
        }

        /// <summary>
        /// Флаг включения шума
        /// </summary>
        public bool IsNoise
        {
            get 
            {
                if (IsNoiseAvailable)
                    return (PhonoStimulator.GetState().GetState(typeof(EEG5AudioStimulatorState)) as EEG5AudioStimulatorState).Noise;
                return false;
            }
            set
            {
                EEG5AudioStimulatorState currentPhonoState = (EEG5AudioStimulatorState)PhonoStimulator.GetState().GetState(typeof(EEG5AudioStimulatorState));
                if (currentPhonoState != null)
                {
                    if (currentPhonoState.Noise != value)
                    {
                        currentPhonoState.Noise = value;
                        PhonoStimulator.SetState(currentPhonoState);
                        OnPropertyChanged("IsNoise");
                    }
                }
            }
        }

        /// <summary>
        /// Усиление маскирующего шума
        /// </summary>
        public float NoiseGain
        {
            get 
            {
                if (IsNoiseAvailable)
                    return (PhonoStimulator.GetState().GetState(typeof(EEG5AudioStimulatorState)) as EEG5AudioStimulatorState).NoiseGain;
                return 0;
            }
            set
            {
                EEG5AudioStimulatorState currentPhonoState = (EEG5AudioStimulatorState)PhonoStimulator.GetState().GetState(typeof(EEG5AudioStimulatorState));
                if (currentPhonoState != null)
                {
                    if (currentPhonoState.NoiseGain != value)
                    {
                        currentPhonoState.NoiseGain = value;
                        currentPhonoState.MaskOnlyStimulation = false;      // To avoid noise turning off when change its gain
                        PhonoStimulator.SetState(currentPhonoState);
                        OnPropertyChanged("NoiseGain");
                    }
                }
            }
        }

        private bool stimulationIsRun = false;
        /// <summary>
        /// Признак запуска стимуляции
        /// </summary>
        public bool StimulationIsRun
        {
            get { return stimulationIsRun; }
            set
            {
                stimulationIsRun = value;
                OnPropertyChanged("StimulationIsRun");
            }
        }
        /// <summary>
        /// Флаг, указывающий, что проверяется прибор НС-1...НС-4
        /// </summary>
        public bool IsEEG4
        {
            get { return PhonoStimulator is EEG4Device; }
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

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    ApplySettingsAndStartStimulation();
        //}

        /// <summary>
        /// Применяет настройки и запускает стимуляцию
        /// </summary>
        public void ApplySettingsAndStartStimulation()
        {
            StimulusSide side = StimulusSide.Both;
            if (LeftSide)
                side = StimulusSide.Left;
            else if (RightSide)
                side = StimulusSide.Right;
            PhonoStimulator.SetStimulusSide(side);
            //FlashStimulator.SetStimulusCount(1000);
            PhonoStimulator.SetFrequency((float)(1.0 / Period));
            PhonoStimulator.SetStimulusDuration((float)(Duration / 1000.0));
            PhonoStimulator.SetStimulusAmplitude((float)Amplitude);
            PhonoStimulator.SetStimulusTone((float)FrequencyTone);
            //PhonoStimulator.SetStimulusType(currentAudioStimType);
            //PhonoStimulator.SetPolarity(currentStimPolarity);
            PhonoStimulator.SetEnabled(true);
            PhonoStimulator.StartStimulation();
            StimulationIsRun = true;
        }

        /// <summary>
        /// Останавливает стимуляцию
        /// </summary>
        public void StopStimulation()
        {
            PhonoStimulator.SetEnabled(false);
            PhonoStimulator.StopStimulation();
            StimulationIsRun = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (StimulationIsRun)
                StopStimulation();
            else
                ApplySettingsAndStartStimulation();
        }
    }
}
