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

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTestGroupsDialog.xaml
    /// </summary>
    public partial class EditTestGroupsDialog : DATDialogWindow
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly NSCommand RemoveSelectedGroupCommand = new NSCommand("RemoveSelectedGroupCommand", typeof(EditTestGroupsDialog));
        /// <summary>
        /// 
        /// </summary>
        public static readonly NSCommand RenameSelectedGroupCommand = new NSCommand("RenameSelectedGroupCommand", typeof(EditTestGroupsDialog));
        /// <summary>
        /// 
        /// </summary>
        public static readonly NSCommand RemoveSelectedTestCommand = new NSCommand("RemoveSelectedTestCommand", typeof(EditTestGroupsDialog));
        /// <summary>
        ///
        /// </summary>
        public static readonly NSCommand GruopItemUpCommand = new NSCommand("GruopItemUpCommand", typeof(EditTestGroupsDialog));
        /// <summary>
        ///
        /// </summary>
        public static readonly NSCommand GruopItemDownCommand = new NSCommand("GruopItemDownCommand", typeof(EditTestGroupsDialog));

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="template"></param>
        public EditTestGroupsDialog(DATTemplate template)
        {
            InitializeComponent();
            ParentTemplate = template;
            DataContext = this;
            RemoveSelectedGroupCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            GroupsGroupBox.CommandBindings.Add(new CommandBinding(RemoveSelectedGroupCommand, OnExecutedRemoveSelectedGroupCommand, OnCanExecuteRemoveSelectedGroupCommand));
            RenameSelectedGroupCommand.InputGestures.Add(new KeyGesture(Key.F2));
            GroupsGroupBox.CommandBindings.Add(new CommandBinding(RenameSelectedGroupCommand, OnExecutedRenameSelectedGroupCommand, OnCanExecuteRenameSelectedGroupCommand));

            RemoveSelectedTestCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            TestsGroupBox.CommandBindings.Add(new CommandBinding(RemoveSelectedTestCommand, OnExecutedRemoveSelectedTestCommand, OnCanExecuteRemoveSelectedTestCommand));

            GruopItemUpCommand.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));
            TestsGroupBox.CommandBindings.Add(new CommandBinding(GruopItemUpCommand, OnExecutedGruopItemUpCommand, OnCanExecuteGruopItemUpCommand));
            GruopItemDownCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Alt));
            TestsGroupBox.CommandBindings.Add(new CommandBinding(GruopItemDownCommand, OnExecutedGruopItemDownCommand, OnCanExecuteGruopItemDownCommand));
        }

        private DATTemplate parentTemplate;
        /// <summary>
        /// 
        /// </summary>
        public DATTemplate ParentTemplate
        {
            get { return parentTemplate; }
            private set
            {
                parentTemplate = value;
            }
        }

        private TestItemGroup selectedGroup;

        /// <summary>
        /// 
        /// </summary>
        public TestItemGroup SelectedGroup
        {
            get { return selectedGroup; }
            set 
            {
                if (selectedGroup != value)
                {
                    selectedGroup = value;
                    OnPropertyChanged("SelectedGroup");
                    OnPropertyChanged("HasSelectedGroup");
                }
            }
        }

        private TestTemplateItem selectedTest;

        /// <summary>
        /// 
        /// </summary>
        public TestTemplateItem SelectedTest
        {
            get { return selectedTest; }
            set
            {
                if (selectedTest != value)
                {
                    selectedTest = value;
                    OnPropertyChanged("SelectedTest");
                    OnPropertyChanged("HasSelectedGroup");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasSelectedGroup
        {
            get { return SelectedGroup != null; }
        }

        private void AddTestMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (SelectedGroup == null)
                return;
            MenuItem menuItem = sender as MenuItem;
            SelectedGroup.AddTestItem(menuItem.DataContext as TestTemplateItem);
        }

        private void AddGroupClick(object sender, RoutedEventArgs e)
        {
            TestItemGroup newGroup = new TestItemGroup(Properties.Resources.DefaultTestGroupName, ParentTemplate);
            if (newGroup.Rename())
            {
                ParentTemplate.TestGroups.Add(newGroup);
            }
        }

        private void RemoveSelectedGroup()
        {
            if (SelectedGroup == null)
                return;
            if (NSMessageBox.Show(string.Format(Properties.Resources.ConfirmRemoveTestGroup, SelectedGroup.Name), Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var group = SelectedGroup;
                int index = ParentTemplate.TestGroups.IndexOf(group);
                ParentTemplate.TestGroups.Remove(group);
                if (index > ParentTemplate.TestGroups.Count - 1)
                {
                    index = ParentTemplate.TestGroups.Count - 1;
                }
                if (index > -1 && index < ParentTemplate.TestGroups.Count)
                {
                    SelectedGroup = ParentTemplate.TestGroups[index];
                }
                ParentTemplate.OnModified();
            }
        }

        private void RenameSelectedGroup()
        {
            if (SelectedGroup == null)
                return;
            SelectedGroup.Rename();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CanAddTest
        {
            get
            {
                return SelectedGroup != null && SelectedAvailableTests.Count > 0;
            }
        }

        private ObservableCollection<TestTemplateItem> selectedAvailableTests = new ObservableCollection<TestTemplateItem>();
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<TestTemplateItem> SelectedAvailableTests
        {
            get { return selectedAvailableTests; }            
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {                        
            if (e.RemovedItems != null)
            {
                foreach (TestTemplateItem item in e.RemovedItems)
                {
                    SelectedAvailableTests.Remove(item);
                }
            }
            if (e.AddedItems != null)
            {
                foreach (TestTemplateItem item in e.AddedItems)
                {
                    SelectedAvailableTests.Add(item);
                }
            }
            OnPropertyChanged("CanAddTest");
        }

        private void AddTestClick(object sender, RoutedEventArgs e)
        {
            if (!CanAddTest)
                return;
            List<TestTemplateItem> toAdd = new List<TestTemplateItem>(SelectedAvailableTests);
            foreach (var item in toAdd)
            {
                SelectedGroup.AddTestItem(item);
            }
        }

        #region Commands

        #region RemoveSelectedGroupCommand
        private void OnCanExecuteRemoveSelectedGroupCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedGroup != null;
        }

        private void OnExecutedRemoveSelectedGroupCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveSelectedGroup();
        }
        #endregion

        #region RenameSelectedGroupCommand
        private void OnCanExecuteRenameSelectedGroupCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedGroup != null;
        }

        private void OnExecutedRenameSelectedGroupCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RenameSelectedGroup();
        }
        #endregion

        #region RemoveSelectedTestCommand
        private void OnCanExecuteRemoveSelectedTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedGroup != null && SelectedTest != null;
        }

        private void OnExecutedRemoveSelectedTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            int index = SelectedGroup.Tests.IndexOf(SelectedTest);
            SelectedGroup.RemoveTestItem(SelectedTest);
            if (index > SelectedGroup.Tests.Count - 1)
            {
                index--;
            }
            if (index > -1 && index < SelectedGroup.Tests.Count)
            {
                SelectedTest = SelectedGroup.Tests[index];
            }
        }
        #endregion

        #region GruopItemUpCommand
        private void OnCanExecuteGruopItemUpCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedGroup != null && SelectedTest != null && SelectedGroup.Tests.IndexOf(SelectedTest) > 0;
        }

        private void OnExecutedGruopItemUpCommand(object sender, ExecutedRoutedEventArgs e)
        {
            int index = SelectedGroup.Tests.IndexOf(SelectedTest);
            var savedSelectedTest = SelectedTest;
            SelectedGroup.MoveTestItem(savedSelectedTest, --index);
            SelectedTest = savedSelectedTest;
        }
        #endregion       

        #region GruopItemDownCommand
        private void OnCanExecuteGruopItemDownCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedGroup != null && SelectedTest != null && SelectedGroup.Tests.IndexOf(SelectedTest) < SelectedGroup.Tests.Count - 1;
        }

        private void OnExecutedGruopItemDownCommand(object sender, ExecutedRoutedEventArgs e)
        {
            int index = SelectedGroup.Tests.IndexOf(SelectedTest);
            var savedSelectedTest = SelectedTest;
            SelectedGroup.MoveTestItem(savedSelectedTest, ++index);
            SelectedTest = savedSelectedTest;
        }
        #endregion       
        #endregion

        
    }    
}
