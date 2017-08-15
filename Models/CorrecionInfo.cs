using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using NeuroSoft.Prototype.Database;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TemplateCorrections : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="templateGuid"></param>        
        public TemplateCorrections(string templateGuid)
        {
            TemplateGuid = templateGuid;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected TemplateCorrections(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Static
        /// <summary>
        /// 
        /// </summary>
        public const string CorrectionsKeyPrefix = "TemplateCorrections_";
        /// <summary>
        /// Ключ параметра, содержащего список новых описаний возможных исправлений для инструкции
        /// </summary>
        /// <param name="templateGuid"></param>
        /// <returns></returns>
        public static string GetTemplateCorrectionsKey(string templateGuid)
        {
            return CorrectionsKeyPrefix + templateGuid;
        }
        /// <summary>
        /// Чтение из базы списка новых исправлений
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="templateGuid"></param>
        /// <returns></returns>
        public static TemplateCorrections Read(DataConnection connection, string templateGuid)
        {            
            byte[] dataBytes = connection.ReadData(GetTemplateCorrectionsKey(templateGuid));
            if (dataBytes == null)
            {
                return new TemplateCorrections(templateGuid);
            }                
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(dataBytes))
            {
                return formatter.Deserialize(ms) as TemplateCorrections;
            }
        }
            
        #endregion

        #region Local
        [Serialize]
        private string templateGuid;
        [Serialize]
        private SerializedList<CorrectionInfo> corrections = new SerializedList<CorrectionInfo>();
        /// <summary>
        /// Идентификтор теста
        /// </summary>
        public string TemplateGuid
        {
            get { return templateGuid; }
            private set { templateGuid = value; }
        }

        /// <summary>
        /// Список исправлений
        /// </summary>
        public SerializedList<CorrectionInfo> Corrections
        {
            get { return corrections; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="testId"></param>
        public List<CorrectionInfo> GetTestCorrections(string testId)
        {
            List<CorrectionInfo> result = new List<CorrectionInfo>();
            foreach (var correction in Corrections)
            {
                if (correction.TestId == testId)
                    result.Add(correction);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testId"></param>
        /// <param name="correction"></param>
        public void AddCorrection(string testId, string correction)
        {
            AddCorrection(new CorrectionInfo(testId, correction));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="correctionInfo"></param>
        public void AddCorrection(CorrectionInfo correctionInfo)
        {
            if (!Corrections.Contains(correctionInfo))
                Corrections.Add(correctionInfo);
        }

        /// <summary>
        /// Сохранение списка новых исправлений в базе
        /// </summary>
        /// <param name="connection"></param>
        public void Save(DataConnection connection)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(100000))
            {
                formatter.Serialize(ms, this);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                connection.WriteData(GetTemplateCorrectionsKey(TemplateGuid), ms.ToArray());
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CorrectionInfo : SimpleSerializedData
    {
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="testId"></param>
        /// <param name="correctionString"></param>
        public CorrectionInfo(string testId, string correctionString)
        {
            TestId = testId;
            CorrectionString = correctionString;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected CorrectionInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        [Serialize]
        private string testId;
        [Serialize]
        private string correctionString;
        /// <summary>
        /// Идентификтор теста
        /// </summary>
        public string TestId
        {
            get { return testId; }
            private set { testId = value; }
        }

        private string displayTestName;
        /// <summary>
        /// 
        /// </summary>
        public string DisplayTestName
        {
            get
            {
                if (string.IsNullOrEmpty(displayTestName))
                    return TestId;
                return displayTestName;
            }
            set
            {
                if (displayTestName != value)
                {
                    displayTestName = value;
                    OnPropertyChanged("DisplayTestName");
                }
            }
        }
        /// <summary>
        /// Текст исправления
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
                }
            }
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var corrInfo = obj as CorrectionInfo;
            if (corrInfo != null)
                return corrInfo.TestId == TestId && corrInfo.CorrectionString == CorrectionString;
            return base.Equals(obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
