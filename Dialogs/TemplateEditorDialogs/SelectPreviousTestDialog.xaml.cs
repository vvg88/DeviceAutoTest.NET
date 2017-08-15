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
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.WPFComponents.ScalableWindows;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectPreviousTestDialog.xaml
    /// </summary>
    public partial class SelectPreviousTestDialog : DATDialogWindow
    {        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="testItem"></param>        
        public SelectPreviousTestDialog(TestTemplateItem testItem)
        {
            InitializeComponent();
            TestItem = testItem;            
            DataContext = this;
        }

        private TestTemplateItem testItem;

        internal TestTemplateItem TestItem
        {
            get { return testItem; }
            private set 
            {
                testItem = value;
                SelectedTestItem = testItem.TemplateParent.FindTestById(testItem.PreviousTestId);
            }
        }

        /// <summary>
        /// Список доступных для текущего теста тестов, после которых следует выполнять текущий
        /// </summary>
        public List<TestTemplateItem> AvailablePrevTests
        {
            get
            {
                var availablePrevTests = new List<TestTemplateItem>();
                var allTests = TestItem.TemplateParent.GetAllTests();
                foreach (var test in allTests)
                {
                    if (test == TestItem)
                        continue;

                    var parentContainer = TestItem.ParentContainer;
                    bool isParentContainer = false;
                    while (parentContainer != null)
                    {
                        if (test == parentContainer)
                        {
                            isParentContainer = true;
                            break;
                        }
                        parentContainer = parentContainer.ParentContainer;
                    }
                    if (isParentContainer)
                        continue;

                    var _test = (from t in allTests where t != TestItem && t.PreviousTestId == test.TestId select t).FirstOrDefault();
                    if (_test != null)
                        continue;
                    availablePrevTests.Add(test);
                }
                return availablePrevTests;
            }
        }

        private TestTemplateItem selectedTestItem;
        /// <summary>
        /// 
        /// </summary>
        public TestTemplateItem SelectedTestItem
        {
            get { return selectedTestItem; }
            set
            {
                if (selectedTestItem != value)
                {
                    selectedTestItem = value;
                    OnPropertyChanged("SelectedTestItem");
                }
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            SelectedTestItem = null;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            TestItem.PreviousTestId = SelectedTestItem != null ? SelectedTestItem.TestId : string.Empty;
        }
    }
}
