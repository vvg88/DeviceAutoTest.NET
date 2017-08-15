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

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameTestGroupDialog.xaml
    /// </summary>
    public partial class RenameTestGroupDialog : DATDialogWindow
    {
        /// <summary>
        /// Редактируемое значение
        /// </summary>
        public string EditedValue { get; set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="editedValue"></param>
        public RenameTestGroupDialog(string editedValue)
        {
            InitializeComponent();
            EditedValue = editedValue;
            DataContext = this;
            this.Loaded += delegate
            {
                EditedValueTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                EditedValueTextBox.Focus();
                EditedValueTextBox.SelectAll();
            };            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }    
}
