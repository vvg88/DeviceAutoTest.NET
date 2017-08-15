using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.WPFPrototype.Interface;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// 
    /// </summary>
    public class TestTreeObject : TreeObject
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <param name="testId"></param>
        /// <param name="imageKey"></param>
        public TestTreeObject(string name, string text, string testId, string imageKey)
            : base(name, text, imageKey)
        {
            TestId = testId;
        }

        private InspectorTreeViewItem inspectorItem;
        /// <summary>
        /// 
        /// </summary>
        public InspectorTreeViewItem InspectorItem
        {
            get { return inspectorItem; }
            internal set { inspectorItem = value; }
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

        /// <summary>
        /// Поиск TreeObject теста с идентификатором testId
        /// </summary>
        /// <param name="searchIn"></param>
        /// <param name="testId"></param>
        /// <returns></returns>
        public static TestTreeObject FindTestTreeObject(TreeObject searchIn, string testId)
        {
            if (searchIn != null)
            {
                for (int i = 0; i < searchIn.Count; i++)
                {
                    var node = searchIn[i];
                    if (node is TestTreeObject)
                    {
                        if ((node as TestTreeObject).TestId == testId)
                        {
                            return node as TestTreeObject;
                        }
                    }
                    var candidate = FindTestTreeObject(node, testId);
                    if (candidate != null)
                    {
                        return candidate;
                    }
                }
            }
            return null;
        }        
    }

    /// <summary>
    /// 
    /// </summary>
    public class TestTreeViewItem : InspectorTreeViewItem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="treeObject"></param>
        public TestTreeViewItem(InspectorTreeViewItem parent, TreeObject treeObject, TestObject testObject)
            : base(parent, treeObject)
        {
            TestObject = testObject;
            if (TestObject != null)
                IsExpanded = TestObject.TemplateItem.IsExpanded;
        }

        private TestObject testObject;

        /// <summary>
        /// Объект теста
        /// </summary>
        public TestObject TestObject
        {
            get { return testObject; }
            set
            {
                if (testObject != value)
                {
                    testObject = value;
                    OnPropertyChanged("TestObject");
                }
            }
        }
    }   
}
