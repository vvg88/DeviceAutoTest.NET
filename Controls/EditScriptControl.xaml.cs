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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using NeuroSoft.DeviceAutoTest.Dialogs;
using System.Collections;

namespace NeuroSoft.DeviceAutoTest.Controls
{
    /// <summary>
    /// Interaction logic for EditScriptControl.xaml
    /// </summary>
    public partial class EditScriptControl : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public EditScriptControl()
        {
            InitializeComponent();                     
        }
        
        /// <summary>
        /// Редактируемый скрипт
        /// </summary>
        public ScriptInfo ScriptObject
        {
            get { return (ScriptInfo)GetValue(ScriptObjectProperty); }
            set { SetValue(ScriptObjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Script.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScriptObjectProperty =
            DependencyProperty.Register("ScriptObject", typeof(ScriptInfo), typeof(EditScriptControl));


        /// <summary>
        /// Список тегов
        /// </summary>
        public IList Tags
        {
            get { return (IList)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(IList), typeof(EditScriptControl), new FrameworkPropertyMetadata(null, OnTagsChanged));

        private static void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EditScriptControl).OnTagsChanged(e);
        }

        private void OnTagsChanged(DependencyPropertyChangedEventArgs e)
        {
            List<ITag> tagList = new List<ITag>();
            foreach (var item in Tags)
            {
                if (item is ITag)
                {
                    tagList.Add(item as ITag);
                }
            }
            ScriptTextBox.DataProvider = new TagsDataProvider(tagList) { IgnorePrefix = true };
            ScriptTextBox.AutoCompleteManager.AutoUpdateText = false;
        }
                      
        private void UsingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var scriptInfo = ScriptObject;
            var dialog = new ScriptUsingsListDialog(scriptInfo.ScriptUsings);
            if (dialog.ShowDialog() == true)
            {
                scriptInfo.ScriptUsings.Clear();
                foreach (var scriptUsing in dialog.Usings)
                {
                    scriptInfo.ScriptUsings.Add(scriptUsing);
                }
                scriptInfo.NotifyModified();
            }
        }

        private void AssembliesBtn_Click(object sender, RoutedEventArgs e)
        {
            var scriptInfo = ScriptObject;
            var dialog = new ScriptAssembliesListDialog(scriptInfo.Assemblies);
            if (dialog.ShowDialog() == true)
            {
                scriptInfo.Assemblies.Clear();
                foreach (var scriptAssembly in dialog.Assemblies)
                {
                    scriptInfo.Assemblies.Add(scriptAssembly);
                }
                scriptInfo.NotifyModified();
            }
        }

        private void ErrorsBtn_Click(object sender, RoutedEventArgs e)
        {
            ScriptClassGenerator scriptGenerator = new ScriptClassGenerator(ScriptObject);
                        
            foreach (var varDescr in ScriptObject.TemplateItemParent.TemplateParent.Variables)
            {
                scriptGenerator.Variables.Add(new DATVariable(varDescr));
            }
            foreach (var templateItem in ScriptObject.TemplateItemParent.TemplateParent.GetAllTests())
            {
                scriptGenerator.Tests.Add(new TestObject(null, templateItem));
            }
            scriptGenerator.Preview();
        }

        #region VariablesPopup

        /// <summary>
        /// Обработка события нажатия клавиши на списке переменных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VariablesListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    InsertVariableFromPopup(TagsListBox.SelectedItem as ITag);
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
        /// <param name="tag"></param>
        private void InsertVariableFromPopup(ITag tag)
        {
            closeTagsPopup();
            if (tag != null)
            {
                ScriptTextBox.SelectedText = tag.Name;
                ScriptTextBox.Select(ScriptTextBox.SelectionStart + ScriptTextBox.SelectionLength, 0);
            }
        }

        /// <summary>
        /// Закрытие всплывающего окна со списком доступных тегов
        /// </summary>
        private void closeTagsPopup()
        {            
            VariablesPopup.IsOpen = false;
            ScriptTextBox.Focus();
        }

        private void VariablesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InsertVariableFromPopup(TagsListBox.SelectedItem as ITag);
        }

        private void ScriptTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                //OpenVariablesPopup();
            }
        }

        private void OpenVariablesPopup()
        {
            if (Tags == null || Tags.Count == 0)
                return;
            BindingOperations.GetBindingExpression(this, TagsProperty).UpdateTarget();
            VariablesPopup.PlacementTarget = ScriptTextBox;

            VariablesPopup.PlacementRectangle = ScriptTextBox.GetRectFromCharacterIndex(ScriptTextBox.CaretIndex);
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
    }
}
