using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class AutoTestSettings : SimpleSerializedData
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public AutoTestSettings()
        {
        }
        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected AutoTestSettings(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [Serialize]
        private int timeout;
        [Serialize]
        private bool resetFinishedTests = true;
        [Serialize]
        private bool manual;
        /// <summary>
        /// Таймаут ожидания выполнения теста (мс)
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set 
            {
                if (timeout != value)
                {
                    timeout = value;
                    OnPropertyChanged("Timeout");
                    NotifyModified();
                }
            }
        }
        
        /// <summary>
        /// Отменять (перевыполнять) ранее выполненные тесты
        /// </summary>
        public bool ResetFinishedTests
        {
            get { return resetFinishedTests; }
            set
            {
                if (resetFinishedTests != value)
                {
                    resetFinishedTests = value;
                    OnPropertyChanged("ResetFinishedTests");
                    NotifyModified();
                }
            }
        }

        /// <summary>
        /// Ручной режим выполнения теста
        /// </summary>
        public bool Manual
        {
            get { return manual; }
            set
            {
                if (manual != value)
                {
                    manual = value;
                    OnPropertyChanged("Manual");
                    NotifyModified();
                }
            }
        }

        internal TestTemplateItem TestParent;
        private void NotifyModified()
        {
            if (TestParent != null)
                TestParent.NotifyModified();
        }
    }    
}
