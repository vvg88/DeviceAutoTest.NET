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
using System.Text.RegularExpressions;
using NeuroSoft.DeviceAutoTest.ScriptExecution;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Диалог с информацией о новом тесте
    /// </summary>
    public partial class ScriptTagInfoDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="template"></param>
        public ScriptTagInfoDialog(ScriptInfo script, bool newScript = true) 
        {
            InitializeComponent();            
            ExistingScripts = new List<ScriptInfo>(script.TemplateItemParent.ButtonScripts);
            if (newScript)
            {
                string scriptName = Properties.Resources.DefaultButtonScriptName;
                int i = 2;
                while ((from s in ExistingScripts where s.Name == scriptName select s).FirstOrDefault() != null)
                {
                    scriptName = Properties.Resources.DefaultButtonScriptName + (i++);
                }
                ScriptName = scriptName;
                ScriptDescription = Properties.Resources.DefaultButtonScriptDescription;
            }
            else
            {
                ExistingScripts.Remove(script);
                ScriptName = script.Name;
                ScriptDescription = script.Description;
            }
            DataContext = this;
        }

        #region Properties

        internal static List<ScriptInfo> ExistingScripts = new List<ScriptInfo>();        

        private string scriptDescription;
        /// <summary>
        /// Имя теста
        /// </summary>
        public string ScriptDescription
        {
            get { return scriptDescription; }
            set
            {
                if (scriptDescription != value)
                {
                    scriptDescription = value;
                    OnPropertyChanged("ScriptDescription");
                }
            }
        }

        private string scriptName;
        /// <summary>
        /// Идентификатор теста
        /// </summary>
        public string ScriptName
        {
            get { return scriptName; }
            set
            {
                if (scriptName != value)
                {
                    scriptName = value;
                    OnPropertyChanged("ScriptName");
                }
            }
        }
        #endregion 
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            BindingOperations.GetBindingExpression(ScriptNameTextBox, TextBox.TextProperty).UpdateSource();
            ScriptNameTextBox.SelectAll();
            ScriptDescriptionTextBox.SelectAll();            
            ScriptNameTextBox.Focus();
            base.OnActivated(e);            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ExistingScripts = null;
        }
    }

    /// <summary>
    /// Валидатор имени скрипта, выполняемого по кнопке
    /// </summary>
    internal class ScriptNameValidator : ValidationRule
    {
        /// <summary>
        /// Валидация
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string currentName = (string)value;
            if (string.IsNullOrEmpty(currentName))
            {
                return new ValidationResult(false, Properties.Resources.ScriptNameMustBeNotEmpty);
            }

            if (!Regex.IsMatch(currentName, DATVariableDescriptor.NamePattern))
            {
                return new ValidationResult(false, Properties.Resources.InvalidScriptName);
            }

            foreach (var script in ScriptTagInfoDialog.ExistingScripts)
            {
                if (currentName == script.Name)
                {
                    return new ValidationResult(false, string.Format(Properties.Resources.ScriptNameExists, currentName));
                }                
            }
            return new ValidationResult(true, null);
        }
    }
}
