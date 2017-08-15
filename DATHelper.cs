using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NeuroSoft.Components.Depository;
using System.Collections;
using NeuroSoft.Common;
using System.Windows.Documents;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using NeuroSoft.Devices;
using System.Text.RegularExpressions;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Класс, содержащий вспомогательные методы для работы с программой автотестирования устройств
    /// </summary>
    public static class DATHelper
    {
        /// <summary>
        /// Имя папки в депозитории, соответствующей сценариям тестирования
        /// </summary>
        public static readonly string DepositoryTemplatesFolder = "DeviceTestTemplates";
        /// <summary>
        /// Имя папки в депозитории, соответствующей дескрипторам сценариев тестирования
        /// </summary>
        public static readonly string DepositoryTemplateDescriptorsFolder = "DeviceTestTemplateDescriptors"; 

        /// <summary>
        /// Клонирование документа FlowDocument
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        internal static FlowDocument Clone(this FlowDocument document)
        {
            SerializedFlowDocumentContent serializedContent = new SerializedFlowDocumentContent();
            serializedContent.Load(document);
            return serializedContent.Save();
        }

        /// <summary>
        /// Преобразовать абсолютный путь в относительный
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <returns></returns>
        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Поиск имен переменных, используемых в тесте
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="onlyInputs">Искать только запрашиваемые переменные</param>
        internal static List<string> GetVariableNames(string inputText, bool onlyInputs = false)
        {
            List<string> result = new List<string>();
            var matches = Regex.Matches(inputText, (onlyInputs ? @"\?" : "") + @"\$(" + DATVariableDescriptor.NameSymbolPattern + ")");
            foreach (Match match in matches)
            {
                if (match.Success && !result.Contains(match.Groups[1].Value))
                {
                    result.Add(match.Groups[1].Value);
                }
            }
            return result;
        }
    }
}
