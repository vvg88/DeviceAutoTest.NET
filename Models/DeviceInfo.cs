using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Common;
using System.Runtime.Serialization;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Информация об устройстве
    /// </summary>
    [Serializable]
    public class DeviceInfo : SimpleSerializedData
    {
        [Serialize]
        private string serialNumber;
        [Serialize]
        private string deviceType;

        /// <summary>
        /// Серийный номер устройства
        /// </summary>
        public string SerialNumber
        {
            get { return serialNumber; }
            set
            {
                if (serialNumber != value)
                {
                    serialNumber = value;
                    OnPropertyChanged("SerialNumber");
                }
            }
        }
        /// <summary>
        /// Тип устройства
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
        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public DeviceInfo(string deviceType, string serialNumber)
        {
            DeviceType = deviceType;
            SerialNumber = serialNumber;
        }

        /// <summary>
        /// Конструктор для десериализации
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DeviceInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
    }
}
