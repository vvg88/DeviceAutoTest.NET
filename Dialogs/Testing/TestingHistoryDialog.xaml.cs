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
using System.Collections.ObjectModel;
using NeuroSoft.WPFComponents.NSGridView;
using NeuroSoft.WPFComponents.WPFToolkit;
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using NeuroSoft.WPFComponents.CommandManager;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.Prototype.Database;
using System.ComponentModel;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.DeviceAutoTest.Controls;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for TestingHistoryDialog.xaml
    /// </summary>
    public partial class TestingHistoryDialog : DATDialogWindow
    {       
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="testingAncestor"></param>
        /// <param name="test"></param>
        public TestingHistoryDialog(DeviceTestCheckupAncestor testingAncestor, TestObject test)
        {
            InitializeComponent();
            Ancestor = testingAncestor;
            Test = test;
            DataContext = this;
            //TestingHistoryControl historyControl = new TestingHistoryControl(testingAncestor, test);
            //Title = historyControl.Title;
        }

        private DeviceTestCheckupAncestor ancestor;
        /// <summary>
        /// 
        /// </summary>
        public DeviceTestCheckupAncestor Ancestor
        {
            get { return ancestor; }
            set
            {
                if (ancestor != value)
                {
                    ancestor = value;
                    OnPropertyChanged("Ancestor");
                }
            }
        }

        private TestObject test;
        /// <summary>
        /// 
        /// </summary>
        public TestObject Test
        {
            get { return test; }
            set
            {
                if (test != value)
                {
                    test = value;
                    OnPropertyChanged("Test");
                }
            }
        }        
        
        private void DetailsBtn_Click(object sender, RoutedEventArgs e)
        {
            HistoryControl.ShowDetails();
        }        
    }    
}
