//
//    Copyright (c) NeuroSoft 
//
//    Модуль содержит описание класса Хранителя для приложения. 
//
//    Необходимо переименовать класс и переопределить некоторые методы
//


#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.Prototype.DataModel;
using NeuroSoft.Prototype.Database;
using NeuroSoft.Common;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NeuroSoft.DeviceAutoTest.TestTemplateEditor;
using System.Windows;
using NeuroSoft.WPFComponents.ScalableWindows;

#endregion

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Класс объекта тестирования устройства.
    /// </summary>
    [Serializable]
    [Anamnesis(false)]
    [Conclusion(false)]
    public class DeviceTestCheckupAncestor : BaseCheckupAncestor
    {
        #region Constructors
        /// <summary>
        /// Стандартный конструктор используемый при сериализации.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DeviceTestCheckupAncestor(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //try
            //{
            //    testTemplate = info.GetValue("testTemplate", typeof(DATTemplate)) as DATTemplate;
                
            //    testTemplate.Nodes.ListDeserializationComplete += new System.Windows.Forms.MethodInvoker(Nodes_ListDeserializationComplete);
            //}
            //catch (SerializationException) { }//скорее всего шаблон не был сериализован, что нормально
        }
        
        /// <summary>
        /// Конструктор.
        /// </summary>
        public DeviceTestCheckupAncestor(CheckupInfo checkupInfo, CheckupDataStorage storage)
            : base(checkupInfo, storage)
        {
            //Будем считать, что пациент - процесс наладки устройства, его имя - серийный номер устройства, фамилия - тип устройства, страховой полис - guid инструкции по наладке,
            //Рост - версия инструкции
            //биологический вид - "DeviceAutoTest"
            //Пол - необходимость проведения только повторной проверки (0 - полная наладка, 1 - только повторная проверка)
            DeviceInfo = new DeviceInfo(checkupInfo.PacientInfo.LastName, checkupInfo.PacientInfo.FirstName);
            DATTemplateDescriptor templateDescriptor = DATTemplate.FindDescriptor(new Guid(checkupInfo.PacientInfo.Polis), Convert.ToInt32(checkupInfo.PacientInfo.Height));            
            InitTestTemplate(templateDescriptor);
            checkupInfo.WriteParam(TemplateKeyParam, TestTemplateKey);
            editUsers = new TestingUserInfoList() { new TestingUserInfo(Globals.UserName) };
            DoSnapshot(null, TestActionType.Init, string.Empty);
            Save(storage);
        }
        #endregion 

        #region Fields And Properties     
        private TestingUserInfoList editUsers;
        /// <summary>
        /// Информация о наладчиках, причастных к данному процессу наладки
        /// </summary>
        public TestingUserInfoList EditUsers 
        {
            get
            {
                if (editUsers == null)
                {
                    var editUsersStr = CheckupInfo.ReadParam(EditUsersInfoParam, null) as string;
                    if (editUsersStr != null)
                    {
                        editUsers = new TestingUserInfoList(editUsersStr);
                    }
                    else if (editUsers == null) //открыто старое обследование, необходимо собрать статистику по пользователям
                    {
                        editUsers = new TestingUserInfoList();
                        foreach (var snapshot in Snapshots)
                        {
                            var user = editUsers.GetUser(snapshot.User);
                            if (user == null)
                            {
                                user = new TestingUserInfo(snapshot.User);
                                editUsers.Add(user);
                            }
                            if (snapshot.ExecutionTime != null)
                            {
                                user.TestingTime += snapshot.ExecutionTime.Value.Ticks;
                            }
                        }
                    }
                }
                return editUsers;
            }
        }

        private TestingUserInfo currentUserInfo = null;
        /// <summary>
        /// Информация о текущем наладчике
        /// </summary>
        internal TestingUserInfo CurrentUserInfo
        {
            get
            {
                if (currentUserInfo == null)
                {
                    foreach (var userInfo in EditUsers)
                    {
                        if (userInfo.UserName == Globals.UserName)
                        {
                            currentUserInfo = userInfo;
                            break;
                        }
                    }
                    if (currentUserInfo == null)
                    {
                        currentUserInfo = new TestingUserInfo(Globals.UserName);
                        EditUsers.Add(currentUserInfo);
                    }
                }
                return currentUserInfo;
            }
        }
        private DATTemplate testTemplate;
        [Serialize]
        private SerializedList<TestObject> tests = new SerializedList<TestObject>();
        [Serialize]
        private SerializedList<DATVariable> variables = new SerializedList<DATVariable>();
        [Serialize]
        private SerializedList<ProtocolPatternDescriptor> protocolPatterns = new SerializedList<ProtocolPatternDescriptor>();        
        [Serialize]
        private DeviceInfo deviceInfo;
        [Serialize]
        private SerializedList<TestingSnapshot> snapshots = new SerializedList<TestingSnapshot>();
        [Serialize]
        private string testTemplateKey;
        internal const string TemplateKeyParam = "TemplateKey";
        internal const string EditUsersInfoParam = "EditUsersInfo";        
        internal const string LastEditDateParam = "LastEditDate";
        internal const string IsFinishedTesingParam = "TestingIsFinished";

        private ScriptEnvironment scriptExecutionEnvironment;

        /// <summary>
        /// Временное окружение скрипта при выполнении
        /// </summary>
        public ScriptEnvironment ScriptExecutionEnvironment
        {
            get 
            {
                if (scriptExecutionEnvironment == null)
                {
                    scriptExecutionEnvironment = new ScriptEnvironment(DeviceInfo.SerialNumber, System.Diagnostics.Debugger.IsAttached); // Изменено определение режима запуска под отладкой
                }
                return scriptExecutionEnvironment; 
            }
        }

        private DeviceTestCheckupManager CurrentMainCheckupManager
        {
            get
            {
                return MainWindow.CurrentMainWindow.CurrentMainCheckupManager as DeviceTestCheckupManager;
            }
        }        

        /// <summary>
        /// Список доступных групп тестов для автоматической проверки
        /// </summary>
        public List<TestItemGroup> AutoTestGroups
        {
            get
            { 
                List<TestItemGroup> result = new List<TestItemGroup>();
                foreach (var group in TestTemplate.TestGroups)
	            {
                    if (group.IsAutoTestingGroup)
                        result.Add(group);
	            }
                return result; 
            }
        }
        
        /// <summary>
        /// "Биологический вид" пациента-теста.
        /// </summary>
        public const string TestSpecies = "DeviceAutoTest";

        /// <summary>
        /// Информация об устройстве, для которого создан сценарий.
        /// </summary>
        public DeviceInfo DeviceInfo
        {
            get { return deviceInfo; }
            private set { deviceInfo = value; }
        }
        /// <summary>
        /// Список тестов сценария.
        /// </summary>
        public SerializedList<TestObject> Tests
        {
            get { return tests; }
        }

        /// <summary>
        /// Значения переменных на текущий момент
        /// </summary>
        public SerializedList<DATVariable> Variables
        {
            get { return variables; }            
        }

        /// <summary>
        /// Ключ для доступа к инструкции в базе
        /// </summary>
        public string TestTemplateKey
        {
            get { return testTemplateKey; }
            private set { testTemplateKey = value; }
        }        

        /// <summary>
        /// Сценарий тестирования, на основе которого тестируется прибор
        /// </summary>
        public DATTemplate TestTemplate
        {
            get { return testTemplate; }         
        }        

        private void InitTestTemplate(DATTemplateDescriptor templateDescriptor)
        {
            DATTemplate template = null;
            long lastEditDateDB = templateDescriptor.GetDBLastEditDate(Globals.CurrentConnection.Connection);
            bool loadedFromDB = false;
            //Проверим, содержится ли в базе последняя версия инструкции. Если да, то будем использовать её.
            if (lastEditDateDB > 0 && lastEditDateDB >= templateDescriptor.LastEditDateTicks)
            {
                template = templateDescriptor.LoadTemplateFromDB(Globals.CurrentConnection.Connection);
                loadedFromDB = (template != null);
            }
            if (!loadedFromDB)
            {                
                template = templateDescriptor.GetTestTemplate();
            }
            testTemplate = template;
            testTemplate.IsUsed = true;
            TestTemplateKey = DATTemplate.GetDBKey(templateDescriptor);
            if (!loadedFromDB)
            {
                SaveTestTemplate(testTemplate);
            }
            ParseDATTemplate(testTemplate);
            DATTemplate.CurrentTemplate = testTemplate;
        }

        internal void SaveTestTemplate(DATTemplate template)
        {
            template.SaveInDatabase(Globals.CurrentConnection.Connection);            
        }

        /// <summary>
        /// Снимки состояний процесса тестирования
        /// </summary>
        public SerializedList<TestingSnapshot> Snapshots
        {
            get { return snapshots; }
        }


        /// <summary>
        /// Список описателей шаблонов для данного сценария
        /// </summary>
        public SerializedList<ProtocolPatternDescriptor> ProtocolPatterns
        {
            get { return protocolPatterns; }
        }
        #endregion

        #region Methods        

        /// <summary>
        /// Сделать снимок тестирования
        /// </summary>
        /// <param name="test"></param>
        /// <param name="action"></param>
        /// <param name="comments"></param>
        internal void DoSnapshot(TestObject test, TestActionType action, string comments)
        {
            var snapshot = new TestingSnapshot(this, test, action, comments);
            var currentUserTestInfo = CurrentUserInfo;
            if (snapshot.ExecutionTime != null)
            {
                currentUserInfo.TestingTime += snapshot.ExecutionTime.Value.Ticks;
            }
            Snapshots.Add(snapshot);
            if (CurrentMainCheckupManager != null)
            {
                var form = CurrentMainCheckupManager.GetTestForm(test) as TestDockableContent;
                if (form != null && form.HistoryContent != null)
                {
                    form.HistoryContent.InvalidateHistory();
                }
            }
        }

        /// <summary>
        /// Создание набора тестов на основе сценария тестирования
        /// </summary>
        /// <param name="template"></param>
        private void ParseDATTemplate(DATTemplate template)
        {
            tests = new SerializedList<TestObject>();
            List<TestTemplateItem> testTemplateItems = template.GetAllTests();
            foreach (var item in testTemplateItems)
            {
                tests.Add(new TestObject(this, item));
            }
            if (CheckupInfo.PacientInfo.Sex == 1)
                foreach (var test in tests)
                    test.Finished = true;
            foreach (var variable in template.Variables)
            {
                var var = new DATVariable(variable);
                Variables.Add(var);
                var.Modified += new System.Windows.RoutedEventHandler(variable_Modified);
            }
        }      

        /// <summary>
        /// Редактирование инструкции, которая используется в данном обледовании
        /// </summary>
        internal void EditCurrentTestTemplate()
        {            
            TestTemplateViewModel viewModel = new TestTemplateViewModel(testTemplate);
            TestTemplateEditor.TestTemplateEditorWindow editor = new TestTemplateEditorWindow(viewModel);
            editor.ShowDialog();
            if (viewModel.Modified)
            {
                TestTemplateChanged = true;
                Modified = true;
                SynchronizeVariables();
                if (CurrentMainCheckupManager != null)
                {
                    if (CurrentMainCheckupManager.MainWindow is MainWindow)
                        (CurrentMainCheckupManager.MainWindow as MainWindow).UpdateAutoTestingToolBarState();
                    CurrentMainCheckupManager.AutoTestGroup = null;
                }
                OnPropertyChanged("AutoTestGroups");                
                NSMessageBox.Show(string.Format(Properties.Resources.TestTemplateWasUpdated, TestTemplate.Name, TestTemplate.VersionString), "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storage"></param>
        public override void Save(CheckupDataStorage storage)
        {
            base.Save(storage);
            if (TestTemplateChanged)
            {
                SaveTestTemplate(TestTemplate);
                TestTemplateChanged = false;
            }            
            CheckupInfo.WriteParam(LastEditDateParam, DateTime.UtcNow.Ticks);
            CheckupInfo.WriteParam(EditUsersInfoParam, EditUsers.ToString());
            bool finished = true;
            foreach (var test in Tests)
            {
                if (!test.FinishingIsConfirmed)
                {
                    finished = false;
                    break;
                }
            }
            CheckupInfo.WriteParam(IsFinishedTesingParam, finished);
            CheckupInfo.Update();
        }

        private bool testTemplateChanged = false;

        internal bool TestTemplateChanged
        {
            get { return testTemplateChanged; }
            set { testTemplateChanged = value; }
        }

        /// <summary>
        /// Синхронизация переменных после редактирования инструкции
        /// </summary>
        private void SynchronizeVariables()
        {            
            List<DATVariable> newVariables = new List<DATVariable>();
            List<DATVariable> oldVariables = new List<DATVariable>(Variables);
            foreach (var variable in TestTemplate.Variables)
            {
                var var = new DATVariable(variable);
                var existsVar = oldVariables.Find(v => v.TestVariableID == variable.Name);
                if (existsVar != null)
                {
                    var.VariableValue = existsVar.VariableValue;
                    existsVar.Modified -= new System.Windows.RoutedEventHandler(variable_Modified);
                }
                newVariables.Add(var);
                var.Modified += new System.Windows.RoutedEventHandler(variable_Modified);                
            }
            Variables.Clear();
            foreach (var variable in newVariables)
            {
                Variables.Add(variable);
            }
        }
        /// <summary>
        /// Удаление теста
        /// </summary>
        /// <param name="test"></param>
        private void RemoveTest(TestObject test)
        {            
            foreach (var child in test.Children)
            {
                RemoveTest(child);
            }
            Tests.Remove(test);            
        }       

        protected override void Deserialized()
        {
            base.Deserialized();            
            if (testTemplate == null)
            {
                testTemplate = ReadTestTemplateByKey(TestTemplateKey);
            }
            testTemplate.IsUsed = true;
            tests.ListDeserializationComplete += new System.Windows.Forms.MethodInvoker(tests_ListDeserializationComplete);            
            Snapshots.ListDeserializationComplete += new System.Windows.Forms.MethodInvoker(Snapshots_ListDeserializationComplete);
            Variables.ListDeserializationComplete += new System.Windows.Forms.MethodInvoker(Variables_ListDeserializationComplete);
            DATTemplate.CurrentTemplate = TestTemplate;
        }

        //void Nodes_ListDeserializationComplete()
        //{
        //    testTemplate.Nodes.ListDeserializationComplete -= new System.Windows.Forms.MethodInvoker(Nodes_ListDeserializationComplete);
        //    SaveTestTemplate(testTemplate);
        //}
        
        /// <summary>
        /// Чтение данных инструкции из базы по ключу
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal DATTemplate ReadTestTemplateByKey(string key)
        {
            byte[] dataBytes = Globals.CurrentConnection.Connection.ReadData(key);
            if (dataBytes == null)
                return null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(dataBytes))
            {
                return formatter.Deserialize(ms) as DATTemplate;                
            }
        }

        void Variables_ListDeserializationComplete()
        {
            Variables.ListDeserializationComplete -= new System.Windows.Forms.MethodInvoker(Variables_ListDeserializationComplete);
            foreach (var variable in Variables)
            {
                variable.Modified += new System.Windows.RoutedEventHandler(variable_Modified);
                variable.InitVarDescriptor(TestTemplate);
            }
        }

        void variable_Modified(object sender, System.Windows.RoutedEventArgs e)
        {
            Modified = true;
        }

        void Snapshots_ListDeserializationComplete()
        {
            Snapshots.ListDeserializationComplete -= new System.Windows.Forms.MethodInvoker(Snapshots_ListDeserializationComplete);
            foreach (var snapshot in Snapshots)
            {
                snapshot.Ancestor = this;
            }
        }

        void tests_ListDeserializationComplete()
        {
            tests.ListDeserializationComplete -= new System.Windows.Forms.MethodInvoker(tests_ListDeserializationComplete);
            PrepareTests();            
        }

        private void PrepareTests()
        {
            foreach (var test in tests)
            {
                test.AncestorParent = this;
            }
        }

        /// <summary>
        /// Получение объекта теста по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TestObject GetTestById(string id)
        {
            foreach (var test in Tests)
            {
                if (test.TestId == id)
                    return test;
            }
            return null;
        }


        /// <summary>
        /// Обновляем признаки возможности выполнения для всех тестов
        /// </summary>
        public void InvalidateCanExecuteTests()
        {
            foreach (var test in Tests)
            {
                test.InvalidateCanExecute();
            }
        }

        #endregion
    }
}
