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
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices.Bluetooth;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Controls
{
    /// <summary>
    /// Логика взаимодействия для BreakeElectrode.xaml
    /// </summary>
    public partial class BreakeElectrode : UserControl, IDisposable
    {
        ScriptEnvironment environment;
        public static BluetoothDevice Device8EXv2012;
        public static PolySpectrum8EX Device8EX;

        public BreakeElectrode(ScriptEnvironment environment)
        {
            this.environment = environment;
            InitializeComponent();
            DataContext = this;
            if (SelectBTDevices.nameBTDevice.IndexOf(PolySpectrum8EX_2012.DeviceNameSign) > -1)
            {
                Device8EXv2012 = new BluetoothDevice();
            }
            if (SelectBTDevices.nameBTDevice.IndexOf(PolySpectrum8EX.DeviceNameSign) > -1)
            {
                Device8EX = new PolySpectrum8EX();
                Device8EX.BTAddress = SelectBTDevices.addressBTDevice;
                Device8EX.Open();
                Device8EX.ReceiveData += new ReceiveDataDelegate(Device8EX_ReceiveData);
                Device8EX.BeginTransmit();
            }
        }

        void Device8EX_ReceiveData(object sender, ReceiveDataArgs e)
        {
            IsBreaked(labelL, ECGElectrode.L);
            IsBreaked(labelF, ECGElectrode.F);
            IsBreaked(labelC1, ECGElectrode.C1);
            IsBreaked(labelC2, ECGElectrode.C2);
            IsBreaked(labelC3, ECGElectrode.C3);
            IsBreaked(labelC4, ECGElectrode.C4);
            IsBreaked(labelC5, ECGElectrode.C5);
            IsBreaked(labelC6, ECGElectrode.C6);
            IsBreaked(labelR, ECGElectrode.R);
            IsBreaked(labelN, ECGElectrode.N);
        }

        void IsBreaked(Label l, ECGElectrode electrode)
        {
            if (Device8EX.IsBreaked(electrode))
            {
                l.Foreground = Brushes.Red;
            }
            else
            {
                l.Foreground = Brushes.GreenYellow;
            }
        }

        public void Dispose()
        {
            if (SelectBTDevices.nameBTDevice.IndexOf(PolySpectrum8EX_2012.DeviceNameSign) > -1)
            {
                Device8EXv2012 = null;
            }
            if (SelectBTDevices.nameBTDevice.IndexOf(PolySpectrum8EX.DeviceNameSign) > -1)
            {
                Device8EX.StopTransmit();
                Device8EX.Close();
                Device8EX = null;
            }
        }

        private void checkBoxL_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 4, false);
        }

        private void checkBoxL_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 4, true);
        }

        private void checkBoxF_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 6, false);
        }

        private void checkBoxF_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 6, true);
        }

        private void checkBoxC1_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 8, false);
        }

        private void checkBoxC1_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 8, true);
        }

        private void checkBoxC2_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 1, false);
        }

        private void checkBoxC2_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 1, true);
        }

        private void checkBoxC3_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 3, false);
        }

        private void checkBoxC3_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 3, true);
        }

        private void checkBoxC4_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 5, false);
        }

        private void checkBoxC4_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 5, true);
        }

        private void checkBoxC5_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 7, false);
        }

        private void checkBoxC5_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 7, true);
        }

        private void checkBoxC6_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 9, false);
        }

        private void checkBoxC6_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 9, true);
        }

        private void checkBoxR_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 2, false);
        }

        private void checkBoxR_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 2, true);
        }

        private void checkBoxN_Checked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 12, false);
        }

        private void checkBoxN_Unchecked(object sender, RoutedEventArgs e)
        {
            StandOperation.BreakElectrode(this.environment, 12, true);
        }

    }
}
