using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using NeuroSoft.Hardware.Devices;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using NeuroSoft.Hardware.Common;
using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.WPFComponents.ScalableWindows;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for NeuroMEPKeyboardControl.xaml
    /// </summary>
    public partial class NeuroMEPKeyboardControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Эквиваленты номеров событий старым индексам
        /// </summary>
        private Dictionary<ControlPanelItemId, int> indexesEquals = new Dictionary<ControlPanelItemId, int>
        {
            {ControlPanelItemId.Marker, 0},
            {ControlPanelItemId.Curve, 1},
            {ControlPanelItemId.Amplitude, 2},
            {ControlPanelItemId.Scale, 3},
            {ControlPanelItemId.ScaleXIncrease, 4},
            {ControlPanelItemId.ScaleYDecrease, 5 },
            {ControlPanelItemId.ScaleXDecrease, 6 },
            {ControlPanelItemId.ScaleYIncrease, 7 },
            {ControlPanelItemId.F4, 8 },
            {ControlPanelItemId.DeleteCurve, 9 },
            {ControlPanelItemId.Next, 10 },
            {ControlPanelItemId.SingleAcquisition, 11 },
            {ControlPanelItemId.DurationIncrease, 12 },
            {ControlPanelItemId.DurationDecrease, 13 },
            {ControlPanelItemId.F3, 14 },
            {ControlPanelItemId.Tab, 15 },
            {ControlPanelItemId.Ok, 16 },
            {ControlPanelItemId.RhythmicAcquisition, 17 },
            {ControlPanelItemId.Pause, 18 },
            {ControlPanelItemId.Monitoring, 19 },
            {ControlPanelItemId.Averaging, 20 },
            {ControlPanelItemId.Previous, 21 },
            {ControlPanelItemId.Esc, 22 },
            {ControlPanelItemId.F2, 23 },
            {ControlPanelItemId.Impedance, 24 },
            {ControlPanelItemId.F1, 25 },
            {ControlPanelItemId.F7, 26 },
            {ControlPanelItemId.F5, 27 },
            {ControlPanelItemId.F6, 28 },
            {ControlPanelItemId.PatientButton1, 29 },
            {ControlPanelItemId.PatientButton2, 30 },
            {ControlPanelItemId.PatientButton3, 31 },
            {ControlPanelItemId.PatientButtonsConnection, 32 },
            {ControlPanelItemId.Pedal1, 33 },
            {ControlPanelItemId.Pedal2, 34 },
            {ControlPanelItemId.Pedal3, 35 },
            {ControlPanelItemId.PedalsConnection, 36 },
            {ControlPanelItemId.MarkerIncrease, 37 },
            {ControlPanelItemId.MarkerDecrease, 38 },
            {ControlPanelItemId.CurveIncrease, 39 },
            {ControlPanelItemId.CurveDecrease, 40 },
            {ControlPanelItemId.AmplitudeIncrease, 41 },
            {ControlPanelItemId.AmplitudeDecrease, 42 }
        };

        /// <summary>
        /// 
        /// </summary>
        public NeuroMEPKeyboardControl(NeuroMepBase device)
        {
            InitializeComponent();
            DataContext = this;
            Device = device;
            if (device != null)
            {
                if (device.ControlPanel.IsRun)
                    device.ControlPanel.Stop();     // To avoid exeption on restart
                device.ControlPanel.Start(KeyboardStateChange, KeyboardStateErrHandler);
            }
            DataContext = this;
            for (int i = 0; i < 37; i++)
            {
                KeyBrushes.Add(Brushes.White);
            }
        }
        
        #region Properties
        private NeuroMepBase device;
        /// <summary>
        /// 
        /// </summary>
        public NeuroMepBase Device
        {
            get { return device; }
            private set { device = value; }
        }
        private bool?[] buttonStates = new bool?[43]; //37 кнопок и 3 энкодора, каждый вращается в двух направлениях        
        private ObservableCollection<Brush> keyBrushes = new ObservableCollection<Brush>();

        /// <summary>
        /// Цвета клавиш
        /// </summary>
        public ObservableCollection<Brush> KeyBrushes => keyBrushes;

        private double encoderAAngle = 0;
        /// <summary>
        /// Угол поворота энкодера A Маркер
        /// </summary>
        public double EncoderAAngle
        {
            get { return encoderAAngle; }
            set 
            { 
                encoderAAngle = value;
                OnPropertyChanged("EncoderAAngle");
            }
        }

        private double encoderBAngle = 0;
        /// <summary>
        /// Угол поворота энкодера B Кривая
        /// </summary>
        public double EncoderBAngle
        {
            get { return encoderBAngle; }
            set
            {
                encoderBAngle = value;
                OnPropertyChanged("EncoderBAngle");
            }
        }

        private double encoderCAngle = 0;
        /// <summary>
        /// Угол поворота энкодера C Ампилтуда
        /// </summary>
        public double EncoderCAngle
        {
            get { return encoderCAngle; }
            set
            {
                encoderCAngle = value;
                OnPropertyChanged("EncoderCAngle");
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
                    if (i == 31 || (i > 32 && i < 37)) //проигнорируем нажатие педалей
                        continue;
                    if (i == 32) //индикатор подключения кнопки пациента должен быть активны либо нажаты
                    {
                        if (buttonStates[i] == null)                        
                            return false;                        
                    }
                    else if (buttonStates[i] != false)
                    {
                        return false;
                    }
                }
                return true;
                //return !recieved.Contains(false);
            }
        }
        #endregion

        void KeyboardStateChange(AsyncActionArgs<ControlPanelData> args)
        {
            if (args.Data.ItemAction == ControlPanelItemAction.PressOrRotate)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
                {
                    encoderRefresh(args.Data);
                }));
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
                {
                    buttonRefresh(args.Data);
                }));
            }
        }

        void KeyboardStateErrHandler(AsyncActionArgs<DeviceErrorInfo> args)
        {
            NSMessageBox.Show(this, "При чтении данных с клавиатуры возникла ошибка:" + args.Data.Descriptor.Message,
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void encoderRefresh(ControlPanelData e)
        {
            switch (e.ItemId)
            {
                case ControlPanelItemId.MarkerIncrease:
                    EncoderAAngle += 15; /*15 градусов на смещение*/
                    break;
                case ControlPanelItemId.MarkerDecrease:
                    EncoderAAngle -= 15;
                    break;
                case ControlPanelItemId.CurveIncrease:
                    EncoderBAngle += 15;
                    break;
                case ControlPanelItemId.CurveDecrease:
                    EncoderBAngle -= 15;
                    break;
                case ControlPanelItemId.AmplitudeIncrease:
                    EncoderCAngle += 15;
                    break;
                case ControlPanelItemId.AmplitudeDecrease:
                    EncoderCAngle -= 15;
                    break;
                default:
                    return;
            }
            if (buttonStates[indexesEquals[e.ItemId]] != false)
                buttonStates[indexesEquals[e.ItemId]] = false;
        }

        void buttonRefresh(ControlPanelData e)
        {
            var brush = e.ItemAction == ControlPanelItemAction.Down
                ? Brushes.Gold
                : (e.ItemAction == ControlPanelItemAction.Up ? Brushes.LightGray : Brushes.White);
            var currentIndx = indexesEquals[e.ItemId];
            KeyBrushes[currentIndx] = brush;
            OnPropertyChanged($"KeyBrushes[{currentIndx}]");
            buttonStates[currentIndx] = e.ItemAction == ControlPanelItemAction.Down;
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
                device.ControlPanel.Stop();
                device = null;
            }
        }
    }

    internal class MinValueConverter : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.Min();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
