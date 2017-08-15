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
using NeuroSoft.DeviceAutoTest.Common;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateTestTemplateWindow.xaml
    /// </summary>
    public partial class TestTemplateInfoDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public TestTemplateInfoDialog()
        {
            InitializeComponent();
            string defaultName = Properties.Resources.DefaultTemplateName;
            int i = 2;
            while ((from descr in DATTemplate.TestTemplateDescriptors where descr.Name == defaultName select descr).SingleOrDefault() != null)
            {
                defaultName = Properties.Resources.DefaultTemplateName + i;
                i++;
            }
            TemplateName = defaultName;
            if (DevicesManipulation.AvailableDevices.Count > 0)
            {
                SelectedDevice = DevicesManipulation.AvailableDevices[0];
            }
            DataContext = this;           
        }

        #region Properties
        private DeviceTypeInfo selectedDevice;

        /// <summary>
        /// Устройство, для тестирования которого создается шаблон
        /// </summary>
        public DeviceTypeInfo SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                if (selectedDevice != value)
                {
                    selectedDevice = value;
                    OnPropertyChanged("SelectedDevice");
                }
            }
        }

        private bool canEditDevice = true;

        /// <summary>
        /// Возможность изменения устройства
        /// </summary>
        public bool CanEditDevice
        {
            get { return canEditDevice; }
            set
            {
                if (canEditDevice != value)
                {
                    canEditDevice = value;
                    OnPropertyChanged("CanEditDevice");
                }
            }
        }

        private string templateName;
        /// <summary>
        /// Имя создаваемого шаблона
        /// </summary>
        public string TemplateName
        {
            get { return templateName; }
            set
            {
                if (templateName != value)
                {
                    templateName = value;
                    OnPropertyChanged("TemplateName");
                }
            }
        }
        #endregion 
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }        
    }   
}
