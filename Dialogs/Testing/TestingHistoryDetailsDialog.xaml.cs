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
using System.Globalization;
using NeuroSoft.DeviceAutoTest.Common.Converters;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for TestingHistoryDetailsDialog.xaml
    /// </summary>
    public partial class TestingHistoryDetailsDialog : DATDialogWindow
    {
        private TestingSnapshot snapshot = null;
        /// <summary>
        /// Снимок процесса наладки
        /// </summary>
        public TestingSnapshot Snapshot
        {
            get { return snapshot; }
            private set
            { 
                snapshot = value;
                DataContext = snapshot;
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public TestingHistoryDetailsDialog(TestingSnapshot snapshot)
        {            
            InitializeComponent();
            Snapshot = snapshot;
            Title = snapshot.Ancestor.CheckupInfo.PacientInfo.FirstName + " (" + snapshot.Ancestor.CheckupInfo.PacientInfo.LastName + ")";
            if (snapshot.ExecutedTestInfo != null)
            {
                Title += " - " + snapshot.ExecutedTestInfo.TestName; 
            }
            Title += " - " + new TicksToDateStringConverter().Convert(snapshot.SnapshotTime, typeof(string), null, CultureInfo.CurrentCulture);            
        }
    }   
}
