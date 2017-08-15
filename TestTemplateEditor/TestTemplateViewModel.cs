using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NeuroSoft.WPFPrototype.Interface.Common;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using NeuroSoft.DeviceAutoTest.Commands;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using NeuroSoft.Prototype.Interface;

namespace NeuroSoft.DeviceAutoTest.TestTemplateEditor
{
    /// <summary>
    /// Модель представления редактора сценариев тестирования
    /// </summary>
    public class TestTemplateViewModel : BaseViewModel
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="model"></param>
        public TestTemplateViewModel(DATTemplate model)
        {
            Model = model;
            InitCommandBindings();
        }

        #region Properties
        private DATTemplate model;
        /// <summary>
        /// Модель
        /// </summary>
        public DATTemplate Model
        {
            get { return model; }
            private set 
            {                
                model = value;
                OnPropertyChanged("Model");
                model.Modified += new RoutedEventHandler(model_Modified);
            }
        }

        void model_Modified(object sender, RoutedEventArgs e)
        {
            Modified = true;
        }

        private bool modified = false;

        /// <summary>
        /// Признак наличия изменений
        /// </summary>
        public bool Modified
        {
            get 
            {
                return modified; 
            }
            set
            {
                if (modified != value)
                {
                    modified = value;
                    OnPropertyChanged("Modified");
                }
                if (modified)
                {
                    Model.LastEditDateTicks = DateTime.UtcNow.Ticks;
                }
            }
        }


        private TestTemplateItem currentTest;

        /// <summary>
        /// Текущий (редактируемый) тест сценария тестирования
        /// </summary>
        public TestTemplateItem CurrentTest
        {
            get { return currentTest; }
            set 
            {
                if (currentTest != value)
                {
                    currentTest = value;
                    OnPropertyChanged("CurrentTest");
                    OnCurrentTestChanged();
                }
            }
        }

        private DATTemplateTreeViewItem selectedItem;

        /// <summary>
        /// Элемент дерева тестов, выделенный в текущий момент
        /// </summary>
        public DATTemplateTreeViewItem SelectedItem
        {
            get { return selectedItem; }
            set 
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        internal event RoutedEventHandler CurrentTestChanged;

        private void OnCurrentTestChanged()
        {
            if (CurrentTestChanged != null)
            {
                CurrentTestChanged(this, new RoutedEventArgs());
            }
        }


        internal void NotifyTagsChanged()
        {
            OnPropertyChanged("TestContentTags");
        }

        private List<ITag> AllTags
        {
            get
            {
                List<ITag> result = new List<ITag>();
                result.Add(new CustomTag("environment", Properties.Resources.EnvironmentTagDescription) { TagInsertMode = TagInsertMode.OnlyScript });
                result.Add(CustomTag.SupplyCurrentTag);
                result.AddRange(Model.Variables);
                if (CurrentTest != null)
                {
                    result.AddRange(CurrentTest.ContentPresenters);
                    result.AddRange(CurrentTest.ButtonScripts);
                }
                //result.AddRange(Model.TestInfoVariables);
                return result;
            }
        }
        /// <summary>
        /// Список доступных тегов
        /// </summary>
        public List<ITag> TestContentTags
        {
            get
            {
                List<ITag> result = new List<ITag>();
                result.AddRange(from tag in AllTags
                                where tag.TagInsertMode != TagInsertMode.OnlyScript
                                select tag);
                return result;
            }
        }

        /// <summary>
        /// Список доступных тегов
        /// </summary>
        public List<ITag> ScriptTags
        {
            get
            {
                List<ITag> result = new List<ITag>();
                result.AddRange(from tag in AllTags
                                where tag.TagInsertMode != TagInsertMode.OnlyTestContent
                                select tag);
                return result;
            }
        }
        
        /// <summary>
        /// Папка с прикрепленными к инструкции изображениями
        /// </summary>
        public DirectoryInfo ImagesDirectory
        {
            get
            {
                string path = Model.ImagesPath;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);                    
                }
                return new DirectoryInfo(path);
            }
        }
        /// <summary>
        /// Список доступных изображений для вставки из файла
        /// </summary>
        public List<string> ImagesFromFiles
        {
            get
            {
                List<string> result = new List<string>();
                foreach (var fileInfo in ImagesDirectory.GetFiles())
                {
                    if (Regex.IsMatch(fileInfo.Extension, DATTemplate.AvailableImageTypesPattern))
                        result.Add(fileInfo.Name);
                }
                return result;
            }
        }

        internal void InvalidateImagesFromFiles()
        {
            OnPropertyChanged("ImagesFromFiles");
        }

        /// <summary>
        /// 
        /// </summary>
        public int NewCorrectionsCount
        {
            get
            {
                return TemplateCorrections.Read(Globals.CurrentConnection.Connection, Model.GUID.ToString()).Corrections.Count;                
            }
        }

        #endregion

        #region Commands
        private CommandBindingCollection navigationCommandBindings = new CommandBindingCollection();

        internal CommandBindingCollection NavigationCommandBindings
        {
            get { return navigationCommandBindings; }            
        }
        /// <summary>
        /// Инициализация привязок команд
        /// </summary>
        private void InitCommandBindings()
        {
            DATTemplateCommands.RemoveNodeCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            NavigationCommandBindings.Add(new CommandBinding(DATTemplateCommands.RemoveNodeCommand, OnExecutedRemoveNodeCommand, OnCanExecuteRemoveNodeCommand));
            DATTemplateCommands.RenameNodeCommand.InputGestures.Add(new KeyGesture(Key.F2));
            NavigationCommandBindings.Add(new CommandBinding(DATTemplateCommands.RenameNodeCommand, OnExecutedRenameNodeCommand, OnCanExecuteRenameNodeCommand));
            NavigationCommandBindings.Add(new CommandBinding(DATTemplateCommands.ChangeTestIdCommand, OnExecutedChangeTestIdCommand, OnCanExecuteChangeTestIdCommand));
            DATTemplateCommands.MoveUpNodeCommand.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));
            NavigationCommandBindings.Add(new CommandBinding(DATTemplateCommands.MoveUpNodeCommand, OnExecutedMoveUpNodeCommand, OnCanExecuteMoveUpNodeCommand));
            DATTemplateCommands.MoveDownNodeCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Alt));
            NavigationCommandBindings.Add(new CommandBinding(DATTemplateCommands.MoveDownNodeCommand, OnExecutedMoveDownNodeCommand, OnCanExecuteMoveDownNodeCommand));
            NavigationCommandBindings.Add(new CommandBinding(DATTemplateCommands.OpenTestCommand, OnExecutedOpenTestCommand, OnCanExecuteOpenTestCommand));
            NavigationCommandBindings.Add(new CommandBinding(DATTemplateCommands.ChangeIsContainerCommand, OnExecutedChangeIsContainerCommand, OnCanExecuteChangeIsContainerCommand));

            CommandBindings.Add(new CommandBinding(DATEditingCommands.FormatLeftAlignCommand, OnExecutedFormatLeftAlignCommand, OnCanExecuteFormatLeftAlignCommand));
            CommandBindings.Add(new CommandBinding(DATEditingCommands.FormatCenterAlignCommand, OnExecutedFormatCenterAlignCommand, OnCanExecuteFormatCenterAlignCommand));
            CommandBindings.Add(new CommandBinding(DATEditingCommands.FormatRightAlignCommand, OnExecutedFormatRightAlignCommand, OnCanExecuteFormatRightAlignCommand));
            CommandBindings.Add(new CommandBinding(DATEditingCommands.ToggleBoldCommand, OnExecutedToggleBoldCommand, OnCanExecuteToggleBoldCommand));
            CommandBindings.Add(new CommandBinding(DATEditingCommands.ToggleItalicCommand, OnExecutedToggleItalicCommand, OnCanExecuteToggleItalicCommand));
            CommandBindings.Add(new CommandBinding(DATEditingCommands.ToggleUnderlineCommand, OnExecutedToggleUnderlineCommand, OnCanExecuteToggleUnderlineCommand));
            CommandBindings.Add(new CommandBinding(DATEditingCommands.ToggleBulletsCommand, OnExecutedToggleBulletsCommand, OnCanExecuteToggleBulletsCommand));
            CommandBindings.Add(new CommandBinding(DATEditingCommands.EditColorCommand, OnExecutedEditColorCommand, OnCanExecuteEditColorCommand));
            CommandBindings.Add(new CommandBinding(DATEditingCommands.EditBackColorCommand, OnExecutedEditBackColorCommand, OnCanExecuteEditBackColorCommand));            

            DATTemplateCommands.AddTestCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.AddTestCommand, OnExecutedAddTestCommand, OnCanExecuteAddTestCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.CreateDATFolderCommand, OnExecutedCreateDATFolderCommand, OnCanExecuteCreateDATFolderCommand));
            DATTemplateCommands.SaveTemplateCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.SaveTemplateCommand, OnExecutedSaveTemplateCommand, OnCanExecuteSaveTemplateCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.RenameTemplateCommand, OnExecutedRenameTemplateCommand, OnCanExecuteRenameTemplateCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.ExportTemplateCommand, OnExecutedExportTemplateCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.SaveTemplateInDBCommand, OnExecutedSaveTemplateInDBCommand, OnCanExecuteSaveTemplateInDBCommand));
            
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditDATVariablesCommand, OnExecutedEditDATVariablesCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.ValidateScriptsCommand, OnExecutedValidateScriptsCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.PreviewCommand, OnExecutedPreviewCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditValidationCommand, OnExecutedEditValidationCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditBeginTestScriptCommand, OnExecutedEditBeginTestScriptCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditEndTestScriptCommand, OnExecutedEditEndTestScriptCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.SelectPreviousTestCommand, OnExecutedSelectPreviousTestCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.TestScriptUsingsCommand, OnExecutedTestScriptUsingsCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.ProtocolPatternsCommand, OnExecutedProtocolPatternsCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.ContentPresenterTagsCommand, OnExecutedContentPresenterTagsCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.ButtonScriptsCommand, OnExecutedButtonScriptsCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditTestGroupsCommand, OnExecutedEditTestGroupsCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.AutoTestSettingsCommand, OnExecutedAutoTestSettingsCommandCommand));            

            CommandBindings.Add(new CommandBinding(DATTemplateCommands.PasteImageLinkCommand, OnExecutedPasteImageLinkCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.OpenImagesFolderCommand, OnExecutedOpenImagesFolderCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.ChangeTemplateVersionCommand, OnExecutedChangeTemplateVersionCommand, OnCanExecuteChangeTemplateVersionCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditTestCorrectionsCommand, OnExecutedEditTestCorrectionsCommand, OnCanExecuteEditTestCorrectionsCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditUsedInTestVariablesListCommand, OnExecutedEditUsedInTestVariablesListCommand, OnCanExecuteEditUsedInTestVariablesListCommand));
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditDefaultCardCommand, OnExecutedEditDefaultCardCommand));
        }

        #region SaveTemplateCommand
        private void OnCanExecuteSaveTemplateCommand(object sender, CanExecuteRoutedEventArgs e)
        {        
            e.CanExecute = Modified;
        }

        private void OnExecutedSaveTemplateCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Save();            
        }
        #endregion

        #region RenameTemplateCommand
        private void OnCanExecuteRenameTemplateCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Model.IsUsed;
        }

        private void OnExecutedRenameTemplateCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TestTemplateNameDialog nameDialog = new TestTemplateNameDialog(Model.Name, Model.GUID);
            if (nameDialog.ShowDialog() == true)
            {
                if (Model.Name != nameDialog.EditedValue)
                {
                    Model.Name = nameDialog.EditedValue;
                    Modified = true;
                }
            }
        }
        #endregion

        #region ExportTemplateCommand

        private void OnExecutedExportTemplateCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.DefaultExt = "dattmp";
            saveDialog.Filter = Properties.Resources.DATTemplateFilter;            
            if (saveDialog.ShowDialog() == true)
            {
                Model.Export(saveDialog.FileName);
            }
        }
        #endregion

        #region SaveTemplateInDBCommand
        private void OnCanExecuteSaveTemplateInDBCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Model.IsUsed;
        }
        private void OnExecutedSaveTemplateInDBCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (NSMessageBox.Show(string.Format(Properties.Resources.ConfirmExportToDB, Model.Name, Model.VersionString, Globals.CurrentConnection.Connection.DbPath), Properties.Resources.Confirm, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Model.SaveInDatabase(Globals.CurrentConnection.Connection);
            }
        }
        #endregion

        #region TestTemplateRemoveCommand
        private void OnCanExecuteRemoveNodeCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItem != null && !Model.IsUsed;
        }

        private void OnExecutedRemoveNodeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = SelectedItem;
            if (selected != null)
            {
                string message = SelectedItem is DATTemplateFolder ? Properties.Resources.ConfirmRemoveFolder : Properties.Resources.ConfirmRemoveTest;
                if (NSMessageBox.Show(string.Format(message, selected.Name), Properties.Resources.Confirm, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    selected.Remove();
                    if (selected.Contains(CurrentTest))
                    {
                        CurrentTest = null;
                    }
                    if (selected.Contains(SelectedItem))
                    {
                        SelectedItem = null;
                    }
                    Modified = true;
                }
            }
        }
        #endregion

        #region RenameNodeCommand
        private void OnCanExecuteRenameNodeCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItem != null;
        }

        private void OnExecutedRenameNodeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.Rename())
                {
                    Modified = true;
                }
            }
        }
        #endregion

        #region ChangeTestIdCommand
        private void OnCanExecuteChangeTestIdCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItem is TestTemplateItem && !Model.IsUsed;
        }

        private void OnExecutedChangeTestIdCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var testItem = SelectedItem as TestTemplateItem;
            if (testItem != null)
            {
                TestItemInfoDialog dialog = new TestItemInfoDialog(testItem);
                if (dialog.ShowDialog() == true)
                {
                    string oldTestId = testItem.TestId;
                    testItem.TestId = dialog.TestId;
                    foreach (var test in Model.GetAllTests())
                    {
                        if (test.PreviousTestId == oldTestId)
                        {
                            test.PreviousTestId = testItem.TestId;
                        }
                        for (int i = 0; i < test.TestDependencesIdList.Count; i++)
                        {
                            if (test.TestDependencesIdList[i] == oldTestId)
                            {
                                test.TestDependencesIdList[i] = testItem.TestId;
                            }
                        }
                    }
                    testItem.Name = dialog.TestName;                    
                }
            }
        }
        #endregion
        
        #region ChangeIsContainerCommand

        private void OnCanExecuteChangeIsContainerCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Model.IsUsed;
        }

        private void OnExecutedChangeIsContainerCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TestTemplateItem selected = e.Parameter as TestTemplateItem;
            if (selected != null)
            {
                selected.IsContainer = !selected.IsContainer;
            }
        }
        #endregion

        #region AddTestCommand
        private void OnCanExecuteAddTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Model.IsUsed;
        }

        private void OnExecutedAddTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TestTemplateItem test = Model.CreateNewTestItem(null);
            if (test == null)
                return;
            DATTemplateTreeViewItem parent = e.Parameter as DATTemplateTreeViewItem;
            AddNode(test, parent);
            if (parent != null)
            {
                parent.IsExpanded = true;
            }
            CurrentTest = test;
        }
        #endregion


        #region OpenTestCommand

        private void OnCanExecuteOpenTestCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItem is TestTemplateItem;
        }
        private void OnExecutedOpenTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentTest = SelectedItem as TestTemplateItem;            
        }
        #endregion

        #region MoveUpNodeCommand
        private void OnCanExecuteMoveUpNodeCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItem != null && SelectedItem.CanMoveUp && !Model.IsUsed;
        }

        private void OnExecutedMoveUpNodeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedItem.MoveUp();
        }
        #endregion

        #region MoveDownNodeCommand
        private void OnCanExecuteMoveDownNodeCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItem != null && SelectedItem.CanMoveDown && !Model.IsUsed;
        }

        private void OnExecutedMoveDownNodeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedItem.MoveDown();
        }
        #endregion      

        #region EditDATVariablesCommand
        private void OnExecutedEditDATVariablesCommand(object sender, ExecutedRoutedEventArgs e)
        {
            EditDATVariablesDialog dialog = new EditDATVariablesDialog(Model);
            if (dialog.ShowDialog() == true)
            {
                Modified = true;
            }
        }
        #endregion

        #region ValidateScriptsCommand

        private void OnExecutedValidateScriptsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TemplateValidationWindow validationWindow = new TemplateValidationWindow(Model, ScriptTags);
            validationWindow.ShowDialog();
        }
        #endregion

        #region PreviewCommand
        private void OnExecutedPreviewCommand(object sender, ExecutedRoutedEventArgs e)
        {
            PreviewTemplateWindow preview = new PreviewTemplateWindow(CurrentTest);
            preview.ShowDialog();
        }
        #endregion

        #region EditValidationCommand
        private void OnExecutedEditValidationCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentTest == null)
                return;            
            EditValidationWindow editValidationDialog = new EditValidationWindow(CurrentTest, ScriptTags);
            editValidationDialog.ShowDialog();            
        }
        #endregion

        #region EditBeginTestScriptCommand
        private void OnExecutedEditBeginTestScriptCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentTest == null)
                return;            
            EditScriptWindow editScriptDialog = new EditScriptWindow(CurrentTest.BeginTestScript, ScriptTags);
            editScriptDialog.ShowDialog();
        }
        #endregion

        #region EditEndTestScriptCommand
        private void OnExecutedEditEndTestScriptCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentTest == null)
                return;
            EditScriptWindow editScriptDialog = new EditScriptWindow(CurrentTest.EndTestScript, ScriptTags);
            editScriptDialog.ShowDialog();
        }
        #endregion
        #region SelectPreviousTestCommand
        private void OnExecutedSelectPreviousTestCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var test = e.Parameter as TestTemplateItem;
            if (test == null)
                return;
            SelectPreviousTestDialog dialog = new SelectPreviousTestDialog(test);
            if (dialog.ShowDialog() == true)
            {
                Modified = true;
            }
        }
        #endregion

        #region CreateDATFolderCommand
        private void OnCanExecuteCreateDATFolderCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Model.IsUsed;
        }

        private void OnExecutedCreateDATFolderCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RenameNodeDialog dialog = new RenameNodeDialog(Properties.Resources.DefaultFolderName);
            if (dialog.ShowDialog() == true)
            {
                DATTemplateFolder folder = new DATTemplateFolder();
                folder.Name = dialog.EditedValue;
                AddNode(folder, e.Parameter as DATTemplateTreeViewItem);                
            }
        }
        #endregion

        #region TestScriptUsingsCommand

        private void OnExecutedTestScriptUsingsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentTest == null)
                return;
            ScriptInfo scriptInfo = CurrentTest.ExecutionScript;
            var dialog = new ScriptUsingsListDialog(scriptInfo.ScriptUsings);
            if (dialog.ShowDialog() == true)
            {
                scriptInfo.ScriptUsings.Clear();
                foreach (var scriptUsing in dialog.Usings)
                {
                    scriptInfo.ScriptUsings.Add(scriptUsing);
                }
                Modified = true;
            }
        }
        #endregion

        #region ContentPresenterTagsCommand

        private void OnExecutedContentPresenterTagsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentTest == null)
                return;
            ContentPresentersDialog dialog = new ContentPresentersDialog(CurrentTest);            
            if (dialog.ShowDialog() == true)
            {            
                Modified = true;
            }
        }
        #endregion

        #region ButtonScriptsCommand

        private void OnExecutedButtonScriptsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentTest == null)
                return;
            ButtonScriptsWindow dialog = new ButtonScriptsWindow(CurrentTest, ScriptTags);
            dialog.ShowDialog();
        }
        #endregion

        #region EditTestGroupsCommand

        private void OnExecutedEditTestGroupsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            EditTestGroupsDialog dialog = new EditTestGroupsDialog(Model);
            dialog.ShowDialog();
        }
        #endregion

        #region AutoTestSettingsCommand

        private void OnExecutedAutoTestSettingsCommandCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentTest == null)
                return;
            AutoTestSettingsDialog dialog = new AutoTestSettingsDialog(CurrentTest);
            dialog.ShowDialog();
        }
        #endregion

        #region PasteImageLinkCommand

        private void OnExecutedPasteImageLinkCommand(object sender, ExecutedRoutedEventArgs e)
        {
            string imageFileName = Convert.ToString(e.Parameter);
            try
            {
                Image image = new Image();
                Uri imageSourceUri = new Uri(Model.ImagesPath + "/" + imageFileName, UriKind.Relative);
                BitmapImage imageSource = new BitmapImage(imageSourceUri);
                image.BeginInit();
                image.Source = imageSource;
                image.EndInit();
                Selection.Text = "";                
                new InlineUIContainer(image, Selection.Start);
                var parentRTB = ProtocolPatternMakerHelper.FindAncestor<DATRichTextBox>(image);
                if (parentRTB != null)
                {
                    parentRTB.AddResizeAdornerToImage(image);
                }
            }
            catch { }
        }
        #endregion

        #region OpenImagesFolderCommand
        private void OnExecutedOpenImagesFolderCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Directory.Exists(Model.ImagesPath))
            {
                Directory.CreateDirectory(Model.ImagesPath);
            }
            Process.Start("explorer.exe", Model.ImagesPath);
        }
        #endregion

        #region ChangeTemplateVersionCommand
        private void OnCanExecuteChangeTemplateVersionCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !Model.IsUsed;
        }
        private void OnExecutedChangeTemplateVersionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (NSMessageBox.Show(string.Format(Properties.Resources.ConfirmIncreaseTemplateVersion, Model.Name),
                Properties.Resources.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Model.Version++;
                Modified = true;
            }
        }
        #endregion

        #region EditTestCorrectionsCommand
        private void OnCanExecuteEditTestCorrectionsCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentTest != null;
        }
        private void OnExecutedEditTestCorrectionsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            EditTestCorrectionsDialog dialog = new EditTestCorrectionsDialog(CurrentTest);
            if (dialog.ShowDialog() == true)
            {                
                Modified = true;
            }
        }
        #endregion

        #region EditUsedInTestVariablesListCommand
        private void OnCanExecuteEditUsedInTestVariablesListCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentTest != null && !CurrentTest.IsContainer;
        }
        private void OnExecutedEditUsedInTestVariablesListCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new EditUsedTestVarsListDialog(CurrentTest);
            if (dialog.ShowDialog() == true)
            {
                Modified = true;
            }
        }

        #endregion

        #region OnExecutedEditDefaultCardCommand
        private void OnExecutedEditDefaultCardCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new EditDefaultCardDialog(Model.DefaultCardInfoPath);
            if (dialog.ShowDialog() == true)
            {
                Model.DefaultCardInfoPath = dialog.DefaultCardPath;
            }
        }
        #endregion
        #region ProtocolPatternsCommand

        private void OnExecutedProtocolPatternsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MainModel.Current.ShowProtocolTemplates();
        }

        #endregion

        private void AddNode(DATTemplateTreeViewItem node, DATTemplateTreeViewItem insertInto)
        {
            if (node == null)
                return;
            if (SelectedItem == null)
            {
                Model.AddNode(node);
            }
            else
            {
                if (insertInto != null && insertInto.CanContainsInnerNodes)
                {
                    SelectedItem.AddNode(node);
                }
                else
                {
                    var parent = SelectedItem.Parent;
                    int index = parent.Nodes.IndexOf(SelectedItem) + 1;
                    if (index > parent.Nodes.Count - 1)
                    {
                        parent.AddNode(node);
                    }
                    else
                    {
                        parent.InsertNode(index, node);
                    }
                }
            }
            node.IsCurrent = true;
            Modified = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Сохранение шаблона
        /// </summary>
        public void Save()
        {
            if (Model.IsUsed)
                return;
            if (Modified)
            {                
                Model.Save();
            }
            Modified = false;
        }

        /// <summary>
        /// Действия при изменении содержимого теста
        /// </summary>
        /// <param name="e"></param>
        internal void OnContentTextChanged(System.Windows.Controls.TextChangedEventArgs e)
        {
            Modified = true;
        }

        /// <summary>
        /// Вставка тега 
        /// </summary>
        /// <param name="tag"></param>
        public void InsertTagLink(ITag tag)
        {
            if (tag == null)
                return;
            Selection.Text = tag.Prefix + tag.Name;
            Selection.Select(Selection.End, Selection.End);
        }

        internal void CheckNewCorrections()
        {
            NewCorrectionsDialog dialog = new NewCorrectionsDialog(Model, Globals.CurrentConnection.Connection);
            dialog.ShowDialog();
            OnPropertyChanged("NewCorrectionsCount");
        }

        #endregion

        #region Content Selection

        private TextRange selection;
        /// <summary>
        /// Выделенный фрагмент документа
        /// </summary>
        public TextRange Selection
        {
            get { return selection; }
            set
            {
                selection = value;
                OnSelectionChanged();
            }
        }

        private bool selectionIsBold;
        /// <summary>
        /// Признак жирности выделенного текста
        /// </summary>
        public bool SelectionIsBold
        {
            get { return selectionIsBold; }
            set
            {
                selectionIsBold = value;
                OnPropertyChanged("SelectionIsBold");
            }
        }

        private bool selectionIsItalic;
        /// <summary>
        /// Признак наклонности выделенного текста
        /// </summary>
        public bool SelectionIsItalic
        {
            get { return selectionIsItalic; }
            set
            {
                selectionIsItalic = value;
                OnPropertyChanged("SelectionIsItalic");
            }
        }

        private bool selectionIsUnderline;
        /// <summary>
        /// Признак подчеркнутости выделенного текста
        /// </summary>
        public bool SelectionIsUnderline
        {
            get { return selectionIsUnderline; }
            set
            {
                selectionIsUnderline = value;
                OnPropertyChanged("SelectionIsUnderline");
            }
        }

        private bool selectionIsLeftAlignment;
        /// <summary>
        /// Признак выравнивания по левому краю выделенного текста
        /// </summary>
        public bool SelectionIsLeftAlignment
        {
            get { return selectionIsLeftAlignment; }
            set
            {
                selectionIsLeftAlignment = value;
                OnPropertyChanged("SelectionIsLeftAlignment");
            }
        }

        private bool selectionIsCenterAlignment;
        /// <summary>
        /// Признак выравнивания по центру выделенного текста
        /// </summary>
        public bool SelectionIsCenterAlignment
        {
            get { return selectionIsCenterAlignment; }
            set
            {
                selectionIsCenterAlignment = value;
                OnPropertyChanged("SelectionIsCenterAlignment");
            }
        }

        private bool selectionIsRightAlignment;
        /// <summary>
        /// Признак выравнивания по правому краю выделенного текста
        /// </summary>
        public bool SelectionIsRightAlignment
        {
            get { return selectionIsRightAlignment; }
            set
            {
                selectionIsRightAlignment = value;
                OnPropertyChanged("SelectionIsRightAlignment");
            }
        }

        private bool selectionIsList;
        /// <summary>
        /// Признак списка в выделении
        /// </summary>
        public bool SelectionIsList
        {
            get { return selectionIsList; }
            set
            {
                selectionIsList = value;
                OnPropertyChanged("SelectionIsList");
            }
        }

        /// <summary>
        /// События при изменении выделения
        /// </summary>
        private void OnSelectionChanged()
        {
            OnPropertyChanged("Selection");
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                selectedFontFamily = GetPropertyValue(selection, TextElement.FontFamilyProperty) as FontFamily;
                try
                {
                    fontSize = (double)GetPropertyValue(selection, TextElement.FontSizeProperty) * 72d / DpiX;
                    fontSize = Math.Round(fontSize, 1);
                }
                catch
                {
                    fontSize = -1;
                }
                OnPropertyChanged("SelectedFontSize");
                OnPropertyChanged("SelectedFontFamily");
                UpdateSelectionTextAlignment();
            }), DispatcherPriority.Background, null);
            SetNeedUpdateChecked();
        }

        private TextAlignment? SelectionTextAlignment;
        private void UpdateSelectionTextAlignment()
        {
            SelectionTextAlignment = GetPropertyValue(Selection, Block.TextAlignmentProperty) as TextAlignment?;
        }
        private FontFamily selectedFontFamily;
        /// <summary>
        /// Шрифт выделенного фрагмента
        /// </summary>
        public FontFamily SelectedFontFamily
        {
            get
            {                
                return selectedFontFamily;
            }
            set
            {
                selectedFontFamily = value;                
                SetPropertyValue(Selection, TextElement.FontFamilyProperty, value);
                OnPropertyChanged("SelectedFontFamily");
            }
        }

        private static List<FontFamily> fontFamilies;
        /// <summary>
        /// Список доступных шрифтов
        /// </summary>
        public static List<FontFamily> FontFamilies
        {
            get
            {
                if (fontFamilies == null)
                {
                    fontFamilies = Fonts.SystemFontFamilies.ToList<FontFamily>();
                    fontFamilies.Sort(new Comparison<FontFamily>((FontFamily left, FontFamily right) =>
                    {
                        return left.Source.CompareTo(right.Source);
                    }));
                }
                return fontFamilies;
            }
        }

        double[] fontSizes = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        /// <summary>
        /// Список доступных размеров шрифта
        /// </summary>
        public double[] FontSizes
        {
            get { return fontSizes; }
        }

        #region DPI
        private static double dpiX = -1;
        private static double dpiY = -1;
        /// <summary>
        /// DpiX
        /// </summary>
        /// <returns></returns>
        public static double DpiX
        {
            get
            {
                if (dpiX < 0)
                {
                    try
                    {
                        dpiX = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M11 * 96d;
                    }
                    catch
                    {
                        dpiX = 96;
                    }
                }
                return dpiX;
            }
        }

        /// <summary>
        /// DpiY
        /// </summary>
        /// <returns></returns>
        public static double DpiY
        {
            get
            {
                if (dpiY < 0)
                {
                    try
                    {
                        dpiY = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M22 * 96d;
                    }
                    catch
                    {
                        dpiY = 96;
                    }
                }
                return dpiY;
            }
        }

        private static double screenResolution = -1d;

        /// <summary>
        /// Разрешение экрана
        /// </summary>
        internal static double ScreenResolution
        {
            get
            {
                if (screenResolution <= 0)
                {
                    screenResolution = DpiX / 25.4d;
                }
                return screenResolution;
            }
        }
        #endregion

        private double fontSize;
        /// <summary>
        /// Размер шрифта выделенного фрагмента
        /// </summary>
        public double SelectedFontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                fontSize = value;
                double appliedSize = (double)new FontSizeConverter().ConvertFrom(value + "pt");
                SetPropertyValue(Selection, TextElement.FontSizeProperty, appliedSize);
                //Selection.ApplyPropertyValue(TextElement.FontSizeProperty, value);
                OnPropertyChanged("SelectedFontSize");
            }
        }

        private void setFontFamilyForTextRange(TextRange range, FontFamily fontFamily)
        {
            Span span = new Span(range.Start, range.End);
            span.FontFamily = fontFamily;
        }

        /// <summary>
        /// Разрешить обновление состояния кнопок ToggleButton
        /// </summary>
        private void SetNeedUpdateChecked()
        {
            needUpdateBoldCheked = true;
            needUpdateBulletsCheked = true;
            needUpdateItalicCheked = true;
            needUpdateUnderlineCheked = true;
            setNeedAlignUpdateChecked();
        }

        /// <summary>
        /// Разрешить обновление состояния кнопок, отвечающих за выравнивание текста
        /// </summary>
        private void setNeedAlignUpdateChecked()
        {
            needUpdateCenterAlignCheked = true;
            needUpdateLeftAlignCheked = true;
            needUpdateRightAlignCheked = true;
        }

        private void UpdateCheckedCommandSource(object obj, bool? isCommandChecked)
        {
            if (obj is ToggleButton && isCommandChecked.HasValue)
            {
                (obj as ToggleButton).IsChecked = isCommandChecked.Value;
            }
        }

        /// <summary>
        /// Определяет тип маркирования указанной области
        /// </summary>
        /// <returns></returns>
        private TextMarkerStyle? GetListTypeForTextRange(TextRange range)
        {
            return ProtocolPatternMakerHelper.GetListTypeForTextRange(range);
        }

        private void SetPropertyValue(TextRange range, DependencyProperty property, object value)
        {
            ProtocolPatternMakerHelper.SetPropertyValue(range, property, value);
        }

        private object GetPropertyValue(TextRange range, DependencyProperty property)
        {
            return ProtocolPatternMakerHelper.GetPropertyValue(range, property);
        }

        private bool IsUnderline(TextRange range)
        {
            return ProtocolPatternMakerHelper.IsUnderline(range);
        }

        #region TextAlignment
        private bool needUpdateLeftAlignCheked = false;

        private void OnCanExecuteFormatLeftAlignCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (needUpdateLeftAlignCheked)
            {
                needUpdateLeftAlignCheked = false;
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    SelectionIsLeftAlignment = TextRangeAlignIs(Selection, TextAlignment.Left);
                }), DispatcherPriority.Background, null);
            }
            e.CanExecute = EditingCommands.AlignLeft.CanExecute(e.Parameter, null);
        }

        private void OnExecutedFormatLeftAlignCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SetSelectionAlignment(TextAlignment.Left, e);
        }

        private bool needUpdateCenterAlignCheked = false;

        private void OnCanExecuteFormatCenterAlignCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (needUpdateCenterAlignCheked)
            {
                needUpdateCenterAlignCheked = false;
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    SelectionIsCenterAlignment = TextRangeAlignIs(Selection, TextAlignment.Center);
                }), DispatcherPriority.Background, null);
            }
            e.CanExecute = EditingCommands.AlignCenter.CanExecute(e.Parameter, null);
        }

        private void OnExecutedFormatCenterAlignCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SetSelectionAlignment(TextAlignment.Center, e);
        }

        private bool needUpdateRightAlignCheked = false;
        private void OnCanExecuteFormatRightAlignCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (needUpdateRightAlignCheked)
            {
                needUpdateRightAlignCheked = false;
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    SelectionIsRightAlignment = TextRangeAlignIs(Selection, TextAlignment.Right);
                }), DispatcherPriority.Background, null);
            }
            e.CanExecute = EditingCommands.AlignRight.CanExecute(e.Parameter, null);
        }

        private void OnExecutedFormatRightAlignCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SetSelectionAlignment(TextAlignment.Right, e);
        }

        private void SetSelectionAlignment(TextAlignment textAlignment, ExecutedRoutedEventArgs e)
        {
            if (TextRangeAlignIs(Selection, textAlignment))
            {
                EditingCommands.AlignJustify.Execute(e.Parameter, null);
                SelectionTextAlignment = TextAlignment.Justify;
            }
            else
            {
                switch (textAlignment)
                {
                    case TextAlignment.Left:
                        EditingCommands.AlignLeft.Execute(e.Parameter, null);
                        break;
                    case TextAlignment.Center:
                        EditingCommands.AlignCenter.Execute(e.Parameter, null);
                        break;
                    case TextAlignment.Right:
                        EditingCommands.AlignRight.Execute(e.Parameter, null);
                        break;
                    default:
                        EditingCommands.AlignJustify.Execute(e.Parameter, null);
                        break;
                }
                SelectionTextAlignment = textAlignment;
            }
            setNeedAlignUpdateChecked();
        }

        private bool TextRangeAlignIs(TextRange range, TextAlignment align)
        {
            TextAlignment? textAlignment = SelectionTextAlignment;
            return (textAlignment.HasValue && textAlignment.Value == align);
        }
        #endregion

        #region ToggleBold
        private bool needUpdateBoldCheked = false;
        private void OnCanExecuteToggleBoldCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (needUpdateBoldCheked)
            {
                needUpdateBoldCheked = false;
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    FontWeight? fontWeight = GetPropertyValue(Selection, TextElement.FontWeightProperty) as FontWeight?;
                    SelectionIsBold = (fontWeight.HasValue && fontWeight.Value == FontWeights.Bold);
                }), DispatcherPriority.Background, null);
            }
            e.CanExecute = EditingCommands.ToggleBold.CanExecute(e.Parameter, null);
        }


        private void OnExecutedToggleBoldCommand(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleBold.Execute(e.Parameter, null);
            needUpdateBoldCheked = true;
        }
        #endregion

        #region ToggleItalic
        private bool needUpdateItalicCheked = false;
        private void OnCanExecuteToggleItalicCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (needUpdateItalicCheked)
            {
                needUpdateItalicCheked = false;
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    FontStyle? fontStyle = GetPropertyValue(Selection, TextElement.FontStyleProperty) as FontStyle?;
                    SelectionIsItalic = (fontStyle.HasValue && fontStyle.Value == FontStyles.Italic);
                }), DispatcherPriority.Background, null);
            }
            e.CanExecute = EditingCommands.ToggleItalic.CanExecute(e.Parameter, null);
        }

        private void OnExecutedToggleItalicCommand(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleItalic.Execute(e.Parameter, null);
            needUpdateItalicCheked = true;
        }
        #endregion

        #region ToggleUnderline
        private bool needUpdateUnderlineCheked = false;
        private void OnCanExecuteToggleUnderlineCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (needUpdateUnderlineCheked)
            {
                needUpdateUnderlineCheked = false;
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    SelectionIsUnderline = IsUnderline(Selection);
                }), DispatcherPriority.Background, null);
            }
            e.CanExecute = EditingCommands.ToggleUnderline.CanExecute(e.Parameter, null);
        }

        private void OnExecutedToggleUnderlineCommand(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleUnderline.Execute(e.Parameter, null);
            needUpdateUnderlineCheked = true;
        }
        #endregion

        #region ToggleBullets
        private bool needUpdateBulletsCheked = false;
        private void OnCanExecuteToggleBulletsCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (needUpdateBulletsCheked)
            {
                needUpdateBulletsCheked = false;
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    TextMarkerStyle? markerStyle = GetListTypeForTextRange(Selection);
                    SelectionIsList = (markerStyle.HasValue && markerStyle.Value == TextMarkerStyle.Disc);
                }), DispatcherPriority.Background, null);
            }
            e.CanExecute = EditingCommands.ToggleBullets.CanExecute(e.Parameter, null);
        }

        private void OnExecutedToggleBulletsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            EditingCommands.ToggleBullets.Execute(e.Parameter, null);
        }
        #endregion

        #region EditColor
        private void OnCanExecuteEditColorCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteTextFormat();
        }

        private void OnExecutedEditColorCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Color? color = e.Parameter as Color?;
            if (color.HasValue)
            {
                Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color.Value));
            }
        }
        #endregion

        #region EditBackColor
        private void OnCanExecuteEditBackColorCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteTextFormat();
        }

        private void OnExecutedEditBackColorCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Color? color = e.Parameter as Color?;
            if (color.HasValue)
            {
                Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color.Value));
            }
        }

        /// <summary>
        /// Признак возможности форматирования текста
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteTextFormat()
        {
            //Исходим из того, что форматировать текст можно тогда и только тогда,
            //когда можно сделать шрифт жирным
            return EditingCommands.ToggleBold.CanExecute(null, null);
        }
        #endregion
        #endregion
    }
}
