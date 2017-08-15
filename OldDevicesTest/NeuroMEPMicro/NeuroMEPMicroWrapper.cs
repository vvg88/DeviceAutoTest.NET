using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest
{
    public class NeuroMEPMicroWrapper : INsDeviceWrapper
    {
        private NeuroMEPMicro device;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public NeuroMEPMicroWrapper(NeuroMEPMicro device) 
        {
            Device = device;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            var device = Device as NeuroMEPMicro;
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
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
                device = value as NeuroMEPMicro;
            }
        }
    }
}
