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
using NeuroSoft.Prototype.Database;
using System.Collections.ObjectModel;
using NeuroSoft.Prototype.Interface;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for TestingDescriptionDialog.xaml
    /// </summary>
    public partial class TestingDescriptionDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public TestingDescriptionDialog(PacientInfo patient)
        {
            InitializeComponent();
            Description = patient.FirstName + " (" + patient.LastName + ")";
            DataContext = this;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            DescriptionTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            DescriptionTextBox.Focus();
            DescriptionTextBox.SelectAll();
        }
        #region Properties

        private string description;

        /// <summary>
        /// Описание процесса тестирования
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged("Description");                    
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
