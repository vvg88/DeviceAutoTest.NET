using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.Common.Scripts
{
    public static class StandOperationsEEG
    {
        /// <summary>
        /// Подключение каналов ЭЭГ и ЭКГ к земле прибора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void ConnectChannelsTo10K(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            // Установка отведений ЭЭГ
            for (int i = 0; i < 32; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._10kOhm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.GND;
            }
            // Установка SH1 и REF
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;
            // Установка ЭКГ-, ЭКГ+ и SH2
            stand.CommutatorSetting.ChannelsState[36].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[36].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[38].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[38].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[39].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[39].TestSignal = CommutatorTestSignal.GND;
            // Установка A1 и A2
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Подключение каналов ВП к земле прибора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void ConnectVpChannelsTo10K(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            // Установка канала E1
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[51].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[51].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E2
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E3
            stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[10].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[10].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[44].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[44].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[45].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[45].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E4
            stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[14].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[14].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[46].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[46].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[47].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[47].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Подключение каналов ЭЭГ к земле прибора
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void ConnectEegChannelsToGnd(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            // Установка отведений ЭЭГ
            for (int i = 0; i < 32; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.GND;
            }
            // Установка SH1 и REF
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;
            // Установка SH2
            stand.CommutatorSetting.ChannelsState[39].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[39].TestSignal = CommutatorTestSignal.GND;
            // Установка A1 и A2
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }
        /// <summary>
        /// Подключение каналов ВП к земле устройства
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void ConnectVpChannelsToGnd(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            // Установка канала E1
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[51].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[51].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E2
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E3
            stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[10].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[10].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[44].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[44].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[45].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[45].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E4
            stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[14].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[14].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[46].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[46].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[47].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[47].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Подключение канала ЭКГ к земле
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void ConnectEcgChannelToGnd(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            // Установка отрицательного электрода
            stand.CommutatorSetting.ChannelsState[36].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[36].TestSignal = CommutatorTestSignal.GND;
            // Установка положительного электрода
            stand.CommutatorSetting.ChannelsState[38].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[38].TestSignal = CommutatorTestSignal.GND;
            // Установка экрана
            stand.CommutatorSetting.ChannelsState[39].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[39].TestSignal = CommutatorTestSignal.GND;
            // Подключение земли канала дыхания. Иначе сильно возрастает сетевая помеха
            stand.CommutatorSetting.ChannelsState[41].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[41].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает коммутатор для подачи дифференциального сигнала
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void SetEegDiffSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;
            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;

            // Установка отведений ЭЭГ
            for (int i = 0; i < 32; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
            }
            // Установка SH1
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;
            // Установка REF
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.S2;
            // Установка A1 и A2
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;

            stand.CommutatorSetting.SetCommutatorState();
        }
        /// <summary>
        /// Устанавливает коммутатор для подачи дифференциального сигнала относительно A1
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void SetEegDiffSignalRefA1(ScriptEnvironment environment, bool isMirroredHead)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;
            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;

            // Установка отведений ЭЭГ
            for (int i = 0; i < 32; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
            }
            // Установка SH1
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;
            // Установка REF
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.S1;
            // Установка A1
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = isMirroredHead ? CommutatorTestSignal.S1 : CommutatorTestSignal.S2;
            // Установка A2
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = isMirroredHead ? CommutatorTestSignal.S2 : CommutatorTestSignal.S1;

            stand.CommutatorSetting.SetCommutatorState();
        }
        /// <summary>
        /// Устанавливает коммутатор для подачи синфазного сигнала на каналы ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void SetSynphaseEegSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.FloatSupplyControl.IsFloatSupplyON = true;

            // Установка отведений ЭЭГ
            for (int i = 0; i < 32; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
            }
            // Установка SH1
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;
            // Установка REF
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.S1;
            // Установка A1
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.S1;
            // Установка A2
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;

            //stand.CommutatorSetting.ChannelsState[41].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[41].TestSignal = CommutatorTestSignal.GND;

            // На провод земли в толстом кабел подать тот же сигнал, чтобы попробовать компенсировать его емкость
            //stand.CommutatorSetting.ChannelsState[49].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[49].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }
        /// <summary>
        /// Устанавливает коммутатор для проверки входного импеданса каналов ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="isRefTested"> Флаг, указывающий, какие электроды проверяются в данный момент </param>
        public static void SetEegSynphForImpedance(ScriptEnvironment environment, bool isRefTested)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.GND;
            stand.FloatSupplyControl.IsFloatSupplyON = true;

            // Установка отведений ЭЭГ
            for (int i = 0; i < 32; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = isRefTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
            }
            // Установка SH1
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.S1;
            // Установка REF
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = isRefTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.S1;
            // Установка A1
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = isRefTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[41].TestCircuit = CommutatorTestCircuit._0_Ohm; // На этот канал сигнал подается только для компенсации емкости
            stand.CommutatorSetting.ChannelsState[41].TestSignal = CommutatorTestSignal.S1;
            // Установка A2
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = isRefTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает коммутатор для подачи синфазного сигнала на каналы ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void SetSynphaseVpSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.FloatSupplyControl.IsFloatSupplyON = true;

            // Установка канала E1
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.S1;
            //stand.CommutatorSetting.ChannelsState[51].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[51].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E2
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.S1;
            //stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E3
            stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[10].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[10].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[44].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[44].TestSignal = CommutatorTestSignal.S1;
            //stand.CommutatorSetting.ChannelsState[45].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[45].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E4
            stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[14].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[14].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[46].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[46].TestSignal = CommutatorTestSignal.S1;
            //stand.CommutatorSetting.ChannelsState[47].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[47].TestSignal = CommutatorTestSignal.S1;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает коммутатор для проверки входного импеданса каналов ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="isPositiveTested"> Положительный или отрицательный электрод проверяется </param>
        public static void SetVpForInputImpedance(ScriptEnvironment environment, bool isPositiveTested)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.FloatSupplyControl.IsFloatSupplyON = true;

            // Установка канала E1
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = isPositiveTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = isPositiveTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.S1;
            //stand.CommutatorSetting.ChannelsState[51].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[51].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E2
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = isPositiveTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = isPositiveTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.S1;
            //stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E3
            stand.CommutatorSetting.ChannelsState[9].TestCircuit = isPositiveTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
            stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[11].TestCircuit = isPositiveTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[10].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[10].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[44].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[44].TestSignal = CommutatorTestSignal.S1;
            //stand.CommutatorSetting.ChannelsState[45].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[45].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E4
            stand.CommutatorSetting.ChannelsState[13].TestCircuit = isPositiveTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
            stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[15].TestCircuit = isPositiveTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[14].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[14].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[46].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[46].TestSignal = CommutatorTestSignal.S1;
            //stand.CommutatorSetting.ChannelsState[47].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[47].TestSignal = CommutatorTestSignal.S1;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает дифференциальный сигнал для каналов ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void SetDifferetialVpSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;
            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;

            // Установка канала E1
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.GND;
            //stand.CommutatorSetting.ChannelsState[51].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[51].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E2
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;
            //stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E3
            stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[10].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[10].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[44].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[44].TestSignal = CommutatorTestSignal.GND;
            //stand.CommutatorSetting.ChannelsState[45].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[45].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E4
            stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[14].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[14].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[46].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[46].TestSignal = CommutatorTestSignal.GND;
            //stand.CommutatorSetting.ChannelsState[47].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[47].TestSignal = CommutatorTestSignal.S1;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Сменить текущий электрод при проверке распайки разъемов DIN
        /// </summary>
        /// <param name="stand"> Экземпляр стенда </param>
        /// <param name="electrodeNumber"> Номер проверяемого электрода </param>
        public static void ChangeCommStateSolderingTest(UniversalTestStand stand, int electrodeNumber)
        {
            if (stand == null)
                return;
            switch (electrodeNumber)
            {
                case 1:
                    // Установка канала E1
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
                    // Установка канала E2
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E3
                    stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E4
                    stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
                case 2:
                    // Установка канала E1
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E2
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E3
                    stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E4
                    stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
                case 3:
                    // Установка канала E1
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E2
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
                    // Установка канала E3
                    stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E4
                    stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
                case 4:
                    // Установка канала E1
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E2
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E3
                    stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E4
                    stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
                case 5:
                    // Установка канала E1
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E2
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E3
                    stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.GND;
                    // Установка канала E4
                    stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
                case 6:
                    // Установка канала E1
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E2
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E3
                    stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.GND;
                    stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E4
                    stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
                case 7:
                    // Установка канала E1
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E2
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E3
                    stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E4
                    stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.GND;
                    break;
                case 8:
                    // Установка канала E1
                    stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E2
                    stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E3
                    stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    stand.CommutatorSetting.ChannelsState[11].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[11].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    // Установка канала E4
                    stand.CommutatorSetting.ChannelsState[13].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[13].TestSignal = CommutatorTestSignal.GND;
                    stand.CommutatorSetting.ChannelsState[15].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[15].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                    break;
            }
            // Установка экранов E1 (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.GND;
            //stand.CommutatorSetting.ChannelsState[51].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[51].TestSignal = CommutatorTestSignal.S1;

            // Установка экранов E2 (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;
            //stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;

            // Установка экранов E3 (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[10].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[10].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[44].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[44].TestSignal = CommutatorTestSignal.GND;
            //stand.CommutatorSetting.ChannelsState[45].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[45].TestSignal = CommutatorTestSignal.S1;

            // Установка экранов E4 (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[14].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[14].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[46].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[46].TestSignal = CommutatorTestSignal.GND;
            //stand.CommutatorSetting.ChannelsState[47].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[47].TestSignal = CommutatorTestSignal.S1;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Установка коммутатора для тестирования каналов DC
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void SetDcChannelsState(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INT2;
            stand.CommutatorOfTestSignal.TestSignalLineS3 = TestSignalS3.EXT1;
            
            // Установка канала DC1
            stand.CommutatorSetting.ChannelsState[37].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[37].TestSignal = CommutatorTestSignal.S2;
            // Установка канала DC2
            stand.CommutatorSetting.ChannelsState[34].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[34].TestSignal = CommutatorTestSignal.S2;
            // Замкнуть REF на землю, чтобы по нему не шла наводка
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает состояние канала дыхания
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="channelOn"> Состояние канала (true = ВКЛ, false = ВЫКЛ) </param>
        public static void SetBreathChannel(ScriptEnvironment environment, bool channelOn)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS3 = TestSignalS3.INT1;
            // Подключение земли канала дыхания
            stand.CommutatorSetting.ChannelsState[41].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[41].TestSignal = channelOn ? CommutatorTestSignal.GND : CommutatorTestSignal.NOT_SIGNAL;
            // Замкнуть REF на землю, чтобы по нему не шла наводка
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;
            // Включить канал дыхания
            stand.BreathChannelControl.IsBreathChannelON = channelOn;
            if (!channelOn)
                StandOperations.SetGeneratorState(environment, 1e-3f, 10.0f);

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает коммутатор для подачи дифференциального сигнала в канал ЭКГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void SetEcgDifferetialSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;
            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;

            // Установка отрицательного электрода
            stand.CommutatorSetting.ChannelsState[36].TestCircuit = CommutatorTestCircuit._51kOhm_47nF;
            stand.CommutatorSetting.ChannelsState[36].TestSignal = CommutatorTestSignal.S2;
            // Установка положительного электрода
            stand.CommutatorSetting.ChannelsState[38].TestCircuit = CommutatorTestCircuit._51kOhm_47nF;
            stand.CommutatorSetting.ChannelsState[38].TestSignal = CommutatorTestSignal.S1;
            // Установка экрана
            stand.CommutatorSetting.ChannelsState[39].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[39].TestSignal = CommutatorTestSignal.GND;
            // Подключение земли канала дыхания. Иначе сильно возрастает сетевая помеха
            //stand.CommutatorSetting.ChannelsState[41].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[41].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает синфазный сигнал в канал ЭКГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void SetSynphaseSignalEcg(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.FloatSupplyControl.IsFloatSupplyON = true;
            // Установка отрицательного электрода
            stand.CommutatorSetting.ChannelsState[36].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[36].TestSignal = CommutatorTestSignal.S1;
            // Установка положительного электрода
            stand.CommutatorSetting.ChannelsState[38].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[38].TestSignal = CommutatorTestSignal.S1;
            // Установка экрана
            stand.CommutatorSetting.ChannelsState[39].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[39].TestSignal = CommutatorTestSignal.S1;
            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Проверяет состояние стенда
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <returns> Экземпляр  стенда </returns>
        private static UniversalTestStand GetStand(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return null;
            }
            return stand;
        }

        /// <summary>
        /// Подключение тестовых сопротивлений 10 кОм для энцефалографов НС-1...НС-4
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4ConnectChannelsTo10K(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            // Установка каналов ЭЭГ
            for (int i = 0; i < 23; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._10kOhm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.GND;
            }
            // Подключение на землю экрана каналов ЭЭГ
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;

            // Установка A1 и A2 на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.GND;
            // Подключение на землю экрана каналов ЭЭГ на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Замыкает на землю все каналы ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4GndEegChannels(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            // Установка каналов ЭЭГ
            for (int i = 0; i < 23; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.GND;
            }
            // Подключение на землю экрана каналов ЭЭГ
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;

            // Установка A1 и A2 на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.GND;
            // Подключение на землю экрана каналов ЭЭГ на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает дифференциальный сигнал на входах ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4SetEegDiffSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            // Установка тестового сигнала
            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;

            // Установка каналов ЭЭГ
            for (int i = 0; i < 23; i++)
            {
                if (i >= 21)
                {
                    stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
                    stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S2;
                    continue;
                }
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
            }
            // Подключение на землю экрана каналов ЭЭГ
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;//S2; 

            // Установка A1 и A2 на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S2;
            // Подключение на землю экрана каналов ЭЭГ на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }

        public static void EEG4SetEegMonoSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            // Установка тестового сигнала
            //stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            //stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INT2;

            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;

            // Установка каналов ЭЭГ
            for (int i = 0; i < 23; i++)
            {
                if (i >= 21)
                {
                    stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
                    stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.GND;
                    continue;
                }
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S2;//S1;
            }
            // Подключение на землю экрана каналов ЭЭГ
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;

            // Установка A1 и A2 на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.GND;
            // Подключение на землю экрана каналов ЭЭГ на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает дифференциальный сигнал на канале A1-A2 ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4SetEegA1A2DiffSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            // Установка тестового сигнала
            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;

            // Установка каналов ЭЭГ
            for (int i = 0; i < 23; i++)
            {
                if (i == 21)
                {
                    stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
                    stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
                    continue;
                }
                if (i == 22)
                {
                    stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
                    stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S2;
                    continue;
                }
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
            }
            // Подключение на землю экрана каналов ЭЭГ
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;

            // Установка A1 и A2 на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S2;
            // Подключение на землю экрана каналов ЭЭГ на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает коммутатор для подачи синфазного сигнала для НС-1...НС-4
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4SetSynphaseEegSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            // Установка тестового сигнала
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.GND;

            // Установка каналов ЭЭГ
            for (int i = 0; i < 23; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
            }
            // Подключение на землю экрана каналов ЭЭГ
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;

            // Установка A1 и A2 на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;
            // Подключение на землю экрана каналов ЭЭГ на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.S1;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает коммутатор для проверки входных импедансов каналов ЭЭГ
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="isRefTested"> Признак проверки опорных электродов </param>
        public static void EEG4SetEegSynphForImpedance(ScriptEnvironment environment, bool isRefTested)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            // Установка тестового сигнала
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.GND;
            stand.FloatSupplyControl.IsFloatSupplyON = true;

            // Установка каналов ЭЭГ
            for (int i = 0; i < 23; i++)
            {
                if (i >= 21)
                {
                    stand.CommutatorSetting.ChannelsState[i].TestCircuit = isRefTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
                    continue;
                }
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = isRefTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
            }
            // Подключение на землю экрана каналов ЭЭГ
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;

            // Установка A1 и A2 на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[40].TestCircuit = isRefTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[40].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[43].TestCircuit = isRefTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[43].TestSignal = CommutatorTestSignal.S1;
            // Подключение на землю экрана каналов ЭЭГ на тот случай, если используется кабель для НС-5
            stand.CommutatorSetting.ChannelsState[33].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[33].TestSignal = CommutatorTestSignal.S1;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Установка коммутатора для тестирования каналов DC
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4SetDcChannelsState(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS3 = TestSignalS3.EXT1;

            // Установка канала DC1
            stand.CommutatorSetting.ChannelsState[35].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[35].TestSignal = CommutatorTestSignal.S1;
            // Установка канала DC2
            stand.CommutatorSetting.ChannelsState[32].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[32].TestSignal = CommutatorTestSignal.S1;
            // Замкнуть REF на землю, чтобы по нему не шла наводка
            //stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            //stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Подключает на 10 кОм электроды каналов ВП для приборов с одним таким каналом
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4ConnectVpChannelsTo10K(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            // Установка канала E1 для кабеля НСФТ 801103.006
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E1 для кабеля НСФТ 801103.002
            stand.CommutatorSetting.ChannelsState[24].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[24].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[26].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[26].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[25].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[25].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Подключение каналов ВП к земле устройства для приборов с одним ВП каналом
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4ConnectVpChannelsToGnd(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            // Установка канала E1 для кабеля НСФТ 801103.006
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E2 для кабеля НСФТ 801103.002
            stand.CommutatorSetting.ChannelsState[24].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[24].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[26].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[26].TestSignal = CommutatorTestSignal.GND;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[25].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[25].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает подачу синфазного сигнала на каналы ВП для устройств с одним каналом ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4SetSynphaseVpSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.FloatSupplyControl.IsFloatSupplyON = true;

            // Установка канала E1 для кабеля НСФТ 801103.006
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E2 для кабеля НСФТ 801103.002
            stand.CommutatorSetting.ChannelsState[24].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[24].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[26].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[26].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[25].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[25].TestSignal = CommutatorTestSignal.S1;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает подачу дифференциального сигнала на каналы ВП для устройств с одним каналом ВП
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4SetDifferetialVpSignal(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;
            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;

            // Установка канала E1 для кабеля НСФТ 801103.006
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;

            // Установка канала E2 для кабеля НСФТ 801103.002
            stand.CommutatorSetting.ChannelsState[24].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[24].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[26].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[26].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[25].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[25].TestSignal = CommutatorTestSignal.GND;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает коммутатор для проверки входного импеданса каналов ВП для устройств с одним каналом
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void EEG4SetVpForInputImpedance(ScriptEnvironment environment, bool isPositiveTested)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.FloatSupplyControl.IsFloatSupplyON = true;

            // Установка канала E1 для кабеля НСФТ 801103.006
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = isPositiveTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = isPositiveTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.S1;

            // Установка канала E2 для кабеля НСФТ 801103.002
            stand.CommutatorSetting.ChannelsState[24].TestCircuit = isPositiveTested ? CommutatorTestCircuit._0_Ohm : CommutatorTestCircuit._200kOhm_22nF;
            stand.CommutatorSetting.ChannelsState[24].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[26].TestCircuit = isPositiveTested ? CommutatorTestCircuit._200kOhm_22nF : CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[26].TestSignal = CommutatorTestSignal.S1;
            // Установка экранов (сначала общий, потом экраны по жилам)
            stand.CommutatorSetting.ChannelsState[25].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[25].TestSignal = CommutatorTestSignal.S1;

            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }
    }
}
