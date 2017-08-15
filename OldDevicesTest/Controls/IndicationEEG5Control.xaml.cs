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
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices;
using System.ComponentModel;
using System.Windows.Threading;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for IndicationEEG5.xaml
    /// </summary>
    public partial class IndicationEEG5Control : UserControl, IDisposable, INotifyPropertyChanged
    {
        public IndicationEEG5Control()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public IndicationEEG5Control(EEG5Device device)
        {
            InitializeComponent();
            this.device = device;
            DataContext = this;
            EEG5LedsControl.Device = this.device;
            (Device as ICurrentStimulator).SetStimulusAmplitude(0.0f);
            //ledsColors = new Dictionary<string, EEGLedColor>();
            //foreach (string ledName in Enum.GetNames(typeof(EEG5LedsStates)).OrderBy(name => name).ToArray())
            //    LedsColors.Add(ledName, new EEGLedColor(device, ledName, Brushes.Transparent, (EEG5LedsStates)Enum.Parse(typeof(EEG5LedsStates), ledName)));
            illuminationTimer.Interval = TimeSpan.FromSeconds(1.0/3.0);
            illuminationTimer.Tick += new EventHandler(illuminationTimer_Tick);
        }

        #region Свойства

        private EEG5Device device;
        /// <summary>
        /// Указатель на устройство
        /// </summary>
        public EEG5Device Device
        {
            get { return device; }
            private set { device = value; }
        }

        //private Dictionary<string, EEGLedColor> ledsColors;
        ///// <summary>
        ///// Масиив с цветами светодиодов
        ///// </summary>
        //public Dictionary<string, EEGLedColor> LedsColors
        //{
        //    get { return ledsColors; }
        //    set { ledsColors = value; }
        //}
        
        
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
                    device.LightsOf();
                    foreach (EEGLedColor ledColor in EEG5LedsControl.LedsColors.Values)
                        ledColor.LedBrush = Brushes.Transparent;
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
                        foreach (EEGLedColor ledColor in EEG5LedsControl.LedsColors.Values)
                            ledColor.LedBrush = Brushes.LightGreen;
                        EEG5LedsControl.SetLedLights();
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
                        foreach (EEGLedColor ledColor in EEG5LedsControl.LedsColors.Values)
                            ledColor.LedBrush = Brushes.Yellow;
                        EEG5LedsControl.SetLedLights();
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
                        foreach (EEGLedColor ledColor in EEG5LedsControl.LedsColors.Values)
                            ledColor.LedBrush = Brushes.Red;
                        EEG5LedsControl.SetLedLights();
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
                        EEGLedColor.LoadInDevice = true;
                    }
                    else
                    {
                        illuminationTimer.IsEnabled = false;
                        EEGLedColor.LoadInDevice = false;
                    }
                    illuminationPressed = true;
                    OnPropertyChanged("LedsIllumination");
                }
            }
        }

        #region ForkStim

        private bool leftForkLed = true;
        /// <summary>
        /// Зажечь левый светодиод на стимуляторе
        /// </summary>
        public bool LeftForkLed
        {
            get { return leftForkLed; }
            set 
            {
                if (value != leftForkLed)
                {
                    leftForkLed = value;
                    if (leftForkLed)
                    {
                        if (!(Device as ICurrentStimulator).GetEnabled())
                            (Device as ICurrentStimulator).SetEnabled(true);
                        (Device as ICurrentStimulator).SetPolarity(StimulusPolarity.Plus);
                    }
                    OnPropertyChanged("LeftForkLed");
                    leftForkPressed = true;
                }
            }
        }

        private bool rightForkLed;
        /// <summary>
        /// Зажечь левый светодиод на стимуляторе
        /// </summary>
        public bool RightForkLed
        {
            get { return rightForkLed; }
            set
            {
                if (value != rightForkLed)
                {
                    rightForkLed = value;
                    if (rightForkLed)
                    {
                        if (!(Device as ICurrentStimulator).GetEnabled())
                            (Device as ICurrentStimulator).SetEnabled(true);
                        (Device as ICurrentStimulator).SetPolarity(StimulusPolarity.Minus);
                    }
                    OnPropertyChanged("RightForkLed");
                    rightForkPressed = true;
                }
            }
        }

        private bool forkStimBlink;
        /// <summary>
        /// Установка мигания светодиода на вилочковом стимуляторе
        /// </summary>
        public bool ForkStimBlink
        {
            get { return forkStimBlink; }
            set 
            {
                if (value != forkStimBlink)
                {
                    forkStimBlink = value;
                    if (forkStimBlink)
                    {
                        if (!(Device as ICurrentStimulator).GetEnabled())
                            (Device as ICurrentStimulator).SetEnabled(true);
                        (Device as ICurrentStimulator).StartStimulation();
                    }
                    else
                    {
                        (Device as ICurrentStimulator).StopStimulation();
                    }
                    blinkForkPressed = true;
                }
            }
        }
        

        #endregion


        ///// <summary>
        ///// 
        ///// </summary>
        //public Brush LedsColor
        //{
        //    get
        //    {
        //        if (LedsRed)
        //            return Brushes.Red;
        //        if (LedsGreen)
        //            return Brushes.LightGreen;
        //        if (LedsYellow)
        //            return Brushes.Yellow;
        //        return Brushes.Transparent;
        //    }
        //}
        

        //private void InitLeds()
        //{
        //    var chancount = device.GetChannelsCount();
        //    for (int i = 0; i <= chancount; i++)
        //    {
        //        ledsStates.Add(device.GetChannelName(i), new LedState(device.GetChannelName(i), Brushes.Transparent));
        //    }
        //}

        #endregion

        /// <summary>
        /// Таймер для иллюминации
        /// </summary>
        DispatcherTimer illuminationTimer = new DispatcherTimer();

        #endregion

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
            if (ledsCounter == EEG5LedsControl.LedsColors.Count)
            {
                if (EEGLedColor.LoadInDevice)
                    EEGLedColor.LoadInDevice = false;
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
                        ledBlinkCounter = 0;
                        ledsCounter = 0;
                        EEGLedColor.LoadInDevice = true;
                        break;
                }
                foreach (EEGLedColor ledColor in EEG5LedsControl.LedsColors.Values)
                    ledColor.LedBrush = currentBrush;
                EEG5LedsControl.SetLedLights();
                return;
            }
            string electrodName = /*Enum.GetNames(typeof(EEG5LedsStates)).OrderBy(name => name).ToArray()*/EEG5LedsControl.LedsColors.Values.ToArray()[ledsCounter].LedName;
            switch (ledBlinkCounter++)
            {
                case 0:
                    EEG5LedsControl.LedsColors[electrodName].LedBrush = Brushes.LightGreen;
                    break;
                case 1:
                    EEG5LedsControl.LedsColors[electrodName].LedBrush = Brushes.Yellow;
                    break;
                case 2:
                    EEG5LedsControl.LedsColors[electrodName].LedBrush = Brushes.Red;
                    break;
                case 3:
                    EEG5LedsControl.LedsColors[electrodName].LedBrush = Brushes.Transparent;
                    ledsCounter++;
                    ledBlinkCounter = 0;
                    break;
            }
        }

        private bool greenPressed = false;
        private bool yellowPressed = false;
        private bool redPressed = false;
        private bool illuminationPressed = false;
        private bool leftForkPressed = false;
        private bool rightForkPressed = false;
        private bool blinkForkPressed = false;

        /// <summary>
        /// 
        /// </summary>
        public bool AllChecked
        {
            get
            {
                return greenPressed && yellowPressed && redPressed && illuminationPressed && leftForkPressed && rightForkPressed && blinkForkPressed;
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
                if ((Device as EEG5Device).isCurrentStimulation)
                    (Device as ICurrentStimulator).StopStimulation();
                (Device as ICurrentStimulator).SetEnabled(false);
                illuminationTimer.Stop();
                device.LightsOf();
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
