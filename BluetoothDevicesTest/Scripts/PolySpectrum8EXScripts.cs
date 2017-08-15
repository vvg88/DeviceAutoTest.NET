using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.DeviceAutoTest.Common.Scripts;
using NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Controls;
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Scripts
{
    /// <summary>
    /// Набор скриптов для наладки прибора Poly-Spectrum-8EX
    /// </summary>
    public static class PolySpectrum8EXScripts
    {
        #region LPC_Programming

        /// <summary>
        /// Сохраняет имя файла прошивки LPC контроллера
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="fileNameTemplate"> Часть имени нужного файла прошивки </param>
        /// <param name="fileName">Имя файла, с помощью которого осуществлялось программирование </param>
        public static bool? GetLPCFileName(ScriptEnvironment environment, string fileNameTemplate, ref string fileName)
        {
            return CommonScripts.ProgramChipProg(environment, fileNameTemplate, FirmwareFolder, ref fileName);
        }

        internal static string FirmwareFolder
        {
            get
            {
                string fwFolder = @"\\Server\firmware\[017] ПС-8ЕХ\";
                return Directory.Exists(fwFolder) ? fwFolder : @"C:\";
            }
        }

        #endregion

        #region Bluetooth_View

        /// <summary>
        /// Выводит список Bluetooth-устройств
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="deviceNameTemp"> Часть имени нужного устройства </param>      
        public static void ViewBluetoothDevices(ScriptEnvironment environment, string deviceNameTemp, string contentPresenterName)
        {
            SelectBTDevices selectBTDevice = new SelectBTDevices(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            presenter.Content = selectBTDevice;
            selectBTDevice.nameTemp = deviceNameTemp;
        }

        public static bool? SelectedDevice(ScriptEnvironment environment)
        {
            SelectBTDevices selectBTDevice = new SelectBTDevices(environment);
            if (environment["nameBTDevice"] != null)
            {
                return true;
            }
            else return false;
        }

        // проверяем, что подключен PS-8EX v1
        public static void TestPS8EX(ScriptEnvironment environment)
        {
            if ((SelectBTDevices.nameBTDevice.IndexOf(PolySpectrum8EX.DeviceNameSign) > -1)
                && (SelectBTDevices.nameBTDevice.IndexOf("v1") > -1))
            {
                StandOperation.BreakElectrode(environment, 10, true);
                StandOperation.BreakElectrode(environment, 11, true);
            }
        }

        #endregion

        #region Monitoring_Data

        /// <summary>
        /// Выводит окно мониторинга данных с Bluetooth-устройства
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void ViewMonitoringData(ScriptEnvironment environment, string contentPresenterName, MonitoringType type)
        {
            MonitoringData monitoringData = new MonitoringData(environment, type);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            presenter.Content = monitoringData;
        }

        #endregion

        /// <summary>
        /// Выводим окно обрывов электродов
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        public static void ViewBreakeData(ScriptEnvironment environment, string contentPresenterName)
        {
            BreakeElectrode breakeElectrode = new BreakeElectrode(environment);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            presenter.Content = breakeElectrode;
        }

        static float[] paceMakerWaveFormArray = new float[2048];
        /// <summary>
        /// Проверка PaceMaker
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="contentPresenterName"></param>
        public static void ViewPaceMaker(ScriptEnvironment environment, string contentPresenterName, MonitoringType type)
        {
            paceMakerWaveFormArray = GeneratePaceMaker();
            MonitoringData monitoringData = new MonitoringData(environment, type);
            ContentPresenter presenter = environment[contentPresenterName] as ContentPresenter;
            presenter.Content = monitoringData;

            UniversalTestStand stand = environment.Stand;
            // загружаем в генератор свою форму сигнала
            stand.Generators[0].SetNonstandardWaveForm(paceMakerWaveFormArray);
            stand.Generators[0].StandardWaveForm = false;
        }

        static float[] GeneratePaceMaker()
        {
            float[] res = new float[2048];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = res[i] + 1.0f * (float)System.Math.Sin((2 * System.Math.PI * i) / res.Length);
                if (i < 100) res[i] = 0;
            }
            return res;
        }
    }

    class TypeMonitoring
    {
        public MonitoringType monitoringType
        {
            get { return monitoringType; }
            set
            {
                monitoringType = value;
            }
        }
    }

    public enum MonitoringType
    {
        /// <summary>
        /// Данные ЭКГ
        /// </summary>
        ECG_DATA,
        /// <summary>
        /// Измерение шума
        /// </summary>
        NOISE_DATA,
        /// <summary>
        /// Измерение подавления синфазного сигнала
        /// </summary>
        CMRR_DATA,
        /// <summary>
        /// Измерение канала дыхания
        /// </summary>
        BR_DATA,
        /// <summary>
        /// Данные о pacemakers
        /// </summary>
        PACE_DATA
    }
}
