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
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for PatternStimualtorControl.xaml
    /// </summary>
    public partial class PatternStimualtorControl : UserControl, INotifyPropertyChanged
    {
        public PatternStimualtorControl(IEEGPatternStimulator patternStimulator)
        {
            InitializeComponent();
            PatternStimulator = patternStimulator;

            Period = 1.0 / PatternStimulator.GetFrequency();
            PatternPole = PatternStimulator.GetPole();
            DataContext = this;
        }

        IEEGPatternStimulator PatternStimulator;
        /// <summary>
        /// Период стимуляции
        /// </summary>
        public double Period
        {
            get { return 1.0 / PatternStimulator.GetFrequency(); }
            set
            {
                PatternStimulator.SetFrequency((float)(1.0 / value));
                OnPropertyChanged("Period");
            }
        }
        /// <summary>
        /// Поле паттерна
        /// </summary>
        public PatternStimulationPole PatternPole
        {
            get { return PatternStimulator.GetPole(); }
            set
            {
                PatternStimulator.SetPole(value);
                OnPropertyChanged("PatternPole");
            }
        }

        private bool stimulationIsRun = false;
        /// <summary>
        /// Признак запуска стимуляции
        /// </summary>
        public bool StimulationIsRun
        {
            get { return stimulationIsRun; }
            set
            {
                stimulationIsRun = value;
                OnPropertyChanged("StimulationIsRun");
            }
        }

        /// <summary>
        /// Запускает стимуляцию
        /// </summary>
        public void StarStimulation()
        {
            PatternStimulator.SetEnabled(true);
            PatternStimulator.StartStimulation();
            StimulationIsRun = true;
        }

        /// <summary>
        /// Останавливает стимуляцию
        /// </summary>
        public void StopStimulation()
        {
            PatternStimulator.SetEnabled(false);
            PatternStimulator.StopStimulation();
            StimulationIsRun = false;
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
