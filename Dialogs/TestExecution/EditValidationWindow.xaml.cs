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
    /// Interaction logic for EditValidationWindow.xaml
    /// </summary>
    public partial class EditValidationWindow : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public EditValidationWindow(TestTemplateItem testItem, List<ITag> currentTags)
        {
            InitializeComponent();
            TestItem = testItem;            
            tags = currentTags;
            AllTests = testItem.TemplateParent.GetAllTests();
            //исключим все лишние тесты - дочерние элементы, а также родительские главы инструкций
            AllTests.Remove(testItem);
            foreach (string testId in testItem.TestDependencesIdList)
            {
                TestTemplateItem test = testItem.TemplateParent.FindTestById(testId);
                if (test != null)
                {
                    TestDependences.Add(test);
                }
            }
            DataContext = this;
        }

        private IList<ITag> tags = new List<ITag>();
        private TestTemplateItem testItem;
        private ObservableCollection<TestTemplateItem> testDependences = new ObservableCollection<TestTemplateItem>();
        internal List<TestTemplateItem> AllTests = new List<TestTemplateItem>();
        private TestTemplateItem selectedTest;

        /// <summary>
        /// Шаблон действия
        /// </summary>
        public TestTemplateItem TestItem
        {
            get { return testItem; }
            private set { testItem = value; }
        }
        
        /// <summary>
        /// Список тестов
        /// </summary>
        public ObservableCollection<TestTemplateItem> TestDependences
        {
            get { return testDependences; }
        }        
        /// <summary>
        /// Тесты доступные к добавлению
        /// </summary>
        internal List<TestTemplateItem> AvailableTests
        {
            get
            {
                List<TestTemplateItem> availableTests = new List<TestTemplateItem>();
                foreach (var test in AllTests)
                {
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
                    if (!TestDependences.Contains(test))
                    {
                        availableTests.Add(test);
                    }
                }
                return availableTests;
            }
        }
        /// <summary>
        /// Выделенный тест
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
        /// <summary>
        /// Можно ли добавлять тесты
        /// </summary>
        public bool HasAvailableTests
        {
            get
            {
                return AllTests.Count > TestDependences.Count;
            }
        }

        /// <summary>
        /// Теги
        /// </summary>
        public IList<ITag> Tags
        {
            get { return tags; }
        }
        

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void AddTestDependenceBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectTestDependenceDialog dialog = new SelectTestDependenceDialog(AvailableTests);
            if (dialog.ShowDialog() == true)
            {
                TestDependences.Add(dialog.SelectedTest);
                TestItem.TestDependencesIdList.Add(dialog.SelectedTest.TestId);
                SelectedTest = dialog.SelectedTest;
                OnPropertyChanged("HasAvailableTests");
                TestItem.NotifyModified();
            }
        }

        private void DeleteTestDependenceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTest != null)
            {
                int index = TestDependences.IndexOf(SelectedTest);
                TestItem.TestDependencesIdList.Remove(SelectedTest.TestId);
                TestDependences.Remove(SelectedTest);                
                if (index >= TestDependences.Count)
                {
                    index = TestDependences.Count - 1;
                }
                if (index > -1)
                {
                    SelectedTest = TestDependences[index];
                }
                OnPropertyChanged("HasAvailableTests");
                TestItem.NotifyModified();
            }
        }        
    }
}
