using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest.Dialogs;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NeuroSoft.Hardware.Tools.Usb;
using System.IO;
using NeuroSoft.Hardware.Devices.Base;
using System.Diagnostics;
using System.Reflection;
using NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.Hardware.Common;
using NeuroSoft.Hardware.Tools.Cypress;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Scripts
{    
    /// <summary>
    /// Набор скриптов для наладки прибора NeuroMEP-MicroM
    /// </summary>
    public static class NeuroMEPMicroMScripts
    {        
        #region Test Voltage and Supply Current
        
        /// <summary>
        /// Проверка допустимости напряжения
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="voltage"></param>
        /// <param name="minVoltage"></param>
        /// <param name="maxVoltage"></param>
        /// <param name="voltageName"></param>
        /// <returns></returns>
        public static bool TestVoltage(ScriptEnvironment environment, double voltage, double minVoltage, double maxVoltage, string voltageName)
        {
            if (voltage < minVoltage || voltage > maxVoltage)
            {
                NSMessageBox.Show(string.Format("Недопустимое значение напряжения: '{0}'. Внесите исправления и запустите тест заново.", voltageName), Properties.Resources.Error, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                //environment.TestWasCorrected = true;
                return false;
            }
            return true;
        }        
        
        #endregion

        #region Firmware

        #region EEPROM

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool? DoFirmwareEeprom(ScriptEnvironment environment, ref string fileName, string fwFileNameTemp = "CY7C68013A_r*_d*.iic", string fwFolderPath = null)
        {            
            SelectDeviceDialog selectDeviceDialog = new SelectDeviceDialog();
            if (selectDeviceDialog.ShowDialog() == true)
            {
                string fileNameTemplate = fwFileNameTemp;
                if (fwFolderPath == null)
                    fwFolderPath = FirmwareFolder;
                else
                    fwFolderPath = Directory.Exists(fwFolderPath) ? fwFolderPath : @"C:\";
                var usbItem = selectDeviceDialog.SelectedDeviceItem.Device;
                
                //var usbDevsListCntr = new UsbDevicesListControl();
                var weavingInformation = new ProgramDescriptor();
                weavingInformation.SerialNumberBuilder = new SerialNumberBuilderString(SerialNumberBuilderString.StringType.StringUsbDescriptor, 126);
                weavingInformation.Address = 7936;
                weavingInformation.CypressCpuType = CypressTools.CypressCpuType.FX1_FX2LP;                

                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "iic|" + fileNameTemplate;
                openFileDialog.InitialDirectory = fwFolderPath;
                openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate);
                if (openFileDialog.ShowDialog() != true)
                {
                    return null;
                }
                weavingInformation.FileName = openFileDialog.FileName;
                bool result = CommonScripts.DoFirmwareEeprom(environment.DeviceSerialNumber, usbItem, weavingInformation);
                if (result)
                {
                    fileName = GetShortFileName(weavingInformation.FileName);
                    return true;
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        public static string DateNowAsString
        {
            get
            {
                return DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            }
        }
        #endregion

        #region CPLD  
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool? ProgramCPLD(ScriptEnvironment environment, ref string fileName, string fwFileNameTemp = "EPM240GT100_*_d*.jbc", string fwFolderPath = null)
        {
            string fileNameTemplate = fwFileNameTemp;
            if (fwFolderPath == null)
                fwFolderPath = FirmwareFolder;
            else
                fwFolderPath = Directory.Exists(fwFolderPath) ? fwFolderPath : @"C:\";
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "jbc|" + fileNameTemplate;
            openFileDialog.InitialDirectory = fwFolderPath;
            openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate);
            if (openFileDialog.ShowDialog() != true)
            {
                return null;
            }
            //чтение данных файла
            byte[] program = null;
            string CPLDFileName = openFileDialog.FileName;            
            try
            {
                program = File.ReadAllBytes(CPLDFileName);
            }
            catch
            {
                NSMessageBox.Show(Properties.Resources.OpenFileError, Properties.Resources.Error, MessageBoxButton.OK);
                return null;
            }
            //открытие девайса
            NeuroMepMicroM2 device = (NeuroMepMicroM2)NeuroMepMicroM2.DeviceType.OpenDevice(environment.DeviceSerialNumber,
                                                                                            callBackArgs =>
                                                                                            {
                                                                                                NSMessageBox.Show(callBackArgs.Data.Descriptor.Message, Properties.Resources.Error, MessageBoxButton.OK);
                                                                                            });

            if (device == null)
            {
                NSMessageBox.Show(Properties.Resources.CouldNotOpenDevice, Properties.Resources.Error, MessageBoxButton.OK);
                return null;
            }
            StringBuilder cpldLog = new StringBuilder();
            Action<DebugMsgEventArgs> messageListener = (args) =>
            {
                cpldLog.AppendLine(args.Type + ": " + args.Message);
            };
            try
            {             
                //программирование ПЛИС
                bool success = device.DebugCpldRun(program, "PROGRAM", messageListener);
                if (success)
                {
                    fileName = GetShortFileName(CPLDFileName);
                    NSMessageBox.Show(Properties.Resources.SuccessProgramCpld, "", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                else
                {
                    NSMessageBox.Show(string.Format(Properties.Resources.CouldNotProgramCpld, cpldLog), Properties.Resources.Error, MessageBoxButton.OK);
                }
            }
            finally
            {
                device.Close();
            }
            return null;
        }

        #endregion

        #region ChipProg
        /// <summary>
        /// Сохранение имя файла прошивки
        /// </summary>
        /// <param name="environment"> переменная окружения скрипта </param>
        /// <param name="fileNameTmp"> Шаблон имени файла прошивки </param>
        /// <param name="fileName"> Имя файла прошивки </param>
        /// <returns> Результат сохранения имени файла </returns>
        public static bool? ProgramChipProg(ScriptEnvironment environment, ref string fileName, string fileNameTmp = "PIC18F252_r*_d*", string fwFolderPath = null)
        {
            string fileNameTemplate = fileNameTmp;
            if (fwFolderPath == null)
                fwFolderPath = FirmwareFolder;
            else
                fwFolderPath = Directory.Exists(fwFolderPath) ? fwFolderPath : @"C:\";
            return CommonScripts.ProgramChipProg(environment, fileNameTemplate, fwFolderPath, ref fileName);
        }
        #endregion

        #region LPC_Programming
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetFlashMagicFileName(string fwFileNameTemp = "lpc2103_r*_d*.hex", string fwFolderPath = null)
        {
            string fileNameTemplate = fwFileNameTemp;
            if (fwFolderPath == null)
                fwFolderPath = FirmwareFolder;
            else
                fwFolderPath = Directory.Exists(fwFolderPath) ? fwFolderPath : @"C:\";
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "hex|" + fileNameTemplate;
            openFileDialog.InitialDirectory = fwFolderPath;
            openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate);
            if (openFileDialog.ShowDialog() != true)
            {
                return null;
            }
            return GetShortFileName(openFileDialog.FileName);
        }
        #endregion

        #region C8051F330 Programming
        /// <summary>
        /// Возвращает имя файла прошивки контроллера блока токового стимулятора.
        /// </summary>
        /// <returns>Имя файла прошивки контроллера блока токового стимулятора.</returns>
        //public static string GetCurstimControllerFileName()
        //{
        //    string fileNameTemplate = "C8051F330_r*_d*.hex";
        //    Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        //    openFileDialog.Filter = "hex|" + fileNameTemplate;
        //    openFileDialog.InitialDirectory = FirmwareFolder;
        //    openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate);
        //    if (openFileDialog.ShowDialog() != true)
        //    {
        //        return null;
        //    }
        //    return openFileDialog.FileName;
        //}
        #endregion

        private static string GetFirmwareFileName(string pattern)
        {            
            string[] files = Directory.GetFiles(FirmwareFolder, pattern, SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                return files[0];
            }            
            return null;
        }

        internal static string FirmwareFolder
        {
            get
            {
                string fwFolder = @"\\server\firmware\[034] НейроМВП-МикроМ\";
                return Directory.Exists(fwFolder) ? fwFolder : @"C:\";
            }
        }
        #endregion        

        #region DeviceOpening
        /// <summary>
        /// Валидация возможности открытия устройства
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static ValidationResult ValidateDeviceOpening(ScriptEnvironment environment, bool details = false)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
            {                
                try
                {
                    device = (NeuroMepMicroM2)NeuroMepMicroM2.DeviceType.OpenDevice(environment.DeviceSerialNumber,
                                                                                   callBackArgs => NSMessageBox.Show(callBackArgs.Data.Descriptor.Message, Properties.Resources.Error, MessageBoxButton.OK));
                    if (device == null)
                    {
                        return new ValidationResult(false, Properties.Resources.CouldNotOpenDevice);
                    }
                }
                catch
                {
                    return new ValidationResult(false, Properties.Resources.CouldNotOpenDevice);
                }               
            }
            return new ValidationResult(true, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static NeuroMepMicroM2 OpenDevice(ScriptEnvironment environment)
        {
            NeuroMepMicroM2 device = environment.Device as NeuroMepMicroM2;            
            if (device == null)
            {
                device = (NeuroMepMicroM2)NeuroMepMicroM2.DeviceType.OpenDevice(environment.DeviceSerialNumber,
                                                                               callBackArgs => NSMessageBox.Show(callBackArgs.Data.Descriptor.Message, Properties.Resources.Error, MessageBoxButton.OK));
                environment.InitDevice(new NeuroMepMicroMWrapper(device));
            }            
            return device;
        }
        #endregion

        #region SynchroOut
        
        /// <summary>
        /// Проверка синхровыхода при положительной полярности
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="positivePolary">Полярность импульса</param>
        /// <param name="durationIndex">Индекс продолжительности импульса</param>        
        public static void StartSynchroOut(ScriptEnvironment environment, bool positivePolary, int durationIndex)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            device.SynchroOutEnable = true;
            device.SynchroOutImpulseDurationIndex = durationIndex;
            device.SynchroOutInversePolarity = !positivePolary;

            var timer = environment.CreateItem("SynchroOutTimer", new DispatcherTimer()) as DispatcherTimer;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                device.DebugGenerateSyncPulse();
            });
            timer.Start();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void StopSynchroOut(ScriptEnvironment environment)
        {
            var timer = environment["SynchroOutTimer"] as DispatcherTimer;
            if (timer != null)
            {
                timer.Stop();                
            }
            environment.RemoveAndDispose("SynchroOutTimer");
            environment.CloseDevice();
        }

        #endregion

        #region FlashStimulator

        /// <summary>
        /// Включение фотостимулятора
        /// </summary>
        /// <param name="environment"></param>
        public static void EnableFlashStim(ScriptEnvironment environment)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //if (!DebugSetControlWord(environment, null, null, false, false, false, true, 0))
            //    CommonScripts.ShowError("Не удалось включить 5 В!");
            //
            if (!device.FlashStimulator.ModuleIsOn)
                device.FlashStimulator.ModuleOn(errInfo => CommonScripts.ShowError(Common.Properties.Resources.CantEnableStimulator));
        }

        /// <summary>
        /// Выключение фотостимулятора
        /// </summary>
        /// <param name="environment"></param>
        public static void DisableFlashStim(ScriptEnvironment environment)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //DebugSetControlWord(environment, null, null, false, false, false, false, 0);
            device.FlashStimulator.ModuleOff();
        }

        internal const string FlashStimulatorKey = "FlashStimulatorSettings";        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void InitFlashStim(ScriptEnvironment environment, string presenterStr, bool captureSyncImp = false, double defaultPeriod = 0.1, double defaultDuration = 0.01)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //if (!DebugSetControlWord(environment, null, null, false, false, false, true, 0))
            //    CommonScripts.ShowError("Не удалось включить 5 В!");
            //
            FlashStimulatorSettings settings = new FlashStimulatorSettings(device);
            settings.Period = defaultPeriod;
            settings.Duration = defaultDuration;
            var presenter = environment.GetPresenter(presenterStr);
            if (presenter != null)
            {
                presenter.Content = settings;
            }
            environment[FlashStimulatorKey] = settings;
            settings.StartStimulation(captureSyncImp);
        }

        /// <summary>
        /// Проверяет наличие синхроимпульсов от стимулятора
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static bool? CheckFlashStimSyncImp(ScriptEnvironment environment)
        {
            StimulatorSettings<IFlashStimulator> stimSettings = environment[FlashStimulatorKey] as StimulatorSettings<IFlashStimulator>;
            if (stimSettings != null)
            {
                if (stimSettings.StimulsCaptured)
                    return true;
                else
                {
                    CommonScripts.ShowError(Properties.Resources.SyncStimsNotCaptured);
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Выключение фотостимуляции
        /// </summary>
        /// <param name="environment"></param>     
        public static void StopFlashStim(ScriptEnvironment environment)
        {
            environment.RemoveAndDispose(FlashStimulatorKey);
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //DebugSetControlWord(environment, null, null, false, false, false, false, 0);
        }
        #endregion

        #region PhonoStimulator

        internal const string PhonoStimulatorKey = "PhonoStimulatorSettings";
        /// <summary>
        /// Включение фоностимулятора
        /// </summary>
        /// <param name="environment"></param>
        public static void EnablePhonoStim(ScriptEnvironment environment)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //if (!DebugSetControlWord(environment, null, null, false, false, false, true, 0))
            //    CommonScripts.ShowError("Не удалось включить 5 В!");

            if (!device.PhonoStimulator.ModuleIsOn)
                device.PhonoStimulator.ModuleOn(errInfo => CommonScripts.ShowError(Common.Properties.Resources.CantEnableStimulator));
        }

        /// <summary>
        /// Выключение фоностимулятора
        /// </summary>
        /// <param name="environment"></param>
        public static void DisablePhonoStim(ScriptEnvironment environment)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //DebugSetControlWord(environment, null, null, false, false, false, false, 0);
            device.PhonoStimulator.ModuleOff();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="presenterStr"></param>
        public static void StartPhonoStim(ScriptEnvironment environment, string presenterStr = "", double defaultPeriod = 0.1, double defaultAmpl = 100.0, double defaultDuration = 0.001)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //if (!DebugSetControlWord(environment, null, null, false, false, false, true, 0))
            //    CommonScripts.ShowError("Не удалось включить 5 В!");
            //
            PhonoStimulatorSettings settings = new PhonoStimulatorSettings(device);
            settings.Period = defaultPeriod;
            settings.ValueMeanLeft = defaultAmpl;
            settings.ValueMeanRight = defaultAmpl;
            settings.LeftImpulseDuration = defaultDuration;
            settings.RightImpulseDuration = defaultDuration;
            var presenter = environment.GetPresenter(presenterStr);
            if (presenter != null)
            {
                presenter.Content = settings;
            }
            environment[PhonoStimulatorKey] = settings;
            settings.StartStimulation();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="presenterStr"></param>
        public static void StartPhonoStim(ScriptEnvironment environment, double defaultPeriod = 0.1, double defaultAmpl = 100.0, double defaultDuration = 0.001)
        {
            StartPhonoStim(environment, "", defaultPeriod, defaultAmpl, defaultDuration);            
        }

        /// <summary>
        /// Выключение фоностимуляции
        /// </summary>
        /// <param name="environment"></param>     
        public static void StopPhonoStim(ScriptEnvironment environment)
        {       
            environment.RemoveAndDispose(PhonoStimulatorKey);
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //DebugSetControlWord(environment, null, null, false, false, false, false, 0);
        }
        #endregion

        #region Pattern Stimulation        
        internal const string PatternStimulatorKey = "PatternStimulatorSettings";
        /// <summary>
        /// Включение шахматного паттерна
        /// </summary>
        /// <param name="environment"></param>
        public static bool EnablePatternStim(ScriptEnvironment environment)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return false;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //if (!DebugSetControlWord(environment, null, null, false, false, true, true, 0))
            //    CommonScripts.ShowError("Не удалось включить 5 В!");

            if (device.PatternStimulator.ModuleIsOn)
            {
                return true;
            }
            var onResult = true;
            if (!device.PatternStimulator.ModuleIsOn)
                device.PatternStimulator.ModuleOn(errInfo =>
                {
                    CommonScripts.ShowError(Common.Properties.Resources.CantEnableStimulator);
                    onResult = false;
                });
            return onResult;
        }

        /// <summary>
        /// Выключение шахматного паттерна
        /// </summary>
        /// <param name="environment"></param>
        public static void DisablePatternStim(ScriptEnvironment environment)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //DebugSetControlWord(environment, null, null, false, false, false, false, 0);
            device.PatternStimulator.ModuleOff();
        }


        /// <summary>
        /// Запуск стимуляции шахматным паттерном
        /// </summary>
        /// <param name="environment"></param>        
        public static void StartPatternStim(ScriptEnvironment environment, string presenterStr = "", double defaultPeriod = 0.5,
            PatternTypeEnum defaultPatternType = PatternTypeEnum._8x6, PatternPointSizeEnum defaultPointSize = PatternPointSizeEnum.Small)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //if (!DebugSetControlWord(environment, null, null, false, false, true, true, 0))
            //    CommonScripts.ShowError("Не удалось включить 5 В!");
            //
            PatternStimulatorSettings settings = new PatternStimulatorSettings(device);
            settings.Period = defaultPeriod;
            settings.PatternType = defaultPatternType;
            settings.PatternPointSize = defaultPointSize;
            var presenter = environment.GetPresenter(presenterStr);
            if (presenter != null)
            {
                presenter.Content = settings;
            }
            environment[PatternStimulatorKey] = settings;
            device.SynchroOutEnable = true;
            settings.StartStimulation();            
        }

        /// <summary>
        /// Выключение стимуляции шахматным паттерном
        /// </summary>
        /// <param name="environment"></param>     
        public static void StopPatternStim(ScriptEnvironment environment)
        {
            environment.RemoveAndDispose(PatternStimulatorKey);
            // Временное решение для случая, когда фото-фоно стимулятор стали запитывать от стабилизатора 5 В (07.12.2012)
            //DebugSetControlWord(environment, null, null, false, false, false, false, 0);
        }

        #endregion     

        #region CPLD ControlWord
        /// <summary>
        /// Отправка слова CPLD
        /// </summary>
        /// <param name="syncInPol">Отрицательная полярность синхровхода?</param>
        /// <param name="syncOutPol">Отрицательная полярность синхровыхода?</param>
        /// <param name="bitAdcOn">Включить АЦП</param>
        /// <param name="bitCurOn">Включить токовый стимулятор</param>
        /// <param name="bitPhoOn">Включить фоностимулятор</param>
        /// <param name="bit5vOn">Включить 5 вольт</param>
        /// <param name="synchroOutDurationIndex">Длительность импульса синхровыхода: 0 - выкл, 1 - 200 мкс, 2 - 500 мкс, 3 - 1 мс, 4 - 2 мс, 5 - 5мс</param>
        /// <returns></returns>
        public static bool DebugSetControlWord(ScriptEnvironment environment, bool? syncInPol, bool? syncOutPol, bool? bitAdcOn, bool? bitCurOn, bool? bitPhoOn, bool? bit5vOn, int? synchroOutDurationIndex)
        {            
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return false;            
            device.DebugSetControlWord(syncInPol, syncOutPol, bitAdcOn, bitCurOn, bitPhoOn, bit5vOn, (NeuroMepMicroM2.DebugSynchroOutDuration?)synchroOutDurationIndex);
            return true;    //device.DebugSetControlWord(syncInPol, syncOutPol, bitAdcOn, bitCurOn, bitPhoOn, bit5vOn, (NeuroMepMicroM2.DebugSynchroOutDuration?)synchroOutDurationIndex);
        }
        #endregion

        #region Impedance

        private static Dictionary<int, string> GetImpedanceDescriptions()
        {            
            Dictionary<int, string> impedanceDescriptions = new Dictionary<int, string>();
            impedanceDescriptions.Add(0, "Ground");
            impedanceDescriptions.Add(1, "Ch1+");
            impedanceDescriptions.Add(2, "Ch1-");
            impedanceDescriptions.Add(3, "Ch2+");
            impedanceDescriptions.Add(4, "Ch2-");
            return impedanceDescriptions;
        }

        /// <summary>
        /// Проверка распайки кабелей от платы до панели разъемов
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>        
        public static void ShowImpedanceSoldering(ScriptEnvironment environment, string contentPresenterName)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
            {
                return;
            }
            environment.RemoveAndDispose("ImpedanceSoldering");
            double measureFreq = (double)NeuroMepMicroM2.AmplifierCapabilities.GetType()
                                                          .GetField("ImpedanceMeasureFreq", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic)
                                                          .GetValue(NeuroMepMicroM2.AmplifierCapabilities);
            //double measureFreq = (double)tp; //NeuroMepMicroM2.AmplifierCapabilities.ImpedanceMeasureFreq;
            ImpedancesSolderingControl solderingControl = new ImpedancesSolderingControl(environment, device, measureFreq, NeuroMepMicroM2.AmplifierCapabilities.ChannelsCount);
            solderingControl.TestImpedanceRange.Min = 0;
            solderingControl.TestImpedanceRange.Max = 1000;
            solderingControl.StartTestSoldering += solderingControl_StartTestSoldering;
            presenter.Content = solderingControl;
            environment["ImpedanceSoldering"] = solderingControl;      
            solderingControl.StartTesting();      
        }

        static void solderingControl_StartTestSoldering(object sender, int electrode)
        {
            var solderingControl = sender as ImpedancesSolderingControl;
            if (solderingControl == null)
                return;            
            var environment = solderingControl.Environment;
            environment.OpenStand();
            var stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            switch (electrode)
            {
                case 1:
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
                case 2:
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
                case 3:
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
                    break;
                case 4:
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
            }
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static bool? TestImpedanceSoldering(ScriptEnvironment environment)
        {
            ImpedancesSolderingControl impedanceSoldering = environment["ImpedanceSoldering"] as ImpedancesSolderingControl;
            if (impedanceSoldering == null)
                return null;
            if (impedanceSoldering.TestingResult == true)
            {
                return true;
            }
            else 
            {
                CommonScripts.ShowError("Проверка распайки не пройдена. Добейтесь успешного прохождения проверки, после чего повторно выполните тест.");
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void StopImpedanceSoldering(ScriptEnvironment environment)
        {
            ImpedancesSolderingControl impedanceSoldering = environment["ImpedanceSoldering"] as ImpedancesSolderingControl;
            if (impedanceSoldering != null)
            {                
                impedanceSoldering.StartTestSoldering -= new StartTestSolderingEventHandler(solderingControl_StartTestSoldering);
                impedanceSoldering.Stop();
            }
            ResetCommutatorState(environment);
            CommonScripts.DoEvents();
        }

        /// <summary>
        /// Метод отображения импеданса внутри заданного ContentPresenter'а
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        /// <param name="minResistance"></param>
        /// <param name="maxResistance"></param>
        /// <param name="minCapacity"></param>
        /// <param name="maxCapacity"></param>
        public static void ShowImpedance(ScriptEnvironment environment, string contentPresenterName, double minResistance, double maxResistance, double minCapacity, double maxCapacity)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
            {
                return;
            }
            ImpedancesView impedancesControl = new ImpedancesView();
            var impedanceTitles = GetImpedanceDescriptions();
            //double measureFreq = NeuroMepMicroM2.AmplifierCapabilities.ImpedanceMeasureFreq;
            double measureFreq = (double)NeuroMepMicroM2.AmplifierCapabilities.GetType()
                                                                              .GetField("ImpedanceMeasureFreq", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic)
                                                                              .GetValue(NeuroMepMicroM2.AmplifierCapabilities);
            foreach (int key in impedanceTitles.Keys)
            {
                if (key == 0)
                    impedancesControl.Impedances.Add(new ImpedanceInfo(key, impedanceTitles[key], measureFreq));
                else
                    impedancesControl.Impedances.Add(new ImpedanceInfo(key, impedanceTitles[key], measureFreq, minResistance, maxResistance, minCapacity, maxCapacity));
            }
            presenter.Content = impedancesControl;
            environment.RemoveAndDispose("ImpedanceIndicator");
            ImpedanceIndicator indicator = new ImpedanceIndicator(environment, impedancesControl, device);
            environment["ImpedanceIndicator"] = indicator;
            indicator.Start();
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stand"></param>
        public static void ResetCommutatorState(ScriptEnvironment environment)
        {
            StandOperations.ResetCommutatorState(environment);
        }

        /// <summary>
        /// Установка сопротивления (Ом)
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="is10Or100KOm">Если true, то 10 КОм, если false - 100 КОм</param>
        public static void SetTestImpedance(ScriptEnvironment environment, bool is10KOm = true)
        {
            environment.OpenStand();
            var stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            if (is10KOm)
            {
                stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._10kOhm;
                stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._10kOhm;
                stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._10kOhm;
                stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._10kOhm;
                stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
            }
            else
            {
                //stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1._100_Ohm;
                stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
            }
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Остановка отображения импеданса
        /// </summary>
        /// <param name="environment"></param>
        public static void StopImpedance(ScriptEnvironment environment)
        {
            environment.RemoveAndDispose("ImpedanceIndicator");
        }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        /// <param name="errorContent"></param>
        public static void ShowError(object errorContent)
        {
            CommonScripts.ShowError(errorContent);
        }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        /// <param name="errorMsg"></param>
        public static void ShowError(string errorMsg)
        {
            CommonScripts.ShowError(errorMsg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static ValidationResult ValidateOpenStand(ScriptEnvironment environment)
        {
            return CommonScripts.ValidateOpenStand(environment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static ValidationResult ValidateImpedance(ScriptEnvironment environment)
        {
            var validateOpenStand = CommonScripts.ValidateOpenStand(environment);
            if (!validateOpenStand.IsValid)
            {
                return validateOpenStand;
            }
            var device = OpenDevice(environment);
            if (device == null)
            {
                return new ValidationResult(false, Properties.Resources.CouldNotOpenDevice);
            }
            var powerOnResult = true;
            if (!device.Amplifier.ModuleIsOn)
                device.AmplifierModuleOn(errInfo => powerOnResult = false);
            if (!powerOnResult)
            {
                return new ValidationResult(false, "Не удалось включить блок усилителя");
            }
            else
            {
                device.AmplifierPowerOff();
            }
            return new ValidationResult(true, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static RangedValue<double>[] GetLastImpedances(ScriptEnvironment environment, bool isResistanceRequest = true)
        {
            ImpedanceIndicator impedanceIndicator = environment["ImpedanceIndicator"] as ImpedanceIndicator;
            if (impedanceIndicator == null)
                return null;
            if (isResistanceRequest)
                return impedanceIndicator.LastResistances;
            else
                return impedanceIndicator.LastCapacities;
        }
        #endregion

        #region Adc
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static bool? CheckAdcPowerOn(ScriptEnvironment environment)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return null;
            try
            {
                var ampOnResult = true;
                if (!device.Amplifier.ModuleIsOn)
                    device.AmplifierModuleOn(err => ampOnResult = false); 
                return ampOnResult;
            }
            finally
            {
                device.AmplifierPowerOff();
            }
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        /// <param name="settings"></param>
        public static void StartReadSamples(ScriptEnvironment environment, string contentPresenterName, ReadDataSettings settings = null)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;            
            if (presenter == null)
            {
                return;
            }
            if (!device.Amplifier.ModuleIsOn)
                device.AmplifierModuleOn(errInfo => CommonScripts.ShowError(errInfo.Data.Descriptor.Message));
            ReadDataControl readDataControl = new ReadDataControl(device, NeuroMepMicroM2.AmplifierCapabilities);

            if (settings != null)
            {
                readDataControl.SetSettings(settings);
            }            
            presenter.Content = readDataControl;
            environment.RemoveAndDispose("ReadDataControl");
            environment["ReadDataControl"] = readDataControl;
            readDataControl.StartReadData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void StopReadSamples(ScriptEnvironment environment)
        {
            environment.RemoveAndDispose("ReadDataControl");
            if (environment.Device is NeuroMepBase)
            {
                (environment.Device as NeuroMepBase).AmplifierPowerOff();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static bool TestAdcSignalAverage(ScriptEnvironment environment)
        {
            ReadDataControl readDataControl = environment["ReadDataControl"] as ReadDataControl;
            if (readDataControl == null)
            {
                CommonScripts.ShowError("Не удалось определить среднее значение сигнала.");
                return false;
            }
            foreach (var statistics in readDataControl.StatisticsCollection)
            {
                if (!statistics.Average.IsValidValue)
                {
                    CommonScripts.ShowError("Недопустимое среднее значение сигнала. Канал: " + statistics.ChannelName);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static bool TestAdcSignalAmplitude(ScriptEnvironment environment)
        {
            ReadDataControl readDataControl = environment["ReadDataControl"] as ReadDataControl;
            if (readDataControl == null)
            {
                CommonScripts.ShowError("Не удалось определить значение размаха.");
                return false;
            }
            foreach (var statistics in readDataControl.StatisticsCollection)
            {
                if (!statistics.Swing.IsValidValue)
                {
                    CommonScripts.ShowError("Недопустимое значение размаха. Канал: " + statistics.ChannelName);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static bool TestAdcBandWidthSignalAmplitude(ScriptEnvironment environment, double maxAmpl)
        {
            ReadDataControl readDataControl = environment["ReadDataControl"] as ReadDataControl;
            if (readDataControl == null)
            {
                CommonScripts.ShowError(Properties.Resources.CouldNotReadSwing);
                return false;
            }
            foreach (var statistics in readDataControl.StatisticsCollection)
            {
                if (Math.Abs(statistics.Average.Value - statistics.MinSignal) > maxAmpl ||
                    Math.Abs(statistics.Average.Value - statistics.MaxSignal) > maxAmpl)
                {
                    CommonScripts.ShowError("Недопустимое значение амплитуды сигнала (не должно превышать " + maxAmpl + "). Канал: " + statistics.ChannelName);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="readDataPresenterString"></param>
        /// <param name="freq"></param>
        /// <param name="maxSwing"></param>
        public static void StartTestSynPhaseNoise(ScriptEnvironment environment, string readDataPresenterString, float freq, double maxSwing = double.MaxValue, double? filterMinFreq = null, double? filterMaxFreq = null)
        {
            StandOperations.SetGeneratorState(environment, 1, freq);
            SetSynpNoiseCommutatorState(environment);
            ReadDataSettings settings = new ReadDataSettings();
            settings.SwingRange.Max = maxSwing;
            settings.MinFreqFilter = filterMinFreq;
            settings.MaxFreqFilter = filterMaxFreq;
            settings.AverageRange.Max = 200e-6d;
            settings.AverageRange.Min = -200e-6d;
            
            StartReadSamples(environment, readDataPresenterString, settings);
            NeuroMepMicroM2 device = environment.Device as NeuroMepMicroM2;
            if (device != null)
            {
                device.AmplifierSetChannelLowFreqPassBandIndex(0, 1);
                device.AmplifierSetChannelLowFreqPassBandIndex(1, 1);
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void SetSynpNoiseCommutatorState(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            /* Попытка компенсации емкости проводников */
            stand.CommutatorSetting.ChannelsState[19].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[19].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[20].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[20].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[24].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[24].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[25].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[25].TestCircuit = CommutatorTestCircuit._0_Ohm;

            stand.CommutatorSetting.ChannelsState[1].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[1].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[2].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[2].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[3].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[3].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[4].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[4].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[8].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[8].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
            /* Попытка компенсации емкости проводников */

            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.SetCommutatorState();

            stand.FloatSupplyControl.IsFloatSupplyON = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="readDataPresenterString"></param>
        public static void StartTestAdcNoise(ScriptEnvironment environment, string readDataPresenterString)
        {
            StandOperations.ConnectChannelsToGnd(environment);
            ReadDataSettings settings = new ReadDataSettings();            
            settings.MinFreqFilter = 20;
            settings.MaxFreqFilter = 10000;
            settings.RangeIndex = 0;
            settings.RMSRange.Max = 0.5e-6d;
            settings.SwingRange.Max = 5e-6d;
            settings.EnablePowerRejector = true;
            StartReadSamples(environment, readDataPresenterString, settings);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="channel">канал</param>
        /// <param name="swingValue">Размах сигнала (мкВ)</param>
        /// <param name="rmsValue">RMS сигнала (мкВ)</param>
        /// <returns></returns>
        public static bool TestAdcNoise(ScriptEnvironment environment, int channel, ref double swingValue, ref double rmsValue)
        {
            ReadDataControl readDataControl = environment["ReadDataControl"] as ReadDataControl;
            if (readDataControl == null)
            {
                CommonScripts.ShowError(Properties.Resources.CouldNotReadSwing);
                return false;
            }
            
            var statistics = readDataControl.StatisticsCollection[channel];
            swingValue = statistics.Swing.Value * 1000000;
            rmsValue = statistics.RMS.Value * 1000000;
            if (!statistics.Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха шума. Канал: " + statistics.ChannelName);
                return false;
            }
            else if (!statistics.RMS.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение RMS шума. Канал: " + statistics.ChannelName);
                return false;
            }
            else
            {
                return true;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void TestChannelsRange(ScriptEnvironment environment, string readDataPresenterString, int rangeIndex)
        {
            float signalAmpl = 0.00025f;
            switch (rangeIndex)
            {
                case 1: signalAmpl = 0.0025f; break;
                case 2: signalAmpl = 0.01f; break;
                case 3: signalAmpl = 0.025f; break;
            }                        
            StandOperations.SetGeneratorState(environment, signalAmpl, 10);
            StandOperations.SetDifferentialSignalCommutatorState(environment);
            var DefaultSettings = new ReadDataSettings() { YScaleIndex = 12 };
            if (rangeIndex == 1)
                DefaultSettings.YScaleIndex = 15;
            else if (rangeIndex == 2)
                DefaultSettings.YScaleIndex = 17;
            else if (rangeIndex == 3)
                DefaultSettings.YScaleIndex = 18;
            StartReadSamples(environment, readDataPresenterString, DefaultSettings);
            NeuroMepMicroM2 device = environment.Device as NeuroMepMicroM2;
            if (device != null)
            {
                for (int i = 0; i < NeuroMepMicroM2.AmplifierCapabilities.ChannelsCount; i++)
                {
                    device.AmplifierSetChannelRangeIndex(i, rangeIndex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        /// <param name="lowPassIndex"></param>
        /// <param name="freq"></param>
        /// <param name="minSwing"></param>
        public static void StartLowPassFilterTest(ScriptEnvironment environment, string contentPresenterName, int lowPassIndex, float freq, double minSwing)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return;            
            if (!device.Amplifier.ModuleIsOn)
                device.AmplifierModuleOn(errInfo => CommonScripts.ShowError(errInfo.Data.Descriptor.Message));
            StandOperations.SetGeneratorState(environment, 0.25e-3f, freq);
            StandOperations.SetDifferentialSignalCommutatorStateMEPMicro(environment);
            for (int i = 0; i < NeuroMepMicroM2.AmplifierCapabilities.ChannelsCount; i++)
            {
                device.AmplifierSetChannelLowFreqPassBandIndex(i, lowPassIndex);
            }

            ReadDataSettings settings = new ReadDataSettings();
            settings.SwingRange.Min = minSwing;
            double interval = 1d / freq;
            if (interval < 0.5)
            {
                interval = 0.5;
            }
            settings.TickInterval = TimeSpan.FromSeconds(interval);            
            StartReadSamples(environment, contentPresenterName, settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="channel"></param>
        /// <param name="swingValue">Размах сигнала (мВ)</param>
        /// <returns></returns>
        public static bool TestLowPassFilterSwing(ScriptEnvironment environment, int channel, ref double swingValue)
        {
            ReadDataControl readDataControl = environment["ReadDataControl"] as ReadDataControl;
            if (readDataControl == null)
            {
                CommonScripts.ShowError("Не удалось определить значение размаха.");
                return false;
            }            
            var statistics = readDataControl.StatisticsCollection[channel];
            swingValue = statistics.Swing.Value * 1000;
            if (!statistics.Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха. Канал: " + statistics.ChannelName);
                return false;
            }
            else
            {
                return true;
            }            
        }

        #region AFC

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        public static void ShowTestAFC(ScriptEnvironment environment, string contentPresenterName)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
            {
                return;
            }
            if (!device.Amplifier.ModuleIsOn)
                device.AmplifierModuleOn(errInfo => CommonScripts.ShowError(errInfo.Data.Descriptor.Message));

            StandOperations.SetMonoPolarSignalCommutatorState(environment);

            TestAFCViewModel testAFCViewModel = new TestAFCViewModel(environment, device, NeuroMepMicroM2.AmplifierCapabilities, 0, 0.0005f);
            //1 Гц
            testAFCViewModel.Rows.Add(new AFCItemsRow(0, 1, 0.95e-3d, 1.05e-3d));
            testAFCViewModel.Rows.Add(new AFCItemsRow(1, 1, 0.95e-3d, 1.05e-3d));
            //10 Гц
            testAFCViewModel.Rows.Add(new AFCItemsRow(0, 10, 0.95e-3d, 1.05e-3d));
            testAFCViewModel.Rows.Add(new AFCItemsRow(1, 10, 0.95e-3d, 1.05e-3d));
            //100 Гц
            testAFCViewModel.Rows.Add(new AFCItemsRow(0, 100, 0.95e-3d, 1.05e-3d));
            testAFCViewModel.Rows.Add(new AFCItemsRow(1, 100, 0.95e-3d, 1.05e-3d));
            //1000 Гц
            testAFCViewModel.Rows.Add(new AFCItemsRow(0, 1000, 0.95e-3d, 1.05e-3d));
            testAFCViewModel.Rows.Add(new AFCItemsRow(1, 1000, 0.95e-3d, 1.05e-3d));
            //10000 Гц
            testAFCViewModel.Rows.Add(new AFCItemsRow(0, 10000, 0.85e-3d, 1.05e-3d));
            testAFCViewModel.Rows.Add(new AFCItemsRow(1, 10000, 0.85e-3d, 1.05e-3d));
            //18000 Гц
            testAFCViewModel.Rows.Add(new AFCItemsRow(0, 18000, 0.6e-3d, 1.05e-3d));
            testAFCViewModel.Rows.Add(new AFCItemsRow(1, 18000, 0.6e-3d, 1.05e-3d));
            //20000 Гц
            testAFCViewModel.Rows.Add(new AFCItemsRow(0, 20000, 0.55e-3d, 1.05e-3d));
            testAFCViewModel.Rows.Add(new AFCItemsRow(1, 20000, 0.55e-3d, 1.05e-3d));

            TestAFCControl testAFCControl = new TestAFCControl(testAFCViewModel);

            presenter.Content = testAFCControl;
            environment.RemoveAndDispose("TestAFCControl");
            environment["TestAFCControl"] = testAFCControl;
            testAFCViewModel.StartProcess(1, 0, true);
        }

        /// <summary>
        /// Метод возвращает значение размаха для заданных частоты и канала
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="freq"></param>
        /// <param name="channel"></param>
        /// <param name="swingValue">Размаха сигнала (мВ)</param>
        /// <returns></returns>
        public static bool GetAFCSwing(ScriptEnvironment environment, float freq, int channel, ref double swingValue)
        {
            TestAFCControl testAFCControl = environment["TestAFCControl"] as TestAFCControl;
            if (testAFCControl == null)
            {
                CommonScripts.ShowError("Не удалось получить доступ к элементу чтения данных");
                return false;
            }
            AFCItemsRow row = (from r in testAFCControl.ViewModel.Rows where r.Channel == channel && r.Frequency == freq select r).FirstOrDefault();
            if (row == null)
            {
                CommonScripts.ShowError("Не удалось получить значение размаха. Канал " + (channel + 1) + ", частота " + freq + " Гц");
                return false;
            }
            swingValue = row.ResultSwing * 1000;
            return row.SuccessTest;            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void CloseTestAFC(ScriptEnvironment environment)
        {
            environment.RemoveAndDispose("TestAFCControl");
            if (environment.Device is NeuroMepBase)
            {
                (environment.Device as NeuroMepBase).AmplifierPowerOff();
            }
            StandOperations.ResetCommutatorState(environment);
        }
        #endregion


        #region Input Resistance

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        /// <param name="positiveInput"></param>
        public static void ShowTestInputResistance(ScriptEnvironment environment, string contentPresenterName, bool positiveInput = true)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
            {
                return;
            }
            if (!device.Amplifier.ModuleIsOn)
                device.AmplifierModuleOn(errInfo => CommonScripts.ShowError(errInfo.Data.Descriptor.Message));
            
            StandOperations.SetCommutatorSyncPhaseImpedance(environment, positiveInput);

            TestInputResistanceViewModel testInputResistanceViewModel = new TestInputResistanceViewModel(environment, device, NeuroMepMicroM2.AmplifierCapabilities);
            TestInputResistanceControl testInputResistanceControl = new TestInputResistanceControl(testInputResistanceViewModel);

            presenter.Content = testInputResistanceControl;
            environment.RemoveAndDispose("TestInputResistanceControl");
            environment["TestInputResistanceControl"] = testInputResistanceControl;
            testInputResistanceViewModel.StartTest();            
        }

        /// <summary>
        /// Сохраняет значения размахов сигнала при проверке входов усилителей
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="swing2Hz1"></param>
        /// <param name="swing2Hz2"></param>
        /// <param name="swing2000Hz1"></param>
        /// <param name="swing2000Hz2"></param>
        public static void SaveSwingsElectrods(ScriptEnvironment environment, ref double swing2Hz1, ref double swing2Hz2,
                                                       ref double swing2000Hz1, ref double swing2000Hz2)
        {
            TestInputResistanceControl InputImprdanceControl = environment["TestInputResistanceControl"] as TestInputResistanceControl;
            //if (InputImprdanceControl)
            swing2Hz1 = InputImprdanceControl.ViewModel.Rows[0].Swing2HzInfo.SwingValue * 1e6;
            //swing2Hz1Neg = InputImprdanceControl.ViewModel.Rows[1].Swing2HzInfo.SwingValue;
            swing2Hz2 = InputImprdanceControl.ViewModel.Rows[1].Swing2HzInfo.SwingValue * 1e6;
            //swing2Hz2Neg = InputImprdanceControl.ViewModel.Rows[3].Swing2HzInfo.SwingValue;
            swing2000Hz1 = InputImprdanceControl.ViewModel.Rows[0].Swing2000HzInfo.SwingValue * 1e6;
            //swing2000Hz1Neg = InputImprdanceControl.ViewModel.Rows[5].Swing2000HzInfo.SwingValue;
            swing2000Hz2 = InputImprdanceControl.ViewModel.Rows[1].Swing2000HzInfo.SwingValue * 1e6;
            //swing2000Hz2Neg = InputImprdanceControl.ViewModel.Rows[7].Swing2000HzInfo.SwingValue;
        }

        /// <summary>
        /// Метод, производящий проверку синфазного входного сопротивления усилителей.
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        /// <param name="isPositive">Параметр, указывающий, проверка какого входа сейчас осуществляется (true - положительного, false - отрицательного.)</param>
        /// <param name="resistanceValue1">Возвращаемое значение сопротивления для первого канала (ГОм)</param>
        /// <param name="resistanceValue2">Возвращаемое значение сопротивления для второго канала (ГОм)</param>
        /// <returns></returns>
        public static bool CheckInputResistancesRange(ScriptEnvironment environment, bool isPositive, ref double resistanceValue1, ref double resistanceValue2)
        {
            string signStr;
            TestInputResistanceControl InputImprdanceControl = environment["TestInputResistanceControl"] as TestInputResistanceControl;
            if (isPositive)
                signStr = "+";
            else
                signStr = "-";
            if (InputImprdanceControl.ViewModel.Rows[0].Resistance.Value < 1.8 * 1e9 ||
                InputImprdanceControl.ViewModel.Rows[0].Resistance.Value > 2.2 * 1e9)
            {
                CommonScripts.ShowError("Недопустимое значение входного сопротивления входа (" + 1 + signStr + ") усилителя");
                return false;
            }
            if (InputImprdanceControl.ViewModel.Rows[1].Resistance.Value < 1.8 * 1e9 ||
                InputImprdanceControl.ViewModel.Rows[1].Resistance.Value > 2.2 * 1e9)
            {
                CommonScripts.ShowError("Недопустимое значение входного сопротивления входа (" + 2 + signStr + ") усилителя");
                return false;
            }
            resistanceValue1 = InputImprdanceControl.ViewModel.Rows[0].Resistance.Value * 1e-9;
            resistanceValue2 = InputImprdanceControl.ViewModel.Rows[1].Resistance.Value * 1e-9;
            return true;
        }
        /// <summary>
        /// Метод, производящий проверку входной емкости усилителей.
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        /// <param name="isPositive">Параметр, указывающий, проверка какого входа сейчас осуществляется (true - положительного, false - отрицательного.)</param>
        /// <param name="capacityValue1">Возвращаемое значение емкости для первого канала (пФ)</param>
        /// <param name="capacityValue2">Возвращаемое значение емкости для второго канала (пФ)</param>
        /// <returns></returns>
        public static bool CheckInputCapacitiesRange(ScriptEnvironment environment, bool isPositive, ref double capacityValue1, ref double capacityValue2)
        {
            string signStr;
            TestInputResistanceControl InputImprdanceControl = environment["TestInputResistanceControl"] as TestInputResistanceControl;
            if (isPositive)
                signStr = "+";
            else
                signStr = "-";
            if (InputImprdanceControl.ViewModel.Rows[0].Capacity.Value < 13 * 1e-12 ||
                InputImprdanceControl.ViewModel.Rows[0].Capacity.Value > 22 * 1e-12)
            {
                CommonScripts.ShowError("Недопустимое значение входной емкости входа (" + 1 + signStr + ") усилителя. Внесите необходимые исправления и повторите тест.");
                return false;
            }
            if (InputImprdanceControl.ViewModel.Rows[1].Capacity.Value < 13 * 1e-12 ||
                InputImprdanceControl.ViewModel.Rows[1].Capacity.Value > 22 * 1e-12)
            {
                CommonScripts.ShowError("Недопустимое значение входной емкости входа (" + 2 + signStr + ") усилителя. Внесите необходимые исправления и повторите тест.");
                return false;
            }
            capacityValue1 = InputImprdanceControl.ViewModel.Rows[0].Capacity.Value * 1e12;
            capacityValue2 = InputImprdanceControl.ViewModel.Rows[1].Capacity.Value * 1e12;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void CloseTestInputResistance(ScriptEnvironment environment)
        {
            environment.RemoveAndDispose("TestInputResistanceControl");
            if (environment.Device is NeuroMepBase)
            {
                (environment.Device as NeuroMepBase).AmplifierPowerOff();
            }
            StandOperations.ResetCommutatorState(environment);
        }
        #endregion
        #endregion

        #region Current Stimulator
        internal const string CurrentStimulatorKey = "CurrentStimulatorSettings";
        /// <summary>
        /// Проверка возможности включить токовый стимулятор
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        /// <returns>Результат валидации</returns>
        public static ValidationResult ValidateCurrentStimulator(ScriptEnvironment environment)
        {
            NeuroMepMicroM2 device = environment.Device as NeuroMepMicroM2;
            if (device != null)
            {
                var curStimPowOnResult = true;
                if (!device.CurrentStimulator.ModuleIsOn)
                    device.CurrentStimulator.ModuleOn(errInf => curStimPowOnResult = false);
                if (curStimPowOnResult)
                {
                    device.CurrentStimulator.ModuleOff();
                    return new ValidationResult(true, null);
                }
                return new ValidationResult(false, "Не удалось включить токовый стимулятор.");
            }
            return null;
        }

        /// <summary>
        /// Запускает стимуляцию токовым стимулятором
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        /// <param name="period">Период стимуляции</param>
        /// <param name="duration">Длительность стимула</param>
        /// <param name="currentOfStimul">Ток стимула</param>
        public static void StartCurrentStimulator(ScriptEnvironment environment, string presenterStr ="", bool captureSyncImp = false, double period = 0.01, double duration = 0.0001, double currentOfStimul = 100)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            CurrentStimulatorSettings settings = new CurrentStimulatorSettings(device);
            settings.Period = period;
            settings.Duration = duration;
            settings.CurrentValue = currentOfStimul;
            settings.StimulationMode = CurrentStimulatorExtModeEnum.Normal;
            var presenter = environment.GetPresenter(presenterStr);
            if (presenter != null)
            {
                presenter.Content = settings;
            }
            environment[CurrentStimulatorKey] = settings;
            settings.StartStimulation(captureSyncImp);
        }

        /// <summary>
        /// Проверяет наличие синхроимпульсов от стимулятора
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static bool? CheckCurrStimSyncImp(ScriptEnvironment environment)
        {
            StimulatorSettings<ICurrentStimulatorExt> stimSettings = environment[CurrentStimulatorKey] as StimulatorSettings<ICurrentStimulatorExt>;
            if (stimSettings != null)
            {
                if (stimSettings.StimulsCaptured)
                    return true;
                else
                {
                    CommonScripts.ShowError(Properties.Resources.SyncStimsNotCaptured);
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Останавливает стимуляцию токовым стимулятором
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        public static void StopCurrentStimulator(ScriptEnvironment environment)
        {
            environment.RemoveAndDispose(CurrentStimulatorKey);
        }

        /// <summary>
        /// Отображает контрол вилочкового токового стимулятора
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        /// <param name="contentPresenterName">Имя контент презентера</param>
        public static void ShowForkStimulator(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.CurrentStimulator.ModuleIsOn)
                device.CurrentStimulator.ModuleOn(errInf => CommonScripts.ShowError(errInf.Data.Descriptor.Message));
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            ForkStimulatorControl ForkCurStimControl = new ForkStimulatorControl(device);
            presenter.Content = ForkCurStimControl;            
            environment["ForkStimulatorControl"] = ForkCurStimControl;
        }

        public static bool? CheckForkCurrentStimulator(ScriptEnvironment environment)
        {
            ForkStimulatorControl forkStimulatorControl = environment["ForkStimulatorControl"] as ForkStimulatorControl;
            foreach (bool checkButtonPush in forkStimulatorControl.ButtonClikcs)
            {
                if (!checkButtonPush)
                {
                    CommonScripts.ShowError("Получены не все уведомления о нажатии кнопок вилочкового стимулятора. Выполните тест полностью либо устраните возможные неисправности и повторите проверку.");
                    return null;
                }
            }
            return true;
        }

        /// <summary>
        /// Выключение вилочкового стимулятора
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        public static void CloseForkStimulator(ScriptEnvironment environment)
        {            
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            device.CurrentStimulator.ModuleOff();
            environment.RemoveAndDispose("ForkStimulatorControl");
        }

        #region Программирование контроллера
        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        public static void ShowFlashUtilProgrammator(ScriptEnvironment environment, string contentPresenterName, string fwFileName = "C8051F330_r*_d*.hex", string fwFolderPath = null)
        {       
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            FlashUtilControl flashUtilControl = new FlashUtilControl(environment, fwFileName);
            if (fwFolderPath == null)
                fwFolderPath = FirmwareFolder;
            else
                fwFolderPath = Directory.Exists(fwFolderPath) ? fwFolderPath : @"C:\";
            flashUtilControl.DefaultFirmwareFolder = fwFolderPath;
            presenter.Content = flashUtilControl;
            environment["FlashUtilControl"] = flashUtilControl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="fileName"></param>
        /// <param name="progDate"></param>
        /// <returns></returns>
        public static bool? SaveFlashUtilFileName(ScriptEnvironment environment, ref string fileName)
        {
            if (environment["FlashUtilFileName"] != null)
            {
                fileName = environment["FlashUtilFileName"] as string;                
                return true;
            }
            CommonScripts.ShowError("Контроллер не был запрограммирован. Выполните программирование либо устраните ошибку.");
            return null;
        }

        #endregion

        #endregion

        #region Indicator and KeyBoard

        #region Programming MSP430
        /// <summary>
        /// Метод программирования контроллера платы индикации
        /// </summary>
        /// <returns>Имя файла прошивки</returns>
        public static string ProgramMsp430(string fwNameTemp = "MSP430F149_r*_d*.hex")
        {            
            string fileNameMsp430;
            string executingFolder = "";
            try
            {
                executingFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
            }
            catch { }
            string processFileName = executingFolder + @"MspFetCon.exe";
            string fileNameTemplate = fwNameTemp;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "hex|" + fileNameTemplate;
            openFileDialog.InitialDirectory = FirmwareFolder;
            openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate);
            if (openFileDialog.ShowDialog() == true)
                fileNameMsp430 = Encoding.GetEncoding("cp866").GetString(Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding("cp866"), Encoding.Unicode.GetBytes(openFileDialog.FileName)));
            else
                return null;
            string args = "+ebpv " + @"""" + fileNameMsp430 + @"""" + " -RAW=LPT1";
            StringBuilder log = new StringBuilder();
            log.AppendLine("Trying to program with args: " + args);
            int exitCode = ProgramMsp430(processFileName, args);
            log.AppendLine("Programming is finished. ExitCode: " + exitCode);
            if (exitCode == 4) //Возможно, надо установить другие параметры
            {
                args = "+ebpv " + @"""" + fileNameMsp430 + @"""" + " -FET=LPT1";
                log.AppendLine("Trying to program with args: " + args);
                exitCode = ProgramMsp430(processFileName, args);
                log.AppendLine("Programming is finished. ExitCode: " + exitCode);
            }
            if (exitCode != TimeoutMsp430Code)
            {
                if (exitCode != 0)
                {
                    CommonScripts.ShowError("Контроллер платы индикации не был запрограммирован, так как программа вернула код ошибки " + exitCode + ".\r\n" + log.ToString());
                    return null;
                }
                else
                {
                    MessageBox.Show("Программирование контроллера платы индакации завершилось успешно.", "Сообщение",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return GetShortFileName(fileNameMsp430);
                }
            }
            else
            {
                CommonScripts.ShowError("Не удалось запрограммировать контроллер платы индакации. Истекло время ожидания.");
                return null;               
            }
        }

        private const int TimeoutMsp430Code = -123;
        private static int ProgramMsp430(string processFileName, string args)
        {
            Process ProgrammingMsp430 = Process.Start(processFileName, args);
            try
            {
                if (ProgrammingMsp430.WaitForExit(60000))
                {                    
                    return ProgrammingMsp430.ExitCode;
                }
                else
                {
                    return TimeoutMsp430Code;
                }
            }
            finally
            {
                ProgrammingMsp430.Close();
            }
        }
        /// <summary>
        /// Метод, возвращающий имя файла прошивки контроллера платы индикации при ручном программировании
        /// </summary>
        /// <returns>Имя файла прошивки</returns>
        public static string ManualProgramMsp430(string fileNameTmp = "MSP430F149_r*_d*.hex", string fwFolderPath = null)
        {
            string fileNameTemplate = fileNameTmp;
            if (fwFolderPath == null)
                fwFolderPath = FirmwareFolder;
            else
                fwFolderPath = Directory.Exists(fwFolderPath) ? fwFolderPath : @"C:\";

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "hex|" + fileNameTemplate;
            openFileDialog.InitialDirectory = fwFolderPath;
            openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate);
            if (openFileDialog.ShowDialog() != true)
                return null;
            else
                return GetShortFileName(openFileDialog.FileName);
        }

        #endregion

        #region Keyboard
        /// <summary>
        /// Отображает контрол для проверки клавиатуры
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        /// <param name="contentPresenterName">Имя контент презентера</param>
        public static void ShowKeyboardControl(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMepMicroM2 device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            NeuroMEPKeyboardControl keyboardControl = new NeuroMEPKeyboardControl(device);
            presenter.Content = keyboardControl;            
            environment["KeyboardControl"] = keyboardControl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static bool CheckKeyboardButtons(ScriptEnvironment environment)
        {
            NeuroMEPKeyboardControl keyboardControl = environment["KeyboardControl"] as NeuroMEPKeyboardControl;
            if (keyboardControl == null || !keyboardControl.AllButtonsPressed)
            {
                CommonScripts.ShowError("Не все клавишы были нажаты.");
                return false;
            }
            return true;
        }        
       
        #endregion

        #region Display

        /// <summary>
        /// Отображает контрол для проверки дисплея
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        /// <param name="contentPresenterName">Имя контент презентера</param>
        public static void ShowDisplayControl(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMepBase device = OpenDevice(environment);
            if (device == null)
                return;            
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            DisplayControl displayControl = new DisplayControl(device);
            presenter.Content = displayControl;            
            environment["DisplayControl"] = displayControl;
        }

        /// <summary>
        /// Производит проверку, все ли параметры дисплея были проверены
        /// </summary>
        /// <param name="environment"> Переменная окруженя скрипта </param>
        /// <param name="settingsSaved"> Признак, что установки яркости и контрастности были сохранены </param>
        /// <returns></returns>
        public static bool CheckDisplayControl(ScriptEnvironment environment, out bool settingsSaved)
        {
            DisplayControl displayControl = environment["DisplayControl"] as DisplayControl;
            if (displayControl == null || !displayControl.AllChecked)
            {
                CommonScripts.ShowError(Properties.Resources.NotAllDisplayCheckMade);
                settingsSaved = false;
                return false;
            }
            if (displayControl == null || !displayControl.SettingsSaved)
            {
                CommonScripts.ShowError(Properties.Resources.DisplaySettingsNotSaved);
                settingsSaved = false;
                return false;
            }
            settingsSaved = true;
            return true;
        }
        #endregion

        #endregion

        #region Проверка при повышенном и пониженном напряжении питания

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="voltageIsHigh"></param>
        /// <returns></returns>
        public static bool? CheckLowHighSupplyVoltage(ScriptEnvironment environment, bool voltageIsHigh = true)
        {
            var device = OpenDevice(environment);
            if (device == null)
                return null;
            environment.OpenStand();
            UniversalTestStand Stand = environment.Stand;
            if (voltageIsHigh)
                Stand.USBVoltageChanger.UsbSupplyVoltage = USBSupplyVoltage.HIGH;
            else
                Stand.USBVoltageChanger.UsbSupplyVoltage = USBSupplyVoltage.LOW;
            if (!DebugSetControlWord(environment, null, null, false, false, false, true, null))
            {
                CommonScripts.ShowError("Не удалось включить пять вольт.");
                return null;
            }
            if (!DebugSetControlWord(environment, null, null, true, true, true, true, null))
            {
                CommonScripts.ShowError("Не удалось включить блок фото-фоно стимулятора.");
                return null;
            }
            if (!DebugSetControlWord(environment, null, null, true, false, false, true, null))
            {
                CommonScripts.ShowError("Не удалось включить блок усилителей.");
                return null;
            }
            if (!DebugSetControlWord(environment, null, null, true, true, false, true, null))
            {
                CommonScripts.ShowError("Не удалось включить блок токового стимулятора.");
                return null;
            }
            //if (!DebugSetControlWord(environment, null, null, true, true, true, true, null))
            //{
            //    CommonScripts.ShowError("Не удалось включить блок фото-фоно стимулятора.");
            //    return null;
            //}
            //Stand.USBVoltageChanger.UsbSupplyVoltage = USBSupplyVoltage.NORMAL;
            //DebugSetControlWord(environment, null, null, false, false, false, false, null);
            string voltageString = voltageIsHigh ? "повышенном" : "пониженном";
            MessageBox.Show("Тест проверки при " + voltageString + " напряжении питания пройден успешно.", "Сообщение", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void StopCheckLowHighSupplyVoltage(ScriptEnvironment environment)
        {
            UniversalTestStand Stand = environment.Stand;
            if (Stand.DeviceOpened)
                Stand.USBVoltageChanger.UsbSupplyVoltage = USBSupplyVoltage.NORMAL;
            DebugSetControlWord(environment, null, null, false, false, false, false, null);
        }

        #endregion

        #region Common

        ///// <summary>
        ///// Сообщение об ошибке
        ///// </summary>
        ///// <param name="errorContent"></param>
        //public static void ShowError(object errorContent)
        //{
        //    NSMessageBox.Show(Convert.ToString(errorContent), Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        //}


        ///// <summary>
        ///// Сообщение об ошибке
        ///// </summary>
        ///// <param name="errorMsg"></param>
        //public static void ShowError(string errorMsg)
        //{
        //    NSMessageBox.Show(errorMsg, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        //}
        #endregion

        #region Stand Operations

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        

        #endregion
    }  

}
