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
    /// Interaction logic for PatternStimulatorSettings.xaml
    /// </summary>
    public partial class PatternStimulatorSettings : StimulatorSettings<IPatternStimulator>
    {
        /// <summary>
        /// Конструктор для дизайнера
        /// </summary>
        public PatternStimulatorSettings()
            : base(null)
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        public PatternStimulatorSettings(INsDevice device) 
            : base(device)
        {            
            InitializeComponent();            
        }
            
        private ISynchroOut _synchroOutModule;

        private ISynchroOut SynchroOutModule
        {
            get
            {
                if (_synchroOutModule == null && Device != null)
                {
                    _synchroOutModule = Device.FindModule<ISynchroOut>();
                }
                return _synchroOutModule;
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
        /// Тип поля
        /// </summary>
        public PatternTypeEnum PatternType
        {
            get { return Stimulator.FieldType; }
            set
            {
                if (Stimulator.FieldType != value)
                {
                    Stimulator.FieldType = value;
                    OnPropertyChanged("PatternType");
                    OnPropertyChanged("StimulationIsRun");
                }
            }
        }


        /// <summary>
        /// Размер точки
        /// </summary>
        public PatternPointSizeEnum PatternPointSize
        {
            get { return Stimulator.PointSize; }
            set
            {
                if (Stimulator.PointSize != value)
                {
                    Stimulator.PointSize = value;
                    OnPropertyChanged("PatternPointSize");
                    OnPropertyChanged("StimulationIsRun");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool OnStartStimulation()
        {
            if (SynchroOutModule != null)
            {
                SynchroOutModule.Enable = true;
                return true;
            }
            return false;
        }       
    }  
}
