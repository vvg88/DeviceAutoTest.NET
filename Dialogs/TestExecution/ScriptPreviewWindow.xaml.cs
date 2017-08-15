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

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for ScriptPreviewWindow.xaml
    /// </summary>
    public partial class ScriptPreviewWindow : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public ScriptPreviewWindow(ScriptClassGenerator scriptGenerator, string sourceCode, CompilerResults compilerResults)
        {
            InitializeComponent();
            ScriptGenerator = scriptGenerator;
            SourceCode = sourceCode;
            foreach (CompilerError error in compilerResults.Errors)
            {
                if (!error.IsWarning)
                {
                    Errors.Add(error);
                }
            }
            IsValid = errors.Count == 0;
            DataContext = this;
        }

        private ScriptClassGenerator ScriptGenerator;

        private string sourceCode;

        /// <summary>
        /// Исходный код
        /// </summary>
        public string SourceCode
        {
            get { return sourceCode; }
            set 
            {                 
                sourceCode = value;
                OnPropertyChanged("SourceCode");
            }
        }

        private List<CompilerError> errors = new List<CompilerError>();

        /// <summary>
        /// Список ошибок
        /// </summary>
        public List<CompilerError> Errors
        {
            get { return errors; }
            private set
            {
                errors = value;
                OnPropertyChanged("Errors");
            }
        }

        private int currLine;

        /// <summary>
        /// Текущая строка в исходном коде 
        /// </summary>
        public int CurrLine
        {
            get { return currLine; }
            set
            {
                currLine = value;
                OnPropertyChanged("CurrLine");
            }
        }

        private int currColumn;

        /// <summary>
        /// Текущий столбец в исходном коде 
        /// </summary>
        public int CurrColumn
        {
            get { return currColumn; }
            set
            {
                currColumn = value;
                OnPropertyChanged("CurrColumn");
            }
        }

        private bool isValid = false;

        /// <summary>
        /// Признак отсутствия ошибок
        /// </summary>
        public bool IsValid
        {
            get { return isValid; }
            set 
            {
                isValid = value;
                OnPropertyChanged("IsValid");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {            
            TextBox textBox = sender as TextBox;
            CurrLine = textBox.GetLineIndexFromCharacterIndex(textBox.SelectionStart) + 1;
            int prevLinesLen = 0;
            for (int i = 0; i < CurrLine-1; i++)
            {
                prevLinesLen += textBox.GetLineLength(i);
            }
            CurrColumn = textBox.SelectionStart - prevLinesLen;
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CompilerError error = (sender as DataGridRow).Item as CompilerError;
            if (error != null)
            {                
                int pos = 0;
                for (int i = 0; i < error.Line - 1; i++)
                {                    
                    pos += SourceCodeTextBox.GetLineLength(i);
                }
                pos += error.Column - 1;
                if (pos >= SourceCode.Length)
                {
                    pos = SourceCode.Length - 1;
                }
                if (pos < 0)
                {
                    pos = 0;
                }
                SourceCodeTextBox.Select(pos, 0);
                SourceCodeTextBox.ScrollToLine(SourceCodeTextBox.GetLineIndexFromCharacterIndex(pos));
                SourceCodeTextBox.Focus();
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid)
                return;            
            var environment = new ScriptEnvironment("!DebugSN", true);
            ScriptGenerator.Execute(environment);
        }
    }
}
