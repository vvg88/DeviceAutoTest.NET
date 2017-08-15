using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest
{
    /// <summary>
    /// 
    /// </summary>
    public class NeuroMepMicroMWrapper : INsDeviceWrapper
    {
        private NeuroMepMicroM2 device;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public NeuroMepMicroMWrapper(NeuroMepMicroM2 device) 
        {
            Device = device;            
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            var device = Device as NeuroMepMicroM2;
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(()=>
            {
                if (device != null)
                {
                    device.Close();
                }
            }));
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(() => { }));
        }

        /// <summary>
        /// 
        /// </summary>
        public object Device
        {
            get
            {
                return device;
            }
            private set
            {
                device = value as NeuroMepMicroM2;
            }
        }
    }
}
