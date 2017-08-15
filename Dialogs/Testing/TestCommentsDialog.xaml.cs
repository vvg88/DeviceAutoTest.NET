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

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for TestCommentsDialog.xaml
    /// </summary>
    public partial class TestCommentsDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="commentsLabel"></param>
        public TestCommentsDialog(string caption, string commentsLabel)
        {
            InitializeComponent();
            Caption = caption;
            CommentsDescription = commentsLabel;
            DataContext = this;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            CommentsTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            CommentsTextBox.SelectAll();
            CommentsTextBox.Focus();
        }
        #region Properties

        private string comments;

        /// <summary>
        /// Комментарий
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
                }
            }
        }

        private string caption;

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Caption
        {
            get { return caption; }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    OnPropertyChanged("Caption");
                }
            }
        }

        private string commentsDescription;

        /// <summary>
        /// Описание комментария
        /// </summary>
        public string CommentsDescription
        {
            get { return commentsDescription; }
            set
            {
                if (commentsDescription != value)
                {
                    commentsDescription = value;
                    OnPropertyChanged("CommentsDescription");
                }
            }
        }
        #endregion         

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
