using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.Prototype.Database;

namespace NeuroSoft.DeviceAutoTest
{
    internal static class Converters
    {
        internal static TestActionType ToTestAction(AutoTestAction autoTestAction)
        {
            switch (autoTestAction)
            {
                case AutoTestAction.Error:
                    return TestActionType.HasErrors;
                case AutoTestAction.Success:
                    return TestActionType.Success;
                default:
                    return TestActionType.Execute;
            }
        }

        internal static AutoTestAction ToAutoTestAction(TestActionType testAction)
        {
            switch (testAction)
            {
                case TestActionType.HasErrors:
                    return AutoTestAction.Error;
                case TestActionType.Success:
                    return AutoTestAction.Success;
                default:
                    return AutoTestAction.Execute;
            }
        }
    }
    internal class PatientInfoToTemplateNameConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var patientInfo = value as PacientInfo;
            if (patientInfo == null)
                return value;
            var template = DATTemplate.TestTemplateDescriptors.FirstOrDefault(d => d.GUID == new Guid(patientInfo.Polis) && (d.Version == System.Convert.ToInt32(patientInfo.Height) || patientInfo.Height == 0));
            if (template == null)
                return patientInfo.Guid;
            return patientInfo.Height == 0 ? template.Name : template.FullName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class StatusToImageConverter : IValueConverter
    {
        private ResourceDictionary imagesDictionary = new ResourceDictionary();

        internal ResourceDictionary ImagesDictionary
        {
            get
            {
                if (imagesDictionary.Source == null)
                {
                    imagesDictionary.Source = new Uri("pack://application:,,,/NeuroSoft.DeviceAutoTest;component/Resources/VectorImages.xaml");
                }
                return imagesDictionary;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TestObjectStatus status = (TestObjectStatus)value;
            ImageSource img = null;
            switch (status)
            {
                case TestObjectStatus.HasErrors:
                    img = (ImageSource)ImagesDictionary["DI_TestHasErrors"];
                    break;
                case TestObjectStatus.Success:
                    img = (ImageSource)ImagesDictionary["DI_TestSuccess"];
                    break;
                case TestObjectStatus.Corrected:
                    img = (ImageSource)ImagesDictionary["DI_TestCorrected"];
                    break;
                default:
                    img = (ImageSource)ImagesDictionary["DI_TestNotExecuted"];
                    break;
            }
            return img;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class IntToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToInt32(value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class TimeSpanToStringConverter : IValueConverter
    {      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return value;
            TimeSpan timeSpan = (TimeSpan)value;            
            return string.Format("{0:###00}:{1:00}:{2:00}", System.Convert.ToInt32(timeSpan.TotalHours), timeSpan.Minutes, timeSpan.Seconds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class DecimalPlacesToIncrementConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int decimalPlaces = System.Convert.ToInt32(value);
            return Math.Pow(10, -1*decimalPlaces);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToInt32(value);
        }
    }

    internal class ItemsControlAlternatingStyleSelector : StyleSelector
    {
        Style evenStyle, oddStyle;

        /// <summary>
        /// 
        /// </summary>
        public Style EvenStyle
        {
            get { return evenStyle; }
            set { evenStyle = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Style OddStyle
        {
            get { return oddStyle; }
            set { oddStyle = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            ItemsControl control = ItemsControl.ItemsControlFromItemContainer(container);

            return (0 == (control.Items.IndexOf(item) % 2)) ? EvenStyle : OddStyle;
        }
    }

    internal class ItemsControlAlternatingTemplateSelector : DataTemplateSelector
    {
        DataTemplate evenTemplate, oddTemplate;

        /// <summary>
        /// 
        /// </summary>
        public DataTemplate EvenTemplate
        {
            get { return evenTemplate; }
            set { evenTemplate = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTemplate OddTemplate
        {
            get { return oddTemplate; }
            set { oddTemplate = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ItemsControl control = ItemsControl.ItemsControlFromItemContainer(container);

            return (0 == (control.Items.IndexOf(item) % 2)) ? EvenTemplate : OddTemplate;
        }
    }     
}
