using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.WPFPrototype.Interface.Common;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using NeuroSoft.DeviceAutoTest.Commands;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common.Converters;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Модель представления выполняемого теста
    /// </summary>
    public class TestContentViewModel : BaseViewModel
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="testModel"></param>
        public TestContentViewModel(TestObject testModel)
        {
            Model = testModel;            
        }

        #region Properties
        private TestObject model;

        /// <summary>
        /// Модель теста
        /// </summary>
        public TestObject Model
        {
            get { return model; }
            set
            {
                if (model != value)
                {
                    model = value;
                    OnPropertyChanged("Model");
                }
            }
        }

        private double zoom = 100d;
        /// <summary>
        /// Масштаб содержимого теста
        /// </summary>
        public double Zoom
        {
            get { return zoom; }
            set
            {
                if (zoom != value)
                {
                    zoom = value;
                    OnPropertyChanged("Zoom");
                    OnZoomChanged();
                }
            }
        }
        /// <summary>
        /// Событие изменения масштаба
        /// </summary>
        public event RoutedEventHandler ZoomChanged;

        private void OnZoomChanged()
        {
            if (ZoomChanged != null)
            {
                ZoomChanged(this, new RoutedEventArgs());
            }
        }

        FlowDocument modelContent = null;
        /// <summary>
        /// Содержимое теста с контролами-запросами значений переменных
        /// </summary>
        public FlowDocument ModelContent
        {
            get
            {
                if (modelContent == null)
                {
                    modelContent = Model.TemplateItem.ContentDocument.Clone();
                    TextRange contentRange = new TextRange(modelContent.ContentStart, modelContent.ContentEnd);
                    
                    var usedVariables = contentRange.GetVariableNames();
                    foreach (var variable in Model.AncestorParent.Variables)
                    {
                        if (usedVariables.Contains(variable.TestVariableID))
                        {
                            contentRange.ReplaceVariableLinks(variable);
                        }
                    }
                    foreach (var contentPresenterTag in Model.TemplateItem.ContentPresenters)
                    {
                        contentRange.ReplaceContentPresenterLink(contentPresenterTag);                        
                    }
                    foreach (var script in Model.TemplateItem.ButtonScripts)
                    {
                        contentRange.ReplaceButtonScriptLink(Model, script);
                    }
                    if (contentRange.ReplaceSupplyCurrentIndicator(Model.AncestorParent.ScriptExecutionEnvironment, CustomTag.SupplyCurrentTag))
                    {
                        Model.ContainsSupplyCurrent = true;
                    }
                }
                return modelContent;
            }
        }
        
        private bool isExecuting = false;

        /// <summary>
        /// Признак выполнения действия
        /// </summary>
        public bool IsExecuting
        {
            get { return isExecuting; }
            set
            {
                if (isExecuting != value)
                {                    
                    isExecuting = value;
                    OnPropertyChanged("IsExecuting");
                }
            }
        }

        private bool showManualAutoTestMessage = false;
        /// <summary>
        /// Признак отображения сообщении о выполнении теста в ручном режиме
        /// </summary>
        public bool ShowManualAutoTestMessage
        {
            get { return showManualAutoTestMessage; }
            set 
            {
                if (showManualAutoTestMessage != value)
                {
                    showManualAutoTestMessage = value;
                    OnPropertyChanged("ShowManualAutoTestMessage");
                }
            }
        }

        private bool testStopped = false;

        /// <summary>
        /// Признак старта тестирования (выполнена валидация и начальный скрипт) и не выполнен скрипт окночания теста
        /// </summary>
        public bool TestStarted
        {
            get { return testStopped; }
            set
            {
                if (testStopped != value)
                {
                    testStopped = value;
                    OnPropertyChanged("TestStarted");
                }
            }
        }

        #endregion
    }

    internal static class TextRangeEx
    {

        /// <summary>
        /// Поиск имен переменных, используемых в тесте
        /// </summary>
        /// <param name="range"></param>
        /// <param name="onlyInputs">Искать только запрашиваемые переменные</param>
        internal static List<string> GetVariableNames(this TextRange range, bool onlyInputs = false)
        {                        
            //Вставка запросов значений переменной
            var currPos = range.Start;
            string rangeText = range.Text;
            return DATHelper.GetVariableNames(rangeText, onlyInputs);            
        }

        /// <summary>
        /// Замена ссылок на переменную на соответствующий запрос или текст значения
        /// </summary>
        /// <param name="range"></param>
        /// <param name="variable"></param>
        internal static bool ReplaceVariableLinks(this TextRange range, DATVariable variable)
        {
            bool success = false;
            //Вставка запросов значений переменной
            var currPos = range.Start;
            while (currPos != null && currPos.CompareTo(range.End) < 0)
            {
                TextRange varRange = range.GetTextRangeFromPosition(ref currPos, variable.QueryValueString);
                if (varRange == null)
                    break;
                //Проверим, является ли найденная переменная целым словом (не найдена ли часть другой переменной)
                TextPointer next = varRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next != null)
                {
                    var nextSymbolRange = new TextRange(varRange.End, next);
                    if (Regex.IsMatch(nextSymbolRange.Text, DATVariableDescriptor.NameSymbolPattern))
                    {                        
                        continue;
                    }
                }                
                InsertInlineUIContainer(VariableQueryControl.CreateVariableQuery(variable), varRange);
                success = true;
            }

            //вставка значения переменной
            currPos = range.Start;
            while (currPos != null && currPos.CompareTo(range.End) < 0)
            {         
                TextRange varRange = range.GetTextRangeFromPosition(ref currPos, variable.ShowValueString);
                if (varRange == null)
                    break;                
                var varStart = varRange.Start;
                //Проверим, является ли найденная переменная целым словом (не найдена ли часть другой переменной)
                TextPointer next = varRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next != null)
                {
                    var nextSymbolRange = new TextRange(varRange.End, next);
                    if (Regex.IsMatch(nextSymbolRange.Text, DATVariableDescriptor.NameSymbolPattern))
                    {
                        continue;
                    }
                }

                var fontInfo = new TagFontInfo(varRange);
                varRange.Text = string.Empty;
                Binding varValueBinding = new Binding("VariableValue");
                varValueBinding.Source = variable;
                varValueBinding.Mode = BindingMode.OneWay;                
                Run run = new Run(Convert.ToString(variable.VariableValue), varStart);
                fontInfo.SetFontToTextElement(run);
                BindingOperations.SetBinding(run, Run.TextProperty, varValueBinding);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Замена первой найденной ссылки на содержимое экземпляром ContentPresenter
        /// </summary>
        /// <param name="range"></param>
        /// <param name="contentPresenterTag"></param>
        internal static bool ReplaceContentPresenterLink(this TextRange range, ContentPresenterTag contentPresenterTag)
        {
            bool success = false;
            string tagString = contentPresenterTag.Prefix + contentPresenterTag.Name;
            var currPos = range.Start;
            while (currPos != null && currPos.CompareTo(range.End) < 0)
            {
                TextRange varRange = range.GetTextRangeFromPosition(ref currPos, tagString);
                if (varRange == null)
                    break;                
                var varStart = varRange.Start;
                //Проверим, является ли найденная переменная целым словом (не найдена ли часть другой переменной)
                TextPointer next = varRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next != null)
                {
                    var nextSymbolRange = new TextRange(varRange.End, next);
                    if (Regex.IsMatch(nextSymbolRange.Text, DATVariableDescriptor.NameSymbolPattern))
                    {
                        continue;
                    }
                }
                                
                if (contentPresenterTag.ContentPresenterInstance.Parent is InlineUIContainer)
                {                 
                    (contentPresenterTag.ContentPresenterInstance.Parent as InlineUIContainer).Child = null;                    
                }
                InsertInlineUIContainer(contentPresenterTag.ContentPresenterInstance, varRange);
                success = true;
                break;
            }
            return success;
        }

        /// <summary>
        /// Вставка индикатора тока потребления
        /// </summary>
        /// <param name="range"></param>
        /// <param name="environment"></param>
        /// <param name="supplyCurrentTag"></param>
        internal static bool ReplaceSupplyCurrentIndicator(this TextRange range, ScriptEnvironment environment, CustomTag supplyCurrentTag)
        {
            string tagString = supplyCurrentTag.Prefix + supplyCurrentTag.Name;
            var currPos = range.Start;
            while (currPos != null && currPos.CompareTo(range.End) < 0)
            {
                TextRange varRange = range.GetTextRangeFromPosition(ref currPos, tagString);
                if (varRange == null)
                    break;
                var varStart = varRange.Start;
                //Проверим, является ли найденная переменная целым словом (не найдена ли часть другой переменной)
                TextPointer next = varRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next != null)
                {
                    var nextSymbolRange = new TextRange(varRange.End, next);
                    if (Regex.IsMatch(nextSymbolRange.Text, DATVariableDescriptor.NameSymbolPattern))
                    {
                        continue;
                    }
                }

                Border indicator = new Border();
                indicator.DataContext = environment.SupplyCurrent;
                indicator.CornerRadius = new CornerRadius(4);
                TextBlock indicatorTextBlock = new TextBlock();
                indicatorTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                Binding valueBinding = new Binding("LastSupplyCurrent");
                valueBinding.Mode = BindingMode.OneWay;
                valueBinding.StringFormat = "{0,6:N2}";
                BindingOperations.SetBinding(indicatorTextBlock, TextBlock.TextProperty, valueBinding);
                Binding backGroundBinding = new Binding("IsValidCurrent");
                backGroundBinding.Converter = new IsValidToBrushConverter();
                backGroundBinding.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(indicator, Border.BackgroundProperty, backGroundBinding);
                indicator.Child = indicatorTextBlock;
                InsertInlineUIContainer(indicator, varRange);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Замена первой найденной ссылки на кнопку
        /// </summary>
        /// <param name="range"></param>
        /// <param name="test"></param>
        /// <param name="buttonScript"></param>
        internal static bool ReplaceButtonScriptLink(this TextRange range, TestObject test, ScriptInfo buttonScript)
        {
            bool success = false;
            string tagString = buttonScript.Prefix + buttonScript.Name;
            var currPos = range.Start;
            while (currPos != null && currPos.CompareTo(range.End) < 0)
            {
                TextRange varRange = range.GetTextRangeFromPosition(ref currPos, tagString);
                if (varRange == null)
                    break;
                var varStart = varRange.Start;
                //Проверим, является ли найденная переменная целым словом (не найдена ли часть другой переменной)
                TextPointer next = varRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next != null)
                {
                    var nextSymbolRange = new TextRange(varRange.End, next);
                    if (Regex.IsMatch(nextSymbolRange.Text, DATVariableDescriptor.NameSymbolPattern))
                    {
                        continue;
                    }
                }

                Button button = new Button();               
                button.Padding = new Thickness(5, 2, 5, 2);
                button.Margin = new Thickness();
                button.Content = buttonScript.Description;
                if (test != null)
                {
                    button.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                    {
                        test.ExecuteInternal(buttonScript);
                    });
                }
                InsertInlineUIContainer(button, varRange);
                success = true;
                break;
            }
            return success;
        }        

        private static void InsertInlineUIContainer(UIElement content, TextRange insertTo)
        {
            var fontInfo = new TagFontInfo(insertTo);
            insertTo.Text = string.Empty;            
            var inline = new InlineUIContainer(content, insertTo.Start) { BaselineAlignment = System.Windows.BaselineAlignment.Center };
            fontInfo.SetFontToTextElement(inline);                
        }
       
        /// <summary>
        /// Метод определяет, содержит ли TextRange заданный текст
        /// </summary>
        /// <param name="range"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static bool ContainsText(this TextRange range, string text)
        {
            return Regex.IsMatch(range.Text, text.Replace("$", "\\$").Replace("?", "\\?"), RegexOptions.IgnoreCase);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="position"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static TextRange GetTextRangeFromPosition(this TextRange range, ref TextPointer position, string input)
        {
            TextRange textRange = null;
            if (input.Length <= 1)
                return null;
            string startSymbol = input.Remove(1);
            while (position != null)
            {
                if (position.CompareTo(range.End) >= 0)
                {
                    break;
                }

                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);                    
                    StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase;
                    int indexInRun = textRun.IndexOf(startSymbol, stringComparison);
                    if (indexInRun >= 0)
                    {
                        position = position.GetPositionAtOffset(indexInRun);
                        TextPointer nextPointer = null;
                        for (int i = 0; i < input.Length; i++)
                        {
                            if (nextPointer == null)
                            {
                                nextPointer = position.GetNextInsertionPosition(LogicalDirection.Forward);
                            }
                            else
                            {
                                nextPointer = nextPointer.GetNextInsertionPosition(LogicalDirection.Forward);
                            }
                        }
                        if (nextPointer != null)
                        {
                            //TextPointer nextPointer = position.GetPositionAtOffset(input.Length);
                            var tmpRange = new TextRange(position, nextPointer);
                            if (tmpRange.Text == input)
                            {
                                textRange = tmpRange;
                                // If a none-WholeWord match is found, directly terminate the loop.
                                position = position.GetPositionAtOffset(input.Length);
                                break;
                            }
                            else
                            {
                                position = position.GetNextInsertionPosition(LogicalDirection.Forward);
                            }
                        }
                        else
                        {
                            position = position.GetNextInsertionPosition(LogicalDirection.Forward);
                        }
                    }
                    else
                    {
                        // If a match is not found, go over to the next context position after the "textRun".
                        position = position.GetPositionAtOffset(textRun.Length);
                    }
                }
                else
                {
                    //If the current position doesn't represent a text context position, go to the next context position.
                    // This can effectively ignore the formatting or embedded element symbols.
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

            return textRange;
        }
    }

    internal class TagFontInfo
    {
        private double fontSize;
        /// <summary>
        /// 
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            private set { fontSize = value; }
        }
        private FontWeight fontWeight;
        /// <summary>
        /// 
        /// </summary>
        public FontWeight FontWeight
        {
            get { return fontWeight; }
            private set { fontWeight = value; }
        }
        private FontFamily fontFamily;
        /// <summary>
        /// 
        /// </summary>
        public FontFamily FontFamily
        {
            get { return fontFamily; }
            private set { fontFamily = value; }
        }
        private FontStyle fontStyle;
        /// <summary>
        /// 
        /// </summary>
        public FontStyle FontStyle
        {
            get { return fontStyle; }
            private set { fontStyle = value; }
        }
        private Brush foreground;
        /// <summary>
        /// 
        /// </summary>
        public Brush Foreground
        {
            get { return foreground; }
            private set { foreground = value; }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="tagTextRange"></param>
        public TagFontInfo(TextRange tagTextRange)
        {
            TextRange firstSymbol = new TextRange(tagTextRange.Start, tagTextRange.Start.GetNextInsertionPosition(LogicalDirection.Forward));
            FontSize = (double)ProtocolPatternMakerHelper.GetPropertyValue(firstSymbol, TextElement.FontSizeProperty);
            FontWeight = (FontWeight)ProtocolPatternMakerHelper.GetPropertyValue(firstSymbol, TextElement.FontWeightProperty);
            FontStyle = (FontStyle)ProtocolPatternMakerHelper.GetPropertyValue(firstSymbol, TextElement.FontStyleProperty);
            FontFamily = (FontFamily)ProtocolPatternMakerHelper.GetPropertyValue(firstSymbol, TextElement.FontFamilyProperty);
            Foreground = ProtocolPatternMakerHelper.GetPropertyValue(firstSymbol, TextElement.ForegroundProperty) as Brush;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textElement"></param>
        public void SetFontToTextElement(TextElement textElement)
        {
            textElement.FontSize = FontSize;
            textElement.FontWeight = FontWeight;
            textElement.FontStyle = FontStyle;
            textElement.FontFamily = FontFamily;
            textElement.Foreground = Foreground;
        }
    }    
}
