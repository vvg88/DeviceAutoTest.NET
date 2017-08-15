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
using System.Collections.ObjectModel;
using NeuroSoft.WPFComponents.NSGridView;
using NeuroSoft.WPFComponents.WPFToolkit;
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using NeuroSoft.WPFComponents.CommandManager;
using NeuroSoft.WPFComponents.ScalableWindows;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for EditDATVariablesDialog.xaml
    /// </summary>
    public partial class EditDATVariablesDialog : DATDialogWindow
    {
        private ObservableCollection<DATVariableDescriptor> variables = new ObservableCollection<DATVariableDescriptor>();
        /// <summary>
        /// Список переменных
        /// </summary>
        public ObservableCollection<DATVariableDescriptor> Variables
        {
            get { return variables; }
            private set { variables = value; }
        }

        private ObservableCollection<DATVariableDescriptor> selectedVariables = new ObservableCollection<DATVariableDescriptor>();

        /// <summary>
        /// Выделенные переменные
        /// </summary>
        public ObservableCollection<DATVariableDescriptor> SelectedVariables
        {
            get { return selectedVariables; }
            private set { selectedVariables = value; }
        }

        private ICollectionView variablesView;
        /// <summary>
        /// 
        /// </summary>
        public ICollectionView VariablesView
        {
            get { return variablesView; }
            private set { variablesView = value; }
        }
        

        /// <summary>
        /// Список существующих имен переменных
        /// </summary>
        internal static List<string> ExistingNames { get; set; }

        private void UpdateExistingNamesList(DATVariableDescriptor currentVariable)
        {
            ExistingNames = new List<string>((from var in ParentTemplate.Variables.Union(ParentTemplate.TestInfoVariables)
                                              where currentVariable == null || var.Name != currentVariable.Name
                                             select var.Name));
        }

        /// <summary>
        /// Количество выделенных тегов
        /// </summary>
        public int SelectionCount
        {
            get { return (int)GetValue(SelectionCountProperty); }
            set { SetValue(SelectionCountProperty, value); }
        }

        /// <summary>
        ///    Свойство зависимостей для SelectionCount
        /// </summary>
        public static readonly DependencyProperty SelectionCountProperty =
            DependencyProperty.Register("SelectionCount", typeof(int), typeof(EditDATVariablesDialog), new FrameworkPropertyMetadata(0, null, SelectedCountCoerce));

        private static object SelectedCountCoerce(DependencyObject d, object baseValue)
        {            
            return (int)baseValue < 0 ? 0 : baseValue;
        }

        private DATTemplate ParentTemplate;

        private string filter;
        /// <summary>
        /// Фильтр отображения переменных
        /// </summary>
        public string Filter
        {
            get { return filter; }
            set
            {
                if (filter != value)
                {
                    filter = value;
                    VariablesView.Filter = v =>
                    {
                        if (string.IsNullOrWhiteSpace(filter))
                            return true;
                        var variable = v as DATVariableDescriptor;
                        if (variable == null)
                            return false;
                        string tagName = variable.Prefix + variable.Name;                        
                        return tagName.Contains(filter);// || variable.Description.Contains(filter);
                    };
                    OnPropertyChanged("Filter");
                }
            }
        }

        private bool showDetails;
        /// <summary>
        /// 
        /// </summary>
        public bool ShowDetails
        {
            get { return showDetails; }
            set
            {
                if (showDetails != value)
                {
                    showDetails = value;
                    OnPropertyChanged("ShowDetails");
                }
            }
        }

        

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="userTags"></param>
        /// <param name="existingAvailableTags"></param>
        public EditDATVariablesDialog(DATTemplate template)
        {
            InitializeComponent();
            ParentTemplate = template;
            Variables = new ObservableCollection<DATVariableDescriptor>(from var in template.Variables select var.Clone() as DATVariableDescriptor);
            DataContext = this;
            selectedVariables.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(selectedVariables_CollectionChanged);
            VariablesView = CollectionViewSource.GetDefaultView(Variables); 
        }      

        void selectedVariables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SelectionCount = SelectedVariables.Count;
            if (!selectedVariablesChanging)
            {
                foreach (var item in SelectedVariables)
                {
                    if (!nsGrid.SelectedItems.Contains(item))
                    {
                        nsGrid.SelectedItems.Add(item);
                    }
                }
            }
        }
        private bool selectedVariablesChanging = false;

        private void AddTagBtn_Click(object sender, RoutedEventArgs e)
        {
            var newVariable = new DATVariableDescriptor("variable", DATVariableType.String) { Description = Properties.Resources.DefaultVariableDescription };
            InsertVariable(-1, newVariable);
        }

        private void InsertVariable(int index, DATVariableDescriptor newVar)
        {
            CancelEdit();
            string varName = newVar.Name;
            int i = 1;
            while (Variables.SingleOrDefault(var => var.Name == varName) != null)
            {
                varName = newVar.Name + "_" + i++;
            }
            newVar.Name = varName;
            if (index > -1 && index < Variables.Count - 1)
            {
                Variables.Insert(index, newVar);
            }
            else
            {
                Variables.Add(newVar);
            }
            nsGrid.SelectedItem = newVar;
            nsGrid.ScrollIntoView(newVar);
        }
        
        private void DeleteTagBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelEdit();
            int index = nsGrid.SelectedIndex;// SelectedTags.Min(tag => UserTags.IndexOf(tag)); // UserTags.IndexOf(SelectedTag);
            List<DATVariableDescriptor> wasSelected = new List<DATVariableDescriptor>(SelectedVariables);
            foreach (var selectedTag in wasSelected)
            {
                Variables.Remove(selectedTag);
            }
            if (index >= 0)
            {
                if (Variables.Count > 0)
                {
                    if (Variables.Count - 1 < index)
                    {
                        nsGrid.SelectedIndex = Variables.Count - 1;
                    }
                    else
                    {
                        nsGrid.SelectedIndex = index;
                    }
                }
            }
        }

        private void CancelEdit()
        {
            if (!nsGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                nsGrid.CancelEdit();
            }
        }
        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelEdit();
            DialogResult = true;
        }
       

        private void NSGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if ((e.Column as NSGridTextColumn)?.Name == "NameColumn")
            {
                UpdateExistingNamesList(e.Row.Item as DATVariableDescriptor);
            }
        }
  
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            selectedVariables.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(selectedVariables_CollectionChanged);
            if (DialogResult == true)
            {
                ParentTemplate.Variables.Clear();
                foreach (var variable in Variables)
                {
                    ParentTemplate.Variables.Add(variable);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            NameScope.SetNameScope(nsGrid.ContextMenu, NameScope.GetNameScope(this));
        }

        private void nsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedVariablesChanging = true;
            try
            {
                foreach (var item in e.RemovedItems)
                {
                    if (item is DATVariableDescriptor && SelectedVariables.Contains(item as DATVariableDescriptor))
                    {
                        SelectedVariables.Remove(item as DATVariableDescriptor);
                    }
                }
                foreach (var item in e.AddedItems)
                {
                    if (item is DATVariableDescriptor)
                    {
                        SelectedVariables.Add(item as DATVariableDescriptor);
                    }
                }
            }
            finally
            {
                selectedVariablesChanging = false;
            }
        }

        private void UpTagBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedVariables[0];
            int index = Variables.IndexOf(selected);
            if (index > 0)
            {
                Variables.Move(index, index - 1);
            }
        }

        private void DownTagBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedVariables[0];
            int index = Variables.IndexOf(selected);
            if (index < Variables.Count - 1)
            {
                Variables.Move(index, index + 1);
            }
        }

        private void CopyVariable_Click(object sender, RoutedEventArgs e)
        {
            if (SelectionCount != 1)
                return;
            var selected = SelectedVariables[0];
            int index = Variables.IndexOf(selected);
            var copy = selected.Clone() as DATVariableDescriptor;
            InsertVariable(index + 1, copy);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                BindingOperations.GetBindingExpression(FilterTextBox, TextBox.TextProperty).UpdateSource();
                e.Handled = true;
            }
        }
    }


    /// <summary>
    /// Валидация имени тега
    /// </summary>
    public class ValidateVariableName : ValidationRule
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                DATVariableDescriptor variable = new DATVariableDescriptor((string)value, DATVariableType.String);
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, ex.Message);
            }
            if (EditDATVariablesDialog.ExistingNames.Contains((string)value))
            {
                return new ValidationResult(false, string.Format(Properties.Resources.VariableAlreadyExists, value));
            }
            return new ValidationResult(true, null);
        }
    }
}
