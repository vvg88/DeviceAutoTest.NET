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
using System.Windows.Threading;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;
using System.Collections.ObjectModel;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for EEG4LedsControl.xaml
    /// </summary>
    public partial class EEG4LedsControl : UserControl, INotifyPropertyChanged
    {
        public EEG4LedsControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private ScriptEnvironment environment;
        /// <summary>
        /// Переменная окружения скрипта
        /// </summary>
        public ScriptEnvironment Environment
        {
            get { return environment; }
            set
            {
                if (environment != value)
                {
                    environment = value;
                    Device = environment.Device as EEG4Device;
                }
            }
        }

        #region Свойства

        private Dictionary<string, EEG4LedColor> ledsColors;
        /// <summary>
        /// Масиив с цветами светодиодов
        /// </summary>
        public Dictionary<string, EEG4LedColor> LedsColors
        {
            get { return ledsColors; }
            set { ledsColors = value; }
        }

        private EEG4Device device;
        /// <summary>
        /// Ссылка на экземпляр устройства
        /// </summary>
        public EEG4Device Device
        {
            get { return device; }
            private set
            {
                if (device != value)
                {
                    device = value;
                    ledsColors = new Dictionary<string, EEG4LedColor>();
                    switch (EEG4Scripts.GetDeviceType(environment))
                    {
                        case EEG4Scripts.NeuronSpectrumTypes.NS_1:
                            IsNS1 = System.Windows.Visibility.Visible;
                            break;
                        case EEG4Scripts.NeuronSpectrumTypes.NS_2:
                            IsNS1 = IsNS2 = System.Windows.Visibility.Visible;
                            break;
                        case EEG4Scripts.NeuronSpectrumTypes.NS_3:
                            IsNS1 = IsNS2 = IsNS3 = System.Windows.Visibility.Visible;
                            break;
                        case EEG4Scripts.NeuronSpectrumTypes.NS_4:
                            IsNS1 = IsNS2 = IsNS3 = IsNS4 = System.Windows.Visibility.Visible;
                            break;
                        case EEG4Scripts.NeuronSpectrumTypes.NS_4P:
                        case EEG4Scripts.NeuronSpectrumTypes.NS_4EP:
                            IsNS1 = IsNS2 = IsNS3 = IsNS4 = IsNS4P = System.Windows.Visibility.Visible;
                            break;
                    }
                    string[] ledsNames = EEG4Scripts.SortLedNames(environment, Enum.GetNames(typeof(EEG4LedsStates)), true);
                    foreach (string ledName in ledsNames)
                        LedsColors.Add(ledName, new EEG4LedColor(device, ledName, Brushes.Transparent, (EEG4LedsStates)Enum.Parse(typeof(EEG4LedsStates), ledName)));
                }
            }
        }

        private Visibility isNS1 = Visibility.Hidden;
        /// <summary>
        /// Флаг проверки прибора НС-1
        /// </summary>
        public Visibility IsNS1
        {
            get { return isNS1; }
            private set 
            {
                if (isNS1 != value)
                {
                    isNS1 = value;
                    OnPropertyChanged("IsNS1");
                }
            }
        }

        private Visibility isNS2 = Visibility.Hidden;
        /// <summary>
        /// Флаг проверки прибора НС-2
        /// </summary>
        public Visibility IsNS2
        {
            get { return isNS2; }
            private set
            {
                if (isNS2 != value)
                {
                    isNS2 = value;
                    OnPropertyChanged("IsNS2");
                }
            }
        }

        private Visibility isNS3 = Visibility.Hidden;
        /// <summary>
        /// Флаг проверки прибора НС-3
        /// </summary>
        public Visibility IsNS3
        {
            get { return isNS3; }
            private set
            {
                if (isNS3 != value)
                {
                    isNS3 = value;
                    OnPropertyChanged("IsNS3");
                }
            }
        }

        private Visibility isNS4 = Visibility.Hidden;
        /// <summary>
        /// Флаг проверки прибора НС-4
        /// </summary>
        public Visibility IsNS4
        {
            get { return isNS4; }
            private set
            {
                if (isNS4 != value)
                {
                    isNS4 = value;
                    OnPropertyChanged("IsNS4");
                }
            }
        }

        private Visibility isNS4P = Visibility.Collapsed;
        /// <summary>
        /// Флаг проверки прибора НС-4П
        /// </summary>
        public Visibility IsNS4P
        {
            get { return isNS4P; }
            private set
            {
                if (isNS4P != value)
                {
                    isNS4P = value;
                    OnPropertyChanged("IsNS4P");
                }
            }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Выключает светодиоды
        /// </summary>
        public void LedsOff()
        {
            device.LightOffLed();
        }

        /// <summary>
        /// Зажигает светодиоды
        /// </summary>
        /// <param name="color"></param>
        public void SetLedLights()
        {
            device.LightOnLed(EEG4LedColor.LedsLightsArray);
        }

        #endregion

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

    public class EEG4LedColor : INotifyPropertyChanged
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name"> Имя отведения </param>
        /// <param name="color"> Цвет светодиода </param>
        public EEG4LedColor(EEG4Device device, string name, Brush color, EEG4LedsStates ledState)
        {
            ledName = name;
            ledBrush = color;
            this.device = device;
            this.ledState = ledState;
        }

        #region Свойства

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
                    //if (LoadInDevice)
                        //Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        //{
                            SetLedLight();
                        //}));
                    OnPropertyChanged("LedBrush");
                }
            }
        }

        private string ledName;
        /// <summary>
        /// Название отведения для этого электрода
        /// </summary>
        public string LedName
        {
            get { return ledName; }
            set { ledName = value; }
        }

        private EEG4Device device;
        /// <summary>
        /// Признак, что состояния светодиодов нужно грузить в аппаратуру
        /// </summary>
        public static bool LoadInDevice = false;
        
        private static Int32[] ledsLightsArray = new Int32[8];
        /// <summary>
        /// Массив с состояниями светодиодов
        /// </summary>
        public static Int32[] LedsLightsArray
        {
            get { return ledsLightsArray; }
        }

        /// <summary>
        /// Состояние светодиода с указанием номера байта в массиве и номеров битов для красного и зеленого цвета
        /// </summary>
        private EEG4LedsStates ledState;
        /// <summary>
        /// Индекс байта в массиве состояний светодиодов
        /// </summary>
        public int Index
        {
            get { return ((int)ledState & 0xF0) >> 4; }
        }
        /// <summary>
        /// Байт, задающий биты состояния данного светодиода
        /// </summary>
        public byte ByteLedState
        {
            get
            {
                if (LedBrush == Brushes.Red)
                    return (byte)(1 << ((int)ledState & 0x0F));
                else
                    if (LedBrush == Brushes.LightGreen)
                        return (byte)(1 << (((int)ledState & 0xF00) >> 8));
                    else
                        if (LedBrush == Brushes.Yellow)
                            return (byte)((1 << ((int)ledState & 0x0F)) | (1 << (((int)ledState & 0xF00) >> 8)));
                        else
                            return 0;
            }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Устанавливает цвет светодиода
        /// </summary>
        /// <param name="color"></param>
        private void SetLedLight()
        {
            //Int32[] lights = device.GetLeds();
            ledsLightsArray[Index] &= ~(byte)((1 << ((int)ledState & 0x0F)) | (1 << (((int)ledState & 0xF00) >> 8)));
            ledsLightsArray[Index] |= ByteLedState;
            if (LoadInDevice)
                device.LightOnLed(ledsLightsArray);
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

        #endregion
    }

    /// <summary>
    /// Байты и биты для управления соответствующими светодиодами в массиве светодиодов
    /// первая цифра слева -    номер столбца зеленого светодиода - номер байта массива
    /// вторая цифра слева -    номер строки  зеленого светодиода - номер бита массива
    /// третья цифра слева -    номер столбца красного светодиода - номер байта массива
    /// четвертая цифра слева - номер строки  красного светодиода - номер бита массива
    /// </summary>
    public enum EEG4LedsStates
    {
        FP1 = 0x1110,
        F3 = 0x2120,
        C3 = 0x3332,
        P3 = 0x2524,
        O1 = 0x0504,
        F7 = 0x0100,
        T3 = 0x2322,
        T5 = 0x1514,
        FP2 = 0x6160,
        F4 = 0x5150,
        C4 = 0x5352,
        P4 = 0x5554,
        O2 = 0x6564,
        F8 = 0x7170,
        T4 = 0x6362,
        T6 = 0x7574,
        FPZ = 0x3130,
        FZ = 0x4140,
        CZ = 0x4342,
        PZ = 0x4544,
        OZ = 0x3534,
        A1 = 0x1312,
        A2 = 0x7372,
        E1_P = 0x7776,
        E1_M = 0x5756,
        E2_P = 0x6766,
        E2_M = 0x4746,
        E3_P = 0x3736,
        E3_M = 0x1716,
        E4_P = 0x2726,
        E4_M = 0x0706,
        ZERO = 0x0302
        //FP1 = 0x7574,
        //F3 = 0x8584,
        //C3 = 0x6564,
        //P3 = 0x4544,
        //O1 = 0x5554,
        //F7 = 0x7170,
        //T3 = 0xB1B0,
        //T5 = 0xA1A0,
        //FP2 = 0x9392,
        //F4 = 0x8382,
        //C4 = 0x6362,
        //P4 = 0x4342,
        //O2 = 0x5352,
        //F8 = 0x7776,
        //T4 = 0x4746,
        //T6 = 0x5756,
        //FPZ = 0x9190,
        //FZ = 0x8180,
        //CZ = 0x6160,
        //PZ = 0x4140,
        //OZ = 0x5150,
        //A1 = 0xB5B4,
        //A2 = 0x6766,
        ////DRL = 0x7372,
        ////ECG_PLUS = 0xA5A4,
        //E1_P = 0x3332,
        //E2_P = 0x2322,
        //E3_P = 0x1312,
        //E4_P = 0x0302,
        //E1_M = 0x3130,
        //E2_M = 0x2120,
        //E3_M = 0x1110,
        //E4_M = 0x0100,
        ////ECG_MINUS = 0xA7A6,
        ////REF = 0xB3B2,
        //ZERO = 0xA3A2
    }
}
