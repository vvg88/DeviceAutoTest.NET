using System;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices;
using NeuroSoft.Hardware.Tools.Usb;
using System.IO;
using System.Windows.Controls;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.Hardware.Tools.Cypress;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts
{
    public static class NeuroMEPMicroScripts
    {
        #region Программирование контроллеров

        #region Программирование PIC'ов

        /// <summary>
        /// Cохраняет имя файла прошивки PIC контроллера
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

        #region Программирование USB контроллера

        /// <summary>
        /// Программирует EEPROM через Cypress
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="normPanelPosotion"> Признак ориентации панели: true - нормальная ориентация (плюс сверху), false - инверсная ориентация (плюс снизу) </param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool? DoFirmwareEeprom(ScriptEnvironment environment, bool normPanelPosotion, ref string fileName, bool isEmgMs = false, string fwFolder = "")
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
                weavingInformation.Address = 0x80C/*2060*/;
                weavingInformation.PID_Enable = true;
                weavingInformation.PID_Addr = 0x80A/*2058*/;
                weavingInformation.PID_Value = (ushort)(isEmgMs ? 0x8263 : 0x8260)/*33376*/;
                weavingInformation.AddData_Addr = 0x812/*2066*/;
                weavingInformation.CypressCpuType = CypressTools.CypressCpuType.FX1_FX2LP;

                weavingInformation.AddData_Value = normPanelPosotion ? new byte[] { 0, 0, 0, 0 } : new byte[] { 1, 0, 0, 0 };

                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "iic|" + fileNameTemplate;
                openFileDialog.InitialDirectory = firmwareFolder;
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
        /// Программирует EEPROM через Cypress Нейро-ЭМГ-Микро
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="fileName"> Название файла прошивки </param>
        /// <returns> Успешность выполнения операции </returns>
        public static bool? DoFirmwareEeprom(ScriptEnvironment environment, ref string fileName, string fwFolder = "")
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
                weavingInformation.PID_Value = 0x8261;
                weavingInformation.AddData_Addr = 0x812;
                weavingInformation.CypressCpuType = Hardware.Tools.Cypress.CypressTools.CypressCpuType.FX1_FX2LP;
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "iic|" + fileNameTemplate;
                openFileDialog.InitialDirectory = firmwareFolder;
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

        #region Вспомогательные функции программирования

        /// <summary>
        /// Папка с прошивками
        /// </summary>
        internal static string FirmwareFolder
        {
            get
            {
                string fwFolder = @"\\SERVER\firmware\Нейро-МВП-микро\";
                return Directory.Exists(fwFolder) ? fwFolder : @"C:\";
            }
        }

        #endregion

        #endregion

        #region DeviceOpening

        /// <summary>
        /// Открывает устройство
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static NeuroMEPMicro OpenDevice(ScriptEnvironment environment)
        {
            NeuroMEPMicro device = environment.Device as NeuroMEPMicro;
            if (device == null)
            {
                device = new NeuroMEPMicro(environment.DeviceSerialNumber);
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
        public static ValidationResult ValidateDeviceOpening(ScriptEnvironment environment, bool details = false)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
            {
                return new ValidationResult(false, NeuroSoft.DeviceAutoTest.Common.Properties.Resources.CouldNotOpenDevice);
            }
            return new ValidationResult(true, null);
        }

        #endregion

        #region Проверка информации о приборе

        /// <summary>
        /// Отображает контрол с информацией о приборе
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя презентера </param>
        public static void ShowDeviceInformation(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return;
            }
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            DeviceInformationControl deviceInfoControl = new DeviceInformationControl(environment, false);
            presenter.Content = deviceInfoControl;
        }

        public static bool? TestDeviceInformation(ScriptEnvironment environment, string contentPresenterName, bool removeControl = true)
        {
            string errorMessage;
            bool? devInfoResult = ((environment[contentPresenterName] as ContentPresenter).Content as DeviceInformationControl).CheckDeviceInformationValid(out errorMessage);
            if (devInfoResult != true)
                CommonScripts.ShowError(errorMessage);
            if (removeControl)
                environment.RemoveAndDispose(contentPresenterName);
            return devInfoResult;
        }

        #endregion

        #region Проверка функционирования идикации и клавиатуры

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя презентера </param>
        public static void ShowKeyboardControl(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return;
            }
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            KeyboardControl keyboardControl = new KeyboardControl(device);
            presenter.Content = keyboardControl;
        }

        public static bool? TestKeyboard(ScriptEnvironment environment, string contentPresenterName)
        {            
            bool? result = ((environment[contentPresenterName] as ContentPresenter).Content as KeyboardControl).AllButtonsPressed;
            if (result != true)
            {
                CommonScripts.ShowError(Properties.Resources.KeyBoardErrorString);
                result = null;                
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя презентера </param>
        public static void ShowDisplayControl(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return;
            }
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            DisplayControl display = new DisplayControl(device);
            presenter.Content = display;
        }

        public static bool? TestDisplay(ScriptEnvironment environment, string contentPresenterName)
        {
            bool? result = ((environment[contentPresenterName] as ContentPresenter).Content as DisplayControl).AllChecked;
            if (result != true)
            {
                CommonScripts.ShowError(Properties.Resources.IndicationErrorString);
                result = null;
            }
            return result;
        }

        #endregion

        #region Чтение данных с прибора

        private static string ReadDataControlName;

        /// <summary>
        /// Метода запуска чтиния данных с прибора с прибора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="readDataControlSettings"> Установки для конкретного контрола </param>
        private static void StartDataReading(ScriptEnvironment environment, string contentPresenterName, ReadDataSettings readDataControlSettings = null)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return;
            }
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
                presenter = new ContentPresenter();
            ReadDataViewModel readDataViewModel = new ReadDataViewModel(environment, device/*, new NeuroMEPMicroAmplifierState()*/);
            ReadDataControl readDataControl = new ReadDataControl(readDataViewModel);
            presenter.Content = readDataControl;
            // Финт ушами при установке фильтров. Если используются программные фильтры, то их сначала нужно установить, а потом запустить чтение.
            // Если используются аппаратные фильтры, то сначала нужно запустить чтение, а потом их устанавливать.
            if (readDataControlSettings != null && (!readDataViewModel.ChannelsInfo[0].HighFreqPassBands.Contains((float)readDataControlSettings.MaxFreqFilter) || 
                                                    !readDataViewModel.ChannelsInfo[0].LowFreqPassBands.Contains((float)readDataControlSettings.MinFreqFilter)))
            {
                readDataControl.SetSettings(readDataControlSettings);
                readDataViewModel.StartReadData();
            }
            //presenter.Content = readDataControl;
            //readDataViewModel.StartReadData();
            //if (readDataControlSettings != null)
            else
            {
                readDataViewModel.StartReadData();
                readDataControl.SetSettings(readDataControlSettings);
            }
            ReadDataControlName = contentPresenterName;
        }

        /// <summary>
        /// Останавливает чтение данных с прибора
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        public static void StopDataReading(ScriptEnvironment environment, string contentPresenterName)
        {
            ((environment[contentPresenterName] as ContentPresenter).Content as ReadDataControl).ViewModel.StopReadData();
            environment.RemoveAndDispose(contentPresenterName);
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

        #endregion

        #region Проверка блока усилителей и АЦП

        #region Проверка сигнала калибровки

        /// <summary>
        /// Запуск проверки калибровочного сигнала
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartCalibrSignal(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings calibrSettings = new ReadDataSettings();
            calibrSettings.SwingRange.Min = 3.4e-3;
            calibrSettings.SwingRange.Max = 4.2e-3;
            calibrSettings.MaxFreqFilter = 250.0;
            calibrSettings.MinFreqFilter = 0.05;
            calibrSettings.XScale = new ScaleItem(20e-3f, "20 мс/мм");
            calibrSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return;
            }
            device.SetWorkMode(NeuroMEPMicroWorkMode.Kalibrovka);
            StartDataReading(environment, contentPresenterName, calibrSettings);
        }

        /// <summary>
        /// Проверка значений размаха калибровочного сигнала
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="calibrSignalSwingFirstCh"> Сохраняемое значение размаха для первого канала </param>
        /// <param name="calibrSignalSwingSecondCh"> Сохраняемое значение размаха для второго канала </param>
        /// <returns></returns>
        public static bool TestCalibrSignalSwings(ScriptEnvironment environment, string contentPresenterName, ref double calibrSignalSwingFirstCh, ref double calibrSignalSwingSecondCh)
        {
            ReadDataControl readDataControl = ((environment[contentPresenterName] as ContentPresenter).Content as ReadDataControl);
            if (readDataControl == null)
            {
                CommonScripts.ShowError("Невозможно получить значния размаха калибровочного сигнала");
                return false;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха калибровочного сигнала в канале 1");
                return false;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха калибровочного сигнала в канале 2");
                return false;
            }
            calibrSignalSwingFirstCh = readDataControl.ViewModel.StatisticsCollection[0].Swing.Value * 1e3;
            calibrSignalSwingSecondCh = readDataControl.ViewModel.StatisticsCollection[1].Swing.Value * 1e3;
            return true;
        }

        #endregion

        #region Проверка уровня шума

        /// <summary>
        /// Метод запуска тестов уровня шума
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="testingMode"> Режим тестирования шума (на диапазоне 100 мВ, 200 мкВ или проверка изолинии) </param>
        public static void StartNoiseTest(ScriptEnvironment environment, string contentPresenterName, ModeOfTestNoise testingMode)
        {
            ReadDataSettings noiseSettings = new ReadDataSettings();
            switch (testingMode)
            {
                case ModeOfTestNoise.Range100mV:
                    noiseSettings.SwingRange.Max = 19e-6;
                    noiseSettings.SwingRange.Min = 0;
                    noiseSettings.RangeIndex = 8;
                    noiseSettings.MinFreqFilter = 1.0;
                    noiseSettings.MaxFreqFilter = 20000.0;
                    noiseSettings.YScale = new ScaleItem(2e-6f, "2 мкВ/мм");
                    break;
                case ModeOfTestNoise.Range200mkV:
                    noiseSettings.SwingRange.Max = 5e-6;
                    noiseSettings.SwingRange.Min = 0;
                    noiseSettings.RangeIndex = 0;
                    noiseSettings.MinFreqFilter = 1.0;
                    noiseSettings.MaxFreqFilter = 20000.0;
                    noiseSettings.YScale = new ScaleItem(2e-6f, "2 мкВ/мм");
                    break;
                case ModeOfTestNoise.IsoLineAt005Hz:
                    //noiseSettings.SwingRange.Max = 2e-6;
                    //noiseSettings.SwingRange.Min = 0;
                    noiseSettings.AverageRange.Min = -2e-3;//-17.7e-6;
                    noiseSettings.AverageRange.Max = 2e-3;//17.7e-6;
                    noiseSettings.RangeIndex = 6;//0;
                    noiseSettings.MaxFreqFilter = 250.0;
                    noiseSettings.MinFreqFilter = 0.05;
                    noiseSettings.YScale = new ScaleItem(1e-6f, "1 мкВ/мм");
                    break;
                case ModeOfTestNoise.IsoLineAt05Hz:
                    noiseSettings.SwingRange.Max = 2e-6;
                    noiseSettings.SwingRange.Min = 0;
                    noiseSettings.AverageRange.Min = -2e-5;//-17.7e-6;
                    noiseSettings.AverageRange.Max = 2e-5;//17.7e-6;
                    noiseSettings.RangeIndex = 0;
                    noiseSettings.MaxFreqFilter = 250.0;
                    noiseSettings.MinFreqFilter = 0.5;
                    noiseSettings.YScale = new ScaleItem(1e-6f, "1 мкВ/мм");
                    break;
            }
            noiseSettings.XScale = new ScaleItem(10e-3f, "10 мс/мм");
            StandOperations.ConnectChannelsToGnd(environment);
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            device.SetWorkMode(NeuroMEPMicroWorkMode.VPTransmit);
            StartDataReading(environment, contentPresenterName, noiseSettings);
        }

        /// <summary>
        /// Перечисление режимов проверки шума в каналах ВП
        /// </summary>
        public enum ModeOfTestNoise { Range100mV, Range200mkV, IsoLineAt005Hz, IsoLineAt05Hz }

        /// <summary>
        /// Метод проверки значений размахов сигнала при проверке шума
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="noiseSignalSwingFirstCh"> Значение размаха сигнала шума для первого канала </param>
        /// <param name="noiseSignalSwingSecondCh"> Значение размаха сигнала шума для второго канала </param>
        /// <returns></returns>
        public static bool? TestNoiseSignalSwing(ScriptEnvironment environment, string contentPresenterName, ref double noiseSignalSwingFirstCh, ref double noiseSignalSwingSecondCh)
        {
            ReadDataControl readDataControl = ((environment[contentPresenterName] as ContentPresenter).Content as ReadDataControl);
            if (readDataControl == null)
            {
                CommonScripts.ShowError("Невозможно получить значния размаха сигнала шума");
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха сигнала шума в канале 1");
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха сигнала шума в канале 2");
                return null;
            }
            noiseSignalSwingFirstCh = readDataControl.ViewModel.StatisticsCollection[0].Swing.Value * 1e6;
            noiseSignalSwingSecondCh = readDataControl.ViewModel.StatisticsCollection[1].Swing.Value * 1e6;
            return true;
        }

        /// <summary>
        /// Метод проверки размахов и смещений сигнала при проверке изолинии
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="noiseSignalSwingFirstCh"> Значение размаха сигнала шума для первого канала </param>
        /// <param name="noiseSignalSwingSecondCh"> Значение размаха сигнала шума для второго канала </param>
        /// <param name="noiseAverageFirstCh"> Значение смещения сигнала для первого канала </param>
        /// <param name="noiseAverageSecCh"> Значение смещения сигнала для второго канала </param>
        /// <returns></returns>
        public static bool? TestNoiseSignalSwing(ScriptEnvironment environment, string contentPresenterName, ref double noiseSignalSwingFirstCh,
                                                 ref double noiseSignalSwingSecondCh, ref double noiseAverageFirstCh, ref double noiseAverageSecCh)
        {
            ReadDataControl readDataControl = ((environment[contentPresenterName] as ContentPresenter).Content as ReadDataControl);
            if (readDataControl == null)
            {
                CommonScripts.ShowError("Невозможно получить значния размаха сигнала шума");
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха сигнала шума в канале 1");
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха сигнала шума в канале 2");
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Average.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение смещения сигнала в канале 1");
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Average.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение смещения сигнала в канале 2");
                return null;
            }
            noiseSignalSwingFirstCh = readDataControl.ViewModel.StatisticsCollection[0].Swing.Value * 1e6;
            noiseSignalSwingSecondCh = readDataControl.ViewModel.StatisticsCollection[1].Swing.Value * 1e6;
            noiseAverageFirstCh = readDataControl.ViewModel.StatisticsCollection[0].Average.Value * 1e6;
            noiseAverageSecCh = readDataControl.ViewModel.StatisticsCollection[1].Average.Value * 1e6;
            return true;
        }

        #endregion

        #region Проверка измерения импедансов

        private static string ImpedancesControlName;

        /// <summary>
        /// Отображает контрол измерения импеданса
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowImpedancesControl(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
            {
                return;
            }
            StandOperations.Set10kMEPMicro(environment);
            NSDevicesImpedanceControl electrImpedancesControl = new NSDevicesImpedanceControl(device, new RangedValue<double>(10000.0, 9000.0, 11000.0), null);
            presenter.Content = electrImpedancesControl;
            electrImpedancesControl.Start();
            ImpedancesControlName = contentPresenterName;
        }

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
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
            {
                return;
            }
            NSDeviceSolderingControl electrodeImpControl = new NSDeviceSolderingControl(environment, device, -1.0, 101.0);
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

        #region Проверка входных диапазонов

        /// <summary>
        /// Метод запуска тестирования входных диапазонов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartTestInputRanges(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return;
            }
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            TestInputRangesViewModel testRangesViewModel = new TestInputRangesViewModel(environment, device, new NeuroMEPMicroAmplifierState());
            TestInputRangesControl testRangesDataControl = new TestInputRangesControl(testRangesViewModel);
            presenter.Content = testRangesDataControl;
            testRangesDataControl.StartTest();
        }

        /// <summary>
        /// Метод, проверящий значения размаха сигнала и сохраняющий значения
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="firstChannelSwings"> Массив для хранения размахов сигнала первого канала </param>
        /// <param name="secondChannelSwings"> Массив для хранения размахов сигнала второго канала </param>
        /// <returns> Результаты проверки </returns>
        public static bool? CheckAndSaveInputRangeResults(ScriptEnvironment environment, string contentPresenterName, double[] firstChannelSwings, double[] secondChannelSwings)
        {
            TestInputRangesControl inputRangesControl = ((environment[contentPresenterName] as ContentPresenter).Content as TestInputRangesControl);
            if (inputRangesControl == null)
            {
                CommonScripts.ShowError(Properties.Resource1.UnableGetSwingsValues);
                return null;
            }
            inputRangesControl.ViewModel.StopReadData();
            if (inputRangesControl.ViewModel.IsTesting || inputRangesControl.ViewModel.TestWasLaunched)
            {
                CommonScripts.ShowError(Properties.Resource1.TestingNotFinished);
                StopTestInputRanges(environment, contentPresenterName);
                return null;
            }
            for (int i = 0; i < inputRangesControl.ViewModel.InputRangeTests.Count; i++)
            {
                if (!inputRangesControl.ViewModel.InputRangeTests[i].IsValidValues)
                {
                    CommonScripts.ShowError(String.Format(Properties.Resource1.InputRagesTestInvalidMessage,
                        inputRangesControl.ViewModel.InputRangeTests[i].ChannelsSwings[0].IsValidValue ? 2 : 1, inputRangesControl.ViewModel.InputRangeTests[i].Range, inputRangesControl.ViewModel.InputRangeTests[i].SwingString));
                    return null;
                }
                firstChannelSwings[i] = Math.Round(inputRangesControl.ViewModel.InputRangeTests[i].ChannelsSwings[0].Value * 1e3, 3);
                secondChannelSwings[i] = Math.Round(inputRangesControl.ViewModel.InputRangeTests[i].ChannelsSwings[1].Value * 1e3, 3);
            }
            return true;
        }

        /// <summary>
        /// Заканчивает тест входных диапазонов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StopTestInputRanges(ScriptEnvironment environment, string contentPresenterName)
        {
            environment.RemoveAndDispose(contentPresenterName);
        }

        #endregion

        #region Проверка аппаратных фильтров

        /// <summary>
        /// Запуск проверки аппаратных фильтров
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="modeTestFilters"> Режим проверки фильтров </param>
        public static void StartTestFilters(ScriptEnvironment environment, string contentPresenterName, ModeOfTestFilters modeTestFilters)
        {
            ReadDataSettings testFiltersSettings = new ReadDataSettings();
            float testingFrequency = 0;
            switch (modeTestFilters)
            {
                case ModeOfTestFilters.HighFreq_005_Hz:
                    testingFrequency = 0.05f;
                    testFiltersSettings.XScale = new ScaleItem(1f, "1 с/мм");
                    testFiltersSettings.YScale = new ScaleItem(5e-3f, "5 мВ/мм");
                    testFiltersSettings.TickIntervalsProcessing = 40;
                    break;
                case ModeOfTestFilters.HighFreq_05_Hz:
                    testingFrequency = 0.5f;
                    testFiltersSettings.XScale = new ScaleItem(100e-3f, "0,1 с/мм");
                    testFiltersSettings.YScale = new ScaleItem(5e-3f, "5 мВ/мм");
                    testFiltersSettings.TickIntervalsProcessing = 4;
                    break;
                case ModeOfTestFilters.HighFreq_160_Hz:
                    testingFrequency = 160f;
                    testFiltersSettings.XScale = new ScaleItem(1e-3f, "1 мс/мм");
                    testFiltersSettings.YScale = new ScaleItem(5e-3f, "5 мВ/мм");
                    break;
                case ModeOfTestFilters.LowFreq_250_Hz:
                    testingFrequency = 250.0f;
                    testFiltersSettings.XScale = new ScaleItem(1e-3f, "1 мс/мм");
                    testFiltersSettings.YScale = new ScaleItem(5e-3f, "5 мВ/мм");
                    break;
                case ModeOfTestFilters.LowFreq_10000_Hz:
                    testingFrequency = 10000.0f;
                    testFiltersSettings.XScale = new ScaleItem(20e-6f, "20 мкс/мм");
                    testFiltersSettings.YScale = new ScaleItem(5e-3f, "5 мВ/мм");
                    break;
            }
            testFiltersSettings.SwingRange.Max = 75.0e-3;
            testFiltersSettings.SwingRange.Min = 66.8e-3;
            testFiltersSettings.RangeIndex = 8;
            if (modeTestFilters == ModeOfTestFilters.LowFreq_10000_Hz || modeTestFilters == ModeOfTestFilters.LowFreq_250_Hz)
            {
                testFiltersSettings.MinFreqFilter = 0.5;
                testFiltersSettings.MaxFreqFilter = testingFrequency;
            }
            else
            {
                testFiltersSettings.MinFreqFilter = testingFrequency;
                testFiltersSettings.MaxFreqFilter = 10000.0;
            }
            if (testingFrequency == 0.5f)
                StandOperations.SetGeneratorState(environment, 0.025f, testingFrequency + 0.05f);
            else
                if (testingFrequency == 250.0f)
                    StandOperations.SetGeneratorState(environment, 0.025f, testingFrequency - 20.0f);
                else
                    StandOperations.SetGeneratorState(environment, 0.025f, testingFrequency);
            StandOperations.SetDifferentialSignalCommutatorState(environment);
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            device.SetWorkMode(NeuroMEPMicroWorkMode.VPTransmit);
            StartDataReading(environment, contentPresenterName, testFiltersSettings);
        }

        /// <summary>
        /// Проверка размахов сигнала при тестировании аппаратных фильтров
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        /// <param name="noiseSignalSwingFirstCh"> Размах сигнала в первом канале </param>
        /// <param name="noiseSignalSwingSecondCh"> Размах сигнала во втором канале </param>
        /// <returns></returns>
        public static bool? TestAndSaveSignalSwing(ScriptEnvironment environment, string contentPresenterName, ref double SignalSwingFirstCh, ref double SignalSwingSecondCh, double savingScale = 1e3)
        {
            ReadDataControl readDataControl = ((environment[contentPresenterName] as ContentPresenter).Content as ReadDataControl);
            if (readDataControl == null)
            {
                CommonScripts.ShowError("Невозможно получить значния размаха сигнала");
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха сигнала в канале 1");
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Swing.IsValidValue)
            {
                CommonScripts.ShowError("Недопустимое значение размаха сигнала в канале 2");
                return null;
            }
            SignalSwingFirstCh = readDataControl.ViewModel.StatisticsCollection[0].Swing.Value * savingScale;
            SignalSwingSecondCh = readDataControl.ViewModel.StatisticsCollection[1].Swing.Value * savingScale;
            return true;
        }

        /// <summary>
        /// Перечисление режимов проверки аппарвтных фильтров
        /// </summary>
        public enum ModeOfTestFilters { LowFreq_250_Hz, LowFreq_10000_Hz, HighFreq_005_Hz, HighFreq_05_Hz, HighFreq_160_Hz, }

        #endregion

        #region Проверка подавления синфазной помехи

        /// <summary>
        /// Запуск проверки подавления синфазной помехи
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartSynphNoiseTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings testSynphNoiseSettings = new ReadDataSettings();
            testSynphNoiseSettings.SwingRange.Max = 1e-5;
            testSynphNoiseSettings.SwingRange.Min = 0;
            testSynphNoiseSettings.RangeIndex = 0;
            testSynphNoiseSettings.MinFreqFilter = 1.0;//0.5;
            testSynphNoiseSettings.MaxFreqFilter = 250.0f;
            testSynphNoiseSettings.XScale = new ScaleItem(2e-3f, "2 мс/мм");
            testSynphNoiseSettings.YScale = new ScaleItem(1e-6f, "1 мкВ/мм");
            StandOperations.SetGeneratorState(environment, 0.5f, 50.0f);
            StandOperations.SetCommutatorSynphSignal(environment);
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            device.SetWorkMode(NeuroMEPMicroWorkMode.VPTransmit);
            StartDataReading(environment, contentPresenterName, testSynphNoiseSettings);
        }

        #endregion

        #region Проверка входного импеданса усилителей

        private static string InputImpedanceControlName;

        /// <summary>
        /// Запускает тест проверки входных импедансов усилителей
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера контрола проверки импедансов </param>
        /// <param name="positiveElectrodes"> Проверяемый электрод </param>
        public static void StartTestInputImpedances(ScriptEnvironment environment, string contentPresenterName, bool positiveElectrodes = true)
        {
            StandOperations.SetCommutatorSyncPhaseImpedance(environment, positiveElectrodes);
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return;
            }
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            TestInputImpedanceViewModel testInputImpViewModel = new TestInputImpedanceViewModel(environment, device, device.GetDeviceState().GetState(typeof(NeuroMEPMicroAmplifierState)) as NeuroMEPMicroAmplifierState);
            TestImputImpedanceControl testInputImpsControl = new TestImputImpedanceControl(testInputImpViewModel);
            presenter.Content = testInputImpsControl;
            InputImpedanceControlName = contentPresenterName;
            testInputImpViewModel.StartTest();
        }

        /// <summary>
        /// Метод, производящий проверку синфазного входного сопротивления усилителей.
        /// </summary>
        /// <param name="environment">Переменная окружения скрипта</param>
        /// <param name="isPositive">Параметр, указывающий, проверка какого входа сейчас осуществляется (true - положительного, false - отрицательного.)</param>
        /// <param name="resistanceValue1">Возвращаемое значение сопротивления для первого канала (ГОм)</param>
        /// <param name="resistanceValue2">Возвращаемое значение сопротивления для второго канала (ГОм)</param>
        /// <returns></returns>
        public static bool? CheckInputResistancesRange(ScriptEnvironment environment, bool isPositive, ref double resistanceValue1, ref double resistanceValue2)
        {
            string signStr;
            TestImputImpedanceControl inputImpedanceControl = null;
            if (InputImpedanceControlName != null)
                inputImpedanceControl = (environment[InputImpedanceControlName] as ContentPresenter).Content as TestImputImpedanceControl;
            if (isPositive)
                signStr = "+";
            else
                signStr = "-";
            foreach (var rowInfo in inputImpedanceControl.ViewModel.Rows)
            {
                if (!rowInfo.Resistance.IsValidValue)
                {
                    CommonScripts.ShowError("Недопустимое значение входного сопротивления входа (" + rowInfo.Channel + signStr + ") усилителя");
                    return null;
                }
            }
            resistanceValue1 = inputImpedanceControl.ViewModel.Rows[0].Resistance.Value * 1e-6;
            resistanceValue2 = inputImpedanceControl.ViewModel.Rows[1].Resistance.Value * 1e-6;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="isPositive"></param>
        /// <param name="capacityValue1"></param>
        /// <param name="capacityValue2"></param>
        /// <returns></returns>
        public static bool? CheckInputCapacitiesRange(ScriptEnvironment environment, bool isPositive, ref double capacityValue1, ref double capacityValue2)
        {
            string signStr;
            TestImputImpedanceControl inputImpedanceControl = null;
            if (InputImpedanceControlName != null)
                inputImpedanceControl = (environment[InputImpedanceControlName] as ContentPresenter).Content as TestImputImpedanceControl;
            if (isPositive)
                signStr = "+";
            else
                signStr = "-";
            foreach (var rowInfo in inputImpedanceControl.ViewModel.Rows)
            {
                if (!rowInfo.Capacity.IsValidValue)
                {
                    CommonScripts.ShowError("Недопустимое значение входной емкости входа (" + rowInfo.Channel + signStr + ") усилителя");
                    return null;
                }
            }
            capacityValue1 = inputImpedanceControl.ViewModel.Rows[0].Capacity.Value * 1e12;
            capacityValue2 = inputImpedanceControl.ViewModel.Rows[1].Capacity.Value * 1e12;
            return true;
        }

        /// <summary>
        /// Заканчивает проверку входных импедансов усилителей
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopTestInputImpedances(ScriptEnvironment environment)
        {
            if (InputImpedanceControlName != null)
            {
                ((environment[InputImpedanceControlName] as ContentPresenter).Content as TestImputImpedanceControl).ViewModel.StopReadData();
                environment.RemoveAndDispose(InputImpedanceControlName);
            }
        }

        #endregion

        #endregion

        #region Проверка стимуляторов

        #region Проверка фотостимулятора

        /// <summary>
        /// Запускает фотостимулятор с заданными параметрами
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="duration"> Длительность стимула </param>
        /// <param name="period"> Период стимуляции </param>
        public static void StartPhotoStimulation(ScriptEnvironment environment, double duration, double period)
        {
            NeuroMEPMicro device = OpenDevice(environment);
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
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            PhotoStimulatorControl photoStimulatorControl = new PhotoStimulatorControl(device as NeuroSoft.Devices.IFlashStimulator);
            presenter.Content = photoStimulatorControl;
        }

        /// <summary>
        /// Останавливает фотостимуляцию
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopPhotoStimulation(ScriptEnvironment environment)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            (device as NeuroSoft.Devices.IFlashStimulator).StopStimulation();
            (device as NeuroSoft.Devices.IFlashStimulator).SetEnabled(false);
        }

        #endregion

        #region Проверка фоностимулятора

        /// <summary>
        /// Запускает фоностимулятор
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="duration"> Длительность стимулов </param>
        /// <param name="period"> Период стимуляции </param>
        /// <param name="amplitude"> Амплитуда стимула </param>
        public static void StartPhonoStimulation(ScriptEnvironment environment, double duration, double period, double amplitude = 110.0)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            (device as NeuroSoft.Devices.IAudioStimulator).SetStimulusDuration((float)duration);
            (device as NeuroSoft.Devices.IAudioStimulator).SetFrequency((float)(1 / period));
            (device as NeuroSoft.Devices.IAudioStimulator).SetStimulusAmplitude((float)amplitude);
            (device as NeuroSoft.Devices.IAudioStimulator).SetEnabled(true);
            (device as NeuroSoft.Devices.IAudioStimulator).StartStimulation();
        }

        /// <summary>
        /// Отображает контрол с установками фоностимулятора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void ShowPhonoStimControl(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            PhonoStimulatorControl photoStimulatorControl = new PhonoStimulatorControl(device as NeuroSoft.Devices.IAudioStimulator);
            presenter.Content = photoStimulatorControl;
        }

        /// <summary>
        /// Останавливает фоностимуляцию
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void StopPhonoStimulation(ScriptEnvironment environment)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
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
        public static void StartCurrentStimulation(ScriptEnvironment environment, double duration,  double period, double amplitude, bool isPositivePolarity = true)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            /* Закрытие-открытие добавлено для успешного запуска токового стимулятора после тестов усилителя */
            device.Close();
            device.Open();
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
            NeuroMEPMicro device = OpenDevice(environment);
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
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (currentStimPresenterName != null && environment[currentStimPresenterName] != null)
            {
                CurrentStimulatorControl currStimControl = (environment[currentStimPresenterName] as ContentPresenter).Content as CurrentStimulatorControl;
                if (currStimControl != null)
                {
                    currStimControl.StopStimulation();
                    environment.RemoveAndDispose(currentStimPresenterName);
                    return;
                }
            }
            (device as NeuroSoft.Devices.ICurrentStimulator).StopStimulation();
            (device as NeuroSoft.Devices.ICurrentStimulator).SetEnabled(false);
        }

        #endregion

        #region Проверка шахматного паттерна
        /// <summary>
        /// Имя контент презентера контрола паттерн-стимулятора
        /// </summary>
        private static string patternStimPresenterName;
        /// <summary>
        /// Запуск стимуляции шахматным паттерном
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера контрола паттерн-стимулятора </param>
        public static void StartPatternStimulation(ScriptEnvironment environment, string contentPresenterName)
        {
            NeuroMEPMicro device = OpenDevice(environment);
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
            PatternStimualtorControl patternStimControl = (environment[patternStimPresenterName] as ContentPresenter).Content as PatternStimualtorControl;
            if (patternStimControl != null)
            {
                patternStimControl.StopStimulation();
                environment.RemoveAndDispose(patternStimPresenterName);
            }
        }

        #endregion

        #region Проверка синхроимпульса

        /// <summary>
        /// Запуск проверки калибровочного сигнала
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера </param>
        public static void StartSyncInpTest(ScriptEnvironment environment, string contentPresenterName)
        {
            ReadDataSettings synchroInSettings = new ReadDataSettings();
            synchroInSettings.SwingRange.Min = 3.4e-3;
            synchroInSettings.SwingRange.Max = 4.2e-3;
            synchroInSettings.MaxFreqFilter = 250.0;
            synchroInSettings.MinFreqFilter = 0.05;
            synchroInSettings.XScale = new ScaleItem(20e-3f, "20 мс/мм");
            synchroInSettings.YScale = new ScaleItem(200e-6f, "200 мкВ/мм");
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                CommonScripts.ShowError("Прибор не подключен к компьютеру");
                return;
            }
            if (device is NeuroSoft.Devices.ICurrentStimulator)
            {
                NeuroMEPMicroCurrentStimulatorState currStimState = (NeuroMEPMicroCurrentStimulatorState)(device as NeuroSoft.Devices.ICurrentStimulator).GetState();
                currStimState.TrigInEnabled = true;
                (device as NeuroSoft.Devices.ICurrentStimulator).SetState(currStimState);
            }
            device.SetWorkMode(NeuroMEPMicroWorkMode.Kalibrovka);
            StartDataReading(environment, contentPresenterName, synchroInSettings);
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
            StopDataReading(environment, ReadDataControlName);
            if (readDataControl.EventsCounter == 0)
            {
                CommonScripts.ShowError(Properties.Resource1.NotSynchroImpulse);
                return null;
            }
            return true;
        }

        #endregion

        #endregion
    }
    /// <summary>
    /// Перечисление режимов проверки шума в каналах ВП
    /// </summary>
    public enum ModesOfTestNoise { Range100mV, Range200mkV, IsoLineAt005Hz, IsoLineAt05Hz }
}
