using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace NeuroSoft.DeviceAutoTest
{    
    /// <summary>
    /// Объект теста.
    /// </summary>
    [Serializable]
    public class TestObject : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="templateItem"></param>
        public TestObject(DeviceTestCheckupAncestor parent, TestTemplateItem templateItem)
        {
            AncestorParent = parent;
            testId = templateItem.TestId;
            TemplateItem = templateItem;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TestObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        [Serialize]
        private string testId;
        [Serialize]
        private string comments = string.Empty;
        [Serialize]
        private bool finished;
        [Serialize]
        private bool wasCorrected;
        [Serialize]
        private bool hasErrors;
        [Serialize]
        private bool finishingIsConfirmed = false;
        [Serialize]
        private bool reExecuted = false;

        private TestTemplateItem templateItem;
        private DeviceTestCheckupAncestor ancestorParent;
        private bool ignoreTestDependencies = false;
        
        
        /// <summary>
        /// Шаблон, на основе которого создан тест.
        /// </summary>
        public TestTemplateItem TemplateItem
        {
            get 
            {
                if (templateItem == null && AncestorParent != null)
                {
                    templateItem = AncestorParent.TestTemplate.FindTestById(TestId);
                }
                return templateItem;
            }
            internal set 
            { 
                templateItem = value;
            }
        }
        
        /// <summary>
        /// Родитель-хранитель тестов
        /// </summary>
        public DeviceTestCheckupAncestor AncestorParent
        {
            get { return ancestorParent; }
            internal set
            {
                if (ancestorParent != value)
                {
                    ancestorParent = value;                    
                }
            }
        }

        private bool containsSupplyCurrent = false;
        /// <summary>
        /// Признак отображения тока потребления в тесте
        /// </summary>
        public bool ContainsSupplyCurrent
        {
            get { return containsSupplyCurrent; }
            internal set { containsSupplyCurrent = value; }
        }        
        /// <summary>
        /// Родительское составное действие
        /// </summary>
        public TestObject ParentContainer
        {
            get
            {
                if (TemplateItem != null)
                {
                    var parentContainerTemplate = TemplateItem.ParentContainer;
                    if (parentContainerTemplate != null)
                    {
                        return AncestorParent.GetTestById(parentContainerTemplate.TestId);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Признак завершенности действия.
        /// </summary>
        public bool Finished
        {
            get { return finished; }
            set
            {
                if (finished != value)
                {
                    finished = value;
                    if (!finished || TemplateItem != null && !TemplateItem.ReExecutable)
                    {
                        FinishingIsConfirmed = finished;
                    }
                    else if (TemplateItem != null && TemplateItem.IsContainer)
                    {
                        OnPropertyChanged("FinishingIsConfirmed");
                    }
                    OnPropertyChanged("Finished");
                    OnPropertyChanged("Status");
                    OnPropertyChanged("TestCorrections");

                    if (ReExecuted && finished)
                    {
                        FinishingIsConfirmed = true;
                        OnPropertyChanged("FinishingIsConfirmed");
                    }
                }                
            }
        }

        /// <summary>
        /// Признак пользовательского подтверждения завершенности тестирования
        /// </summary>
        public bool FinishingIsConfirmed
        {
            get 
            {
                if (TemplateItem != null && TemplateItem.IsContainer)
                {
                    return !Children.Any(t => t != null && !t.FinishingIsConfirmed);
                }
                return finishingIsConfirmed; 
            }
            set
            {
                if (finishingIsConfirmed != value)
                {
                    finishingIsConfirmed = value;                    
                    var parent = ParentContainer;
                    OnPropertyChanged("FinishingIsConfirmed");
                    while (parent != null)
                    {
                        parent.OnPropertyChanged("FinishingIsConfirmed");
                        parent = parent.ParentContainer;
                    }
                }
            }
        }        
        
        /// <summary>
        /// Признак того, что тест был повторно выполнен
        /// </summary>
        public bool ReExecuted
        {
            get 
            {
                if (TemplateItem != null && TemplateItem.IsContainer)
                {
                    return !Children.Any(t => t.TemplateItem != null && t.TemplateItem.ReExecutable && !t.ReExecuted);
                }
                return reExecuted; 
            }
            set
            {
                if (reExecuted != value)
                {
                    reExecuted = value;
                    OnPropertyChanged("ReExecuted");
                    var parent = ParentContainer;                    
                    while (parent != null)
                    {
                        parent.OnPropertyChanged("ReExecuted");
                        parent = parent.ParentContainer;
                    }
                }
            }
        }


        /// <summary>
        /// Признак того, что для выполнения действия в процессе наладки 
        /// были внесены какие-то исправления
        /// </summary>
        public bool WasCorrected
        {
            get { return wasCorrected; }
            set
            {
                if (wasCorrected != value)
                {
                    wasCorrected = value;
                    OnPropertyChanged("WasCorrected");
                    OnPropertyChanged("Status");                    
                }
            }
        }

        /// <summary>
        /// Выявлены ли ошибки/отклонения от нормы при выполнении действия
        /// </summary>
        public bool HasErrors
        {
            get { return hasErrors; }
            set
            {
                if (hasErrors != value)
                {
                    hasErrors = value;
                    OnPropertyChanged("HasErrors");
                    OnPropertyChanged("Status");
                }
            }
        }

        private long startTime = -1;
        /// <summary>
        /// Время запуска теста
        /// </summary>
        public long StartTime
        {
            get { return startTime; }
            internal set { startTime = value; }
        }
        

        /// <summary>
        /// Текущее состояние действия
        /// </summary>
        public TestObjectStatus Status
        {
            get
            {
                if (!Finished)
                {
                    return TestObjectStatus.NotExecuted;
                }
                else if (HasErrors)
                {
                    return TestObjectStatus.HasErrors;
                }
                else if (WasCorrected)
                {
                    return TestObjectStatus.Corrected;
                }
                else
                {
                    return TestObjectStatus.Success;
                }
            }
        }
        /// <summary>
        /// Комментарии к тесту
        /// </summary>
        public string Comments
        {
            get { return comments; }
            set
            {
                if (comments != value)
                {
                    comments = value;
                    OnPropertyChanged("Comments");
                    if (AncestorParent != null)
                    {
                        AncestorParent.Modified = true;
                    }
                }
            }
        }
        /// <summary>
        /// Список всех исправлений теста (для составного теста - исправления во вложенных)
        /// </summary>
        public List<string> TestCorrections
        {
            get
            {
                List<string> result = new List<string>();
                if (!WasCorrected)
                    return result;
                if (TemplateItem.IsContainer)
                {
                    foreach (var test in Children)
                    {
                        result.AddRange(test.TestCorrections);
                    }
                }
                else
                {
                    result.Add(Comments);
                }
                return result;
            }
        }

        /// <summary>
        /// Признак принудительного игнорирования зависимостей теста
        /// </summary>
        public bool IgnoreTestDependencies
        {
            get 
            {
                if (AncestorParent.ScriptExecutionEnvironment.IsDebug)
                    return true;
                return ignoreTestDependencies;
            }
            internal set
            {
                if (ignoreTestDependencies != value)
                {
                    ignoreTestDependencies = value;
                    InvalidateCanExecute();
                    OnPropertyChanged("IgnoreTestDependencies");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CanIgnoreTestDependencies
        {
            get
            {
                return !IgnoreTestDependencies && requareToExecDeps;
            }
        }

        /// <summary>
        /// Список вложенных действий
        /// </summary>
        internal List<TestObject> Children
        {
            get
            {
                List<TestObject> result = new List<TestObject>();
                if (TemplateItem.IsContainer)
                {
                    foreach (var testTemplate in TemplateItem.Nodes)
                    {
                        if (testTemplate is TestTemplateItem)
                        {
                            var test = AncestorParent.GetTestById((testTemplate as TestTemplateItem).TestId);
                            result.Add(test);
                        }
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// Идентификатор теста (для связи с шаблоном теста)
        /// </summary>
        public string TestId
        {
            get { return testId; }
            private set { testId = value; }
        }

        /// <summary>
        /// Тест, который следует запускать после завершения данного теста
        /// </summary>
        public TestObject NextTest
        {
            get
            {
                return (from test in AncestorParent.Tests where test.TemplateItem != null && test.TemplateItem.PreviousTestId == TestId select test).FirstOrDefault();                
            }
        }


        /// <summary>
        /// Следующее невыполненное действие
        /// </summary>
        public TestObject NextNotFinishedTest
        {
            get
            {
                var nextTest = NextTest;
                if (nextTest != null)
                {
                    if (IsNextTestCandidate(nextTest))
                    {
                        return nextTest;
                    }
                    else
                    {
                        return nextTest.NextNotFinishedTest;
                    }
                }
                TestObject parentContainer = ParentContainer;
                List<TestObject> siblings = parentContainer != null ? parentContainer.Children : AncestorParent.Tests.ToList();
                int index = siblings.IndexOf(this);
                for (int i = index+1; i < siblings.Count; i++)
                {
                    if (IsNextTestCandidate(siblings[i]))
                    {
                        return siblings[i];
                    }
                }
                for (int i = 0; i < index; i++)
                {
                    if (IsNextTestCandidate(siblings[i]))
                    {
                        return siblings[i];
                    }
                }
                if (parentContainer != null)
                {
                    if (IsNextTestCandidate(parentContainer))
                    {
                        return parentContainer;
                    }
                    else
                    {
                        return parentContainer.NextNotFinishedTest;
                    }
                }                
                return null;
            }
        }

        /// <summary>
        /// Определяет, подходит ли тест для перехода с текущего
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        private bool IsNextTestCandidate(TestObject test)
        {
            //Если текущий тест повторно выполним и его выполнение подтверждено, либо он был выполнен повторно, то для перехода доступны все неподтвержденные тесты
            if (ReExecuted && TemplateItem.ReExecutable)
                return !test.FinishingIsConfirmed;
            //Иначе доступны все невыполненные ни разу
            return !test.Finished;            
        }

        /// <summary>
        /// Признак наличия истории по данному действию
        /// </summary>
        public bool HasHistory
        {
            get
            {
                return (from snapshot in AncestorParent.Snapshots where snapshot.ExecutedTestId == TestId select snapshot).FirstOrDefault() != null;
            }
        }

        private bool testOpened = false;

        /// <summary>
        /// Признак того, что тест открыт
        /// </summary>
        internal bool TestOpened
        {
            get { return testOpened; }
            set
            {
                if (testOpened != value)
                {
                    testOpened = value;
                    StartTime = testOpened ? DateTime.UtcNow.Ticks : -1;
                    OnPropertyChanged("CanExecute");
                }
            }
        }

        private bool? canExecute;
        /// <summary>
        /// Признак возможности выполнения теста
        /// </summary>
        public bool CanExecute
        {
            get
            {
                if (!TestOpened)
                {
                    LockExecutionReason = Properties.Resources.TestIsNotLoaded;
                    return false;
                }
                return InternalCanExecute;
            }
        }

        /// <summary>
        /// Может ли быть выполнен тест вне зависимости от того, открыт ли он
        /// </summary>
        internal bool InternalCanExecute
        {
            get
            {
                if (canExecute == null)
                {
                    LockExecutionReason = string.Empty;
                    canExecute = true;
                    if (TemplateItem != null && TemplateItem.IsContainer)
                    {
                        var notExecuted = (from t in Children where !t.Finished select t).FirstOrDefault();
                        if (notExecuted != null)
                        {
                            LockExecutionReason = Properties.Resources.NeedToExecuteTests;
                            canExecute = false;
                            return canExecute.Value;
                        }
                        if (!Finished)
                        {
                            canExecute = true;
                            return canExecute.Value;
                        }
                    }
                    if (Finished)
                    {
                        if (HasErrors)
                        {
                            LockExecutionReason = Properties.Resources.TestExecutedWithErrors;
                        }
                        else if (WasCorrected)
                        {
                            LockExecutionReason = Properties.Resources.TestExecutedWithCorrections;
                        }
                        else
                        {
                            LockExecutionReason = Properties.Resources.TestExecutedSuccessfully;
                        }
                        canExecute = false;
                        return canExecute.Value;
                    }
                    if (TemplateItem == null)
                    {
                        LockExecutionReason = string.Empty;
                        canExecute = false;
                        return canExecute.Value;
                    }

                    if (RequareToExecuteTests.Count > 0)
                    {
                        LockExecutionReason = Properties.Resources.NeedToExecuteTests;
                        canExecute = false;
                        return canExecute.Value;
                    }
                    if (TemplateItem.ValidationScript.IsEnabled)
                    {
                        ValidationResult validationResult = ExecuteInternal(TemplateItem.ValidationScript, true) as ValidationResult;
                        if (validationResult == null || !validationResult.IsValid)
                        {
                            if (validationResult == null)
                            {
                                LockExecutionReason = Properties.Resources.ValidationError;
                            }
                            else
                            {
                                LockExecutionReason = Convert.ToString(validationResult.ErrorContent);
                            }
                            canExecute = false;
                            OnPropertyChanged("CanExecute");
                            return canExecute.Value;
                        }
                    }
                }
                return canExecute.Value;
            }
        }
        

        /// <summary>
        /// Выполнение скрипта начала теста (выполняется, если тест может быть выполнен)
        /// </summary>
        internal void BeginTest()
        {                        
            if (CanExecute)
            {
                var beginTestScript = TemplateItem.BeginTestScript;
                if (beginTestScript.IsEnabled)
                {
                    ExecuteInternal(beginTestScript, true);
                }
                if (ContainsSupplyCurrent)
                {
                    AncestorParent.ScriptExecutionEnvironment.StartReadSupplyCurrent();
                }
            }
        }

        /// <summary>
        /// Выполнение скрипта окончания теста (выполняется, если тест может быть выполнен)
        /// </summary>
        /// <param name="force">Принудительное выполнение</param>
        internal void EndTest(bool force = false)
        {
            if (force || CanExecute)
            {                
                var endTestScript = TemplateItem.EndTestScript;
                if (endTestScript.IsEnabled)
                {
                    ExecuteInternal(endTestScript, true);
                }
                foreach (var contentPresenter in TemplateItem.ContentPresenters)
                {
                    if (contentPresenter.ContentPresenterInstance != null && contentPresenter.ContentPresenterInstance.Content is IDisposable)
                    {
                        (contentPresenter.ContentPresenterInstance.Content as IDisposable).Dispose();
                    }                    
                }
                if (ContainsSupplyCurrent)
                {
                    AncestorParent.ScriptExecutionEnvironment.StopReadSupplyCurrent();
                }
            }
        }
        /// <summary>
        /// Выполнение скрипта для теста
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        internal object ExecuteInternal(ScriptInfo script, bool lockVariablesNotification = false)
        {
            try
            {
                ScriptClassGenerator scriptGenerator = new ScriptClassGenerator(script, AncestorParent.Variables, AncestorParent.Tests);
                scriptGenerator.LockVariablesNotification = lockVariablesNotification;
                //Выполнение скрипта валидации
                AncestorParent.ScriptExecutionEnvironment.SetTestInfo(this);
                // Добавить объект управления осциллографом
                AncestorParent.ScriptExecutionEnvironment.CreateItem("OscScopeControl", (App.Current.MainWindow as MainWindow).OscScopeCntrl); 
                object result = scriptGenerator.Execute(AncestorParent.ScriptExecutionEnvironment);
                if (AncestorParent.ScriptExecutionEnvironment.TestWasCorrected && AncestorParent.ScriptExecutionEnvironment.IsAutoTesting)
                {
                    WasCorrected = AncestorParent.ScriptExecutionEnvironment.TestWasCorrected;
                }
                if (AncestorParent.ScriptExecutionEnvironment.OverrideComments != null)
                {
                    Comments = AncestorParent.ScriptExecutionEnvironment.OverrideComments;
                }
                return result;
            }
            finally
            {
                AncestorParent.ScriptExecutionEnvironment.ClearTestInfo(this);
            }
        }

        private string lockExecutionReason;

        /// <summary>
        /// Причина невозможности выполнить тест
        /// </summary>
        public string LockExecutionReason
        {
            get { return lockExecutionReason; }
            set
            {
                if (lockExecutionReason != value)
                {
                    lockExecutionReason = value;
                    OnPropertyChanged("LockExecutionReason");
                }
            }
        }

        List<TestObject> requareToExecuteTests = null;
        /// <summary>
        /// Список тестов, которые необходимо выполнить для выполнения текущего теста
        /// </summary>
        public List<TestObject> RequareToExecuteTests
        {
            get
            {                
                if (requareToExecuteTests == null)
                {
                    if (IgnoreTestDependencies)
                    {
                        requareToExecuteTests = new List<TestObject>();
                        foreach (var test in Children)
                        {
                            if (!test.Finished && !requareToExecuteTests.Contains(test))
                            {
                                requareToExecuteTests.Add(test);
                            }
                        }
                    }
                    else
                    {
                        requareToExecuteTests = GetRequareToExecuteTests();
                    }
                    OnPropertyChanged("CanIgnoreTestDependencies");
                }
                return requareToExecuteTests;
            }
        }

        private bool requareToExecDeps = false;
        private List<TestObject> GetRequareToExecuteTests(bool ignoreChildren = false)
        {
            List<TestObject> result = new List<TestObject>();
            if (!Finished && TemplateItem != null)
            {
                requareToExecDeps = false;
                foreach (var id in TemplateItem.TestDependencesIdList)
                {
                    var test = AncestorParent.GetTestById(id);
                    if (test != null)
                    {
                        if (!test.Finished)
                        {
                            result.Add(test);
                            requareToExecDeps = true;
                        }
                    }
                }
                
                var parentContainer = TemplateItem.ParentContainer;
                if (parentContainer != null)
                {
                    var parentTest = AncestorParent.GetTestById(parentContainer.TestId);
                    if (parentTest != null)
                    {
                        var parentDependencies = parentTest.GetRequareToExecuteTests(true);
                        foreach (var test in parentDependencies)
                        {
                            if (!result.Contains(test))
                            {
                                result.Add(test);
                                requareToExecDeps = true;
                            }
                        }
                    }
                }
                if (!ignoreChildren)
                {
                    foreach (var test in Children)
                    {
                        if (!test.Finished && !result.Contains(test))
                        {
                            result.Add(test);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Обновление признака возможности выполнения теста
        /// </summary>
        public void InvalidateCanExecute()
        {
            requareToExecuteTests = null;
            canExecute = null;
            OnPropertyChanged("CanExecute");
            OnPropertyChanged("RequareToExecuteTests");            
        }

        /// <summary>
        /// Отмена действия
        /// </summary>
        /// <param name="abortChildren"></param>
        public void Abort(bool abortChildren = true)
        {
            if (isAborting)
                return;
            isAborting = true;
            try
            {
                if (!Finished)
                    return;
                Finished = false;
                HasErrors = false;
                WasCorrected = false;
                ReExecuted = false;                
                InvalidateCanExecute();

                if (AncestorParent == null)
                    return;
                foreach (var test in AncestorParent.Tests)
                {
                    if (test.TemplateItem.TestDependencesIdList.Contains(TestId) ||
                        abortChildren && TemplateItem.Nodes.Contains(test.TemplateItem))
                    {
                        test.Abort();
                    }
                }
                var parentContainer = TemplateItem.ParentContainer;
                if (parentContainer != null)
                {
                    var test = AncestorParent.GetTestById(parentContainer.TestId);
                    if (test != null)
                    {
                        test.Abort(false);
                    }
                }

                foreach (var group in TemplateItem.ParentGroups)
                {
                    if (group.SyncAbortTest)
                    {
                        foreach (var siblingId in group.TestIdList)
                        {
                            var test = AncestorParent.GetTestById(siblingId);
                            if (test != null)
                            {
                                test.Abort();
                            }
                        }
                    }
                }
            }
            finally
            {
                isAborting = false;
            }            
        }

        internal void ConfirmFinish(bool confirmChildren = true)
        {
            if (FinishingIsConfirmed || !Finished || !TemplateItem.ReExecutable)
                return;
            FinishingIsConfirmed = true;
            if (confirmChildren)
            {
                foreach (var child in Children)
                {
                    child.ConfirmFinish();
                }
            }
            var parent = ParentContainer;
            if (parent != null)
            {
                if (!parent.Children.Any(t => !t.FinishingIsConfirmed))
                {
                    parent.ConfirmFinish(false);
                }
            }
        }

        private bool isAborting = false;

        /// <summary>
        /// Сброс действий, находящихся в одной группе с данным
        /// </summary>     
        internal void Reset(List<string> alreadyResetTests = null, bool resetChildren = true, bool resetDependencies = true, bool retesting = false)
        {
            //Проверим, не сбрасывался ли уже данный тест
            if (alreadyResetTests == null)
            {
                alreadyResetTests = new List<string>();
            }
            if (alreadyResetTests.Contains(TestId))
                return;
            alreadyResetTests.Add(TestId);
            if (!Finished)
                return;
            if (AncestorParent == null)
                return;
            if (retesting && TemplateItem.ReExecutable && !AncestorParent.ScriptExecutionEnvironment.IsAutoTesting)
            {
                ReExecuted = true;
                if (!FinishingIsConfirmed) //исключим из повторного выполнения уже подтвержденные вложенные тесты
                {
                    foreach (var child in Children)
                    {
                        bool ignoreRetesting = child.FinishingIsConfirmed || !child.TemplateItem.ReExecutable;
                        if (ignoreRetesting && !alreadyResetTests.Contains(child.TestId))
                            alreadyResetTests.Add(child.TestId);
                    }
                }                
            }
            if (!retesting || retesting && TemplateItem.ReExecutable || AncestorParent.ScriptExecutionEnvironment.IsAutoTesting)
            {
                Finished = false;
            }
            if (resetDependencies)
            {                
                //Сбросим зависимые тесты, если вдруг они оказались выполненными, что маловероятно
                //В случае, если они не выполнены, ничего не произойдет
                foreach (var test in AncestorParent.Tests)
                {
                    if (test.TemplateItem.TestDependencesIdList.Contains(TestId))
                    {
                        test.Reset(alreadyResetTests, resetChildren, resetDependencies);
                    }
                }
            }
            if (resetChildren)
            {
                foreach (var test in Children)
                {
                    test.Reset(alreadyResetTests, resetChildren, resetDependencies, retesting);
                }
            }
            var parentContainer = TemplateItem.ParentContainer;
            if (parentContainer != null)
            {
                var test = AncestorParent.GetTestById(parentContainer.TestId);
                if (test != null)
                {
                    test.Reset(alreadyResetTests, false, resetDependencies, retesting);
                }
            }
            if (resetDependencies)
            {
                foreach (var group in TemplateItem.ParentGroups)
                {
                    if (group.SyncResetTest)
                    {
                        foreach (var siblingId in group.TestIdList)
                        {
                            var test = AncestorParent.GetTestById(siblingId);
                            if (test != null)
                            {
                                test.Reset(alreadyResetTests, resetChildren, resetDependencies);
                            }
                        }
                    }
                }
            }
            InvalidateCanExecute();
        }
    }

    /// <summary>
    /// Тип выполненного над тестом действия
    /// </summary>
    public enum TestActionType
    {
        /// <summary>
        /// Выполнение скрипта
        /// </summary>
        Execute,
        /// <summary>
        /// Пометка действия как успешно выполненного
        /// </summary>
        Success,
        /// <summary>
        /// Пометка действия как выполненного с ошибками
        /// </summary>
        HasErrors,
        /// <summary>
        /// Отмена результатов теста
        /// </summary>
        Abort,
        /// <summary>
        /// Внесение исправлений
        /// </summary>
        Correct,
        /// <summary>
        /// Не было произведено действий
        /// </summary>
        None,
        /// <summary>
        /// Повторное прохождение теста
        /// </summary>
        ReTest,
        /// <summary>
        /// Подтвержение завершенности теста
        /// </summary>
        ConfirmFinish,
        /// <summary>
        /// Инициализация процесса наладки
        /// </summary>
        Init
    }

    /// <summary>
    /// Статус выполненности теста.
    /// </summary>
    public enum TestObjectStatus
    {
        /// <summary>
        /// Не выполнен
        /// </summary>
        NotExecuted,
        /// <summary>
        /// Выполнено успешно
        /// </summary>
        Success,
        /// <summary>
        /// Выполнено с ошибками
        /// </summary>
        HasErrors,
        /// <summary>
        /// Выли выполнены исправления
        /// </summary>
        Corrected
    }

    internal static class ScriptEnvironmentEx
    {
        /// <summary>
        /// Добавление окружения для теста
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="test"></param>
        internal static void SetTestInfo(this ScriptEnvironment environment, TestObject test)
        {
            foreach (var contentPresenterTag in test.TemplateItem.ContentPresenters)
            {
                environment[contentPresenterTag.Name] = contentPresenterTag.ContentPresenterInstance;
            }
            environment.TestWasCorrected = false;// test.WasCorrected;
            environment.OverrideComments = null;
        }

        /// <summary>
        /// Удаление окружения для конкретного теста
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="test"></param>
        internal static void ClearTestInfo(this ScriptEnvironment environment, TestObject test)
        {
            foreach (var contentPresenterTag in test.TemplateItem.ContentPresenters)
            {
                environment.Remove(contentPresenterTag.Name);
            }
            environment.ResetTempProps();
        }
    }
}
