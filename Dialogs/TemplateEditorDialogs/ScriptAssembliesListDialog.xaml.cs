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
    public partial class ScriptAssembliesListDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public ScriptAssembliesListDialog(IEnumerable<string> scriptAssemblies)
        {
            InitializeComponent();
            Assemblies.Clear();
            foreach (var assemblyStr in scriptAssemblies)
            {
                Assemblies.Add(assemblyStr);
            }
            DataContext = this;
        }

        #region Properties       

        private string selectedAssembly;
        /// <summary>
        /// Выделенный в списке путь к библиотеке
        /// </summary>
        public string SelectedAssembly
        {
            get { return selectedAssembly; }
            set 
            {
                if (selectedAssembly != value)
                {
                    selectedAssembly = value;
                    OnPropertyChanged("SelectedAssembly");
                }
            }
        }

        private ObservableCollection<string> assemblies = new ObservableCollection<string>();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<string> Assemblies
        {
            get { return assemblies; }
        }
        #endregion
        private void OkButtonClick(object sender, RoutedEventArgs e)
        {            
            DialogResult = true;
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string executablePath = ScriptInfo.GetExecutablePath();
            openFileDialog.InitialDirectory = executablePath;
            openFileDialog.Filter = "Assemblies (*.dll)|*.dll";
            if (openFileDialog.ShowDialog() == true)
            {
                string newAssembly = DATHelper.MakeRelativePath(executablePath + System.IO.Path.DirectorySeparatorChar, openFileDialog.FileName);
                if (!Assemblies.Contains(newAssembly))
                {
                    Assemblies.Add(newAssembly);
                }
            }
        }        

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedAssembly == null)
                return;

            int selIndex = Assemblies.IndexOf(SelectedAssembly);
            Assemblies.RemoveAt(selIndex);
            if (selIndex > Assemblies.Count - 1)
            {
                selIndex--;
            }
            if (selIndex > -1)
            {
                SelectedAssembly = Assemblies[selIndex];
            }
            
        }
    }  
}
