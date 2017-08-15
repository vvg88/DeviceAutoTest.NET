using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices;
using NeuroSoft.Hardware.Tools.Usb;
using NeuroSoft.Hardware.Applications.CypressLoader;
using System.IO;
using System.Windows.Controls;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.Hardware.Devices;
using System.Windows;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls;


namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts
{
    public static class NeuroMEPMicroScripts
    {
        #region Программирование контроллеров

        #region Программирование PIC'ов

        /// <summary>
        /// сохраняет имя файла прошивки PIC контроллера
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="fileNameTemplate"> Часть имени нужного файла прошивки </param>
        /// <param name="fileName">Имя файла, с помощью которого осущствлялось программирование </param>
        /// <returns></returns>
        public static bool? ProgramChipProg(ScriptEnvironment environment, string fileNameTemplate, ref string fileName)
        {
            //string fileNameTemplate = "PIC18F252_r*_d*.hex";
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "HEX files|" + fileNameTemplate + "*.hex";
            openFileDialog.InitialDirectory = FirmwareFolder;
            openFileDialog.FileName = GetFirmwareFileName(fileNameTemplate + "*");
            if (openFileDialog.ShowDialog() != true)
            {
                return null;
            }
            fileName = GetShortFileName(openFileDialog.FileName);
            return true;
        }

        #endregion

        #region Программирование USB контроллера

        public static bool? DoFirmwareEeprom(ScriptEnvironment environment, ref string fileName)
        {
            SelectDeviceDialog selectDeviceDialog = new SelectDeviceDialog();
            if (selectDeviceDialog.ShowDialog() == true)
            {
                string fileNameTemplate = "НС-5_ver*.iic";
                string firmwareFolder = @"\\SERVER\firmware\_Common\Прошивки для приборов переделанных с CY7C64613-80 на CY7C68013A-100AXI\";
                UsbDevicesListControl.Item usbItem = selectDeviceDialog.SelectedDeviceItem;
                var weavingInformation = new ProgramDescriptor();
                //weavingInformation.SerialNumberBuilder = new SerialNumberBuilderString(SerialNumberBuilderString.StringType.StringUsbDescriptor, 126);
                weavingInformation.SerialNumberBuilder = new SerialNumberBuilderNumeric(SerialNumberBuilderNumeric.NumericType.B2, false);
                weavingInformation.Address = 2060;
                weavingInformation.PID_Enable = true;
                weavingInformation.PID_Addr = 2058;
                weavingInformation.PID_Value = 33376;
                weavingInformation.CypressCpuType = Hardware.Tools.CypressTools.CypressCpuType.FX1_FX2LP;

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
                    fileName = GetShortFileName(weavingInformation.FileName);
                    return true;
                }
            }
            return null;
        }

        #endregion

        #region Вспомогательные функции программирования

        /// <summary>
        /// Возвращает название первого найденного файла прошивки, удовлетворяющего критерию поиска
        /// </summary>
        /// <param name="pattern"> Часть имени файла</param>
        /// <returns> Имя первого обнаруженного файла, удовлетворяющего условиям </returns>
        private static string GetFirmwareFileName(string pattern)
        {
            string[] files = Directory.GetFiles(FirmwareFolder, pattern, SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                return files[0];
            }
            return null;
        }
        /// <summary>
        /// Папка с прошивками
        /// </summary>
        internal static string FirmwareFolder
        {
            get
            {
                return @"\\SERVER\firmware\Нейро-МВП-микро\";
            }
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
                    environment.InitDevice(new NeuroMEPMicroWrapper(device));
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
                    return new ValidationResult(false, Properties.NeuroMEPMicroTestResources.CantOpenStand);
                }
            }
            finally
            {
                environment.CloseStand();
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
                NSMessageBox.Show("Прибор не подключен к компьютеру", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            DeviceInformationControl deviceInfoControl = new DeviceInformationControl(environment);
            presenter.Content = deviceInfoControl;
        }

        public static bool? TestDeviceInformation(ScriptEnvironment environment, string contentPresenterName)
        {
            string errorMessage;
            bool? devInfoResult = ((environment[contentPresenterName] as ContentPresenter).Content as DeviceInformationControl).CheckDeviceInformationValid(out errorMessage);
            if (devInfoResult != true)
                NSMessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                NSMessageBox.Show("Прибор не подключен к компьютеру", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                NSMessageBox.Show(Properties.NeuroMEPMicroTestResources.KeyBoardErrorString, Properties.NeuroMEPMicroTestResources.ErrorString, MessageBoxButton.OK, MessageBoxImage.Error);
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
                NSMessageBox.Show("Прибор не подключен к компьютеру", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                NSMessageBox.Show(Properties.NeuroMEPMicroTestResources.IndicationErrorString, Properties.NeuroMEPMicroTestResources.ErrorString, MessageBoxButton.OK, MessageBoxImage.Error);
                result = null;
            }
            return result;
        }

        #endregion

        #region Чтение данных с прибора

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
                NSMessageBox.Show("Прибор не подключен к компьютеру", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            if (presenter == null)
                presenter = new ContentPresenter();
            ReadDataViewModel readDataViewModel = new ReadDataViewModel(environment, device, new NeuroMEPMicroAmplifierState());
            ReadDataControl readDataControl = new ReadDataControl(readDataViewModel);
            // Финт ушами при установке фильтров. Если используются программные фильтры, то их сначала нужно установить, а потом запустить чтение.
            // Если используются аппаратные фильтры, то сначала нужно запустить чтение, а потом их устанавливать.
            if (readDataControlSettings != null && (!readDataViewModel.ChannelsInfo[0].HighFreqPassBands.Contains((float)readDataControlSettings.MaxFreqFilter) || 
                                                    !readDataViewModel.ChannelsInfo[0].LowFreqPassBands.Contains((float)readDataControlSettings.MinFreqFilter)))
            {
                readDataControl.SetSettings(readDataControlSettings);
            }
            presenter.Content = readDataControl;
            readDataViewModel.StartReadData();
            if (readDataControlSettings != null)
            {
                readDataControl.SetSettings(readDataControlSettings);
            }
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
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            if (!device.Detect())
            {
                NSMessageBox.Show("Прибор не подключен к компьютеру", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                NSMessageBox.Show("Невозможно получить значния размаха калибровочного сигнала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение размаха калибровочного сигнала в канале 1", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Swing.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение размаха калибровочного сигнала в канале 2", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    break;
                case ModeOfTestNoise.Range200mkV:
                    noiseSettings.SwingRange.Max = 5e-6;
                    noiseSettings.SwingRange.Min = 0;
                    noiseSettings.RangeIndex = 0;
                    noiseSettings.MinFreqFilter = 1.0;
                    noiseSettings.MaxFreqFilter = 20000.0;
                    break;
                case ModeOfTestNoise.IsoLineAt005Hz:
                    noiseSettings.SwingRange.Max = 2e-6;
                    noiseSettings.SwingRange.Min = 0;
                    noiseSettings.AverageRange.Min = -17.7e-6;
                    noiseSettings.AverageRange.Max = 17.7e-6;
                    noiseSettings.RangeIndex = 0;
                    noiseSettings.MaxFreqFilter = 250.0;
                    noiseSettings.MinFreqFilter = 0.05;
                    break;
                case ModeOfTestNoise.IsoLineAt05Hz:
                    noiseSettings.SwingRange.Max = 2e-6;
                    noiseSettings.SwingRange.Min = 0;
                    noiseSettings.AverageRange.Min = -17.7e-6;
                    noiseSettings.AverageRange.Max = 17.7e-6;
                    noiseSettings.RangeIndex = 0;
                    noiseSettings.MaxFreqFilter = 250.0;
                    noiseSettings.MinFreqFilter = 0.5;
                    break;
            }
            StandOperations.ConnectChannelsToGnd(environment);
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            device.SetWorkMode(NeuroMEPMicroWorkMode.VPTransmit);
            StartDataReading(environment, contentPresenterName, noiseSettings);
        }

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
                NSMessageBox.Show("Невозможно получить значния размаха сигнала шума", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение размаха сигнала шума в канале 1", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Swing.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение размаха сигнала шума в канале 2", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                NSMessageBox.Show("Невозможно получить значния размаха сигнала шума", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение размаха сигнала шума в канале 1", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Swing.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение размаха сигнала шума в канале 2", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Average.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение смещения сигнала в канале 1", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Average.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение смещения сигнала в канале 2", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            noiseSignalSwingFirstCh = readDataControl.ViewModel.StatisticsCollection[0].Swing.Value * 1e6;
            noiseSignalSwingSecondCh = readDataControl.ViewModel.StatisticsCollection[1].Swing.Value * 1e6;
            noiseAverageFirstCh = readDataControl.ViewModel.StatisticsCollection[0].Average.Value * 1e6;
            noiseAverageSecCh = readDataControl.ViewModel.StatisticsCollection[1].Average.Value * 1e6;
            return true;
        }

        public enum ModeOfTestNoise { Range100mV, Range200mkV, IsoLineAt005Hz, IsoLineAt05Hz }

        #endregion

        #region Проверка измерения импедансов

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
            ElectrodeImpedancesControl electrodeImpControl = new ElectrodeImpedancesControl(environment, device, 9000.0, 11000.0);
            electrodeImpControl.StartTestSoldering += new StartTestSolderingEventHandler(electrodeImpControl_StartTestImpedances);
            presenter.Content = electrodeImpControl;
            electrodeImpControl.StartTesting();
        }

        /// <summary>
        /// Обработчик события переключения текущего измеряемого электрода
        /// </summary>
        /// <param name="sender"> Источник </param>
        /// <param name="electrode"> Номер проверяемого электрода </param>
        private static void electrodeImpControl_StartTestImpedances(object sender, int electrode)
        {
            var electrImpControl = sender as ElectrodeImpedancesControl;
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
        public static bool? CheckAndSaveImpedances(ScriptEnvironment environment, string contentPresenterName, ref double[] electrodeImpedances)
        {
            ElectrodeImpedancesControl impControl = ((environment[contentPresenterName] as ContentPresenter).Content as ElectrodeImpedancesControl);
            foreach (var impInfo in impControl.SavedImpedances)
            {
                if (!impInfo.Resistance.IsValidValue)
                {
                    NSMessageBox.Show(String.Format("{0} имеет недопустимое значение сопротивления. Внесите необходимые исправления и посторите тест", impInfo.Title), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            for (int i = 0; i < impControl.SavedImpedances.Count; i++)
            {
                if (i != 0)
                {
                    electrodeImpedances[i - 1] = impControl.SavedImpedances[i].Resistance.Value;
                }
            }
            return true;
        }

        /// <summary>
        /// Заврешение проверки измерения импедансов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="contentPresenterName"> Имя контент презентера контрола проверки импедансов </param>
        public static void StopElectrodImpedancesCheck(ScriptEnvironment environment, string contentPresenterName)
        {
            environment.RemoveAndDispose(contentPresenterName);
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
                NSMessageBox.Show("Прибор не подключен к компьютеру", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                NSMessageBox.Show("Не удалось получить значения размахов сигнала", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
            for (int i = 0; i < inputRangesControl.ViewModel.InputRangeTests.Count; i++)
            {
                if (!inputRangesControl.ViewModel.InputRangeTests[i].IsValidValues)
                {
                    NSMessageBox.Show(String.Format("Размах сигнала в канале {0} на диапазоне {1} не соответствует требуемому значению {2}.",
                        inputRangesControl.ViewModel.InputRangeTests[i].ChannelOneSwing.IsValidValue ? 2 : 1, inputRangesControl.ViewModel.InputRangeTests[i].Range, inputRangesControl.ViewModel.InputRangeTests[i].SwingString),
                        "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return null;
                }
                firstChannelSwings[i] = Math.Round(inputRangesControl.ViewModel.InputRangeTests[i].ChannelOneSwing.Value * 1e3, 3);
                secondChannelSwings[i] = Math.Round(inputRangesControl.ViewModel.InputRangeTests[i].ChannelTwoSwing.Value * 1e3, 3);
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
                    testFiltersSettings.TickIntervalsProcessing = 40;
                    break;
                case ModeOfTestFilters.HighFreq_05_Hz:
                    testingFrequency = 0.5f;
                    testFiltersSettings.TickIntervalsProcessing = 4;
                    break;
                case ModeOfTestFilters.HighFreq_160_Hz:
                    testingFrequency = 160f;
                    break;
                case ModeOfTestFilters.LowFreq_250_Hz:
                    testingFrequency = 250.0f;
                    break;
                case ModeOfTestFilters.LowFreq_10000_Hz:
                    testingFrequency = 10000.0f;
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
        public static bool? TestAndSaveSignalSwing(ScriptEnvironment environment, string contentPresenterName, ref double SignalSwingFirstCh, ref double SignalSwingSecondCh)
        {
            ReadDataControl readDataControl = ((environment[contentPresenterName] as ContentPresenter).Content as ReadDataControl);
            if (readDataControl == null)
            {
                NSMessageBox.Show("Невозможно получить значния размаха сигнала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[0].Swing.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение размаха сигнала в канале 1", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            if (!readDataControl.ViewModel.StatisticsCollection[1].Swing.IsValidValue)
            {
                NSMessageBox.Show("Недопустимое значение размаха сигнала в канале 2", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            SignalSwingFirstCh = readDataControl.ViewModel.StatisticsCollection[0].Swing.Value * 1e3;
            SignalSwingSecondCh = readDataControl.ViewModel.StatisticsCollection[1].Swing.Value * 1e3;
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
            testSynphNoiseSettings.MinFreqFilter = 1.0;
            testSynphNoiseSettings.MaxFreqFilter = 250.0f;
            StandOperations.SetGeneratorState(environment, 1.0f, 50.0f);
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
                NSMessageBox.Show("Прибор не подключен к компьютеру", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                    NSMessageBox.Show("Недопустимое значение входного сопротивления входа (" + rowInfo.Channel + signStr + ") усилителя", Properties.NeuroMEPMicroTestResources.ErrorString, 
                                       MessageBoxButton.OK, MessageBoxImage.Error);
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
                    NSMessageBox.Show("Недопустимое значение входной емкости входа (" + rowInfo.Channel + signStr + ") усилителя", Properties.NeuroMEPMicroTestResources.ErrorString,
                                       MessageBoxButton.OK, MessageBoxImage.Error);
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
                environment.RemoveAndDispose(InputImpedanceControlName);
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
        public static void StartPhonoStimulation(ScriptEnvironment environment, double duration, double period)
        {
            NeuroMEPMicro device = OpenDevice(environment);
            if (device == null)
                return;
            (device as NeuroSoft.Devices.IAudioStimulator).SetStimulusDuration((float)duration);
            (device as NeuroSoft.Devices.IAudioStimulator).SetFrequency((float)(1 / period));
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

        #endregion

        #endregion
    }
}
