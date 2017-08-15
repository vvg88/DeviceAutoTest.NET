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
using NeuroSoft.WPFComponents.ScalableWindows;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for AutoTestFailedDialog.xaml
    /// </summary>
    public partial class AutoTestFailedDialog : DATDialogWindow
    {
        private AutoTestErrorAction action;
        /// <summary>
        /// 
        /// </summary>
        internal AutoTestErrorAction Action
        {
            get { return action; }
            private set { action = value; }
        }

        private TestObject test;
        /// <summary>
        /// 
        /// </summary>
        public TestObject Test
        {
            get { return test; }
            private set { test = value; }
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public AutoTestFailedDialog(TestObject test)
        {
            InitializeComponent();
            Test = test;
            DataContext = this;
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            Action = AutoTestErrorAction.Retry;
            DialogResult = true;
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            Action = AutoTestErrorAction.Skip;
            DialogResult = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Action = AutoTestErrorAction.Stop;
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (DialogResult != true)
                e.Cancel = true;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static AutoTestErrorAction Show(TestObject test)
        {
            AutoTestFailedDialog dialog = new AutoTestFailedDialog(test);
            dialog.ShowDialog();
            return dialog.Action;
        }
    }

    /// <summary>
    /// Возможные действия при ошибке в процессе автоматического тестирования
    /// </summary>
    public enum AutoTestErrorAction
    {
        Skip,
        Retry,
        Stop
    }
}
