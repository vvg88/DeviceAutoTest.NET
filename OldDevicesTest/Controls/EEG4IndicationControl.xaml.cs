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
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for EEG4IndicationControl.xaml
    /// </summary>
    public partial class EEG4IndicationControl : UserControl, IDisposable, INotifyPropertyChanged
    {
        public EEG4IndicationControl()
        {
            InitializeComponent();
        }

        public EEG4IndicationControl(ScriptEnvironment environment)
        {
            InitializeComponent();
            this.device = environment.Device as EEG4Device;
            DataContext = this;
            EEG4LedsControl.Environment = environment;
            illuminationTimer.Interval = TimeSpan.FromSeconds(1.0/3.0);
            illuminationTimer.Tick += new EventHandler(illuminationTimer_Tick);
        }

        #region Свойства

        private EEG4Device device;
        /// <summary>
        /// Указатель на устройство
        /// </summary>
        public EEG4Device Device
        {
            get { return device; }
            private set { device = value; }
        }

        #region Leds

        private bool ledsOff = true;
        /// <summary>
        /// Отключить светодиоды
        /// </summary>
        public bool LedsOff
        {
            get { return ledsOff; }
            set
            {
                ledsOff = value;
                if (ledsOff)
                {
                    //device.LightsOf();
                    if (EEG4LedColor.LoadInDevice)
                        EEG4LedColor.LoadInDevice = false;
                    foreach (EEG4LedColor ledColor in EEG4LedsControl.LedsColors.Values)
                        ledColor.LedBrush = Brushes.Transparent;
                    EEG4LedsControl.SetLedLights();
                }
                OnPropertyChanged("LedsOff");
                //OnPropertyChanged("LedsColor");
            }
        }

        private bool ledsGreen;
        /// <summary>
        /// Зажечь зеленым
        /// </summary>
        public bool LedsGreen
        {
            get { return ledsGreen; }
            set
            {
                if (ledsGreen != value)
                {
                    ledsGreen = value;
                    if (ledsGreen)
                    {
                        if (EEG4LedColor.LoadInDevice)
                            EEG4LedColor.LoadInDevice = false;
                        foreach (EEG4LedColor ledColor in EEG4LedsControl.LedsColors.Values)
                            ledColor.LedBrush = Brushes.LightGreen;
                        EEG4LedsControl.SetLedLights();
                        greenPressed = true;
                    }
                    OnPropertyChanged("LedsGreen");
                    //OnPropertyChanged("LedsColor");
                }
            }
        }

        private bool ledsYellow;
        /// <summary>
        /// Зажечь желтым
        /// </summary>
        public bool LedsYellow
        {
            get { return ledsYellow; }
            set
            {
                if (ledsYellow != value)
                {
                    ledsYellow = value;
                    if (ledsYellow)
                    {
                        if (EEG4LedColor.LoadInDevice)
                            EEG4LedColor.LoadInDevice = false;
                        foreach (EEG4LedColor ledColor in EEG4LedsControl.LedsColors.Values)
                            ledColor.LedBrush = Brushes.Yellow;
                        EEG4LedsControl.SetLedLights();
                        yellowPressed = true;
                    }
                    OnPropertyChanged("LedsYellow");
                    //OnPropertyChanged("LedsColor");
                }
            }
        }
        private bool ledsRed;
        /// <summary>
        /// Зажечь красным
        /// </summary>
        public bool LedsRed
        {
            get { return ledsRed; }
            set
            {
                if (ledsRed != value)
                {
                    ledsRed = value;
                    if (ledsRed)
                    {
                        if (EEG4LedColor.LoadInDevice)
                            EEG4LedColor.LoadInDevice = false;
                        foreach (EEG4LedColor ledColor in EEG4LedsControl.LedsColors.Values)
                            ledColor.LedBrush = Brushes.Red;
                        EEG4LedsControl.SetLedLights();
                        redPressed = true;
                    }
                    OnPropertyChanged("LedsRed");
                    //OnPropertyChanged("LedsColor");
                }
            }
        }

        private bool ledsIllumination;
        /// <summary>
        /// Включение режима иллюминации
        /// </summary>
        public bool LedsIllumination
        {
            get { return ledsIllumination; }
            set
            {
                if (ledsIllumination != value)
                {
                    ledsIllumination = value;
                    if (ledsIllumination)
                    {
                        LedsOff = true;
                        illuminationTimer.IsEnabled = true;
                        EEG4LedColor.LoadInDevice = true;
                        ledBlinkCounter = 0;
                        ledsCounter = 0;
                    }
                    else
                    {
                        illuminationTimer.IsEnabled = false;
                        EEG4LedColor.LoadInDevice = false;
                    }
                    illuminationPressed = true;
                    OnPropertyChanged("LedsIllumination");
                }
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Таймер для иллюминации
        /// </summary>
        DispatcherTimer illuminationTimer = new DispatcherTimer();

        #region Методы

        private int ledBlinkCounter = 0;
        private int ledsCounter = 0;
        /// <summary>
        /// Обработчик тика таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void illuminationTimer_Tick(object sender, EventArgs e)
        {
            if (ledsCounter == EEG4LedsControl.LedsColors.Count)
            {
                if (EEG4LedColor.LoadInDevice)
                    EEG4LedColor.LoadInDevice = false;
                Brush currentBrush = Brushes.Transparent;
                switch (ledBlinkCounter++)
                {
                    case 0:
                        currentBrush = Brushes.LightGreen;
                        break;
                    case 1:
                        currentBrush = Brushes.Yellow;
                        break;
                    case 2:
                        currentBrush = Brushes.Red;
                        break;
                    case 3:
                        currentBrush = Brushes.Transparent;
                        foreach (EEG4LedColor ledColor in EEG4LedsControl.LedsColors.Values)
                            ledColor.LedBrush = currentBrush;
                        EEG4LedsControl.SetLedLights();
                        ledBlinkCounter = 0;
                        ledsCounter = 0;
                        EEG4LedColor.LoadInDevice = true;
                        break;
                }
                foreach (EEG4LedColor ledColor in EEG4LedsControl.LedsColors.Values)
                    ledColor.LedBrush = currentBrush;
                EEG4LedsControl.SetLedLights();
                return;
            }
            string electrodName = /*Enum.GetNames(typeof(EEG5LedsStates)).OrderBy(name => name).ToArray()*/EEG4LedsControl.LedsColors.Values.ToArray()[ledsCounter].LedName;
            switch (ledBlinkCounter++)
            {
                case 0:
                    EEG4LedsControl.LedsColors[electrodName].LedBrush = Brushes.LightGreen;
                    break;
                case 1:
                    EEG4LedsControl.LedsColors[electrodName].LedBrush = Brushes.Yellow;
                    break;
                case 2:
                    EEG4LedsControl.LedsColors[electrodName].LedBrush = Brushes.Red;
                    break;
                case 3:
                    EEG4LedsControl.LedsColors[electrodName].LedBrush = Brushes.Transparent;
                    ledsCounter++;
                    ledBlinkCounter = 0;
                    break;
            }
        }

        private bool greenPressed = false;
        private bool yellowPressed = false;
        private bool redPressed = false;
        private bool illuminationPressed = false;

        /// <summary>
        /// 
        /// </summary>
        public bool AllChecked
        {
            get
            {
                return greenPressed && yellowPressed && redPressed && illuminationPressed;
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (device != null)
            {
                //if ((Device as EEG5Device).isCurrentStimulation)
                //    (Device as ICurrentStimulator).StopStimulation();
                //(Device as ICurrentStimulator).SetEnabled(false);
                illuminationTimer.Stop();
                LedsOff = true;
                device.LightOffLed();
                device = null;
                DataContext = null;
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