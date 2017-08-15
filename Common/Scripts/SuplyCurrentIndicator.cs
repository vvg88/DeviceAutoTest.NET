using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using System.Windows.Controls;
using NeuroSoft.Hardware.Devices;
using System.Windows.Media;
using NeuroSoft.Devices;

namespace NeuroSoft.DeviceAutoTest.Common.Scripts
{
    /// <summary>
    /// 
    /// </summary>
    public class SupplyCurrentIndicator : Indicator
    {
        private float sumCurrent = 0;
        private int samples = 0;
        private float lastSupplyCurrent;
        private double supplyCurrentMin = 30;        
        private double supplyCurrentMax = 50;
        
        /// <summary>
        /// Последнее среднее за секунду значение тока потребления
        /// </summary>
        public float LastSupplyCurrent
        {
            get { return lastSupplyCurrent; }
            private set
            {
                if (lastSupplyCurrent != value)
                {
                    lastSupplyCurrent = value;
                    OnPropertyChanged("LastSupplyCurrent");
                    OnPropertyChanged("IsValidCurrent");
                }
            }
        }

        /// <summary>
        /// Признак того, находится ли последнее (текущее) значение тока в допустимом диапазоне
        /// </summary>
        public bool IsValidCurrent
        {
            get 
            {
                return lastSupplyCurrent > SupplyCurrentMin && lastSupplyCurrent < SupplyCurrentMax; 
            }
        }

        /// <summary>
        /// Минимально допустимое значение тока потребления
        /// </summary>
        public double SupplyCurrentMin
        {
            get { return supplyCurrentMin; }
            set { supplyCurrentMin = value; }
        }
        /// <summary>
        /// Максимально допустимое значение тока потребления
        /// </summary>
        public double SupplyCurrentMax
        {
            get { return supplyCurrentMax; }
            set { supplyCurrentMax = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="stand"></param>
        public SupplyCurrentIndicator(ScriptEnvironment environment) 
            : base(environment, null)
        {
            Environment = environment;            
        }


        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            if (Environment.Stand != null && Environment.Stand.DeviceOpened)
            {
                if (Environment.Stand.SupplyCurrentFlowStarted)
                {
                    Environment.Stand.SupplyCurrentReadingStop();
                }
                Environment.Stand.SupplyCurrentReadingStart(SupplyCurrentSampleFreq._1_kHz, ReadSupplyCurrent);
            }
            base.Start();
        }

        protected override void OnTimerTick()
        {
            base.OnTimerTick();
            if (samples != 0)
                LastSupplyCurrent = sumCurrent / samples;            
            /*if (samples == 0)*/
            else       // если количество отсчетов равно 0, то, вероятно, стенд был выключен. Попробуем начать чтение данных заново
            {
                if (!(Environment.Device is EEG5Device))  // Если девайс не энцефалограф, так как для него резирвируется 
                {
                    if (Environment.Stand != null)
                    {
                        Environment.Stand.Close();
                    }
                    Environment.OpenStand();
                    Start();
                }
            }
            sumCurrent = 0;
            samples = 0;
        }


        private void ReadSupplyCurrent(float[][] supplyCurrentsArray, int samplesNumInArray, uint numOfFirstSample)
        {
            foreach (float[] supCurSample in supplyCurrentsArray)
            {
                if (/*supplyCurrentsArray[0]*/supCurSample != null)
                {
                    sumCurrent += supCurSample[0]/*supplyCurrentsArray[0][0]*/;
                    samples++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            if (Environment.Stand != null && Environment.Stand.DeviceOpened)
            {
                if (Environment.Stand.SupplyCurrentFlowStarted)
                {
                    Environment.Stand.SupplyCurrentReadingStop();
                }
            }
        }
    }
}
