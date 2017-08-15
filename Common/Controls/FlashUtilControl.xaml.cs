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
using System.Collections.ObjectModel;
using NeuroSoft.Hardware.Tools;
using System.IO;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common.Scripts;

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// Interaction logic for FlashUtilControl.xaml
    /// </summary>
    public partial class FlashUtilControl : UserControl
    {
        private string defaultFirmwareFolder = "";

        /// <summary>
        /// 
        /// </summary>
        public string DefaultFirmwareFolder
        {
            get { return defaultFirmwareFolder; }
            set { defaultFirmwareFolder = value; }
        }

        string fileNameTemplate = "*";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="fileNameTemplate">Шаблон имени файла прошивки</param>
        public FlashUtilControl(ScriptEnvironment environment, string fileNameTemplate)
        {
            InitializeComponent();
            this.fileNameTemplate = fileNameTemplate;
            UpdateAdapters();
            DataContext = this;
            if (USBSerialNumbers.Count > 0)
                UseUSB = true;
            this.environment = environment;
        }

        private void UpdateAdapters()
        {
            string[] ComPorts = System.IO.Ports.SerialPort.GetPortNames();
            SerialAdapters.Clear();
            foreach (string comPort in ComPorts)
            {
                SerialAdapters.Add(comPort);
            }
            if (SerialAdapters.Count > 0)
            {
                SelectedCOMPort = SerialAdapters[0];
            }

            string[] UsbAdapters = SiliconLabsUtil.GetUsbAdapterSerialNumbers();
            USBSerialNumbers.Clear();
            if (UsbAdapters != null)
            {
                foreach (string usbSerNum in UsbAdapters)
                {
                    USBSerialNumbers.Add(usbSerNum);
                }
                if (USBSerialNumbers.Count > 0)
                {
                    SelectedUSBSN = USBSerialNumbers[0];                    
                }
            }
        }

        #region Properties

        /// <summary>
        /// Признак использования USB для прошивки
        /// </summary>
        public bool UseUSB
        {
            get { return (bool)GetValue(UseUSBProperty); }
            set { SetValue(UseUSBProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UseUSB.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseUSBProperty =
            DependencyProperty.Register("UseUSB", typeof(bool), typeof(FlashUtilControl), new UIPropertyMetadata(false));

        
        #region COM
        private ObservableCollection<string> serialAdapters = new ObservableCollection<string>();
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> SerialAdapters
        {
            get { return serialAdapters; }
        }

        private string selectedCOMPort;
        /// <summary>
        /// 
        /// </summary>
        public string SelectedCOMPort
        {
            get { return selectedCOMPort; }
            set { selectedCOMPort = value; }
        }
        #endregion

        #region USB
        private ObservableCollection<string> usbSerialNumbers = new ObservableCollection<string>();
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> USBSerialNumbers
        {
            get { return usbSerialNumbers; }
        }

        private string selectedUSBSN;
        /// <summary>
        /// 
        /// </summary>
        public string SelectedUSBSN
        {
            get { return selectedUSBSN; }
            set { selectedUSBSN = value; }
        }
        #endregion

        private string hexFileName = null;
        /// <summary>
        /// Имя файла прошивки контроллера
        /// </summary>
        public string HexFileName
        {
            get { return hexFileName; }
            private set { hexFileName = value; }
        }

        private string programmDate = null;
        /// <summary>
        /// Время программирования контроллера
        /// </summary>
        public string ProgrammDate
        {
            get { return programmDate; }
            private set { programmDate = value; }
        }

        private ScriptEnvironment environment;
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            string FirmwareFolder = DefaultFirmwareFolder;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "hex|" + fileNameTemplate;
            openFileDialog.InitialDirectory = FirmwareFolder;
            openFileDialog.FileName = Directory.GetFiles(FirmwareFolder, fileNameTemplate, SearchOption.TopDirectoryOnly)[0];
            if (openFileDialog.ShowDialog() == true)
            {
                bool programmResult;
                if (UseUSB)
                    programmResult = SiliconLabsUtil.ProgrammDeviceUSB(openFileDialog.FileName, selectedUSBSN);
                else
                    programmResult = SiliconLabsUtil.ProgrammDevice(openFileDialog.FileName, int.Parse(selectedCOMPort.Remove(selectedCOMPort.IndexOf("COM"), 3)));
                
                if (programmResult)
                {
                    MessageBox.Show("Программирование завершилось успешно", "Сообщение", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    environment["FlashUtilFileName"] = new FileInfo(openFileDialog.FileName).Name;                    
                } 
                else
                    MessageBox.Show("Во время программирования возникла ошибка " + SiliconLabsUtil.ErrorMessage, 
                                    "Сообщение об ошибке", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            UpdateAdapters();
        }
    }
}
