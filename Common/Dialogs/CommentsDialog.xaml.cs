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
using System.Collections.ObjectModel;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for CommentsDialog.xaml
    /// </summary>
    public partial class CommentsDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="commentsLabel"></param>
        /// <param name="canCancel"></param>
        public CommentsDialog(string caption, string commentsLabel, string defaultValue = null, bool canCancel = true)
        {
            InitializeComponent();
            Caption = caption;
            CommentsLabel = commentsLabel;
            CanCancel = canCancel;
            if (defaultValue != null)
            {
                Comments = defaultValue;
            }
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

        private string commentsLabel;

        /// <summary>
        /// Описание комментария
        /// </summary>
        public string CommentsLabel
        {
            get { return commentsLabel; }
            set
            {
                if (commentsLabel != value)
                {
                    commentsLabel = value;
                    OnPropertyChanged("CommentsLabel");
                }
            }
        }

        private bool canCancel = true;

        /// <summary>
        /// Возможность отмены
        /// </summary>
        public bool CanCancel
        {
            get { return canCancel; }
            set
            {
                if (canCancel != value)
                {
                    canCancel = value;
                    OnPropertyChanged("CanCancel");
                }
            }
        }
        #endregion         

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CanCancel && DialogResult != true)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
    }
}
