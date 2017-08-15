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
    /// Interaction logic for CurrentStimulatorSettings.xaml
    /// </summary>
    public partial class CurrentStimulatorSettings : StimulatorSettings<ICurrentStimulatorExt>
    {
        /// <summary>
        /// Конструктор для дизайнера
        /// </summary>
        public CurrentStimulatorSettings()
            : base(null)
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public CurrentStimulatorSettings(INsDevice device) 
            : base(device)
        {            
            InitializeComponent();            
            Stimulator.CurrentStimulExtEvent += new CurrentStimulusExtEventHandler(Stimulator_CurrentStimulExtEvent);
            Stimulator.CurrentStimulEvent += new CurrentStimulusEventHandler(Stimulator_CurrentStimulEvent);
            //Stimulator.VoltageEvent += new VoltageEventHandler(Stimulator_VoltageEvent);
        }

        #region Properties
        private float lastCurrent;        
        /// <summary>
        /// Последнее считанное значение амплитуды основного стимула
        /// </summary>
        public float LastCurrent
        {
            get { return lastCurrent; }
            private set
            {
                if (lastCurrent != value)
                {
                    lastCurrent = value;
                    OnPropertyChanged("LastCurrent");
                }
            }
        }

        private float lastCurrentEx;
        /// <summary>
        /// Последнее считанное значение амплитуды дополнительного стимула
        /// </summary>
        public float LastCurrentEx
        {
            get { return lastCurrentEx; }
            private set
            {
                if (value != lastCurrentEx)
                {
                    lastCurrentEx = value;
                    OnPropertyChanged("LastCurrentEx");
                }
            }
        }

        private float lastVoltage;
        /// <summary>
        /// Последнее считанное значение напряжения
        /// </summary>
        public float LastVoltage
        {
            get { return lastVoltage; }
            private set
            {
                if (lastVoltage != value)
                {
                    lastVoltage = value;
                    OnPropertyChanged("LastVoltage");
                }
            }
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
                }
            }
        }
        
        /// <summary>
        /// Величина тока импульса стимула (мА)
        /// </summary>
        public double CurrentValue
        {
            get { return Stimulator.Value; }
            set
            {
                if (value != Stimulator.Value)
                {
                    Stimulator.Value = value;
                    OnPropertyChanged("CurrentValue");
                }
            }
        }

        
        /// <summary>
        /// Номер выхода, используемого для стимуляции
        /// </summary>
        public int OutputNumber
        {
            get { return Stimulator.OutputNumber; }
            set
            {
                if (Stimulator.OutputNumber != value)
                {
                    Stimulator.OutputNumber = value;
                    OnPropertyChanged("OutputNumber");
                }
            }
        }

        
        /// <summary>
        /// Инвертирование полярности импульса стимула
        /// </summary>
        public bool InversePolarity
        {
            get { return Stimulator.InversingPolarity; }
            set
            {
                if (Stimulator.InversingPolarity != value)
                {
                    Stimulator.InversingPolarity = value;
                    OnPropertyChanged("InversePolarity");                    
                }
            }
        }

        
        /// <summary>
        /// Режим работы стимулятора
        /// </summary>
        public CurrentStimulatorExtModeEnum StimulationMode
        {
            get { return Stimulator.Mode; }
            set
            {
                if (Stimulator.Mode != value)
                {
                    Stimulator.Mode = value;
                    OnPropertyChanged("StimulationMode");
                }
            }
        }

        /// <summary>
        /// Длительность дополнительного стимула при парной стимуляции (с)
        /// </summary>
        public double AdditionalPulseDuration
        {
            get { return Stimulator.AdditionalPulseDuration; }
            set
            {
                if (Stimulator.AdditionalPulseDuration != value)
                {
                    Stimulator.AdditionalPulseDuration = value;
                    OnPropertyChanged("AdditionalDuration");
                }
            }
        }

        /// <summary>
        /// Отступ между началами импульсов при парной стимуляции. (с)
        /// </summary>
        public double AdditionalPulseInterval
        {
            get { return Stimulator.AdditionalPulseInterval; }
            set
            {
                if (Stimulator.AdditionalPulseInterval != value)
                {
                    Stimulator.AdditionalPulseInterval = value;
                    OnPropertyChanged("AdditionalPulseInterval");                    
                }
            }
        }

        /// <summary>
        /// Величина тока дополнительного стимула при парной стимуляции, мА.
        /// </summary>
        public double AdditionalPulseValue
        {
            get { return Stimulator.AdditionalPulseValue; }
            set
            {
                if (value != Stimulator.AdditionalPulseValue)
                {
                    Stimulator.AdditionalPulseValue = value;
                    OnPropertyChanged("AdditionalPulseValue");
                }
            }
        }


        /// <summary>
        /// Номер выхода дополнительного стимула.
        /// </summary>
        public int AdditionalOutputNumber
        {
            get { return Stimulator.AdditionalPulseOutputNumber; }
            set
            {
                if (Stimulator.AdditionalPulseOutputNumber != value)
                {                    
                    Stimulator.AdditionalPulseOutputNumber = value;
                    OnPropertyChanged("AdditionalOutputNumber");
                }
            }
        }


        /// <summary>
        /// Инвертирование полярности дополнительного стимула.
        /// </summary>
        public bool AdditionalInversePolarity
        {
            get { return Stimulator.AdditionalPulseInversingPolarity; }
            set
            {
                if (Stimulator.AdditionalPulseInversingPolarity != value)
                {
                    Stimulator.AdditionalPulseInversingPolarity = value;
                    OnPropertyChanged("AdditionalInversePolarity");
                }
            }
        }

        /// <summary>
        /// Количество импульсов в трейне.
        /// </summary>
        public int TrainPulseCount
        {
            get { return Stimulator.TrainPulseCount; }
            set
            {
                if (Stimulator.TrainPulseCount != value)
                {
                    Stimulator.TrainPulseCount = value;
                    OnPropertyChanged("TrainPulseCount");
                }
            }
        }

        /// <summary>
        /// Количество импульсов в трейне.
        /// </summary>
        public double TrainPulseInterval
        {
            get { return Stimulator.TrainPulseInterval; }
            set
            {
                if (Stimulator.TrainPulseInterval != value)
                {
                    Stimulator.TrainPulseInterval = value;
                    OnPropertyChanged("TrainPulseInterval");
                }
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            if (Stimulator != null)
            {
                Stimulator.CurrentStimulEvent -= new CurrentStimulusEventHandler(Stimulator_CurrentStimulEvent);
                Stimulator.CurrentStimulExtEvent -= new CurrentStimulusExtEventHandler(Stimulator_CurrentStimulExtEvent);
                //Stimulator.VoltageEvent -= new VoltageEventHandler(Stimulator_VoltageEvent);
            }
            base.Dispose();
        }

        #region Event Handlers
        private long updateCurrentTime = 0;
        private long updateCurrentExtTime = 0;
        private int updateCurrentPeriodTicks = 5000000; //период обновления значений индикаторов (в тиках)

        void Stimulator_CurrentStimulEvent(object sender, float current)
        {
            lastCurrent = current;
            long now = DateTime.UtcNow.Ticks;
            if (now - updateCurrentTime > updateCurrentPeriodTicks)
            {
                updateCurrentTime = now;
                Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OnPropertyChanged("LastCurrent");
                    }));
            }
        }      
        void Stimulator_CurrentStimulExtEvent(object sender, float current, bool isPaired)
        {
            lastCurrentEx = current;
            long now = DateTime.UtcNow.Ticks;
            if (now - updateCurrentExtTime > updateCurrentPeriodTicks)
            {
                updateCurrentExtTime = now;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    OnPropertyChanged("LastCurrentEx");
                }));
            }    
        }
        void Stimulator_VoltageEvent(object sender, float value)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LastVoltage = value;
            })); 
        }
        #endregion
    }
}
