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
using System.CodeDom.Compiler;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for EditScriptWindow.xaml
    /// </summary>
    public partial class EditScriptWindow : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public EditScriptWindow(ScriptInfo script, List<ITag> currentTags)
        {
            InitializeComponent();
            Script = script;
            tags = currentTags;            
            DataContext = this;
        }

        private IList<ITag> tags = new List<ITag>();
        private ScriptInfo script;

        /// <summary>
        /// Скрипт
        /// </summary>
        public ScriptInfo Script
        {
            get { return script; }
            private set { script = value; }
        }             

        /// <summary>
        /// Теги
        /// </summary>
        public IList<ITag> Tags
        {
            get { return tags; }
        }
        

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }        
    }
}
