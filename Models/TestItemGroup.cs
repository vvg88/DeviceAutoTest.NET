using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using NeuroSoft.DeviceAutoTest.Dialogs;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Группа тестов
    /// </summary>
    [Serializable]
    public class TestItemGroup : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public TestItemGroup(string groupName, DATTemplate parent)
        {
            ParentTemplate = parent;
            Name = groupName;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TestItemGroup(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {            
        }
 
        #endregion

        #region Properties
        [Serialize]
        private string name;        
        [Serialize]
        private SerializedList<string> testIdList = new SerializedList<string>();
        [Serialize]
        private bool syncResetTest;
        [Serialize]
        private bool syncAbortTest;
        [Serialize]
        private bool isAutoTestingGroup;  
        
        /// <summary>
        /// Список идентификаторов тестов группы
        /// </summary>
        public SerializedList<string> TestIdList
        {
            get { return testIdList; }
        }
        /// <summary>
        /// Имя группы
        /// </summary>
        public string Name
        {
            get { return name; }
            set 
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                    ParentTemplate.OnModified();
                }
            }
        }

        /// <summary>
        /// Синхронизировать сброс тестов
        /// </summary>
        public bool SyncResetTest
        {
            get { return syncResetTest; }
            set
            {
                if (syncResetTest != value)
                {
                    syncResetTest = value;
                    OnPropertyChanged("SyncResetTest");
                    ParentTemplate.OnModified();
                }
            }
        }
        
        /// <summary>
        /// Синхронизировать отмену тестов
        /// </summary>
        public bool SyncAbortTest
        {
            get { return syncAbortTest; }
            set
            {
                if (syncAbortTest != value)
                {
                    syncAbortTest = value;
                    OnPropertyChanged("SyncAbortTest");
                    ParentTemplate.OnModified();
                }
            }
        }

        /// <summary>
        /// Группа содержит тесты для автоматического выполнения
        /// </summary>
        public bool IsAutoTestingGroup
        {
            get { return isAutoTestingGroup; }
            set
            {
                if (isAutoTestingGroup != value)
                {
                    isAutoTestingGroup = value;
                    OnPropertyChanged("IsAutoTestingGroup");
                    ParentTemplate.OnModified();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<TestTemplateItem> Tests
        {
            get
            {
                List<TestTemplateItem> result = new List<TestTemplateItem>();
                foreach (var testId in TestIdList)
                {
                    result.Add(ParentTemplate.FindTestById(testId));
                }
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<TestTemplateItem> AvailableTests
        {
            get
            {
                List<TestTemplateItem> result = new List<TestTemplateItem>(from test in ParentTemplate.GetAllTests() where !ContainsTest(test) select test);                
                return result;
            }
        }

        private DATTemplate parentTemplate;

        internal DATTemplate ParentTemplate
        {
            get { return parentTemplate; }
            set { parentTemplate = value; }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Добавление теста в группу
        /// </summary>
        /// <param name="testId"></param>
        public void AddTestItem(string testId)
        {
            if (!TestIdList.Contains(testId))
            {
                TestIdList.Add(testId);
                OnPropertyChanged("Tests");
                OnPropertyChanged("AvailableTests");
                ParentTemplate.OnModified();
            }
        }

        /// <summary>
        /// Добавление теста в группу
        /// </summary>
        /// <param name="item"></param>
        public void AddTestItem(TestTemplateItem item)
        {
            AddTestItem(item.TestId);
        }

        /// <summary>
        /// Удаление теста из группы
        /// </summary>
        /// <param name="testId"></param>
        public void RemoveTestItem(string testId)
        {
            if (TestIdList.Contains(testId))
            {
                TestIdList.Remove(testId);
                OnPropertyChanged("Tests");
                OnPropertyChanged("AvailableTests");
                ParentTemplate.OnModified();
            }
        }
        
        /// <summary>
        /// Удаление теста из группы
        /// </summary>
        /// <param name="item"></param>
        public void RemoveTestItem(TestTemplateItem item)
        {
            RemoveTestItem(item.TestId);
        }

        /// <summary>
        /// Перемещение теста в группе
        /// </summary>
        /// <param name="testId"></param>
        public void MoveTestItem(string testId, int newIndex)
        {
            if (TestIdList.Contains(testId))
            {
                TestIdList.Remove(testId);
                if (newIndex < 0) newIndex = 0;
                if (newIndex > TestIdList.Count - 1)
                {
                    TestIdList.Add(testId);
                }
                else
                {
                    TestIdList.Insert(newIndex, testId);
                }
                OnPropertyChanged("Tests");
                OnPropertyChanged("AvailableTests");
                ParentTemplate.OnModified();
            }
        }

        /// <summary>
        /// Перемещение теста в группе
        /// </summary>
        /// <param name="item"></param>
        public void MoveTestItem(TestTemplateItem item, int newIndex)
        {
            MoveTestItem(item.TestId, newIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testItem"></param>
        /// <returns></returns>
        public bool ContainsTest(TestTemplateItem testItem)
        {
            return TestIdList.Contains(testItem.TestId);
        }

        /// <summary>
        /// Переименование группы
        /// </summary>
        public bool Rename()
        {
            RenameTestGroupDialog renameDialog = new RenameTestGroupDialog(Name);
            if (renameDialog.ShowDialog() == true)
            {
                Name = renameDialog.EditedValue;
                ParentTemplate.OnModified();
                return true;
            }
            return false;
        }
        #endregion
    }
}
