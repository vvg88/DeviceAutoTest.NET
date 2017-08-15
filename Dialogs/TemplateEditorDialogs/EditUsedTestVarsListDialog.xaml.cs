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
using Microsoft.Win32;
using System.IO;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for EditUsedTestVarsListDialog.xaml
    /// </summary>
    public partial class EditUsedTestVarsListDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public EditUsedTestVarsListDialog(TestTemplateItem testItem)
        {
            InitializeComponent();
            TestItem = testItem;        
            DataContext = this;
        }

        #region Properties

        private TestTemplateItem testItem;
        /// <summary>
        /// 
        /// </summary>
        public TestTemplateItem TestItem
        {
            get { return testItem; }
            private set 
            { 
                testItem = value;
                ParseUsedVariablesFromContent = testItem.ParseUsedVariablesFromContent;
                UsedVariables.Clear();
                foreach (var item in testItem.UsedVariables)
                {
                    var varialbe = testItem.TemplateParent.Variables.FirstOrDefault(v => v.Name == item);
                    if (varialbe != null)
                    {
                        UsedVariables.Add(varialbe);
                    }
                }
                UpdateAvailableVariables();
            }
        }
        
        private bool parseUsedVariablesFromContent;
        /// <summary>
        /// Признак извлечения используемых переменных из содержимого теста
        /// </summary>
        public bool ParseUsedVariablesFromContent
        {
            get { return parseUsedVariablesFromContent; }
            set
            {
                if (parseUsedVariablesFromContent != value)
                {
                    parseUsedVariablesFromContent = value;
                    OnPropertyChanged("ParseUsedVariablesFromContent");
                }
            }
        }

        private DATVariableDescriptor selectedVariable;
        /// <summary>
        /// 
        /// </summary>
        public DATVariableDescriptor SelectedVariable
        {
            get
            {
                return selectedVariable;
            }
            set
            {
                if (selectedVariable != value)
                {
                    selectedVariable = value;                                   
                    OnPropertyChanged("SelectedVariable");
                    OnPropertyChanged("HasSelectedVariable");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool HasSelectedVariable
        {
            get
            {
                return SelectedVariable != null;
            }
        }
        private ObservableCollection<DATVariableDescriptor> availableVariables = new ObservableCollection<DATVariableDescriptor>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<DATVariableDescriptor> AvailableVariables
        {
            get { return availableVariables; }
        }

        private ObservableCollection<DATVariableDescriptor> usedVariables = new ObservableCollection<DATVariableDescriptor>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<DATVariableDescriptor> UsedVariables
        {
            get { return usedVariables; }
        }
        #endregion

        private void RemoveSelectedVariable()
        {
            if (!HasSelectedVariable)
                return;

            int selIndex = UsedVariables.IndexOf(SelectedVariable);
            UsedVariables.RemoveAt(selIndex);
            if (selIndex > UsedVariables.Count - 1)
            {
                selIndex--;
            }
            if (selIndex > -1)
            {
                SelectedVariable = UsedVariables[selIndex];
            }
            UpdateAvailableVariables();
        }

        private void UpdateAvailableVariables()
        {
            AvailableVariables.Clear();
            foreach (var item in testItem.TemplateParent.Variables)
            {
                if (!AvailableVariables.Contains(item))
                    AvailableVariables.Add(item);
            }
            foreach (var item in UsedVariables)
            {
                if (AvailableVariables.Contains(item))
                    AvailableVariables.Remove(item);
            }
        }

        private void AddUsedVariable(DATVariableDescriptor variable)
        {
            if (variable == null)
                return;
            UsedVariables.Add(variable);
            if (AvailableVariables.Contains(variable))
                AvailableVariables.Remove(variable);
        }
        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            TestItem.ParseUsedVariablesFromContent = ParseUsedVariablesFromContent;
            TestItem.UsedVariables.Clear();
            foreach (var item in UsedVariables)
            {
                TestItem.UsedVariables.Add(item.Name);
            }
        }   

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedVariable();
        }
        
        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                RemoveSelectedVariable();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    AddUsedVariable(item as DATVariableDescriptor);
                }
            }            
        }
    }    
}
