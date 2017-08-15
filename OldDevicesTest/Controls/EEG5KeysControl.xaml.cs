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
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for EEG5KeysControl.xaml
    /// </summary>
    public partial class EEG5KeysControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        public EEG5KeysControl(EEG5Device device)
        {
            InitializeComponent();
            this.device = device;
            DataContext = this;
            device.CurrentStimulatorButtonClick += new CurrentStimulatorButtonClick(CurrentStimulatorButtonClick_Handler);
            device.KeyPressed += new ButtonFunc(device_KeyPressed);
            buttonClicks = new bool[6] { false, false, false, false, false, false };
        }

        private EEG5Device device;
        /// <summary>
        /// Ссылка на экземпляр объекта
        /// </summary>
        public EEG5Device Device
        {
            get { return device; }
            set { device = value; }
        }
        

        private Brush leftButtonBrush = Brushes.LightGray;

        /// <summary>
        /// Кисть левой кнопки
        /// </summary>
        public Brush LeftButtonBrush
        {
            get { return leftButtonBrush; }
            set
            {
                leftButtonBrush = value;
                OnPropertyChanged("LeftButtonBrush");
            }
        }

        private Brush centralButtonBrush = Brushes.LightGray;

        /// <summary>
        /// Кисть центральной кнопки
        /// </summary>
        public Brush CentralButtonBrush
        {
            get { return centralButtonBrush; }
            set
            {
                centralButtonBrush = value;
                OnPropertyChanged("CentralButtonBrush");
            }
        }

        private Brush rigtButtonBrush = Brushes.LightGray;

        /// <summary>
        /// Кисть правой кнопки
        /// </summary>
        public Brush RightButtonBrush
        {
            get { return rigtButtonBrush; }
            set
            {
                rigtButtonBrush = value;
                OnPropertyChanged("RightButtonBrush");
            }
        }

        private Brush topButtonBrush = Brushes.LightGray;

        /// <summary>
        /// Кисть верхней кнопки
        /// </summary>
        public Brush TopButtonBrush
        {
            get { return topButtonBrush; }
            set
            {
                topButtonBrush = value;
                OnPropertyChanged("TopButtonBrush");
            }
        }

        private Brush bottomButtonBrush = Brushes.LightGray;

        /// <summary>
        /// Кисть нижней кнопки
        /// </summary>
        public Brush BottomButtonBrush
        {
            get { return bottomButtonBrush; }
            set
            {
                bottomButtonBrush = value;
                OnPropertyChanged("BottomButtonBrush");
            }
        }

        private Brush underBottomButtonBrush = Brushes.LightGray;

        /// <summary>
        /// Кисть самой нижней кнопки
        /// </summary>
        public Brush UnderBottomButtonBrush
        {
            get { return underBottomButtonBrush; }
            set
            {
                underBottomButtonBrush = value;
                OnPropertyChanged("UnderBottomButtonBrush");
            }
        }

        private Brush deviceButtonBrush = Brushes.LightGray;

        /// <summary>
        /// Кисть самой нижней кнопки
        /// </summary>
        public Brush DeviceButtonBrush
        {
            get { return deviceButtonBrush; }
            set
            {
                deviceButtonBrush = value;
                OnPropertyChanged("DeviceButtonBrush");
            }
        }

        /// <summary>
        /// Возвращает состояние всех кнопок (были нажаты или нет)
        /// </summary>
        public bool? AllButtonsChecked
        {
            get
            {
                for (int i = 0; i < buttonClicks.Length; i++)
                {
                    if (buttonClicks[i] == false)
                        return null;
                }
                return true;
            }
        }

        private ColorAnimation highlightAnimation = new ColorAnimation(Colors.Yellow, Colors.LightGray, new Duration(new TimeSpan(0, 0, 1)));

        /// <summary>
        /// Массив с нажатиями на кнопки (были нажатия или нет)
        /// </summary>
        private bool[] buttonClicks;

        /// <summary>
        /// Обработчик нажатий кнопок на вилочковом токовом стимуляторе
        /// </summary>
        /// <param name="button"></param>
        void CurrentStimulatorButtonClick_Handler(NeuroMEP4CurrentStimulatorButtons button)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
                {
                    switch (button)
                    {
                        case NeuroMEP4CurrentStimulatorButtons.Minus:
                            if (buttonClicks[0] != true)
                                buttonClicks[0] = true;
                            UnderBottomButtonBrush = new SolidColorBrush(Colors.Yellow);
                            UnderBottomButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                            break;
                        case NeuroMEP4CurrentStimulatorButtons.Pause:
                            if (buttonClicks[1] != true)
                                buttonClicks[1] = true;
                            
                            CentralButtonBrush = new SolidColorBrush(Colors.Yellow);
                            CentralButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                            break;
                        case NeuroMEP4CurrentStimulatorButtons.Plus:
                            if (buttonClicks[2] != true)
                                buttonClicks[2] = true;
                            BottomButtonBrush = new SolidColorBrush(Colors.Yellow);
                            BottomButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                            break;
                        case NeuroMEP4CurrentStimulatorButtons.Polaryty:
                            if (buttonClicks[3] != true)
                                buttonClicks[3] = true;
                            LeftButtonBrush = new SolidColorBrush(Colors.Yellow);
                            RightButtonBrush = new SolidColorBrush(Colors.Yellow);
                            LeftButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                            RightButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                            break;
                        case NeuroMEP4CurrentStimulatorButtons.StartStimulation:
                            if (buttonClicks[4] != true)
                                buttonClicks[4] = true;
                            TopButtonBrush = new SolidColorBrush(Colors.Yellow);
                            TopButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                            break;
                    }
                }));
        }

        /// <summary>
        /// Обработчик нажатия кнопки на устройстве
        /// </summary>
        /// <param name="pressed"> Код нажатия (1 - кнопка нажата, 0 - отжата)</param>
        void device_KeyPressed(int pressed)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
                {
                    if (pressed == 1)
                    {
                        if (buttonClicks[5] != true)
                            buttonClicks[5] = true;
                        DeviceButtonBrush = new SolidColorBrush(Colors.Yellow);
                        DeviceButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                    }
                }));
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
            if (Device != null)
            {
                Device.CurrentStimulatorButtonClick -= new CurrentStimulatorButtonClick(CurrentStimulatorButtonClick_Handler);
                Device = null;
            }
        }
    }
}
