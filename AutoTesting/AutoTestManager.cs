using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.WPFComponents.ScalableWindows;
using System.Windows;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// 
    /// </summary>
    public class AutoTestManager : BaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="autoTests"></param>
        public AutoTestManager(DeviceTestCheckupManager checkupManager, TestItemGroup autoTestGroup)
        {
            CheckupManager = checkupManager;
            ParseTestGroup(autoTestGroup);
            timer.Tick += new EventHandler(timer_Tick);
            Statistics = new AutoTestingStatistics(Tests.Count);
        }        

        #region Properties

        private DispatcherTimer timer = new DispatcherTimer();
        
        private DeviceTestCheckupManager checkupManager;
        /// <summary>
        /// Менеджер обследований
        /// </summary>
        public DeviceTestCheckupManager CheckupManager
        {
            get { return checkupManager; }
            private set { checkupManager = value; }
        }
        private List<TestObject> tests = new List<TestObject>();        
        /// <summary>
        /// Список тестов для автоматического выполнения
        /// </summary>
        public List<TestObject> Tests
        {
            get { return tests; }
        }

        private TestObject currentTest;
        /// <summary>
        /// Текущий тест
        /// </summary>
        public TestObject CurrentTest
        {
            get { return currentTest; }
            private set 
            {
                if (currentTest != value)
                {
                    currentTest = value;
                    OnPropertyChanged("CurrentTest");
                }
            }
        }

        private bool started;
        /// <summary>
        /// Признак того, что процесс автоматического тестирования был запущен
        /// </summary>
        public bool Started
        {
            get { return started; }
            private set 
            {
                if (started != value)
                {
                    started = value;
                    OnPropertyChanged("Started");
                    OnPropertyChanged("CanStart");
                    OnPropertyChanged("CanStop");
                    OnPropertyChanged("CanPause");
                    OnPropertyChanged("CanResume");
                    CheckupManager.TestsAncestor.ScriptExecutionEnvironment.IsAutoTesting = value;
                }
            }
        }

        private bool active;
        /// <summary>
        /// Признак активности автоматического тестирования
        /// </summary>
        public bool Active
        {
            get { return active; }
            private set
            {
                if (active != value)
                {
                    active = value;
                    OnPropertyChanged("Active");
                    OnPropertyChanged("CanPause");
                    OnPropertyChanged("CanResume");
                }
            }
        }

        /// <summary>
        /// Возможность приостановки тестирования
        /// </summary>
        public bool CanPause
        {
            get
            {
                return Started && Active;
            }
        }

        /// <summary>
        /// Возможность возобновления тестирования
        /// </summary>
        public bool CanResume
        {
            get
            {
                return Started && !Active;
            }
        }

        /// <summary>
        /// Возможность начала тестирования
        /// </summary>
        public bool CanStart
        {
            get
            {
                return !Started;
            }
        }

        /// <summary>
        /// Возможность остановки тестирования
        /// </summary>
        public bool CanStop
        {
            get
            {
                return Started;
            }
        }

        private string status;

        /// <summary>
        /// Состояние процесса автоматического тестирования
        /// </summary>
        public string Status
        {
            get { return status; }
            private set 
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        private AutoTestingStatistics statistics;

        /// <summary>
        /// 
        /// </summary>
        public AutoTestingStatistics Statistics
        {
            get { return statistics; }
            private set
            { 
                statistics = value;
                OnPropertyChanged("Statistics");
            }
        }

        private bool showProgress;

        /// <summary>
        /// 
        /// </summary>
        public bool ShowProgress
        {
            get { return showProgress; }
            set 
            {
                if (showProgress != value)
                {
                    showProgress = value;
                    OnPropertyChanged("ShowProgress");
                }
            }
        }
        
        #endregion

        #region Methods
        internal void SetStatus(string status)
        {
            Status = status;
        }
        /// <summary>
        /// Начало автоматического тестирования набора тестов
        /// </summary>
        public void Start()
        {
            Statistics = new AutoTestingStatistics(Tests.Count);
            if (Started)
                return;
            if (Tests.Count > 0)
            {
                CurrentTest = Tests[0];
                Started = true;
                Active = true;
                ShowProgress = true;
                StartTest();
            }
        }
        /// <summary>
        /// Остановка автоматического тестирования
        /// </summary>
        public void Stop(bool stopped = false)
        {
            if (!Started)
                return;
            Active = false;
            Started = false;
            CurrentTest = null;
            CompleteTest();
            string status = stopped ? Properties.Resources.atStopped : Properties.Resources.atComplete;
            SetStatus(status);
            NSMessageBox.Show(string.Format(Properties.Resources.atCompleteFull, Statistics.Started, Statistics.All, Statistics.Skipped, Statistics.Completed), status, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            ShowProgress = false;            
        }
        /// <summary>
        /// Приостановка автоматического тестирования
        /// </summary>
        public void Pause()
        {
            if (!CanPause)
                return;
            Active = false;
            CompleteTest();
        }
        /// <summary>
        /// Возобновление автоматического тестирования
        /// </summary>
        public void Resume()
        {
            if (!CanResume)
                return;
            Active = true;
            StartTest();
        }

        /// <summary>
        /// Выбрать следующий тест для выполнения
        /// </summary>
        internal void SelectNext()
        {
            int currentIndex = Tests.IndexOf(CurrentTest);
            if (Tests.Count - 1> currentIndex)
            {
                CurrentTest = Tests[currentIndex + 1];
            }
            else
            {
                CurrentTest = null;
            }
        }

        /// <summary>
        /// Выполнить текущий тест и продолжить автоматическое тестирование
        /// </summary>
        internal void StartTest()
        {
            var currentTest = CurrentTest;
            if (currentTest == null)
                return;
            string testName = currentTest.TemplateItem.Name;
            Statistics.Started++;
            SetStatus(string.Format(Properties.Resources.atStartTest, testName));
            var autoTestSettings = currentTest.TemplateItem.AutoTestingSettings;
            timer.Interval = TimeSpan.FromMilliseconds(autoTestSettings.Timeout);
            if (autoTestSettings.ResetFinishedTests)
            {
                currentTest.Reset(null, false, false, true);
                //currentTest.Finished = false;
            }
            else if (currentTest.Finished)
            {
                Statistics.Skipped++;
                SwitchTest();
                return;
            }
            if (autoTestSettings.Timeout > 0 || autoTestSettings.Manual)
            {
                CheckupManager.TestsAncestor.ScriptExecutionEnvironment.AutoTestComplete += new System.Windows.RoutedEventHandler(TemplateItem_AutoTestComplete);
            }
            if (!currentTest.TestOpened)
            {
                CheckupManager.OpenTest(currentTest);
                Services.DoEvents();
            }
            else
            {
                TestDockableContent testContent = CheckupManager.GetTestForm(currentTest) as TestDockableContent;
                if (testContent != null)
                {
                    testContent.OnDeactivated();                    
                    testContent.OnActivated();
                    Services.DoEvents();                    
                }
            }
            if (autoTestSettings.Manual)
            {
                SetStatus(string.Format(Properties.Resources.atManualTest, testName));                
                Console.Beep();
            }
            else
            {
                if (autoTestSettings.Timeout > 0 && currentTest.CanExecute)
                {
                    timer.Start();
                    SetStatus(string.Format(Properties.Resources.atWaitTest, testName));
                }
                else
                {
                    CompleteTest();
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            CompleteTest();
        }
        void TemplateItem_AutoTestComplete(object sender, System.Windows.RoutedEventArgs e)
        {
            AutoTestAction action = AutoTestAction.Execute;
            if (sender is AutoTestAction)
            {
                action = (AutoTestAction)sender;
            }            
            CompleteTest(Converters.ToTestAction(action));
        }

        /// <summary>
        /// Завершение теста
        /// </summary>
        private void CompleteTest(TestActionType? testAction = null)
        {            
            Application.Current.Dispatcher.BeginInvoke(new Action(() => 
            {                
                CheckupManager.TestsAncestor.ScriptExecutionEnvironment.AutoTestComplete -= new System.Windows.RoutedEventHandler(TemplateItem_AutoTestComplete);
                timer.Stop();
                var currentTest = CurrentTest;
                if (currentTest == null)
                    return;                
                if (!Active)
                    return;
                if (testAction == null)
                {
                    if (currentTest.TemplateItem.Buttons == TestButtons.Execute || currentTest.TemplateItem.Buttons == TestButtons.ExecuteCorrect)
                        testAction = TestActionType.Execute;
                    else
                        testAction = TestActionType.Success;
                }
                if (currentTest.CanExecute)
                {
                    SetStatus(string.Format(Properties.Resources.atExecuteTest, currentTest.TemplateItem.Name));                    
                    CheckupManager.DoTestAction(currentTest, testAction.Value, null, true);
                }
                if (!currentTest.Finished || currentTest.HasErrors)
                {
                    Console.Beep();
                    var action = AutoTestFailedDialog.Show(currentTest);                    
                    if (action == AutoTestErrorAction.Retry)
                    {
                        Statistics.Started--;
                        StartTest();
                    }
                    else if (action == AutoTestErrorAction.Skip)
                    {
                        Statistics.Skipped++;
                        SwitchTest();
                    }
                    else if (action == AutoTestErrorAction.Stop)
                    {
                        Stop(true);
                    }
                    return;
                }
                Statistics.Completed++;
                Services.DoEvents();
                SwitchTest();                
            }));            
        }        

        /// <summary>
        /// Переключение тестов в процессе автоматического тестирования
        /// </summary>
        private void SwitchTest()
        {
            if (!Active)
                return;
            SelectNext();
            if (CurrentTest != null)
            {
                StartTest();
            }
            else
            {
                Stop();
            }
        }       
        #endregion

        #region Parse Group
        private void ParseTestGroup(TestItemGroup group)
        {
            if (!group.IsAutoTestingGroup)
                return;
            foreach (var testId in group.TestIdList)
            {
                AddTest(testId);
            }
        }

        private void AddTest(TestObject test)
        {
            if (test == null)
                return;
            if (test.TemplateItem.IsContainer)
            {
                foreach (var child in test.Children)
                {
                    AddTest(child);
                }
            }
            else if (!Tests.Contains(test))
            {
                Tests.Add(test);
            }
        }

        private void AddTest(string testId)
        {
            TestObject test = CheckupManager.TestsAncestor.GetTestById(testId);
            AddTest(test);
        }
        #endregion        
    }

    /// <summary>
    /// 
    /// </summary>
    public class AutoTestingStatistics : BaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public AutoTestingStatistics(int testCount)
        {
            All = testCount;
        }

        private int all;
        /// <summary>
        /// 
        /// </summary>
        public int All
        {
            get { return all; }
            internal set
            {
                if (all != value)
                {
                    all = value;
                    OnPropertyChanged("All");
                }
            }
        }

        private int started;
        /// <summary>
        /// 
        /// </summary>
        public int Started
        {
            get { return started; }
            internal set
            {
                if (started != value)
                {
                    started = value;
                    OnPropertyChanged("Started");
                }
            }
        }

        private int skipped;
        /// <summary>
        /// 
        /// </summary>
        public int Skipped
        {
            get { return skipped; }
            internal set
            {
                if (skipped != value)
                {
                    skipped = value;
                    OnPropertyChanged("Skipped");
                    OnPropertyChanged("Finished");
                }
            }
        }

        private int completed;
        /// <summary>
        /// 
        /// </summary>
        public int Completed
        {
            get { return completed; }
            internal set
            {
                if (completed != value)
                {
                    completed = value;
                    OnPropertyChanged("Completed");
                    OnPropertyChanged("Finished");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Finished
        {
            get { return Skipped + Completed; }
        }
    }
}
