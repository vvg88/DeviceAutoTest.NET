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

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for PhotoStimulatorControl.xaml
    /// </summary>
    public partial class PhotoStimulatorControl : UserControl, INotifyPropertyChanged
    {
        public PhotoStimulatorControl(IFlashStimulator flashStimulator)
        {
            InitializeComponent();
            FlashStimulator = flashStimulator;

            Duration = FlashStimulator.GetStimulusDuration() * 1000;
            Period = 1.0 / FlashStimulator.GetFrequency();
            FlashStimulator.SetEnabled(true);
            FlashStimulator.StartStimulation();
            StimulationIsRun = true;
        }

        private IFlashStimulator FlashStimulator;
        private double duration;
        /// <summary>
        /// Длительность стимуляции в мс
        /// </summary>
        public double Duration
        {
            get { return duration; }
            set
            {
                if (duration != value)
                {
                    duration = value;
                    FlashStimulator.SetStimulusDuration((float)(Duration / 1000.0));
                    OnPropertyChanged("Duration");
                }
            }
        }

        private double period;
        /// <summary>
        /// Период стимуляции
        /// </summary>
        public double Period
        {
            get { return period; }
            set
            {
                if (period != value)
                {
                    period = value;
                    FlashStimulator.SetFrequency((float)(1.0 / Period));
                    OnPropertyChanged("Period");
                }
            }
        }

        private bool leftSide;
        /// <summary>
        /// Левая сторона стимуляции
        /// </summary>
        public bool LeftSide
        {
            get { return leftSide; }
            set
            {
                if (leftSide != value)
                {
                    leftSide = value;
                    OnPropertyChanged("LeftSide");
                    if (value)
                    {
                        FlashStimulator.SetStimulusSide(StimulusSide.Left);
                        RightSide = false;
                        BothSides = false;
                    }
                }
            }
        }
        private bool rightSide;
        /// <summary>
        /// Правая сторона стимуляции
        /// </summary>
        public bool RightSide
        {
            get { return rightSide; }
            set
            {
                if (rightSide != value)
                {
                    rightSide = value;
                    OnPropertyChanged("RightSide");
                    if (value)
                    {
                        FlashStimulator.SetStimulusSide(StimulusSide.Right);
                        LeftSide = false;
                        BothSides = false;
                    }
                }
            }
        }
        private bool bothSides = true;
        /// <summary>
        /// Обе стороны стимуляции
        /// </summary>
        public bool BothSides
        {
            get { return bothSides; }
            set
            {
                if (bothSides != value)
                {
                    bothSides = value;
                    OnPropertyChanged("BothSides");
                    if (value)
                    {
                        FlashStimulator.SetStimulusSide(StimulusSide.Both);
                        RightSide = false;
                        LeftSide = false;
                    }
                }
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

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    ApplySettingsAndStartStimulation();
        //}

        /// <summary>
        /// Применяет настройки и запускает стимуляцию
        /// </summary>
        //public void ApplySettingsAndStartStimulation()
        //{
        //    StimulusSide side = StimulusSide.Both;
        //    if (LeftSide)
        //        side = StimulusSide.Left;
        //    else if (RightSide)
        //        side = StimulusSide.Right;
        //    FlashStimulator.SetStimulusSide(side);
        //    //FlashStimulator.SetStimulusCount(1000);
        //    FlashStimulator.SetFrequency((float)(1.0 / Period));
        //    FlashStimulator.SetStimulusDuration((float)(Duration / 1000.0));
        //    FlashStimulator.SetEnabled(true);
        //    FlashStimulator.StartStimulation();
        //}
    }
}
