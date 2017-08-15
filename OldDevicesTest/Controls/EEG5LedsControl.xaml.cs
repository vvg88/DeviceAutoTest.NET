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
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for EEG5LedsControl.xaml
    /// </summary>
    public partial class EEG5LedsControl : UserControl
    {
        public EEG5LedsControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private Dictionary<string, EEGLedColor> ledsColors;
        /// <summary>
        /// Масиив с цветами светодиодов
        /// </summary>
        public Dictionary<string, EEGLedColor> LedsColors
        {
            get { return ledsColors; }
            set { ledsColors = value; }
        }

        private EEG5Device device;
        /// <summary>
        /// Ссылка на экземпляр устройства
        /// </summary>
        public EEG5Device Device
        {
            get { return device; }
            set 
            {
                if (device != value)
                {
                    device = value;
                    ledsColors = new Dictionary<string, EEGLedColor>();
                    //string electrNames = Enum.GetNames(typeof(EEG5LedsStates)).GroupBy(
                    string[] ledsNames = EEG5Scripts.SortLedNames(Enum.GetNames(typeof(EEG5LedsStates)), isNS5 == Visibility.Visible ? true : false/*.OrderBy(name => name).ToArray()*/);
                    foreach (string ledName in ledsNames)
                        LedsColors.Add(ledName, new EEGLedColor(device, ledName, Brushes.Transparent, (EEG5LedsStates)Enum.Parse(typeof(EEG5LedsStates), ledName)));
                }
            }
        }

        /// <summary>
        /// Флаг, указывающий, какое утройство тестируется (НС-5 или НС-4/ВПМ)
        /// </summary>
        public Visibility isNS5
        {
            get 
            {
                if ((Device.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
        }

        /// <summary>
        /// Зажигает светодиоды
        /// </summary>
        /// <param name="color"></param>
        public void SetLedLights()
        {
            Int32[] lights = device.GetLeds();
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i] &= 0x00;
            }
            foreach (EEGLedColor ledColor in LedsColors.Values)
            {
                lights[ledColor.Index] |= ledColor.ByteLedState;
            }
            device.LightOnLed(lights);
        }
    }

    /// <summary>
    /// Класс с установкой цвета светодиода для НС-5
    /// </summary>
    public class EEGLedColor : INotifyPropertyChanged
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name"> Имя отведения </param>
        /// <param name="color"> Цвет светодиода </param>
        public EEGLedColor(EEG5Device device, string name, Brush color, EEG5LedsStates ledState)
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
                    if (LoadInDevice)
                        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            SetLedLight();
                        }));
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

        private EEG5Device device;
        /// <summary>
        /// Признак, что состояния светодиодов нужно грузить в аппаратуру
        /// </summary>
        public static bool LoadInDevice = false;

        /// <summary>
        /// Состояние светодиода с указанием номера байта в массиве и номеров битов для красного и зеленого цвета
        /// </summary>
        private EEG5LedsStates ledState;
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
            Int32[] lights = device.GetLeds();
            //for (int i = 0; i < 12; i++)
            //{
            lights[Index] &= ~(byte)((1 << ((int)ledState & 0x0F)) | (1 << (((int)ledState & 0xF00) >> 8)));
            lights[Index] |= ByteLedState;
            //}
            device.LightOnLed(lights);
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
    public enum EEG5LedsStates
    {
        FP1 = 0x7574,
        F3 = 0x8584,
        C3 = 0x6564,
        P3 = 0x4544,
        O1 = 0x5554,
        F7 = 0x7170,
        T3 = 0xB1B0,
        T5 = 0xA1A0,
        FP2 = 0x9392,
        F4 = 0x8382,
        C4 = 0x6362,
        P4 = 0x4342,
        O2 = 0x5352,
        F8 = 0x7776,
        T4 = 0x4746,
        T6 = 0x5756,
        FPZ = 0x9190,
        FZ = 0x8180,
        CZ = 0x6160,
        PZ = 0x4140,
        OZ = 0x5150,
        X1 = 0x9594,
        X2 = 0x3534,
        X3 = 0x2524,
        X4 = 0x1514,
        X5 = 0x0504,
        X6 = 0x9796,
        X7 = 0x3736,
        X8 = 0x2726,
        X9 = 0x1716,
        X10 = 0x0706,
        X11 = 0x8786,
        A1 = 0xB5B4,
        A2 = 0x6766,
        //DRL = 0x7372,
        ECG_PLUS = 0xA5A4,
        E1_PLUS = 0x3332,
        E2_PLUS = 0x2322,
        E3_PLUS = 0x1312,
        E4_PLUS = 0x0302,
        E1_MINUS = 0x3130,
        E2_MINUS = 0x2120,
        E3_MINUS = 0x1110,
        E4_MINUS = 0x0100,
        ECG_MINUS = 0xA7A6,
        REF = 0xB3B2,
        ZERO = 0xA3A2
    }
}
