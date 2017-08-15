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
using NeuroSoft.Hardware.Devices.Base;
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
                if (ledsOff) device.LightsOf();
                OnPropertyChanged("LedsOff");
                OnPropertyChanged("LedsColor");
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
                        SetLedLights(0x20);
                        greenPressed = true;
                    }
                    OnPropertyChanged("LedsGreen");
                    OnPropertyChanged("LedsColor");
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
                        SetLedLights(0x30);
                        yellowPressed = true;
                    }                    
                    OnPropertyChanged("LedsYellow");
                    OnPropertyChanged("LedsColor");
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
                        SetLedLights(0x10);                        
                        redPressed = true;
                    }
                    OnPropertyChanged("LedsRed");
                    OnPropertyChanged("LedsColor");                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Brush LedsColor
        {
            get
            {
                if (LedsRed)
                    return Brushes.Red;
                if (LedsGreen)
                    return Brushes.LightGreen;
                if (LedsYellow)
                    return Brushes.Yellow;
                return Brushes.Transparent;
            }
        }
        /// <summary>
        /// Зажигает светодиоды определенного цвета
        /// </summary>
        /// <param name="color"></param>
        private void SetLedLights(int color)
        {
            Int32[] lights = device.GetLeds();
            for (int i = 7; i > 2; i--)
            {
                lights[i] &= ~0x30;
                lights[i] |= color;
            }
            device.LightOnLed(lights);
        }

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
            DisplayFigures = figuresBase * figureVal;
            figureVal++;
            if (figureVal > 9)
            {
                figureVal = 1;
                figuresBase = figuresBase * 10;
                if (Math.Abs(figuresBase) > 10)
                {
                    figuresBase *= -0.1 / figuresBase;
                }
            }
        }

        private bool isTestingFigures;
        /// <summary>
        /// 
        /// </summary>
        public bool IsTestingFigures
        {
            get { return isTestingFigures; }
            set 
            {
                if (isTestingFigures != value)
                {
                    isTestingFigures = value;
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
}
