using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using System.Windows.Documents;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.WPFComponents.ProtocolPatternMaker.Serialization;
using NeuroSoft.Prototype.Interface;
using System.Windows;
using System.Text.RegularExpressions;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Шаблон действия инструкции по наладке
    /// </summary>
    [Serializable]
    public class TestTemplateItem : DATTemplateTreeViewItem
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="testId"></param>
        public TestTemplateItem(DATTemplateTreeViewItem parent, string testId)
            : base(parent)
        {                        
            TestId = testId;
            Name = Properties.Resources.DefaultTestItemName;
            beginTestScript = new ScriptInfo(TestId + "_BeginTestScript", "ExecutionScript", "void", "");
            endTestScript = new ScriptInfo(TestId + "_EndTestScript", "ExecutionScript", "void", "");
            executionScript = new ScriptInfo(TestId + "_ExecutionScript", "ExecutionScript", "bool?", "return true;") { IsEnabled = true };
            validationScript = new ScriptInfo(TestId + "_ValidationScript", "ExecutionScript", "System.Windows.Controls.ValidationResult", "return new System.Windows.Controls.ValidationResult(true, null);");
            TestContentChanged = true;
            AutoTestingSettings.TestParent = this;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TestTemplateItem(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            AutoTestingSettings.TestParent = this;
        }
        
        #endregion

        #region Properties
              
        [Serialize]
        private string testId;
        [Serialize]
        private SerializedFlowDocumentContent serializedContent = new SerializedFlowDocumentContent();
        [Serialize]
        private TestButtons buttons;
        [Serialize]
        private SerializedList<string> testDependencesIdList = new SerializedList<string>();
        [Serialize]
        private ScriptInfo executionScript;
        [Serialize]
        private ScriptInfo beginTestScript;
        [Serialize]
        private ScriptInfo endTestScript;
        [Serialize]
        private ScriptInfo validationScript;
        [Serialize]
        private SerializedList<ScriptInfo> buttonScripts = new SerializedList<ScriptInfo>();
        [Serialize]
        private SerializedList<ContentPresenterTag> contentPresenters = new SerializedList<ContentPresenterTag>();
        [Serialize]
        private SerializedList<string> probableCorrections = new SerializedList<string>();
        [Serialize]
        private string previousTestId;
        [Serialize]
        private bool isContainer = false;
        [Serialize]
        private bool reExecutable = false;
        [Serialize]
        private string reExecuteDescription;
        [Serialize]
        private AutoTestSettings autoTestingSettings = new AutoTestSettings();
        [Serialize]
        private bool parseUsedVariablesFromContent;
        [Serialize]
        private SerializedList<string> usedVariables = new SerializedList<string>();
        [Serialize]
        private SerializedList<string> serializedVariablesFromContent = null;
        /// <summary>
        /// Идентификатор теста
        /// </summary>
        public string TestId
        {
            get { return testId; }
            internal set 
            {
                if (testId != value)
                {
                    testId = value;
                    OnPropertyChanged("TestId");
                }
            }
        }

        /// <summary>
        /// Скрипт выполнения действия
        /// </summary>
        public ScriptInfo ExecutionScript
        {
            get 
            {
                if (executionScript.TemplateItemParent == null)
                {
                    executionScript.TemplateItemParent = this;
                }
                return executionScript; 
            }
            set 
            {
                executionScript = value;
                OnPropertyChanged("ExecutionScript");
            }
        }

        /// <summary>
        /// Скрипт валидации действия
        /// </summary>
        public ScriptInfo ValidationScript
        {
            get
            {
                if (validationScript.TemplateItemParent == null)
                {
                    validationScript.TemplateItemParent = this;
                }
                return validationScript;
            }
            set
            {
                validationScript = value;
                OnPropertyChanged("ValidationScript");
            }
        }

        /// <summary>
        /// Скрипт, выполняемый при открытии теста
        /// </summary>
        public ScriptInfo BeginTestScript
        {
            get
            {
                if (beginTestScript.TemplateItemParent == null)
                {
                    beginTestScript.TemplateItemParent = this;
                }
                return beginTestScript;
            }
            set
            {
                beginTestScript = value;
                OnPropertyChanged("BeginTestScript");
            }
        }

        /// <summary>
        /// Скрипт, выполняемый при закрытии теста
        /// </summary>
        public ScriptInfo EndTestScript
        {
            get
            {
                if (endTestScript.TemplateItemParent == null)
                {
                    endTestScript.TemplateItemParent = this;
                }
                return endTestScript;
            }
            set
            {
                endTestScript = value;
                OnPropertyChanged("EndTestScript");
            }
        }

        /// <summary>
        /// Список тегов-ссылок на ContentPresentor'ы внутри содержимого теста
        /// </summary>
        public SerializedList<ContentPresenterTag> ContentPresenters
        {
            get { return contentPresenters; }
        }

        /// <summary>
        /// Набор скриптов, выполняемых по нажатию соответствующей кнопки
        /// </summary>
        public SerializedList<ScriptInfo> ButtonScripts
        {
            get { return buttonScripts; }
            private set { buttonScripts = value; }
        }

        /// <summary>
        /// Список возможных исправлений в ходе теста
        /// </summary>
        public SerializedList<string> ProbableCorrections
        {
            get { return probableCorrections; }
        }

        /// <summary>
        /// Признак извлечения используемых переменных из содержимого теста
        /// </summary>
        public bool ParseUsedVariablesFromContent
        {
            get 
            {
                if (IsContainer)
                    return false;
                return parseUsedVariablesFromContent; 
            }
            set
            {
                if (parseUsedVariablesFromContent != value)
                {
                    parseUsedVariablesFromContent = value;  
                    OnPropertyChanged("ParseUsedVariablesFromContent");
                }
            }
        }
        /// <summary>
        /// Список используемых в тесте переменных
        /// </summary>
        public SerializedList<string> UsedVariables
        {
            get { return usedVariables; }
        }

        /// <summary>
        /// Получить список всех связанных с тестом переменных
        /// </summary>
        /// <param name="useCache">Использовать кэш для повторного чтения</param>
        /// <returns></returns>
        internal List<string> GetAllUsedVariables()
        {
            List<string> result = new List<string>();            
            foreach (var item in UsedVariables)
            {
                if (!result.Contains(item))
                    result.Add(item);
            }
            if (ParseUsedVariablesFromContent && serializedVariablesFromContent != null)
            {
                foreach (var item in serializedVariablesFromContent)
                {
                    if (!result.Contains(item) && TemplateParent.Variables.Any(v => v.Name == item))
                        result.Add(item);
                }
            }
            return result;
        }
        /// <summary>
        /// Признак составного действия - главы инструкции
        /// </summary>
        public bool IsContainer
        {
            get { return isContainer; }
            set
            {
                if (isContainer != value)
                {
                    isContainer = value;
                    if (!isContainer)
                    {
                        ClearNodes();
                    }                    
                    OnPropertyChanged("IsContainer");
                }
            }
        }

        /// <summary>
        /// Признак того, что тест может проходиться повторно
        /// </summary>
        public bool ReExecutable
        {
            get 
            {
                if (IsContainer)
                {                    
                    if (GetAllTests().Any(t => t.ReExecutable))
                        return true;
                }
                return reExecutable; 
            }
            set
            {
                if (reExecutable != value)
                {
                    reExecutable = value;                    
                    OnPropertyChanged("ReExecutable");
                    if (IsContainer)
                    {
                        foreach (var test in GetAllTests())
                        {
                            test.reExecutable = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Описание возможных причин для выполения теста повторно.
        /// </summary>
        public string ReExecuteDescription
        {
            get { return reExecuteDescription; }
            set
            {                
                if (reExecuteDescription != value)
                {
                    reExecuteDescription = value;
                    OnPropertyChanged("ReExecuteDescription");
                }
            }
        }

        private FlowDocument contentDocument = new FlowDocument() { FontFamily = new System.Windows.Media.FontFamily("Segoe UI"), LineHeight = 1 };
        /// <summary>
        /// Содержимое теста в виде FlowDocument
        /// </summary>
        public FlowDocument ContentDocument
        {
            get
            {
                EnsureDeserializeContent();
                return contentDocument;
            }
            set
            {
                if (contentDocument != value)
                {
                    contentDocument = value;                    
                    OnPropertyChanged("ContentDocument");
                }
            }
        }

        /// <summary>
        /// Свойство определяет, какие кнопки будут отображаться на тесте
        /// </summary>
        public TestButtons Buttons
        {
            get { return buttons; }
            set
            {
                buttons = value;
                OnPropertyChanged("Buttons");
            }
        }

        /// <summary>
        /// Настройки автоматического тестирования
        /// </summary>
        public AutoTestSettings AutoTestingSettings
        {
            get { return autoTestingSettings; }
        }

        /// <summary>
        /// Список идентификаторов тестов, успешное выполнение которых необходимо для выполениния данного теста
        /// </summary>
        public SerializedList<string> TestDependencesIdList
        {
            get { return testDependencesIdList; }
        }
        
        /// <summary>
        /// Id предыдущего теста
        /// </summary>
        public string PreviousTestId
        {
            get { return previousTestId; }
            set { previousTestId = value; }
        }

        /// <summary>
        /// Признак возможности содержать вложенные узлы
        /// </summary>
        public override bool CanContainsInnerNodes
        {
            get
            {
                return IsContainer;
            }
        }

        /// <summary>
        /// Родительское составное действие
        /// </summary>
        public TestTemplateItem ParentContainer
        {
            get
            {
                var parent = Parent;
                while (parent != null)
                {
                    if (parent is TestTemplateItem && (parent as TestTemplateItem).IsContainer)
                    {
                        return parent as TestTemplateItem;
                    }
                    parent = parent.Parent;
                }
                return null;
            }
        }

        /// <summary>
        /// Группы, содержащие данный тест
        /// </summary>
        public List<TestItemGroup> ParentGroups
        {
            get
            {
                List<TestItemGroup> parentGroups = new List<TestItemGroup>();
                foreach (var group in TemplateParent.TestGroups)
                {
                    if (group.ContainsTest(this))
                        parentGroups.Add(group);
                }
                return parentGroups;
            }
        }

        private bool testContentChanged = false;

        /// <summary>
        /// Признак изменений в содержимом теста
        /// </summary>
        internal bool TestContentChanged
        {
            get { return testContentChanged; }
            set
            {
                if (testContentChanged != value)
                {
                    testContentChanged = value;
                }
            }
        }        
        
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return testId;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Deserialized()
        {
            base.Deserialized();
            needDeserialize = true;
            ButtonScripts.ListDeserializationComplete += new System.Windows.Forms.MethodInvoker(ButtonScripts_ListDeserializationComplete);
        }

        private bool needDeserialize = false;
        private void EnsureDeserializeContent()
        {
            if (!needDeserialize)
                return;
            try
            {
                FlowDocumentDeserialization.ImageUriDeserialization += new ImageUriSerializationEventHandler(FlowDocumentDeserialization_ImageUriDeserialization);                
                contentDocument = serializedContent.Save();
            }
            finally
            {
                FlowDocumentDeserialization.ImageUriDeserialization -= new ImageUriSerializationEventHandler(FlowDocumentDeserialization_ImageUriDeserialization);
                needDeserialize = false;
            }
        }

        void ButtonScripts_ListDeserializationComplete()
        {
            ButtonScripts.ListDeserializationComplete -= new System.Windows.Forms.MethodInvoker(ButtonScripts_ListDeserializationComplete);            
            foreach (var script in ButtonScripts)
            {
                script.TemplateItemParent = this;
            }
        }

        /// <summary>
        /// Метод определяет, содержится ли тест с идентификатором testId во вложенных тестах, либо сам тест имеет идентификатор testId
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Contains(string testId)
        {
            if (TestId == testId)
                return true;
            foreach (var child in Nodes)
            {
                var innerTest = child as TestTemplateItem;
                if (innerTest != null)
                {
                    if (innerTest.Contains(testId))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            SerializeFlowDocumentContent();
        }

        internal void SerializeFlowDocumentContent()
        {
            if (!TestContentChanged)
            {
                if (serializedVariablesFromContent == null)
                {
                    SerializeVariablesFromContent();
                }
                return;
            }
            try
            {
                FlowDocumentSerialization.ImageUriSerialization += new ImageUriSerializationEventHandler(FlowDocumentSerialization_ImageUriSerialization);
                serializedContent.Load(ContentDocument);
                if (!ParseUsedVariablesFromContent)
                    serializedVariablesFromContent = null;
                else
                    SerializeVariablesFromContent();
            }
            finally
            {
                FlowDocumentSerialization.ImageUriSerialization -= new ImageUriSerializationEventHandler(FlowDocumentSerialization_ImageUriSerialization);
                TestContentChanged = false;
            }
        }

        private void SerializeVariablesFromContent()
        {            
            if (ParseUsedVariablesFromContent)
            {
                serializedVariablesFromContent = new SerializedList<string>();
                foreach (var item in new TextRange(ContentDocument.ContentStart, ContentDocument.ContentEnd).GetVariableNames(true))
                {
                    serializedVariablesFromContent.Add(item);
                }
            }
        }

        void FlowDocumentSerialization_ImageUriSerialization(ImageUriSerializationEventArgs e)
        {
            Uri configPathUri = new Uri(Globals.GetConfigFolder());
            e.UriString = e.UriString.Replace(configPathUri.ToString(), DATTemplate.ConfigFolderUriLink);
        }

        void FlowDocumentDeserialization_ImageUriDeserialization(ImageUriSerializationEventArgs e)
        {            
            e.UriString = e.UriString.Replace(DATTemplate.ConfigFolderUriLink, Globals.GetConfigFolder());
        }

        /// <summary>
        /// Сравнение на равенство
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is TestTemplateItem)
            {
                return TestId == (obj as TestTemplateItem).TestId;
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Удаление шаблона теста из сценария тестирования
        /// </summary>
        /// <returns></returns>
        public override bool Remove()
        {            
            //удалим ссылки на данный тест
            foreach (var test in TemplateParent.GetAllTests())
            {
                test.TestDependencesIdList.Remove(TestId);
            }

            foreach (var group in ParentGroups)
            {
                group.RemoveTestItem(TestId);
            }
            return base.Remove();
        }

        /// <summary>
        /// Клонирование
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            TestTemplateItem clone = base.Clone() as TestTemplateItem;
            clone.ContentDocument = ContentDocument.Clone();
            clone.serializedContent = new SerializedFlowDocumentContent();
            clone.previousTestId = string.Empty;
            clone.executionScript = executionScript.Clone() as ScriptInfo;            
            clone.validationScript = validationScript.Clone() as ScriptInfo;
            clone.beginTestScript = beginTestScript.Clone() as ScriptInfo;
            clone.endTestScript = endTestScript.Clone() as ScriptInfo;

            clone.beginTestScript.Name = TestId + "_BeginTestScript";
            clone.endTestScript.Name = TestId + "_EndTestScript";
            clone.executionScript.Name = TestId + "_ExecutionScript";
            clone.validationScript.Name = TestId + "_ValidationScript";

            clone.testDependencesIdList = new SerializedList<string>();
            foreach (var testId in TestDependencesIdList)
            {
                clone.TestDependencesIdList.Add(testId);
            }
            clone.ButtonScripts = new SerializedList<ScriptInfo>();
            foreach (var script in ButtonScripts)
            {
                clone.ButtonScripts.Add(script.Clone() as ScriptInfo);
            }
            clone.contentPresenters = new SerializedList<ContentPresenterTag>();
            foreach (var presenter in contentPresenters)
            {
                clone.ContentPresenters.Add(presenter.Clone() as ContentPresenterTag);
            }
            clone.probableCorrections = new SerializedList<string>();
            foreach (var correction in probableCorrections)
            {
                clone.ProbableCorrections.Add(correction);
            }

            clone.usedVariables = new SerializedList<string>();
            foreach (var variable in UsedVariables)
            {
                clone.UsedVariables.Add(variable);
            }            

            var allTests = TemplateParent.GetAllTests();
            int i = 1;
            string newTestId = TestId + "_" + i;
            while ((from test in allTests where test.TestId == newTestId select test).Any())
            {
                i++;
                newTestId = TestId + "_" + i;
            }
            clone.TestId = newTestId;
            return clone;
        }

        #region NotifyModified
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        protected override void OnPropertyChanged(string info)
        {
            base.OnPropertyChanged(info);
            NotifyModified();
        }

        internal void NotifyModified()
        {
            if (TemplateParent != null)
            {
                TemplateParent.OnModified();
            }
        }

        #endregion
        #endregion
    } 

    /// <summary>
    /// Варианты кнопок на тесте
    /// </summary>
    public enum TestButtons
    { 
        Execute,
        ExecuteCorrect,
        YesNo,
        YesCorrect,
        YesNoCorrect
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class TestInfoVariableHelper
    {
        private static DATVariableDescriptor GenerateVariable(TestTemplateItem testTemplateItem, string propertyName, DATVariableType type)
        {
            return new DATVariableDescriptor(testTemplateItem.TestId + "_" + propertyName, type) { IsReadonly = true, Description = testTemplateItem.Name };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testTemplateItem"></param>
        /// <returns></returns>
        public static List<DATVariableDescriptor> GetTestInfoVariables(TestTemplateItem testTemplateItem)
        {
            List<DATVariableDescriptor> result = new List<DATVariableDescriptor>();
            if (testTemplateItem == null) 
                return result;
            result.Add(GenerateVariable(testTemplateItem, "Finished", DATVariableType.Boolean));
            result.Add(GenerateVariable(testTemplateItem, "HasErrors", DATVariableType.Boolean));
            result.Add(GenerateVariable(testTemplateItem, "WasCorrected", DATVariableType.Boolean));
            result.Add(GenerateVariable(testTemplateItem, "Comments", DATVariableType.String));
            return result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public static List<DATVariable> GetTestInfoVariables(TestObject test)
        {
            List<DATVariable> result = new List<DATVariable>();
            bool finished = test.Finished;
            if (test.TemplateItem.ReExecutable)
            {
                finished = test.Finished && test.FinishingIsConfirmed;
            }
            result.Add(new DATVariable(GenerateVariable(test.TemplateItem, "Finished", DATVariableType.Boolean)) { VariableValue = finished });
            result.Add(new DATVariable(GenerateVariable(test.TemplateItem, "HasErrors", DATVariableType.Boolean)) { VariableValue = test.HasErrors });
            result.Add(new DATVariable(GenerateVariable(test.TemplateItem, "WasCorrected", DATVariableType.Boolean)) { VariableValue = test.WasCorrected });
            result.Add(new DATVariable(GenerateVariable(test.TemplateItem, "Comments", DATVariableType.String)) { VariableValue = test.Comments });
            return result;
        }
    }    
}
