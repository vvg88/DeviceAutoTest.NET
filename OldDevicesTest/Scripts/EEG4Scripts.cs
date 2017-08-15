using System;
using System.Collections.Generic;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.Hardware.Tools.Usb;
using System.Windows.Controls;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using System.IO;
using NeuroSoft.MathLib.Filters;
using NeuroSoft.Hardware.Tools.Cypress;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts
{
    public static class EEG4Scripts
    {
        #region Вспомогательные функции

        /// <summary>
        /// Возвращает тип тестируемого прибора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Тип тестируемого прибора </returns>
        public static NeuronSpectrumTypes GetDeviceType(ScriptEnvironment environment)
        {
            EEG4Device device = GetDevice(environment);
            if (device != null)
                return GetDeviceType(device);
            return NeuronSpectrumTypes.Null;
        }

        /// <summary>
        /// Возвращает тип тестируемого прибора
        /// </summary>
        /// <param name="device"> Ссылка на  экземпляр устройства </param>
        /// <returns> Тип тестируемого прибора </returns>
        public static NeuronSpectrumTypes GetDeviceType(EEG4Device device)
        {
            //EEG4Device device = GetDevice(environment);
            switch ((device.GetDeviceState().GetState(typeof(EEG4AmplifierState)) as EEG4AmplifierState).deviceType)
            {
                case 1:
                    return NeuronSpectrumTypes.NS_1;
                case 2:
                    return NeuronSpectrumTypes.NS_2;
                case 3:
                    return NeuronSpectrumTypes.NS_3;
                case 4:
                    return NeuronSpectrumTypes.NS_4;
                case 5:
                    return NeuronSpectrumTypes.NS_4EP;
                case 6:
                    return NeuronSpectrumTypes.EMG_Micro4;
                case 7:
                    return NeuronSpectrumTypes.EMG_Micro2;
                case 8:
                    return NeuronSpectrumTypes.NS_4P;
                default:
                    return NeuronSpectrumTypes.Null;
            }
        }

        /// <summary>
        /// Сортирует массив имен отведений по порядку расположения разъемов на передней панели
        /// в зависимости от тестируемого в данный момент устройства
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="originalArray"> Исходный массив </param>
        /// <returns> Отсортированный массив имен </returns>
        public static string[] SortLedNames(ScriptEnvironment environment, string[] originalArray, bool isSortForIndication/*, bool isSortOn = true*/)
        {
            //int derivationNumber = 0;
            string[] destinationStr;
            NeuronSpectrumTypes deviceModel = GetDeviceType(environment);
            switch (deviceModel)
            {
                case NeuronSpectrumTypes.NS_1:
                    destinationStr = new string[13];
                    foreach (string electrodeName in originalArray)
                    {
                        switch (electrodeName)
                        {
                            case "A2":
                                destinationStr[0] = electrodeName;
                                break;
                            case "T4":
                                destinationStr[1] = electrodeName;
                                break;
                            case "FP2":
                                destinationStr[2] = electrodeName;
                                break;
                            case "C4":
                                destinationStr[3] = electrodeName;
                                break;
                            case "O2":
                                destinationStr[4] = electrodeName;
                                break;
                            case "FP1":
                                destinationStr[5] = electrodeName;
                                break;
                            case "C3":
                                destinationStr[6] = electrodeName;
                                break;
                            case "O1":
                                destinationStr[7] = electrodeName;
                                break;
                            case "T3":
                                destinationStr[8] = electrodeName;
                                break;
                            case "A1":
                                destinationStr[9] = electrodeName;
                                break;
                            case "E1_P":
                                destinationStr[10] = isSortForIndication ? "E2_P" : electrodeName;
                                break;
                            case "E1_M":
                                destinationStr[11] = isSortForIndication ? "E2_M" : electrodeName;
                                break;
                            case "ZERO":
                                destinationStr[12] = isSortForIndication ? "E4_P" : electrodeName;
                                break;
                        }
                    }
                    break;
                case NeuronSpectrumTypes.NS_2:
                    destinationStr = new string[21];
                    //if (isSortOn)
                    //{
                        foreach (string electrodeName in originalArray)
                        {
                            switch (electrodeName)
                            {
                                case "A2":
                                    destinationStr[0] = electrodeName;
                                    break;
                                case "F8":
                                    destinationStr[1] = electrodeName;
                                    break;
                                case "T4":
                                    destinationStr[2] = electrodeName;
                                    break;
                                case "T6":
                                    destinationStr[3] = electrodeName;
                                    break;
                                case "FP2":
                                    destinationStr[4] = electrodeName;
                                    break;
                                case "F4":
                                    destinationStr[5] = electrodeName;
                                    break;
                                case "C4":
                                    destinationStr[6] = electrodeName;
                                    break;
                                case "P4":
                                    destinationStr[7] = electrodeName;
                                    break;
                                case "O2":
                                    destinationStr[8] = electrodeName;
                                    break;
                                case "FP1":
                                    destinationStr[9] = electrodeName;
                                    break;
                                case "F3":
                                    destinationStr[10] = electrodeName;
                                    break;
                                case "C3":
                                    destinationStr[11] = electrodeName;
                                    break;
                                case "P3":
                                    destinationStr[12] = electrodeName;
                                    break;
                                case "O1":
                                    destinationStr[13] = electrodeName;
                                    break;
                                case "F7":
                                    destinationStr[14] = electrodeName;
                                    break;
                                case "T3":
                                    destinationStr[15] = electrodeName;
                                    break;
                                case "T5":
                                    destinationStr[16] = electrodeName;
                                    break;
                                case "A1":
                                    destinationStr[17] = electrodeName;
                                    break;
                                case "E1_P":
                                    destinationStr[18] = isSortForIndication ? "E2_P" : electrodeName;
                                    break;
                                case "E1_M":
                                    destinationStr[19] = isSortForIndication ? "E2_M" : electrodeName;
                                    break;
                                case "ZERO":
                                    destinationStr[20] = isSortForIndication ? "E4_P" : electrodeName;
                                    break;
                            }
                        }
                    //}
                    //else
                    //    destinationStr = new string[] { "A2", "F8", "T4", "T6", "FP2", "F4", "C4", "P4", "O2", /*"FZ", "CZ", "PZ",*/ "FP1", "F3", "C3", "P3", "O1", "F7", "T3", "T5", "A1", "E1_P", "E1_M", "ZERO" };
                    break;
                case NeuronSpectrumTypes.NS_3:
                    destinationStr = new string[24];
                    foreach (string electrodeName in originalArray)
                    {
                        switch (electrodeName)
                        {
                            case "A2":
                                destinationStr[0] = electrodeName;
                                break;
                            case "F8":
                                destinationStr[1] = electrodeName;
                                break;
                            case "T4":
                                destinationStr[2] = electrodeName;
                                break;
                            case "T6":
                                destinationStr[3] = electrodeName;
                                break;
                            case "FP2":
                                destinationStr[4] = electrodeName;
                                break;
                            case "F4":
                                destinationStr[5] = electrodeName;
                                break;
                            case "C4":
                                destinationStr[6] = electrodeName;
                                break;
                            case "P4":
                                destinationStr[7] = electrodeName;
                                break;
                            case "O2":
                                destinationStr[8] = electrodeName;
                                break;
                            case "FZ":
                                destinationStr[9] = electrodeName;
                                break;
                            case "CZ":
                                destinationStr[10] = electrodeName;
                                break;
                            case "PZ":
                                destinationStr[11] = electrodeName;
                                break;
                            case "FP1":
                                destinationStr[12] = electrodeName;
                                break;
                            case "F3":
                                destinationStr[13] = electrodeName;
                                break;
                            case "C3":
                                destinationStr[14] = electrodeName;
                                break;
                            case "P3":
                                destinationStr[15] = electrodeName;
                                break;
                            case "O1":
                                destinationStr[16] = electrodeName;
                                break;
                            case "F7":
                                destinationStr[17] = electrodeName;
                                break;
                            case "T3":
                                destinationStr[18] = electrodeName;
                                break;
                            case "T5":
                                destinationStr[19] = electrodeName;
                                break;
                            case "A1":
                                destinationStr[20] = electrodeName;
                                break;
                            case "E1_P":
                                destinationStr[21] = isSortForIndication ? "E2_P" : electrodeName;
                                break;
                            case "E1_M":
                                destinationStr[22] = isSortForIndication ? "E2_M" : electrodeName;
                                break;
                            case "ZERO":
                                destinationStr[23] = isSortForIndication ? "E4_P" : electrodeName;
                                break;
                        }
                    }
                    break;
                case NeuronSpectrumTypes.NS_4:
                    destinationStr = new string[26];
                    foreach (string electrodeName in originalArray)
                    {
                        switch (electrodeName)
                        {
                            case "A2":
                                destinationStr[0] = electrodeName;
                                break;
                            case "F8":
                                destinationStr[1] = electrodeName;
                                break;
                            case "T4":
                                destinationStr[2] = electrodeName;
                                break;
                            case "T6":
                                destinationStr[3] = electrodeName;
                                break;
                            case "FP2":
                                destinationStr[4] = electrodeName;
                                break;
                            case "F4":
                                destinationStr[5] = electrodeName;
                                break;
                            case "C4":
                                destinationStr[6] = electrodeName;
                                break;
                            case "P4":
                                destinationStr[7] = electrodeName;
                                break;
                            case "O2":
                                destinationStr[8] = electrodeName;
                                break;
                            case "FPZ":
                                destinationStr[9] = electrodeName;
                                break;
                            case "FZ":
                                destinationStr[10] = electrodeName;
                                break;
                            case "CZ":
                                destinationStr[11] = electrodeName;
                                break;
                            case "PZ":
                                destinationStr[12] = electrodeName;
                                break;
                            case "OZ":
                                destinationStr[13] = electrodeName;
                                break;
                            case "FP1":
                                destinationStr[14] = electrodeName;
                                break;
                            case "F3":
                                destinationStr[15] = electrodeName;
                                break;
                            case "C3":
                                destinationStr[16] = electrodeName;
                                break;
                            case "P3":
                                destinationStr[17] = electrodeName;
                                break;
                            case "O1":
                                destinationStr[18] = electrodeName;
                                break;
                            case "F7":
                                destinationStr[19] = electrodeName;
                                break;
                            case "T3":
                                destinationStr[20] = electrodeName;
                                break;
                            case "T5":
                                destinationStr[21] = electrodeName;
                                break;
                            case "A1":
                                destinationStr[22] = electrodeName;
                                break;
                            case "E1_P":
                                destinationStr[23] = isSortForIndication ? "E2_P" : electrodeName;
                                break;
                            case "E1_M":
                                destinationStr[24] = isSortForIndication ? "E2_M" : electrodeName;
                                break;
                            case "ZERO":
                                destinationStr[25] = isSortForIndication ? "E4_P" : electrodeName;
                                break;
                        }
                    }
                    break;
                case NeuronSpectrumTypes.NS_4EP:
                    destinationStr = new string[32];
                    foreach (string electrodeName in originalArray)
                    {
                        switch (electrodeName)
                        {
                            case "A2":
                                destinationStr[0] = electrodeName;
                                break;
                            case "F8":
                                destinationStr[1] = electrodeName;
                                break;
                            case "T4":
                                destinationStr[2] = electrodeName;
                                break;
                            case "T6":
                                destinationStr[3] = electrodeName;
                                break;
                            case "FP2":
                                destinationStr[4] = electrodeName;
                                break;
                            case "F4":
                                destinationStr[5] = electrodeName;
                                break;
                            case "C4":
                                destinationStr[6] = electrodeName;
                                break;
                            case "P4":
                                destinationStr[7] = electrodeName;
                                break;
                            case "O2":
                                destinationStr[8] = electrodeName;
                                break;
                            case "FPZ":
                                destinationStr[9] = electrodeName;
                                break;
                            case "FZ":
                                destinationStr[10] = electrodeName;
                                break;
                            case "CZ":
                                destinationStr[11] = electrodeName;
                                break;
                            case "PZ":
                                destinationStr[12] = electrodeName;
                                break;
                            case "OZ":
                                destinationStr[13] = electrodeName;
                                break;
                            case "FP1":
                                destinationStr[14] = electrodeName;
                                break;
                            case "F3":
                                destinationStr[15] = electrodeName;
                                break;
                            case "C3":
                                destinationStr[16] = electrodeName;
                                break;
                            case "P3":
                                destinationStr[17] = electrodeName;
                                break;
                            case "O1":
                                destinationStr[18] = electrodeName;
                                break;
                            case "F7":
                                destinationStr[19] = electrodeName;
                                break;
                            case "T3":
                                destinationStr[20] = electrodeName;
                                break;
                            case "T5":
                                destinationStr[21] = electrodeName;
                                break;
                            case "A1":
                                destinationStr[22] = electrodeName;
                                break;
                            case "E1_P":
                                destinationStr[23] = electrodeName;
                                break;
                            case "E1_M":
                                destinationStr[24] = electrodeName;
                                break;
                            case "E2_P":
                                destinationStr[25] = electrodeName;
                                break;
                            case "E2_M":
                                destinationStr[26] = electrodeName;
                                break;
                            case "E3_P":
                                destinationStr[27] = electrodeName;
                                break;
                            case "E3_M":
                                destinationStr[28] = electrodeName;
                                break;
                            case "E4_P":
                                destinationStr[29] = electrodeName;
                                break;
                            case "E4_M":
                                destinationStr[30] = electrodeName;
                                break;
                            case "ZERO":
                                destinationStr[31] = electrodeName;
                                break;
                        }
                    }
                    break;
                case NeuronSpectrumTypes.EMG_Micro4:
                    destinationStr = new string[8];
                    foreach (string electrodeName in originalArray)
                    {
                        switch (electrodeName)
                        {
                            case "E1_P":
                                destinationStr[1] = electrodeName;
                                break;
                            case "E1_M":
                                destinationStr[2] = electrodeName;
                                break;
                            case "E2_P":
                                destinationStr[3] = electrodeName;
                                break;
                            case "E2_M":
                                destinationStr[4] = electrodeName;
                                break;
                            case "E3_P":
                                destinationStr[5] = electrodeName;
                                break;
                            case "E3_M":
                                destinationStr[6] = electrodeName;
                                break;
                            case "E4_P":
                                destinationStr[7] = electrodeName;
                                break;
                            case "E4_M":
                                destinationStr[8] = electrodeName;
                                break;
                            case "ZERO":
                                destinationStr[9] = electrodeName;
                                break;
                        }
                    }
                    break;
                case NeuronSpectrumTypes.EMG_Micro2:
                    destinationStr = new string[4];
                    foreach (string electrodeName in originalArray)
                    {
                        switch (electrodeName)
                        {
                            case "E1_P":
                                destinationStr[1] = electrodeName;
                                break;
                            case "E1_M":
                                destinationStr[2] = electrodeName;
                                break;
                            case "E2_P":
                                destinationStr[3] = electrodeName;
                                break;
                            case "E2_M":
                                destinationStr[4] = electrodeName;
                                break;
                            case "ZERO":
                                destinationStr[5] = electrodeName;
                                break;
                        }
                    }
                    break;
                case NeuronSpectrumTypes.NS_4P:
                    destinationStr = new string[32];
                    foreach (string electrodeName in originalArray)
                    {
                        switch (electrodeName)
                        {
                            case "A2":
                                destinationStr[0] = electrodeName;
                                break;
                            case "F8":
                                destinationStr[1] = electrodeName;
                                break;
                            case "T4":
                                destinationStr[2] = electrodeName;
                                break;
                            case "T6":
                                destinationStr[3] = electrodeName;
                                break;
                            case "FP2":
                                destinationStr[4] = electrodeName;
                                break;
                            case "F4":
                                destinationStr[5] = electrodeName;
                                break;
                            case "C4":
                                destinationStr[6] = electrodeName;
                                break;
                            case "P4":
                                destinationStr[7] = electrodeName;
                                break;
                            case "O2":
                                destinationStr[8] = electrodeName;
                                break;
                            case "FPZ":
                                destinationStr[9] = electrodeName;
                                break;
                            case "FZ":
                                destinationStr[10] = electrodeName;
                                break;
                            case "CZ":
                                destinationStr[11] = electrodeName;
                                break;
                            case "PZ":
                                destinationStr[12] = electrodeName;
                                break;
                            case "OZ":
                                destinationStr[13] = electrodeName;
                                break;
                            case "FP1":
                                destinationStr[14] = electrodeName;
                                break;
                            case "F3":
                                destinationStr[15] = electrodeName;
                                break;
                            case "C3":
                                destinationStr[16] = electrodeName;
                                break;
                            case "P3":
                                destinationStr[17] = electrodeName;
                                break;
                            case "O1":
                                destinationStr[18] = electrodeName;
                                break;
                            case "F7":
                                destinationStr[19] = electrodeName;
                                break;
                            case "T3":
                                destinationStr[20] = electrodeName;
                                break;
                            case "T5":
                                destinationStr[21] = electrodeName;
                                break;
                            case "A1":
                                destinationStr[22] = electrodeName;
                                break;
                            case "E1_P":
                                destinationStr[23] = electrodeName;
                                break;
                            case "E1_M":
                                destinationStr[24] = electrodeName;
                                break;
                            case "E2_P":
                                destinationStr[25] = electrodeName;
                                break;
                            case "E2_M":
                                destinationStr[26] = electrodeName;
                                break;
                            case "E3_P":
                                destinationStr[27] = electrodeName;
                                break;
                            case "E3_M":
                                destinationStr[28] = electrodeName;
                                break;
                            case "E4_P":
                                destinationStr[29] = electrodeName;
                                break;
                            case "E4_M":
                                destinationStr[30] = electrodeName;
                                break;
                            case "ZERO":
                                destinationStr[31] = electrodeName;
                                break;
                        }
                    }
                    break;
                default:
                    destinationStr = null;
                    break;
            }
            return destinationStr;
        }

        #endregion

        #region Программирование контроллеров

        #region Программирование PIC'ов

        /// <summary>
        /// Сохраняет имя файла прошивки PIC контроллера
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="fileNameTemplate"> Часть имени нужного файла прошивки </param>
        /// <param name="fileName">Имя файла, с помощью которого осущствлялось программирование </param>
        /// <returns></returns>
        public static bool? ProgramChipProg(ScriptEnvironment environment, string fileNameTemplate, ref string fileName, string fwFolder = "")
        {
            var fwFoldr = string.IsNullOrEmpty(fwFolder) ? FirmwareFolder : Directory.Exists(fwFolder) ? fwFolder : @"C:\";
            return CommonScripts.ProgramChipProg(environment, fileNameTemplate, fwFoldr, ref fileName);
        }

        #endregion

        #region Программирование контроллера USB

        /// <summary>
        /// Осуществляет программирование EEPROM контроллера USB в зависимости от ориентации головы
        /// и усилителей, установленных в каналах ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="headAmpType"> Ориентация головы на передней панели и тип усилителей, установленных в каналах ЭЭГ </param>
        /// <returns> Имя файла прошивки или null, если что-то пошло не так </returns>
        public static bool? DoFirmwareEeprom(ScriptEnvironment environment, NeuronSpectrumTypes deviceType, EEG5Scripts.HeadAndAmplifType headAmpType, ref string fileName, string fwFolder = "")
        {
            SelectDeviceDialog selectDeviceDialog = new SelectDeviceDialog();
            if (selectDeviceDialog.ShowDialog() == true)
            {
                string fileNameTemplate = "НС-4_ver*.iic";
                var defAultFwFolder = @"\\SERVER\firmware\_Common\Прошивки для приборов переделанных с CY7C64613-80 на CY7C68013A-100AXI\";
                string firmwareFolder = string.IsNullOrEmpty(fwFolder) ? defAultFwFolder : Directory.Exists(fwFolder) ? fwFolder : @"C:\";
                var usbItem = selectDeviceDialog.SelectedDeviceItem.Device;
                var weavingInformation = new ProgramDescriptor();
                weavingInformation.SerialNumberBuilder = new SerialNumberBuilderNumeric(SerialNumberBuilderNumeric.NumericType.B2, false);
                weavingInformation.Address = 0x80C;
                weavingInformation.PID_Enable = true;
                weavingInformation.PID_Addr = 0x80A;
                switch (deviceType)
                {
                    case NeuronSpectrumTypes.NS_1:
                        weavingInformation.PID_Value = 0x8251;
                        break;
                    case NeuronSpectrumTypes.NS_2:
                        weavingInformation.PID_Value = 0x8252;
                        break;
                    case NeuronSpectrumTypes.NS_3:
                        weavingInformation.PID_Value = 0x8253;
                        break;
                    case NeuronSpectrumTypes.NS_4:
                        weavingInformation.PID_Value = 0x8254;
                        break;
                    case NeuronSpectrumTypes.NS_4P:
                        weavingInformation.PID_Value = 0x8257;
                        break;
                }
                weavingInformation.AddData_Addr = 0x812;
                weavingInformation.CypressCpuType = Hardware.Tools.Cypress.CypressTools.CypressCpuType.FX1_FX2LP;
                switch (headAmpType)
                {
                    case EEG5Scripts.HeadAndAmplifType.A1rightINA118:
                        weavingInformation.AddData_Value = new byte[] { 0, 0, 0, 0 };
                        break;
                    case EEG5Scripts.HeadAndAmplifType.A1leftINA118:
                        weavingInformation.AddData_Value = new byte[] { 1, 0, 0, 0 };
                        break;
                    case EEG5Scripts.HeadAndAmplifType.A1rightLT1168:
                        weavingInformation.AddData_Value = new byte[] { 2, 0, 0, 0 };
                        break;
                    case EEG5Scripts.HeadAndAmplifType.A1leftLT1168:
                        weavingInformation.AddData_Value = new byte[] { 3, 0, 0, 0 };
                        break;
                }
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "iic|" + fileNameTemplate;
                openFileDialog.InitialDirectory = firmwareFolder;
                //openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate);
                if (openFileDialog.ShowDialog() != true)
                {
                    return null;
                }
                weavingInformation.FileName = openFileDialog.FileName;
                bool result = CommonScripts.DoFirmwareEeprom(environment.DeviceSerialNumber, usbItem, weavingInformation);
                if (result)
                {
                    fileName = CommonScripts.GetShortFileName(weavingInformation.FileName);
                    return true;
                }
            }
            return null;
        }

        public static bool? DoFirmwareEeprom(ScriptEnvironment environment, NeuronSpectrumTypes deviceType, ref string fileName)
        {
            SelectDeviceDialog selectDeviceDialog = new SelectDeviceDialog();
            if (selectDeviceDialog.ShowDialog() == true)
            {
                string fileNameTemplate = "НС-4_ver*.iic";
                string firmwareFolder = @"\\SERVER\firmware\_Common\Прошивки для приборов переделанных с CY7C64613-80 на CY7C68013A-100AXI\";
                if (!Directory.Exists(firmwareFolder))
                    firmwareFolder = @"C:\";
                var usbItem = selectDeviceDialog.SelectedDeviceItem.Device;
                var weavingInformation = new ProgramDescriptor();
                weavingInformation.SerialNumberBuilder = new SerialNumberBuilderNumeric(SerialNumberBuilderNumeric.NumericType.B2, false);
                weavingInformation.Address = 0x80C;
                weavingInformation.PID_Enable = true;
                weavingInformation.PID_Addr = 0x80A;
                switch (deviceType)
                {
                    case NeuronSpectrumTypes.EMG_Micro4:
                        weavingInformation.PID_Value = 0x8255;
                        break;
                    case NeuronSpectrumTypes.EMG_Micro2:
                        weavingInformation.PID_Value = 0x8256;
                        break;
                }
                weavingInformation.AddData_Addr = 0x812;
                weavingInformation.CypressCpuType = Hardware.Tools.Cypress.CypressTools.CypressCpuType.FX1_FX2LP;
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "iic|" + fileNameTemplate;
                openFileDialog.InitialDirectory = firmwareFolder;
                //openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate);
                if (openFileDialog.ShowDialog() != true)
                {
                    return null;
                }
                weavingInformation.FileName = openFileDialog.FileName;
                bool result = CommonScripts.DoFirmwareEeprom(environment.DeviceSerialNumber, usbItem, weavingInformation);
                if (result)
                {
                    fileName = CommonScripts.GetShortFileName(weavingInformation.FileName);
                    return true;
                }
            }
            return null;
        }


        #endregion

        /// <summary>
        /// Папка с прошивками
        /// </summary>
        internal static string FirmwareFolder
        {
            get
            {
                string fwFolder = @"\\server\firmware\НС4М";
                return Directory.Exists(fwFolder) ? fwFolder : @"C:\";
            }
        }

        #endregion

        #region Открытие и валидация устройства

        /// <summary>
        /// Открывает устройство
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Ссылка на экземпляр устройства </returns>
        public static EEG4Device OpenDevice(ScriptEnvironment environment)
        {
            EEG4Device device = environment.Device as EEG4Device;
            if (device == null)
            {
                device = new EEG4Device(environment.DeviceSerialNumber);
                if (device.Open())
                    environment.InitDevice(new NSDeviceWrapper(device));
                else
                    device = null;
            }
            return device;
        }

        /// <summary>
        /// Валидация открытия девайса
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат Валидации </returns>
        public static ValidationResult ValidateDeviceOpening(ScriptEnvironment environment)
        {
            EEG4Device device = OpenDevice(environment);
            if (device == null)
            {
                return new ValidationResult(false, NeuroSoft.DeviceAutoTest.Common.Properties.Resources.CouldNotOpenDevice);
            }
            return new ValidationResult(true, null);
        }

        /// <summary>
        /// Проверяет ссылку на прибор и если он подключен и все ОК возвращает ее
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Ссылка на экземпляр устройства </returns>
        private static EEG4Device GetDevice(ScriptEnvironment environment)
        {
            EEG4Device device = OpenDevice(environment);
            if (device == null)
                return null;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return null;
            }
            return device;
        }

        #endregion

        #region Проверка информации об устройстве

        /// <summary>
        /// Имя контент презентера контрола с информацией о приборе
        /// </summary>
        private static string deviceInfoControlName;

        /// <summary>
        /// Отображает контрол с информацией о приборе
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя презентера </param>
        public static void ShowDeviceInformation(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
                presenter = new ContentPresenter();
            DeviceInformationControl deviceInfoControl = new DeviceInformationControl(environment, false);
            presenter.Content = deviceInfoControl;
            deviceInfoControlName = contentPresenterName;
        }

        /// <summary>
        /// Проверяет информацию о приборе
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат проверки информации о приборе </returns>
        public static bool? TestDeviceInformation(ScriptEnvironment environment, bool removeControl = true)
        {
            string errorMessage;
            bool? devInfoResult = ((environment[deviceInfoControlName] as ContentPresenter).Content as DeviceInformationControl).CheckDeviceInformationValid(out errorMessage);
            if (devInfoResult != true)
                CommonScripts.ShowError(errorMessage);
            if (removeControl)
                environment.RemoveAndDispose(deviceInfoControlName);
            return devInfoResult;
        }

        #endregion

        #region Проверка напряжений питания и токов потребления устройства

        /// <summary>
        /// Запускает режим калибровки при проверке напряжений и токов потребления
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StartVoltagesChecking(ScriptEnvironment environment)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            device.SetWorkMode(EEGWorkMode.Kalibrovka);
            device.BeginTransmit();
        }

        /// <summary>
        /// Останавливает режим калибровки при проверке напряжений и токов потребления
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopVoltagesChecking(ScriptEnvironment environment)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            device.StopTransmit();
        }

        #endregion

        #region Проверка индикации

        private static string IndicationControlName;

        /// <summary>
        /// Отображает контрол индикации
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя презентера </param>
        public static void ShowIndicationControl(ScriptEnvironment environment, string contentPresenterName)
        {
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            EEG4IndicationControl ledsControl = new EEG4IndicationControl(environment);
            presenter.Content = ledsControl;
            IndicationControlName = contentPresenterName;
        }

        /// <summary>
        /// Проверяет, все ли цвета поменялись при проверке индикации
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат проверки: true - было все проверено, null - что-то не было выполнено </returns>
        public static bool? TestIndication(ScriptEnvironment environment)
        {
            if (IndicationControlName == null)
                return null;
            bool? result = ((environment[IndicationControlName] as ContentPresenter).Content as EEG4IndicationControl).AllChecked;
            if (result != true)
            {
                CommonScripts.ShowError(Properties.Resources.IndicationErrorString);
                result = null;
            }
            return result;
        }

        #endregion

        #region Проверка распайки разъемов передней панели

        /// <summary>
        /// Имя контент презентера контрола проверки распайки фронтальных разъемов
        /// </summary>
        private static string checkSocketsControlName;
        /// <summary>
        /// Показывает контрол проверки распайки разъемов передней панели
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowCheckSolderingControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            EEG4SocketsControl checkSocketsControl = new EEG4SocketsControl(environment, new RangedValue<double>(0, -1.0, 250));
            presenter.Content = checkSocketsControl;
            checkSocketsControl.Start();
            checkSocketsControlName = contentPresenterName;
        }

        /// <summary>
        /// Проверяет результат тестирования распайки разъемов фронтальной панели прибора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат проверки </returns>
        public static bool? CheckSolderingTest(ScriptEnvironment environment)
        {
            ContentPresenter controlPresenter = environment[checkSocketsControlName] as ContentPresenter;
            if (controlPresenter == null)
            {
                CommonScripts.ShowError(Properties.Resource1.UnableGetCheckSocketsResult);
                return null;
            }
            EEG4SocketsControl socketsContr = controlPresenter.Content as EEG4SocketsControl;
            if (socketsContr == null)
            {
                CommonScripts.ShowError(Properties.Resource1.UnableGetCheckSocketsResult);
                return null;
            }
            if (socketsContr.TestingResult == null)
            {
                CommonScripts.ShowError(Properties.Resource1.TestingNotFinished);
                return null;
            }
            else
            {
                if (socketsContr.TestingResult != true)
                {
                    CommonScripts.ShowError(Properties.Resource1.CheckSocketsErrors);
                    return null;
                }
            }
            environment.RemoveAndDispose(checkSocketsControlName);
            return true;
        }

        #region Проверка распайки для ЭМГ-Микро-4

        private static string ImpedancesControlName;

        /// <summary>
        /// Запуск проверки схемы измерения импедансов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartElectrodImpedancesCheck(ScriptEnvironment environment, string contentPresenterName)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return;
            (device as EEG4Device).SetWorkMode(EEGWorkMode.EEGTransmit);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
            {
                return;
            }
            NSDeviceSolderingControl electrodeImpControl = new NSDeviceSolderingControl(environment, device, -1.0, 250.0);
            electrodeImpControl.StartTestSoldering += new StartTestSolderingEventHandler(electrodeImpControl_StartTestImpedances);
            presenter.Content = electrodeImpControl;
            electrodeImpControl.StartTesting();
            ImpedancesControlName = contentPresenterName;
        }

        /// <summary>
        /// Обработчик события переключения текущего измеряемого электрода
        /// </summary>
        /// <param name="sender"> Источник </param>
        /// <param name="electrode"> Номер проверяемого электрода </param>
        private static void electrodeImpControl_StartTestImpedances(object sender, int electrode)
        {
            var electrImpControl = sender as NSDeviceSolderingControl;
            if (electrImpControl == null)
                return;
            var environment = electrImpControl.Environment;
            var testedDevice = electrImpControl.Device;
            StandOperations.ChangeCommutatorStateInImpedanceTest(environment, testedDevice, electrode);
        }

        /// <summary>
        /// Проверка и сохранение результатов измерений импедансов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера контрола импедансов </param>
        /// <param name="electrodeImpedances"> Массив, в который сохраняются значения импедансов </param>
        /// <returns></returns>
        public static bool? CheckAndSaveImpedances(ScriptEnvironment environment, ref double[] electrodeImpedances)
        {
            NSDevicesImpedanceControl impControl = (environment[ImpedancesControlName] as ContentPresenter).Content as NSDevicesImpedanceControl;
            if (impControl != null)
            {
                impControl.Stop();
                if (!impControl.ValidImpedances)
                {
                    return null;
                }
                for (int i = 0; i < impControl.SavedImpedances.Count; i++)
                {
                    if (i != 0)
                    {
                        electrodeImpedances[i - 1] = impControl.SavedImpedances[i].Resistance.Value;
                    }
                }
            }
            else
            {
                NSDeviceSolderingControl soldControl = (environment[ImpedancesControlName] as ContentPresenter).Content as NSDeviceSolderingControl;
                if (soldControl == null)
                    return null;
                if (!soldControl.NSDevicesImpedancesControl.ValidImpedances)
                {
                    return null;
                }
            }
            return true;
        }

        /// <summary>
        /// Заврешение проверки измерения импедансов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера контрола проверки импедансов </param>
        public static void StopElectrodImpedancesCheck(ScriptEnvironment environment)
        {
            environment.RemoveAndDispose(ImpedancesControlName);
        }
        #endregion
        #endregion

        #region Проверка усилителей

        private static string ReadDataControlName;

        /// <summary>
        /// Запускает чтение данных
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="workMode"> Режим передачи данных </param>
        /// <param name="readDataControlSettings"> Установки для ReadDataControl </param>
        private static void StartReadData(ScriptEnvironment environment, string contentPresenterName, EEGTestingMode workMode, ReadDataSettings readDataControlSettings = null, bool useRangeForA1A2 = false)
        {
            EEG4Device device = GetDevice(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
                presenter = new ContentPresenter();
            ReadDataViewModelEEG4 viewModelEEG = new ReadDataViewModelEEG4(environment, device, workMode);
            ReadDataControl readDataControl = new ReadDataControl(viewModelEEG);
            presenter.Content = readDataControl;
            viewModelEEG.UseRangeForA1A2 = useRangeForA1A2;
            // Финт ушами при установке фильтров. Если используются программные фильтры, то их сначала нужно установить, а потом запустить чтение.
            // Если используются аппаратные фильтры, то сначала нужно запустить чтение, а потом их устанавливать.
            if (readDataControlSettings != null && viewModelEEG.TestingMode == EEGTestingMode.VPTransmit &&
               (!viewModelEEG.ChannelsInfo[0].HighFreqPassBands.Contains((float)readDataControlSettings.MaxFreqFilter) ||
                !viewModelEEG.ChannelsInfo[0].LowFreqPassBands.Contains((float)readDataControlSettings.MinFreqFilter)))
            {
                readDataControl.SetSettings(readDataControlSettings);
                viewModelEEG.StartReadData();
            }
            else
            {
                viewModelEEG.StartReadData();
                if (readDataControlSettings != null)
                    readDataControl.SetSettings(readDataControlSettings);
            }
            // Еще один финт ушами для компенсации изменений в ревизии 353 библиотеки EEGDevice
            // (refs #8232 Подбор новых параметров фильтра поднятия нижних частот для сертификации по новым ТУ для поликаналов каналов НС-1-5)
            // Из-за этого амплитуда сигнала калибровки каналов ВП была занижена
            if (environment.Device is EEG4Device && (workMode == EEGTestingMode.Calibration || workMode == EEGTestingMode.VPcalibration))
            {
                for (int i = NSEEG4Consts.NUMBER_CHANEL_ON_Of_EEG4M - NSEEG4Consts.NUMBER_CHANEL_ON_Of_VP4M; i < NSEEG4Consts.NUMBER_CHANEL_ON_Of_EEG4M; i++)
                    foreach (var filter in (environment.Device as EEG4Device).Filters[i])
                        if (filter is IIRFilterCompensating)
                            filter.Active = false;
            }
            ReadDataControlName = contentPresenterName;
        }

        /// <summary>
        /// Возвращает контрол чтения данных с каналов ЭЭГ из контент презентера
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Ссылка на контрол чтения данных с каналов ЭЭГ </returns>
        private static ReadDataControl GetReadDataControl(ScriptEnvironment environment)
        {
            ContentPresenter presenter = environment[ReadDataControlName] as ContentPresenter;
            if (presenter == null)
            {
                CommonScripts.ShowError(Properties.Resource1.UnableGetReadDataControl);
                return null;
            }
            ReadDataControl readDataControl = presenter.Content as ReadDataControl;
            if (readDataControl == null)
            {
                CommonScripts.ShowError(Properties.Resource1.UnableGetReadDataControl);
                return null;
            }
            return readDataControl;
        }

        /// <summary>
        /// Останавливает чтение данных с прибора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopDataReading(ScriptEnvironment environment)
        {
            ((environment[ReadDataControlName] as ContentPresenter).Content as ReadDataControl).ViewModel.StopReadData();
            environment.RemoveAndDispose(ReadDataControlName);
        }

        #region Проверка калибровочного сигнала

        /// <summary>
        /// Представляет контрол, отображающий сигнал калибровки
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowCalibrationSignal(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings calibrationSettings = new ReadDataSettings();
            calibrationSettings.SwingRange.Min = 2.15e-3;
            calibrationSettings.SwingRange.Max = 2.45e-3;
            calibrationSettings.MaxFreqFilter = 36.0;
            calibrationSettings.MinFreqFilter = 0.5;
            calibrationSettings.TickIntervalsProcessing = 2;
            calibrationSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
            calibrationSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");
            StartReadData(environment, contentPresenterName, EEGTestingMode.Calibration, calibrationSettings);
        }

        /// <summary>
        /// Представляет контрол, отображающий сигнал калибровки для Нейро-ЭМГ-Микро 4
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowVpCalibrationSignal(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings calibrationSettings = new ReadDataSettings();
            calibrationSettings.SwingRange.Min = 2.15e-3;
            calibrationSettings.SwingRange.Max = 2.45e-3;
            calibrationSettings.MaxFreqFilter = 36.0;
            calibrationSettings.MinFreqFilter = 0.5;
            calibrationSettings.TickIntervalsProcessing = 2;
            calibrationSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
            calibrationSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");
            StartReadData(environment, contentPresenterName, EEGTestingMode.VPcalibration, calibrationSettings);
        }

        /// <summary>
        /// Проверяет значения размаха сигнала калибровки
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Успешность проведения теста </returns>
        public static bool? TestCalibrationSwings(ScriptEnvironment environment, ref double worstValue)
        {
            double bestValue;
            ReadDataControl readDataControl = GetReadDataControl(environment);
            if (readDataControl == null)
                return null;
            StopDataReading(environment);
            worstValue = bestValue = (readDataControl.ViewModel.StatisticsCollection[0].Swing.MaxValue + readDataControl.ViewModel.StatisticsCollection[0].Swing.MinValue) / 2;
            foreach (DataStatistics dataStat in readDataControl.ViewModel.StatisticsCollection)
            {
                if (!dataStat.Swing.IsValidValue)
                {
                    CommonScripts.ShowError(String.Format(Properties.Resource1.CalibrSignalSwingsInvalid, dataStat.ChannelName));
                    worstValue = 0;
                    return null;
                }
                if (Math.Abs(dataStat.Swing.Value - bestValue) > Math.Abs(worstValue - bestValue) && dataStat.ChannelName != "A1")
                    worstValue = dataStat.Swing.Value;
            }
            worstValue *= 1e6;
            return true;
        }

        #endregion

        #region Проверка каналов ЭЭГ

        #region Проверка измерения импеданса

        /// <summary>
        /// Имя контент презентера контрола импедансов
        /// </summary>
        private static string ImpedanceControlName;
        /// <summary>
        /// Отображает контрол измерения импеданса для ЭЭГ или ВП режима 
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isEEGImpedance"> Режим отображения импеданса (ЭЭГ или ВП) </param>
        public static void ShowImpedancesControl(ScriptEnvironment environment, string contentPresenterName, bool isEEGImpedance)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            if (isEEGImpedance)
                StandOperationsEEG.EEG4ConnectChannelsTo10K(environment);
            else
                if (GetDeviceType(device) == NeuronSpectrumTypes.NS_4EP || GetDeviceType(device) == NeuronSpectrumTypes.NS_4P || GetDeviceType(device) == NeuronSpectrumTypes.EMG_Micro4)
                    StandOperationsEEG.ConnectVpChannelsTo10K(environment);
                else
                    StandOperationsEEG.EEG4ConnectVpChannelsTo10K(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            NSDevicesImpedanceControl impedancesControl = new NSDevicesImpedanceControl(environment, new RangedValue<double>(10e3, 9e3, 11e3), isEEGImpedance);
            presenter.Content = impedancesControl;
            impedancesControl.Start();
            ImpedanceControlName = contentPresenterName;
        }
        /// <summary>
        /// Проверяет на валидность сохраняемые значния импеданса
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат проверки </returns>
        public static bool? TestImpedancesValues(ScriptEnvironment environment)
        {
            if (ImpedanceControlName == null)
                return null;
            NSDevicesImpedanceControl impedancesControl = (environment[ImpedanceControlName] as ContentPresenter).Content as NSDevicesImpedanceControl;
            if (impedancesControl != null)
            {
                //bool result = impedancesControl.ValidImpedances;
                impedancesControl.Stop();
                //(environment.Device as EEG4Device).StopTransmit();
                //(environment.Device as EEG4Device).SetWorkMode(EEGWorkMode.EEGTransmit);
                bool result = impedancesControl.ValidImpedances;
                if (result)
                    return true;
                return null;
            }
            return null;
        }

        #endregion

        #region Проверка уровня шума

        /// <summary>
        /// Запуск проверки уровня шума в каналах ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isWideBandpass"> Проверяемый диапазон частот </param>
        public static void StartEegNoiseTest(ScriptEnvironment environment, string contentPresenterName, bool isWideBandpass = false)
        {
            ReadDataSettings eegNoiseSettings = new ReadDataSettings();
            if (isWideBandpass)
            {
                eegNoiseSettings.MinFreqFilter = 0.5;
                eegNoiseSettings.SwingRange.Min = 0.0;
                eegNoiseSettings.SwingRange.Max = 3e-6;
                eegNoiseSettings.XScale = new ScaleItem(20e-3f, "20 мс/мм");
                eegNoiseSettings.YScale = new ScaleItem(1e-6f, "1 мкВ/мм");
            }
            else
            {
                eegNoiseSettings.MinFreqFilter = 0.5;
                eegNoiseSettings.MaxFreqFilter = 35.0;
                eegNoiseSettings.SwingRange.Min = 0.0;
                eegNoiseSettings.SwingRange.Max = 1e-6;
                eegNoiseSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
                eegNoiseSettings.YScale = new ScaleItem(200e-9f, "200 нВ/мм");
            }
            StandOperationsEEG.EEG4GndEegChannels(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransmit/*EEGNoiseTransmit*/, eegNoiseSettings);
        }

        /// <summary>
        /// Проверяет и сохраняет значения размаха сигнала шума 
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="savedSwings"> Массив сохраняемых значений </param>
        /// <returns> Успешность проверки </returns>
        public static bool? TestAndSaveEegSwings(ScriptEnvironment environment, ref double[] savedSwings, ref double worstSwing, bool ismVscale = true, bool isNoiseTest = false, bool? isMinMaxBest = null)
        {
            ReadDataControl readDataContr = GetReadDataControl(environment);
            if (readDataContr == null)
                return null;
            StopDataReading(environment);
            double bestValue = (readDataContr.ViewModel.StatisticsCollection[0].Swing.MaxValue + readDataContr.ViewModel.StatisticsCollection[0].Swing.MinValue) / 2;
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            worstSwing = bestValue;
            List<double> eegSwings = new List<double>();
            for (int i = 0; i < readDataContr.ViewModel.StatisticsCollection.Count; i++)
            {
                if (!readDataContr.ViewModel.StatisticsCollection[i].Swing.IsValidValue)
                {
                    CommonScripts.ShowError(String.Format(Properties.Resource1.NoiseSwingsInvalidMessage, isNoiseTest ? Properties.Resource1.OfNoise : "", readDataContr.ViewModel.StatisticsCollection[i].ChannelName));
                    return null;
                }
                eegSwings.Add(readDataContr.ViewModel.StatisticsCollection[i].Swing.Value * (ismVscale ? 1e3 : 1e6));
                if (readDataContr.ViewModel.StatisticsCollection[i].ChannelName != "A1")
                {
                    if (isMinMaxBest == null)
                    {
                        if (Math.Abs(readDataContr.ViewModel.StatisticsCollection[i].Swing.Value - bestValue) > Math.Abs(worstSwing - bestValue))
                            worstSwing = readDataContr.ViewModel.StatisticsCollection[i].Swing.Value;
                    }
                    else
                    {
                        if (isMinMaxBest.Value)
                        {
                            if (readDataContr.ViewModel.StatisticsCollection[i].Swing.Value < minValue)
                                worstSwing = minValue = readDataContr.ViewModel.StatisticsCollection[i].Swing.Value;
                        }
                        else
                        {
                            if (readDataContr.ViewModel.StatisticsCollection[i].Swing.Value > maxValue)
                                worstSwing = maxValue = readDataContr.ViewModel.StatisticsCollection[i].Swing.Value;
                        }
                    }
                }
            }
            savedSwings = eegSwings.ToArray();
            worstSwing *= (ismVscale ? 1e3 : 1e6);
            return true;
        }

        #endregion

        #region Проверка коэффициента усиления каналов ЭЭГ

        /// <summary>
        /// Запускает проверку коэффициентов усиления каналов ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isA1A2tested"> Флаг проверки канала A1-A2 </param>
        public static void StartEegGainsTest(ScriptEnvironment environment, string contentPresenterName, bool isA1A2tested = false)
        {
            ReadDataSettings eegGainsSettings = new ReadDataSettings();
            eegGainsSettings.MinFreqFilter = 0.5;
            //eegGainsSettings.MaxFreqFilter = 35.0;
            eegGainsSettings.SwingRange.Max = 1.04e-3;
            eegGainsSettings.SwingRange.Min = 0.96e-3;
            eegGainsSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            eegGainsSettings.YScale = isA1A2tested ? new ScaleItem(50e-6f, "50 мкВ/мм") : new ScaleItem(100e-6f, "100 мкВ/мм");
            StandOperations.SetGeneratorState(environment, 0.25e-3f, 10.0f);
            if (!isA1A2tested)
                StandOperationsEEG.EEG4SetEegDiffSignal(environment);
            else
                StandOperationsEEG.EEG4SetEegA1A2DiffSignal(environment);
            StartReadData(environment, contentPresenterName, isA1A2tested ? EEGTestingMode.EEGTransRefA1 : EEGTestingMode.EEGTransmit, eegGainsSettings);
        }

        /// <summary>
        /// Сохраняет значение размаха сигнала из одного канала
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="chanName"> Имя канала, размах сигнала в котором требуется сохранить </param>
        /// <param name="savedSwing"> Переменная, в которую сохраняется значение </param>
        /// <param name="ismVscale"> Флаг, указывающий, в каких единицах будет сохраняться значение размаха (мВ или мкВ) </param>
        /// <returns></returns>
        public static bool? TestAndSaveEegSwing(ScriptEnvironment environment, string chanName, ref double savedSwing, bool ismVscale = true)
        {
            ReadDataControl readDataContr = GetReadDataControl(environment);
            if (readDataContr == null)
                return null;
            StopDataReading(environment);
            foreach (DataStatistics dataStat in readDataContr.ViewModel.StatisticsCollection)
            {
                if (!dataStat.Swing.IsValidValue)
                {
                    CommonScripts.ShowError(String.Format(Properties.Resource1.NoiseSwingsInvalidMessage, "", dataStat.ChannelName));
                    return null;
                }
                if (dataStat.ChannelName == chanName)
                    savedSwing = dataStat.Swing.Value * (ismVscale ? 1e3 : 1e6);
            }
            return true;
        }

        #endregion

        #region Проверка фильтров каналов ЭЭГ

        /// <summary>
        /// Запускает проверку аппаратных фильтров ФНЧ 150 Гц всех каналов или только A1-A2
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isA1A2tested"> Флаг, указывающий, что производится тестирование канала A1-A2 </param>
        public static void StartEEGLowFreqTest(ScriptEnvironment environment, string contentPresenterName, bool isA1A2tested = false)
        {
            ReadDataSettings eegLowFreqSettings = new ReadDataSettings();
            eegLowFreqSettings.SwingRange.Min = 6.3e-3;
            eegLowFreqSettings.SwingRange.Max = 7.35e-3;
            eegLowFreqSettings.MinFreqFilter = 0.5;
            //eegLowFreqSettings.MaxFreqFilter = 1e3;
            eegLowFreqSettings.XScale = new ScaleItem(1e-3f, "1 мс/мм");
            eegLowFreqSettings.YScale = isA1A2tested ? new ScaleItem(500e-6f, "500 мкВ/мм") : new ScaleItem(1e-3f, "1 мВ/мм");
            StandOperations.SetGeneratorState(environment, 2.5e-3f, 150.0f);
            if (isA1A2tested)
                StandOperationsEEG.EEG4SetEegA1A2DiffSignal(environment);
            else
                StandOperationsEEG.EEG4SetEegDiffSignal(environment);
            //StandOperationsEEG.EEG4SetEegMonoSignal(environment);
            StartReadData(environment, contentPresenterName, isA1A2tested ? EEGTestingMode.EEGTransRefA1 : EEGTestingMode.EEGTransmit, eegLowFreqSettings);
        }

        /// <summary>
        /// Запускает проверку аппаратных фильтров ФВЧ 0.5 Гц всех каналов или только A1-A2
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isA1A2tested"> Флаг, указывающий, что производится тестирование канала A1-A2 </param>
        public static void StartEEGHighFreqTest(ScriptEnvironment environment, string contentPresenterName, bool isA1A2tested = false)
        {
            ReadDataSettings eegHighFreqSettings = new ReadDataSettings();
            eegHighFreqSettings.SwingRange.Min = 6.3e-3;
            eegHighFreqSettings.SwingRange.Max = 7.7e-3;
            //eegLowFreqSettings.MinFreqFilter = 0.05;
            eegHighFreqSettings.MaxFreqFilter = 35;
            eegHighFreqSettings.TickIntervalsProcessing = 4;
            eegHighFreqSettings.XScale = new ScaleItem(100e-3f, "0,1 с/мм");
            eegHighFreqSettings.YScale = isA1A2tested ? new ScaleItem(500e-6f, "500 мкВ/мм") : new ScaleItem(1e-3f, "1 мВ/мм");
            StandOperations.SetGeneratorState(environment, 2.5e-3f, 0.5f);
            if (isA1A2tested)
                StandOperationsEEG.EEG4SetEegA1A2DiffSignal(environment);
            else
                StandOperationsEEG.EEG4SetEegDiffSignal(environment);
            StartReadData(environment, contentPresenterName, isA1A2tested ? EEGTestingMode.EEGTransRefA1 : EEGTestingMode.EEGTransmit, eegHighFreqSettings);
        }

        #endregion

        #region Проверка подавления синфазной помехи

        /// <summary>
        /// Запускает проверку подавления синфазной помехи каналов ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartSynphaseNoiseTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings synphNoiseSettings = new ReadDataSettings();
            synphNoiseSettings.SwingRange.Min = 0.0;
            synphNoiseSettings.SwingRange.Max = 10e-6;
            synphNoiseSettings.MinFreqFilter = 0.5;
            //synphNoiseSettings.MaxFreqFilter = 35.0;
            synphNoiseSettings.XScale = new ScaleItem(5e-3f, "5 мс/мм");
            synphNoiseSettings.YScale = new ScaleItem(2e-6f, "2 мкВ/мм");
            StandOperations.SetGeneratorState(environment, 0.5f, 50.0f);
            StandOperationsEEG.EEG4SetSynphaseEegSignal(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransmit, synphNoiseSettings);
        }

        #endregion

        #region Проверка входного сопротивления каналов ЭЭГ

        /// <summary>
        /// Запускает проверку входного импеданса каналов ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isRefTested"> Флаг, указывающий, какие электроды проверяются в данный момент </param>
        public static void StartEegImputImpedances(ScriptEnvironment environment, string contentPresenterName, bool isRefTested)
        {
            ReadDataSettings eegImpImpedanceSettings = new ReadDataSettings();
            eegImpImpedanceSettings.SwingRange.Min = 0.0;
            eegImpImpedanceSettings.SwingRange.Max = 3.0e-3;
            eegImpImpedanceSettings.MinFreqFilter = 0.5;
            eegImpImpedanceSettings.MaxFreqFilter = 200.0;
            eegImpImpedanceSettings.XScale = new ScaleItem(5e-3f, "5 мс/мм");
            eegImpImpedanceSettings.YScale = new ScaleItem(500e-6f, "500 мкВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, 100.0f);
            StandOperationsEEG.EEG4SetEegSynphForImpedance(environment, isRefTested);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransmit, eegImpImpedanceSettings, true);
        }

        #endregion

        #endregion

        #region Проверка каналов постоянного тока

        /// <summary>
        /// Запускает проверку каналов DC
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="testSignalFreq"> Частота тестового сигнала </param>
        public static void StartTestDcChannels(ScriptEnvironment environment, string contentPresenterName, float testFreq = 2.0f)
        {
            ReadDataSettings dcChannelsSettings = new ReadDataSettings();
            dcChannelsSettings.SwingRange.Min = 0.95;
            dcChannelsSettings.SwingRange.Max = 1.05;
            //if (testSignalFreq == 1.0)
                dcChannelsSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
            //else
                //dcChannelsSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            dcChannelsSettings.YScale = new ScaleItem(100e-3f, "100 мВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, testFreq);
            //StandOperations.SetGeneratorState(environment, 0.0001f, 1.0f);
            StandOperationsEEG.EEG4SetDcChannelsState(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.DCTransmit, dcChannelsSettings);
        }

        #endregion

        #region Проверка канала дыхания

        /// <summary>
        /// Запуск проверки канала дыхания
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartBreathTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings breathSettings = new ReadDataSettings();
            // Подается сигнал амплитудой 5 В. Делитель в сто раз в стенде, потом резистор 6,8 кОм в стенде и резистор 20 кОм в устройстве также образуют делитель
            breathSettings.SwingRange.Min = 5.0 * 2.0 * (100.0 / 10100.0) * (20.0 / 26.8) * 0.9;
            breathSettings.SwingRange.Max = 5.0 * 2.0 * (100.0 / 10100.0) * (20.0 / 26.8) * 1.1;
            breathSettings.MinFreqFilter = 0.1;
            breathSettings.MaxFreqFilter = 40.0;    // Changed from 10.0 Hz otherwise result was not correct;
            breathSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
            breathSettings.YScale = new ScaleItem(2e-3f, "2 мВ/мм");
            StandOperations.SetGeneratorState(environment, 5.0f, 2.0f);
            //StandOperations.SetGeneratorState(environment, 0.001f, 1.0f, WaveForm.Sinus, 1);
            StandOperationsEEG.SetBreathChannel(environment, true);
            StartReadData(environment, contentPresenterName, EEGTestingMode.BreathTransmit, breathSettings);
        }

        #endregion

        #region Проверка каналов ВП

        #region Проверка уровня шума

        /// <summary>
        /// Запускает проверку шума каналов ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="is100mvRange"> Флаг, указывающий, в каком режиме осуществляется проверка </param>
        /// <param param name="maxSwing"> Максимально допустимое значение размаха сигнала </param>
        public static void StartVpNoiseTest(ScriptEnvironment environment, string contentPresenterName, ModesOfTestNoise testingMode, double maxSwing = double.Epsilon)
        {
            EEG4Device device = GetDevice(environment);
            ReadDataSettings vpNoiseSettings = new ReadDataSettings();
            switch (testingMode)
            {
                case ModesOfTestNoise.Range100mV:
                    //vpNoiseSettings.SwingRange.Max = 20e-6;
                    vpNoiseSettings.MinFreqFilter = 0.5;
                    vpNoiseSettings.MaxFreqFilter = 20000.0;
                    vpNoiseSettings.RangeIndex = 8;
                    vpNoiseSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
                    vpNoiseSettings.YScale = new ScaleItem(2e-6f, "2 мкВ/мм");
                    break;
                case ModesOfTestNoise.Range200mkV:
                    //vpNoiseSettings.SwingRange.Max = 5e-6;
                    vpNoiseSettings.MinFreqFilter = 0.5;
                    vpNoiseSettings.MaxFreqFilter = 20000.0;
                    vpNoiseSettings.RangeIndex = 0;
                    vpNoiseSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
                    vpNoiseSettings.YScale = new ScaleItem(1e-6f, "1 мкВ/мм");
                    break;
                case ModesOfTestNoise.IsoLineAt005Hz:
                    //vpNoiseSettings.SwingRange.Max = 2e-6;
                    vpNoiseSettings.MinFreqFilter = 0.5;
                    vpNoiseSettings.MaxFreqFilter = 250.0;
                    vpNoiseSettings.RangeIndex = 0;
                    vpNoiseSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
                    vpNoiseSettings.YScale = new ScaleItem(200e-9f, "200 нВ/мм");
                    break;
            }
            vpNoiseSettings.SwingRange.Max = maxSwing;
            vpNoiseSettings.SwingRange.Min = 0.0;
            if (GetDeviceType(device) == NeuronSpectrumTypes.NS_4EP || GetDeviceType(device) == NeuronSpectrumTypes.NS_4P || GetDeviceType(device) == NeuronSpectrumTypes.EMG_Micro4)
                StandOperationsEEG.ConnectVpChannelsToGnd(environment);
            else
                StandOperationsEEG.EEG4ConnectVpChannelsToGnd(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.VPTransmit, vpNoiseSettings);
        }

        /// <summary>
        /// Проверяет и сохраняет значения размаха сигнала и шума 
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="savedSwings"> Массив сохраняемых значений </param>
        /// <returns> Успешность проверки </returns>
        public static bool? TestAndSaveVPSwings(ScriptEnvironment environment, ref double[] savedSwings, ref double worstSwing, bool ismVScale = true, bool isSaveNoise = false, bool? isMinMaxWorst = null)
        {
            ReadDataControl readDataContr = GetReadDataControl(environment);
            if (readDataContr == null)
                return null;
            StopDataReading(environment);
            double bestValue = (readDataContr.ViewModel.StatisticsCollection[0].Swing.MaxValue + readDataContr.ViewModel.StatisticsCollection[0].Swing.MinValue) / 2;
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            worstSwing = bestValue;
            List<double> noiseSwings = new List<double>();
            for (int i = 0; i < readDataContr.ViewModel.StatisticsCollection.Count; i++)
            {
                if (!readDataContr.ViewModel.StatisticsCollection[i].Swing.IsValidValue)
                {
                    CommonScripts.ShowError(String.Format(Properties.Resource1.NoiseSwingsInvalidMessage, isSaveNoise ? Properties.Resource1.OfNoise : "", readDataContr.ViewModel.StatisticsCollection[i].ChannelName));
                    return null;
                }
                noiseSwings.Add(readDataContr.ViewModel.StatisticsCollection[i].Swing.Value * (ismVScale ? 1e3 : 1e6));
                if (isMinMaxWorst == null)
                {
                    if (Math.Abs(readDataContr.ViewModel.StatisticsCollection[i].Swing.Value - bestValue) > Math.Abs(worstSwing - bestValue))
                        worstSwing = readDataContr.ViewModel.StatisticsCollection[i].Swing.Value;
                }
                else
                {
                    if (isMinMaxWorst.Value)
                    {
                        if (readDataContr.ViewModel.StatisticsCollection[i].Swing.Value < minValue)
                            worstSwing = minValue = readDataContr.ViewModel.StatisticsCollection[i].Swing.Value;
                    }
                    else
                    {
                        if (readDataContr.ViewModel.StatisticsCollection[i].Swing.Value > maxValue)
                            worstSwing = maxValue = readDataContr.ViewModel.StatisticsCollection[i].Swing.Value;
                    }
                }
            }
            savedSwings = noiseSwings.ToArray();
            worstSwing *= (ismVScale ? 1e3 : 1e6);
            return true;
        }

        #endregion

        #region Проверка подавления синфазной помехи

        /// <summary>
        /// Запускает проверку подавления синфазной помехи каналов ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartVpSynphNoiseTest(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG4Device device = GetDevice(environment);
            ReadDataSettings vpSynphNoiseSetts = new ReadDataSettings();
            vpSynphNoiseSetts.SwingRange.Max = 10e-6;
            vpSynphNoiseSetts.SwingRange.Min = 0.0;
            vpSynphNoiseSetts.MinFreqFilter = 1;
            vpSynphNoiseSetts.MaxFreqFilter = 250.0;
            vpSynphNoiseSetts.RangeIndex = 0;
            vpSynphNoiseSetts.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            vpSynphNoiseSetts.YScale = new ScaleItem(500e-9f, "500 нВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, 50.0f);
            if (GetDeviceType(device) == NeuronSpectrumTypes.NS_4EP || GetDeviceType(device) == NeuronSpectrumTypes.NS_4P || GetDeviceType(device) == NeuronSpectrumTypes.EMG_Micro4)
                StandOperationsEEG.SetSynphaseVpSignal(environment);
            else
                StandOperationsEEG.EEG4SetSynphaseVpSignal(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.VPTransmit, vpSynphNoiseSetts);
        }

        #endregion

        #region Проверка аппаратных фильтров ВП

        /// <summary>
        /// Запускает аппаратную проверку фильтров каналов ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="testedFilter"> Частота среза проверяемого фильтра </param>
        public static void StartVpFiltersTest(ScriptEnvironment environment, string contentPresenterName, VpFiltersForEEG5 testedFilter)
        {
            EEG4Device device = GetDevice(environment);
            ReadDataSettings vpFiltsSettings = new ReadDataSettings();
            float currentFreq = 0.0f;
            switch (testedFilter)
            {
                case VpFiltersForEEG5.HighFreq_0212_Hz:
                    currentFreq = device.DeviceInformation.DeviceVersion >= 14 ? 0.2f : 0.05f;
                    vpFiltsSettings.SwingRange.Max = 7.7e-3;
                    vpFiltsSettings.SwingRange.Min = 6.3e-3;
                    vpFiltsSettings.MinFreqFilter = device.DeviceInformation.DeviceVersion >= 14 ? 0.212 : 0.05;
                    vpFiltsSettings.MaxFreqFilter = 10000.0;
                    vpFiltsSettings.TickIntervalsProcessing = device.DeviceInformation.DeviceVersion >= 14 ? 10 : 40;
                    vpFiltsSettings.XScale = new ScaleItem(200e-3f, "0,2 с/мм");
                    vpFiltsSettings.YScale = new ScaleItem(500e-6f, "500 мкВ/мм");
                    break;
                case VpFiltersForEEG5.HighFreq_05_Hz:
                    currentFreq = device.DeviceInformation.DeviceVersion >= 14 ? 0.7f : 0.5f;
                    vpFiltsSettings.SwingRange.Max = 7.7e-3;
                    vpFiltsSettings.SwingRange.Min = 6.3e-3;
                    vpFiltsSettings.MinFreqFilter = device.DeviceInformation.DeviceVersion >= 14 ? 0.7f : 0.5f;
                    vpFiltsSettings.MaxFreqFilter = 10000.0;
                    vpFiltsSettings.TickIntervalsProcessing = 4;
                    vpFiltsSettings.XScale = new ScaleItem(100e-3f, "0,1 с/мм");
                    vpFiltsSettings.YScale = new ScaleItem(500e-6f, "500 мкВ/мм");
                    break;
                case VpFiltersForEEG5.HighFreq_160_Hz:
                    currentFreq = 160.0f;
                    vpFiltsSettings.SwingRange.Max = 7.7e-3;
                    vpFiltsSettings.SwingRange.Min = 6.3e-3;
                    vpFiltsSettings.MinFreqFilter = 160.0;
                    vpFiltsSettings.MaxFreqFilter = 10000.0;
                    vpFiltsSettings.XScale = new ScaleItem(1e-3f, "1 мс/мм");
                    vpFiltsSettings.YScale = new ScaleItem(500e-6f, "500 мкВ/мм");
                    break;
                case VpFiltersForEEG5.LowFreq_250_Hz:
                    currentFreq = 250.0f;
                    vpFiltsSettings.SwingRange.Max = 7.35e-3;
                    vpFiltsSettings.SwingRange.Min = 6.65e-3;
                    vpFiltsSettings.MinFreqFilter = device.DeviceInformation.DeviceVersion >= 14 ? 0.7f : 0.5f;
                    vpFiltsSettings.MaxFreqFilter = 250.0;
                    vpFiltsSettings.XScale = new ScaleItem(500e-6f, "0,5 мс/мм");
                    vpFiltsSettings.YScale = new ScaleItem(500e-6f, "500 мкВ/мм");
                    break;
                case VpFiltersForEEG5.LowFreq_10000_Hz:
                    currentFreq = 10000.0f;
                    vpFiltsSettings.SwingRange.Max = 80e-3;
                    vpFiltsSettings.SwingRange.Min = 60e-3;
                    vpFiltsSettings.MinFreqFilter = device.DeviceInformation.DeviceVersion >= 14 ? 0.7f : 0.5f;
                    vpFiltsSettings.MaxFreqFilter = 10000.0;
                    vpFiltsSettings.XScale = new ScaleItem(20e-6f, "20 мкс/мм");
                    vpFiltsSettings.YScale = new ScaleItem(5e-3f, "5 мВ/мм");
                    break;
            }
            vpFiltsSettings.RangeIndex = 8;
            StandOperations.SetGeneratorState(environment, (testedFilter == VpFiltersForEEG5.LowFreq_10000_Hz ? 0.025f : 0.0025f), currentFreq);
            if (GetDeviceType(device) == NeuronSpectrumTypes.NS_4EP || GetDeviceType(device) == NeuronSpectrumTypes.NS_4P || GetDeviceType(device) == NeuronSpectrumTypes.EMG_Micro4)
                StandOperationsEEG.SetDifferetialVpSignal(environment);
            else
                StandOperationsEEG.EEG4SetDifferetialVpSignal(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.VPTransmit, vpFiltsSettings);
        }

        #endregion

        #region Проверка коэффициентов усиления каналов ВП

        /// <summary>
        /// Запуск проверки диапазонов каналов ВП в автоматическом режиме
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartTestGainFactorsVP(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG4Device device = GetDevice(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            TestInputRangesViewModelEEG4 testRangesViewModel = new TestInputRangesViewModelEEG4(environment, device);
            TestInputRangesControl testRangesDataControl = new TestInputRangesControl(testRangesViewModel);
            presenter.Content = testRangesDataControl;
            ReadDataControlName = contentPresenterName;
            testRangesDataControl.StartTest();
        }

        /// <summary>
        /// Метод, проверящий значения размаха сигнала и сохраняющий значения
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="firstChannelSwings"> Массив для хранения размахов сигнала </param>
        /// <returns> Результаты проверки </returns>
        public static bool? CheckAndSaveInputRangeResults(ScriptEnvironment environment, double[][] ChannelsSwings)
        {
            TestInputRangesControl inputRangesControl = ((environment[ReadDataControlName] as ContentPresenter).Content as TestInputRangesControl);
            if (inputRangesControl == null)
            {
                CommonScripts.ShowError(Properties.Resource1.UnableGetSwingsValues);
                return null;
            }
            List<double>[] channelsSwingsLists = new List<double>[ChannelsSwings.Length] /*{new List<double>(), new List<double>(), new List<double>()}*/;
            StopInputRangesDataReading(environment);
            for (int i = 0; i < ChannelsSwings.Length; i++)
                channelsSwingsLists[i] = new List<double>();
            if (inputRangesControl.ViewModel.IsTesting || inputRangesControl.ViewModel.TestWasLaunched)
            {
                CommonScripts.ShowError(Properties.Resource1.TestingNotFinished);
                return null;
            }
            for (int i = 0; i < inputRangesControl.ViewModel.InputRangeTests.Count; i++)
            {
                for (int j = 0; j < inputRangesControl.ViewModel.InputRangeTests[i].ChannelsSwings.Length; j++)
                {
                    if (!inputRangesControl.ViewModel.InputRangeTests[i].IsValidValues)
                    {
                        CommonScripts.ShowError(String.Format(Properties.Resource1.InputRagesTestInvalidMessage, j + 1, inputRangesControl.ViewModel.InputRangeTests[i].Range, inputRangesControl.ViewModel.InputRangeTests[i].SwingString));
                        return null;
                    }
                    channelsSwingsLists[j].Add(Math.Round(inputRangesControl.ViewModel.InputRangeTests[i].ChannelsSwings[j].Value * 1e3, 3));
                    //secondChannelSwings[i] = Math.Round(inputRangesControl.ViewModel.InputRangeTests[i].ChannelsSwings[1].Value * 1e3, 3);
                }
            }
            for (int i = 0; i < channelsSwingsLists.Length; i++)
                ChannelsSwings[i] = channelsSwingsLists[i].ToArray();
            return true;
        }

        /// <summary>
        /// Останавливает чтение данных с прибора при проверке входных диапазонов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopInputRangesDataReading(ScriptEnvironment environment)
        {
            ((environment[ReadDataControlName] as ContentPresenter).Content as TestInputRangesControl).ViewModel.StopReadData();
            environment.RemoveAndDispose(ReadDataControlName);
        }

        #endregion

        #region Проверка входного сопротивления каналов ВП

        /// <summary>
        /// Запускает проверку входного импеданса каналов ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isPositiveTested"> Флаг, указывающий какой электрод проверяется в данный момент </param>
        public static void StartTestVpInputImpedances(ScriptEnvironment environment, string contentPresenterName, bool isPositiveTested)
        {
            EEG4Device device = GetDevice(environment);
            ReadDataSettings vpImpedancesSettings = new ReadDataSettings();
            vpImpedancesSettings.SwingRange.Min = 0.0;
            vpImpedancesSettings.SwingRange.Max = 2.0e-3;
            vpImpedancesSettings.MinFreqFilter = 1.0;
            vpImpedancesSettings.MaxFreqFilter = 200.0;
            vpImpedancesSettings.RangeIndex = 5;
            vpImpedancesSettings.XScale = new ScaleItem(5e-3f, "5 мс/мм");
            vpImpedancesSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, 100.0f);
            if (GetDeviceType(device) == NeuronSpectrumTypes.NS_4EP || GetDeviceType(device) == NeuronSpectrumTypes.NS_4P || GetDeviceType(device) == NeuronSpectrumTypes.EMG_Micro4)
                StandOperationsEEG.SetVpForInputImpedance(environment, isPositiveTested);
            else
                StandOperationsEEG.EEG4SetVpForInputImpedance(environment, isPositiveTested);
            StartReadData(environment, contentPresenterName, EEGTestingMode.VPTransmit, vpImpedancesSettings);
        }

        #endregion

        #endregion

        #endregion

        #region Проверка стимуляторов

        #region Проверка фотостимулятора

        private static string PhotoStimControlName;

        /// <summary>
        /// Запускает фотостимулятор с заданными параметрами
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="duration"> Длительность стимула в секундах </param>
        /// <param name="period"> Период стимуляции в секундах </param>
        public static void StartPhotoStimulation(ScriptEnvironment environment, double duration, double period)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            (device as NeuroSoft.Devices.IFlashStimulator).SetStimulusDuration((float)duration);
            (device as NeuroSoft.Devices.IFlashStimulator).SetFrequency((float)(1 / period));
            (device as NeuroSoft.Devices.IFlashStimulator).SetEnabled(true);
            (device as NeuroSoft.Devices.IFlashStimulator).StartStimulation();
        }

        /// <summary>
        /// Отображает контрол с установками фотостимулятора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowPhotoStimControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            PhotoStimulatorControl photoStimulatorControl = new PhotoStimulatorControl(device as NeuroSoft.Devices.IFlashStimulator);
            presenter.Content = photoStimulatorControl;
            PhotoStimControlName = contentPresenterName;
        }

        /// <summary>
        /// Останавливает фотостимуляцию
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopPhotoStimulation(ScriptEnvironment environment)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            if (PhotoStimControlName != null && environment[PhotoStimControlName] != null)
            {
                PhotoStimulatorControl photoStimulatorControl = (environment[PhotoStimControlName] as ContentPresenter).Content as PhotoStimulatorControl;
                if (photoStimulatorControl != null)
                {
                    photoStimulatorControl.StopStimulation();
                    environment.RemoveAndDispose(PhotoStimControlName);
                    PhotoStimControlName = null;
                    return;
                }
            }
            (device as NeuroSoft.Devices.IFlashStimulator).StopStimulation();
            (device as NeuroSoft.Devices.IFlashStimulator).SetEnabled(false);
        }

        #endregion

        #region Проверка шахматного паттерна

        /// <summary>
        /// Имя контент презентера контрола паттерн-стимулятора
        /// </summary>
        private static string patternStimPresenterName;

        /// <summary>
        /// Валидация шахматного паттерна
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат выполнения валидации </returns>
        public static ValidationResult ValidatePatternConnecting(ScriptEnvironment environment)
        {
            EEG4Device device = OpenDevice(environment);
            if (device.IsPatternPresent == false)
            {
                return new ValidationResult(false, Properties.Resource1.PatternNotConnected);
            }
            return new ValidationResult(true, null);
        }

        /// <summary>
        /// Запускает стимуляцию шахматным паттерном
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="patternSize"> Размер паттерна </param>
        public static void StartPatternStimulation(ScriptEnvironment environment, /*int patternSizeIndx/*,*/ PatternStimulationPole patternSize)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            (device as IEEGPatternStimulator).SetPole(patternSize);
            (device as IEEGPatternStimulator).SetEnabled(true);
            (device as IEEGPatternStimulator).StartStimulation();
        }

        /// <summary>
        /// Запуск стимуляции шахматным паттерном
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера контрола паттерн-стимулятора </param>
        public static void ShowPatternStimulatorControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            PatternStimualtorControl patternStimControl = new PatternStimualtorControl(device as IEEGPatternStimulator);
            presenter.Content = patternStimControl;
            patternStimPresenterName = contentPresenterName;
            patternStimControl.StarStimulation();
        }

        /// <summary>
        /// Остановка стимуляции шахматным паттерном
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopPatternStimulation(ScriptEnvironment environment)
        {

            EEG4Device device;
            if (patternStimPresenterName != null && environment[patternStimPresenterName] != null)
            {
                PatternStimualtorControl patternStimControl = (environment[patternStimPresenterName] as ContentPresenter).Content as PatternStimualtorControl;
                if (patternStimControl != null)
                {
                    patternStimControl.StopStimulation();
                    environment.RemoveAndDispose(patternStimPresenterName);
                    patternStimPresenterName = null;
                }
            }
            else
            {
                device = GetDevice(environment);
                if (device == null)
                    return;
                (device as IEEGPatternStimulator).StopStimulation();
                (device as IEEGPatternStimulator).SetEnabled(false);
            }
        }

        #endregion

        #region Проверка фоностимулятора

        /// <summary>
        /// Имя контент презентера контрола управления фоностимулятором
        /// </summary>
        private static string PhonoStimControlName;

        /// <summary>
        /// Запускает фоностимулятор
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="duration"> Длительность стимулов </param>
        /// <param name="period"> Период стимуляции </param>
        /// <param name="amplitude"> Амплитуда стимула </param>
        public static void StartPhonoStimulation(ScriptEnvironment environment, double duration, double period, double amplitude = 110.0)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            (device as NeuroSoft.Devices.IAudioStimulator).SetEnabled(true);
            (device as NeuroSoft.Devices.IAudioStimulator).SetStimulusDuration((float)duration);
            (device as NeuroSoft.Devices.IAudioStimulator).SetFrequency((float)(1 / period));
            (device as NeuroSoft.Devices.IAudioStimulator).SetStimulusAmplitude((float)amplitude);
            (device as NeuroSoft.Devices.IAudioStimulator).StartStimulation();
        }

        /// <summary>
        /// Отображает контрол с установками фоностимулятора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowPhonoStimControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            PhonoStimulatorControl phonoStimulatorControl = new PhonoStimulatorControl(device as NeuroSoft.Devices.IAudioStimulator);
            presenter.Content = phonoStimulatorControl;
        }

        /// <summary>
        /// Останавливает фоностимуляцию
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopPhonoStimulation(ScriptEnvironment environment)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            if (PhonoStimControlName != null && environment[PhonoStimControlName] != null)
            {
                PhonoStimulatorControl phonoStimulatorControl = (environment[PhonoStimControlName] as ContentPresenter).Content as PhonoStimulatorControl;
                if (phonoStimulatorControl != null)
                {
                    phonoStimulatorControl.StopStimulation();
                    environment.RemoveAndDispose(PhonoStimControlName);
                    PhonoStimControlName = null;
                    return;
                }
            }
            (device as NeuroSoft.Devices.IAudioStimulator).StopStimulation();
            (device as NeuroSoft.Devices.IAudioStimulator).SetEnabled(false);
        }

        #endregion

        #region Проверка токового стимулятора

        /// <summary>
        /// Запускает стимуляцию токовым стимулятором
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="duration"> Длительность импульсов стимуляции </param>
        /// <param name="amplitude"> Амплитуда импульсов стимуляции </param>
        /// <param name="period"> Период следования импульсов стимуляции </param>
        /// <param name="isPositivePolarity"> Полярность импульсов </param>
        public static void StartCurrentStimulation(ScriptEnvironment environment, double duration, double period, double amplitude, bool isPositivePolarity = true)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            (device as NeuroSoft.Devices.ICurrentStimulator).SetStimulusDuration((float)duration);
            (device as NeuroSoft.Devices.ICurrentStimulator).SetStimulusAmplitude((float)amplitude);
            (device as NeuroSoft.Devices.ICurrentStimulator).SetFrequency((float)(1.0 / period));
            if (isPositivePolarity)
                (device as NeuroSoft.Devices.ICurrentStimulator).SetPolarity(StimulusPolarity.Plus);
            else
                (device as NeuroSoft.Devices.ICurrentStimulator).SetPolarity(StimulusPolarity.Minus);

           (device as EEG4Device).SetWorkMode(EEGWorkMode.Kalibrovka);
           device.BeginTransmit();

            (device as NeuroSoft.Devices.ICurrentStimulator).SetEnabled(true);
            (device as NeuroSoft.Devices.ICurrentStimulator).StartStimulation();
        }

        /// <summary>
        /// Отображает контрол с установками токового стимулятора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowCurrentStimControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            CurrentStimulatorControl currentStimulatorControl = new CurrentStimulatorControl(device as NeuroSoft.Devices.ICurrentStimulator, device);
            presenter.Content = currentStimulatorControl;
            currentStimPresenterName = contentPresenterName;
            currentStimulatorControl.StartStimulation();
        }

        private static string currentStimPresenterName;

        /// <summary>
        /// Останавливает стимуляцию токовым стимулятором
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopCurrentStimulation(ScriptEnvironment environment)
        {
            EEG4Device device = GetDevice(environment);
            if (device == null)
                return;
            if (currentStimPresenterName != null && environment[currentStimPresenterName] != null)
            {
                CurrentStimulatorControl currStimControl = (environment[currentStimPresenterName] as ContentPresenter).Content as CurrentStimulatorControl;
                if (currStimControl != null)
                {
                    currStimControl.StopStimulation();
                    environment.RemoveAndDispose(currentStimPresenterName);
                    currentStimPresenterName = null;
                    return;
                }
            }
            (device as NeuroSoft.Devices.ICurrentStimulator).StopStimulation();
            (device as NeuroSoft.Devices.ICurrentStimulator).SetEnabled(false);
        }

        #endregion

        #region Проверка синхровхода

        /// <summary>
        /// Представляет контрол, отображающий сигнал калибровки
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartSynchroInTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings synchroInSettings = new ReadDataSettings();
            synchroInSettings.SwingRange.Min = 0;
            synchroInSettings.SwingRange.Max = 1;
            synchroInSettings.XScale = new ScaleItem(20e-3f, "20 мс/мм");
            synchroInSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");

            EEG4Device device = GetDevice(environment);
            if (device is NeuroSoft.Devices.IFlashStimulator)
            {
                EEG4FlashStimulatorState flashStimState = (EEG4FlashStimulatorState)(device as NeuroSoft.Devices.IFlashStimulator).GetState();
                flashStimState.TrigInEnabled = true;
                (device as NeuroSoft.Devices.IFlashStimulator).SetState(flashStimState);
            }
            StandOperationsEEG.ConnectEegChannelsToGnd(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransmit, synchroInSettings);
        }

        /// <summary>
        /// Представляет контрол, отображающий сигнал калибровки для прибора Нейро-ЭМГ-Микро 4
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartEmg4SynchroInTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings synchroInSettings = new ReadDataSettings();
            synchroInSettings.SwingRange.Min = 0;
            synchroInSettings.SwingRange.Max = 10;
            synchroInSettings.MinFreqFilter = 0.5;
            synchroInSettings.MaxFreqFilter = 1000;
            synchroInSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
            synchroInSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");

            EEG4Device device = GetDevice(environment);
            if (device is NeuroSoft.Devices.ICurrentStimulator)
            {
                EEG4CurrentStimulatorState currStimState = (EEG4CurrentStimulatorState)(device as NeuroSoft.Devices.ICurrentStimulator).GetState();
                currStimState.TrigInEnabled = true;
                (device as NeuroSoft.Devices.ICurrentStimulator).SetState(currStimState);
            }
            StandOperationsEEG.ConnectVpChannelsToGnd(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.VPTransmit, synchroInSettings);
        }

        /// <summary>
        /// Проверяет, зафиксировались ли синхроимпульсы
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Успешность выполнения проверки </returns>
        public static bool? CheckSynchroImpulse(ScriptEnvironment environment)
        {
            ReadDataControl readDataControl = GetReadDataControl(environment);
            if (readDataControl == null)
                return null;
            StopDataReading(environment);
            if (readDataControl.EventsCounter == 0)
            {
                CommonScripts.ShowError(Properties.Resource1.NotSynchroImpulse);
                return null;
            }
            return true;
        }

        #endregion

        #endregion

        /// <summary>
        /// Перечисление типов приборов, поддерживаемых данным классом
        /// </summary>
        public enum NeuronSpectrumTypes { Null = 0, NS_1 = 1, NS_2 = 2, NS_3 = 3, NS_4 = 4, NS_4EP = 5, EMG_Micro4 = 6, EMG_Micro2 = 7, NS_4P = 8 }
    }
}
