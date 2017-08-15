using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using System.Windows;
using System.Reflection;


namespace NeuroSoft.DeviceAutoTest.ScriptExecution
{
    /// <summary>
    /// Класс с информацией о скрипте
    /// </summary>
    [Serializable]
    public class ScriptInfo : SimpleSerializedData, ITag
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name"></param>
        /// <param name="className"></param>
        /// <param name="returnType"></param>
        /// <param name="script"></param>
        public ScriptInfo(string name, string className, string returnType, string script)
        {
            Name = name;
            ClassName = className;
            ReturnType = returnType;
            this.script = script;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ScriptInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        [Serialize]
        private string name;
        [Serialize]
        private string description = Properties.Resources.DefaultScriptDescription;
        [Serialize]
        private string className;
        [Serialize]
        private string script = "";
        [Serialize]
        private string returnType = "void";
        [Serialize]
        private bool isEnabled = false;
        [Serialize]
        private SerializedList<string> scriptUsings = new SerializedList<string>();
        [Serialize]
        private SerializedList<string> assemblies = new SerializedList<string>();

        /// <summary>
        /// Код скрипта.
        /// </summary>
        public string Script
        {
            get { return script; }
            set
            {
                if (script != value)
                {
                    script = value;
                    OnPropertyChanged("Script");
                    NotifyModified();
                }
            }
        }

        /// <summary>
        /// Имя скрипта
        /// </summary>
        public string Name
        {
            get { return name; }
            set 
            {
                name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("DisplayValue");
                NotifyModified();
            }
        }
        /// <summary>
        /// Описание скрипта.
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged("Description");
                    OnPropertyChanged("DisplayValue");
                    NotifyModified();
                }
            }
        }

        /// <summary>
        /// Признак использования скрипта
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged("IsEnabled");
                    NotifyModified();
                }
            }
        }

        /// <summary>
        /// Имя генерируемого класса.
        /// </summary>
        public string ClassName
        {
            get { return className; }
            private set { className = value; }
        }

        /// <summary>
        /// Возвращаемый тип
        /// </summary>
        public string ReturnType
        {
            get { return returnType; }
            private set { returnType = value; }
        }
        /// <summary>
        /// Список пространств имен, используемых в скрипте
        /// </summary>
        public SerializedList<string> ScriptUsings
        {
            get { return scriptUsings; }
        }

        /// <summary>
        /// Список внешних библиотек, используемых в скрипте
        /// </summary>
        public SerializedList<string> Assemblies
        {
            get { return assemblies; }
        }


        internal static string GetExecutablePath()
        {
            string executingAssambly = Assembly.GetExecutingAssembly().Location;
            return System.IO.Path.GetDirectoryName(executingAssambly);
        }

        internal TestTemplateItem TemplateItemParent;

        internal void NotifyModified()
        {
            if (TemplateItemParent != null)
            {
                TemplateItemParent.NotifyModified();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            ScriptInfo clone = new ScriptInfo(Name, ClassName, ReturnType, Script);
            clone.Description = Description;
            clone.IsEnabled = IsEnabled;
            foreach (var scriptUsing in ScriptUsings)
            {
                clone.ScriptUsings.Add(scriptUsing);
            }
            foreach (var assembly in Assemblies)
            {
                clone.Assemblies.Add(assembly);
            }
            return clone;
        }


        #region ITag
        internal const string TagPrefix = "@";
        /// <summary>
        /// 
        /// </summary>
        public string Prefix
        {
            get 
            {
                return TagPrefix;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TagInsertMode TagInsertMode
        {
            get { return TagInsertMode.OnlyTestContent; }
        }

        /// <summary>
        /// Оторбажаемое значение
        /// </summary>
        public string DisplayValue
        {
            get
            {
                return Name + " (Button)" + (!string.IsNullOrEmpty(Description) ? " : " + Description : "");
            }
        }
        #endregion
    }    
}
