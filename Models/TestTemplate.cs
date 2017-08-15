using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using NeuroSoft.WPFComponents.ScalableWindows;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.DeviceAutoTest.TestTemplateEditor;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using System.Text.RegularExpressions;
using NeuroSoft.Prototype.Interface;
using System.IO.Compression;
using NeuroSoft.WPFComponents.ProtocolPatternMaker.Serialization;
using NeuroSoft.Prototype.Database;
using System.Xml;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Сценарий тестирования устройства
    /// </summary>
    [Serializable]
    public class DATTemplate : DATTemplateTreeViewItem
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public DATTemplate(string deviceType, string name)
        {
            if (guid == null || guid == Guid.Empty)
            {
                guid = Guid.NewGuid();
            }
            DeviceType = deviceType;
            Name = name;
            LastEditDateTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DATTemplate(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        #region Fields And Properties
        [Serialize]
        private string deviceType;
        [Serialize]
        private Guid guid = Guid.Empty;
        [Serialize]
        private SerializedList<DATVariableDescriptor> variables = new SerializedList<DATVariableDescriptor>();
        [Serialize]
        private SerializedList<Guid> availableProtocolPatternGuids = new SerializedList<Guid>();
        [Serialize]
        private SerializedList<TestItemGroup> testGroups = new SerializedList<TestItemGroup>();
        [Serialize]
        private int version = 1;
        [Serialize]
        private long lastEditDate = -1;
        private bool isUsed = false;
        [Serialize]
        private string defaultCardInfoPath = "";

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid GUID
        {
            get 
            {
                if (guid == null || guid == Guid.Empty)
                {
                    guid = Guid.NewGuid();
                }
                return guid;
            }            
        }

        /// <summary>
        /// Тип устройства, для которого создан данный сценарий.
        /// </summary>
        public string DeviceType
        {
            get { return deviceType; }
            set 
            {
                if (deviceType != value)
                {
                    deviceType = value;
                    OnPropertyChanged("DeviceType");
                }
            }
        }

        /// <summary>
        /// Версия инструкции
        /// </summary>
        public int Version
        {
            get { return version; }
            internal set
            {
                if (version != value)
                {
                    version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        /// <summary>
        /// Дата последнего изменения
        /// </summary>
        internal long LastEditDateTicks
        {
            get 
            {                
                return lastEditDate; 
            }
            set
            {                
                if (lastEditDate != value)
                {
                    lastEditDate = value;
                    OnPropertyChanged("LastEditDateTicks");
                    OnPropertyChanged("LastEditDate");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastEditDate
        {
            get
            {
                if (lastEditDate < 0)
                    return null;
                return new DateTime(lastEditDate, DateTimeKind.Utc).ToLocalTime();
            }
        }
        /// <summary>
        /// Признак того, что данная инструкция уже используется для наладки
        /// </summary>
        internal bool IsUsed
        {
            get { return isUsed; }
            set { isUsed = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static string GetDBKey(DATTemplateDescriptor descriptor)
        {
            return GetDBKeyByGuidAndVersion(descriptor.GUID, descriptor.Version);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string GetDBKeyByGuidAndVersion(Guid guid, int version)
        {
            return guid.ToString() + ".v" + version;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static string GetLastEditDBKey(DATTemplateDescriptor descriptor)
        {
            return GetLastEditDBKey(descriptor.GUID, descriptor.Version);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string GetLastEditDBKey(Guid guid, int version)
        {
            return GetDBKeyByGuidAndVersion(guid, version) + "_LastEditDate";
        }
        /// <summary>
        /// Ключ шаблона для доступа в базе данных
        /// </summary>
        public string TemplateDBKeyString
        {
            get
            {
                return GetDBKeyByGuidAndVersion(GUID, Version);
            }
        }

        /// <summary>
        /// Ключ шаблона для доступа к полю, содержащему значение даты последнего изменения шаблона
        /// </summary>
        internal string LastEditDateDBKey
        {
            get
            {
                return GetLastEditDBKey(GUID, Version);
            }
        }

        /// <summary>
        /// Путь к картотеке по умолчанию
        /// </summary>
        public string DefaultCardInfoPath
        {
            get { return defaultCardInfoPath; }
            set
            {
                if (defaultCardInfoPath != value)
                {
                    defaultCardInfoPath = value;
                    OnPropertyChanged("DefaultCardInfoPath");
                    OnModified();
                }
            }
        }

        /// <summary>
        /// Версия инструкции в виде строки
        /// </summary>
        public string VersionString
        {
            get { return Version.ToString(); }
        }

        /// <summary>
        /// Переменные, участвующие в тестировании.
        /// </summary>
        public SerializedList<DATVariableDescriptor> Variables
        {
            get { return variables; }
        }

        /// <summary>
        /// Группы тестов
        /// </summary>
        public SerializedList<TestItemGroup> TestGroups
        {
            get { return testGroups; }
        }

        /// <summary>
        /// Переменные с информацией о тестах
        /// </summary>
        public List<DATVariableDescriptor> TestInfoVariables
        {
            get
            {
                List<DATVariableDescriptor> result = new List<DATVariableDescriptor>();
                foreach (var test in GetAllTests())
                {
                    result.AddRange(TestInfoVariableHelper.GetTestInfoVariables(test));
                }
                return result;
            }
        }
      
        /// <summary>
        /// Список guid'ов шаблонов протоколов, доступных для данного сценария тестирования
        /// </summary>
        public SerializedList<Guid> AvailableProtocolPatternGuids
        {
            get { return availableProtocolPatternGuids; }
        }

        private static DATTemplate currentTemplate;
        /// <summary>
        /// Текущая используемая инструкция по наладке
        /// </summary>
        public static DATTemplate CurrentTemplate
        {
            get
            {
                return currentTemplate;
            }
            set
            {
                if (currentTemplate != value)
                {
                    currentTemplate = value;
                    if (MainWindow.CurrentMainWindow is MainWindow)                        
                    {
                        if (!ProtocolPattern.IsDefaultDepositoryUsing)
                        {
                            ProtocolPattern.CurrentDepository.Save();
                        }
                        if (currentTemplate != null)
                        {
                            string currentProtocolPatternsPath = currentTemplate.ProtocolPatternsDepositoryPath;
                            ProtocolPattern.CurrentDepository = new Components.Depository.Depository(currentProtocolPatternsPath, "");
                        }
                        else
                        {
                            ProtocolPattern.CurrentDepository = null;
                        }
                        (MainWindow.CurrentMainWindow as MainWindow).UpdateProtocolPatternList();
                    }
                }
            }
        }

        /// <summary>
        /// Путь для хранения изображений
        /// </summary>
        internal string ImagesPath
        {
            get 
            {
                return NeuroSoft.Prototype.Interface.Globals.GetConfigFolder() + Path.DirectorySeparatorChar + ShortImagesPath; 
            }
        }

        /// <summary>
        /// Путь для хранения изображений относительно папки с настройками программы
        /// </summary>
        internal string ShortImagesPath
        {
            get
            {
                return "Images" + Path.DirectorySeparatorChar + GUID.ToString();
            }
        }
        /// <summary>
        /// Обозначение пути к папке с настройками программы для сериализации/десериализации ссылок на изображения
        /// </summary>
        internal const string ConfigFolderUriLink = @"%ConfigFolder%";
        /// <summary>
        /// 
        /// </summary>
        public const string AvailableImageTypesPattern = @"^(\.jpg|\.jpeg|\.bmp|\.png)$";

        /// <summary>
        /// Путь к папке с инструкциями
        /// </summary>
        public static string TemplatesFolder
        {
            get
            {                
                string templatesFolder = NeuroSoft.Prototype.Interface.Globals.GetConfigFolder() + Path.DirectorySeparatorChar + "Templates";
                if (!Directory.Exists(templatesFolder))
                {
                    Directory.CreateDirectory(templatesFolder);
                }
                return templatesFolder;
            }
        }

        /// <summary>
        /// Путь к папке с шаблонами протоколов инструкций
        /// </summary>
        public static string ProtocolPatternsFolder
        {
            get
            {
                string protocolPatternsFolder = NeuroSoft.Prototype.Interface.Globals.GetConfigFolder() + Path.DirectorySeparatorChar + "ProtocolPatterns";
                if (!Directory.Exists(protocolPatternsFolder))
                {
                    Directory.CreateDirectory(protocolPatternsFolder);
                }
                return protocolPatternsFolder;
            }
        }
        /// <summary>
        /// Путь к файлу с шаблонами протоколов для данной инструкции
        /// </summary>
        public string ProtocolPatternsDepositoryPath
        {
            get
            {
                return DATTemplate.ProtocolPatternsFolder + Path.DirectorySeparatorChar + currentTemplate.GUID.ToString() + ".datprotocol";
            }
        }
        #endregion
        #region Methods

        #region Операции над тестами сценария
        /// <summary>
        /// Создание нового теста
        /// </summary>
        /// <param name="parent"></param>
        public TestTemplateItem CreateNewTestItem(DATTemplateTreeViewItem parent)
        {            
            TestItemInfoDialog newTestDialog = new TestItemInfoDialog(this);
            if (newTestDialog.ShowDialog() == true)
            {
                TestTemplateItem test = null;
                if (newTestDialog.IsBasedOnTest)
                {
                    test = newTestDialog.TestBase.Clone() as TestTemplateItem;                    
                    test.TestId = newTestDialog.TestId;                    
                    test.Parent = parent;
                }
                else
                {
                    test = new TestTemplateItem(parent, newTestDialog.TestId);
                    test.IsContainer = newTestDialog.IsContainer;
                }
                test.Name = newTestDialog.TestName;
                return test;
            }
            return null;
        }

        #endregion

        #region Основные операции над сценариями тестирования
        /// <summary>
        /// Открытие сценария тестирования из списка
        /// </summary>
        public static void OpenTestTemplate()
        {
            TemplatesListDialog openDialog = new TemplatesListDialog();
            if (openDialog.ShowDialog() == true)
            {                
                DATTemplate template = null;
                if (openDialog.CreateNewTemplate)
                {
                    template = CreateTestTemplate();
                } 
                else if (openDialog.SelectedTemplate != null)
                {
                    template = openDialog.SelectedTemplate.GetTestTemplate();
                }
                EditTestTemplate(template);
            }
        }
        /// <summary>
        /// Вызов окна редактирования шаблона тестов.
        /// </summary>
        /// <param name="template"></param>
        public static void EditTestTemplate(DATTemplate template)
        {
            if (template == null) return;
            TestTemplateEditor.TestTemplateEditorWindow editor = new TestTemplateEditorWindow(new TestTemplateViewModel(template));
            var savedCurrentTemplate = CurrentTemplate;
            CurrentTemplate = template;
            try
            {
                editor.ShowDialog();
            }
            finally
            {
                CurrentTemplate = savedCurrentTemplate;
            }            
        }

        /// <summary>
        /// Создание нового шаблона тестов
        /// </summary>
        /// <returns></returns>
        public static DATTemplate CreateTestTemplate()
        {            
            TestTemplateInfoDialog templateInfoDialog = new TestTemplateInfoDialog();
            if (templateInfoDialog.ShowDialog() == true)
            {
                DATTemplate template = new DATTemplate(templateInfoDialog.SelectedDevice.DeviceType, templateInfoDialog.TemplateName);
                template.Save();
                return template;
            }
            return null;
        }

        #endregion

        #region Список описателей шаблона тестов
        private static ObservableCollection<DATTemplateDescriptor> testTemplateDescriptors = null;

        /// <summary>
        /// Список описателей шаблонов
        /// </summary>
        public static ObservableCollection<DATTemplateDescriptor> TestTemplateDescriptors
        {
            get 
            {
                if (testTemplateDescriptors == null)
                {
                    testTemplateDescriptors = new ObservableCollection<DATTemplateDescriptor>();
                    UpdateTestTemplateDescriptorsList();
                }
                return testTemplateDescriptors;
            }
        }

        /// <summary>
        /// Обновление списка описателей шаблонов
        /// </summary>        
        public static void UpdateTestTemplateDescriptorsList()
        {
            TestTemplateDescriptors.Clear();
            List<DATTemplateDescriptor> descriptors = new List<DATTemplateDescriptor>();
            foreach (var file in Directory.GetFiles(TemplatesFolder, "*." + DATTemplateExt))
            {
                var templateInfo = ReadTemplateInfoFromXml(file);
                if (templateInfo == null) continue;
                descriptors.Add(templateInfo);
            }
            var orderedDescs = descriptors.OrderByDescending(testTemp => testTemp.FullName);
            foreach (var descr in orderedDescs)
            {
                TestTemplateDescriptors.Add(descr);
            }
        }
        #endregion

        #region Операции сохранения, удаления и загрузки шаблонов
        /// <summary>
        /// Генерация описателя инструкции по Guid и версии
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static DATTemplateDescriptor FindDescriptor(Guid guid, int version)
        {
            return ReadTemplateInfoFromXml(FindFileName(guid, version));            
        }

        /// <summary>
        /// Удаление шаблона
        /// </summary>
        /// <param name="fileName"></param>        
        public static void RemoveTemplate(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                UpdateTestTemplateDescriptorsList();
            }
            else
            {
                NSMessageBox.Show(Properties.Resources.TemplateNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Удаление шаблона
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="version"></param>
        public static void RemoveTemplate(Guid guid, int version)
        {
            string fileName = FindFileName(guid, version);
            RemoveTemplate(fileName);
        }

        /// <summary>
        /// Удаление шаблона
        /// </summary>
        public void RemoveTemplate()
        {
            RemoveTemplate(GUID, Version);
        }

        /// <summary>
        /// Сохранение данных шаблона в депозиторий
        /// </summary>  
        public void Save()
        {
            if (CustomSave != null)
            {
                CustomSave(this, new RoutedEventArgs());
            }       
            else
            {
                string fileName = TemplatesFolder + Path.DirectorySeparatorChar + GUID + "(v." + Version + ")." + DATTemplateExt;
                WriteTemplateAsXml(fileName);
            }
        }

        #region SaveAndLoad        
        /// <summary>
        /// Событие пользовательского сохранения данных инструкции
        /// </summary>
        public event RoutedEventHandler CustomSave;

        /// <summary>
        /// Расширение для файлов с инструкциями
        /// </summary>
        private const string DATTemplateExt = "dattemplate";

        private const string DatTemplateTagName = "DATTemplate";
        private const string GuidAttribute = "Guid";
        private const string VersionAttribute = "Version";
        private const string NameAttribute = "Name";
        private const string DeviceTypeAttribute = "DeviceType";
        private const string LastEditDateAttribute = "LastEditDate";
        private const string DefaultCardPathAttribute = "DefaultCardPath";
        

        private void WriteTemplateAsXml(String fileName)
        {
            var fXmlWriterSettings = new XmlWriterSettings();
            fXmlWriterSettings.Encoding = Encoding.UTF8;
            fXmlWriterSettings.NewLineOnAttributes = false;
            fXmlWriterSettings.Indent = false;
            var xmlWriter = XmlWriter.Create(fileName, fXmlWriterSettings);
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlNode root = doc.CreateElement(DatTemplateTagName);
                AppendAttribute(doc, root, GuidAttribute, GUID.ToString());
                AppendAttribute(doc, root, VersionAttribute, Version.ToString());
                AppendAttribute(doc, root, NameAttribute, Name);
                AppendAttribute(doc, root, DeviceTypeAttribute, DeviceType);
                AppendAttribute(doc, root, LastEditDateAttribute, LastEditDateTicks.ToString());
                AppendAttribute(doc, root, DefaultCardPathAttribute, DefaultCardInfoPath);
                root.AppendChild(doc.CreateCDataSection(SerializeTemplate()));
                doc.AppendChild(root);
                doc.Save(xmlWriter);
            }
            finally
            {
                xmlWriter.Flush();
                xmlWriter.Close();
                UpdateTestTemplateDescriptorsList();
            }
        }

        private static XmlNode LoadTemplateElementFromFile(string fileName)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(fileName);
            var templateNodes = xmlDocument.GetElementsByTagName(DatTemplateTagName);
            if (templateNodes.Count < 1)
                return null;
            var templateNode = templateNodes[0];
            if (templateNode == null || !(templateNode.FirstChild is XmlCDataSection))
                return null;
            return templateNode;
        }
        /// <summary>
        /// Десериализация инструкции из xml файла
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static DATTemplate ReadTemplateFromXml(string fileName)
        {
            var root = LoadTemplateElementFromFile(fileName);
            var cData = root.FirstChild as XmlCDataSection;
            if (cData == null)
                return null;
            byte[] dataBytes = FlowDocumentDeserialization.ConvertFromString(cData.Value);            
            using (MemoryStream ms = new MemoryStream(dataBytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                DATTemplate result = formatter.Deserialize(ms) as DATTemplate;
                return result;
            }
        }

        /// <summary>
        /// Чтение информации об инструкции из xml файла
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DATTemplateDescriptor ReadTemplateInfoFromXml(string fileName)
        {
            var root = LoadTemplateElementFromFile(fileName);
            if (root == null)
                return null;
            Guid guid = Guid.Parse(root.Attributes[GuidAttribute].Value);
            string name = root.Attributes[NameAttribute].Value;
            string deviceType = root.Attributes[DeviceTypeAttribute].Value;
            string defaultCardPath = "";
            if (root.Attributes[DefaultCardPathAttribute] != null)
            {
                defaultCardPath = root.Attributes[DefaultCardPathAttribute].Value;
            }
            int version = -1;
            if (root.Attributes[VersionAttribute] != null)
            {
                Int32.TryParse(root.Attributes[VersionAttribute].Value, out version);
            }
            long lastEditDate = -1;
            if (root.Attributes[LastEditDateAttribute] != null)
            {
                long.TryParse(root.Attributes[LastEditDateAttribute].Value, out lastEditDate);
            }
            return new DATTemplateDescriptor(guid, name, deviceType, version, lastEditDate, defaultCardPath);
        }

        private string SerializeTemplate()
        {
            using (MemoryStream ms = new MemoryStream(100000))
            {
                Save(ms);                    
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return FlowDocumentSerialization.BytesToString(ms.ToArray());
            }
        }

        private void AppendAttribute(XmlDocument doc, XmlNode node, string attrName, string attrValue)
        {            
            var attr = doc.CreateAttribute(attrName);
            attr.Value = attrValue;
            node.Attributes.Append(attr);
        }

        private const string TemplateNameInAcriche = "TemplateContent";
        /// <summary>
        /// Экспорт данных шаблона в файл
        /// </summary>
        /// <param name="fileName"></param>
        public void Export(string fileName)
        {
            using (ZipStorer zip = ZipStorer.Create(fileName, "Device Auto Test template archive"))
            {
                zip.EncodeUTF8 = true;
                string imgPath = ImagesPath;
                if (Directory.Exists(imgPath))
                {
                    //запакуем помимо самой инструкции все картинки, на которые могут быть ссылки внутри инструкции
                    DirectoryInfo dir = new DirectoryInfo(imgPath);
                    foreach (var fileInfo in dir.GetFiles())
                    {
                        if (Regex.IsMatch(fileInfo.Extension, DATTemplate.AvailableImageTypesPattern))
                        {
                            zip.AddFile(ZipStorer.Compression.Store, fileInfo.FullName, ShortImagesPath + Path.DirectorySeparatorChar + fileInfo.Name, "");
                        }
                    }
                }                
                MemoryStream ms = new MemoryStream();
                {
                    Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    zip.AddStream(ZipStorer.Compression.Store, TemplateNameInAcriche, ms, DateTime.Now, "Template Content");
                    ms.Flush();
                    ms.Close();
                }
            }
        }
        /// <summary>
        /// Сохранение инструкции в потоке
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream)
        {
            bool savedIsUsed = IsUsed;
            try
            {
                IsUsed = false;                
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);                
            }
            finally
            {
                IsUsed = savedIsUsed;
            }
        }

        internal void ResetGuid()
        {
            guid = Guid.Empty;
        }

        /// <summary>
        /// Поиск файла, содержащего данные инструкции
        /// </summary>        
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string FindFileName(Guid guid)
        {
            return FindFileName(guid, -1);
        }

        /// <summary>
        /// Поиск файла, содержащего данные инструкции
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string FindFileName(Guid guid, int version)
        {
            string fileName = null;            
            int lastVersion = -1;       
            //найдем файл с искомой инструкцией
            foreach (var file in Directory.GetFiles(TemplatesFolder, "*." + DATTemplateExt))
            {
                var templateInfo = ReadTemplateInfoFromXml(file);
                if (templateInfo == null || templateInfo.GUID != guid) continue;
                if (version > 0 && templateInfo.Version == version)
                {
                    fileName = file;
                    break;
                }
                else if (version < 1 && templateInfo.Version > lastVersion)
                {
                    lastVersion = templateInfo.Version;
                    fileName = file;
                }
            }
            return fileName;
        }

        /// <summary>
        /// Загрузка данных шаблона
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static DATTemplate Load(Guid guid)
        {
            return Load(FindFileName(guid));
        }
        /// <summary>
        /// Загрузка данных шаблона
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static DATTemplate Load(Guid guid, int version)
        {
            return Load(FindFileName(guid, version));
        }
        /// <summary>
        /// Загрузка данных шаблона
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static DATTemplate Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                NSMessageBox.Show(Properties.Resources.TemplateNotFound, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            try
            {
                DATTemplate pattern = ReadTemplateFromXml(fileName);
                return PrepareLoadedTemplate(pattern);
            }
            catch (Exception ex)
            {
                NSMessageBox.Show(string.Format(Properties.Resources.LoadTemplateError, ex.Message), Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        
        /// <summary>
        /// Импорт инструкции из файла
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DATTemplate Import(string fileName)
        {
            try
            {
                DATTemplate template = null;
                using (ZipStorer zip = ZipStorer.Open(fileName, FileAccess.Read))
                {
                    // Read the central directory collection
                    List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();
                    foreach (var entry in dir)
                    {
                        if (entry.FilenameInZip == TemplateNameInAcriche)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                zip.ExtractFile(entry, ms);                                
                                ms.Seek(0, SeekOrigin.Begin);
                                BinaryFormatter formatter = new BinaryFormatter();
                                template = formatter.Deserialize(ms) as DATTemplate;
                            }
                        }
                        else
                        {
                            try
                            {
                                string path = Path.Combine(Globals.GetConfigFolder(), entry.FilenameInZip);
                                zip.ExtractFile(entry, path);
                            }
                            catch { } //если не удалось извлечь файл, то просто продолжим процесс распаковки
                        }
                    }                  
                }
                return PrepareLoadedTemplate(template);
            }
            catch (Exception ex)
            {
                NSMessageBox.Show(string.Format(Properties.Resources.LoadTemplateError, ex.Message), Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Deserialized()
        {
            base.Deserialized();
            TestGroups.ListDeserializationComplete += new System.Windows.Forms.MethodInvoker(TestGroups_ListDeserializationComplete);
        }

        void TestGroups_ListDeserializationComplete()
        {
            TestGroups.ListDeserializationComplete -= new System.Windows.Forms.MethodInvoker(TestGroups_ListDeserializationComplete);
            foreach (var group in TestGroups)
            {
                group.ParentTemplate = this;
            }
        }

        /// <summary>
        /// Подготовка шаблона тестирования к работе после загрузки шаблона
        /// </summary>
        /// <param name="template"></param>
        public static DATTemplate PrepareLoadedTemplate(DATTemplate template)
        {
            return template;
        }
        #endregion
        #endregion

        /// <summary>
        /// Уведомление об изменении шаблона
        /// </summary>
        public event RoutedEventHandler Modified;

        /// <summary>
        /// 
        /// </summary>
        protected internal void OnModified()
        {
            if (Modified != null)
            {
                Modified(this, new RoutedEventArgs());
            }
        }
        /// <summary>
        /// Сохранение копии инструкции в базе данных
        /// </summary>
        /// <param name="connection"></param>
        public void SaveInDatabase(DataConnection connection)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(100000))
            {
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                Globals.CurrentConnection.Connection.WriteData(this.TemplateDBKeyString, ms.ToArray());
            }
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, this.LastEditDateTicks);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                Globals.CurrentConnection.Connection.WriteData(this.LastEditDateDBKey, ms.ToArray());
            }
        }
        #endregion
    }

    /// <summary>
    /// Описатель сценария тестирования
    /// </summary>
    [Serializable]
    public class DATTemplateDescriptor : INotifyPropertyChanged, IComparable
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public DATTemplateDescriptor(Guid GUID)
        {
            this.GUID = GUID;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public DATTemplateDescriptor(Guid GUID, string Name, string DeviceType, int version, long lastEditDate, string defaultCardPath)
        {
            this.GUID = GUID;
            this.Name = Name;
            this.DeviceType = DeviceType;
            this.Version = version;
            this.LastEditDateTicks = lastEditDate;
            this.DefaultCardPath = defaultCardPath;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public DATTemplateDescriptor(DATTemplate template)
        {
            this.GUID = template.GUID;
            this.Name = template.Name;
            this.DeviceType = template.DeviceType;
            this.Version = template.Version;
            this.DefaultCardPath = template.DefaultCardInfoPath;
        }

        #endregion
        
        private Guid guid;        
        private string name;
        private string deviceType;
        private int version = -1;
        private long lastEditDate = -1;
        private string defaultCardPath;
        
        
        /// <summary>
        /// 
        /// </summary>
        public int Version
        {
          get { return version; }
          internal set 
          { 
              version = value; 
              OnPropertyChanged("Version");
              OnPropertyChanged("FullName");
          }
        }

        /// <summary>
        /// Guid сценария тестирования
        /// </summary>
        public Guid GUID
        {
            get { return guid; }
            internal set 
            { 
                guid = value;
                OnPropertyChanged("GUID");
            }
        }

        /// <summary>
        /// Имя сценария тестирования
        /// </summary>
        public string Name
        {
            get { return name; }
            internal set
            {
                name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("FullName");
            }
        }
        /// <summary>
        /// Полное имя (с учетом версии)
        /// </summary>
        public string FullName
        {
            get { return Name + " (v." + Version + ")"; }
        }
        
        /// <summary>
        /// Тип устройства, для которого создан данный сценарий.
        /// </summary>
        public string DeviceType
        {
            get { return deviceType; }
            set
            {
                if (deviceType != value)
                {
                    deviceType = value;
                    OnPropertyChanged("DeviceType");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal long LastEditDateTicks
        {
            get { return lastEditDate; }
            set
            {
                lastEditDate = value;                
                OnPropertyChanged("LastEditDate");                
            }
        }        

        private string fileName;
        /// <summary>
        /// Имя файла, содержащего данные инструкции
        /// </summary>
        public string FileName
        {
            get 
            {
                if (!File.Exists(fileName))
                {
                    fileName = DATTemplate.FindFileName(GUID, Version);
                }
                return fileName; 
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        public string DefaultCardPath
        {
            get { return defaultCardPath; }
            internal set
            {
                if (defaultCardPath != value)
                {
                    defaultCardPath = value;
                    OnPropertyChanged("DefaultCardPath");
                }
            }
        }

        /// <summary>
        /// Метод возвращает шаблон, соответствующий описателю
        /// </summary>
        public DATTemplate GetTestTemplate()
        {            
            return DATTemplate.Load(FileName);
        }        

        /// <summary>
        /// Получение даты последнего изменения инструкции, хранящейся в базе данных
        /// </summary>
        /// <param name="connection"></param>
        public long GetDBLastEditDate(DataConnection connection)
        {
            byte[] data = Globals.CurrentConnection.Connection.ReadData(DATTemplate.GetLastEditDBKey(this));
            if (data == null)
                return -1;
            BinaryFormatter formatter = new BinaryFormatter();            
            using (MemoryStream ms = new MemoryStream(data))
            {
                object value = formatter.Deserialize(ms);
                if (value is long)
                    return (long)value;
            }
            return -1;
        }

        /// <summary>
        /// Загружает инструкцию из базы данных
        /// </summary>
        /// <param name="connection"></param>
        public DATTemplate LoadTemplateFromDB(DataConnection connection)
        {
            byte[] data = Globals.CurrentConnection.Connection.ReadData(DATTemplate.GetDBKey(this));
            if (data == null)
                return null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object value = formatter.Deserialize(ms);
                return value as DATTemplate;
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Сравнение на равенство
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var descr = obj as DATTemplateDescriptor;
            if (descr == null)
                return false;
            return GUID == descr.GUID && Version == descr.Version;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Событие на изменение свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Уведомление об изменении свойства (все объекты представления привязаные к этому свойству автоматически обновят себя)
        /// </summary>
        /// <param name="propertyName">Имя свойства принимающего новое значение</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var descr = obj as DATTemplateDescriptor;
            if (descr == null)
                return -1;
            if (GUID == descr.GUID && Version == descr.Version)
                return 0;
            if (GUID == GUID)
            {
                return descr.Version.CompareTo(Version);
            }
            else
            {
                return GUID.CompareTo(descr.GUID);
            }
        }

        private int newCorrectionsCount;
        /// <summary>
        /// Количество новых исправлений в базе знаний
        /// </summary>
        public int NewCorrectionsCount
        {
            get { return newCorrectionsCount; }
            private set
            {
                if (newCorrectionsCount != value)
                {
                    newCorrectionsCount = value;
                    OnPropertyChanged("NewCorrectionsCount");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public void CheckForNewCorrections()
        {            
            var corrections = TemplateCorrections.Read(Globals.CurrentConnection.Connection, GUID.ToString());
            if (corrections == null)
            {
                NewCorrectionsCount = 0;
            }
            else
            {
                NewCorrectionsCount = corrections.Corrections.Count();
            }
        }
    }
}
