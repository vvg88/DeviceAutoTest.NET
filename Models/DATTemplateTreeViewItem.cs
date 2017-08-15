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
    /// Узел дерева в сценарии тестирования
    /// </summary>
    [Serializable]
    public class DATTemplateTreeViewItem : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public DATTemplateTreeViewItem()
        {
            nodes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(nodes_CollectionChanged);
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public DATTemplateTreeViewItem(DATTemplateTreeViewItem parent) 
            : this()
        {
            this.Parent = parent;
        }     

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DATTemplateTreeViewItem(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            nodes.CollectionChanged +=new System.Collections.Specialized.NotifyCollectionChangedEventHandler(nodes_CollectionChanged);
        }
        #endregion

        #region Properties
        [Serialize]
        private string name;
        [Serialize]
        protected SerializedList<DATTemplateTreeViewItem> nodes = new SerializedList<DATTemplateTreeViewItem>();
        [Serialize]
        private bool isCurrent;                
        [Serialize]
        private bool isExpanded;
        private bool isCurrentTest;

        private DATTemplateTreeViewItem parent;

        private bool parentChanging = false;
        /// <summary>
        /// Родительский узел
        /// </summary>
        public DATTemplateTreeViewItem Parent
        {
            get { return parent; }
            set
            {
                if (parent != value)
                {
                    if (parentChanging)
                        return;
                    parentChanging = true;
                    try
                    {
                        if (parent != null)
                        {
                            parent.Nodes.Remove(this);
                        }
                        parent = value;
                        if (parent != null)
                        {
                            parent.Nodes.Add(this);
                        }
                        OnPropertyChanged("Parent");
                    }
                    finally
                    {
                        parentChanging = false;
                    }
                }
            }
        }

        private DATTemplate templateParent;
        /// <summary>
        /// Родитель - шаблон тестирования устройства. 
        /// </summary>
        public DATTemplate TemplateParent
        {
            get
            {
                if (templateParent == null)
                {
                    DATTemplateTreeViewItem parent = Parent;
                    while (parent != null)
                    {
                        if (parent is DATTemplate)
                        {
                            templateParent = parent as DATTemplate;
                            break;
                        }
                        parent = parent.Parent;
                    }
                }
                return templateParent;
            }
        }

        /// <summary>
        /// Признак того, что данный элемент выбран в дереве тестов
        /// </summary>
        public bool IsCurrent
        {
            get { return isCurrent; }
            set
            {
                if (isCurrent != value)
                {
                    isCurrent = value;
                    OnPropertyChangedIgnoreModified("IsCurrent");
                }
            }
        }

        /// <summary>
        /// Признак того, что данный элемент является текущим в редакторе тестов
        /// </summary>
        public bool IsCurrentTest
        {
            get { return isCurrentTest; }
            set
            {
                if (isCurrentTest != value)
                {
                    isCurrentTest = value;
                    OnPropertyChangedIgnoreModified("IsCurrentTest");
                }
            }
        }

        /// <summary>
        /// Признак "раскрытости" элемента в дереве (актуально только для папок)
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChangedIgnoreModified("IsExpanded");
                }
            }
        }

        /// <summary>
        /// Имя папки
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
                }
            }
        }
        /// <summary>
        /// Вложенные папки
        /// </summary>
        public SerializedList<DATTemplateTreeViewItem> Nodes
        {
            get { return nodes; }
        }

        /// <summary>
        /// Признак возможности содержать вложенные узлы
        /// </summary>
        public virtual bool CanContainsInnerNodes
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Метод возвращает сквозной список всех тестов, содержащихся внутри данного узла
        /// </summary>        
        /// <param name="ignoreContainers"></param>
        /// <returns></returns>
        public List<TestTemplateItem> GetAllTests(bool ignoreContainers = false)
        {
            List<TestTemplateItem> result = new List<TestTemplateItem>();
            foreach (var node in Nodes)
            {
                if (node is TestTemplateItem)
                {
                    var testItem = node as TestTemplateItem;
                    if (!(ignoreContainers && testItem.IsContainer))
                    {
                        result.Add(node as TestTemplateItem);
                    }                    
                }
                result.AddRange(node.GetAllTests(ignoreContainers));
            }
            return result;
        }

        /// <summary>
        /// Поиск теста по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TestTemplateItem FindTestById(string id)
        {
            foreach (var node in Nodes)
            {
                if (node is TestTemplateItem)
                {
                    if ((node as TestTemplateItem).TestId == id)
                    {
                        return node as TestTemplateItem;
                    }
                }
                var candidate = node.FindTestById(id);
                if (candidate != null)
                {
                    return candidate;
                }
            }
            return null;
        }

        void nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (parentChanging)
                return;
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (DATTemplateTreeViewItem node in e.NewItems)
                {
                    node.parent = this;
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || 
                     e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (DATTemplateTreeViewItem node in e.OldItems)
                {
                    node.parent = null;
                }
            }
            NotifyModified();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Deserialized()
        {
            base.Deserialized();            
            //назначим родителя дочерним узлам при десериализации
            if (nodes != null)
            {
                nodes.ListDeserializationComplete += new System.Windows.Forms.MethodInvoker(nodes_ListDeserializationComplete);
            }
        }

        void nodes_ListDeserializationComplete()
        {
            nodes.ListDeserializationComplete -= new System.Windows.Forms.MethodInvoker(nodes_ListDeserializationComplete);
            foreach (var node in nodes)
            {
                node.parent = this;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ignoreModified"></param>
        protected void OnPropertyChangedIgnoreModified(string info)
        {
            base.OnPropertyChanged(info);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        protected override void OnPropertyChanged(string info)
        {
            base.OnPropertyChanged(info);
            NotifyModified();
        }

        private void NotifyModified()
        {
            if (TemplateParent != null)
            {
                TemplateParent.OnModified();
            }
        }

        /// <summary>
        /// Удаление узла из дерева тестов шаблона
        /// </summary>
        /// <returns></returns>
        public virtual bool Remove()
        {
            if (Parent == null)
                return false;
            ClearNodes();
            return Parent.Nodes.Remove(this);
        }

        /// <summary>
        /// Удаление всех дочерних элементов
        /// </summary>
        public void ClearNodes()
        {
            for (int i = Nodes.Count - 1; i >= 0; i--)
            {
                Nodes[i].Remove();
            }
        }
        /// <summary>
        /// Вызов диалога переименования узла
        /// </summary>        
        /// <returns></returns>
        public bool Rename()
        {
            RenameNodeDialog renameDialog = new RenameNodeDialog(Name);
            if (renameDialog.ShowDialog() == true)
            {
                if (Name != renameDialog.EditedValue)
                {
                    Name = renameDialog.EditedValue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Вставка узла в список дочерних узлов
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public void InsertNode(int index, DATTemplateTreeViewItem node)
        {
            if (node != null)
            {
                if (index < 0)
                    index = 0;
                if (index > Nodes.Count - 1)
                    index = Nodes.Count - 1;
                Nodes.Insert(index, node);
            }
        }

        /// <summary>
        /// Добавление узла в список дочерних узлов
        /// </summary>        
        /// <param name="node"></param>
        /// <returns></returns>
        public void AddNode(DATTemplateTreeViewItem node)
        {
            if (node != null)
            {                
                Nodes.Add( node);
            }
        }

        /// <summary>
        /// Метод определяет, содержится ли узел node в данном узле
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Contains(DATTemplateTreeViewItem node)
        {
            if (this == node)
                return true;
            foreach (var child in Nodes)
            {
                if (child.Contains(node))
                    return true;
            }
            return false;
        }        

        /// <summary>
        /// Перемещение узла "вверх" в дереве
        /// </summary>
        public void MoveUp()
        {
            if (Parent == null)
                return;
            var parent = Parent;
            int currIndex = parent.Nodes.IndexOf(this);
            if (currIndex > 0)
            {
                parent.Nodes.RemoveAt(currIndex);
                parent.Nodes.Insert(currIndex - 1, this);
            }
        }

        /// <summary>
        /// Возможность перемещения узла "вверх"
        /// </summary>
        public bool CanMoveUp
        {
            get
            {
                return Parent != null && Parent.Nodes.IndexOf(this) > 0;
            }
        }

        /// <summary>
        /// Перемещение узла "вниз" в дереве
        /// </summary>
        public void MoveDown()
        {
            if (Parent == null)
                return;
            var parent = Parent;
            int currIndex = parent.Nodes.IndexOf(this);
            if (currIndex < parent.Nodes.Count - 1)
            {
                parent.Nodes.RemoveAt(currIndex);
                parent.Nodes.Insert(currIndex + 1, this);
            }
        }
        /// <summary>
        /// Возможность перемещения узла "вниз"
        /// </summary>
        public bool CanMoveDown
        {
            get
            {
                return Parent != null && Parent.Nodes.IndexOf(this) < Parent.Nodes.Count - 1;
            }
        }
        #endregion        

        /// <summary>
        /// Клонирование
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            DATTemplateTreeViewItem clone = MemberwiseClone() as DATTemplateTreeViewItem;
            clone.nodes = new SerializedList<DATTemplateTreeViewItem>();
            foreach (var node in Nodes)
            {
                clone.Nodes.Add(node.Clone() as DATTemplateTreeViewItem);
            }
            return clone;
        }
    }

    /// <summary>
    /// Папка с тестами в сценарии тестирования
    /// </summary>
    [Serializable]
    public class DATTemplateFolder : DATTemplateTreeViewItem
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public DATTemplateFolder() : base()
        {            
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public DATTemplateFolder(DATTemplateTreeViewItem parent)
            : base(parent)
        {            
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DATTemplateFolder(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {            
        }
        #endregion        
    }
}
