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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for TemplateValidationWindow.xaml
    /// </summary>
    public partial class TemplateValidationWindow : DATDialogWindow
    {        
        /// <summary>
        /// Конструктор
        /// </summary>
        public TemplateValidationWindow(DATTemplate template, List<ITag> scriptTags)
        {
            InitializeComponent();
            DATTemplate = template;
            if (scriptTags != null)
            {
                ScriptTags = scriptTags;
            }
            DataContext = this;            
            Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            Worker.WorkerSupportsCancellation = true;
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            Loaded += new RoutedEventHandler(TemplateValidationWindow_Loaded);
        }        

        void TemplateValidationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private bool Start()
        {
            if (!Worker.IsBusy)
            {
                CheckedScriptsCount = 0;
                Worker.RunWorkerAsync();                
                OnPropertyChanged("ValidationStarted");
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool Stop()
        {
            if (Worker.IsBusy)
            {
                Worker.CancelAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnPropertyChanged("ValidationStarted");
            if (e.Cancelled || e.Error != null)
            {
                ProgressText = Properties.Resources.ValidationCanceled;
            }
            else
            {
                ProgressText = Properties.Resources.ValidationFinished;
            }
        }

        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {                        
            Dispatcher.Invoke(DispatcherPriority.Input, new Action(() =>
            {
                Errors.Clear();
            }));
            foreach (var script in AllScripts)
            {
                if (Worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                ProgressText = string.Format(Properties.Resources.ValidationScriptProgressMsg, script.TemplateItemParent.Name, script.Name);
                if (script.IsEnabled)
                {
                    ScriptClassGenerator scriptGenerator = new ScriptClassGenerator(script, DebugVariables, DebugTests);
                    scriptGenerator.CheckErrors(false);
                    if (Worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    foreach (CompilerError error in scriptGenerator.LastCompilerResults.Errors)
                    {
                        if (!error.IsWarning)
                        {
                            var scriptError = new ScriptError(script, error);
                            lock (scriptError)
                            {
                                Dispatcher.Invoke(DispatcherPriority.Input, new Action(() =>
                                    {
                                        Errors.Add(new ScriptError(script, error));
                                    }));
                            }
                        }
                    }
                }
                CheckedScriptsCount++;
            }
        }

        private BackgroundWorker Worker = new BackgroundWorker();
        List<ITag> ScriptTags = new List<ITag>();

        List<DATVariable> DebugVariables = new List<DATVariable>();
        List<TestObject> DebugTests = new List<TestObject>();

        private DATTemplate datTemplate;
        /// <summary>
        /// 
        /// </summary>
        public DATTemplate DATTemplate
        {
            get { return datTemplate; }
            private set 
            { 
                datTemplate = value;
                foreach (var test in datTemplate.GetAllTests())
                {                    
                    AllScripts.Add(test.ValidationScript);
                    AllScripts.Add(test.BeginTestScript);
                    if (!test.IsContainer)
                    {
                        AllScripts.Add(test.ExecutionScript);
                    }
                    foreach (var script in test.ButtonScripts)
                    {
                        AllScripts.Add(script);
                    }
                    AllScripts.Add(test.EndTestScript);
                }
                ScriptsCount = AllScripts.Count;

                DebugVariables.Clear();
                foreach (var varDescr in DATTemplate.Variables)
                {
                    DebugVariables.Add(new DATVariable(varDescr));
                }
                DebugTests.Clear();
                foreach (var templateItem in DATTemplate.GetAllTests())
                {
                    DebugTests.Add(new TestObject(null, templateItem));
                }
            }
        }

        private List<ScriptInfo> AllScripts = new List<ScriptInfo>();

        private int scriptsCount;
        /// <summary>
        /// 
        /// </summary>
        public int ScriptsCount
        {
            get { return scriptsCount; }
            private set
            {
                scriptsCount = value;
                OnPropertyChanged("ScriptsCount");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ValidationStarted
        {
            get
            {
                return Worker.IsBusy;
            }
        }

        private int checkedScriptsCount = 0;
        /// <summary>
        /// 
        /// </summary>
        public int CheckedScriptsCount
        {
            get { return checkedScriptsCount; }
            private set
            {
                checkedScriptsCount = value;
                OnPropertyChanged("CheckedScriptsCount");
            }
        }

        private ScriptError selectedError;

        /// <summary>
        /// 
        /// </summary>
        public ScriptError SelectedError
        {
            get { return selectedError; }
            set
            { 
                selectedError = value;
                OnPropertyChanged("SelectedError");
            }
        }

        private string progressText;
        /// <summary>
        /// 
        /// </summary>
        public string ProgressText
        {
            get { return progressText; }
            set
            {
                progressText = value;
                OnPropertyChanged("ProgressText");
            }
        }

        private ObservableCollection<ScriptError> errors = new ObservableCollection<ScriptError>();
        /// <summary>
        /// Список ошибок
        /// </summary>
        public ObservableCollection<ScriptError> Errors
        {
            get { return errors; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            Stop();
            base.OnClosed(e);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedError == null)
                return;
            Stop();
            var SelectedScript = SelectedError.ErrorScript;
            int index = Errors.IndexOf(SelectedError);
            int savedIndex = index;
            EditScriptWindow editScriptDialog = new EditScriptWindow(SelectedScript, ScriptTags);
            editScriptDialog.ShowDialog();
            ScriptClassGenerator scriptGenerator = new ScriptClassGenerator(SelectedScript, DebugVariables, DebugTests);
            scriptGenerator.CheckErrors(false);
            for (int i = Errors.Count - 1; i >= 0; i--)
            {
                if (Errors[i].ErrorScript == SelectedScript)
                {
                    Errors.RemoveAt(i);
                }
            }            
            foreach (CompilerError error in scriptGenerator.LastCompilerResults.Errors)
            {
                if (!error.IsWarning)
                {
                    Errors.Insert(index++, new ScriptError(SelectedScript, error));
                }
            }
            if (savedIndex > 0 && savedIndex < Errors.Count)
            {
                SelectedError = Errors[savedIndex];
            }
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationStarted)
            {
                Stop();
            }
            else
            {
                Start();
            }            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScriptError
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="error"></param>
        public ScriptError(ScriptInfo script, CompilerError error)
        {
            ErrorScript = script;
            Error = error;
        }

        private CompilerError error;        
        /// <summary>
        /// Текст ошибки
        /// </summary>
        public CompilerError Error
        {
            get { return error; }
            private set { error = value; }
        }

        private ScriptInfo errorScript;
        /// <summary>
        /// Скрипт, в котором найдена ошибка
        /// </summary>
        public ScriptInfo ErrorScript
        {
            get { return errorScript; }
            set { errorScript = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public TestTemplateItem TemplateItem
        {
            get
            {
                return ErrorScript != null ? ErrorScript.TemplateItemParent : null;
            }
        }
    }
}
