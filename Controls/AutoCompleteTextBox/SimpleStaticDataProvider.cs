using System;
using System.Collections.Generic;
using System.Windows;

namespace NeuroSoft.DeviceAutoTest.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class SimpleStaticDataProvider : DependencyObject, IAutoCompleteDataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public SimpleStaticDataProvider()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public SimpleStaticDataProvider(IEnumerable<string> source)
        {
            StringItems = source;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textPattern"></param>
        /// <returns></returns>
        public IEnumerable<AutoCompleteItem> GetItems(string textPattern)
        {
            foreach (var item in StringItems)
            {
                if (textPattern == null || item.StartsWith(textPattern, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new AutoCompleteItem(item);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> StringItems
        {
            get { return (IEnumerable<string>)GetValue(StringItemsProperty); }
            set { SetValue(StringItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StringItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringItemsProperty =
            DependencyProperty.Register("StringItems", typeof(IEnumerable<string>), typeof(SimpleStaticDataProvider), new UIPropertyMetadata(new List<string>()));        
    }
}