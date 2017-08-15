using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Снимок процесса тестирования
    /// </summary>
    [Serializable]
    public class TestingSnapshot : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="datTestAncestor"></param>
        /// <param name="executedTest"></param>
        /// <param name="action"></param>        
        /// <param name="comments"></param>
        public TestingSnapshot(DeviceTestCheckupAncestor datTestAncestor, TestObject executedTest, TestActionType action, string comments)
        {
            Ancestor = datTestAncestor;            
            foreach (var variable in datTestAncestor.Variables)
            {
                if (variable.ValueChanged || action == TestActionType.Init)
                {
                    variable.ValueChanged = false;
                    VariableValues.Add(variable.Clone() as DATVariable);
                }
            }
            SnapshotTime = DateTime.UtcNow.Ticks; 
            if (executedTest != null)
            {
                ExecutedTestInfo = new TestSnapshotInfo(executedTest, action);
                ExecutedTestInfo.StartTestTime = executedTest.StartTime > 0 ? executedTest.StartTime : SnapshotTime;
            }
                       
            User = NeuroSoft.Prototype.Interface.Globals.UserName;
            if (string.IsNullOrEmpty(comments))
            {
                Comments = executedTest != null ? executedTest.Comments : "";
            }
            else
            {
                Comments = comments;
            }
            ClientInfo = new ClientInfo();
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TestingSnapshot(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        [Serialize]
        private SerializedList<TestSnapshotInfo> testInfoList = new SerializedList<TestSnapshotInfo>();
        [Serialize]
        private SerializedList<DATVariable> variableValues = new SerializedList<DATVariable>();
        [Serialize]
        private long snapshotTime;        
        [Serialize]
        private string user;        
        [Serialize]
        private string comments;        
        /// <summary>
        /// Значения измененных переменных на момент снимка
        /// </summary>
        public SerializedList<DATVariable> VariableValues
        {
            get { return variableValues; }
        }

        private List<DATVariable> allVariables = null;
        /// <summary>
        /// Значения всех переменных на момент снимка
        /// </summary>
        public List<DATVariable> AllVariables
        {
            get 
            {
                if (allVariables == null)
                {
                    int currSnapshotIndex = Ancestor.Snapshots.IndexOf(this);
                    if (currSnapshotIndex == 0)
                    {
                        allVariables = new List<DATVariable>(VariableValues);
                    }
                    else
                    {
                        allVariables = new List<DATVariable>();
                        foreach (var variable in Ancestor.Variables)
                        {
                            var snapshotVariable = VariableValues.FirstOrDefault(v => v.TestVariableID == variable.TestVariableID);
                            if (snapshotVariable != null)
                            {
                                allVariables.Add(snapshotVariable);
                                continue;
                            }
                            for (int i = currSnapshotIndex - 1; i >= 0; i--)
                            {
                                snapshotVariable = Ancestor.Snapshots[i].VariableValues.FirstOrDefault(v => v.TestVariableID == variable.TestVariableID);
                                if (snapshotVariable != null)
                                {
                                    allVariables.Add(snapshotVariable);
                                    break;
                                }
                            }
                        }
                    }
                }
                return allVariables;
            }
        }        
        
        /// <summary>
        /// Время снимка в тиках
        /// </summary>
        public long SnapshotTime
        {
            get { return snapshotTime; }
            private set { snapshotTime = value; }
        }
        

        /// <summary>
        /// Время выполнения операции с начала открытия теста
        /// </summary>
        public TimeSpan? ExecutionTime
        {
            get 
            {
                if (ExecutedTestInfo == null || !ExecutedTestInfo.Finished || ExecutedTestInfo.StartTestTime <= 0)
                    return null;
                long executionTime = SnapshotTime - ExecutedTestInfo.StartTestTime;
                if (executionTime == 0)
                    return null;
                return new TimeSpan(executionTime);
            }
        }
        
        /// <summary>
        /// Активный пользователь на момент создания снимка
        /// </summary>
        public string User
        {
            get { return user; }
            private set { user = value; }
        }

        [Serialize]
        private ClientInfo clientInfo = null;
        /// <summary>
        /// Информация о клиенте, на котором был сделал снимок
        /// </summary>
        public ClientInfo ClientInfo
        {
            get { return clientInfo; }
            private set { clientInfo = value; }
        }

        private DeviceTestCheckupAncestor ancestor;
        [Serialize]
        private TestSnapshotInfo executedTestInfo;

        /// <summary>
        /// Информация о тесте (на момент снимка), выполнение которого привело к снятию снимка
        /// </summary>
        public TestSnapshotInfo ExecutedTestInfo
        {
            get { return executedTestInfo; }
            private set { executedTestInfo = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ExecutedTestId
        {
            get { return ExecutedTestInfo != null ? ExecutedTestInfo.TestId : null; }
        }
        /// <summary>
        /// Хранитель тестов
        /// </summary>
        public DeviceTestCheckupAncestor Ancestor
        {
            get { return ancestor; }
            internal set { ancestor = value; }
        }

        /// <summary>
        /// Комментарии
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
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TestSnapshotInfo : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="test"></param>
        public TestSnapshotInfo(TestObject test, TestActionType action)
        {
            TestId = test.TestId;
            Finished = test.Finished;
            HasErrors = test.HasErrors;
            WasCorrected = test.WasCorrected;
            TestName = test.TemplateItem.Name;
            Action = action;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TestSnapshotInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        [Serialize]
        private string testId;
        [Serialize]
        private bool finished;
        [Serialize]
        private bool wasCorrected;
        [Serialize]
        private bool hasErrors;
        [Serialize]
        private string testName;
        [Serialize]
        private TestActionType action;
        [Serialize]
        private long startTestTime;
        
        /// <summary>
        /// Идентификатор теста
        /// </summary>
        public string TestId
        {
            get { return testId; }
            private set { testId = value; }
        }

        /// <summary>
        /// Имя теста
        /// </summary>
        public string TestName
        {
            get { return testName; }
            private set { testName = value; }
        }

        /// <summary>
        /// Действие в результате которого сделан снимок
        /// </summary>
        public TestActionType Action
        {
            get { return action; }
            private set { action = value; }
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
                    OnPropertyChanged("Finished");
                }
            }
        }

        /// <summary>
        /// Время открытия теста, за которым последовало действие (tiks)                                                                                                                                                                                                                                                        
        /// </summary>
        public long StartTestTime
        {
            get { return startTestTime; }
            set { startTestTime = value; }
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
                }
            }
        }

        /// <summary>
        /// Признак наличия ошибок
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
                }
            }
        }
    }      

     /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ClientInfo : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="test"></param>
        public ClientInfo()
        {
            ComputerName = Environment.MachineName;
            UserName = Environment.UserName;
            DomainName = Environment.UserDomainName;            
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ClientInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        [Serialize]
        private string computerName;
        /// <summary>
        /// Имя компьютера
        /// </summary>
        public string ComputerName
        {
            get { return computerName; }
            private set { computerName = value; }
        }

        [Serialize]
        private string userName;
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName
        {
            get { return userName; }
            private set { userName = value; }
        }

        [Serialize]
        private string domainName;
        /// <summary>
        /// Домен
        /// </summary>
        public string DomainName
        {
            get { return domainName; }
            private set { domainName = value; }
        }
        /// <summary>
        /// Полное имя пользователя (с учетом домена)
        /// </summary>
        public string UserNameFull
        {
            get
            {
                return (!string.IsNullOrEmpty(DomainName) ? DomainName + @"\" : string.Empty) + UserName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Computer: {0}; User: {1}", ComputerName, UserNameFull);
        }
    }
}
