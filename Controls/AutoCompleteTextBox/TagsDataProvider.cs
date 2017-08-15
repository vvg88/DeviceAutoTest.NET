using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuroSoft.DeviceAutoTest.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class TagsDataProvider : IAutoCompleteDataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public TagsDataProvider()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        public TagsDataProvider(List<ITag> tags)
        {
            TagList = tags;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textPattern"></param>
        /// <returns></returns>
        public IEnumerable<AutoCompleteItem> GetItems(string textPattern)
        {
            foreach (var item in TagList)
            {
                string tagText = (IgnorePrefix ? "" : item.Prefix) + item.Name;
                if (textPattern == null || tagText.StartsWith(textPattern, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new AutoCompleteItem(tagText, item.DisplayValue);
                }
            }
        }

        private List<ITag> tagList = new List<ITag>();
        /// <summary>
        /// 
        /// </summary>
        public List<ITag> TagList
        {
            get { return tagList; }
            set { tagList = value; }
        }

        private bool ignorePrefix;

        /// <summary>
        /// Игнорировать префикс при вставке тега
        /// </summary>
        public bool IgnorePrefix
        {
            get { return ignorePrefix; }
            set { ignorePrefix = value; }
        }
    }
}
