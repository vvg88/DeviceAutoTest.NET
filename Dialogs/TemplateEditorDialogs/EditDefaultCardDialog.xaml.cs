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
using NeuroSoft.Prototype.Interface;
using NeuroSoft.Prototype.Database;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for EditDefaultCardDialog.xaml
    /// </summary>
    public partial class EditDefaultCardDialog : DATDialogWindow
    {
        private DataConnection currentConnection;
        /// <summary>
        /// 
        /// </summary>
        public DataConnection CurrentConnection
        {
            get 
            {                
                if (currentConnection == null)
                {
                    currentConnection = Globals.CurrentConnection.Connection;
                }
                return currentConnection; 
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="defaultCardPath"></param>
        public EditDefaultCardDialog(string defaultCardPath)
        {
            InitializeComponent();
            DefaultCardPath = defaultCardPath ?? "";
            DataContext = this;       
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Путь по умолчанию
        /// </summary>
        public string DefaultCardPath
        {
            get { return (string)GetValue(DefaultCardPathProperty); }
            set { SetValue(DefaultCardPathProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DefaultCardPathProperty =
            DependencyProperty.Register("DefaultCardPath", typeof(string), typeof(EditDefaultCardDialog), new UIPropertyMetadata("", OnDefaultCardPathChanged));

        private static void OnDefaultCardPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EditDefaultCardDialog).OnDefaultCardPathChanged(e);
        }
        private void OnDefaultCardPathChanged(DependencyPropertyChangedEventArgs e)
        {
            CardExists = Globals.GetCardInfoByPath(CurrentConnection, DefaultCardPath) != null;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CardExists
        {
            get { return (bool)GetValue(CardExistsProperty); }
            set { SetValue(CardExistsProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty CardExistsProperty =
            DependencyProperty.Register("CardExists", typeof(bool), typeof(EditDefaultCardDialog), new UIPropertyMetadata(false));

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cardInfo = Globals.GetCardInfoByPath(CurrentConnection, DefaultCardPath);            
            string id = "";
            if (cardInfo != null) id = cardInfo.Id;
            NeuroSoft.Prototype.Interface.Database.SelectCardForm form = new NeuroSoft.Prototype.Interface.Database.SelectCardForm(Globals.CurrentConnection.Connection, id);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CardInfo newCardInfo = CurrentConnection.GetCardInfoById(form.CurrentCardId);
                DefaultCardPath = Globals.GetCardInfoPath(newCardInfo);
            }
        }
    }
}
