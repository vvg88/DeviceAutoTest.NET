using System;
using System.Collections.Generic;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Controls;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.Hardware.Tools.Usb;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.Hardware.Devices;
using System.IO;
using NeuroSoft.MathLib.Filters;
using System.Runtime.InteropServices;
using NeuroSoft.Hardware.Tools.Cypress;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts
{
    public class EEG5Scripts
    {
        #region Открытие и валидация устройств

        /// <summary>
        /// Открывает устройство
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static EEG5Device OpenDevice(ScriptEnvironment environment)
        {
            EEG5Device device = environment.Device as EEG5Device;
            if (device == null)
            {
                device = new EEG5Device(environment.DeviceSerialNumber);
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
        /// <param name="details"></param>
        /// <returns></returns>
        public static ValidationResult ValidateDeviceOpening(ScriptEnvironment environment)
        {
            EEG5Device device = OpenDevice(environment);
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
        /// <returns> Ссылка на устройство </returns>
        private static EEG5Device GetDevice(ScriptEnvironment environment)
        {
            EEG5Device device = OpenDevice(environment);
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

        #region Программирование контроллеров

        #region Программирование PIC'ов

        /// <summary>
        /// Сохраняет имя файла прошивки PIC контроллера
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="fileNameTemplate"> Часть имени нужного файла прошивки </param>
        /// <param name="fileName">Имя файла, с помощью которого осущствлялось программирование </param>
        /// <param name="fwFolder"> Путь к папке с прошивкой </param>
        /// <returns></returns>
        public static bool? ProgramChipProg(ScriptEnvironment environment, string fileNameTemplate, ref string fileName, string fwFolder = "")
        {
            var fwFoldr = string.IsNullOrEmpty(fwFolder) ? FirmwareFolder : Directory.Exists(fwFolder) ? fwFolder : @"C:\";
            return CommonScripts.ProgramChipProg(environment, fileNameTemplate, fwFoldr, ref fileName);
        }

        #endregion

        #region Программирование Silicon Lab

        /// <summary>
        /// Имя контент презентера контрола программирования контроллеров Silicon Labs
        /// </summary>
        private static string FlashUtilPresenterName;
        /// <summary>
        /// Создает контрол программирования контроллеров Silicon Labs
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="fileNameTemplate"> Имя файла прошивки </param>
        public static void ShowFlashUtilProgrammator(ScriptEnvironment environment, string contentPresenterName, string fileNameTemplate, string fwFolder = "")
        {
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            FlashUtilControl flashUtilControl = new FlashUtilControl(environment, fileNameTemplate + "*.hex");
            flashUtilControl.DefaultFirmwareFolder = string.IsNullOrEmpty(fwFolder) ? FirmwareFolder : Directory.Exists(fwFolder) ? fwFolder : @"C:\";
            presenter.Content = flashUtilControl;
            FlashUtilPresenterName = contentPresenterName;
        }

        /// <summary>
        /// Сохраняет имя файла прошивки
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="fileName"> Имя файла прошивки </param>
        /// <returns> Результат программирования и сохранения файла </returns>
        public static bool? SaveFlashUtilFileName(ScriptEnvironment environment, ref string fileName)
        {
            if (environment["FlashUtilFileName"] != null)
            {
                fileName = environment["FlashUtilFileName"] as string;
                environment.RemoveAndDispose(FlashUtilPresenterName);
                environment.RemoveAndDispose("FlashUtilFileName");
                return true;
            }
            CommonScripts.ShowError("Контроллер не был запрограммирован. Выполните программирование либо устраните ошибку.");
            return null;
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
        public static bool? DoFirmwareEeprom(ScriptEnvironment environment, HeadAndAmplifType headAmpType, ref string fileName, bool isNS5 = true, string fwFolder = "")
        {
            SelectDeviceDialog selectDeviceDialog = new SelectDeviceDialog();
            if (selectDeviceDialog.ShowDialog() == true)
            {
                string fileNameTemplate = "НС-5_ver*.iic";
                var defAultFwFolder = @"\\SERVER\firmware\_Common\Прошивки для приборов переделанных с CY7C64613-80 на CY7C68013A-100AXI\";
                string firmwareFolder = string.IsNullOrEmpty(fwFolder) ? defAultFwFolder : Directory.Exists(fwFolder) ? fwFolder : @"C:\";
                var usbItem = selectDeviceDialog.SelectedDeviceItem.Device;
                var weavingInformation = new ProgramDescriptor();
                weavingInformation.SerialNumberBuilder = new SerialNumberBuilderNumeric(SerialNumberBuilderNumeric.NumericType.B2, false);
                weavingInformation.Address = 0x80C;
                weavingInformation.PID_Enable = true;
                weavingInformation.PID_Addr = 0x80A;
                weavingInformation.PID_Value = (ushort)(isNS5 ? 0x8270 : 0x8271);
                weavingInformation.AddData_Addr = 0x812;
                weavingInformation.CypressCpuType = Hardware.Tools.Cypress.CypressTools.CypressCpuType.FX1_FX2LP;
                switch (headAmpType)
                {
                    case HeadAndAmplifType.A1rightINA118:
                        weavingInformation.AddData_Value = new byte[] { 0, 0, 0, 0 };
                        break;
                    case HeadAndAmplifType.A1leftINA118:
                        weavingInformation.AddData_Value = new byte[] { 1, 0, 0, 0 };
                        break;
                    case HeadAndAmplifType.A1rightLT1168:
                        weavingInformation.AddData_Value = new byte[] { 2, 0, 0, 0 };
                        break;
                    case HeadAndAmplifType.A1leftLT1168:
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

        /// <summary>
        /// Перечисление с указанием ориентации головы на передней панели и типа усилителей в каналах ЭЭГ
        /// </summary>
        public enum HeadAndAmplifType { A1rightINA118, A1leftINA118, A1rightLT1168, A1leftLT1168 }

        #endregion

        /// <summary>
        /// Папка с прошивками
        /// </summary>
        internal static string FirmwareFolder
        {
            get
            {
                string fwFolder = @"\\server\firmware\НС5\HEX";
                return Directory.Exists(fwFolder) ? fwFolder : @"C:\";
            }
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
        public static void ShowDeviceInformation(ScriptEnvironment environment, string contentPresenterName, bool isSpO2Testing)
        {
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
                presenter = new ContentPresenter();
            DeviceInformationControl deviceInfoControl = new DeviceInformationControl(environment, isSpO2Testing);
            // Если запущена проверка датчика сатурации кислорода, но он не установлен в приборе, то контрол выводить не будем, а контентпрезентер удалим
            IsSpO2Available = deviceInfoControl.IsSpO2Available;
            isNs4epm = (device.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP;
            if (isSpO2Testing && !IsSpO2Available)
                environment.RemoveAndDispose(contentPresenterName);
            else
            {
                presenter.Content = deviceInfoControl;
                deviceInfoControlName = contentPresenterName;
            }
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

        private static bool isNs4epm;

        public static bool IsNs4epm
        {
            get { return isNs4epm; }
        }

        #endregion

        #region Проверка датчика SpO2

        //private static string SpO2ControlName;
        /// <summary>
        /// Флаг установки модуля SpO2
        /// </summary>
        public static bool IsSpO2Available;
        /// <summary>
        /// Отображает контрол с информацией о девайсе в режиме считывания информации с SpO2
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"></param>
        //public static void ShowSpO2Control(ScriptEnvironment environment, string contentPresenterName)
        //{
        //    EEG5Device device = GetDevice(environment);
        //    if (device == null)
        //        return;
        //    ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
        //    DeviceInformationControl spo2InfoControl = new DeviceInformationControl(environment, true);
        //    presenter.Content = spo2InfoControl;
        //    SpO2ControlName = contentPresenterName;
        //}

        #endregion

        #region Проверка напряжений питания и токов потребелния устройства

        /// <summary>
        /// Запускает режим калибровки при проверке напряжений и токов потребления
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StartChecking(ScriptEnvironment environment)
        {
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            device.BeginKALIBROVKA();
        }

        /// <summary>
        /// Останавливает режим калибровки при проверке напряжений и токов потребления
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopChecking(ScriptEnvironment environment)
        {
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            device.StopTransmit();
        }

        #endregion

        #region Проверка индикации и клавиатуры

        #region Проверка индикации

        private static string IndicationControlName;

        /// <summary>
        /// Отображает контрол индикации
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя презентера </param>
        public static void ShowIndicationControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            IndicationEEG5Control ledsControl = new IndicationEEG5Control(device);
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
            bool? result = ((environment[IndicationControlName] as ContentPresenter).Content as IndicationEEG5Control).AllChecked;
            if (result != true)
            {
                CommonScripts.ShowError(Properties.Resources.IndicationErrorString);
                result = null;
            }
            return result;
        }

        #endregion

        #region Проверка клавиатуры

        /// <summary>
        /// Сохраненное имя контент презентера контрола с кнопками
        /// </summary>
        private static string KeysControlName;

        /// <summary>
        /// Отображает контрол с кнопками
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowKeyBoardControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            EEG5KeysControl keysControl = new EEG5KeysControl(device);
            presenter.Content = keysControl;
            KeysControlName = contentPresenterName;
        }

        /// <summary>
        /// Проверяет, все ли кнопки были нажаты
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат проверки (null - не все кнопки были нажаты, true - все были нажаты) </returns>
        public static bool? TestKeys(ScriptEnvironment environment)
        {
            if (KeysControlName == null)
                return null;
            bool? result = ((environment[KeysControlName] as ContentPresenter).Content as EEG5KeysControl).AllButtonsChecked;
            if (result != true)
            {
                CommonScripts.ShowError(Properties.Resources.KeyBoardErrorString);
                result = null;
            }
            return result;
        }

        #endregion

        #endregion

        #region Проверка измерения электродных импедансов

        private static string ImpedanceControlName;
        /// <summary>
        /// Отображает контрол измерения импеданса для ЭЭГ или ВП режима 
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isEEGImpedance"> Режим отображения импеданса (ЭЭГ или ВП) </param>
        public static void ShowImpedancesControl(ScriptEnvironment environment, string contentPresenterName, bool isEEGImpedance)
        {
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            if (isEEGImpedance)
                StandOperationsEEG.ConnectChannelsTo10K(environment);
            else
                StandOperationsEEG.ConnectVpChannelsTo10K(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            NSDevicesImpedanceControl impedancesControl = new NSDevicesImpedanceControl(device, new RangedValue<double>(10e3, 9e3, 11e3), isEEGImpedance);
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
                bool result = impedancesControl.ValidImpedances;
                (environment.Device as EEG5Device).StopTransmitImpedance();
                if (result)
                    return true;
                return null;
            }
            return null;
        }
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
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            EEG5SocketsControl checkSocketsControl = new EEG5SocketsControl(device, new RangedValue<double>(0, -1.0, (device.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP ? 350 : 350)); // 24.12.2015 приходил Герман и Роман Панков с заявлением, что при проверке НС5 показывается значение 0,3 кОм и тест не проходит. Попросили увеличить порог. Увеличиваю с 250 Ом до 350 Ом
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
            EEG5SocketsControl socketsContr = controlPresenter.Content as EEG5SocketsControl;
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
        /// <summary>
        /// Выводит контрол проверки распайки разъемов DIN
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowDINSolderingControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
                return;
            NSDeviceSolderingControl eegDinSoldControl = new NSDeviceSolderingControl(environment, device, 0.0, 300.0);
            eegDinSoldControl.StartTestSoldering += new StartTestSolderingEventHandler(eegDinSoldControl_StartTestSoldering);
            presenter.Content = eegDinSoldControl;
            eegDinSoldControl.StartTesting();
            checkSocketsControlName = contentPresenterName;
        }

        /// <summary>
        /// Обработчик события переключения текущего проверяемого электрода
        /// </summary>
        /// <param name="sender"> Источник </param>
        /// <param name="electrode"> Номер проверяемого электрода </param>
        private static void eegDinSoldControl_StartTestSoldering(object sender, int electrode)
        {
            var electrImpControl = sender as NSDeviceSolderingControl;
            if (electrImpControl == null)
                return;
            var environment = electrImpControl.Environment;
            var testedDevice = electrImpControl.Device;
            StandOperations.ChangeCommutatorStateInImpedanceTest(environment, testedDevice, electrode);
        }

        /// <summary>
        /// Проверяет успешность теста проверки распайки разъемов DIN
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат проверки </returns>
        public static bool? CheckDinSoldering(ScriptEnvironment environment)
        {
            NSDeviceSolderingControl soldControl = (environment[checkSocketsControlName] as ContentPresenter).Content as NSDeviceSolderingControl;
            if (soldControl == null)
                return null;
            if (!soldControl.NSDevicesImpedancesControl.ValidImpedances)
            {
                return null;
            }
            return true;
        }

        #endregion

        #region Проверка измерения поляризации электродов

        private static string PolarisationsControlName;
        /// <summary>
        /// Отображает контрол измерения импеданса для ЭЭГ или ВП режима 
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowPolarisationsControl(ScriptEnvironment environment, string contentPresenterName)
        {
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            //if (isEEGImpedance)
                StandOperationsEEG.ConnectChannelsTo10K(environment);
            //else
                //StandOperationsEEG.ConnectVpChannelsTo10K(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            EEGPolarisationControl polarisationsControl = new EEGPolarisationControl(device, new RangedValue<double>(5, 0, 10));
            presenter.Content = polarisationsControl;
            //polarisationsControl.Start();
            PolarisationsControlName = contentPresenterName;
        }
        /// <summary>
        /// Проверяет на валидность сохраняемые значния импеданса
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Результат проверки </returns>
        public static bool? TestPolarisationsValues(ScriptEnvironment environment)
        {
            if (PolarisationsControlName == null)
                return null;
            EEGPolarisationControl polarisationsControl = (environment[PolarisationsControlName] as ContentPresenter).Content as EEGPolarisationControl;
            if (polarisationsControl != null)
            {
                polarisationsControl.Stop();
                bool result = polarisationsControl.ValidPolarisations && polarisationsControl.ValidPolarisDiff;
                (environment.Device as EEG5Device).StopTransmitImpedance();
                if (result)
                    return true;
                return null;
            }
            return null;
        }
        /// <summary>
        /// Останавливает чтение импеданса
        /// </summary>
        /// <param name="environment"></param>
        public static void StopImpPolarMeasure(ScriptEnvironment environment)
        {
            if (PolarisationsControlName != null && environment[PolarisationsControlName] != null)
            {
                EEGPolarisationControl polarisationsControl = (environment[PolarisationsControlName] as ContentPresenter).Content as EEGPolarisationControl;
                if (polarisationsControl != null)
                    polarisationsControl.Stop();
            }
            if (ImpedanceControlName != null && environment[ImpedanceControlName] != null)
            {
                NSDevicesImpedanceControl impedancesControl = (environment[ImpedanceControlName] as ContentPresenter).Content as NSDevicesImpedanceControl;
                if (impedancesControl != null)
                    impedancesControl.Stop();
            }
        }

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
        private static void StartReadData(ScriptEnvironment environment, string contentPresenterName, EEGTestingMode workMode, ReadDataSettings readDataControlSettings = null)
        {
            EEG5Device device = GetDevice(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
                presenter = new ContentPresenter();
            ReadDataViewModelEEG5 viewModelEEG = new ReadDataViewModelEEG5(environment, device, workMode);
            ReadDataControl readDataControl = new ReadDataControl(viewModelEEG);
            presenter.Content = readDataControl;
            //if (workMode == EEGTestingMode.DCTransmit)
            //{
            //    EEG5AmplifierState amplifState = device.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState;
            //    if (amplifState != null)
            //    {
            //        amplifState.UsingChannels[34] = 0;
            //        //amplifState.UsingChannels[32] = 0;
            //        //amplifState.UsingChannels[33] = 0;
            //        device.SetDeviceState(amplifState);
            //    }
            //}
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
            // Из-за этого амплитуда сигнала калибровки каналов ВП была завышена
            if (environment.Device is EEG5Device && workMode == EEGTestingMode.Calibration)
            {
                for (int i = 44; i < 48; i++)
                    foreach (var filter in (environment.Device as EEG5Device).Filters[i])
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

        #region Проверка сигнала калибровки
        /// <summary>
        /// Представляет контрол, отображающий сигнал калибровки
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowEegCalibration(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings calibrationSettings = new ReadDataSettings();
            calibrationSettings.SwingRange.Min = 0.9e-3;
            calibrationSettings.SwingRange.Max = 1.3e-3;
            calibrationSettings.MaxFreqFilter = 35.0;
            calibrationSettings.MinFreqFilter = 0.05;
            calibrationSettings.XScale = new ScaleItem(20e-3f, "20 мс/мм");
            calibrationSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");
            StartReadData(environment, contentPresenterName, EEGTestingMode.Calibration, calibrationSettings);
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
                if (Math.Abs(dataStat.Swing.Value - bestValue) > Math.Abs(worstValue - bestValue))
                    worstValue = dataStat.Swing.Value;
            }
            worstValue *= 1e3;
            return true;
        }
        #endregion
        
        #region Проверка каналов ЭЭГ

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
                eegNoiseSettings.MinFreqFilter = 0.05; // Измененно с 0,5 Гц на 0,05 Гц из-за разбора №292 от 23.07.2013.
                eegNoiseSettings.MaxFreqFilter = 35.0;
                eegNoiseSettings.SwingRange.Min = 0.0;
                eegNoiseSettings.SwingRange.Max = 1e-6;
                eegNoiseSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
                eegNoiseSettings.YScale = new ScaleItem(200e-9f, "200 нВ/мм");
            }
            StandOperationsEEG.ConnectEegChannelsToGnd(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGNoiseTransmit, eegNoiseSettings);
        }

        /// <summary>
        /// Проверяет размахи сигнала шума в каналах ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Успешность проверки </returns>
        public static bool? TestNoiseSwings(ScriptEnvironment environment)
        {
            ReadDataControl readDataContr = GetReadDataControl(environment);
            if (readDataContr == null)
                return null;
            StopDataReading(environment);
            foreach (DataStatistics dataStat in readDataContr.ViewModel.StatisticsCollection)
            {
                if (!dataStat.Swing.IsValidValue)
                {
                    CommonScripts.ShowError(String.Format(Properties.Resource1.NoiseSwingsInvalidMessage, dataStat.ChannelName));
                    return null;
                }
            }
            return true;
        }

        #endregion

        #region Проверка коэффициентов усиления каналов ЭЭГ

        /// <summary>
        /// Запускает проверку коэффициентов усиления каналов ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartEegGainsTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings eegGainsSettings = new ReadDataSettings();
            eegGainsSettings.MinFreqFilter = 0.05;
            eegGainsSettings.MaxFreqFilter = 35.0;
            eegGainsSettings.SwingRange.Max = 1.04e-3;
            eegGainsSettings.SwingRange.Min = 0.96e-3;
            eegGainsSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            eegGainsSettings.YScale = new ScaleItem(100e-6f, "100 мкВ/мм");
            StandOperations.SetGeneratorState(environment, 0.25e-3f, 10.0f);
            StandOperationsEEG.SetEegDiffSignal(environment);
            //StandOperationsEEG.ConnectEegChannelsToGnd(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransmit, eegGainsSettings);
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
            savedSwings = eegSwings.ToArray();
            worstSwing *= (ismVscale ? 1e3 : 1e6);
            return true;
        }

        #endregion

        #region Проверка изменения референтного электрода

        /// <summary>
        /// Запускает проверку отключения REF
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartTestOfReferentChange(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings refChangeSettings = new ReadDataSettings();
            refChangeSettings.SwingRange.Max = 1.04e-3;
            refChangeSettings.SwingRange.Min = 0.96e-3;
            refChangeSettings.MinFreqFilter = 0.05;
            refChangeSettings.MaxFreqFilter = 1e3;
            refChangeSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            refChangeSettings.YScale = new ScaleItem(100e-6f, "100 мкВ/мм");
            StandOperations.SetGeneratorState(environment, 0.25e-3f, 10.0f);
            StandOperationsEEG.SetEegDiffSignalRefA1(environment, (environment.Device as EEG5Device).MirroredPanel);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransRefA1, refChangeSettings);
        }
        /// <summary>
        /// Проверяет размахи сигнала в тесте проверки отключения опорного электрода REF
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Успешность проверки </returns>
        public static bool? TestSwingsOfChangeReferent(ScriptEnvironment environment)
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
            }
            return true;
        }

        #endregion

        #region Проверка фильтров каналов ЭЭГ

        /// <summary>
        /// Запускает проверку аппаратных фильтров ФНЧ 150 Гц
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartEEGLowFreqTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings eegLowFreqSettings = new ReadDataSettings();
            eegLowFreqSettings.SwingRange.Min = 6.65e-3;
            eegLowFreqSettings.SwingRange.Max = 7.35e-3;
            eegLowFreqSettings.MinFreqFilter = 0.05;
            eegLowFreqSettings.MaxFreqFilter = 1e3;
            eegLowFreqSettings.XScale = new ScaleItem(1e-3f, "1 мс/мм");
            eegLowFreqSettings.YScale = new ScaleItem(1e-3f, "1 мВ/мм");
            StandOperations.SetGeneratorState(environment, 2.5e-3f, 150.0f);
            StandOperationsEEG.SetEegDiffSignal(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransmit, eegLowFreqSettings);
        }

        /// <summary>
        /// Запускает проверку аппаратных фильтров ФВЧ 0.5 Гц
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartEEGHighFreqTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings eegHighFreqSettings = new ReadDataSettings();
            eegHighFreqSettings.SwingRange.Min = 6.65e-3;
            eegHighFreqSettings.SwingRange.Max = 7.35e-3;
            //eegLowFreqSettings.MinFreqFilter = 0.05;
            eegHighFreqSettings.MaxFreqFilter = 35;
            eegHighFreqSettings.TickIntervalsProcessing = 4;
            eegHighFreqSettings.XScale = new ScaleItem(100e-3f, "0,1 с/мм");
            eegHighFreqSettings.YScale = new ScaleItem(1e-3f, "1 мВ/мм");
            StandOperations.SetGeneratorState(environment, 2.5e-3f, 0.5f);
            StandOperationsEEG.SetEegDiffSignal(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransmit, eegHighFreqSettings);
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
            //synphNoiseSettings.MaxFreqFilter = 20.0;
            synphNoiseSettings.XScale = new ScaleItem(5e-3f, "5 мс/мм");
            synphNoiseSettings.YScale = new ScaleItem(1e-6f, "1 мкВ/мм");
            StandOperations.SetGeneratorState(environment, 0.5f, 50.0f);
            StandOperationsEEG.SetSynphaseEegSignal(environment);
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
            eegImpImpedanceSettings.YScale = new ScaleItem(100e-6f, "100 мкВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, 100.0f);
            StandOperationsEEG.SetEegSynphForImpedance(environment, isRefTested);
            StartReadData(environment, contentPresenterName, EEGTestingMode.EEGTransmit, eegImpImpedanceSettings);
        }

        #endregion

        #endregion

        #region Проверка каналов ВП

        #region Проверка уровня шума

        /// <summary>
        /// Запускает проверку шума каналов ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="is100mvRange"> Флаг, указывающий, в каком режиме осуществляется проверка </param>
        public static void StartVpNoiseTest(ScriptEnvironment environment, string contentPresenterName, ModesOfTestNoise testingMode)
        {
            ReadDataSettings vpNoiseSettings = new ReadDataSettings();
            switch (testingMode)
            {
                case ModesOfTestNoise.Range100mV:
                    vpNoiseSettings.SwingRange.Max = 30e-6;
                    vpNoiseSettings.MinFreqFilter = 1;
                    vpNoiseSettings.MaxFreqFilter = 10000.0;
                    vpNoiseSettings.RangeIndex = 8;
                    vpNoiseSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
                    vpNoiseSettings.YScale = new ScaleItem(2e-6f, "2 мкВ/мм");
                    break;
                case ModesOfTestNoise.Range200mkV:
                    vpNoiseSettings.SwingRange.Max = 5e-6;//12e-6; Значение допустимого шума снизилось до 5 мкВ в соответствии с требованиями ТУ
                    vpNoiseSettings.MinFreqFilter = 1;
                    vpNoiseSettings.MaxFreqFilter = 10000.0;
                    vpNoiseSettings.RangeIndex = 0;
                    vpNoiseSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
                    vpNoiseSettings.YScale = new ScaleItem(1e-6f, "1 мкВ/мм");
                    break;
                case ModesOfTestNoise.IsoLineAt005Hz:
                    vpNoiseSettings.SwingRange.Max = 2e-6;
                    vpNoiseSettings.MinFreqFilter = 0.05;//0.212;
                    vpNoiseSettings.MaxFreqFilter = 1e4;//250.0;
                    vpNoiseSettings.RangeIndex = 0;
                    vpNoiseSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
                    vpNoiseSettings.YScale = new ScaleItem(200e-9f, "200 нВ/мм");
                    break;
            }
            vpNoiseSettings.SwingRange.Min = 0.0;
            StandOperationsEEG.ConnectVpChannelsToGnd(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.VPTransmit, vpNoiseSettings);

            if (testingMode == ModesOfTestNoise.IsoLineAt005Hz)
            {
                EEG5Device device = GetDevice(environment);
                device.SetFilterHiVP(0, 250.0f);
                device.SetFilterHiVP(1, 250.0f);
                device.SetFilterHiVP(2, 250.0f);
                device.SetFilterHiVP(3, 250.0f);
                device.SetChannelsUsing();
            }
        }

        /// <summary>
        /// Проверяет и сохраняет значения размаха сигнала шума 
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
            ReadDataSettings vpSynphNoiseSetts = new ReadDataSettings();
            vpSynphNoiseSetts.SwingRange.Max = 10e-6;
            vpSynphNoiseSetts.SwingRange.Min = 0.0;
            vpSynphNoiseSetts.MinFreqFilter = 1;
            vpSynphNoiseSetts.MaxFreqFilter = 250.0;
            vpSynphNoiseSetts.RangeIndex = 0;
            vpSynphNoiseSetts.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            vpSynphNoiseSetts.YScale = new ScaleItem(500e-9f, "500 нВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, 50.0f);
            StandOperationsEEG.SetSynphaseVpSignal(environment);
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
            ReadDataSettings vpFiltsSettings = new ReadDataSettings();
            float currentFreq = 0.0f;
            switch (testedFilter)
            {
                case VpFiltersForEEG5.HighFreq_0212_Hz:
                    currentFreq = 0.2f;
                    vpFiltsSettings.SwingRange.Max = 7.7e-3;
                    vpFiltsSettings.SwingRange.Min = 6.3e-3;
                    vpFiltsSettings.MinFreqFilter = 0.212;
                    vpFiltsSettings.MaxFreqFilter = 10000.0;
                    vpFiltsSettings.TickIntervalsProcessing = 10;
                    vpFiltsSettings.XScale = new ScaleItem(200e-3f, "0,2 с/мм");
                    vpFiltsSettings.YScale = new ScaleItem(500e-6f, "500 мкВ/мм");
                    break;
                case VpFiltersForEEG5.HighFreq_05_Hz:
                    currentFreq = 0.7f;
                    vpFiltsSettings.SwingRange.Max = 7.7e-3;
                    vpFiltsSettings.SwingRange.Min = 6.3e-3;
                    vpFiltsSettings.MinFreqFilter = 0.7;
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
                    vpFiltsSettings.MinFreqFilter = 0.7;
                    vpFiltsSettings.MaxFreqFilter = 250.0;
                    vpFiltsSettings.XScale = new ScaleItem(500e-6f, "0,5 мс/мм");
                    vpFiltsSettings.YScale = new ScaleItem(500e-6f, "500 мкВ/мм");
                    break;
                case VpFiltersForEEG5.LowFreq_10000_Hz:
                    currentFreq = 10001.0f;//10000.0f;
                    vpFiltsSettings.SwingRange.Max = 80e-3;
                    vpFiltsSettings.SwingRange.Min = 60e-3;
                    vpFiltsSettings.MinFreqFilter = 0.7;
                    vpFiltsSettings.MaxFreqFilter = 10000.0;
                    vpFiltsSettings.XScale = new ScaleItem(20e-6f, "20 мкс/мм");
                    vpFiltsSettings.YScale = new ScaleItem(5e-3f, "5 мВ/мм");
                    break;
            }
            vpFiltsSettings.RangeIndex = 8;
            StandOperations.SetGeneratorState(environment, (testedFilter == VpFiltersForEEG5.LowFreq_10000_Hz ? 0.025f : 0.0025f), currentFreq);
            StandOperationsEEG.SetDifferetialVpSignal(environment);
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
            //ReadDataSettings vpGainsSettings = new ReadDataSettings();
            //vpGainsSettings.RangeIndex = 0;
            //float signalAmpl = 0.00005f;
            //vpGainsSettings.SwingRange.Max = signalAmpl * 4 * 1.08;
            //vpGainsSettings.SwingRange.Min = signalAmpl * 4 * 0.92;
            //vpGainsSettings.MinFreqFilter = 1.0;
            //vpGainsSettings.MaxFreqFilter = 10000.0;
            //vpGainsSettings.XScale = new ScaleItem(1e-3f, "1 мс/мм");
            //vpGainsSettings.YScale = new ScaleItem(20e-6f, "20 мкВ/мм");
            //StandOperations.SetGeneratorState(environment, signalAmpl, 300);
            //StandOperationsEEG.SetDifferetialVpSignal(environment);
            //StartReadData(environment, contentPresenterName, EEGTestingMode.VPTransmit, vpGainsSettings);

            EEG5Device device = GetDevice(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            TestInputRangesViewModelEEG5 testRangesViewModel = new TestInputRangesViewModelEEG5(environment, device);
            TestInputRangesControl testRangesDataControl = new TestInputRangesControl(testRangesViewModel);
            presenter.Content = testRangesDataControl;
            ReadDataControlName = contentPresenterName;
            testRangesDataControl.StartTest();
        }

        /// <summary>
        /// Запуск проверки коэффициентов усиления в каналах ВП
        /// и изменение диапазона и размаха сигнала
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="gainRange"> Диапазон усиления </param>
        /// <param name="signalAmpl"> Амплитуда тестового сигнала </param>
        public static void TestGainFactorsVP(ScriptEnvironment environment, string contentPresenterName, VpTestGainRangeMode gainRange)
        {
            ReadDataSettings vpGainsSettings = new ReadDataSettings();
            float signalAmpl = 0.0f;
            switch (gainRange)
            {
                case VpTestGainRangeMode.Range200mkV_Swing120mkV:
                    vpGainsSettings.RangeIndex = 0;
                    signalAmpl = 0.00005f;
                    vpGainsSettings.XScale = new ScaleItem(1e-3f, "1 мс/мм");
                    vpGainsSettings.YScale = new ScaleItem(20e-6f, "20 мкВ/мм");
                    break;
                case VpTestGainRangeMode.Range500mkV_Swing120mkV:                    
                    vpGainsSettings.RangeIndex = 1;
                    signalAmpl = 0.00005f;
                    break;
                case VpTestGainRangeMode.Range1mV_Swing120mkV:                       
                    vpGainsSettings.RangeIndex = 2;
                    signalAmpl = 0.00005f;
                    break;
                case VpTestGainRangeMode.Range1mV_Swing1mV:
                    vpGainsSettings.RangeIndex = 2;
                    vpGainsSettings.YScale = new ScaleItem(100e-6f, "100 мкВ/мм");
                    signalAmpl = 0.00025f;
                    break;
                case VpTestGainRangeMode.Range2mV_Swing1mV:                       
                    vpGainsSettings.RangeIndex = 3;
                    signalAmpl = 0.00025f;
                    break;
                case VpTestGainRangeMode.Range5mV_Swing1mV:                       
                    vpGainsSettings.RangeIndex = 4;
                    signalAmpl = 0.00025f;
                    break;
                case VpTestGainRangeMode.Range10mV_Swing1mV:                      
                    vpGainsSettings.RangeIndex = 5;
                    signalAmpl = 0.00025f;
                    break;
                case VpTestGainRangeMode.Range10mV_Swing10mV:
                    vpGainsSettings.RangeIndex = 5;
                    signalAmpl = 0.0025f;
                    vpGainsSettings.YScale = new ScaleItem(1e-3f, "1 мВ/мм");
                    break;
                case VpTestGainRangeMode.Range20mV_Swing10mV:                      
                    vpGainsSettings.RangeIndex = 6;
                    signalAmpl = 0.0025f;
                    break;
                case VpTestGainRangeMode.Range50mV_Swing10mV:                      
                    vpGainsSettings.RangeIndex = 7;
                    signalAmpl = 0.0025f;
                    break;
                case VpTestGainRangeMode.Range100mV_Swing10mV:                     
                    vpGainsSettings.RangeIndex = 8;
                    signalAmpl = 0.0025f;
                    break;
                case VpTestGainRangeMode.Range100mV_Swing100mV:
                    vpGainsSettings.RangeIndex = 8;
                    signalAmpl = 0.025f;
                    vpGainsSettings.YScale = new ScaleItem(10e-3f, "10 мВ/мм");
                    break;
            }
            if (gainRange == VpTestGainRangeMode.Range200mkV_Swing120mkV || gainRange == VpTestGainRangeMode.Range500mkV_Swing120mkV || gainRange == VpTestGainRangeMode.Range1mV_Swing120mkV)
            {
                vpGainsSettings.SwingRange.Max = signalAmpl * 4 * 1.08;
                vpGainsSettings.SwingRange.Min = signalAmpl * 4 * 0.92;
            }
            else
            {
                vpGainsSettings.SwingRange.Max = signalAmpl * 4 * 1.05;
                vpGainsSettings.SwingRange.Min = signalAmpl * 4 * 0.95;
            }
            vpGainsSettings.MinFreqFilter = 1.0;
            vpGainsSettings.MaxFreqFilter = 10000.0;
            StandOperations.SetGeneratorState(environment, signalAmpl, 300);
            StandOperationsEEG.SetDifferetialVpSignal(environment);

            EEG5Device device = GetDevice(environment);
            ReadDataControl readDataControl;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter != null)
            {
                readDataControl = presenter.Content as ReadDataControl;
                if (readDataControl != null)
                {
                    readDataControl.SetSettings(vpGainsSettings);
                }
            }
        }

        /// <summary>
        /// Метод, проверящий значения размаха сигнала и сохраняющий значения
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="firstChannelSwings"> Массив для хранения размахов сигнала </param>
        /// <returns> Результаты проверки </returns>
        public static bool? CheckAndSaveInputRangeResults(ScriptEnvironment environment, /*string contentPresenterName,*/ double[][] ChannelsSwings/*, double[] secondChannelSwings*/)
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

        /// <summary>
        /// Проверяет и сохраняет размахи сигналов при проверке коэффициентов усиления каналов ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="savedSwings"> Массив сохраняемых значний </param>
        /// <param name="ismVScale"> Флаг, указывающий в мВ или мкВ сохранять значения </param>
        /// <returns> Успешность выполнения проверки </returns>
        public static bool CheckAndSaveVpSwings(ScriptEnvironment environment, ref double[] savedSwings, bool ismVScale = true)
        {
            ReadDataControl readDataContr = GetReadDataControl(environment);
            if (readDataContr == null)
                return false;
            //StopDataReading(environment);
            List<double> noiseSwings = new List<double>();
            for (int i = 0; i < readDataContr.ViewModel.StatisticsCollection.Count; i++)
            {
                if (!readDataContr.ViewModel.StatisticsCollection[i].Swing.IsValidValue)
                {
                    CommonScripts.ShowError("Значание размаха сигнала в канале " + readDataContr.ViewModel.StatisticsCollection[i].ChannelName +
                                            " не соответствует требованиям. Внесите необходимые исправления и выполните тест заново.");
                    return false;
                }
                noiseSwings.Add(readDataContr.ViewModel.StatisticsCollection[i].Swing.Value * (ismVScale ? 1e3 : 1e6));
            }
            savedSwings = noiseSwings.ToArray();
            return true;
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
            ReadDataSettings vpImpedancesSettings = new ReadDataSettings();
            vpImpedancesSettings.SwingRange.Min = 0.0;
            vpImpedancesSettings.SwingRange.Max = 1.8e-3;
            vpImpedancesSettings.MinFreqFilter = 1.0;
            vpImpedancesSettings.MaxFreqFilter = 200.0;
            vpImpedancesSettings.RangeIndex = 5;
            vpImpedancesSettings.XScale = new ScaleItem(5e-3f, "5 мс/мм");
            vpImpedancesSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, 100.0f);
            StandOperationsEEG.SetVpForInputImpedance(environment, isPositiveTested);
            StartReadData(environment, contentPresenterName, EEGTestingMode.VPTransmit, vpImpedancesSettings);
        }

        #endregion

        #endregion

        #region Проверка каналов ЭКГ и дыхания

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
            breathSettings.MaxFreqFilter = 10.0;
            breathSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
            breathSettings.YScale = new ScaleItem(2e-3f, "2 мВ/мм");
            StandOperations.SetGeneratorState(environment, 5.0f, 2.0f);
            StandOperations.SetGeneratorState(environment, 0.001f, 1.0f, WaveForm.Sinus, 1);
            StandOperationsEEG.SetBreathChannel(environment, true);
            StartReadData(environment, contentPresenterName, EEGTestingMode.BreathTransmit, breathSettings);
        }

        /// <summary>
        /// Запускает проверку шумов в канале ЭКГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isWideFreqRange"> Флаг, указывающий, что проверка проводится в шикором диапазоне частот </param>
        public static void StartEcgNoiseTest(ScriptEnvironment environment, string contentPresenterName, bool isWideFreqRange)
        {
            ReadDataSettings ecgNoiseSettings = new ReadDataSettings();
            ecgNoiseSettings.SwingRange.Min = 0.0;
            ecgNoiseSettings.SwingRange.Max = isWideFreqRange ? 3.0e-6 : 1.0e-6;
            ecgNoiseSettings.MinFreqFilter = 0.5;
            ecgNoiseSettings.MaxFreqFilter = isWideFreqRange ? 1000.0 : 35.0;
            ecgNoiseSettings.XScale = isWideFreqRange ? new ScaleItem(10e-3f, "10 мс/мм") : new ScaleItem(50e-3f, "50 мс/мм");
            ecgNoiseSettings.YScale = isWideFreqRange ? new ScaleItem(500e-9f, "500 нВ/мм") : new ScaleItem(100e-9f, "100 нВ/мм");

            StandOperationsEEG.ConnectEcgChannelToGnd(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.ECGNoiseTransmit, ecgNoiseSettings);
        }

        /// <summary>
        /// Запуск проверки коэффициента усиления канала ЭКГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartEcgGainTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings ecgGainSettings = new ReadDataSettings();
            ecgGainSettings.SwingRange.Min = 0.96e-3;
            ecgGainSettings.SwingRange.Max = 1.04e-3;
            ecgGainSettings.MinFreqFilter = 0.05;
            ecgGainSettings.MaxFreqFilter = 1000.0;
            ecgGainSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            ecgGainSettings.YScale = new ScaleItem(50e-6f, "50 мкВ/мм");

            StandOperations.SetGeneratorState(environment, 0.25e-3f, 10.0f);
            StandOperationsEEG.SetEcgDifferetialSignal(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.ECGTransmit, ecgGainSettings);
        }

        /// <summary>
        /// Запуск проверки аппаратных фильтров канала ЭКГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="isHighFreqFilter"> Флаг, указывающий какой фильтр проверяется </param>
        public static void StartEcgFiltersTest(ScriptEnvironment environment, string contentPresenterName, bool isHighFreqFilter)
        {
            ReadDataSettings ecgFiltersSettings = new ReadDataSettings();
            ecgFiltersSettings.SwingRange.Min = isHighFreqFilter ? 6.3e-3 : 6.8e-3;
            ecgFiltersSettings.SwingRange.Max = isHighFreqFilter ? 7.7e-3 : 7.2e-3;
            ecgFiltersSettings.MinFreqFilter = isHighFreqFilter ? 0 : 0.05;
            ecgFiltersSettings.MaxFreqFilter = isHighFreqFilter ? 35.0 : 1000.0;
            ecgFiltersSettings.XScale = isHighFreqFilter ? new ScaleItem(1f, "1 с/мм") : new ScaleItem(2e-3f, "2 мс/мм");
            ecgFiltersSettings.YScale = /*isHighFreqFilter ? new ScaleItem(500e-9f, "500 нВ/мм") :*/ new ScaleItem(500e-6f, "500 мкВ/мм");
            if (isHighFreqFilter)
                ecgFiltersSettings.TickIntervalsProcessing = 40;

            StandOperations.SetGeneratorState(environment, 2.5e-3f, isHighFreqFilter ? 0.05f : 150.0f);
            StandOperationsEEG.SetEcgDifferetialSignal(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.ECGTransmit, ecgFiltersSettings);
        }

        /// <summary>
        /// Запускает проверку подавления синфазной помехи канала ЭКГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartEcgSynphaseNoiseTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings ecgSynphSettings = new ReadDataSettings();
            ecgSynphSettings.SwingRange.Min = 0.0;
            ecgSynphSettings.SwingRange.Max = 15.0e-6;
            ecgSynphSettings.MinFreqFilter = 0.5;
            ecgSynphSettings.MaxFreqFilter = 1000.0;
            ecgSynphSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            ecgSynphSettings.YScale = new ScaleItem(200e-9f, "200 нВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, 50.0f);
            StandOperationsEEG.SetSynphaseSignalEcg(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.ECGTransmit, ecgSynphSettings);
        }

        /// <summary>
        /// Сохраняет значение размаха для канала ЭКГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="savedValue"> Сохраняемое значение </param>
        /// <param name="ismVScale"> Флаг сохранения значений в мВ (true - мВ, false - мкВ) </param>
        /// <param name="isNoiseTest"> Флаг проверки размаха сигнала шума </param>
        /// <returns> Успешность выполнения проверки </returns>
        public static bool? CheckAndSaveEcgSwing(ScriptEnvironment environment, ref double savedValue, bool ismVScale = true, bool isNoiseTest = false)
        {
            ReadDataControl readDataContr = GetReadDataControl(environment);
            if (readDataContr == null)
                return null;
            StopDataReading(environment);
            List<double> swings = new List<double>();
            if (!readDataContr.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                CommonScripts.ShowError(String.Format(Properties.Resource1.NoiseSwingsInvalidMessage, isNoiseTest ? Properties.Resource1.OfNoise : "", readDataContr.ViewModel.StatisticsCollection[0].ChannelName));
                return null;
            }
            savedValue = readDataContr.ViewModel.StatisticsCollection[0].Swing.Value * (ismVScale ? 1e3 : 1e6);
            return true;
        }

        /// <summary>
        /// Сохраняет значение размаха для одного из каналов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="savedIndex"> Индекс сохраняемого канала </param>
        /// <param name="savedValue"> Сохраняемое значение </param>
        /// <returns> Успешность выполнения проверки </returns>
        public static bool? CheckAndSaveSwings(ScriptEnvironment environment, int savedIndex, ref double savedValue)
        {
            ReadDataControl readDataContr = GetReadDataControl(environment);
            if (readDataContr == null)
                return null;
            StopDataReading(environment);
            List<double> swings = new List<double>();
            if (!readDataContr.ViewModel.StatisticsCollection[savedIndex].Swing.IsValidValue)
            {
                CommonScripts.ShowError(String.Format(Properties.Resource1.NoiseSwingsInvalidMessage, "", readDataContr.ViewModel.StatisticsCollection[savedIndex].ChannelName));
                return null;
            }
            savedValue = readDataContr.ViewModel.StatisticsCollection[savedIndex].Swing.Value * 1e3;
            return true;
        }

        #endregion

        #region Проверка каналов DC1 и DC2

        /// <summary>
        /// Запускает проверку каналов DC
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="testSignalFreq"> Частота тестового сигнала </param>
        public static void StartTestDcChannels(ScriptEnvironment environment, string contentPresenterName, float testSignalFreq)
        {
            ReadDataSettings dcChannelsSettings = new ReadDataSettings();
            dcChannelsSettings.SwingRange.Min = 0.95;
            dcChannelsSettings.SwingRange.Max = 1.05;
            if (testSignalFreq == 1.0)
                dcChannelsSettings.XScale = new ScaleItem(50e-3f, "50 мс/мм");
            else
                dcChannelsSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            dcChannelsSettings.YScale = new ScaleItem(100e-3f, "100 мВ/мм");

            StandOperations.SetGeneratorState(environment, 0.5f, testSignalFreq, WaveForm.Sinus, 1);
            StandOperations.SetGeneratorState(environment, 0.0001f, 1.0f);
            StandOperationsEEG.SetDcChannelsState(environment);
            StartReadData(environment, contentPresenterName, EEGTestingMode.DCTransmit, dcChannelsSettings);
        }

        /// <summary>
        /// Проверяет и сохраняет размахи сигналов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="savedArray"> Массив, в который сохраняются значения </param>
        /// <returns> Успешность выполнения проверки значений </returns>
        public static bool? CheckAndSaveSwings(ScriptEnvironment environment, ref double[] savedArray, ref double worstSwing, bool? isMinMaxBest = null)
        {
            ReadDataControl readDataContr = GetReadDataControl(environment);
            if (readDataContr == null)
                return null;
            StopDataReading(environment);
            double bestValue = (readDataContr.ViewModel.StatisticsCollection[0].Swing.MaxValue + readDataContr.ViewModel.StatisticsCollection[0].Swing.MinValue) / 2;
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            worstSwing = bestValue;
            List<double> swings = new List<double>();
            for (int i = 0; i < readDataContr.ViewModel.StatisticsCollection.Count; i++)
            {
                if (!readDataContr.ViewModel.StatisticsCollection[i].Swing.IsValidValue)
                {
                    CommonScripts.ShowError(String.Format(Properties.Resource1.NoiseSwingsInvalidMessage, "", readDataContr.ViewModel.StatisticsCollection[i].ChannelName));
                    return null;
                }
                swings.Add(readDataContr.ViewModel.StatisticsCollection[i].Swing.Value * 1e3);
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
            savedArray = swings.ToArray();
            worstSwing *= 1e3;
            return true;
        }

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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = OpenDevice(environment);
            if (device.DeviceInformation.StimInfo == 0)
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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = GetDevice(environment);
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
            
            EEG5Device device;
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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = GetDevice(environment);
            if (device == null)
                return;
            (device as NeuroSoft.Devices.ICurrentStimulator).SetStimulusDuration((float)duration);
            (device as NeuroSoft.Devices.ICurrentStimulator).SetStimulusAmplitude((float)amplitude);
            (device as NeuroSoft.Devices.ICurrentStimulator).SetFrequency((float)(1.0 / period));
            if (isPositivePolarity)
                (device as NeuroSoft.Devices.ICurrentStimulator).SetPolarity(StimulusPolarity.Plus);
            else
                (device as NeuroSoft.Devices.ICurrentStimulator).SetPolarity(StimulusPolarity.Minus);
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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = GetDevice(environment);
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
            EEG5Device device = GetDevice(environment);
            ReadDataSettings synchroInSettings = new ReadDataSettings();
            synchroInSettings.SwingRange.Min = 0;
            synchroInSettings.SwingRange.Max = 1;
            synchroInSettings.XScale = new ScaleItem(20e-3f, "20 мс/мм");
            synchroInSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");
            EEG5FlashStimulatorState flashState = ((device as NeuroSoft.Devices.IFlashStimulator).GetState() as EEG5FlashStimulatorState);
            flashState.TrigInEnabled = true;
            (device as NeuroSoft.Devices.IFlashStimulator).SetState(flashState);
            StartReadData(environment, contentPresenterName, EEGTestingMode.Calibration, synchroInSettings);
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

        #region Тест пилы

        private static string PilaTestPresenterName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresName"></param>
        public static void StartPilaTest(ScriptEnvironment environment, string contentPresName)
        {
            /*ContentPresenter presenter*/PilaTestContentPresenter = environment[contentPresName] as ContentPresenter;
            if (PilaTestContentPresenter == null)
                PilaTestContentPresenter = new ContentPresenter();
            SendCommTimer = new System.Threading.Timer(SendCommTimerCallback, null, 100, 100);
            numReceivedBlocks = 0;
            numReceivedBytes = 0;
            numSendedBytes = 0;
            numErrors = 0;
            isSearchPreamb = true;
            TestPilaMessage = String.Format("Количество принятых блоков: {0}.\nКоличество принятых байт: {1}.\nКоличество переданных байт: {2}.\nКолчество ошибок: {3}",
                                            numReceivedBlocks, numReceivedBytes, numSendedBytes, numErrors);
            PilaTestContentPresenter.Content = TestPilaMessage;
            PilaTestPresenterName = contentPresName;
            SetTransmitPILA(PilaUserHandler);
        }

        /// <summary>
        /// Проверяет наличие или отсутствие ошибок в тесте пилы
        /// </summary>
        /// <returns> Результат проверки </returns>
        public static bool? CheckUpPilaTest()
        {
            if (numErrors == 0)
                return true;
            else
            {
                CommonScripts.ShowError(Properties.Resource1.PilaTestErrMsg);
                return null;
            }
        }

        public static void StopPilaTest(ScriptEnvironment environment)
        {
            //EEG5Device device = GetDevice(environment);
            //device.StopTransmit();
            OnStopReceive();
            environment.RemoveAndDispose(PilaTestPresenterName);
        }

        private static string TestPilaMessage;
        private static uint numReceivedBlocks;
        private static uint numReceivedBytes;
        private static uint numSendedBytes;
        private static uint numErrors;
        private static PilaUserFunc PilaUserHandler = GetPilaData;
        private static byte currentByte;
        private static byte nextByte;
        private static bool isSearchPreamb;
        private static byte curPreambula;
        private static ContentPresenter PilaTestContentPresenter;
        private static System.Threading.Timer SendCommTimer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pilaData"></param>
        /// <param name="dataLength"></param>
        private static void GetPilaData(IntPtr pilaData, int dataLength)
        {
            int indx = 6;
            byte[] receivedData = new byte[dataLength];
            Marshal.Copy(pilaData, receivedData, 0, dataLength);

            /* Поиск преамбулы */
            if (isSearchPreamb)
            {
                for (int i = indx; i < dataLength; i++)
                {
                    if (curPreambula == 15)
                    {
                        isSearchPreamb = false;
                        indx = i;
                        curPreambula = 0;
                        break;
                    }
                    if (receivedData[i] == curPreambula)
                        curPreambula++;
                    else
                        curPreambula = 0;
                }
                if (isSearchPreamb)
                    return;
            }

            if (numReceivedBlocks == 0)
            {
                currentByte = receivedData[indx];
                nextByte = receivedData[indx + 1];
            }
            else
                nextByte = receivedData[indx];

            for ( ; ; )
            {
                if (currentByte == 255)
                {
                    if (nextByte != 0)
                        numErrors++;
                }
                else
                {
                    if (currentByte + 1 != nextByte)
                        numErrors++;
                }
                indx++;
                if (indx == dataLength - 1)
                {
                    currentByte = receivedData[indx];
                    break;
                }
                currentByte = receivedData[indx];
                nextByte = receivedData[indx + 1];
            }
            numReceivedBlocks++;
            numReceivedBytes += (uint)dataLength;
            if (numReceivedBlocks % 100 == 0)
                PilaTestContentPresenter.Dispatcher.BeginInvoke(new Action(() =>
                {
                    PilaTestContentPresenter.Content = TestPilaMessage = String.Format("Количество принятых блоков: {0}.\nКоличество принятых байт: {1}.\nКоличество переданных байт: {2}.\nКолчество ошибок: {3}",
                                                                                       numReceivedBlocks, numReceivedBytes, numSendedBytes, numErrors);
                }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        private static void SendCommTimerCallback(object param)
        {
            value_param_tok(1, 100);
            numSendedBytes += 24;
            //EEG5Device device = GetDevice(param as ScriptEnvironment);
            //(device as NeuroSoft.Devices.ICurrentStimulator)
        }

        #endregion

        /// <summary>
        /// Проводит сортировку массива с именами электродав так, чтобы они мигали по порядку
        /// </summary>
        /// <param name="originString"> Исходный массив с именами, отсортированными по алфавиту </param>
        /// <returns> Отсортированный массив </returns>
        public static string[] SortLedNames(string[] originString, bool isSortForNS5, bool isSortForIndication = true)
        {
            int numOfExtraChannels = 11;
            string[] destinationStr = new string[isSortForNS5 ? originString.Length : originString.Length - numOfExtraChannels];
            foreach (string currentName in originString)
            {
                #region switch с перебором имен электродов
                switch (currentName)
                {
                    case "E1_MINUS":
                        destinationStr[0] = currentName;
                        break;
                    case "E2_MINUS":
                        destinationStr[1] = currentName;
                        break;
                    case "E3_MINUS":
                        destinationStr[2] = currentName;
                        break;
                    case "E4_MINUS":
                        destinationStr[3] = currentName;
                        break;
                    case "E1_PLUS":
                        destinationStr[4] = currentName;
                        break;
                    case "E2_PLUS":
                        destinationStr[5] = currentName;
                        break;
                    case "E3_PLUS":
                        destinationStr[6] = currentName;
                        break;
                    case "E4_PLUS":
                        destinationStr[7] = currentName;
                        break;
                    case "X1":
                        if (isSortForNS5)
                            destinationStr[8] = currentName;
                        break;
                    case "X2":
                        if (isSortForNS5)
                            destinationStr[9] = currentName;
                        break;
                    case "X3":
                        if (isSortForNS5)
                            destinationStr[10] = currentName;
                        break;
                    case "X4":
                        if (isSortForNS5)
                            destinationStr[11] = currentName;
                        break;
                    case "X5":
                        if (isSortForNS5)
                            destinationStr[12] = currentName;
                        break;
                    case "X6":
                        if (isSortForNS5)
                            destinationStr[13] = currentName;
                        break;
                    case "X7":
                        if (isSortForNS5)
                            destinationStr[14] = currentName;
                        break;
                    case "X8":
                        if (isSortForNS5)
                            destinationStr[15] = currentName;
                        break;
                    case "X9":
                        if (isSortForNS5)
                            destinationStr[16] = currentName;
                        break;
                    case "X10":
                        if (isSortForNS5)
                            destinationStr[17] = currentName;
                        break;
                    case "X11":
                        if (isSortForNS5)
                            destinationStr[18] = currentName;
                        break;
                    case "A2":
                        destinationStr[isSortForNS5 ? 19 : 19 - numOfExtraChannels] = currentName;
                        break;
                    case "F8":
                        destinationStr[isSortForNS5 ? 20 : 20 - numOfExtraChannels] = currentName;
                        break;
                    case "T4":
                        destinationStr[isSortForNS5 ? 21 : 21 - numOfExtraChannels] = currentName;
                        break;
                    case "T6":
                        destinationStr[isSortForNS5 ? 22 : 22 - numOfExtraChannels] = currentName;
                        break;
                    case "FP2":
                        destinationStr[isSortForNS5 ? 23 : 23 - numOfExtraChannels] = currentName;
                        break;
                    case "F4":
                        destinationStr[isSortForNS5 ? 24 : 24 - numOfExtraChannels] = currentName;
                        break;
                    case "C4":
                        destinationStr[isSortForNS5 ? 25 : 25 - numOfExtraChannels] = currentName;
                        break;
                    case "P4":
                        destinationStr[isSortForNS5 ? 26 : 26 - numOfExtraChannels] = currentName;
                        break;
                    case "O2":
                        destinationStr[isSortForNS5 ? 27 : 27 - numOfExtraChannels] = currentName;
                        break;
                    case "FPZ":
                        destinationStr[isSortForNS5 ? 28 : 28 - numOfExtraChannels] = currentName;
                        break;
                    case "FZ":
                        destinationStr[isSortForNS5 ? 29 : 29 - numOfExtraChannels] = currentName;
                        break;
                    case "CZ":
                        destinationStr[isSortForNS5 ? 30 : 30 - numOfExtraChannels] = currentName;
                        break;
                    case "PZ":
                        destinationStr[isSortForNS5 ? 31 : 31 - numOfExtraChannels] = currentName;
                        break;
                    case "OZ":
                        destinationStr[isSortForNS5 ? 32 : 32 - numOfExtraChannels] = currentName;
                        break;
                    case "FP1":
                        destinationStr[isSortForNS5 ? 33 : 33 - numOfExtraChannels] = currentName;
                        break;
                    case "F3":
                        destinationStr[isSortForNS5 ? 34 : 34 - numOfExtraChannels] = currentName;
                        break;
                    case "C3":
                        destinationStr[isSortForNS5 ? 35 : 35 - numOfExtraChannels] = currentName;
                        break;
                    case "P3":
                        destinationStr[isSortForNS5 ? 36 : 36 - numOfExtraChannels] = currentName;
                        break;
                    case "O1":
                        destinationStr[isSortForNS5 ? 37 : 37 - numOfExtraChannels] = currentName;
                        break;
                    case "F7":
                        destinationStr[isSortForNS5 ? 38 : 38 - numOfExtraChannels] = currentName;
                        break;
                    case "T3":
                        destinationStr[isSortForNS5 ? 39 : 39 - numOfExtraChannels] = currentName;
                        break;
                    case "T5":
                        destinationStr[isSortForNS5 ? 40 : 40 - numOfExtraChannels] = currentName;
                        break;
                    case "A1":
                        destinationStr[isSortForNS5 ? 41 : 41 - numOfExtraChannels] = currentName;
                        break;
                    case "ECG_PLUS":
                        destinationStr[isSortForIndication ? (isSortForNS5 ? 42 : 42 - numOfExtraChannels) : (isSortForNS5 ? 43 : 43 - numOfExtraChannels)] = currentName;
                        break;
                    case "REF":
                        destinationStr[isSortForIndication ? (isSortForNS5 ? 43 : 43 - numOfExtraChannels) : (isSortForNS5 ? 42 : 42 - numOfExtraChannels)] = currentName;
                        break;
                    case "ZERO":
                        destinationStr[isSortForIndication ? (isSortForNS5 ? 44 : 44 - numOfExtraChannels) : (isSortForNS5 ? 45 : 45 - numOfExtraChannels)] = currentName;
                        break;
                    case "ECG_MINUS":
                        destinationStr[isSortForIndication ? (isSortForNS5 ? 45 : 45 - numOfExtraChannels) : (isSortForNS5 ? 44 : 44 - numOfExtraChannels)] = currentName;
                        break;
                }
                #endregion
            }
            return destinationStr;
        }

        #region Импорт функций

        /// <summary>
        /// Запустить передачу пилы с прибора
        /// </summary>
        /// <param name="userFuncPila"></param>
        [DllImport(NSEEG4Consts.EEG5dllName)]
        static extern void SetTransmitPILA(PilaUserFunc userFuncPila);

        /// <summary>
        /// Установить параметры токового стимула
        /// </summary>
        /// <param name="tok"> ток в мА   от -100.0 до 100.0 мА </param>
        /// <param name="l"> длительность стимула от 50 до 5000 мкс </param>
        [DllImport(NSEEG4Consts.EEG5dllName)]
        static extern void value_param_tok(double tok, uint l);

        /// <summary>
        /// Остановить передачу данных из прибора
        /// </summary>
        [DllImport(NSEEG4Consts.EEG5dllName)]
        static extern void OnStopReceive();

        #endregion
    }
    /// <summary>
    /// Перечисление, указывающее аппаратные фильтры каналов ВП в энцефалографе
    /// </summary>
    public enum VpFiltersForEEG5 { LowFreq_250_Hz, LowFreq_10000_Hz, HighFreq_0212_Hz, HighFreq_05_Hz, HighFreq_160_Hz }

    /// <summary>
    /// Перечисление режимов проверки коэффициентов усиления каналов ВП
    /// </summary>
    public enum VpTestGainRangeMode 
    { 
        Range200mkV_Swing120mkV, 
        Range500mkV_Swing120mkV, 
        Range1mV_Swing120mkV, 
        Range1mV_Swing1mV, 
        Range2mV_Swing1mV, 
        Range5mV_Swing1mV, 
        Range10mV_Swing1mV, 
        Range10mV_Swing10mV, 
        Range20mV_Swing10mV, 
        Range50mV_Swing10mV, 
        Range100mV_Swing10mV, 
        Range100mV_Swing100mV 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataPtr"></param>
    /// <param name="dataLength"></param>
    public delegate void PilaUserFunc(IntPtr dataPtr, int dataLength);
}
