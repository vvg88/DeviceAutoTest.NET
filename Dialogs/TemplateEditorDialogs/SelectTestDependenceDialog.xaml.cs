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
using System.Text.RegularExpressions;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SelectTestDependenceDialog : DATDialogWindow
    {        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="availableTests"></param>
        public SelectTestDependenceDialog(List<TestTemplateItem> availableTests) 
        {
            InitializeComponent();
            AvailableTests = availableTests;            
            DataContext = this;
        }

        #region Properties
        private List<TestTemplateItem> availableTests;

        /// <summary>
        /// 
        /// </summary>
        public List<TestTemplateItem> AvailableTests
        {
            get { return availableTests; }
            private set
            {
                availableTests = value;
                if (availableTests != null && availableTests.Count > 0)
                {
                    SelectedTest = AvailableTests[0];
                }
            }
        }

        private TestTemplateItem selectedTest;

        /// <summary>
        /// 
        /// </summary>
        public TestTemplateItem SelectedTest
        {
            get { return selectedTest; }
            set
            {
                if (selectedTest != value)
                {
                    selectedTest = value;
                    OnPropertyChanged("SelectedTest");
                }
            }
        }
        #endregion 
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);            
        }
    }    
}
