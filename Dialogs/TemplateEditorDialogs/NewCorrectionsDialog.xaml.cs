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

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTestGroupsDialog.xaml
    /// </summary>
    public partial class NewCorrectionsDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="template"></param>
        public NewCorrectionsDialog(DATTemplate template, DataConnection connection)
        {
            InitializeComponent();
            ParentTemplate = template;
            Connection = connection;
            CorrectionsList = TemplateCorrections.Read(connection, template.GUID.ToString());
            DataContext = this;
            foreach (var correction in CorrectionsList.Corrections)
            {
                var test = template.FindTestById(correction.TestId);
                if (test != null)
                    correction.DisplayTestName = test.Name;
            }
            CorrectionsListBox.Items.GroupDescriptions.Add(new PropertyGroupDescription("DisplayTestName"));
        }

        private DataConnection Connection;
        private DATTemplate parentTemplate;
        /// <summary>
        /// 
        /// </summary>
        public DATTemplate ParentTemplate
        {
            get { return parentTemplate; }
            private set
            {
                parentTemplate = value;
            }
        }
        private List<CorrectionInfo> AcceptedCorrections = new List<CorrectionInfo>();

        private TemplateCorrections correctionsList;

        /// <summary>
        /// 
        /// </summary>
        public TemplateCorrections CorrectionsList
        {
            get { return correctionsList; }
            private set 
            {
                correctionsList = value;
                OnPropertyChanged("CorrectionsList");
            }
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (CorrectionInfo item in CorrectionsListBox.SelectedItems)
            {
                AcceptedCorrections.Add(item);
            }
            RemoveSelectedItems();
        }

        private void RefuseBtn_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedItems();
        }

        private void RemoveSelectedItems()
        {
            List<CorrectionInfo> selectedItems = new List<CorrectionInfo>();
            foreach (CorrectionInfo item in CorrectionsListBox.SelectedItems)
            {
                selectedItems.Add(item);
            }
            foreach (var item in selectedItems)
            {
                CorrectionsList.Corrections.Remove(item);
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in AcceptedCorrections)
            {
                AddCorrection(item);
            }
            DialogResult = true;
            CorrectionsList.Save(Connection);
            MainWindow.UpdateNewCorrectionsCount();
        }

        private void AddCorrection(CorrectionInfo correction)
        {
            var test = ParentTemplate.FindTestById(correction.TestId);
            if (test == null)
                return;
            if (!test.ProbableCorrections.Contains(correction.CorrectionString))
                test.ProbableCorrections.Add(correction.CorrectionString);
        }
    }  
}
