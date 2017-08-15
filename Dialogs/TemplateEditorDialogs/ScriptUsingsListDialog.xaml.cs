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
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest;
using System.Collections.ObjectModel;
using NeuroSoft.DeviceAutoTest.ScriptExecution;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for ScriptUsingsListDialog.xaml
    /// </summary>
    public partial class ScriptUsingsListDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public ScriptUsingsListDialog(IEnumerable<string> scriptUsings)
        {
            InitializeComponent();
            Usings.Clear();
            foreach (var usingStr in scriptUsings)
            {
                Usings.Add(usingStr);
            }
            DataContext = this;
        }

        #region Properties       

        private string selectedUsing;
        /// <summary>
        /// Выделенное в списке пространство имен
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

        private ObservableCollection<string> usings = new ObservableCollection<string>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> Usings
        {
            get { return usings; }
        }
        #endregion
        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            NewScriptUsingDialog dialog = new NewScriptUsingDialog();
            if (dialog.ShowDialog() == true)
            {
                string newUsing = dialog.SelectedUsing;
                if (!Usings.Contains(newUsing))
                {
                    Usings.Add(newUsing);
                }
            }
        }        

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedUsing == null)
                return;

            int selIndex = Usings.IndexOf(SelectedUsing);
            Usings.RemoveAt(selIndex);
            if (selIndex > Usings.Count - 1)
            {
                selIndex--;
            }
            if (selIndex > -1)
            {
                SelectedUsing= Usings[selIndex];
            }
            
        }
    }  
}
