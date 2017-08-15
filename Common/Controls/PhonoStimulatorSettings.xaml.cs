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
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// Interaction logic for PhonoStimulatorSettings.xaml
    /// </summary>
    public partial class PhonoStimulatorSettings : StimulatorSettings<IPhonoStimulator>
    {
        /// <summary>
        /// Конструктор для дизайнера
        /// </summary>
        public PhonoStimulatorSettings()
            : base(null)
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public PhonoStimulatorSettings(INsDevice device) 
            : base(device)
        {
            InitializeComponent();
            Stimulator.IsMeanLeft = true;
            Stimulator.IsMeanRight = true;
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
                }
            }
        }

        /// <summary>
        /// Признак включения маскирующего шума в левом канале
        /// </summary>
        public bool NoiseLeftOn
        {
            get { return Stimulator.NoiseEnableLeft; }
            set
            {
                if (Stimulator.NoiseEnableLeft != value)
                {
                    Stimulator.NoiseEnableLeft = value;
                    OnPropertyChanged("NoiseLeftOn");                    
                }
            }
        }

        /// <summary>
        /// Признак включения маскирующего шума в правом канале
        /// </summary>
        public bool NoiseRightOn
        {
            get { return Stimulator.NoiseEnableRight; }
            set
            {
                if (Stimulator.NoiseEnableRight != value)
                {
                    Stimulator.NoiseEnableRight = value;
                    OnPropertyChanged("NoiseRightOn");                    
                }
            }
        }

        /// <summary>
        /// Усиление сигнала шума в левом канале
        /// </summary>
        public double NoiseGainLeft
        {
            get { return Stimulator.NoiseGainLeft; }
            set
            {
                if (Stimulator.NoiseGainLeft != value)
                {
                    Stimulator.NoiseGainLeft = value;                    
                    OnPropertyChanged("NoiseGainLeft");                    
                }
            }
        }

        /// <summary>
        /// Усиление сигнала шума в правом канале
        /// </summary>
        public double NoiseGainRight
        {
            get { return Stimulator.NoiseGainRight; }
            set
            {
                if (Stimulator.NoiseGainRight != value)
                {
                    Stimulator.NoiseGainRight = value;
                    OnPropertyChanged("NoiseGainRight");                    
                }
            }
        }

        /// <summary>
        /// Положительная полярность стимулов для левой стороны стимуляции
        /// </summary>
        public bool LeftSidePolarity
        {
            get { return Stimulator.PolarityMeanLeft; }
            set
            {
                if (Stimulator.PolarityMeanLeft != value)
                {
                    Stimulator.PolarityMeanLeft = value;
                    OnPropertyChanged("LeftSidePolarity");                    
                }
            }
        }

        /// <summary>
        /// Положительная полярность стимулов для правой стороны стимуляции
        /// </summary>
        public bool RightSidePolarity
        {
            get { return Stimulator.PolarityMeanRight; }
            set
            {
                if (Stimulator.PolarityMeanRight != value)
                {
                    Stimulator.PolarityMeanRight = value;
                    OnPropertyChanged("RightSidePolarity");                  
                }
            }
        }

        /// <summary>
        /// Смена полярности стимулов для левой стороны стимуляции
        /// </summary>
        public bool LeftSideChangePolarity
        {
            get { return Stimulator.FlipMeanLeft; }
            set
            {
                if (Stimulator.FlipMeanLeft != value)
                {
                    Stimulator.FlipMeanLeft = value;
                    OnPropertyChanged("LeftSideChangePolarity");
                }
            }
        }

        /// <summary>
        /// Смена полярности стимулов для правой стороны стимуляции
        /// </summary>
        public bool RightSideChangePolarity
        {
            get { return Stimulator.FlipMeanRight; }
            set
            {
                if (Stimulator.FlipMeanRight != value)
                {
                    Stimulator.FlipMeanRight = value;
                    OnPropertyChanged("RightSideChangePolarity");                    
                }
            }
        }
        
        /// <summary>
        /// Усиление для левой стороны стимуляции
        /// </summary>
        public double ValueMeanLeft
        {
            get { return Stimulator.ValueMeanLeft; }
            set
            {
                if (Stimulator.ValueMeanLeft != value)
                {
                    Stimulator.ValueMeanLeft = value;
                    OnPropertyChanged("ValueMeanLeft");                    
                }
            }
        }

        /// <summary>
        /// Усиление для правой стороны стимуляции
        /// </summary>
        public double ValueMeanRight
        {
            get { return Stimulator.ValueMeanRight; }
            set
            {
                if (Stimulator.ValueMeanRight != value)
                {
                    Stimulator.ValueMeanRight = value;
                    OnPropertyChanged("ValueMeanRight");                    
                }
            }
        }

        /// <summary>
        /// Длительность одного импульса для левой стороны стимуляции
        /// </summary>
        public double LeftImpulseDuration
        {
            get { return Stimulator.PulseDurationMeanLeft; }
            set
            {
                if (Stimulator.PulseDurationMeanLeft != value)
                {
                    Stimulator.PulseDurationMeanLeft = value;
                    OnPropertyChanged("LeftImpulseDuration");                    
                }
            }
        }

        /// <summary>
        /// Длительность одного импульса для правой стороны стимуляции
        /// </summary>
        public double RightImpulseDuration
        {
            get { return Stimulator.PulseDurationMeanRight; }
            set
            {
                if (Stimulator.PulseDurationMeanRight != value)
                {
                    Stimulator.PulseDurationMeanRight = value;
                    OnPropertyChanged("RightImpulseDuration");                    
                }
            }
        }

        /// <summary>
        /// Количество импульсов в значимом стимуле левого канала
        /// </summary>
        public int PulseCountMeanLeft
        {
            get { return Stimulator.PulseCountMeanLeft; }
            set
            {
                if (PulseCountMeanLeft != value)
                {
                    Stimulator.PulseCountMeanLeft = value;
                    OnPropertyChanged("PulseCountMeanLeft");
                }
            }
        }

        /// <summary>
        /// Количество импульсов в значимом стимуле правого канала
        /// </summary>
        public int PulseCountMeanRight
        {
            get { return Stimulator.PulseCountMeanRight; }
            set
            {
                if (Stimulator.PulseCountMeanRight != value)
                {
                    Stimulator.PulseCountMeanRight = value;
                    OnPropertyChanged("PulseCountMeanRight");
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
                }
            }
        }
    }
}
