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
using System.CodeDom.Compiler;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using NeuroSoft.WPFComponents.CommandManager;
using NeuroSoft.WPFComponents.ScalableWindows;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for ButtonScriptsWindow.xaml
    /// </summary>
    public partial class ButtonScriptsWindow : DATDialogWindow
    {        
        /// <summary>
        /// Конструктор
        /// </summary>
        public ButtonScriptsWindow(TestTemplateItem testItem, List<ITag> currentTags)
        {
            InitializeComponent();
            TestItem = testItem;            
            tags = currentTags;
            InitializeBindings();
            DataContext = this;
        }

        private IList<ITag> tags = new List<ITag>();
        private TestTemplateItem testItem;        
        private ScriptInfo selectedScript;

        /// <summary>
        /// Шаблон действия
        /// </summary>
        public TestTemplateItem TestItem
        {
            get { return testItem; }
            private set { testItem = value; }
        }
       
        /// <summary>
        /// Выделенный скрипт
        /// </summary>
        public ScriptInfo SelectedScript
        {
            get { return selectedScript; }
            set
            {
                if (selectedScript != value)
                {
                    selectedScript = value;
                    OnPropertyChanged("SelectedScript");
                    OnPropertyChanged("HasSelection");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasSelection
        {
            get { return selectedScript != null; }
        }

        /// <summary>
        /// Теги
        /// </summary>
        public IList<ITag> Tags
        {
            get { return tags; }
        }
        

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Команда добавления скрипта
        /// </summary>
        public static readonly NSCommand AddCommand = new NSCommand("AddCommand", typeof(DATDialogWindow));
        /// <summary>
        /// Команда удаления скрипта
        /// </summary>
        public static readonly NSCommand RemoveCommand = new NSCommand("RemoveCommand", typeof(DATDialogWindow));
        /// <summary>
        /// Команда настройки описания скрипта
        /// </summary>
        public static readonly NSCommand RenameCommand = new NSCommand("RenameCommand", typeof(DATDialogWindow));

        private void InitializeBindings()
        {
            AddCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            ScriptListGroup.CommandBindings.Add(new CommandBinding(AddCommand, OnExecutedAddCommand));
            RenameCommand.InputGestures.Add(new KeyGesture(Key.F2));
            ScriptListGroup.CommandBindings.Add(new CommandBinding(RenameCommand, OnExecutedRenameCommand, OnCanExecuteRenameCommand));
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            ScriptListGroup.CommandBindings.Add(new CommandBinding(RemoveCommand, OnExecutedRemoveCommand, OnCanExecuteRemoveCommand));
        }
       
        private void OnExecutedAddCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var scriptInfo = new ScriptInfo(Properties.Resources.DefaultButtonScriptName, "ButtonScript", "void", "") { TemplateItemParent = TestItem };
            ScriptTagInfoDialog dialog = new ScriptTagInfoDialog(scriptInfo, true);
            if (dialog.ShowDialog() == true)
            {
                scriptInfo.Name = dialog.ScriptName;
                scriptInfo.Description = dialog.ScriptDescription;
                TestItem.ButtonScripts.Add(scriptInfo);
            }
        }

        private void OnCanExecuteRenameCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedScript != null;
        }
        private void OnExecutedRenameCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ScriptTagInfoDialog dialog = new ScriptTagInfoDialog(SelectedScript, false);
            if (dialog.ShowDialog() == true)
            {
                SelectedScript.Name = dialog.ScriptName;
                SelectedScript.Description = dialog.ScriptDescription;
            }
        }
        private void OnCanExecuteRemoveCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedScript != null;
        }
        private void OnExecutedRemoveCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (NSMessageBox.Show(string.Format(Properties.Resources.ConfirmRemoveScript, SelectedScript.Name), Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int index = TestItem.ButtonScripts.IndexOf(SelectedScript);
                TestItem.ButtonScripts.RemoveAt(index);
                if (index >= TestItem.ButtonScripts.Count)
                {
                    index = TestItem.ButtonScripts.Count - 1;
                }
                if (index > -1)
                {
                    SelectedScript = TestItem.ButtonScripts[index];
                }
                TestItem.NotifyModified();
            }
        }
    }
}
