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
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.WPFComponents.ScalableWindows;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for ProtocolPatternNameDialog.xaml
    /// </summary>
    public partial class TestTemplateNameDialog : DATDialogWindow
    {
        /// <summary>
        /// Редактируемое значение
        /// </summary>
        public string EditedValue { get; set; }
        /// <summary>
        /// Guid шаблона, имя которого редактируется
        /// </summary>
        internal static Guid TemplateGuid;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="editedValue"></param>
        /// <param name="templateGuid"></param>
        public TestTemplateNameDialog(string editedValue, Guid templateGuid)
        {
            InitializeComponent();
            EditedValue = editedValue;
            DataContext = this;
            TemplateGuid = templateGuid;          
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            EditedValueTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            EditedValueTextBox.Focus();
            EditedValueTextBox.SelectAll();
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
            TemplateGuid = Guid.Empty;
        }
    }

    /// <summary>
    /// Валидатор имени шаблона
    /// </summary>
    internal class TemplateNameValidator : ValidationRule
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
                return new ValidationResult(false, Properties.Resources.TemplateNameMustBeNotEmpty);
            }
            foreach (var templateDescriptor in DATTemplate.TestTemplateDescriptors)
            {
                if (currentName == templateDescriptor.Name && TestTemplateNameDialog.TemplateGuid != templateDescriptor.GUID)
                {
                    return new ValidationResult(false, string.Format(Properties.Resources.TemplateNameExists, currentName));
                }
            }
            return new ValidationResult(true, null);
        }
    }

}
