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
using AvalonDock;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.ComponentModel;
using System.Windows.Media.Animation;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest.Commands;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using NeuroSoft.WPFComponents.CommandManager;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using NeuroSoft.WPFPrototype.Interface;
using NeuroSoft.DeviceAutoTest.Controls;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Interaction logic for TestHistoryDockableContent.xaml
    /// </summary>
    public partial class TestHistoryDockableContent : DockableContent
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="viewModel"></param>
        public TestHistoryDockableContent(DeviceTestCheckupAncestor testingAncestor, TestObject test)
        {
            InitializeComponent();
            HideOnClose = false;
            Test = test;
            Ancestor = testingAncestor;            
            Title = Properties.Resources.TestHistoryStr;
            ContentGrid.DataContext = this;
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
                    NotifyPropertyChanged("Ancestor");
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
                    NotifyPropertyChanged("Test");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void InvalidateHistory()
        {
            HistoryControl.InvalidateHistory();
        }
    }
}
