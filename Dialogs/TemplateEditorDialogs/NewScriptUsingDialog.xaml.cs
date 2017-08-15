using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Reflection;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// 
    /// </summary>
    public partial class NewScriptUsingDialog : DATDialogWindow
    {        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="availableTests"></param>
        public NewScriptUsingDialog() 
        {
            InitializeComponent();
            AvailableUsings = GetAllNamespaces();
            DataContext = this;
        }

        #region Properties
        private List<string> availableUsings;

        /// <summary>
        /// 
        /// </summary>
        public List<string> AvailableUsings
        {
            get { return availableUsings; }
            private set
            {
                availableUsings = value;
                if (availableUsings != null && availableUsings.Count > 0)
                {
                    SelectedUsing = AvailableUsings[0];
                }
            }
        }

        private string selectedUsing;

        /// <summary>
        /// 
        /// </summary>
        public string SelectedUsing
        {
            get { return selectedUsing; }
            set
            {
                if (selectedUsing != value)
                {
                    selectedUsing = value;
                    OnPropertyChanged("SelectedUsing");
                }
            }
        }
        #endregion 
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);            
        }

        private List<string> GetAllNamespaces()
        {
            AppDomain MyDomain = AppDomain.CurrentDomain;
            Assembly[] AssembliesLoaded = MyDomain.GetAssemblies();
            List<string> namespaces = new List<string>();
            foreach (var assembly in AssembliesLoaded)
            {
                foreach (var type in assembly.GetTypes())
                {
                    string ns = type.Namespace;
                    if (ns != null && !namespaces.Contains(ns) && Regex.IsMatch(ns, @"^[a-zA-Z\d\.]+$"))
                    {
                        namespaces.Add(ns);
                    }
                }
            }
            namespaces.Sort();
            return namespaces;
        }
    }    
}
