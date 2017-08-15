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
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest;
using System.Collections.ObjectModel;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for ScriptAssembliesListDialog.xaml
    /// </summary>
    public partial class EditTestCorrectionsDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public EditTestCorrectionsDialog(TestTemplateItem testItem)
        {
            InitializeComponent();
            TestItem = testItem;
            CorrectionsList.Clear();
            foreach (var correctionStr in testItem.ProbableCorrections)
            {
                CorrectionsList.Add(new TestCorrectionInfo(correctionStr));
            }            
            DataContext = this;
        }

        #region Properties       

        private TestTemplateItem testItem;
        /// <summary>
        /// 
        /// </summary>
        public TestTemplateItem TestItem
        {
            get { return testItem; }
            private set { testItem = value; }
        }

        private TestCorrectionInfo selectedItem;
        /// <summary>
        /// 
        /// </summary>
        public TestCorrectionInfo SelectedItem
        {
            get 
            {
                return selectedItem;
            }
            set 
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    TestCorrectionValidator.ExistingCorrections = new List<string>();
                    foreach (var item in CorrectionsList)
                    {
                        if (item != selectedItem)
                        {
                            TestCorrectionValidator.ExistingCorrections.Add(item.CorrectionString);
                        }
                    }
                    OnPropertyChanged("SelectedItem");
                    OnPropertyChanged("HasSelectedItem");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool HasSelectedItem
        {
            get
            {
                return SelectedItem != null;
            }
        }

        private ObservableCollection<TestCorrectionInfo> сorrectionsList = new ObservableCollection<TestCorrectionInfo>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<TestCorrectionInfo> CorrectionsList
        {
            get { return сorrectionsList; }
        }
        #endregion
        private void OkButtonClick(object sender, RoutedEventArgs e)
        {            
            DialogResult = true;
            TestItem.ProbableCorrections.Clear();
            foreach (var item in CorrectionsList)
            {
                TestItem.ProbableCorrections.Add(item.CorrectionString);
            }
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            string newCorrectionStr = Properties.Resources.DefaultTestCorrectionStr;
            int i = 1;
            while (CorrectionsList.Any(item => item.CorrectionString == newCorrectionStr))
            {
                newCorrectionStr = Properties.Resources.DefaultTestCorrectionStr + "(" + i + ")";
                i++;
            }
            CorrectionsList.Add(new TestCorrectionInfo(newCorrectionStr));
            SelectedItem = CorrectionsList[CorrectionsList.Count - 1];
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (!HasSelectedItem)
                return;

            int selIndex = CorrectionsList.IndexOf(SelectedItem);
            CorrectionsList.RemoveAt(selIndex);
            if (selIndex > CorrectionsList.Count - 1)
            {
                selIndex--;
            }
            if (selIndex > -1)
            {
                SelectedItem = CorrectionsList[selIndex];
            }
            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TestCorrectionInfo : BaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public TestCorrectionInfo(string text)
        {
            CorrectionString = text;
        }

        private string correctionString;
        /// <summary>
        /// 
        /// </summary>
        public string CorrectionString
        {
            get { return correctionString; }
            set
            {
                if (correctionString != value)
                {
                    correctionString = value;
                    OnPropertyChanged("CorrectionString");
                    OnPropertyChanged("DisplayValue");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string DisplayValue
        { 
            get
            {
                int displayLimit = 45;
                if (CorrectionString.Length < displayLimit)
                {
                    return CorrectionString;
                }
                return CorrectionString.Remove(displayLimit - 1) + "...";
            }
        }
    }
    internal class TestCorrectionValidator : ValidationRule
    {
        internal static List<string> ExistingCorrections = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string valueStr = Convert.ToString(value);
            if (string.IsNullOrEmpty(valueStr))
            {
                return new ValidationResult(false, Properties.Resources.ValueMustBeNotEmpty);
            }
            if (ExistingCorrections != null && ExistingCorrections.Contains(valueStr))
            {
                return new ValidationResult(false, Properties.Resources.VariableAlreadyExists);
            }
            return new ValidationResult(true, null);
        }
    }    
}
