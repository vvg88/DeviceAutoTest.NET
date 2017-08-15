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
using NeuroSoft.Hardware.Devices;
using System.ComponentModel;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common.Scripts;

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// Interaction logic for FlashStimulatorSettings.xaml
    /// </summary>
    public partial class FlashStimulatorSettings : StimulatorSettings<IFlashStimulator>
    {
        /// <summary>
        /// Конструктор для дизайнера
        /// </summary>
        public FlashStimulatorSettings()
            : base(null)
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FlashStimulatorSettings(INsDevice device) 
            : base(device)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Длительность стимуляции (с)
        /// </summary>
        public double Duration
        {
            get { return Stimulator.ImpulseDuration; }
            set 
            {
                if (Stimulator.ImpulseDuration != value)
                {
                    Stimulator.ImpulseDuration = value;
                    OnPropertyChanged("Duration");
                    OnPropertyChanged("StimulationIsRun");
                }
            }
        }

        /// <summary>
        /// Период стимуляции (с)
        /// </summary>
        public double Period
        {
            get { return Stimulator.StimulationPeriod; }
            set
            {
                if (Stimulator.StimulationPeriod != value)
                {
                    Stimulator.StimulationPeriod = value;                    
                    OnPropertyChanged("Period");
                    OnPropertyChanged("StimulationIsRun");
                }
            }
        }

        /// <summary>
        /// Левая сторона стимуляции
        /// </summary>
        public bool LeftSide
        {
            get { return Stimulator.StimulationSide == StimulationSideEnum.Left; }
            set 
            {
                if (LeftSide != value)
                {                    
                    if (value)
                    {
                        RightSide = false;
                        BothSides = false;
                        Stimulator.StimulationSide = StimulationSideEnum.Left;
                    }
                    OnPropertyChanged("LeftSide");
                    OnPropertyChanged("StimulationIsRun");
                }
            }
        }

        /// <summary>
        /// Правая сторона стимуляции
        /// </summary>
        public bool RightSide
        {
            get { return Stimulator.StimulationSide == StimulationSideEnum.Right; }
            set
            {
                if (RightSide != value)
                {                                        
                    if (value)
                    {
                        LeftSide = false;
                        BothSides = false;
                        Stimulator.StimulationSide = StimulationSideEnum.Right;
                    }
                    OnPropertyChanged("RightSide");
                    OnPropertyChanged("StimulationIsRun");
                }
            }
        }
        /// <summary>
        /// Обе стороны стимуляции
        /// </summary>
        public bool BothSides
        {
            get { return Stimulator.StimulationSide == StimulationSideEnum.Both; }
            set
            {
                if (BothSides != value)
                {               
                    if (value)
                    {
                        RightSide = false;
                        LeftSide = false;
                        Stimulator.StimulationSide = StimulationSideEnum.Both;
                    }
                    OnPropertyChanged("BothSides");
                    OnPropertyChanged("StimulationIsRun");
                }
            }
        }
    }
}
