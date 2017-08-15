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
using NeuroSoft.Hardware.Devices.Base;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for ForkStimulatorControl.xaml
    /// </summary>
    public partial class ForkStimulatorControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public ForkStimulatorControl(NeuroMepBase device)
        {
            InitializeComponent();
            DataContext = this;
            if (device != null)
            {
                Device = device;                
                Device.CurrentStimulatorForkStimulatorEvent += new ForkStimulatorEventHandler(ForkStimulatorButtonsClick);
            }
            buttonClikcs = new bool[5] { false, false, false, false, false };
            ButtonClikcs = new ReadOnlyCollection<bool>(buttonClikcs);
        }

        private NeuroMepBase device;
        /// <summary>
        /// 
        /// </summary>
        public NeuroMepBase Device
        {
            get { return device; }
            private set { device = value; }
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

        private ColorAnimation highlightAnimation = new ColorAnimation(Colors.Yellow, Colors.LightGray, new Duration(new TimeSpan(0, 0, 1)));

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventCode"></param>
        private void ForkStimulatorButtonsClick(object sender, ForkStimulatorEventEnum eventCode)
        {                                   
            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
            {
                switch (eventCode)
                {
                    case ForkStimulatorEventEnum.Pulse:
                        if (!buttonClikcs[0])
                            buttonClikcs[0] = true;
                        TopButtonBrush = new SolidColorBrush(Colors.Yellow);
                        TopButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                        break;
                    case ForkStimulatorEventEnum.Run:
                        if (!buttonClikcs[1])
                            buttonClikcs[1] = true;
                        CentralButtonBrush = new SolidColorBrush(Colors.Yellow);
                        CentralButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                        break;
                    case ForkStimulatorEventEnum.Flip:
                        if (!buttonClikcs[2])
                            buttonClikcs[2] = true;
                        LeftButtonBrush = new SolidColorBrush(Colors.Yellow);
                        RightButtonBrush = new SolidColorBrush(Colors.Yellow);
                        LeftButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                        RightButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);                      
                        break;
                    case ForkStimulatorEventEnum.Increment:
                        if (!buttonClikcs[3])
                            buttonClikcs[3] = true;
                        BottomButtonBrush = new SolidColorBrush(Colors.Yellow);
                        BottomButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                        break;
                    case ForkStimulatorEventEnum.Decrement:
                        if (!buttonClikcs[4])
                            buttonClikcs[4] = true;
                        UnderBottomButtonBrush = new SolidColorBrush(Colors.Yellow);
                        UnderBottomButtonBrush.BeginAnimation(SolidColorBrush.ColorProperty, highlightAnimation);
                        break;
                    
                }
            }));
        }

        /// <summary>
        /// Массив, содержащий информацию о нажатии кнопок на стимуляторе.
        /// </summary>
        public readonly ReadOnlyCollection<bool> ButtonClikcs;
        
        private bool[] buttonClikcs;

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
                Device.CurrentStimulatorForkStimulatorEvent -= new ForkStimulatorEventHandler(ForkStimulatorButtonsClick);
                Device = null;
            }
        }
    }
}
