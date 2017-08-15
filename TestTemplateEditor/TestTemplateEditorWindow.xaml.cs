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
using NeuroSoft.WPFComponents.ScalableWindows;
using System.Windows.Threading;
using NeuroSoft.DeviceAutoTest.Commands;
using NeuroSoft.WPFComponents.CommandManager;
using System.Windows.Controls.Primitives;
using NeuroSoft.DeviceAutoTest.Dialogs;

namespace NeuroSoft.DeviceAutoTest.TestTemplateEditor
{
    /// <summary>
    /// Interaction logic for TestTemplateEditorWindow.xaml
    /// </summary>
    public partial class TestTemplateEditorWindow : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public TestTemplateEditorWindow(TestTemplateViewModel testTemplateViewModel)
        {
            InitializeComponent();
            ViewModel = testTemplateViewModel;
            NavigationGrid.CommandBindings.AddRange(ViewModel.NavigationCommandBindings);
            
            ViewModel.CurrentTestChanged += new RoutedEventHandler(ViewModel_CurrentTestChanged);
            Loaded += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
            {
                OpenTestContent(ViewModel.SelectedItem as TestTemplateItem);
            });            
        }        

        #region Properties
        private TestTemplateViewModel viewModel;

        /// <summary>
        /// Модель представления
        /// </summary>
        public TestTemplateViewModel ViewModel
        {
            get { return viewModel; }
            set 
            {
                if (viewModel != value)
                {
                    viewModel = value;
                    DataContext = viewModel;
                    CommandBindings.Clear();
                    CommandBindings.AddRange(ViewModel.CommandBindings);
                    OnPropertyChanged("ViewModel");
                }
            }
        }        
        #endregion 

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);            
            if (DialogResult != true && ViewModel.Modified && !ViewModel.Model.IsUsed)
            {
                MessageBoxResult result = NSMessageBox.Show(Properties.Resources.SaveModifiedTemplate, "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ViewModel.Save();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ignoreModified = true;
            try
            {
                TestContentRTB.Document = new FlowDocument();
            }
            finally
            {
                ignoreModified = false;
            }
        }
        private void TestContentRTB_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.Selection = TestContentRTB.Selection;
        }

        private void TestContentRTB_TextChanged(object sender, TextChangedEventArgs e)
        {            
            if (!ignoreModified)
            {
                if (ViewModel.CurrentTest != null)
                {
                    ViewModel.CurrentTest.TestContentChanged = true;
                }
                ViewModel.Modified = true;
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedItem = TreeView.SelectedItem as DATTemplateTreeViewItem;
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {            
            if (TreeView.SelectedItem != null)
            {
                OpenTestContent(TreeView.SelectedItem as TestTemplateItem);
                e.Handled = true;
            }
        }

        void ViewModel_CurrentTestChanged(object sender, RoutedEventArgs e)
        {
            ignoreModified = true;
            try
            {
                TestContentRTB.Document = new FlowDocument();
                if (ViewModel.CurrentTest != null)
                {
                    TestContentRTB.SetDocument(ViewModel.CurrentTest.ContentDocument);
                }
                ViewModel.Selection = new TextRange(TestContentRTB.Document.ContentStart, TestContentRTB.Document.ContentStart);
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    TestContentRTB.Focus();
                }));
            }
            finally
            {
                ignoreModified = false;
            }
        }

        private bool ignoreModified = false;
        internal void OpenTestContent(TestTemplateItem testItem)
        {
            if (testItem == null)
                return;
            if (ViewModel.CurrentTest == testItem)
                return;
            ViewModel.CurrentTest = testItem;
        }

        private void TagTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.InsertTagLink((sender as TextBlock).DataContext as ITag);
        }        

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem)
            {
                (sender as TreeViewItem).IsSelected = true;
            }
        }

        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {            
            e.Handled = true;            
        }

        #region VariablesPopup
                
        RichTextBox _editorForPaste = null;
        /// <summary>
        /// Обработка события нажатия клавиши на списке переменных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagsListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    InsertTagFromPopup(TagsListBox.SelectedItem as ITag);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    closeTagsPopup();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Вставка тега
        /// </summary>
        /// <param name="variable"></param>
        private void InsertTagFromPopup(ITag tag)
        {
            closeTagsPopup();
            if (tag != null)
            {
                ViewModel.InsertTagLink(tag);
            }
        }

        /// <summary>
        /// Закрытие всплывающего окна со списком доступных тегов
        /// </summary>
        private void closeTagsPopup()
        {
            VariablesPopup.IsOpen = false;
            if (_editorForPaste != null)
            {
                _editorForPaste.Focus();
            }
        }

        private void TagsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InsertTagFromPopup(TagsListBox.SelectedItem as ITag);
        }

        private void TestContentRTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control && Keyboard.FocusedElement is RichTextBox)
            {
                OpenTagsPopup(sender as RichTextBox, true);
            }
        }

        private void OpenTagsPopup(RichTextBox editor, bool insertVarLink)
        {            
            if (ViewModel.TestContentTags.Count == 0)
                return;
            ViewModel.NotifyTagsChanged();
            _editorForPaste = editor;
            VariablesPopup.PlacementTarget = _editorForPaste;
            VariablesPopup.PlacementRectangle = _editorForPaste.CaretPosition.GetCharacterRect(LogicalDirection.Forward);            
            VariablesPopup.IsOpen = true;

            ListBoxItem firstItem = TagsListBox.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
            if (firstItem != null)
            {
                firstItem.Focus();
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    firstItem.IsSelected = true;
                }), DispatcherPriority.Render);
            }
        }
        #endregion        

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel.InvalidateImagesFromFiles();
        }

        private void CheckCorrectionsLink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CheckNewCorrections();
        }      
          
    }
}
