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
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest;
using System.Windows.Forms;
using NeuroSoft.WPFComponents.CommandManager;
using NeuroSoft.DeviceAutoTest.Common;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for OpenTemplateDialog.xaml
    /// </summary>
    public partial class TemplatesListDialog : DATDialogWindow
    {
        /// <summary>
        ///     Выбранный шаблон
        /// </summary>
        public DATTemplateDescriptor SelectedTemplate
        {
            get { return (DATTemplateDescriptor)GetValue(SelectedTemplateProperty); }
            set { SetValue(SelectedTemplateProperty, value); }
        }

        /// <summary>
        ///    Свойство зависимостей для SelectedTemplate
        /// </summary>
        public static readonly DependencyProperty SelectedTemplateProperty =
            DependencyProperty.Register("SelectedTemplate", typeof(DATTemplateDescriptor), typeof(TemplatesListDialog), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Признак создания нового шаблона
        /// </summary>
        public bool CreateNewTemplate { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public TemplatesListDialog()
        {
            InitializeComponent();
            CreateNewTemplate = false;
            if (DATTemplate.TestTemplateDescriptors.Count > 0)
            {
                SelectedTemplate = DATTemplate.TestTemplateDescriptors[0];
            }            
            DataContext = this;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NewButtonClick(object sender, RoutedEventArgs e)
        {
            CreateNewTemplate = true;
            DialogResult = true;            
        }        

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item == null)
                return;
            DialogResult = true;
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedTemplate == null)
                return;            
            if (NSMessageBox.Show(string.Format(Properties.Resources.ConfirmRemoveDATTemplate, SelectedTemplate.Name), Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int selIndex = DATTemplate.TestTemplateDescriptors.IndexOf(SelectedTemplate);
                DATTemplate.RemoveTemplate(SelectedTemplate.FileName);
                if (selIndex > DATTemplate.TestTemplateDescriptors.Count - 1)
                {
                    selIndex--;
                }
                if (selIndex > -1)
                {
                    SelectedTemplate = DATTemplate.TestTemplateDescriptors[selIndex];
                }
            }
        }       

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = "dattmp";
            dialog.Filter = Properties.Resources.DATTemplateFilter;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var datTemplate = DATTemplate.Import(dialog.FileName);
                if (datTemplate != null)
                {
                    if (DATTemplate.TestTemplateDescriptors.Any(ttd => datTemplate.GUID == ttd.GUID && datTemplate.Version == ttd.Version))
                    {
                        if (NSMessageBox.Show(Properties.Resources.ConfirmOverwriteTemplateByImport, Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {                
                            DATTemplate.RemoveTemplate(datTemplate.GUID, datTemplate.Version);
                        }
                        else
                        {
                            datTemplate.ResetGuid();
                        }
                    }
                    var deviceInfo = (from dev in DevicesManipulation.AvailableDevices where dev.DeviceType == datTemplate.DeviceType select dev).FirstOrDefault();
                    bool showTemplateInfoDialog = deviceInfo == null;
                    if (DATTemplate.TestTemplateDescriptors.Any(ttd => ttd.Name == datTemplate.Name && datTemplate.GUID != ttd.GUID))
                    {
                        showTemplateInfoDialog = true;
                    }
                    if (showTemplateInfoDialog)
                    {
                        TestTemplateInfoDialog templateInfoDialog = new TestTemplateInfoDialog();
                        templateInfoDialog.SelectedDevice = deviceInfo;
                        //templateInfoDialog.CanEditDevice = deviceInfo == null;
                        if (templateInfoDialog.ShowDialog() == true)
                        {
                            datTemplate.Name = templateInfoDialog.TemplateName;
                            if (templateInfoDialog.SelectedDevice != null)
                            {
                                datTemplate.DeviceType = templateInfoDialog.SelectedDevice.DeviceType;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    datTemplate.Save();
                }
            }
        }     
    }  
}
