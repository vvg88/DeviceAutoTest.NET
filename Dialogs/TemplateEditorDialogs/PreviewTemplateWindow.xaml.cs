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
    /// Interaction logic for PreviewTemplateWindow.xaml
    /// </summary>
    public partial class PreviewTemplateWindow : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public PreviewTemplateWindow(TestTemplateItem testTemplate)
        {
            InitializeComponent();
            ContentDocument = testTemplate.ContentDocument.Clone();
            TextRange contentRange = new TextRange(ContentDocument.ContentStart, ContentDocument.ContentEnd);
            foreach (var variableDescriptor in testTemplate.TemplateParent.Variables)
            {
                contentRange.ReplaceVariableLinks(new DATVariable(variableDescriptor));
            }
            foreach (var contentPresenter in testTemplate.ContentPresenters)
            {
                contentPresenter.ResetPresenterInstnace();
                contentRange.ReplaceContentPresenterLink(contentPresenter);
            }
            foreach (var script in testTemplate.ButtonScripts)
            {
                contentRange.ReplaceButtonScriptLink(null, script);
            }            
            DataContext = this;
        }

        private FlowDocument contentDocument;

        /// <summary>
        /// Содержимое теста
        /// </summary>
        public FlowDocument ContentDocument
        {
            get { return contentDocument; }
            set 
            {                 
                contentDocument = value;
                OnPropertyChanged("ContentDocument");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
