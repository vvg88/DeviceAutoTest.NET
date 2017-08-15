using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace NeuroSoft.DeviceAutoTest.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AutoCompleteTextBox : TextBox
    {        
        /// <summary>
        /// 
        /// </summary>
        public AutoCompleteTextBox()
        {
            this.Loaded += AutoCompleteTextBox_Loaded;
        }

        void AutoCompleteTextBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _acm.AttachTextBox(this);            
        }

        private AutoCompleteManager _acm = new AutoCompleteManager();
        /// <summary>
        /// 
        /// </summary>
        public AutoCompleteManager AutoCompleteManager
        {
            get { return _acm; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IAutoCompleteDataProvider DataProvider
        {
            get
            {
                return AutoCompleteManager.DataProvider;
            }
            set
            {
                AutoCompleteManager.DataProvider = value;
            }
        }
    }
}
