using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace NeuroSoft.DeviceAutoTest.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class DevicesManipulation
    {
        /// <summary>
        /// Идентификатор устройств типа НейроМВП-Микро
        /// </summary>
        public const string NeuroMEPMicoType = "NeuroMEPMicro";
        /// <summary>
        /// Идентификатор устройств типа НейроМВП-МикроМ
        /// </summary>
        public const string NeuroMEPMicoMType = "NeuroMEPMicroM";
        /// <summary>
        /// Идентификатор устройства типа Нейрон-Спектр-1
        /// </summary>
        public const string NS1Type = "NeuronSectrum-1";
        /// <summary>
        /// Идентификатор устройства типа Нейрон-Спектр-2
        /// </summary>
        public const string NS2Type = "NeuronSectrum-2";
        /// <summary>
        /// Идентификатор устройства типа Нейрон-Спектр-3
        /// </summary>
        public const string NS3Type = "NeuronSectrum-3";
        /// <summary>
        /// Идентификатор устройства типа Нейрон-Спектр-4
        /// </summary>
        public const string NS4Type = "NeuronSectrum-4";
        /// <summary>
        /// Идентификатор устройства типа Нейрон-Спектр-1 - Нейрон-Спектр-4
        /// </summary>
        public const string NS1_4Type = "NeuronSectrum-1...4";
        /// <summary>
        /// Идентификатор устройства типа Нейрон-Спектр-4/П
        /// </summary>
        public const string NS4PType = "NeuronSectrum-4/P";
        /// <summary>
        /// Идентификатор устройства типа Нейрон-Спектр-4/ВП
        /// </summary>
        public const string NS4EPType = "NeuronSectrum-4/EP";
        /// <summary>
        /// Идентификатор устройства типа Нейрон-Спектр-5
        /// </summary>
        public const string Ns5Type = "NeuronSpectrum-5";
        /// <summary>
        /// Идентификатор устройства Нейрон-Спектр-4/ВПМ
        /// </summary>
        public const string Ns4EpmType = "NeuronSpectrum-4/EPM";
        /// <summary>
        /// Идентификатор устройства Поли-Спектр-8/EX
        /// </summary>
        public const string PolySpectrum8EXType = "Poly-Spectrum-8/EX";
        /// <summary>
        /// Идентификатор устройства Нейро-ЭМГ-МС
        /// </summary>
        public const string NeuroEMGMSType = "Neuro-EMG-MS";
        /// <summary>
        /// Идентификатор устройства Нейро-ЭМГ-Микро 4
        /// </summary>
        public const string NeuroEmgMicro4type = "Neuro-EMG-Micro 4";
        /// <summary>
        /// Идентификатор устройства Нейро-ЭМГ-Микро 2
        /// </summary>
        public const string NeuroEmgMicro2type = "Neuro-EMG-Micro 2";

        private static ObservableCollection<DeviceTypeInfo> availableDevices = new ObservableCollection<DeviceTypeInfo>()
            {
                new DeviceTypeInfo(NeuroMEPMicoType, Properties.Resources.NeuroMEPMicroName),
                new DeviceTypeInfo(NeuroMEPMicoMType, Properties.Resources.NeuroMEPMicroMName),
                //new DeviceTypeInfo(NS1Type, Properties.Resources.NeuronSpectrum1Name),
                //new DeviceTypeInfo(NS2Type, Properties.Resources.NeuronSpectrum2Name),
                //new DeviceTypeInfo(NS3Type, Properties.Resources.NeuronSpectrum3Name),
                //new DeviceTypeInfo(NS4Type, Properties.Resources.NeuronSpectrum4Name),
                new DeviceTypeInfo(NS1_4Type, Properties.Resources.NeuronSpectrum1_4Name),
                new DeviceTypeInfo(NS4PType, Properties.Resources.NeuronSpectrum4pName),
                new DeviceTypeInfo(NS4EPType, Properties.Resources.NeuronSpectrum4epName),
                new DeviceTypeInfo(Ns4EpmType, Properties.Resources.NeuronSpectrum4epmName),
                new DeviceTypeInfo(Ns5Type, Properties.Resources.NeuronSpectrum5Name),
                new DeviceTypeInfo(PolySpectrum8EXType, Properties.Resources.PolySpectrum8exName),
                new DeviceTypeInfo(NeuroEMGMSType, Properties.Resources.NeuroEmgMsName),
                new DeviceTypeInfo(NeuroEmgMicro4type, Properties.Resources.NeuroEmgMicro4Name),
                new DeviceTypeInfo(NeuroEmgMicro2type, Properties.Resources.NeuroEmgMicro2Name)
            };
        /// <summary>
        /// Информация о типах устройств, с которыми работает программа
        /// </summary>
        public static ObservableCollection<DeviceTypeInfo> AvailableDevices
        {
            get { return availableDevices; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DeviceTypeInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceName"></param>
        public DeviceTypeInfo(string deviceType, string deviceName)
        {
            DeviceType = deviceType;
            Name = deviceName;
        }

        private string deviceType;
        /// <summary>
        /// Тип устройства
        /// </summary>
        public string DeviceType
        {
            get { return deviceType; }
            set { deviceType = value; }
        }

        private string name;
        /// <summary>
        /// Имя устройства, отображаемое пользователю
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(Name) ? Name : DeviceType;
        }
        
    }
}
