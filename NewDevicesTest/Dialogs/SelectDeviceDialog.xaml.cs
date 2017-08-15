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

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectDeviceDialog.xaml
    /// </summary>
    public partial class SelectDeviceDialog : DATDialogWindow, IUsbDevicesListChangeCallback
    {
        IUsbDevicesList usbDevicesList;
        private ObservableCollection<UsbDevicesListControl.Item> deviceItems = new ObservableCollection<UsbDevicesListControl.Item>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<UsbDevicesListControl.Item> DeviceItems
        {
            get { return deviceItems; }            
        }

        private UsbDevicesListControl.Item selectedDeviceItem;
        /// <summary>
        /// Выбранный элемент в списке USB устройств
        /// </summary>
        public UsbDevicesListControl.Item SelectedDeviceItem
        {
            get
            {
                return selectedDeviceItem;
            }
            set
            {
                selectedDeviceItem = value;
                OnPropertyChanged("SelectedDeviceItem");
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>        
        public SelectDeviceDialog()
        {
            InitializeComponent();
            usbDevicesList = NeuroSoft.Hardware.Usb.UsbCommon.GetDevicesList();
            DevicesListRefreshManager.AutoRefreshStart();
            usbDevicesList = UsbCommon.GetDevicesList();
            usbDevicesList.ChangeCallbackAdd(this);
            UpdateDeviceList();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {                       
            DialogResult = true;            
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DevicesListRefreshManager.AutoRefreshStop();
        }

        #region IUsbDevicesListChangeCallback Members

        void IUsbDevicesListChangeCallback.OnRefresh(IUsbDevicesList sender)
        {
            if (sender != usbDevicesList)
                throw new ApplicationException();
        }

        void IUsbDevicesListChangeCallback.OnChange(IUsbDevicesList sender, ListChangeActionEnum changeAction, IUsbDevicesListItem item)
        {
            if (sender != usbDevicesList)
                throw new ApplicationException();
            //switch (changeAction)
            //{
            //    case ListChangeActionEnum.Delete:
            //        var deviceItem = (from di in DeviceItems where di.UsbListItem == item select di).FirstOrDefault();
            //        DeviceItems.Remove(deviceItem);                   
            //        break;

            //    case ListChangeActionEnum.Add:
            //        DeviceItems.Add(new NeuroSoft.Hardware.Tools.Usb.UsbDevicesListControl.Item(item));
            //        break;

            //    default:
            //        throw new ArgumentException();
            //}
            UpdateDeviceList();
        }

        private void UpdateDeviceList()
        {
            DeviceItems.Clear();
            foreach (var device in usbDevicesList)
            {
                if (device.UsbDeviceDescriptor != null)
                {
                    if (device.UsbDeviceDescriptor.idProduct == 28674)
                    {
                        continue;
                    }
                }
                //необходимо добавить фильтрацию стендов тестирования
                DeviceItems.Add(new UsbDevicesListControl.Item(device));
            }
        }
        #endregion

        private void ListBoxItem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SelectedDeviceItem != null)
            {
                DialogResult = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UpdateDeviceList();
        }
    }
}
