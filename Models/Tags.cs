using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Тег ContentPresenter'а
    /// </summary>
    [Serializable]
    public class ContentPresenterTag : SimpleSerializedData, ITag
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public ContentPresenterTag(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ContentPresenterTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        [Serialize]
        private string name;
        [Serialize]
        private string description;

        /// <summary>
        /// Идентификатор переменной
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, DATVariableDescriptor.NamePattern))
                    {
                        throw new ArgumentException(Properties.Resources.InvalidId);
                    }
                    name = value;
                    OnPropertyChanged("Name");
                    OnPropertyChanged("DisplayValue");
                }
            }
        }

        /// <summary>
        /// Описание переменной
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
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayValue
        {
            get
            {
                return Name + " (ContentPresenter)" + (string.IsNullOrEmpty(Description) ? "" : ": " + Description);
            }
        }

        /// <summary>
        /// Префикс переменной при использовании в содержимом теста.
        /// </summary>
        public string Prefix
        {
            get
            {
                return @"#";
            }
        }

        /// <summary>
        /// Режим вставки переменной
        /// </summary>
        public TagInsertMode TagInsertMode
        {
            get
            {
                return TagInsertMode.OnlyTestContent;
            }
        }

        private ContentPresenter contentPresenterInstance;
        /// <summary>
        /// Экземпляр ContentPresenter'а, соответствующего этому тегу
        /// </summary>
        public ContentPresenter ContentPresenterInstance
        {
            get
            {
                if (contentPresenterInstance == null)
                {
                    contentPresenterInstance = new ContentPresenter() { Name = Name };
                }
                return contentPresenterInstance;
            }
        }

        internal void ResetPresenterInstnace()
        {
            contentPresenterInstance = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = MemberwiseClone() as ContentPresenterTag;
            clone.contentPresenterInstance = null;
            return clone;
        }
    }

    /// <summary>
    /// Тег
    /// </summary>
    public class CustomTag : ITag
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public CustomTag(string name, string description)
        {
            Name = name;
            Description = description;
        }
        #endregion

        private static CustomTag supplyCurrentTag;

        /// <summary>
        /// Тег индикатора тока потребления
        /// </summary>
        public static CustomTag SupplyCurrentTag
        {
            get 
            {
                if (supplyCurrentTag == null)
                {
                    supplyCurrentTag = new CustomTag("supplyCurrent", Properties.Resources.SupplyCurrentTag) { Prefix = "~", TagInsertMode = TagInsertMode.OnlyTestContent };
                }
                return CustomTag.supplyCurrentTag; 
            }
        }

        [Serialize]
        private string name;
        [Serialize]
        private string description;

        /// <summary>
        /// Идентификатор переменной
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, DATVariableDescriptor.NamePattern))
                    {
                        throw new ArgumentException(Properties.Resources.InvalidId);
                    }
                    name = value;
                }
            }
        }

        /// <summary>
        /// Описание переменной
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayValue
        {
            get
            {
                return Name + (string.IsNullOrEmpty(Description) ? "" : ": " + Description);
            }
        }

        private string prefix = "";
        /// <summary>
        /// Префикс тега при использовании в содержимом теста.
        /// </summary>
        public string Prefix
        {
            get
            {
                return prefix;
            }
            set
            {
                prefix = value;
            }
        }

        private TagInsertMode tagInsertMode = TagInsertMode.Both;
        /// <summary>
        /// Режим вставки переменной
        /// </summary>
        public TagInsertMode TagInsertMode
        {
            get
            {
                return tagInsertMode;
            }
            set
            {
                tagInsertMode = value;
            }
        }
    }

    /// <summary>
    /// Интерфейс тегов для вставки в содержимое инструкции или скрипты 
    /// </summary>
    public interface ITag
    {
        /// <summary>
        /// Имя тега
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Префикс тега при вставке в содержимое инструкции
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Значение, отображаемое в списке
        /// </summary>
        string DisplayValue { get; }

        /// <summary>
        /// Где можно использовать тег
        /// </summary>
        TagInsertMode TagInsertMode { get; }
    }

    /// <summary>
    /// Перечисление, где можно использовать тег
    /// </summary>
    public enum TagInsertMode
    {
        /// <summary>
        /// Только в содержимом теста
        /// </summary>
        OnlyTestContent,
        /// <summary>
        /// Только в скрипте
        /// </summary>
        OnlyScript,
        /// <summary>
        /// Везде
        /// </summary>
        Both
    }
}
