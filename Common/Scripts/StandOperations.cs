using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.Devices;

namespace NeuroSoft.DeviceAutoTest.Common.Scripts
{
    /// <summary>
    /// Общие операции со стендом
    /// </summary>
    public static class StandOperations
    {
        /// <summary>
        /// Устанавливает параметры одного из генераторов
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="ampl"> Амплитуда сигнала </param>
        /// <param name="freq"> Частота сигнала </param>
        /// <param name="waveForm"> Фома волны сигнала </param>
        /// <param name="generatorNumber"> Номер генератора (0 - первый, 1 - второй) </param>
        public static void SetGeneratorState(ScriptEnvironment environment, float? ampl, float? freq, WaveForm? waveForm = WaveForm.Sinus, int generatorNumber = 0)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            if (ampl.HasValue)
                stand.Generators[generatorNumber].Amplitude = ampl.Value;
            if (freq.HasValue)
                stand.Generators[generatorNumber].Frequency = freq.Value;
            if (waveForm.HasValue)
                stand.Generators[generatorNumber].WaveForm = waveForm.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void ConnectChannelsToGnd(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void SetDifferentialSignalCommutatorState(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;

            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;

            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void SetDifferentialSignalCommutatorStateMEPMicro(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INTplus;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INTminus;

            stand.CommutatorOfTestSignal.DifferentialSignal = DifferentialSignal.INT1;
            stand.CommutatorOfTestSignal.SummableSignal = SummableSignal.GND;

            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S2;
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[42].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[42].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public static void SetMonoPolarSignalCommutatorState(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
                        
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S1;
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._4700_Ohm_1nF;
            stand.CommutatorSetting.SetCommutatorState();
        }

        public static void SetCommutatorSynphSignal(ScriptEnvironment environment)
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
        }

        public static void ChangeCommutatorStateInImpedanceTest(ScriptEnvironment environment, object device, int electrodeNumber)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ResetCommutatorState();

            if (device is NeuroMEPMicro)
            {
                ChangeCommStateForMEPMicro(stand, electrodeNumber);
            }
            if ((device is EEG5Device)
                || (device is EEG4Device /*&& EEG4Scripts.GetDeviceType(device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4*/))
                StandOperationsEEG.ChangeCommStateSolderingTest(stand, electrodeNumber);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stand"></param>
        public static void ResetCommutatorState(ScriptEnvironment environment)
        {
            environment.OpenStand();
            var stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stand"></param>
        public static void ResetCommutatorState(this UniversalTestStand stand)
        {
            foreach (var channelState in stand.CommutatorSetting.ChannelsState)
            {
                channelState.TestCircuit = CommutatorTestCircuit._0_Ohm;
                channelState.TestSignal = CommutatorTestSignal.NOT_SIGNAL;
            }
            stand.CommutatorSetting.SetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.INT2;
            stand.CommutatorOfTestSignal.TestSignalLineS3 = TestSignalS3.INT1;
            stand.FloatSupplyControl.IsFloatSupplyON = false;
        }

        /// <summary>
        /// Подключает к электродам миографа тестовые резисторы номиналом 10 кОм
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        public static void Set10kMEPMicro(ScriptEnvironment environment)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._10kOhm;
            stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.GND;

            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Метод изменения состояния коммутаторов при проверке измерения импедансов для НейроМВП-Микро
        /// </summary>
        /// <param name="stand"> Экземпляр стенда </param>
        /// <param name="electrodeNumber"> Номер электрода </param>
        private static void ChangeCommStateForMEPMicro(UniversalTestStand stand, int electrodeNumber)
        {
            if (stand == null)
                return;
            // Переключаем тестируемый электрод
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1._10_kOhm;
            switch (electrodeNumber)
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
                    stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.GND;
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
                    stand.CommutatorSetting.ChannelsState[50].TestCircuit = CommutatorTestCircuit._0_Ohm;
                    stand.CommutatorSetting.ChannelsState[50].TestSignal = CommutatorTestSignal.GND;
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
            // подключаем экраны на землю
            stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Производит смену напряжения питания USB
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="usbVoltage"> Напряжение питания </param>
        public static void ChangeVusb(ScriptEnvironment environment, USBSupplyVoltage usbVoltage)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.USBVoltageChanger.UsbSupplyVoltage = usbVoltage;
        }

        /// <summary>
        /// Устанавливает подачу не дифференциального сигнала на входы усилителя
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="testCircuit"> Используемая тестовая цепь </param>
        /// <param name="gndToMinus"> Земля подключается к минусовому электроду </param>
        /// <param name="ekranIsUsed"> Экраны подключаются к земле (используются) </param>
        public static void SetMonoPolarSignalCommutatorState(ScriptEnvironment environment, CommutatorTestCircuit testCircuit = CommutatorTestCircuit._0_Ohm, bool gndToMinus = true, bool ekranIsUsed = true)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            
            if (gndToMinus)
            {
                stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[21].TestCircuit = testCircuit;
                stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[23].TestCircuit = testCircuit;
                stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[5].TestCircuit = testCircuit;
                stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[7].TestCircuit = testCircuit;
            }
            else
            {
                stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[21].TestCircuit = testCircuit;
                stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[23].TestCircuit = testCircuit;
                stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[5].TestCircuit = testCircuit;
                stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[7].TestCircuit = testCircuit;
            }
            if (ekranIsUsed)
            {
                stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.GND;
                stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            }
            else
            {
                stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
                stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
            }
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает коммутатор при проверке входного импеданса усилителей в зависимости от проверяемого электрода
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="positiveInput"> Положительные электроды </param>
        public static void SetCommutatorSyncPhaseImpedance(ScriptEnvironment environment, bool positiveInput)
        {
            environment.OpenStand();
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.ResetCommutatorState();
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;

            if (!positiveInput)
            {
                stand.CommutatorSetting.ChannelsState[2].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[2].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[3].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[3].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[4].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[4].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[8].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[8].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[18].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[18].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[19].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[19].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[20].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[20].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._0_Ohm;
            }
            else
            {
                stand.CommutatorSetting.ChannelsState[4].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[4].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[5].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[5].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[6].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[6].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[7].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[7].TestCircuit = CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[8].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[8].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[9].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[9].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[10].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[10].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[20].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[20].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[21].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[21].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[22].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[22].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[23].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[23].TestCircuit = CommutatorTestCircuit._200kOhm_22nF;
                stand.CommutatorSetting.ChannelsState[24].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[24].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[25].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[25].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[26].TestSignal = CommutatorTestSignal.S1;
                stand.CommutatorSetting.ChannelsState[26].TestCircuit = CommutatorTestCircuit._0_Ohm;
            }
            stand.FloatSupplyControl.IsFloatSupplyON = true;
            stand.CommutatorSetting.SetCommutatorState();
        }
    }
}
