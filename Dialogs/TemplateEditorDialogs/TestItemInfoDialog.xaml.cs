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
    /// Диалог с информацией о новом тесте
    /// </summary>
    public partial class TestItemInfoDialog : DATDialogWindow
    {
        internal TestItemInfoDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="testItem"></param>
        public TestItemInfoDialog(TestTemplateItem testItem)
            : this()
        {
            CurrentTemplate = testItem.TemplateParent;
            TestName = testItem.Name;
            TestId = testItem.TestId;
            InitialTestId = TestId;
            IsNewTestCreation = false;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="template"></param>
        public TestItemInfoDialog(DATTemplate template) 
            : this()
        {
            CurrentTemplate = template;
            IsNewTestCreation = true;
            string testName = Properties.Resources.DefaultTestItemName;
            var allTests = template.GetAllTests();            
            int i = 2;
            while ((from t in allTests where t.Name == testName select t).FirstOrDefault() != null)
            {
                testName = Properties.Resources.DefaultTestItemName + " (" + (i++) + ")";
            }
            TestName = testName;
            string defaultId = "test1";
            i = 2;
            while ((from test in allTests where test.TestId == defaultId select test).FirstOrDefault() != null)
            {
                defaultId = "test" + i;
                i++;
            }
            TestId = defaultId;
            existingTests = allTests;// template.GetAllTests(true);            
            if (existingTests.Count > 0)
            {
                TestBase = existingTests[0];
            }            
        }

        #region Properties
        private bool isContainer = false;

        /// <summary>
        /// Признак составного теста - главы инструкции
        /// </summary>
        public bool IsContainer
        {
            get { return isContainer; }
            set
            {
                if (isContainer != value)
                {
                    isContainer = value;
                    OnPropertyChanged("IsContainer");
                }
            }
        }

        private bool isNewTestCreation = false;

        /// <summary>
        /// Признак создания нового теста
        /// </summary>
        public bool IsNewTestCreation
        {
            get { return isNewTestCreation; }
            private set
            {
                isNewTestCreation = value;
                OnPropertyChanged("IsNewTestCreation");
            }
        }

        private List<TestTemplateItem> existingTests = new List<TestTemplateItem>();
        /// <summary>
        /// Существующие тесты
        /// </summary>
        public List<TestTemplateItem> ExistingTests
        {
            get { return existingTests; }            
        }

        /// <summary>
        /// 
        /// </summary>
        public bool AnotherTestsExists
        {
            get
            {
                return ExistingTests.Count > 0;
            }
        }

        private TestTemplateItem testBase;
        /// <summary>
        /// Тест, на основе которого необходимо создать новый тест
        /// </summary>
        public TestTemplateItem TestBase
        {
            get { return testBase; }
            set 
            {
                if (testBase != value)
                {
                    testBase = value;
                    OnPropertyChanged("TestBase");
                }
            }
        }

        private bool isBasedOnTest = false;

        /// <summary>
        /// Признак создания нового теста на основе TestBase
        /// </summary>
        public bool IsBasedOnTest
        {
            get { return isBasedOnTest && AnotherTestsExists; }
            set 
            {
                if (isBasedOnTest != value)
                {                    
                    isBasedOnTest = value;
                    OnPropertyChanged("IsBasedOnTest");
                }
            }
        }

        /// <summary>
        /// Текущий шаблон тестов
        /// </summary>
        internal static DATTemplate CurrentTemplate;
        internal static string InitialTestId;

        private string testName;
        /// <summary>
        /// Имя теста
        /// </summary>
        public string TestName
        {
            get { return testName; }
            set
            {
                if (testName != value)
                {
                    testName = value;
                    OnPropertyChanged("TestName");
                }
            }
        }

        private string testId;
        /// <summary>
        /// Идентификатор теста
        /// </summary>
        public string TestId
        {
            get { return testId; }
            set
            {
                if (testId != value)
                {
                    testId = value;
                    OnPropertyChanged("TestId");
                }
            }
        }
        #endregion 
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            TestIdTextBox.SelectAll();
            NameTextBox.SelectAll();
            TestIdTextBox.Focus();
            base.OnActivated(e);            
        }
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
            CurrentTemplate = null;
            InitialTestId = null;
        }
    }

    /// <summary>
    /// Валидатор идентификатора теста
    /// </summary>
    internal class TestIdValidator : ValidationRule
    {
        /// <summary>
        /// Валидация
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string currentId = (string)value;
            if (string.IsNullOrEmpty(currentId))
            {
                return new ValidationResult(false, Properties.Resources.TestIdMustBeNotEmpty);
            }
            if (!Regex.IsMatch(currentId, @"^[a-zA-Z]{1}[a-zA-Z_\d]*$"))
            {
                return new ValidationResult(false, Properties.Resources.TestIdLimitations);
            }
            List<TestTemplateItem> tests = TestItemInfoDialog.CurrentTemplate.GetAllTests();
            foreach (var test in tests)
            {
                if (currentId == test.TestId && TestItemInfoDialog.InitialTestId != currentId)
                {
                    return new ValidationResult(false, string.Format(Properties.Resources.TestIdExists, currentId));
                }                
            }
            foreach (var variable in TestItemInfoDialog.CurrentTemplate.Variables)
            {
                if (variable.Name == currentId + "_Finished" ||
                    variable.Name == currentId + "_HasErrors" ||
                    variable.Name == currentId + "_WasCorrected" ||
                    variable.Name == currentId + "_Comments")
                {
                    new ValidationResult(false, string.Format(Properties.Resources.InvalidTestIdVariableExitsts, variable.Name));
                }
            }
            return new ValidationResult(true, null);
        }
    }
}
