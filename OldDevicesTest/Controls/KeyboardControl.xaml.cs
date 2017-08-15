using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;
//using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.Devices;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for KeyboardControl.xaml
    /// </summary>
    public partial class KeyboardControl : UserControl, INotifyPropertyChanged, IDisposable
    {

        /// <summary>
        /// 
        /// </summary>
        public KeyboardControl(NeuroMEPMicro device)
        {
            InitializeComponent();
            DataContext = this;
            Device = device;
            if (device != null)
            {                
                device.KeyPressed += new NeuroMEPMicroKeyPressedDelegate(device_KeyPressed);                
            }
            DataContext = this;
            for (int i = 0; i < 23; i++)
            {
                KeyBrushes.Add(Brushes.White);
            }
        }        
        
        #region Properties
        private NeuroMEPMicro device = null;
        /// <summary>
        /// 
        /// </summary>
        public NeuroMEPMicro Device
        {
            get { return device; }
            private set { device = value; }
        }

        private bool?[] buttonStates = new bool?[33]; //18 кнопок и 5 энкодоров (нажатие+2направления)
        private ObservableCollection<Brush> keyBrushes = new ObservableCollection<Brush>();

        /// <summary>
        /// Цвета клавиш
        /// </summary>
        public ObservableCollection<Brush> KeyBrushes
        {
            get { return keyBrushes; }
        }

        private double markerEncoderAngle = 0;

        /// <summary>
        /// MarkerEncoderAngle
        /// </summary>
        public double MarkerEncoderAngle
        {
            get { return markerEncoderAngle; }
            set 
            { 
                markerEncoderAngle = value;
                OnPropertyChanged("MarkerEncoderAngle");
            }
        }

        private double sweepEncoderAngle = 0;

        /// <summary>
        /// SweepEncoderAngle
        /// </summary>
        public double SweepEncoderAngle
        {
            get { return sweepEncoderAngle; }
            set
            {
                sweepEncoderAngle = value;
                OnPropertyChanged("SweepEncoderAngle");
            }
        }

        private double sensitivityEncoderAngle = 0;

        /// <summary>
        /// SensitivityEncoderAngle
        /// </summary>
        public double SensitivityEncoderAngle
        {
            get { return sensitivityEncoderAngle; }
            set
            {
                sensitivityEncoderAngle = value;
                OnPropertyChanged("SensitivityEncoderAngle");
            }
        }

        private double durationEncoderAngle = 0;

        /// <summary>
        /// DurationEncoderAngle
        /// </summary>
        public double DurationEncoderAngle
        {
            get { return durationEncoderAngle; }
            set
            {
                durationEncoderAngle = value;
                OnPropertyChanged("DurationEncoderAngle");
            }
        }

        private double intensityEncoderAngle = 0;

        /// <summary>
        /// IntensityEncoderAngle
        /// </summary>
        public double IntensityEncoderAngle
        {
            get { return intensityEncoderAngle; }
            set
            {
                intensityEncoderAngle = value;
                OnPropertyChanged("IntensityEncoderAngle");
            }
        }    

        /// <summary>
        /// Признак того, что все клавишы были нажаты
        /// </summary>
        public bool AllButtonsPressed
        {
            get 
            {
                for (int i = 0; i < buttonStates.Length; i++) 
                {
                    if (i > 17 && i < 24) //проигнорируем нажатие кнопок пациента и педали
                        continue;
                    if (buttonStates[i] != false)
                    {
                        return false;
                    }
                }
                return true;
                //return !recieved.Contains(false);
            }
        }
        #endregion        

        void device_KeyPressed(KeyBoardCodesEnum key, KeyStateEnum state, int rotateValue)
        {            
            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
            {
                if (state == KeyStateEnum.Rotate)
                {
                    encoderRefresh(key, rotateValue);
                }
                else
                {
                    buttonRefresh(key, state);
                }
            }));
        }

        void encoderRefresh(KeyBoardCodesEnum key, int rotateValue)
        {
            int angle = rotateValue * 15; /*15 градусов на смещение*/
            int encoderIndex = 23 + ((int)key - 14) * 2;
            if (rotateValue > 0)
                encoderIndex++;
            switch (key)
            {
                case KeyBoardCodesEnum.MarkerEncoder:
                    MarkerEncoderAngle += angle;
                    break;
                case KeyBoardCodesEnum.SweepEncoder:
                    SweepEncoderAngle += angle;
                    break;
                case KeyBoardCodesEnum.SensitiviEncoder:
                    SensitivityEncoderAngle += angle;
                    break;
                case KeyBoardCodesEnum.DurationEncoder:
                    DurationEncoderAngle += angle;
                    break;
                case KeyBoardCodesEnum.IntensityEncoder:
                    IntensityEncoderAngle += angle;
                    break;                
                default:
                    throw new ApplicationException();
            }

            if (buttonStates[encoderIndex] != false)
                buttonStates[encoderIndex] = false;
        }

        void buttonRefresh(KeyBoardCodesEnum key, KeyStateEnum state)
        {            
            Brush brush;
            if (state == KeyStateEnum.Down)
                brush = Brushes.Gold;
            else if (state == KeyStateEnum.Up)
                brush = Brushes.LightGray;
            else return;

            int keyIndex = (int)key-1;
            if (keyIndex >= 0)
            {
                KeyBrushes[keyIndex] = brush;
                OnPropertyChanged("KeyBrushes[" + keyIndex + "]");
                buttonStates[keyIndex] = state == KeyStateEnum.Down;
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

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (device != null)
            {
                device.KeyPressed -= new NeuroMEPMicroKeyPressedDelegate(device_KeyPressed);                
                device = null;
                DataContext = null;
            }
        }
    }    
}
