using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuroSoft.DeviceAutoTest.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AutoCompleteItem
    {
        private string insertValue;
        private string displayValue;
        /// <summary>
        /// Значение для вставки в текстбокс
        /// </summary>
        public string InsertValue
        {
            get { return insertValue; }
            set { insertValue = value; }
        }
        
        /// <summary>
        /// Представление в списке
        /// </summary>
        public string DisplayValue
        {
            get { return displayValue; }
            set { displayValue = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemStr"></param>
        public AutoCompleteItem(string itemStr)
        {
            InsertValue = itemStr;
            DisplayValue = itemStr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="insertValue"></param>
        /// <param name="displayValue"></param>
        public AutoCompleteItem(string insertValue, string displayValue)
        {
            InsertValue = insertValue;
            DisplayValue = displayValue;
        }

    }
}
