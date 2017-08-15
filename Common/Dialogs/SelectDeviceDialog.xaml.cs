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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using NeuroSoft.Hardware.Tools.Usb;
using NeuroSoft.Hardware.Usb;
using NeuroSoft.Hardware.Common;
using NeuroSoft.DeviceAutoTest.Common.Controls;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectDeviceDialog.xaml
    /// </summary>
    public partial class SelectDeviceDialog : DATDialogWindow
    {
        public UsbDevInfo SelectedDeviceItem => UsbDevsList.SelectedDeviceItem;

        /// <summary>
        /// Конструктор
        /// </summary>        
        public SelectDeviceDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {                       
            DialogResult = true;            
        }
    }
}
