using System.Linq;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.Hardware.Common;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Scripts
{
    /// <summary>
    /// 
    /// </summary>
    public class ImpedanceIndicator : Indicator
    {
        private NeuroMepBase Device;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="impedancesView"></param>
        /// <param name="device"></param>        
        public ImpedanceIndicator(ScriptEnvironment environment, ImpedancesView impedancesView, NeuroMepBase device) 
            : base(environment, impedancesView)
        {
            Environment = environment;
            Content = impedancesView;
            Device = device;            
        }
        /// <summary>
        /// Массив полных сопротивлений электродов
        /// </summary>
        private double[] lastImpedances;
        /// <summary>
        /// Массив активных сопротивлений электродов
        /// </summary>
        public RangedValue<double>[] LastResistances { get; private set; }
        /// <summary>
        /// Массив емкостей электродов
        /// </summary>
        public RangedValue<double>[] LastCapacities { get; private set; }
        /// <summary>
        /// Последнее считанное значение импедансов
        /// </summary>
        public double[] LastImpedances
        {
            get { return lastImpedances; }
            private set { lastImpedances = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {            
            if (Device != null)
            {
                if (!Device.Amplifier.ModuleIsOn)
                    Device.AmplifierModuleOn(errinfo => CommonScripts.ShowError(errinfo.Data.Descriptor.Message));
                Device.AmplifierImpedanceStart(device_AmplifierImpedanceEventNew, errInfo => CommonScripts.ShowError(errInfo.Data.Descriptor.Message));

            }
        }

        void device_AmplifierImpedanceEventNew(AsyncActionArgs<AmplifierImpedanceComplexData> e/*object sender, ImpedanceEventArgs e*/)
        {
            LastImpedances = new double[e.Data.Data.Length];
            LastResistances = new RangedValue<double>[e.Data.Data.Length];
            LastCapacities = new RangedValue<double>[e.Data.Data.Length];
            for (int i = 0; i < e.Data.Data.Length; i++)
            {
                LastImpedances[i] = e.Data.Data[i].Value.Magnitude;
            }
            ImpedancesView impedancesView = Content as ImpedancesView;
            if (impedancesView == null)
            {
                return;
            }
            impedancesView.UpdateImpedaces(e.Data.Data.Select(imp => imp.Value));
            for (int i = 0; i < impedancesView.Impedances.Count; i++)
            {
                LastResistances[i] = impedancesView.Impedances[i].Resistance;
                LastCapacities[i] = impedancesView.Impedances[i].Capacity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Stop()
        {
            if (Device != null)
            {
                Device.AmplifierImpedanceStop();
                Device.AmplifierPowerOff();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Device = null;
        }
    }
}
