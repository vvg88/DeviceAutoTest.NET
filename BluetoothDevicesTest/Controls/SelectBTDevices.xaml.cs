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
using NeuroSoft.Devices;
using NeuroSoft.Devices.Bluetooth;
using InTheHand.Net.Sockets;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.BluetoothDevicesTest;
using NeuroSoft.Common;

namespace NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Controls
{
    /// <summary>
    /// Логика взаимодействия для SelectBTDevices.xaml
    /// </summary>
    public partial class SelectBTDevices : UserControl, IDisposable
    {
        /// <summary>
        /// обеспечивает информацией о возможный устройствах полученных клиентом в процессе поиска
        /// </summary>
        public static List<BluetoothDeviceInfo> devices = new List<BluetoothDeviceInfo>();
        public static string nameBTDevice;
        public string nameTemp;
        public static int numSelectedDevice;
        ScriptEnvironment environment;
        long[] deviceAddress = new long[10];
        public static long addressBTDevice;

        public SelectBTDevices(ScriptEnvironment environment)
        {
            InitializeComponent();
            this.environment = environment;
            DataContext = this;
            OkBbutton.Visibility = Visibility.Hidden;
        }

        private void scanBTDevices_Click(object sender, RoutedEventArgs e)
        {
            listBoxBTDevices.Items.Clear();
            this.Cursor = Cursors.Wait;
            try
            {
                devices = BluetoothDevice.DiscoverDevices(10, true, true, true, false, nameTemp);
                int i = 0;
                foreach (var d in devices)
                {
                    listBoxBTDevices.Items.Add(d.DeviceName);
                    deviceAddress[i++] = ((long)(d.DeviceAddress.Nap) << 32) + (d.DeviceAddress.Sap);
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
                OkBbutton.Visibility = Visibility.Visible;
            }
        }

        private void OkBbutton_Click(object sender, RoutedEventArgs e)
        {
            object newItem;
            if (listBoxBTDevices.SelectedIndex > -1)
            {
                environment["nameBTDevice"] = listBoxBTDevices.Items[listBoxBTDevices.SelectedIndex].ToString();
                newItem = listBoxBTDevices.Items[listBoxBTDevices.SelectedIndex];
                nameBTDevice = listBoxBTDevices.Items[listBoxBTDevices.SelectedIndex].ToString();
                addressBTDevice = deviceAddress[listBoxBTDevices.SelectedIndex];
                listBoxBTDevices.Items.Clear();
                listBoxBTDevices.Items.Add(newItem);
                listBoxBTDevices.BorderBrush = Brushes.Green;
                listBoxBTDevices.Foreground = Brushes.Green;
            }            
        }

        public void Dispose()
        {
            environment["nameBTDevice"] = null;
//            throw new NotImplementedException();
        }
    }
}
