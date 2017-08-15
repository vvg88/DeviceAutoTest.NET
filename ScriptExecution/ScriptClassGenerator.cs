using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest.Dialogs;
using System.Reflection;
using System.Windows;
using System.IO;

namespace NeuroSoft.DeviceAutoTest.ScriptExecution
{
    /// <summary>
    /// Класс для генерации класса выполнения скрипта
    /// </summary>
    public class ScriptClassGenerator
    {
        static ScriptClassGenerator()
        {
            CommonAssamblies.Add("NeuroSoft.DeviceAutoTest.NewDevicesTest.dll");
            CommonAssamblies.Add("NeuroSoft.DeviceAutoTest.OldDevicesTest.dll");
            //CommonAssamblies.Add("NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.dll ");
            CommonAssamblies.Add("NeuroSoft.Hardware.Devices.dll");
            CommonAssamblies.Add("NeuroSoft.Hardware.Devices.UniversalTestStend.dll");
            //Если System.ComponentModel.DataAnnotations будет использоваться только в скриптах, то библиотека не подхватится, необходимо явное использование
            System.ComponentModel.DataAnnotations.RangeAttribute rangeDummy;
            
            CommonUsings.Add("System");
            CommonUsings.Add("System.Reflection");
            CommonUsings.Add("System.Windows");
            CommonUsings.Add("System.Windows.Controls");
            CommonUsings.Add("NeuroSoft.DeviceAutoTest.Common");
            CommonUsings.Add("NeuroSoft.DeviceAutoTest.Common.Scripts");
            CommonUsings.Add("NeuroSoft.DeviceAutoTest.NewDevicesTest");
            CommonUsings.Add("NeuroSoft.DeviceAutoTest.NewDevicesTest.Scripts");            
            CommonUsings.Add("NeuroSoft.DeviceAutoTest.OldDevicesTest");
            CommonUsings.Add("NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts");
            CommonUsings.Add("NeuroSoft.Hardware.Devices");
            CommonUsings.Add("NeuroSoft.WPFComponents.ScalableWindows");
            //CommonUsings.Add("NeuroSoft.DeviceAutoTest.BluetoothDevicesTest");
            //CommonUsings.Add("NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Scripts");
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="testItem"></param>
        /// <param name="variables"></param>
        public ScriptClassGenerator(ScriptInfo scriptInfo, IList<DATVariable> variables = null, IList<TestObject> tests = null)
        {
            ScriptInfo = scriptInfo;
            if (variables != null)
            {
                foreach (var variable in variables)
                {
                    Variables.Add(variable);
                }
            }
            if (tests != null)
            {
                foreach (var test in tests)
                {
                    Tests.Add(test);
                }
            }            
        }

        #region Properties
        private ScriptInfo scriptInfo;

        /// <summary>
        /// Шаблон теста
        /// </summary>
        public ScriptInfo ScriptInfo
        {
            get { return scriptInfo; }
            private set { scriptInfo = value; }
        }

        IList<DATVariable> variables = new List<DATVariable>();
        /// <summary>
        /// Переменные, объявляемые в коде
        /// </summary>
        public IList<DATVariable> Variables
        {
            get { return variables; }
        }

        private bool readonlyVariables = false;

        /// <summary>
        /// Использовать переменные в режиме readonly
        /// </summary>
        public bool ReadonlyVariables
        {
            get { return readonlyVariables; }
            set { readonlyVariables = value; }
        }

        private bool useVariables = true;
        /// <summary>
        /// Использовать ли переменные
        /// </summary>
        public bool UseVariables
        {
            get { return useVariables; }
            set { useVariables = value; }
        }


        private bool lockVariablesNotification = false;
        /// <summary>
        /// Блокировать уведомления об изменении переменных
        /// </summary>
        public bool LockVariablesNotification
        {
            get { return lockVariablesNotification; }
            set { lockVariablesNotification = value; }
        }
        IList<TestObject> tests = new List<TestObject>();
        /// <summary>
        /// Список возможных действий
        /// </summary>
        public IList<TestObject> Tests
        {
            get { return tests; }
        }

        /// <summary>
        /// Список пространств имен, используемых при выполнении любого скрипта
        /// </summary>
        internal static readonly List<string> CommonUsings = new List<string>();
        /// <summary>
        /// Список сборок, используемых при выполнении любого скрипта
        /// </summary>
        internal static readonly List<string> CommonAssamblies = new List<string>();
        /// <summary>
        /// Пространство имен в генерируемом коде
        /// </summary>
        public const string GeneratedNamespace = "NeuroSoft.DeviceAutoTest.ScriptExecution";
        ///// <summary>
        ///// Имя класса в генерируемом коде
        ///// </summary>
        //public const string GeneratedClassName = "TestExecutionClass";
        ///// <summary>
        ///// Имя класса информации о тесте
        ///// </summary>
        //public const string TestInfoClassName = "TestInfoClass";

        /// <summary>
        /// Имя метода выполнения скрипта в генерируемом коде
        /// </summary>
        public const string ExecuteMethodName = "Execute";
        #endregion

        #region Methods
        /// <summary>
        /// Генерация кода
        /// </summary>
        /// <returns></returns>
        public string GenerateClassCode()
        {
            CodeBuilder builder = new CodeBuilder();
            //Генерация using'ов
            builder.AppendCode(GenerateUsings());
            //---------------
            //Пространство имен
            builder.AppendCodeLine("namespace " + GeneratedNamespace);
            builder.OpenCodeBlock();
            //Класс для выполнения скрипта
            builder.AppendCodeLine("public class " + ScriptInfo.ClassName);
            builder.OpenCodeBlock();
            //Метод, содержащий скрипт
            GenerateScriptMethod(builder);
            builder.Builder.AppendLine();
            GenerateIsValidMethodCode(builder);
            builder.Builder.AppendLine();
            //-------------------------------
            //Объявление и инициализация переменных
            if (UseVariables)
            {
                GenerateVariablesCode(builder, Variables, ReadonlyVariables);
                builder.Builder.AppendLine();
                GenerateTestInfoVariablesCode(builder, Tests);
            }
            //---------------            
            builder.CloseCodeBlock();
            //Класс информации о тесте            
            //GenerateTestInfoClass(builder);                                    
            //--------------------------------
            builder.CloseCodeBlock();
            return builder.CodeText;
        }

        private string GenerateUsings()
        {
            StringBuilder result = new StringBuilder();    
            //добавим общие пространства имен
            foreach (var scriptUsing in CommonUsings)
            {                
                result.AppendLine("using " + scriptUsing + ";");
            }
            //добавим пространства имен для выполняемого скрипта
            foreach (var scriptUsing in ScriptInfo.ScriptUsings)
            {
                if (!CommonUsings.Contains(scriptUsing))
                {
                    result.AppendLine("using " + scriptUsing + ";");
                }
            }
            return result.ToString();
        }


        private void GenerateIsValidMethodCode(CodeBuilder builder)
        {
            builder.AppendCodeLine("private bool IsValid(double value, string fieldName, string errorMessage = null)");
            builder.OpenCodeBlock();
            builder.AppendCodeLine(string.Format("var attr = Attribute.GetCustomAttribute(typeof({0}).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance), typeof(System.ComponentModel.DataAnnotations.RangeAttribute)) as System.ComponentModel.DataAnnotations.RangeAttribute;", ScriptInfo.ClassName));
            builder.AppendCodeLine("if (attr == null) return true;");
            builder.AppendCodeLine("if (value < System.Convert.ToDouble(attr.Minimum) || value > System.Convert.ToDouble(attr.Maximum))");
            builder.OpenCodeBlock();
            builder.AppendCodeLine("if (!string.IsNullOrEmpty(errorMessage)) NSMessageBox.Show(errorMessage, \"\", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);");
            builder.AppendCodeLine("return false;");
            builder.CloseCodeBlock();
            builder.AppendCodeLine("return true;");
            builder.CloseCodeBlock();
        }

        private void GenerateTestInfoVariablesCode(CodeBuilder builder, IList<TestObject> tests)
        {
            if (tests == null)
                return;
            foreach (var test in tests)
            {
                builder.AppendCodeLine("readonly bool " + test.TestId + "_Finished = " + BoolToCodeString(test.Finished) + ", " +
                    test.TestId + "_HasErrors = " + BoolToCodeString(test.HasErrors) + ", " +
                    test.TestId + "_WasCorrected = " + BoolToCodeString(test.WasCorrected) + "; ");
                builder.AppendCodeLine("readonly string " + test.TestId + "_Comments = " + StringToCodeString(test.Comments) + "; ");
            }
        }

        internal static string BoolToCodeString(bool boolValue)
        {
            return boolValue ? "true" : "false";
        }
        

        internal static string StringToCodeString(string stringValue)
        {
            if (stringValue == null)
                return "null";            
            return "\"" + stringValue.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace(Environment.NewLine, "\\r\\n") + "\"";
        }

        private void GenerateVariablesCode(CodeBuilder builder, IList<DATVariable> variables, bool readonlyVariables)
        {
            if (variables == null)
                return;
            foreach (var variable in variables)
            {
                if (!readonlyVariables && !string.IsNullOrEmpty(variable.VariableScriptAttributes))
                {
                    builder.AppendCodeLine(variable.VariableScriptAttributes);
                }
                builder.AppendCodeLine((readonlyVariables ? "readonly " : "") + variable.VariableDefinitionString);
            }
        }

        private void GenerateScriptMethod(CodeBuilder builder)
        {
            builder.AppendCodeLine("public " + ScriptInfo.ReturnType + " " + ExecuteMethodName + "(ScriptEnvironment environment)");
            builder.OpenCodeBlock();
            if (!string.IsNullOrEmpty(ScriptInfo.Script))
            {
                string[] lines = ScriptInfo.Script.Split(new string[3] { "\n", "\r\n", "\r" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    builder.AppendCodeLine(line);
                }
            }
            builder.CloseCodeBlock();
        }

        private static List<string> loadedAssemblies = null;

        /// <summary>
        /// Список путей к загруженным сборкам
        /// </summary>
        internal static List<string> LoadedAssemblies
        {
            get 
            {
                if (loadedAssemblies == null)
                {                    
                    loadedAssemblies = new List<string>();                    
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            string location = assembly.Location;
                            if (!string.IsNullOrEmpty(location))
                            {
                                loadedAssemblies.Add(location);
                            }
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                }
                // Если в списке loadedAssemblies отсутствует System.ComponentModel.DataAnnotations.dll, то его необходимо добавить
                if (!loadedAssemblies.Any(assambly => assambly.Contains("System.ComponentModel.DataAnnotations.dll")))
                {
                    // Найти и получить строку с WindowsBase.dll, т.к. путь к этой dll совпадает с путем к System.ComponentModel.DataAnnotations.dll
                    string windowsBasePath = loadedAssemblies.FirstOrDefault(assambly => assambly.Contains("WindowsBase.dll"));
                    //Если путь к WindowsBase.dll существует
                    if (windowsBasePath != null)
                    {
                        // Заменить в этом пути WindowsBase на System.ComponentModel.DataAnnotations
                        string dataAnnotationsPath = windowsBasePath.Replace("WindowsBase", "System.ComponentModel.DataAnnotations");
                        // Если по новому пути к DataAnnotations существует файл dll, то добавить его в список
                        if (File.Exists(dataAnnotationsPath))
                            loadedAssemblies.Add(dataAnnotationsPath);
                    }
                }
                return loadedAssemblies; 
            }            
        }

        internal CompilerParameters GetCompilerParameters()
        {
            string executablePath = ScriptInfo.GetExecutablePath() + System.IO.Path.DirectorySeparatorChar;
            CompilerParameters parameters = new CompilerParameters();            
            foreach (var item in ScriptInfo.Assemblies)
            {
                parameters.ReferencedAssemblies.Add(executablePath + item);
            }
            foreach (var item in CommonAssamblies)
            {
                parameters.ReferencedAssemblies.Add(executablePath + item);
            }            
            parameters.ReferencedAssemblies.AddRange(LoadedAssemblies.ToArray());
            parameters.GenerateInMemory = true;
            return parameters;
        }

        /// <summary>
        /// Проверка кода на ошибки
        /// </summary>
        /// <param name="showErrorsDialog">Надо ли отображать окно с ошибками</param>
        /// <returns>true, если код содержит ошибки</returns>
        public bool CheckErrors(bool showErrorsDialog = false)
        {
            string code = GenerateClassCode();
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();            
            CompilerParameters parameters = GetCompilerParameters();
            LastCompilerResults = codeProvider.CompileAssemblyFromSource(parameters, code);
            if (LastCompilerResults.Errors.HasErrors)
            {
                if (showErrorsDialog)
                {
                    Preview(code, LastCompilerResults);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Просмотр сгенерированного на основе скрипта класса
        /// </summary>
        public void Preview()
        {
            string code = GenerateClassCode();
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters parameters = GetCompilerParameters();
            LastCompilerResults = codeProvider.CompileAssemblyFromSource(parameters, code);
            Preview(code, LastCompilerResults);
        }

        internal void Preview(string code, CompilerResults compilerResults)
        {
            ScriptPreviewWindow scriptErrorsWindow = new ScriptPreviewWindow(this, code, compilerResults);
            scriptErrorsWindow.ShowDialog();
        }

        private CompilerResults lastCompilerResults;
        /// <summary>
        /// Последние результаты компиляции скрипта
        /// </summary>
        public CompilerResults LastCompilerResults
        {
            get { return lastCompilerResults; }
            private set { lastCompilerResults = value; }
        }        
       
        /// <summary>
        /// Выполнение скрипта
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public object Execute(ScriptEnvironment environment)
        {
            //сгенерируем код
            string code = GenerateClassCode();
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();            
            CompilerParameters parameters = GetCompilerParameters();            

            LastCompilerResults = codeProvider.CompileAssemblyFromSource(parameters, code);            
            //проверим код на наличие ошибок
            if (LastCompilerResults.Errors.HasErrors)
            {
                string errorMsg = "";

                errorMsg = LastCompilerResults.Errors.Count.ToString() + " Errors:";
                for (int x = 0; x < LastCompilerResults.Errors.Count; x++)
                    errorMsg = errorMsg + Environment.NewLine + "Line: " + LastCompilerResults.Errors[x].Line.ToString() + " - " +
                        LastCompilerResults.Errors[x].ErrorText;

                NSMessageBox.Show(errorMsg + Environment.NewLine + Environment.NewLine + code, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            Assembly assembly = LastCompilerResults.CompiledAssembly;
            //получим экземпляр сгенерированного класса
            object classObject = assembly.CreateInstance(GeneratedNamespace + "." + ScriptInfo.ClassName);
            if (classObject == null)
            {
                NSMessageBox.Show(string.Format(Properties.Resources.CouldNotLoadClass, GeneratedNamespace + "." + ScriptInfo.ClassName), Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            Type classType = classObject.GetType();
            //выполним скрипт
            object[] args = new object[] { environment };
            try
            {
                object result = classType.InvokeMember(ExecuteMethodName, BindingFlags.InvokeMethod, null, classObject, args);             
                //обновим значения переменных
                if (UseVariables && !ReadonlyVariables)
                {
                    bool savedLockModifyNotification = DATVariable.LockModifyNotification;
                    DATVariable.LockModifyNotification = LockVariablesNotification;
                    try
                    {
                        foreach (var variable in Variables)
                        {
                            FieldInfo field = classType.GetField(variable.TestVariableID, BindingFlags.Instance | BindingFlags.NonPublic);
                            variable.VariableValue = field.GetValue(classObject);
                        }
                    }
                    finally
                    {
                        DATVariable.LockModifyNotification = savedLockModifyNotification;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    NSMessageBox.Show(string.Format(Properties.Resources.ScriptExecutionError, ex.InnerException.Message + Environment.NewLine + ex.InnerException.StackTrace), Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    NSMessageBox.Show(string.Format(Properties.Resources.ScriptExecutionError, ex.Message + Environment.NewLine + ex.StackTrace), Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return null;
            }            
        }
        #endregion
    }

    internal class CodeBuilder
    {
        private StringBuilder builder = new StringBuilder();
        private int openedBlocksCount = 0;

        /// <summary>
        /// StringBuilder
        /// </summary>
        public StringBuilder Builder
        {
            get { return builder; }            
        }

        /// <summary>
        /// Текста сгенерированного на данный момент кода
        /// </summary>
        public string CodeText
        {
            get { return builder.ToString(); }
        }

        /// <summary>
        /// Добавить линию кода
        /// </summary>
        /// <param name="line"></param>
        public void AppendCodeLine(string line)
        {
            string prefix = string.Empty;
            for (int i = 0; i < openedBlocksCount; i++)
            {
                prefix += "    ";
            }
            builder.AppendLine(prefix + line);
        }

        /// <summary>
        /// Добавить код
        /// </summary>
        /// <param name="code"></param>
        public void AppendCode(string code)
        {
            builder.Append(code);
        }
        /// <summary>
        /// Открыть блок кода
        /// </summary>
        public void OpenCodeBlock()
        {
            AppendCodeLine("{");
            openedBlocksCount++;
        }
        /// <summary>
        /// Закрыть блок кода
        /// </summary>
        public void CloseCodeBlock()
        {
            if (openedBlocksCount > 0)
            {
                openedBlocksCount--;
            }
            AppendCodeLine("}");            
        }

    }
}
