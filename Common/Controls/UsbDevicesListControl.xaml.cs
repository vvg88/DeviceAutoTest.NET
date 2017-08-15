using AvalonDock;
using NeuroSoft.Hardware.Usb;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// Interaction logic for UsbDevicesListControl.xaml
    /// </summary>
    public partial class UsbDevicesListControl : DockableContent
    {
        IEnumerable<IUsbDevice> usbDevicesList;
        private ObservableCollection<UsbDevInfo> deviceItems = new ObservableCollection<UsbDevInfo>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<UsbDevInfo> DeviceItems
        {
            get { return deviceItems; }
        }

        private UsbDevInfo selectedDeviceItem;
        /// <summary>
        /// Выбранный элемент в списке USB устройств
        /// </summary>
        public UsbDevInfo SelectedDeviceItem
        {
            get
            {
                return selectedDeviceItem;
            }
            set
            {
                selectedDeviceItem = value;
                NotifyPropertyChanged("SelectedDeviceItem");
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>        
        public UsbDevicesListControl()
        {
            InitializeComponent();
            usbDevicesList = UsbCommon.DevicesEnumerator;
            usbDevicesList = UsbCommon.DevicesEnumerator;
            UpdateDeviceList();
            DataContext = this;
        }

        private void UpdateDeviceList()
        {
            DeviceItems.Clear();
            foreach (var device in UsbCommon.DevicesEnumerator)
            {
                var desc = device.DeviceDescriptor;
                if (desc.idProduct == 28674)//необходимо добавить фильтрацию стендов тестирования
                {
                    continue;
                }
                DeviceItems.Add(new UsbDevInfo(device));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UpdateDeviceList();
        }
    }

    public class UsbDevInfo
    {
        public UsbDriver Driver => device.Driver;
        public UsbDeviceDescriptor Descriptor => device.DeviceDescriptor;
        public string SerialNumber => serialNumber;
        public IUsbDevice Device => device;

        private readonly string serialNumber;
        private readonly IUsbDevice device;
        private readonly string manufacturer;
        private readonly string product;

        public UsbDevInfo(IUsbDevice dev)
        {
            this.device = dev;
            serialNumber = Descriptor.iSerialNumber == 0 ? Descriptor.bcdDevice.ToString() : device.GetStringDescriptor(Descriptor.iSerialNumber);
            manufacturer = device.GetStringDescriptor(Descriptor.iManufacturer);
            product = devicesNames.Keys.Contains(Descriptor.idProduct) ? devicesNames[Descriptor.idProduct] : device.GetStringDescriptor(Descriptor.iProduct);
        }

        public override string ToString()
        {

            var str = $"P: {product} " +
                      $"SN: {serialNumber} " +
                      $"VID: 0x{Descriptor.idVendor:X4}; " +
                      $"PID: 0x{Descriptor.idProduct:X4}; " +
                      $"REV: 0x{Descriptor.bcdDevice:X4}; " +
                      $"M: {manufacturer}; " +
                      $"(Driver<{device.Driver.Name}>)";
            return str;
        }

        /// <summary>
        /// Имена устройств в зависимости от пида, т. к. старые устройства не возвращают наименование в строковом дескрипторе (приходит только Neurosoft)
        /// </summary>
        private static Dictionary<int, string> devicesNames = new Dictionary<int, string>
        {
            { 0x8250, "Neuron-Spectrum-4/EP" },
            { 0x8251, "Neuron-Spectrum-1" },
            { 0x8252, "Neuron-Spectrum-2" },
            { 0x8253, "Neuron-Spectrum-3" },
            { 0x8254, "Neuron-Spectrum-4" },
            { 0x8255, "Neuro-EMG-Micro-4" },
            { 0x8257, "Neuron-Spectrum-4/P" },
              
            { 0x8260, "Neuro-MEP-Micro" },
            { 0x8261, "Neuro-EMG-Micro-2M" },
            { 0x8263, "Neuro-EMG-MC" },
            { 0x8270, "Neuron-Spectrum-5" },
            { 0x8271, "Neuron-Spectrum-4EPM" },
        };
    }
}
