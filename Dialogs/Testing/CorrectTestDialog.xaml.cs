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
using NeuroSoft.Prototype.Database;
using System.Collections.ObjectModel;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.DeviceAutoTest.Controls;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for CorrectTestDialog.xaml
    /// </summary>
    public partial class CorrectTestDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="test"></param>
        /// <param name="newCorrections"></param>
        public CorrectTestDialog(TestObject test)
        {            
            InitializeComponent();
            Test = test;
            if (correctionsToAdd == null && test.AncestorParent != null && test.AncestorParent.TestTemplate != null)
            {
                correctionsToAdd = TemplateCorrections.Read(Globals.CurrentConnection.Connection, test.AncestorParent.TestTemplate.GUID.ToString());
            }
            DataContext = this;
            CommentsTextBox.DataProvider = new SimpleStaticDataProvider(test.TemplateItem.ProbableCorrections);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            CommentsTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            CommentsTextBox.SelectAll();
            CommentsTextBox.Focus();            
        }
        #region Properties

        private TestObject test;
        /// <summary>
        /// Тест
        /// </summary>
        public TestObject Test
        {
            get { return test; }
            private set
            {
                if (test != value)
                {
                    test = value;
                    if (test != null)
                    {
                        Comments = test.Comments;
                    }
                    OnPropertyChanged("TestObject");                    
                }
            }
        }

        private string comments;
        /// <summary>
        /// Описание комментария
        /// </summary>
        public string Comments
        {
            get { return comments; }
            set
            {
                if (comments != value)
                {
                    comments = value;
                    OnPropertyChanged("Comments");
                    OnPropertyChanged("CanSaveInList");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CanSaveInList
        {
            get 
            {
                if (CorrectionsToAdd == null || test == null)
                    return false;
                return !CorrectionsToAdd.Corrections.Contains(new CorrectionInfo(test.TestId, Comments)) &&
                    !Test.TemplateItem.ProbableCorrections.Contains(Comments);
            }
        }

        private bool saveInList;
        /// <summary>
        /// Отправить фразу на добавление разработчику 
        /// </summary>
        public bool SaveInList
        {
            get { return saveInList; }
            set
            {
                if (saveInList != value)
                {
                    saveInList = value;
                    OnPropertyChanged("SaveInList");
                }
            }
        }
        private TemplateCorrections correctionsToAdd;
        /// <summary>
        /// Список описаний исправлений для добавления в инструкцию
        /// </summary>
        public TemplateCorrections CorrectionsToAdd
        {
            get
            {                
                return correctionsToAdd;
            }
        }
        
        #endregion         

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DialogResult == true && Test != null)
            {
                Test.Comments = Comments;
                if (CanSaveInList && SaveInList)
                {
                    CorrectionsToAdd.AddCorrection(test.TestId, Comments);
                    CorrectionsToAdd.Save(Globals.CurrentConnection.Connection);
                    MainWindow.UpdateNewCorrectionsCount();
                }
            }
        }
    }
}
