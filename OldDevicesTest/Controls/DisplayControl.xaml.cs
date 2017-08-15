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
//using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.Devices;
using System.Windows.Threading;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for DisplayControl.xaml
    /// </summary>
    public partial class DisplayControl : UserControl, IDisposable, INotifyPropertyChanged
    {        
        /// <summary>
        /// 
        /// </summary>
        public DisplayControl(NeuroMEPMicro device)
        {
            InitializeComponent();
            Device = device;
            DataContext = this;
            LedsColors = new List<LedColor>();
            LedsColors.Add(new LedColor(this.device, LedsStates.LED_E1_M_GREEN));
            LedsColors.Add(new LedColor(this.device, LedsStates.LED_E1_P_GREEN));
            LedsColors.Add(new LedColor(this.device, LedsStates.LED_E2_M_GREEN));
            LedsColors.Add(new LedColor(this.device, LedsStates.LED_E2_P_GREEN));
            LedsColors.Add(new LedColor(this.device, LedsStates.LED_ZERO_GREEN));
            device.SetDisplayBrightness(displayBrightness);
            DisplayFigures = 0;
            figuresTimer.Interval = TimeSpan.FromSeconds(0.5d);
            figuresTimer.Tick += new EventHandler(figuresTimer_Tick);
        }
        
        #region Properties

        private NeuroMEPMicro device;
        /// <summary>
        /// 
        /// </summary>
        public NeuroMEPMicro Device
        {
            get { return device; }
            private set { device = value; }
        }

        #region Leds

        private List<LedColor> ledsColors;
        /// <summary>
        /// Массив с цветами светодиодов
        /// </summary>
        public List<LedColor> LedsColors
        {
            get { return ledsColors; }
            set { ledsColors = value; }
        }

        private bool ledsOff = true;
        /// <summary>
        /// 
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
                    foreach (LedColor ledColor in LedsColors)
                        ledColor.LedBrush = Brushes.Transparent;
                }
                OnPropertyChanged("LedsOff");
                //OnPropertyChanged("LedsColor");
            }
        }

        private bool ledsGreen;
        /// <summary>
        /// 
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
                        //SetLedLights(0x20);
                        foreach (LedColor ledColor in LedsColors)
                            ledColor.LedBrush = Brushes.LightGreen;
                        greenPressed = true;
                    }
                    OnPropertyChanged("LedsGreen");
                    //OnPropertyChanged("LedsColor");
                }
            }
        }

        private bool ledsYellow;
        /// <summary>
        /// 
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
                        //SetLedLights(0x30);
                        foreach (LedColor ledColor in LedsColors)
                            ledColor.LedBrush = Brushes.Yellow;
                        yellowPressed = true;
                    }                    
                    OnPropertyChanged("LedsYellow");
                    //OnPropertyChanged("LedsColor");
                }
            }
        }
        private bool ledsRed;
        /// <summary>
        /// 
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
                        //SetLedLights(0x10);                        
                        foreach (LedColor ledColor in LedsColors)
                            ledColor.LedBrush = Brushes.Red;
                        redPressed = true;
                    }
                    OnPropertyChanged("LedsRed");
                    //OnPropertyChanged("LedsColor");                    
                }
            }
        }

        private bool ledsIllumination;
        /// <summary>
        /// Включен режим иллюминации
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
                        foreach (LedColor ledColor in LedsColors)
                            ledColor.LedBrush = Brushes.Transparent;
                        figuresTimer.IsEnabled = ledsIllumination;
                        ledsBlinkCounter = 0;
                        ledBlinkedIndx = 0;
                    }
                    else
                        if (!isTestingFigures)
                            figuresTimer.IsEnabled = ledsIllumination;
                    OnPropertyChanged("LedsIllumination");
                }
            }
        }

        private int ledsBlinkCounter = 0;
        private int ledBlinkedIndx = 0;

        /// <summary>
        /// 
        /// </summary>
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
        

        #endregion

        #region Figures

        DispatcherTimer figuresTimer = new DispatcherTimer();
        private double displayFigures = -1;
        /// <summary>
        /// 
        /// </summary>
        public double DisplayFigures
        {
            get { return displayFigures; }
            set
            {
                if (displayFigures != value)
                {
                    displayFigures = value;
                    device.SetNumberToDisplay(value);
                    figuresChecked = true;
                    OnPropertyChanged("DisplayFigures");
                }
            }
        }

        double figuresBase = 0.1;
        double figureVal = 0;
        void figuresTimer_Tick(object sender, EventArgs e)
        {
            if (isTestingFigures)
            {
                DisplayFigures = figuresBase * figureVal;
                figureVal++;
                if (figureVal > 9)
                {
                    figureVal = 1;
                    figuresBase = figuresBase * 10;
                    if (Math.Abs(figuresBase) > 10)
                    {
                        figuresBase *= (figuresBase > 0 ? -0.1 : 0.1) / figuresBase;
                    }
                }
            }
            if (ledsIllumination)
            {
                if (ledBlinkedIndx < 5)
                {
                    switch (ledsBlinkCounter++)
                    {
                        case 0:
                            LedsColors[ledBlinkedIndx].LedBrush = Brushes.LightGreen;
                            break;
                        case 1:
                            LedsColors[ledBlinkedIndx].LedBrush = Brushes.Red;
                            break;
                        case 2:
                            LedsColors[ledBlinkedIndx].LedBrush = Brushes.Yellow;
                            break;
                        case 3:
                            LedsColors[ledBlinkedIndx++].LedBrush = Brushes.Transparent;
                            ledsBlinkCounter = 0;
                            break;
                    }
                }
                else
                {
                    switch (ledsBlinkCounter++)
                    {
                        case 0:
                            foreach(LedColor ledColor in LedsColors)
                                ledColor.LedBrush = Brushes.LightGreen;
                            break;
                        case 1:
                            foreach (LedColor ledColor in LedsColors)
                                ledColor.LedBrush = Brushes.Red;
                            break;
                        case 2:
                            foreach (LedColor ledColor in LedsColors)
                                ledColor.LedBrush = Brushes.Yellow;
                            break;
                        case 3:
                            foreach (LedColor ledColor in LedsColors)
                                ledColor.LedBrush = Brushes.Transparent;
                            ledsBlinkCounter = 0;
                            ledBlinkedIndx = 0;
                            break;
                    }
                }
            }
        }

        private bool isTestingFigures;
        /// <summary>
        /// Режим проверки цифрового индикатора
        /// </summary>
        public bool IsTestingFigures
        {
            get { return isTestingFigures; }
            set 
            {
                if (isTestingFigures != value)
                {
                    isTestingFigures = value;
                    if (isTestingFigures)
                        figuresTimer.IsEnabled = isTestingFigures;
                    else
                        if (!ledsIllumination)
                            figuresTimer.IsEnabled = isTestingFigures;
                    OnPropertyChanged("IsTestingFigures");
                }
            }
        }
        
        #endregion

        private int displayBrightness = 8;
        /// <summary>
        /// 
        /// </summary>
        public int DisplayBrightness
        {
            get { return displayBrightness; }
            set
            {
                if (displayBrightness != value)
                {
                    displayBrightness = value;
                    device.SetDisplayBrightness(value);
                    brightnessChanged = true;
                    OnPropertyChanged("DisplayBrightness");
                }
            }
        }
        private bool greenPressed = false;
        private bool yellowPressed = false;
        private bool redPressed = false;
        private bool figuresChecked = false;
        private bool brightnessChanged = false;
        /// <summary>
        /// 
        /// </summary>
        public bool AllChecked
        {
            get
            {
                return greenPressed && yellowPressed && redPressed && figuresChecked && brightnessChanged;
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
                figuresTimer.Stop();
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

    /// <summary>
    /// Класс цвета светодиода
    /// </summary>
    public class LedColor : INotifyPropertyChanged
    {
        public LedColor(NSDevice device, LedsStates ledState)
        {
            ledIndex = (byte)ledState >> 4;
            this.device = device;
        }

        /// <summary>
        /// Ссылка на экземпляр устройства
        /// </summary>
        private NSDevice device;

        /// <summary>
        /// Индекс байта электрода в массиве
        /// </summary>
        private int ledIndex;

        private Brush ledBrush;
        /// <summary>
        /// Цвет светодиода
        /// </summary>
        public Brush LedBrush
        {
            get { return ledBrush; }
            set 
            {
                if (ledBrush != value)
                {
                    ledBrush = value;
                    if (ledBrush == Brushes.Red)
                    {
                        ledState = 0x10;
                        SetLedLights();
                    }
                    else
                        if (ledBrush == Brushes.LightGreen)
                        {
                            ledState = 0x20;
                            SetLedLights();
                        }
                        else
                            if (ledBrush == Brushes.Yellow)
                            {
                                ledState = 0x30;
                                SetLedLights();
                            }
                            else
                            {
                                ledState = 0x00;
                                SetLedLights();
                            }
                    OnPropertyChanged("LedBrush");
                }
            }
        }

        /// <summary>
        /// Зажигает светодиоды определенного цвета
        /// </summary>
        /// <param name="color"></param>
        private void SetLedLights()
        {
            Int32[] lights = (device as NeuroMEPMicro).GetLeds();
            //for (int i = 7; i > 2; i--)
            //{
                lights[ledIndex] &= ~0x30;
                lights[ledIndex] |= ledState;
            //}
            (device as NeuroMEPMicro).LightOnLed(lights);
        }

        /// <summary>
        /// Состояние светодиода в байте
        /// </summary>
        private byte ledState = 0;

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
    /// Перечисление с состояниями светодиодов.
    /// Первая цифра слева - номер столбца светодиода - номер байта массива.
    /// Вторая цифра слева - номер строки светодиода - номер бита массива. 
    /// </summary>
    public enum LedsStates : byte
    {
        LED_E1_P_RED = 0x44,
        LED_E1_M_RED = 0x74,
        LED_E2_P_RED = 0x34,
        LED_E2_M_RED = 0x64,
        LED_ZERO_RED = 0x54,
        LED_E1_P_GREEN = 0x45,
        LED_E1_M_GREEN = 0x75,
        LED_E2_P_GREEN = 0x35,
        LED_E2_M_GREEN = 0x65,
        LED_ZERO_GREEN = 0x55
    }
}
