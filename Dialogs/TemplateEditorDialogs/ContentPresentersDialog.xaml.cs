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
    /// Interaction logic for ContentPresentersDialog.xaml
    /// </summary>
    public partial class ContentPresentersDialog : DATDialogWindow
    {
        private ObservableCollection<ContentPresenterTag> tags = new ObservableCollection<ContentPresenterTag>();
        /// <summary>
        /// Список тегов
        /// </summary>
        public ObservableCollection<ContentPresenterTag> Tags
        {
            get { return tags; }
            private set { tags = value; }
        }

        private ObservableCollection<ContentPresenterTag> selectedTags = new ObservableCollection<ContentPresenterTag>();

        /// <summary>
        /// Выделенные теги
        /// </summary>
        public ObservableCollection<ContentPresenterTag> SelectedTags
        {
            get { return selectedTags; }
            private set { selectedTags = value; }
        }

        /// <summary>
        /// Список существующих имен тегов
        /// </summary>
        internal static List<string> ExistingNames { get; set; }

        private void UpdateExistingNamesList(ContentPresenterTag currentTag)
        {
            ExistingNames = new List<string>((from tag in TemplateItem.ContentPresenters
                                              where currentTag == null || tag.Name != currentTag.Name
                                              select tag.Name));
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
            DependencyProperty.Register("SelectionCount", typeof(int), typeof(ContentPresentersDialog), new FrameworkPropertyMetadata(0, null, SelectedCountCoerce));

        private static object SelectedCountCoerce(DependencyObject d, object baseValue)
        {            
            return (int)baseValue < 0 ? 0 : baseValue;
        }

        private TestTemplateItem TemplateItem;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="userTags"></param>
        /// <param name="existingAvailableTags"></param>
        public ContentPresentersDialog(TestTemplateItem templateItem)
        {
            InitializeComponent();
            TemplateItem = templateItem;
            Tags = new ObservableCollection<ContentPresenterTag>(from tag in templateItem.ContentPresenters select tag.Clone() as ContentPresenterTag);
            DataContext = this;
            selectedTags.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(selectedTags_CollectionChanged);                        
        }      

        void selectedTags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SelectionCount = SelectedTags.Count;
            if (!selectedTagsChanging)
            {
                foreach (var item in SelectedTags)
                {
                    if (!nsGrid.SelectedItems.Contains(item))
                    {
                        nsGrid.SelectedItems.Add(item);
                    }
                }
            }
        }
        private bool selectedTagsChanging = false;

        private void AddTagBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelEdit();
            string tagName = "contentPresenter";
            int i = 1;
            while (Tags.SingleOrDefault(tag => tag.Name == tagName) != null)
            {
                tagName = "contentPresenter" + i++;
            }
            tags.Add(new ContentPresenterTag(tagName) { Description = Properties.Resources.DefaultContentPresenterTagDescription });
        }
        
        private void DeleteTagBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelEdit();
            int index = nsGrid.SelectedIndex;// SelectedTags.Min(tag => UserTags.IndexOf(tag)); // UserTags.IndexOf(SelectedTag);
            List<ContentPresenterTag> wasSelected = new List<ContentPresenterTag>(SelectedTags);
            foreach (var selectedTag in wasSelected)
            {
                Tags.Remove(selectedTag);
            }
            if (index >= 0)
            {
                if (Tags.Count > 0)
                {
                    if (Tags.Count - 1 < index)
                    {
                        nsGrid.SelectedIndex = Tags.Count - 1;
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
            if ((string)e.Column.Header == "NameColumn")
            {
                UpdateExistingNamesList(e.Row.Item as ContentPresenterTag);
            }
        }
  
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            selectedTags.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(selectedTags_CollectionChanged);
            if (DialogResult == true)
            {
                TemplateItem.ContentPresenters.Clear();
                foreach (var tag in Tags)
                {
                    TemplateItem.ContentPresenters.Add(tag);
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
            selectedTagsChanging = true;
            try
            {
                foreach (var item in e.RemovedItems)
                {
                    if (item is ContentPresenterTag && SelectedTags.Contains(item as ContentPresenterTag))
                    {
                        SelectedTags.Remove(item as ContentPresenterTag);
                    }
                }
                foreach (var item in e.AddedItems)
                {
                    if (item is ContentPresenterTag)
                    {
                        SelectedTags.Add(item as ContentPresenterTag);
                    }
                }
            }
            finally
            {
                selectedTagsChanging = false;
            }
        }
    }


    /// <summary>
    /// Валидация имени тега
    /// </summary>
    public class ValidateContentePresenterTagName : ValidationRule
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
                ContentPresenterTag variable = new ContentPresenterTag((string)value);
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, ex.Message);
            }
            if (ContentPresentersDialog.ExistingNames.Contains((string)value))
            {
                return new ValidationResult(false, string.Format(Properties.Resources.TagAlreadyExists, value));
            }
            return new ValidationResult(true, null);
        }
    }
}
