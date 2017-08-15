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

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Interaction logic for TestDockableContent.xaml
    /// </summary>
    public partial class TestDockableContent : ChildContent, IChildForm
    {        
        private ToolBar[] toolBars;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="viewModel"></param>
        public TestDockableContent(TestContentViewModel viewModel, DeviceTestCheckupManager manager)
        {
            InitializeComponent();
            HideOnClose = true;
            Manager = manager;
            ViewModel = viewModel;
            //InitErrorAnimation();
            ResourceDictionary commonResources = new ResourceDictionary();
            commonResources.Source = new Uri("NeuroSoft.DeviceAutoTest;Component/Resources/Common.xaml", UriKind.Relative);
            ToolBar TestDefaultToolBar = (ToolBar)commonResources["TestDefaultToolBar"];
            TestDefaultToolBar.DataContext = this.DataContext;
            TestDefaultToolBar.Name = "TestDefaultToolBar";
            toolBars = new ToolBar[1] { TestDefaultToolBar };

            CommandBindings.Add(new CommandBinding(DATTestingCommands.OpenTestCommand, OnExecutedOpenTestCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.NextCommand, OnExecutedNextCommand, OnCanExecuteNextCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.PreviousCommand, OnExecutedPreviousCommand, OnCanExecutePreviousCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.RefreshTestCommand, OnExecutedRefreshTestCommand, OnCanExecuteRefreshTestCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.StopTestCommand, OnExecutedStopTestCommand, OnCanExecuteStopTestCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.StartTestCommand, OnExecutedStartTestCommand, OnCanExecuteStartTestCommand));
            
            CommandBindings.Add(new CommandBinding(DATTestingCommands.ShowTestHistoryCommand, OnExecutedShowTestHistoryCommand, OnCanExecuteShowTestHistoryCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.ExecuteTestCommand, OnExecutedExecuteTestCommand, OnCanExecuteExecuteTestCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.SuccessTestCommand, OnExecutedSuccessTestCommand, OnCanExecuteSuccessTestCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.CorrectTestCommand, OnExecutedCorrectTestCommand, OnCanExecuteCorrectTestCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.HasErrorsTestCommand, OnExecutedHasErrorsTestCommand, OnCanExecuteHasErrorsTestCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.ReTestCommand, OnExecutedReTestCommand, OnCanExecuteReTestCommand));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.ConfirmFinishTestCommand, OnExecutedConfirmFinishTestCommand, OnCanExecuteConfirmFinishTestCommand));

            CommandBindings.Add(new CommandBinding(DATTestingCommands.PressButton1Command, OnExecutedPressButton1Command, OnCanExecutePressButton1Command));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.PressButton2Command, OnExecutedPressButton2Command, OnCanExecutePressButton2Command));
            CommandBindings.Add(new CommandBinding(DATTestingCommands.PressButton3Command, OnExecutedPressButton3Command, OnCanExecutePressButton3Command));

            ViewModel.ZoomChanged += new RoutedEventHandler(ViewModel_ZoomChanged);
            InitNextPrevTests();
            Loaded += new RoutedEventHandler(TestDockableContent_Loaded);
            EventManager.RegisterClassHandler(typeof(TestDockableContent), PreviewKeyDownEvent, new KeyEventHandler((object sender, KeyEventArgs e) => {
                ViewModel.ShowManualAutoTestMessage = false;
            }), true);
        }

        void ViewModel_ZoomChanged(object sender, RoutedEventArgs e)
        {
            UpdatePageWidth();
        }

        void TestDockableContent_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePageWidth();
        }

        #region Properties
        private TestContentViewModel viewModel;

        /// <summary>
        /// Модель представления
        /// </summary>
        public TestContentViewModel ViewModel
        {
            get { return viewModel; }
            set 
            {
                if (viewModel != value)
                {
                    viewModel = value;
                    DataContext = viewModel;
                }
            }
        }

        private DeviceTestCheckupManager manager;

        /// <summary>
        /// Менеджер обследования
        /// </summary>
        public DeviceTestCheckupManager Manager
        {
            get { return manager; }
            private set { manager = value; }
        }        

        private TestObject NextTest;
        private TestObject PrevTest;

        /// <summary>
        /// Признак выполнения текущего теста в ручном режиме в процессе автоматического тестирования
        /// </summary>
        public bool IsManualTesting
        {
            get
            {
                return Manager.AutoTestingIsActive && ViewModel.Model.TemplateItem.AutoTestingSettings.Manual && Manager.CurrentAutoTestManager.CurrentTest == ViewModel.Model;
            }
        }
        #endregion
        
        #region Methods

        private void UpdatePageWidth()
        {
            double maxWidth = -1;
            //double actualWidth = ContentGrid.ActualWidth;
            double scale = ViewModel.Zoom / 100;
            foreach (var elem in ProtocolPatternMakerHelper.ExtractFromTextRange<FrameworkElement>(new TextRange(ViewModel.ModelContent.ContentStart, ViewModel.ModelContent.ContentEnd)))
            {
                double currWidth = elem.Width * scale;
                if (currWidth > maxWidth)
                {
                    maxWidth = currWidth;
                }
            }
            if (maxWidth > 0)
            {
                ViewModel.ModelContent.MinPageWidth = maxWidth / scale;
            }
            else
            {
                ViewModel.ModelContent.MinPageWidth = 0;
            }
        }

        private void InitNextPrevTests()
        {
            TestObject test = ViewModel.Model;
            var ancestor = test.AncestorParent;
            TestObject parentContainer = test.ParentContainer;
            List<TestObject> allTests = ancestor.Tests.ToList();// parentContainer != null ? parentContainer.Children : ancestor.Tests.ToList();
            int testIndex = allTests.IndexOf(test);

            PrevTest = ancestor.GetTestById(test.TemplateItem.PreviousTestId);            
            if (PrevTest == null)
            {
                if (testIndex < 1)
                {
                    PrevTest = null;
                }
                else
                {
                    PrevTest = allTests[testIndex - 1];
                }
            }

            NextTest = test.NextTest;
            if (NextTest == null)
            {
                if (test.RequareToExecuteTests.Count > 0)
                {
                    NextTest = test.RequareToExecuteTests[0];
                }
                else if (testIndex >= allTests.Count - 1)
                {
                    NextTest = null;
                }
                else
                {
                    NextTest = allTests[testIndex + 1];
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ToolBar[] GetToolBars()
        {
            return toolBars;
        }

        private void DoExecuteTest()
        {            
            if (ViewModel.Model.WasCorrected && string.IsNullOrEmpty(ViewModel.Model.Comments))
            {
                if (ViewModel.IsExecuting)
                    return;
                ViewModel.IsExecuting = true;
                try
                {
                    var correctDialog = new CorrectTestDialog(ViewModel.Model);                    
                    if (correctDialog.ShowDialog() == true)
                    {                        
                        ViewModel.IsExecuting = false;
                        DoAction(TestActionType.Execute, ViewModel.Model.Comments);
                    }
                }
                finally
                {
                    ViewModel.IsExecuting = false;
                }
            }
            else
            {
                DoAction(TestActionType.Execute);
            }
            
        }

        private void DoSuccessTest()
        {
            DoAction(TestActionType.Success);
        }

        private void DoHasErrorsTest()
        {
            DoAction(TestActionType.HasErrors);
        }

        private void DoCorrectTest(string correctionString = null)
        {            
            if (ViewModel.IsExecuting)
                return;
            ViewModel.IsExecuting = true;
            try
            {
                var correctDialog = new CorrectTestDialog(ViewModel.Model);
                if (correctionString != null)
                    correctDialog.Comments = correctionString;
                if (correctDialog.ShowDialog() == true)
                {
                    ViewModel.IsExecuting = false;
                    DoAction(TestActionType.Correct, ViewModel.Model.Comments);
                }
                
            }
            finally
            {
                ViewModel.IsExecuting = false;
            }
        }
        
        private void DoAction(TestActionType action, string comments = null)
        {
            if (ViewModel.IsExecuting)
                return;
            ViewModel.IsExecuting = true;            
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(()=>
            {
                try
                {
                    Manager.DoTestAction(ViewModel.Model, action, comments);
                    StartTest();
                }
                finally
                {
                    ViewModel.IsExecuting = false;
                }
            }));
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            TestObject test = (sender as Hyperlink).DataContext as TestObject;
            if (test == null) return;
            TestTreeObject treeObject = TestTreeObject.FindTestTreeObject(Manager.TreeObject, test.TestId);
            if (treeObject == null) return;
            treeObject.Open();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            var abortDialog = new TestCommentsDialog(Properties.Resources.AbortStr, Properties.Resources.AbortReason);
            if (abortDialog.ShowDialog() == true)
            {
                Manager.DoTestAction(ViewModel.Model, TestActionType.Abort, abortDialog.Comments);
            }
        }
        private bool WasActivated = false;
        internal void OnActivated()
        {
            ViewModel.Model.TestOpened = true;
            InitNextPrevTests();
            StartTest();
            ViewModel.ShowManualAutoTestMessage = IsManualTesting;
        }

        internal void OnDeactivated()
        {
            StopTest();
            ViewModel.Model.TestOpened = false;
            ViewModel.ShowManualAutoTestMessage = false;
        }

        private void StartTest()
        {
            if (WasActivated)
            {
                ViewModel.Model.InvalidateCanExecute();
            }
            WasActivated = true;
            if (ViewModel.Model.CanExecute)
            {
                CommentsTextBox.Focus();
            }
            ViewModel.Model.BeginTest();
            viewModel.TestStarted = true;
        }

        private void StopTest()
        {
            ViewModel.Model.EndTest();
            viewModel.TestStarted = false;
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            if (NSMessageBox.Show(Properties.Resources.ConfirmIgnoreTestDependencies, Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ViewModel.Model.IgnoreTestDependencies = true;
                OnActivated();
            }
        }
        #endregion

        #region Commands

        #region OpenTestCommand

        private void OnExecutedOpenTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TestObject test = e.Parameter as TestObject;
            if (test != null)
            {
                Manager.OpenTest(test);
            }
        }
        #endregion

        #region NextCommand
        private void OnCanExecuteNextCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = NextTest != null && !Manager.AutoTestingIsActive;
        }

        private void OnExecutedNextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OpenTest(NextTest);
        }

        private void OpenTest(TestObject test)
        {
            var treeObject = TestTreeObject.FindTestTreeObject(Manager.TreeObject, test.TestId);
            if (treeObject != null)
            {
                if (treeObject.Form == null)
                {
                    treeObject.CheckupManager.ShowContent(new DummyChildContent(treeObject, treeObject.Text));
                }
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    treeObject.Open();
                }));
                //treeObject.Open();
            }
        }
        #endregion

        #region PreviousCommand
        private void OnCanExecutePreviousCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = PrevTest != null && !Manager.AutoTestingIsActive;
        }

        private void OnExecutedPreviousCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var treeObject = TestTreeObject.FindTestTreeObject(Manager.TreeObject, PrevTest.TestId);
            if (treeObject != null)
            {
                treeObject.Open();
            }
        }
        #endregion
        
        #region RefreshTestCommand

        private void OnExecutedRefreshTestCommand(object sender, ExecutedRoutedEventArgs e)
        {            
            StopTest();
            Manager.TestsAncestor.ScriptExecutionEnvironment.CloseDevices();
            StartTest();
            //bool oldCanExecute = ViewModel.Model.CanExecute;
            //ViewModel.Model.InvalidateCanExecute();
            //if (!oldCanExecute)
            //{
            //    ViewModel.Model.BeginTest();
            //}
            //else if (!ViewModel.Model.CanExecute)
            //{
            //    ViewModel.Model.EndTest(true);
            //}
        }

        private void OnCanExecuteRefreshTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Manager.AutoTestingIsActive || IsManualTesting;
        }
        #endregion

        #region StopTestCommand
        private void OnCanExecuteStopTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting)
            {
                e.CanExecute = false;
                return;
            }
            var templateItem = ViewModel.Model.TemplateItem;
            e.CanExecute = ViewModel.Model.CanExecute && ViewModel.TestStarted;
        }

        private void OnExecutedStopTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            StopTest();
            Manager.TestsAncestor.ScriptExecutionEnvironment.CloseDevices();
        }
        #endregion

        #region StartTestCommand
        private void OnCanExecuteStartTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting)
            {
                e.CanExecute = false;
                return;
            }
            var templateItem = ViewModel.Model.TemplateItem;
            e.CanExecute = ViewModel.Model.CanExecute && !ViewModel.TestStarted;
        }

        private void OnExecutedStartTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            StartTest();
        }
        #endregion

        #region ShowTestHistoryCommand
        private void OnCanExecuteShowTestHistoryCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = ViewModel.Model.HasHistory;
        }

        private void OnExecutedShowTestHistoryCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TestingHistoryDialog dialog = new TestingHistoryDialog(ViewModel.Model.AncestorParent, ViewModel.Model);
            dialog.ShowDialog();
        }
        #endregion

        #region ExecuteTestCommand
        private void OnCanExecuteExecuteTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting || !ViewModel.TestStarted)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = ViewModel.Model.TemplateItem.Buttons == TestButtons.Execute || ViewModel.Model.TemplateItem.Buttons == TestButtons.ExecuteCorrect;
        }

        private void OnExecutedExecuteTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DoExecuteTest();            
        }
        #endregion

        #region SuccessTestCommand
        private void OnCanExecuteSuccessTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting || !ViewModel.TestStarted)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = ViewModel.Model.TemplateItem.Buttons != TestButtons.Execute;
        }

        private void OnExecutedSuccessTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DoSuccessTest();            
        }
        #endregion

        #region HasErrorsTestCommand
        private void OnCanExecuteHasErrorsTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting || !ViewModel.TestStarted)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = ViewModel.Model.TemplateItem.Buttons == TestButtons.YesNo || ViewModel.Model.TemplateItem.Buttons == TestButtons.YesNoCorrect;
        }

        private void OnExecutedHasErrorsTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DoHasErrorsTest();
        }
        #endregion
        

        #region CorrectTestCommand
        private void OnCanExecuteCorrectTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting || !ViewModel.TestStarted)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = ViewModel.Model.TemplateItem.Buttons == TestButtons.YesCorrect || ViewModel.Model.TemplateItem.Buttons == TestButtons.YesNoCorrect ||
                ViewModel.Model.TemplateItem.Buttons == TestButtons.ExecuteCorrect;
        }

        private void OnExecutedCorrectTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DoCorrectTest();
        }
        #endregion        

        #region ReTestCommand
        private void OnCanExecuteReTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = ViewModel.Model.Finished && ViewModel.Model.TemplateItem.ReExecutable && !ViewModel.Model.FinishingIsConfirmed;
        }

        private void OnExecutedReTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DoAction(TestActionType.ReTest, Properties.Resources.ReTestComment);
        }
        #endregion       

        #region ConfirmFinishTestCommand
        private void OnCanExecuteConfirmFinishTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.Model.Finished && ViewModel.Model.TemplateItem.ReExecutable;
        }

        private void OnExecutedConfirmFinishTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DoAction(TestActionType.ConfirmFinish, Properties.Resources.ConfirmTestFinishing);
        }
        #endregion         

        #region PressButton1Command
        private void OnCanExecutePressButton1Command(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting || !ViewModel.TestStarted)
            {
                e.CanExecute = false;
                return;
            }
            bool canExecute = ViewModel.Model.CanExecute && ViewModel.Model.TemplateItem.Buttons != TestButtons.Execute;
            e.CanExecute = !ViewModel.IsExecuting && canExecute || !canExecute && PrevTest != null;
        }

        private void OnExecutedPressButton1Command(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ViewModel.Model.CanExecute)
            {
                OpenTest(PrevTest);
                return;
            }
            if (ViewModel.Model.TemplateItem.Buttons == TestButtons.YesCorrect || ViewModel.Model.TemplateItem.Buttons == TestButtons.ExecuteCorrect)
            {
                DoCorrectTest();
            }           
            else
            {
                DoHasErrorsTest();
            }
        }
        #endregion

        #region PressButton2Command
        private void OnCanExecutePressButton2Command(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting || !ViewModel.TestStarted)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = ViewModel.Model.TemplateItem.Buttons == TestButtons.YesNoCorrect && !ViewModel.IsExecuting && ViewModel.Model.CanExecute;
        }

        private void OnExecutedPressButton2Command(object sender, ExecutedRoutedEventArgs e)
        {
            DoCorrectTest();
        }
        #endregion

        #region PressButton3Command
        private void OnCanExecutePressButton3Command(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Manager.AutoTestingIsActive && !IsManualTesting || !ViewModel.TestStarted)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = !ViewModel.IsExecuting && (ViewModel.Model.CanExecute || NextTest != null);
        }

        private void OnExecutedPressButton3Command(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ViewModel.Model.CanExecute)
            {
                OpenTest(NextTest);
                return;
            }
            if (ViewModel.Model.TemplateItem.Buttons == TestButtons.Execute)
            {
                DoExecuteTest();
            }
            else
            {
                DoSuccessTest();
            }
        }
        #endregion                        

        private void OnExecutedZoomCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.Zoom = Convert.ToDouble(e.Parameter);
        }
              
        #endregion

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = GetClickedItem(sender as ListBox, e);
            if (item == null) return;
            DoCorrectTest(item.Content as string);
        }

        private ListBoxItem GetClickedItem(ListBox _listBox, MouseButtonEventArgs e)
        {
            if (_listBox == null)
                return null;
            var pos = e.GetPosition(_listBox);
            var hitTestResult = VisualTreeHelper.HitTest(_listBox, pos);
            if (hitTestResult == null)
            {
                return null;
            }
            var d = hitTestResult.VisualHit;
            while (d != null)
            {
                if (d is ListBoxItem)
                {
                    return d as ListBoxItem;
                }
                d = VisualTreeHelper.GetParent(d);
            }
            return null;
        }

        bool alreadyMoved;
        private void ManualAutoTestBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel.ShowManualAutoTestMessage && alreadyMoved)
            {
                ViewModel.ShowManualAutoTestMessage = false;
                alreadyMoved = false;
            }
            if (ViewModel.ShowManualAutoTestMessage)
                alreadyMoved = true;            
        }

        private TestHistoryDockableContent historyContent;
        /// <summary>
        /// 
        /// </summary>
        internal TestHistoryDockableContent HistoryContent
        {
            get
            {
                if (historyContent == null && ViewModel != null)
                {
                    historyContent = new TestHistoryDockableContent(Manager.TestsAncestor, ViewModel.Model);
                }                
                return historyContent;
            }
        }
    }
}
