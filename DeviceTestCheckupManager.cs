//
//    Copyright (c) NeuroSoft 
//
//    Модуль содержит описание класса Диспетчера, наследуемого от CheckupManager.
//
//    Необходимо переименовать класс и переопределить при необходимости указанные ниже методы.  
//
//

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.Prototype.DataModel;
using NeuroSoft.Prototype.Database;
using NeuroSoft.WPFPrototype.Interface;
using System.Windows.Controls;
using System.Windows;
using AvalonDock;
using System.Windows.Input;
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.WPFComponents.AvalonDockHelper;
using System.Windows.Media;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Linq;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using System.Globalization;
using System.Windows.Threading;
using NeuroSoft.WPFComponents.ScalableWindows;
using System.ComponentModel;
using NeuroSoft.DeviceAutoTest.Common.Controls;
#endregion

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Класс Диспетчера с которым работает приложение.
    /// Необходимо переименовать класс в соответствии с названием приложения.
    /// Методы, которые необходимо переопределить в первую очередь, показаны ниже.
    /// </summary>
    public class DeviceTestCheckupManager : WPFMainCheckupManager, INotifyPropertyChanged
    {
        #region Constructors and Destructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="model">модель используемая приложением</param>
        /// <param name="checkupData">хранимая дата</param>
        /// <param name="checkupInfo">информация об обследовании</param>
        public DeviceTestCheckupManager(PrototypeModel model, CheckupData checkupData, CheckupInfo checkupInfo)
            : base(model, checkupData, checkupInfo)
        {
        }

        #endregion

        private ResourceDictionary localVectorImages = new ResourceDictionary();
        internal ResourceDictionary LocalVectorImages
        {
            get
            {                
                if (localVectorImages.Source == null)
                    localVectorImages.Source = new Uri("pack://application:,,,/NeuroSoft.DeviceAutoTest;Component/Resources/VectorImages.xaml", UriKind.RelativeOrAbsolute);
                return localVectorImages;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DeviceTestCheckupAncestor TestsAncestor
        {
            get
            {
                return CheckupAncestor as DeviceTestCheckupAncestor;
            }
        }

        private TestItemGroup autoTestGroup;
        /// <summary>
        /// Группа тестов для автоматической проверки
        /// </summary>
        public TestItemGroup AutoTestGroup
        {
            get { return autoTestGroup; }
            set
            {
                if (autoTestGroup != value)
                {
                    autoTestGroup = value;
                    OnPropertyChanged("AutoTestGroup");
                    CurrentAutoTestManager = value != null ? new AutoTestManager(this, value) : null;
                }
            }
        }

        private AutoTestManager currentAutoTestManager;
        /// <summary>
        /// Менеджер автоматического тестирования
        /// </summary>
        public AutoTestManager CurrentAutoTestManager
        {
            get
            {
                if (currentAutoTestManager == null && AutoTestGroup != null)
                {
                    currentAutoTestManager = new AutoTestManager(this, AutoTestGroup);
                }
                return currentAutoTestManager;
            }
            private set
            {
                if (currentAutoTestManager != value)
                {
                    currentAutoTestManager = value;
                    OnPropertyChanged("CurrentAutoTestManager");
                }
            }
        }

        
        /// <summary>
        /// Признак нахождения в режиме автоматического тестирования
        /// </summary>
        public bool AutoTestingIsActive
        {
            get
            {
                return CurrentAutoTestManager != null && CurrentAutoTestManager.CanStop;
            }
        }
        
        #region Методы

        /// <summary>
        /// Создает новый экземпляр Хранителя, с которым работает Диспетчер.
        /// Указать настоящий класс Хранителя. Описание класса содержится в файле AppCheckupAncestor.cs
        /// </summary>
        /// <returns>экземпляр Хранителя</returns>
        protected override CheckupData CreateCheckupData()
        {            
            return new DeviceTestCheckupAncestor(CheckupInfo, CreateCheckupDataStorage());
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Created()
        {
            base.Created();
            
            //для идентификации обследования
            CheckupInfo.Format = "NeuroSoft.DeviceAutoTest";

            //строка отображаемая в журнале в столбце названия обследования. 
            //CheckupInfo.Description = CheckupInfo.PacientInfo.FirstName + " (" + CheckupInfo.PacientInfo.LastName + ")";
            //Сохраним созданное обследование
            Save();
        }
        
        /// <summary>
        /// Возвращает объект описания дерева для обследования.
        /// Обязательно вызвать наследуемый метод.
        /// Возвращаемый объект содержит корневой узел с данными пациента и список протоколов. 
        /// </summary>
        /// <returns></returns>
        public override TreeObject CreateTreeObject()
        {
            TreeObject to = new TreeObjectFolder(TreeObjectNames.Checkup, GetDescription());            
            to.Add(GenerateTestsFolder());            
            protocols = new TreeObjectFolder(TreeObjectNames.Protocols, NeuroSoft.Prototype.Interface.Strings.str_Protocols);
            protocols.ImageKey = TreeObjectNames.Protocols;
            protocols.SelectedImageKey = TreeObjectNames.Protocols;
            to.Add(protocols);
            foreach (ProtocolCheckupData p in CheckupAncestor.Protocols)
            {
                if (p.ProtocolType == ProtoProtocolType.BuiltIn)
                {
                    TreeObject protocol = new TreeObject(TreeObjectNames.BuiltInProtocol, p.ProtocolName, (ImageSource)VectorImages["DI_Protocol"], TreeObjectNames.BuiltInProtocol);
                    protocols.Add(protocol);
                }
                else
                {
                    TreeObject protocol = new TreeObject(TreeObjectNames.WordProtocol, p.ProtocolName, (ImageSource)VectorImages["DI_Protocol_Word"], TreeObjectNames.WordProtocol);
                    protocols.Add(protocol);
                }
            }
            return to;
        }

        private TreeObjectFolder GenerateTestsFolder()
        {            
            DeviceTestCheckupAncestor ancestor = TestsAncestor;
            TreeObjectFolder testsRoot = new TreeObjectFolder(TestsTreeObjectNames.DeviceTestRoot, string.Format(Properties.Resources.TestsTreeObjectHeader, ancestor.TestTemplate.VersionString));
            foreach (var node in ancestor.TestTemplate.Nodes)
            {
                testsRoot.Add(GenerateTreeObject(node));
            }
            return testsRoot;        
        }

        private TreeObject GenerateTreeObject(DATTemplateTreeViewItem item)
        {
            TreeObject result = null;
            if (item is TestTemplateItem)
            {
                result = new TestTreeObject(TestsTreeObjectNames.DeviceTest, item.Name, (item as TestTemplateItem).TestId, TestsTreeObjectNames.DeviceTest);
            }
            else
            {
                result = new TreeObjectFolder(TestsTreeObjectNames.TestsFolderName, item.Name);                
            }

            foreach (var node in item.Nodes)
            {
                result.Add(GenerateTreeObject(node));
            }
            return result;
        }


        /// <summary>
        /// Создает и возвращает окно относящееся к узлу дерева в Инспекторе обследований.
        /// Необходимо переопределить у наследников.
        /// </summary>
        /// <param name="treeObject">ссылка на элемент дерева.</param>
        public override IChildForm CreateContent(TreeObject treeObject)
        {
            switch (treeObject.Name)
            {
                case TestsTreeObjectNames.DeviceTest:
                    DeviceTestCheckupAncestor ancestor = TestsAncestor;
                    TestObject test = ancestor.GetTestById(((TestTreeObject)treeObject).TestId);
                    if (test == null)
                    {
                        return null;
                    }
                    TestDockableContent content = new TestDockableContent(new TestContentViewModel(test), this);
                    content.Caption = treeObject.Text;
                    return content;
                default: return base.CreateContent(treeObject);
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeObject"></param>
        public override void TreeObjectSelected(TreeObject treeObject)
        {            
            base.TreeObjectSelected(treeObject);            
            if (Closing || treeObject.CheckupManager == null || treeObject.Name != TestsTreeObjectNames.DeviceTest || MainWindow.DockingManager.ActiveContent == treeObject.Form)
                return;
            TestObject test = TestsAncestor.GetTestById((treeObject as TestTreeObject).TestId);
            if (test == null || test.TestOpened || test.IsLoading)
                return;
            DummyChildContent dummy = null;
            if (treeObject.Form == null && treeObject.Name != TreeObjectNames.Protocols
                && treeObject.Name != TestsTreeObjectNames.DeviceTestRoot
                && treeObject.Name != TestsTreeObjectNames.TestsFolderName)
            {
                dummy = new DummyChildContent(treeObject, treeObject.Text);
                ShowContent(dummy);
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                if (treeObject.CheckupManager == null || (treeObject as TestTreeObject).InspectorItem == null || !(treeObject as TestTreeObject).InspectorItem.IsCurrent)
                {                   
                    return;
                }
                treeObject.Open();
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeObject"></param>
        public override void BeforeOpenTreeObject(TreeObject treeObject)
        {
            base.BeforeOpenTreeObject(treeObject);
            OnBeforeOpenTreeObject(treeObject);
        }

        private void OnBeforeOpenTreeObject(TreeObject treeObject)
        {       
            foreach (var item in MainWindow.DockingManager.DockableContents)
            {
                if (!(item is InspectorContent)
                    && (!(item is DummyChildContent) || (item as DummyChildContent).TreeObject != treeObject)
                    && !(item is GlossaryContent)
                    && (treeObject == null || item != treeObject.Form)
                    && !(item is UsbDevicesListControl))        // Добавлено, чтобы не закрывался контрол с подключенными девайсами при открытии обследований
                {
                    if (item is TestHistoryDockableContent)
                    {
                        item.Close();
                    }
                    else
                    {
                        item.Hide();
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeObject"></param>
        public override void TreeObjectDoubleClick(TreeObject treeObject)
        {
            base.TreeObjectDoubleClick(treeObject);
            if (treeObject != null && !AutoTestingIsActive && treeObject.Form is TestDockableContent)
            {
                var historyContent = (treeObject.Form as TestDockableContent).HistoryContent;
                if (!MainWindow.DockingManager.MainDocumentPane.Items.Contains(historyContent))
                    MainWindow.DockingManager.MainDocumentPane.Items.Add(historyContent);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override System.Windows.Forms.ImageList GetImageList()
        {
            var imageList = base.GetImageList();
            if (!imageList.Images.ContainsKey(TestsTreeObjectNames.DeviceTest))
            {
                imageList.Images.Add(TestsTreeObjectNames.DeviceTest, NeuroSoft.WPFPrototype.Interface.Common.Converters.ToWinFormsImage((ImageSource)LocalVectorImages["DI_TestItem"]));
            }
            return imageList;
        }
        /// <summary>
        /// Создание элемента в дереве инспектора обследований.
        /// </summary>
        /// <param name="parent">Родительский узел</param>
        /// <param name="treeObject">Объект описывающий узел (модель узла)</param>
        /// <returns></returns>
        public override InspectorTreeViewItem CreateInspectorTreeViewItem(InspectorTreeViewItem parent, TreeObject treeObject)
        {
            InspectorTreeViewItem item = null;
            if (treeObject is TestTreeObject)
            {
                DeviceTestCheckupAncestor ancestor = TestsAncestor;
                item = new TestTreeViewItem(parent, treeObject, ancestor.GetTestById(((TestTreeObject)treeObject).TestId));
                (treeObject as TestTreeObject).InspectorItem = item;
            }
            else
            {
                item = base.CreateInspectorTreeViewItem(parent, treeObject);
            }
            if (treeObject is TreeObjectFolder)
            {
                item.IsExpanded = true;
            }            
            return item;            
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetDescription()
        {
            if (!string.IsNullOrEmpty(CheckupInfo.Description))
                return CheckupInfo.Description;
            else
                return base.GetDescription();
        }        
        
        /// <summary>
        /// Возвращает локльное меню для элемента дерева
        /// </summary>
        public override object GetContextMenu(TreeObject treeObject)
        {
            return null;
        }

        /// <summary>
        /// Блоки для протокола.
        /// </summary>
        /// <returns></returns>
        public new static List<PatternBlock> GetProtocolBlocks()
        {
            List<PatternBlock> blocks = new List<PatternBlock>();
            // Блок IF
            IfThenElsePatternBlock ifThenElsePatternBlock = new IfThenElsePatternBlock()
            {
                GroupName = Properties.Resources.CommonPPMBlocks,
                Title = Properties.Resources.IfThenElseBlock,
                Name = typeof(IfThenElsePatternBlock).Name,

                // Допустимы любые блоки
                AvailableAllBlocks = true
            };
            blocks.Add(ifThenElsePatternBlock);

            // Блок FOR
            ForCyclePatternBlock forCyclePatternBlock = new ForCyclePatternBlock()
            {
                GroupName = Properties.Resources.CommonPPMBlocks,
                Title = Properties.Resources.CycleBlock,
                Name = typeof(ForCyclePatternBlock).Name,

                // Допустимы любые блоки
                AvailableAllBlocks = true
            };
            blocks.Add(forCyclePatternBlock);

            blocks.AddRange(WPFMainCheckupManager.GetProtocolBlocks());
            return blocks;
        }

        private const string correctionsAndErrorsTagStr = "$CorrectionsAndErrors";
        private const string correctionsAndErrorsTestNamesTagStr = "$CorrectionsAndErrorsTestNames";
        private const string templateVersionTagStr = "$TemplateVersion";
        private const string deviceSNTagStr = "$DeviceSN";
        private const string currentDateTagStr = "$CurrentDate";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public override Dictionary<string, PatternTagValue> GetTagValues(PatternBlockWithNestings block)
        {            
            Dictionary<string, PatternTagValue> result = new Dictionary<string, PatternTagValue>();            
            List<DATVariable> variables = new List<DATVariable>(TestsAncestor.Variables);
            Dictionary<string, List<TestObject>> testsForVariables = new Dictionary<string, List<TestObject>>(); //словарь: переменная-список тестов, использующих данную переменную
            foreach (var test in TestsAncestor.Tests)
            {
                variables.AddRange(TestInfoVariableHelper.GetTestInfoVariables(test));
                foreach (var usedVariable in test.TemplateItem.GetAllUsedVariables())
                {
                    if (!testsForVariables.ContainsKey(usedVariable))
                        testsForVariables.Add(usedVariable, new List<TestObject>());
                    if (!testsForVariables[usedVariable].Contains(test))
                        testsForVariables[usedVariable].Add(test);
                }
            }
            foreach (var variable in variables)
            {
                //ProtocolPattern.DoubleSignificantDigitsCount = 3;
                object stringValue = null;
                //Если есть тесты, использующие переменную, то не будем отображать значение переменной в случае, если тест ещё не выполнен
                if (testsForVariables.ContainsKey(variable.TestVariableID)) 
                {
                    foreach (var test in testsForVariables[variable.TestVariableID])
                    {
                        if (!test.Finished || (test.TemplateItem.ReExecutable && !test.FinishingIsConfirmed))
                        {
                            if (variable.VariableDescriptor.IsArray)
                                stringValue = new string[0];
                            else
                                stringValue = "";
                            break;
                        }
                    }                    
                }
                if (stringValue == null)
                {
                    if (variable.TestVariableType == DATVariableType.Double)
                    {
                        string format = "{0:0";
                        if (variable.DoubleValueDecimalPlaces > 0)
                        {
                            format += ".";
                            for (int i = 0; i < variable.DoubleValueDecimalPlaces; i++)
                            {
                                format += "#";
                            }
                        }
                        format += "}";
                        stringValue = string.Format(format, variable.VariableValue);
                    }
                }
                result.Add("$" + variable.TestVariableID, new PatternTagValue(variable.VariableValue, stringValue));
            }
            List<string> correctionsAndErrors = new List<string>();
            List<string> correctionsAndErrorsTestNames = new List<string>();
            foreach (var test in TestsAncestor.Tests)
            {
                if (test.TemplateItem.IsContainer || string.IsNullOrWhiteSpace(test.Comments))
                    continue;
                if (test.HasErrors || test.WasCorrected)
                {
                    correctionsAndErrorsTestNames.Add(test.TemplateItem.Name);
                    correctionsAndErrors.Add(test.Comments);
                }
            }
            result.Add(correctionsAndErrorsTestNamesTagStr, new PatternTagValue(correctionsAndErrorsTestNames.ToArray()));
            result.Add(correctionsAndErrorsTagStr, new PatternTagValue(correctionsAndErrors.ToArray()));
            result.Add(templateVersionTagStr, new PatternTagValue(TestsAncestor.TestTemplate.VersionString));
            result.Add(currentDateTagStr, new PatternTagValue(DateTime.Now.ToString("dd.MM.yyyy")));
            result.Add(deviceSNTagStr, new PatternTagValue(TestsAncestor.DeviceInfo.SerialNumber));
            return result;
        }

        /// <summary>
        /// Список описателей тегов для шаблонов протоколов текущей инструкции по наладке
        /// </summary>
        /// <returns></returns>
        public static List<PatternTagDescription> GetDATTagDescriptions()
        {
            List<PatternTagDescription> result = new List<PatternTagDescription>();
            result.Add(new PatternTagDescription(templateVersionTagStr, Properties.Resources.TemplateVersionTagDescription, TagValueTypeEnum.String));
            result.Add(new PatternTagDescription(deviceSNTagStr, Properties.Resources.DeviceSNTagDescription, TagValueTypeEnum.String));
            result.Add(new PatternTagDescription(currentDateTagStr, Properties.Resources.CurrentDateTagDescription, TagValueTypeEnum.String));

            if (DATTemplate.CurrentTemplate != null)
            {                
                foreach (var variable in DATTemplate.CurrentTemplate.Variables.Union(DATTemplate.CurrentTemplate.TestInfoVariables))
                {
                    if (variable.TagInsertMode != TagInsertMode.OnlyScript || variable.IsArray)
                    {
                        result.Add(variable.GeneratePatternTag());
                    }
                }
            }
            result.Add(new PatternTagDescription(correctionsAndErrorsTestNamesTagStr, Properties.Resources.CorrectionsAndErrorsTestNamesTagDescription, TagValueTypeEnum.StringArray));
            result.Add(new PatternTagDescription(correctionsAndErrorsTagStr, Properties.Resources.CorrectionsAndErrorsTagDescription, TagValueTypeEnum.StringArray));            
            return result;
        }


        internal static void NSHIDDevices_ReceivePad(uint nsPadState, uint nsPadChange, int syncValue)
        {
            if (App.Current == null || App.Current.MainWindow == null)
                return; // поздновато пришло...
            ((MainWindow)App.Current.MainWindow).NSHIDDevices_ReceivePad(nsPadState, nsPadChange, syncValue);
            CommandManager.InvalidateRequerySuggested();
        }


        /// <summary>
        /// Выполнение действия над тестом
        /// </summary>
        /// <param name="test"></param>
        /// <param name="action"></param>
        /// <param name="comments"></param>
        internal void DoTestAction(TestObject test, TestActionType action, string comments = null, bool autoCompleteTest = false)
        {
            if (test.TemplateItem == null)
                return;
            //Проверим не выполнен ли тест в ручном режиме в процессе автотестирования
            if (!autoCompleteTest && AutoTestingIsActive && test.TemplateItem.AutoTestingSettings.Manual &&
                (action == TestActionType.Execute || action == TestActionType.Success || action == TestActionType.HasErrors))
            {
                if (CurrentAutoTestManager.CurrentTest == test)
                {
                    TestsAncestor.ScriptExecutionEnvironment.DoAutoTest(Converters.ToAutoTestAction(action));
                }
                return;
            }
            //Если действие составное, то перейдем к первому невыполненному вложенному действию
            //Если же все вложенные действия выполнены успешно, то отметим составное действие как выполненное
            if (test.TemplateItem.IsContainer)
            {
                if (action != TestActionType.Abort)
                {
                    List<TestObject> innerTests = test.Children;
                    var NotFinishedTest = (from t in innerTests where !t.Finished select t).FirstOrDefault();
                    if (NotFinishedTest != null)
                    {
                        OpenTest(NotFinishedTest);
                        return;
                    }

                    //Если в дочерних действиях были внесены исправления, отметим это и у родительского действия
                    if ((from t in test.Children where t.WasCorrected select t).FirstOrDefault() != null)
                    {
                        test.WasCorrected = true;
                    }
                }
            }
            if (action != TestActionType.Abort && action != TestActionType.ConfirmFinish && action != TestActionType.ReTest && !test.InternalCanExecute)
                return;

            bool corrected = false;
            bool alreadyFinished = test.Finished;
            bool needInvalidateCanExecute = true;
            if (action == TestActionType.Execute)
            {
                bool? result = (bool?)test.ExecuteInternal(test.TemplateItem.ExecutionScript);
                if (result.HasValue)
                {
                    test.Finished = true;
                    if (result == true)
                    {
                        test.HasErrors = false;
                    }
                    else if (result == false)
                    {
                        test.HasErrors = true;
                    }
                }
                else
                {
                    needInvalidateCanExecute = false;
                    action = TestActionType.None;
                    if (TestsAncestor.ScriptExecutionEnvironment.TestWasCorrected)
                    {
                        corrected = true;
                    }
                }
            }
            else if (action == TestActionType.Success)
            {
                test.Finished = true;
                test.HasErrors = false;
            }
            else if (action == TestActionType.HasErrors)
            {
                test.Finished = true;
                test.HasErrors = true;
            }
            else if (action == TestActionType.Correct)
            {
                test.WasCorrected = true;
                if (test.TemplateItem.Buttons != TestButtons.ExecuteCorrect)
                {
                    test.Finished = true;
                }
                corrected = true;
            }
            else if (action == TestActionType.Abort)
            {
                test.Abort();
            }
            else if (action == TestActionType.ReTest)
            {
                test.Reset(null, true, false, true);
            }
            else if (action == TestActionType.ConfirmFinish)
            {
                test.ConfirmFinish();
            }

            if (corrected)
            {
                test.Reset();
            }
            //Сделаем снимок тестирования и сохраним историю
            TestsAncestor.DoSnapshot(test, action, comments);
            if (needInvalidateCanExecute)
            {
                test.InvalidateCanExecute();
                //Обновим значения свойства CanExecute всех тестов
                //TestsAncestor.InvalidateCanExecuteTests();
            }
            //Сохранение
            TestsAncestor.Modified = true;
            if (((MainWindow as MainWindow).Model as MainModel).AutoSave && !TestsAncestor.ScriptExecutionEnvironment.IsAutoTesting)
            {
                Save();
            }

            bool justFinished = !alreadyFinished && test.Finished;
            if (action == TestActionType.Abort)
            {
                test.BeginTest();
            }
            else if (justFinished)
            {
                test.InvalidateCanExecute();
                test.EndTest(true);
            }

            //Запустим при необходимости следующий тест            
            if (justFinished)
            {
                var parentContainer = test.ParentContainer;
                if (parentContainer != null)
                {
                    parentContainer.InvalidateCanExecute();
                }
                if (parentContainer != null && parentContainer.InternalCanExecute)
                {
                    bool childrenHasErrors = (from t in parentContainer.Children where t.HasErrors select t).FirstOrDefault() != null;
                    DoTestAction(parentContainer, childrenHasErrors ? TestActionType.HasErrors : TestActionType.Success);
                }
                if (!AutoTestingIsActive && !test.TemplateItem.IsContainer)
                {
                    OpenTest(test.NextNotFinishedTest);
                }
            }
            else if (test.FinishingIsConfirmed && !AutoTestingIsActive && !test.TemplateItem.IsContainer)
            {
                OpenTest(test.NextNotFinishedTest);
            }
        }
        
        
        internal void OpenTest(TestObject test)
        {
            if (test != null)
            {
                var treeObject = TestTreeObject.FindTestTreeObject(TreeObject, test.TestId);
                if (treeObject != null)
                {
                    treeObject.Open();
                }
            }
        }

        internal IChildForm GetTestForm(TestObject test)
        {
            if (test != null)
            {
                var treeObject = TestTreeObject.FindTestTreeObject(TreeObject, test.TestId);
                if (treeObject != null)
                {
                    return treeObject.Form;
                }
            }
            return null;
        }

        private bool Closing = false;
        /// <summary>
        /// Закрывает обследование
        /// </summary>
        public override void Close()
        {
            Closing = true;
            OnBeforeOpenTreeObject(null);
            base.Close();
            TestsAncestor.ScriptExecutionEnvironment.Dispose();
            DATTemplate.CurrentTemplate = null;           
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Open()
        {
            base.Open();
            if ((MainWindow as MainWindow).AutoTestingToolBar != null)
                (MainWindow as MainWindow).AutoTestingToolBar.Visibility = Visibility.Visible;
        }
               
        #endregion

        #region INotifyPropertyChanged
        /// <summary>
        /// Событие на изменение свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Уведомление об изменении свойства (все объекты представления привязаные к этому свойству автоматически обновят себя)
        /// </summary>
        /// <param name="propertyName">Имя свойства принимающего новое значение</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public static class TestsTreeObjectNames
    {
        /// <summary>
        /// Имя корня TreeObject
        /// </summary>
        public const string DeviceTestRoot = "DeviceTestRoot";
        /// <summary>
        /// Папка с тестами
        /// </summary>
        public const string TestsFolderName = "DeviceTestsFolder";
        /// <summary>
        /// Тест
        /// </summary>
        public const string DeviceTest = "DeviceTest";
    }

}
