using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using System.Globalization;
using System.Windows;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Text.RegularExpressions;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Переменная теста
    /// </summary>
    [Serializable]
    public class DATVariableDescriptor : SimpleSerializedData, ITag
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public DATVariableDescriptor(string name, DATVariableType variableType)
        {
            Name = name;
            Type = variableType;            
            ResetDefaultValue();
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DATVariableDescriptor(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Properties
        [Serialize]
        private string name;
        [Serialize]
        private string description;
        [Serialize]
        private DATVariableType type;
        [Serialize]
        private string defaultValue = string.Empty;
        [Serialize]
        private bool isReadonly = false;
        [Serialize]
        private int doubleValueDecimalPlaces = 3;
        [Serialize]        
        private double doubleValueIncrement = 0.001;
        [Serialize]
        private double numericMinValue = -100;
        [Serialize]
        private double numericMaxValue = 100;
        [Serialize]
        private bool validateNumerics;

        /// <summary>
        /// Идентификатор переменной
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, NamePattern))
                    {
                        throw new ArgumentException(Properties.Resources.InvalidVariableName);
                    }
                    name = value;
                    OnPropertyChanged("Name");
                    OnPropertyChanged("DisplayValue");
                }
            }
        }        

        /// <summary>
        /// Шаблон, которому должно удовлетворять имя переменной
        /// </summary>
        public const string NamePattern = "^[a-zA-Z]{1}[a-zA-Z_0-9]*$";

        /// <summary>
        /// Шаблон, которому должен удовлетворять символ имени переменной
        /// </summary>
        public const string NameSymbolPattern = "[a-zA-Z_0-9]+";

        /// <summary>
        /// Описание переменной
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged("Description");
                    OnPropertyChanged("DisplayValue");
                }
            }
        }
        
        /// <summary>
        /// Тип переменной
        /// </summary>
        public DATVariableType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;                    
                    OnPropertyChanged("Type");
                    OnPropertyChanged("DisplayValue");                    
                    ResetDefaultValue();
                }
            }
        }

        /// <summary>
        /// Количество отображаемых символов после запятой для типа вещественных чисел
        /// </summary>
        public int DoubleValueDecimalPlaces
        {
            get { return doubleValueDecimalPlaces; }
            set
            {                
                if (doubleValueDecimalPlaces != value)
                {
                    doubleValueDecimalPlaces = value;
                    OnPropertyChanged("DoubleValueDecimalPlaces");
                }
            }
        }

        /// <summary>
        /// Инкремент при вводе значения вещественного числа
        /// </summary>
        public double DoubleValueIncrement
        {
            get { return doubleValueIncrement; }
            set
            {
                if (doubleValueIncrement != value)
                {
                    doubleValueIncrement = value;
                    OnPropertyChanged("DoubleValueIncrement");
                }
            }
        }

        /// <summary>
        /// Ограничение числа снизу
        /// </summary>
        public double NumericMinValue
        {
            get { return numericMinValue; }
            set
            {
                if (numericMinValue != value)
                {
                    numericMinValue = value;
                    OnPropertyChanged("NumericMinValue");
                }
            }
        }
        /// <summary>
        /// Ограничение числа сверху
        /// </summary>
        public double NumericMaxValue
        {
            get { return numericMaxValue; }
            set
            {
                if (numericMaxValue != value)
                {
                    numericMaxValue = value;
                    OnPropertyChanged("NumericMaxValue");
                }
            }
        }        

        /// <summary>
        /// Признак валидации числовых значений
        /// </summary>
        public bool ValidateNumerics
        {
            get { return validateNumerics; }
            set
            {
                if (validateNumerics != value)
                {
                    validateNumerics = value;
                    OnPropertyChanged("ValidateNumerics");
                }
            }
        }               

        /// <summary>
        /// Начальное значение переменной
        /// </summary>
        public object DefaultValue
        {
            get 
            {
                return ValueFromString(defaultValue, Type);                
            }
            set
            {
                defaultValue = ValueToString(value, Type);
                OnPropertyChanged("DefaultValue");
            }
        }

        internal const string ArrayElementStart = @"<#";
        internal const string ArrayElementEnd = @"#>";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="variableType"></param>
        /// <returns></returns>
        public static string ValueToString(object value, DATVariableType variableType)
        {
            string result = string.Empty;
            if (value == null)
                return result;
            switch (variableType)
            { 
                case DATVariableType.ArrayDouble:
                    double[] doubleArray = (double[])value;
                    for (int i = 0; i < doubleArray.Length; i++)
                    {
                        result += ArrayElementStart + Convert.ToString(doubleArray[i], CultureInfo.InvariantCulture) + ArrayElementEnd;
                    }
                    break;
                case DATVariableType.ArrayString:
                    string[] stringArray = (string[])value;
                    for (int i = 0; i < stringArray.Length; i++)
                    {
                        result += ArrayElementStart + stringArray[i] + ArrayElementEnd;
                    }
                    break;
                default:
                    result = Convert.ToString(value, CultureInfo.InvariantCulture);
                    break;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="variableType"></param>
        /// <returns></returns>
        public static object ValueFromString(string strValue, DATVariableType variableType)
        {
            object result = null;
            if (variableType == DATVariableType.Boolean)
            {
                bool boolVal = false;
                bool.TryParse(strValue, out boolVal);
                result = boolVal;
            }
            else if (variableType == DATVariableType.Double)
            {
                double doubleVal = 0;
                double.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleVal);
                result = doubleVal;
            }
            else if (variableType == DATVariableType.Integer)
            {
                int intVal = 0;
                int.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out intVal);
                result = intVal;
            }
            else if (variableType == DATVariableType.ArrayDouble)
            {
                var matches = Regex.Matches(strValue, "(" + ArrayElementStart + @"(?<value>[\s\S]*?)" + ArrayElementEnd + ")");
                double[] doubleArray = new double[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    string strVal = matches[i].Groups["value"].Value;
                    doubleArray[i] = double.Parse(strVal, CultureInfo.InvariantCulture);
                }
                result = doubleArray;
            }
            else if (variableType == DATVariableType.ArrayString)
            {
                var matches = Regex.Matches(strValue, "(" + ArrayElementStart + @"(?<value>[\s\S]*?)" + ArrayElementEnd + ")");
                string[] stringArray = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    stringArray[i] = matches[i].Groups["value"].Value;
                }
                result = stringArray;
            }
            else
            {
                result = strValue;
            }
            return result;
        }        

        /// <summary>
        /// Только чтение
        /// </summary>
        public bool IsReadonly
        {
            get { return isReadonly; }
            set 
            {
                if (isReadonly != value)
                {
                    isReadonly = value;
                    OnPropertyChanged("IsReadonly");
                }
            }
        }

        /// <summary>
        /// Признак того, является ли значение переменной массивом
        /// </summary>
        public bool IsArray
        {
            get { return Type == DATVariableType.ArrayDouble || Type == DATVariableType.ArrayString; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayValue
        {
            get
            {
                return Name + "(" + Type + ") " + (string.IsNullOrEmpty(Description) ? "" : ": " + Description);
            }
        }

        /// <summary>
        /// Префикс переменной при использовании в содержимом теста.
        /// </summary>
        public string Prefix
        {
            get
            {
                return InternalPrefix;
            }
        }

        /// <summary>
        /// Режим вставки переменной
        /// </summary>
        public TagInsertMode TagInsertMode
        {
            get
            {
                return IsArray ? DeviceAutoTest.TagInsertMode.OnlyScript : DeviceAutoTest.TagInsertMode.Both;
            }
        }

        internal static string InternalPrefix = @"$";
        #endregion

        #region Methods

        private void ResetDefaultValue()
        {
            object val = string.Empty;
            if (Type == DATVariableType.Boolean)
            {
                val = false;
            }
            else if (Type == DATVariableType.Double)
            {
                val = 0d;
            }
            else if (Type == DATVariableType.Integer)
            {
                val = 0;
            }
            else if (Type == DATVariableType.ArrayDouble)
            {
                val = new double[0];
            }
            else if (Type == DATVariableType.ArrayString)
            {
                val = new string[0];
            }
            DefaultValue = val;
        }

        /// <summary>
        /// Генерация тега шаблона протоколов на основе описателя переменной
        /// </summary>
        public PatternTagDescription GeneratePatternTag()
        {
            return new PatternTagDescription("$" + Name, Description, GetTagValueType());            
        }

        private TagValueTypeEnum GetTagValueType()
        {
            switch (Type)
            {
                case DATVariableType.Double:
                    return TagValueTypeEnum.Double;
                case DATVariableType.String:
                    return TagValueTypeEnum.String;
                case DATVariableType.Boolean:
                    return TagValueTypeEnum.Boolean;
                case DATVariableType.ArrayDouble:
                    return TagValueTypeEnum.DoubleArray;
                case DATVariableType.ArrayString:
                    return TagValueTypeEnum.StringArray;
                default:
                    return TagValueTypeEnum.Int;
            }
        }
        #endregion
    }
 
    /// <summary>
    /// Тип переменной
    /// </summary>
    public enum DATVariableType
    { 
        /// <summary>
        /// Вещественное число
        /// </summary>
        Double,
        /// <summary>
        /// Целое число
        /// </summary>
        Integer,
        /// <summary>
        /// Строка
        /// </summary>
        String,
        /// <summary>
        /// Булева переменная
        /// </summary>
        Boolean,
        /// <summary>
        /// Массив чисел
        /// </summary>
        ArrayDouble,
        /// <summary>
        /// Массив строк
        /// </summary>
        ArrayString
    }

    /// <summary>
    /// Значение переменной теста
    /// </summary>
    [Serializable]
    public class DATVariable : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public DATVariable(DATVariableDescriptor variable)
        {
            variableDescriptor = variable;
            TestVariableID = variable.Name;
            TestVariableType = variable.Type;
            VariableValue = variable.DefaultValue;  
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DATVariable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Properties
        [Serialize]
        private string testVariableID = "";
        [Serialize]
        private DATVariableType testVariableType;
        [Serialize]
        private string serializedValue = string.Empty;
        
        internal event RoutedEventHandler Modified;
        /// <summary>
        /// Идентификатор переменной
        /// </summary>
        public string TestVariableID
        {
            get { return testVariableID; }
            private set { testVariableID = value; }
        }

        /// <summary>
        /// Тип переменной
        /// </summary>
        public DATVariableType TestVariableType
        {
            get { return testVariableType; }
            internal set { testVariableType = value; }
        }
        
        private object variableValue;
        /// <summary>
        /// Значение переменной
        /// </summary>
        public object VariableValue
        {
            get { return variableValue; }
            set
            {
                bool areEquals = variableValue == value;
                if (variableValue is IComparable)
                {
                    areEquals = (variableValue as IComparable).CompareTo(value) == 0;
                }
                if (!areEquals)
                {
                    variableValue = (TestVariableType != DATVariableType.Double) ? value : Math.Round(Convert.ToDouble(value), DoubleValueDecimalPlaces);
                    OnPropertyChanged("VariableValue");
                    OnPropertyChanged("IsValid");
                    SerializeVarValue();
                    OnModified();
                    ValueChanged = true;
                }
            }
        }

        private DATVariableDescriptor variableDescriptor;
        /// <summary>
        /// Описатель переменной (не сериализуется, инициализируется при десериализации инструкции)
        /// </summary>
        public DATVariableDescriptor VariableDescriptor
        {
            get { return variableDescriptor; }
            internal set 
            {
                if (value != null && value.Name == TestVariableID)
                {
                    variableDescriptor = value;
                }
            }
        }
        /// <summary>
        /// Ограничение числа снизу
        /// </summary>
        public double NumericMinValue
        {
            get { return VariableDescriptor != null ? VariableDescriptor.NumericMinValue : double.MinValue; }            
        }
        /// <summary>
        /// Ограничение числа сверху
        /// </summary>
        public double NumericMaxValue
        {
            get { return VariableDescriptor != null ? VariableDescriptor.NumericMaxValue : double.MaxValue; }
        }

        /// <summary>
        /// Признак валидации числовых значений
        /// </summary>
        public bool ValidateNumerics
        {
            get { return VariableDescriptor != null ? VariableDescriptor.ValidateNumerics : false; }
        }          

        /// <summary>
        /// Проверка на валидность значения переменной
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (ValidateNumerics && TestVariableType == DATVariableType.Double || TestVariableType == DATVariableType.Integer)
                {
                    var varVal = Convert.ToDouble(VariableValue);                    
                    return varVal >= NumericMinValue && varVal <= NumericMaxValue;
                }
                return true;
            }
        }

        private bool valueChanged = false;
        /// <summary>
        /// Признак того, что значение переменной было изменено
        /// </summary>
        internal bool ValueChanged
        {
            get { return valueChanged; }
            set { valueChanged = value; }
        }

        private static bool lockModifyNotification;
        /// <summary>
        /// Признак блокировки уведомления об изменении значения переменной
        /// </summary>
        public static bool LockModifyNotification
        {
            get { return lockModifyNotification; }
            set { lockModifyNotification = value; }
        }

        /// <summary>
        /// Строка отображения значения переменной
        /// </summary>
        public string DisplayVariableValue
        {
            get
            {
                if (TestVariableType == DATVariableType.ArrayDouble)
                {
                    double[]  array = (double[])VariableValue;
                    string result = "{";
                    for (int i = 0; i < array.Length; i++)
                    {
                        result += Convert.ToString(array[i], CultureInfo.InvariantCulture);
                        if (i < array.Length - 1)
                        {
                            result += ", ";
                        }
                    }
                    result += "}";
                    return result;
                }
                else if (TestVariableType == DATVariableType.ArrayString)
                {
                    string[]  array = (string[])VariableValue;
                    string result = "{";
                    for (int i = 0; i < array.Length; i++)
                    {
                        result += "\"" + array[i] + "\"";
                        if (i < array.Length - 1)
                        {
                            result += ", ";
                        }
                    }
                    result += "}";
                    return result;
                }
                return Convert.ToString(VariableValue, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Строка отображения переменной в содержимом инструкции
        /// </summary>
        public string ShowValueString
        {
            get
            {
                return DATVariableDescriptor.InternalPrefix + TestVariableID;
            }
        }

        /// <summary>
        /// Строка запроса значения переменной
        /// </summary>
        public string QueryValueString
        {
            get
            {
                return "?" + DATVariableDescriptor.InternalPrefix + TestVariableID;
            }
        }

        /// <summary>
        /// Строка объявления переменной в скрипте
        /// </summary>
        public string VariableDefinitionString
        {
            get
            {
                string variableStr = string.Empty;
                switch (TestVariableType)
                {
                    case DATVariableType.Boolean:                        
                        variableStr = "bool " + TestVariableID + " = " + Convert.ToString(VariableValue ?? "false").ToLowerInvariant() + ";";
                        break;
                    case DATVariableType.Double:
                        string valueStr = (Convert.ToString(VariableValue, NumberFormatInfo.InvariantInfo) ?? "0");
                        if (VariableValue is double && double.IsNaN((double)VariableValue))
                        {
                            valueStr = "double." + valueStr;
                        }
                        variableStr = "double " + TestVariableID + " = " + valueStr + ";";
                        break;
                    case DATVariableType.Integer:
                        variableStr = "int " + TestVariableID + " = " + (Convert.ToString(VariableValue, NumberFormatInfo.InvariantInfo) ?? "0") + ";";
                        break;
                    case DATVariableType.String:
                        variableStr = "string " + TestVariableID + " = " + ScriptClassGenerator.StringToCodeString(Convert.ToString(VariableValue)) + ";";
                        break;
                    case DATVariableType.ArrayDouble:
                        double[] doubleArray = (double[])VariableValue;
                        variableStr = "double[] " + TestVariableID + " = new double[] {";
                        for (int i = 0; i < doubleArray.Length; i++)
                        {
                            variableStr += (double.IsNaN(doubleArray[i]) ? "double." : "") + Convert.ToString(doubleArray[i], NumberFormatInfo.InvariantInfo);
                            if (i < doubleArray.Length-1)
                            {
                                variableStr += ", ";
                            }
                        }
                        variableStr += "};";
                        break;
                    case DATVariableType.ArrayString:
                        string[] stringArray = (string[])VariableValue;
                        variableStr = "string[] " + TestVariableID + " = new string[] {";
                        for (int i = 0; i < stringArray.Length; i++)
                        {
                            variableStr += ScriptClassGenerator.StringToCodeString(stringArray[i]);
                            if (i < stringArray.Length - 1)
                            {
                                variableStr += ", ";
                            }
                        }
                        variableStr += "};";
                        break;
                }
                return variableStr;
            }
        }

        /// <summary>
        /// Строка аттрибутов переменной (при использовании в скрипте)
        /// </summary>
        public string VariableScriptAttributes
        {
            get
            {
                if (!ValidateNumerics)
                    return string.Empty;
                return "[System.ComponentModel.DataAnnotations.Range(" + Convert.ToString(NumericMinValue, CultureInfo.InvariantCulture) + ", " + Convert.ToString(NumericMaxValue, CultureInfo.InvariantCulture) + ")]";            
            }
        }
        /// <summary>
        /// Количество отображаемых символов после запятой для тип вещественных чисел
        /// </summary>
        public int DoubleValueDecimalPlaces
        {
            get { return VariableDescriptor != null ? VariableDescriptor.DoubleValueDecimalPlaces : 2; }
        }

        /// <summary>
        /// Инкремент при вводе значения вещественного числа
        /// </summary>
        public double DoubleValueIncrement
        {
            get { return VariableDescriptor != null ? VariableDescriptor.DoubleValueIncrement : 1d; }
        }
        #endregion

        #region Methods

        internal void InitVarDescriptor(DATTemplate template)
        {
            if (template == null)
                return;

            VariableDescriptor = template.Variables.FirstOrDefault(v => v.Name == TestVariableID);
        }

        private void OnModified()
        {            
            if (!LockModifyNotification && Modified != null)
            {
                Modified(this, new RoutedEventArgs());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            SerializeVarValue();
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void Deserialized()
        {
            base.Deserialized();
            DeserializeVarValue();
        }

        private void SerializeVarValue()
        {
            if (!VerifyType(VariableValue))
            {
                throw new InvalidCastException("Invalid variable value type.");
            }
            serializedValue = DATVariableDescriptor.ValueToString(VariableValue, TestVariableType);
        }
        
        private void DeserializeVarValue()
        {            
            variableValue = DATVariableDescriptor.ValueFromString(serializedValue, TestVariableType);
        }

        /// <summary>
        /// Проверка соответствия типа значения типу переменной
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool VerifyType(object value)
        {
            if (value != null)
            {
                TypeCode valueTypeCode = Type.GetTypeCode(value.GetType());
                switch (TestVariableType)
                {
                    case DATVariableType.Boolean:
                        if (valueTypeCode != TypeCode.Boolean)
                        {
                            return false;
                        }
                        break;
                    case DATVariableType.Double:
                    case DATVariableType.Integer:
                        if (valueTypeCode == TypeCode.Boolean || valueTypeCode == TypeCode.Char ||
                            valueTypeCode == TypeCode.DateTime || valueTypeCode == TypeCode.DBNull ||
                            valueTypeCode == TypeCode.Empty || valueTypeCode == TypeCode.Object ||
                            valueTypeCode == TypeCode.String)
                        {
                            return false;
                        }
                        break;
                    case DATVariableType.String:
                        if (valueTypeCode != TypeCode.String)
                        {
                            return false;
                        }
                        break;
                    case DATVariableType.ArrayDouble:
                        return value.GetType().IsAssignableFrom(typeof(double[]));
                    case DATVariableType.ArrayString:
                        return value.GetType().IsAssignableFrom(typeof(string[]));
                }
            }
            return true;
        }

        /// <summary>
        /// Клонирование
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            DATVariable clone = MemberwiseClone() as DATVariable;
            return clone;
        }
        #endregion
    }    
}
