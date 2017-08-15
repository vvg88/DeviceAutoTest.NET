using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.Common.Scripts;

namespace NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Scripts
{
    public static class StandOperation
    {
        /// <summary>
        /// Закрытие стенда
        /// </summary>
        public static void CloseStand(ScriptEnvironment environment)
        {
            UniversalTestStand stand = environment.Stand;
            environment.CloseStand();
        }

        /// <summary>
        /// Открытие стенда
        /// </summary>
        public static void OpenStand(ScriptEnvironment environment)
        {
            UniversalTestStand stand = environment.Stand;
            environment.OpenStand();
        }

        /// <summary>
        /// Сброс коммутаторов
        /// </summary>
        public static void ResetCommutatorState(ScriptEnvironment environment)
        {
            UniversalTestStand stand = environment.Stand;
            stand.ResetCommutatorState();
        }

        /// <summary>
        /// Устанавливает состояние выходных коммутаторов для тестового сигнала S1
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="firstNumber"> номер начального коммутатора </param>      
        /// <param name="lastNumber"> номер конечного коммутатора </param>
        /// <param name="circuit"> Тестовая цепь канала выходного коммутатора (
        /// может принимать значения: _51kOhm_47nF, _3MOhm_1nF, _4700_Ohm_1nF, _200kOhm_22nF, _22kOhm_3300pF, _18MOhm_4700pF, _10kOhm, _0_Ohm) </param>
        public static void SetCommutatorStateTestSignalS1(ScriptEnvironment environment, int firstNumber, int lastNumber, CommutatorTestCircuit circuit)
        {
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.INT1;
            for (var i = firstNumber; i <= lastNumber; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = circuit;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.S1;
            }
            PolySpectrum8EXScripts.TestPS8EX(environment);

            // Выключаем канал дыхания
            stand.BreathChannelControl.IsBreathChannelON = false;
            stand.CommutatorSetting.ChannelsState[0].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[0].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Устанавливает состояние выходных коммутаторов к GND
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта </param>
        /// <param name="firstNumber"> номер начального коммутатора </param>      
        /// <param name="lastNumber"> номер конечного коммутатора </param>
        public static void SetCommutatorStateTestSignalToGND(ScriptEnvironment environment, int firstNumber, int lastNumber)
        {
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            for (var i = firstNumber; i <= lastNumber; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.GND;
            }
            PolySpectrum8EXScripts.TestPS8EX(environment);

            // Выключаем канал дыхания
            stand.CommutatorOfTestSignal.TestSignalLineS3 = TestSignalS3.INT1;
            stand.BreathChannelControl.IsBreathChannelON = false;
            stand.CommutatorSetting.ChannelsState[0].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[0].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
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
            stand.CommutatorOfTestSignal.TestSignalLineS1 = TestSignalS1.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS2 = TestSignalS2.GND;
            stand.CommutatorOfTestSignal.TestSignalLineS3 = TestSignalS3.INT1;
            // все каналы экг на на обрыв
            for (var i = 1; i <= 12; i++)
            {
                stand.CommutatorSetting.ChannelsState[i].TestCircuit = CommutatorTestCircuit._0_Ohm;
                stand.CommutatorSetting.ChannelsState[i].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
            }
            PolySpectrum8EXScripts.TestPS8EX(environment);

            // Включить канал дыхания
            stand.BreathChannelControl.IsBreathChannelON = channelOn;
            stand.CommutatorSetting.ChannelsState[0].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[0].TestSignal = CommutatorTestSignal.GND;
            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Задает обрыв электрода
        /// </summary>
        /// <param name="environment"> Переменная окружения скрипта</param>
        /// <param name="numElectrode"> Номер электрода</param>
        public static void BreakElectrode(ScriptEnvironment environment, int numElectrode, bool breake)
        {
            UniversalTestStand stand = environment.Stand;
            if (stand == null)
            {
                return;
            }
            stand.CommutatorSetting.ChannelsState[numElectrode].TestCircuit = CommutatorTestCircuit._0_Ohm;
            if (breake)
            {
                stand.CommutatorSetting.ChannelsState[numElectrode].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
            }
            else
            {
                stand.CommutatorSetting.ChannelsState[numElectrode].TestSignal = CommutatorTestSignal.GND;
            }
//            PolySpectrum8EXScripts.TestPS8EX(environment);

            // Выключаем канал дыхания
            stand.CommutatorOfTestSignal.TestSignalLineS3 = TestSignalS3.INT1;
            stand.BreathChannelControl.IsBreathChannelON = false;
            stand.CommutatorSetting.ChannelsState[0].TestCircuit = CommutatorTestCircuit._0_Ohm;
            stand.CommutatorSetting.ChannelsState[0].TestSignal = CommutatorTestSignal.NOT_SIGNAL;
            // Загрузить состояние в стенд
            stand.CommutatorSetting.SetCommutatorState();
        }

        /// <summary>
        /// Номера коммутаторов Универсального стенда для приборов ПС-8 и ПС-12Е
        /// </summary>
        public enum PSElectrodeStand : int
        {
            R = 2, L = 4, F = 6, C1 = 8, C7 = 10, N = 12, C2 = 1, C3 = 3, C4 = 5, C5 = 7, C6 = 9, C8 = 11
        }

    }
}
