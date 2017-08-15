using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Devices;
using System.Collections.ObjectModel;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    class ImpedancesViewModel
    {
        ImpedancesViewModel(NeuroMEPMicro device)
        {
            this.device = device;
        }

        private NeuroMEPMicro device;
        /// <summary>
        /// 
        /// </summary>
        public NeuroMEPMicro Device
        {
            get { return device; }
            private set { device = value; }
        }

        private ObservableCollection<ImpedanceInfo> impedances = new ObservableCollection<ImpedanceInfo>();

        /// <summary>
        /// Коллекция импедансов
        /// </summary>
        public ObservableCollection<ImpedanceInfo> Impedances
        {
            get { return impedances; }
        }
    }
}
