using System;
using System.IO;
using NeuroSoft.WPFComponents.ScalableWindows;
using System.Windows;
using NeuroSoft.Hardware.Usb;
using NeuroSoft.Hardware.Tools.Usb;
using NeuroSoft.Hardware.Common;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Controls;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using Neurosoft.Hardware.Tools.Visa;
using NeuroSoft.Hardware.Tools.Cypress;
using System.Collections.Generic;
using System.Linq;

namespace NeuroSoft.DeviceAutoTest.Common
{
    /// <summary>
    /// Общие скрипты
    /// </summary>
    public static class CommonScripts
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialNumber"></param>
        public static bool DoFirmwareEeprom(string serialNumber, IUsbDevice device,
            ProgramDescriptor weavingInformation)
        {
            byte[] serialNumberData;
            if (!weavingInformation.SerialNumberBuilder.Build(serialNumber, out serialNumberData))
                return false;

            byte[] weavingData;

            try
            {
                weavingData = File.ReadAllBytes(weavingInformation.FileName);
            }
            catch (Exception exp)
            {
                NSMessageBox.Show("При загрузке файла образа памяти возникла ошибка: " + exp.Message,
                    Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            byte[] addData;
            if (weavingInformation.AddData_FileName != null && weavingInformation.AddData_Value != null)
                throw new ApplicationException();
            if (weavingInformation.AddData_FileName != null)
            {
                try
                {
                    addData = File.ReadAllBytes(weavingInformation.AddData_FileName);
                }
                catch (Exception exp)
                {
                    NSMessageBox.Show("При загрузке файла дополнительных данных возникла ошибка: " + exp.Message,
                        Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else if (weavingInformation.AddData_Value != null)
                addData = weavingInformation.AddData_Value;
            else
                addData = null;


            if (!device.Lock())
                NSMessageBox.Show(
                    "Захватить блокировку не удалось - возможно устройство используется другой программой.",
                    Properties.Resources.Error);
            else
            {
                byte[] temp;
                int iicOffset;
                CypressTools.ErrorCode ret = CypressTools.UpdateSerialNumberAndDecode(weavingData, serialNumberData,
                    weavingInformation.Address, out temp, out iicOffset, weavingInformation.CypressCpuType);


                bool f;

                if (ret != CypressTools.ErrorCode.NoError)
                    f = (NSMessageBox.Show(CypressTools.GetErrorDescription(ret) + " Продолжить?",
                             Properties.Resources.Error, MessageBoxButton.YesNo, MessageBoxImage.Error) ==
                         MessageBoxResult.Yes);
                else
                    f = true;

                if (f)
                {
                    if (weavingInformation.PID_Enable)
                    {
                        ret = CypressTools.UpdateSerialNumberAndDecode(
                            weavingData,
                            weavingInformation.PID_Value.Int16ToArray_LittleEndian(),
                            weavingInformation.PID_Addr,
                            out temp,
                            out iicOffset,
                            weavingInformation.CypressCpuType);

                        if (ret != CypressTools.ErrorCode.NoError)
                            f = (NSMessageBox.Show(CypressTools.GetErrorDescription(ret) + " Продолжить?",
                                     Properties.Resources.Error, MessageBoxButton.YesNo, MessageBoxImage.Error) ==
                                 MessageBoxResult.Yes);
                        else
                            f = true;
                    }

                    if (f)
                    {
                        if (addData != null)
                        {
                            ret = CypressTools.UpdateSerialNumberAndDecode(weavingData, addData,
                                weavingInformation.AddData_Addr, out temp, out iicOffset,
                                weavingInformation.CypressCpuType);

                            if (ret != CypressTools.ErrorCode.NoError)
                                f = (NSMessageBox.Show(CypressTools.GetErrorDescription(ret) + " Продолжить?",
                                         Properties.Resources.Error, MessageBoxButton.YesNo,
                                         MessageBoxImage.Error) == MessageBoxResult.Yes);
                            else
                                f = true;
                        }

                        if (f)
                        {
                            ret = CypressTools.DownloadVendAxAndVerify(device, weavingInformation.CypressCpuType);

                            if (ret != CypressTools.ErrorCode.NoError)
                                f = (NSMessageBox.Show(CypressTools.GetErrorDescription(ret) + " Продолжить?",
                                         Properties.Resources.Error, MessageBoxButton.YesNo,
                                         MessageBoxImage.Error) == MessageBoxResult.Yes);
                            else
                                f = true;

                            if (f)
                            {
                                ret = CypressTools.DownloadEepromAndVerify(device, 0, weavingData);

                                if (ret != CypressTools.ErrorCode.NoError)
                                    NSMessageBox.Show(CypressTools.GetErrorDescription(ret),
                                        Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                                else
                                {
                                    NSMessageBox.Show("Программирование успешно завершено.", "",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                    return true;
                                }
                            }
                        }
                    }
                }
                device.Unlock();
            }
            device.Dispose();

            return false;
        }

        /// <summary>
        /// Сохраняет имя файла прошивки PIC контроллера
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="fileNameTemplate"> Часть имени нужного файла прошивки </param>
        /// <param name="firmwareFolder"> Путь к папке, где лежат прошивки </param>
        /// <param name="fileName">Имя файла, с помощью которого осущствлялось программирование </param>
        /// <returns></returns>
        public static bool? ProgramChipProg(ScriptEnvironment environment, string fileNameTemplate, string firmwareFolder, ref string fileName)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "HEX files|" + fileNameTemplate + "*.hex";
            openFileDialog.InitialDirectory = firmwareFolder;
            openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate + "*", firmwareFolder);
            if (openFileDialog.ShowDialog() != true)
            {
                return null;
            }
            fileName = GetShortFileName(openFileDialog.FileName);
            return true;
        }

        /// <summary>
        /// Возвращает название первого найденного файла прошивки, удовлетворяющего критерию поиска
        /// </summary>
        /// <param name="pattern"> Часть имени файла</param>
        /// <returns> Имя первого обнаруженного файла, удовлетворяющего условиям </returns>
        private static string GetFirmwareFileName(string pattern, string firmwareFolder)
        {
            string[] files = Directory.GetFiles(firmwareFolder, pattern, SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                return files[0];
            }
            return null;
        }

        /// <summary>
        /// Возвращает только имя файла без пути к нему
        /// </summary>
        /// <param name="fileName"> Полное имя файла </param>
        /// <returns> Имя файла </returns>
        public static string GetShortFileName(string fileName)
        {
            try
            {
                return new FileInfo(fileName).Name;
            }
            catch
            {
                return fileName;
            }
        }

        /// <summary>
        /// DoEvents
        /// </summary>
        public static void DoEvents()
        {
            NeuroSoft.WPFPrototype.Interface.Common.Services.DoEvents();
        }

        /// <summary>
        /// Проверка возможности открыть стенд
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static ValidationResult ValidateOpenStand(ScriptEnvironment environment)
        {
            try
            {
                environment.OpenStand();
                UniversalTestStand stand = environment.Stand;
                if (stand == null)
                {
                    return new ValidationResult(false, Properties.Resources.CantOpenStand);
                }
            }
            finally
            {
                environment.CloseStand();
            }
            return new ValidationResult(true, null);
        }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        /// <param name="errorContent"></param>
        public static void ShowError(object errorContent)
        {
            NSMessageBox.Show(Convert.ToString(errorContent), Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        /// <param name="errorMsg"></param>
        public static void ShowError(string errorMsg)
        {
            NSMessageBox.Show(errorMsg, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region Функции работы с осциллографом

        public static bool CheckOscScopeConnection(ScriptEnvironment environment)
        {
            NS_VisaOsc oscScope = (environment.Data["OscScopeControl"] as OscScopeControl).OscScope as NS_VisaOsc;
            if (oscScope != null)
            {
                if (!oscScope.IsRsrcUsed())
                {
                    ShowError(Properties.Resources.ErrOscScopeNotConnected);
                    return false;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static VENDOR GetOscScopeVendor(ScriptEnvironment environment)
        {
            NS_VisaOsc oscScope = (environment.Data["OscScopeControl"] as OscScopeControl).OscScope as NS_VisaOsc;

            if (oscScope != null)
            {
                if (oscScope.IsRsrcUsed())
                    return oscScope._idVendor;
            }
            return VENDOR.ID_NONE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chNum"></param>
        /// <param name="ampPkPk"></param>
        /// <param name="impPeriod"></param>
        /// <param name="impDuration"></param>
        /// <returns></returns>
        public static bool ReadOscScopeValues(ScriptEnvironment environment, Int32 chNum, ref double ampPkPk, ref double impPeriod, ref double impDuration, bool isAmpget = false)
        {
            CHANNELS chanNum = GetChanNum(chNum);

            try
            {
                NS_VisaOsc oscScope = (environment.Data["OscScopeControl"] as OscScopeControl).OscScope as NS_VisaOsc;
                if (oscScope != null)
                {
                    if (oscScope.IsRsrcUsed())
                    {
                        string tmpStr = oscScope.GetPK2PKValue(chanNum);
                        ampPkPk = double.Parse(oscScope.GetPK2PKValue(chanNum, isAmpget), System.Globalization.NumberFormatInfo.InvariantInfo);
                        impPeriod = double.Parse(oscScope.GetPeriodValue(chanNum), System.Globalization.NumberFormatInfo.InvariantInfo);
                        impDuration = double.Parse(oscScope.GetPWidthValue(chanNum), System.Globalization.NumberFormatInfo.InvariantInfo);
                        return true;
                    }
                    ShowError(Properties.Resources.ErrOscScopeNotConnected);
                }
            }
            catch 
            {
                ShowError(Properties.Resources.ErrOscScopeReadData);
            }
            return false;
        }


        public static bool ReadOscScopePkPkValue(ScriptEnvironment environment, int chNum, ref double ampPkPk, bool isAmpGet = false)
        {
            CHANNELS chanNum = GetChanNum(chNum);
            try
            {
                NS_VisaOsc oscScope = (environment.Data["OscScopeControl"] as OscScopeControl).OscScope as NS_VisaOsc;
                if (oscScope != null)
                {
                    if (oscScope.IsRsrcUsed())
                    {
                        //string tmpStr = oscScope.GetPK2PKValue(chanNum);
                        ampPkPk = double.Parse(oscScope.GetPK2PKValue(chanNum, isAmpGet), System.Globalization.NumberFormatInfo.InvariantInfo);
                        return true;
                    }
                    ShowError(Properties.Resources.ErrOscScopeNotConnected);
                }
            }
            catch
            {
                ShowError(Properties.Resources.ErrOscScopeReadData);
            }
            return false;
        }

        public static bool ReadOscScopePeriodValue(ScriptEnvironment environment, int chNum, ref double period)
        {
            CHANNELS chanNum = GetChanNum(chNum);
            try
            {
                NS_VisaOsc oscScope = (environment.Data["OscScopeControl"] as OscScopeControl).OscScope as NS_VisaOsc;
                if (oscScope != null)
                {
                    if (oscScope.IsRsrcUsed())
                    {
                        //string tmpStr = oscScope.GetPK2PKValue(chanNum);
                        period = double.Parse(oscScope.GetPeriodValue(chanNum), System.Globalization.NumberFormatInfo.InvariantInfo);
                        return true;
                    }
                    ShowError(Properties.Resources.ErrOscScopeNotConnected);
                }
            }
            catch
            {
                ShowError(Properties.Resources.ErrOscScopeReadData);
            }
            return false;
        }

        public static bool ReadOscScopeDurationValue(ScriptEnvironment environment, int chNum, ref double duration, bool isNegWidth = false)
        {
            CHANNELS chanNum = GetChanNum(chNum);
            try
            {
                NS_VisaOsc oscScope = (environment.Data["OscScopeControl"] as OscScopeControl).OscScope as NS_VisaOsc;
                if (oscScope != null)
                {
                    if (oscScope.IsRsrcUsed())
                    {
                        //string tmpStr = oscScope.GetPK2PKValue(chanNum);
                        duration = double.Parse(oscScope.GetPWidthValue(chanNum, isNegWidth), System.Globalization.NumberFormatInfo.InvariantInfo);
                        return true;
                    }
                    ShowError(Properties.Resources.ErrOscScopeNotConnected);
                }
            }
            catch
            {
                ShowError(Properties.Resources.ErrOscScopeReadData);
            }
            return false;
        }

        private static CHANNELS GetChanNum(int chNum)
        {
            CHANNELS chanNum = CHANNELS.ID_CHANNEL1;
            switch (chNum)
            {
                case 1:
                    chanNum = CHANNELS.ID_CHANNEL1;
                    break;
                case 2:
                    chanNum = CHANNELS.ID_CHANNEL2;
                    break;
                case 3:
                    chanNum = CHANNELS.ID_CHANNEL3;
                    break;
                case 4:
                    chanNum = CHANNELS.ID_CHANNEL4;
                    break;
            }
            return chanNum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="chansQuant"></param>
        /// <param name="Yscale"></param>
        /// <param name="Ypos1"></param>
        /// <param name="Ypos2"></param>
        /// <param name="Tscale"></param>
        /// <param name="Tpos"></param>
        /// <param name="trigLevel"></param>
        /// <returns></returns>
        public static bool SetOscScopeSettings(ScriptEnvironment environment, uint chansQuant, double Yscale, double Ypos1, double Ypos2, double Tscale, double Tpos, double trigLevel)
        {
            OscSettings oscSettings = new OscSettings();
            NS_VisaOsc oscScope = (environment.Data["OscScopeControl"] as OscScopeControl).OscScope as NS_VisaOsc;

            switch (chansQuant)
            {
                case 1:
                    oscSettings.Ch1OnOff = "1";
                    oscSettings.Ch2OnOff = "0";
                    break;
                case 2:
                    oscSettings.Ch1OnOff = oscSettings.Ch2OnOff = "1";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (oscScope != null)
            {
                if (oscScope.IsRsrcUsed())
                {
                    oscSettings.Ch1VerScale = oscSettings.Ch2VerScale = Yscale.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    oscSettings.Ch1VerPosition = Ypos1.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    oscSettings.Ch2VerPosition = Ypos2.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    oscSettings.HorScale = Tscale.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (oscSettings.HorScale.Contains("E"))
                        oscSettings.HorScale = String.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{0:0.000000000}", double.Parse(oscSettings.HorScale, System.Globalization.NumberFormatInfo.InvariantInfo));
                    oscSettings.HorPosition = Tpos.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    oscSettings.TrigLevel = trigLevel.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    oscScope.SetOscSettings(oscSettings);
                    return true;
                }
                //ShowError(Properties.Resources.ErrOscScopeNotConnected);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="setAvgAcqMode"></param>
        public static void SetAcquireAvgMode(ScriptEnvironment environment, bool setAvgAcqMode)
        {
            NS_VisaOsc oscScope = (environment.Data["OscScopeControl"] as OscScopeControl).OscScope as NS_VisaOsc;
            if (oscScope != null)
            {
                if (oscScope.IsRsrcUsed())
                {
                    if (oscScope.IsAcquireAverage != setAvgAcqMode)
                        oscScope.IsAcquireAverage = setAvgAcqMode;
                }
            }
        }

        #endregion
        
        /// <summary>
        /// Получить суффикс серийного номера
        /// </summary>
        /// <returns></returns>
        public static string GetSnSuffix()
        {
            var date = DateTime.Now;
            return new string(new[] { letterNumEquals[date.Month], letterNumEquals[date.Year - 2000] });
        }

        /// <summary>
        /// Буквенные эквиваленты номера месяца и года
        /// </summary>
        private static Dictionary<int, char> letterNumEquals = new Dictionary<int, char>()
        {
            { 1, 'K' },
            { 2, 'L' },
            { 3, 'M' },
            { 4, 'N' },
            { 5, 'O' },
            { 6, 'P' },
            { 7, 'Q' },
            { 8, 'R' },
            { 9, 'S' },
            { 10, 'T' },
            { 11, 'U' },
            { 12, 'V' },
            { 13, 'W' },
            { 14, 'X' },
            { 15, 'Y' },
            { 16, 'Z' },
            { 17, 'A' },
            { 18, 'B' },
            { 19, 'C' },
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AutoTestAction
    {
        Execute, 
        Success,
        Error
    }
}
