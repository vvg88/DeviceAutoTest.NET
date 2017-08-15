using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest
{
    public class NSDeviceWrapper : INsDeviceWrapper
    {
        private NSDevice device;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public NSDeviceWrapper(NSDevice device) 
        {
            Device = device;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            var device = Device as NSDevice;
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>     // Removed begininvoke as it caused opening device before its closing
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
                device = value as NSDevice;
            }
        }
    }
}
