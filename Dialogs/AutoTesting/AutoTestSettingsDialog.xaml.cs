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
using System.CodeDom.Compiler;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for AutoTestSettingsDialog.xaml
    /// </summary>
    public partial class AutoTestSettingsDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public AutoTestSettingsDialog(TestTemplateItem testItem)
        {
            InitializeComponent();
            Settings = testItem.AutoTestingSettings;
            testName = testItem.Name;
            DataContext = this;
        }

        private AutoTestSettings settings;

        /// <summary>
        /// Скрипт
        /// </summary>
        public AutoTestSettings Settings
        {
            get { return settings; }
            private set 
            {
                settings = value;                
            }
        }
        private string testName;

        /// <summary>
        /// 
        /// </summary>
        public string TestName
        {
            get { return testName; }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }        
    }
}
