using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices;
using System.ComponentModel;
using System.Threading;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for DeviceInformationControl.xaml
    /// </summary>
    public partial class DeviceInformationControl : UserControl, IDisposable
    {
        public DeviceInformationControl(ScriptEnvironment environment, bool isSpO2Testing)
        {
            InitializeComponent();
            DataContext = this;
            this.environment = environment;
            this.isSpO2Testing = isSpO2Testing;
            InitDeviceInfoCollection();
        }

        private ScriptEnvironment environment;
        /// <summary>
        /// Флаг проверки модуля SpO2
        /// </summary>
        private bool isSpO2Testing;

        /// <summary>
        /// Коллекция элементов с информацией о приборе
        /// </summary>
        private ObservableCollection<DeviceInfoItem> deviceInfoItems = new ObservableCollection<DeviceInfoItem>();
        /// <summary>
        /// Свойство для получения доступа к коллекции с информацией о приборе
        /// </summary>
        public ObservableCollection<DeviceInfoItem> DeviceInfoItems
        {
            get { return deviceInfoItems; }
        }

        private bool isSpO2Available;
        /// <summary>
        /// Указывает, установлен ли в приборе датчик SpO2
        /// </summary>
        public bool IsSpO2Available
        {
            get { return isSpO2Available; }
            set { isSpO2Available = value; }
        }
        

        /// <summary>
        /// Инициализирует список с пунктами информации о приборе
        /// </summary>
        private void InitDeviceInfoCollection()
        {
            NeuroMEPMicro deviceMepMicro;
            EEG5Device deviceEEG5;
            EEG4Device deviceEEG4;
            if ((deviceMepMicro = environment.Device as NeuroMEPMicro) != null)
            {
                SetMepMicroInformation(deviceMepMicro);
            }
            if ((deviceEEG5 = environment.Device as EEG5Device) != null)
            {
                IsSpO2Available = (deviceEEG5.DeviceInformation.StremmerVersia & 0x80) > 0; // Устанавливается флаг установки датчика SpO2 в устройстве
                if (isSpO2Testing && IsSpO2Available)
                {
                    deviceEEG5.NeedSPO2 = true;
                    deviceEEG5.SPO2 += new SPO2EventFunc(SpO2Event);
                }
                SetEEG5Information(deviceEEG5);
            }
            if ((deviceEEG4 = environment.Device as EEG4Device) != null)
            {
                if (EEG4Scripts.GetDeviceType(deviceEEG4) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro2
                    || EEG4Scripts.GetDeviceType(deviceEEG4) == EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4)
                    SetEmgMicroInformation(deviceEEG4);
                else
                    SetEEG4Information(deviceEEG4);
            }
        }

        /// <summary>
        /// Устанавливает информацию для НейроМВП-Микро
        /// </summary>
        /// <param name="deviceMepMicro"></param>
        private void SetMepMicroInformation(NeuroMEPMicro deviceMepMicro)
        {
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceName, deviceMepMicro.DeviceInformation.DeviceName));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceSerialNumber, int.Parse(deviceMepMicro.GetDeviceState().SerialNo), int.Parse(environment.DeviceSerialNumber) - 1,
                                int.Parse(environment.DeviceSerialNumber) + 1));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceVersion, (int)deviceMepMicro.DeviceInformation.DeviceVersia, 0, 100));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, (int)deviceMepMicro.DeviceInformation.DeviceAlarm));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RefVoltageCode, (int)deviceMepMicro.DeviceInformation.Opornoe, 24200, 24800));
            DeviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ReferenceVoltage, Math.Round((deviceMepMicro.DeviceInformation.Opornoe * 6.66 / 65536), 3).ToString() + " " + Properties.Resource1.V, (int)deviceMepMicro.DeviceInformation.Opornoe, 24200, 24600));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.TestVoltageCodePlus, (int)deviceMepMicro.DeviceInformation.U_low_plus, 2500, 4500));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.TestVoltageCodeMinus, (int)deviceMepMicro.DeviceInformation.U_low_minus, -4500, -2500));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ImpedanceMeasureVoltage, "(U+) - (U-) = " +
                                                   Math.Round(((deviceMepMicro.DeviceInformation.U_low_plus - deviceMepMicro.DeviceInformation.U_low_minus) * 6.66 * 1e3 / 65536 / 2.0), 1).ToString() + " " + Properties.Resource1.mV,
                                                   (int)Math.Round(((deviceMepMicro.DeviceInformation.U_low_plus - deviceMepMicro.DeviceInformation.U_low_minus) * 6.66 * 1e3 / 65536 / 2.0), 1), 320, 400));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVoltageCodePlus, deviceMepMicro.DeviceInformation.U_high_plus.ToString()/*, -500, +500*/));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVoltageCodeMinus, deviceMepMicro.DeviceInformation.U_high_minus.ToString()/*, -500, +500*/));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVolatage, "(u+) - (u-) = " +
                                                   Math.Round(((deviceMepMicro.DeviceInformation.U_high_plus - deviceMepMicro.DeviceInformation.U_high_minus) * 6.66 * 1e3 / 65536 / 2.0), 3).ToString() + " " + Properties.Resource1.mV,
                                                   (int)((deviceMepMicro.DeviceInformation.U_high_plus - deviceMepMicro.DeviceInformation.U_high_minus) * 6.66 * 1e3 / 65536 / 2.0 * 10), 34, 42));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.StimModuleInformation, (int)deviceMepMicro.DeviceInformation.info_stim));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.KeyModuleInformation, (int)deviceMepMicro.DeviceInformation.info_klav));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.KeyModuleVersion, (byte)deviceMepMicro.DeviceInformation.versia_klav));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.KeyModuleButtonsState, (int)deviceMepMicro.DeviceInformation.klav_work));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ButtonsStateAtXS1, (int)deviceMepMicro.DeviceInformation.info_ra0));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ButtonsStateAtXS2, (int)deviceMepMicro.DeviceInformation.info_ra1));
        }

        /// <summary>
        /// Устанавливает информацию для НС-5
        /// </summary>
        /// <param name="deviceEEG5"></param>
        private void SetEEG5Information(EEG5Device deviceEEG5)
        {
            if (!isSpO2Testing)
            {
                bool isNS5 = !(deviceEEG5.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP;
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.InfomationFromUSBProc, ""));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceSerialNumber, int.Parse(deviceEEG5.GetDeviceState().SerialNo), int.Parse(environment.DeviceSerialNumber) - 1,
                                                        int.Parse(environment.DeviceSerialNumber) + 1));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.USBProcVersion, (int)deviceEEG5.DeviceInformation.SypressType));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.USBProcFirmwareVersion, (int)deviceEEG5.DeviceInformation.SypressVersion));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.FrontPanelHeadOrientation, (deviceEEG5.DeviceInformation.reserv1 & 0x01) == 0 ? Properties.Resource1.A1Right : Properties.Resource1.A1Left));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.EEGAmplifiersType, (deviceEEG5.DeviceInformation.reserv1 & 0x02) == 0 ? "INA118" : "LT1168"));

                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ADCModuleInformation, ""));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceName, deviceEEG5.DeviceInformation.DeviceName));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.FirmwareVersion, (int)deviceEEG5.DeviceInformation.DeviceVersia, 0, 100));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RestartCode, (int)deviceEEG5.DeviceInformation.AmplifierRestartCod));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, (int)deviceEEG5.DeviceInformation.DeviceAlarm));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.KeysNumber, (int)deviceEEG5.DeviceInformation.SwitchNumber, isNS5 ? 10 : 9, isNS5 ? 12 : 11));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RefVoltageCode, (int)deviceEEG5.DeviceInformation.Uopornoe, 23588, 25554));
                DeviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ReferenceVoltage, Math.Round((deviceEEG5.DeviceInformation.Uopornoe * 6.66 / 65536), 3).ToString() + " " + Properties.Resource1.V,
                                                        (int)deviceEEG5.DeviceInformation.Uopornoe, 23588, 25554));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.TestVoltageCodePlus, deviceEEG5.DeviceInformation.UhightPlus, 2100, 4100));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.TestVoltageCodeMinus, deviceEEG5.DeviceInformation.UhightMinus, -4100, -2100));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ImpedanceMeasureVoltage, "(U+) - (U-) = " +
                                                       Math.Round(((deviceEEG5.DeviceInformation.UhightPlus - deviceEEG5.DeviceInformation.UhightMinus) * 6.66 * 1e3 / 65536 / 2.0), 1).ToString() + " " + Properties.Resource1.mV,
                                                       (int)Math.Round(((deviceEEG5.DeviceInformation.UhightPlus - deviceEEG5.DeviceInformation.UhightMinus) * 6.66 * 1e3 / 65536 / 2.0), 1), 274, 354));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVoltageCodePlus, deviceEEG5.DeviceInformation.UlowPlus.ToString()/*, -15000, 15000*/));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVoltageCodeMinus, deviceEEG5.DeviceInformation.UlowMinus.ToString()/*, -15000, 15000*/));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVolatage, "(u+) - (u-) = " +
                                                       Math.Round(((deviceEEG5.DeviceInformation.UlowPlus - deviceEEG5.DeviceInformation.UlowMinus) * 6.66 * 1e6 / 65536 / 502.0), 3).ToString() + " " + Properties.Resource1.mkV,
                                                       (int)((deviceEEG5.DeviceInformation.UlowPlus - deviceEEG5.DeviceInformation.UlowMinus) * 6.66 * 1e6 / 65536 / 502.0), 800, 1200));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DC1Voltage, (deviceEEG5.DeviceInformation.UDC1 * 0.102).ToString() + " " + Properties.Resource1.mV));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DC2Voltage, (deviceEEG5.DeviceInformation.UDC2 * 0.102).ToString() + " " + Properties.Resource1.mV));

                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CurrentStimInformation, ""));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CurrentStimModuleVersion, (int)deviceEEG5.DeviceInformation.TokVersia));
                //deviceInfoItems.Add(new DeviceInfoItem("Подключение вилочкового токового стимулятора", ));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RestartCode, (int)deviceEEG5.DeviceInformation.CurrStimRestartCod));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MultiChanCurrentStimIsConnected, deviceEEG5.DeviceInformation.CurrentCommutatorPresent ? Properties.Resource1.Connected : Properties.Resource1.NotConnected));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CurrentStimAnswerCode, deviceEEG5.DeviceInformation.TokChannelStimul.ToString() + " = " +
                                                       Math.Round(deviceEEG5.DeviceInformation.TokChannelStimul / (3.0 * 256.0 / 3.3 / 100.0), 2).ToString() + " " + Properties.Resource1.mA, (int)deviceEEG5.DeviceInformation.TokChannelStimul, 0, 2));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CurrentStimSettingCode, deviceEEG5.DeviceInformation.TokChannelControl.ToString() + " = " +
                                                       Math.Round(deviceEEG5.DeviceInformation.TokChannelControl / (3.0 * 256.0 / 3.3 / 100.0), 2).ToString() + " " + Properties.Resource1.mA, (int)deviceEEG5.DeviceInformation.TokChannelControl, 0, 2));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.VoltageControlChannelCode, deviceEEG5.DeviceInformation.TokChannelSupplay.ToString() + " = " +
                                                       Math.Round(deviceEEG5.DeviceInformation.TokChannelSupplay * 3.3 * 11 / 256, 2).ToString() + " " + Properties.Resource1.V,
                                                       (int)Math.Round(deviceEEG5.DeviceInformation.TokChannelSupplay * 3.3 * 11 * 10 / 256), 85/*90*/, 103)); // С разрешения Шмелева С.И. снижен нижний порог напряжения с 9 В до 8,5 В (13.09.2013)
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.HighVoltControlChannelCode, deviceEEG5.DeviceInformation.TokChannelHightVolt.ToString() + " = " +
                                                       Math.Round(deviceEEG5.DeviceInformation.TokChannelHightVolt * 3.3 * (3300 * 3 + 82) / 256 / 82).ToString() + " " + Properties.Resource1.V,
                                                       (int)(deviceEEG5.DeviceInformation.TokChannelHightVolt * 3.3 * (3300 * 3 + 82) / 256 / 82), 270, 330));

                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.PhotoPhonoModuleInformation, ""));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.PhotoPhonoStimVersion, (int)deviceEEG5.DeviceInformation.StimVersia));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RestartCode, (int)deviceEEG5.DeviceInformation.StimulatorsRestartCod));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.PatternConnection, deviceEEG5.DeviceInformation.StimInfo == 0 ? Properties.Resource1.NotConnected : Properties.Resource1.Connected));

                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.KeyModuleInformation, ""));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.KeyIndicationModuleVersion, (int)deviceEEG5.DeviceInformation.KlavVersia));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RestartCode, (int)deviceEEG5.DeviceInformation.KeyboardRestartCod));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.PushButtonInformation, deviceEEG5.DeviceInformation.KlavInfo == 0 ? Properties.Resource1.NotPushed : Properties.Resource1.Pushed));

                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ThreadModuleInformation, ""));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ThreadModuleVersion, (int)(deviceEEG5.DeviceInformation.StremmerVersia & 0x7F)));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RestartCode, (int)deviceEEG5.DeviceInformation.StreamerRestartCod));
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.SpO2Availability, (deviceEEG5.DeviceInformation.StremmerVersia & 0x80) > 0 ? Properties.Resource1.Setted : Properties.Resource1.Absent));
            }
            else
            {
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.SpO2ModuleInformation, ""));
                deviceInfoItems.Add(new DeviceInfoItem("Repeat command 0×50", deviceEEG5.DeviceInformation.Spo2Info[0], 79, 81));
                deviceInfoItems.Add(new DeviceInfoItem("SpO2", deviceEEG5.DeviceInformation.Spo2Info[1], 95, 99));
                deviceInfoItems.Add(new DeviceInfoItem("Frequncy of pulse", deviceEEG5.DeviceInformation.Spo2Info[2], 50, 100));
                deviceInfoItems.Add(new DeviceInfoItem("Index full of pulse", deviceEEG5.DeviceInformation.Spo2Info[3].ToString()));
                deviceInfoItems.Add(new DeviceInfoItem("Reserve", deviceEEG5.DeviceInformation.Spo2Info[4].ToString()));
                deviceInfoItems.Add(new DeviceInfoItem("Condition of modul", deviceEEG5.DeviceInformation.Spo2Info[5].ToString()));
                deviceInfoItems.Add(new DeviceInfoItem("Parameters of modul", deviceEEG5.DeviceInformation.Spo2Info[6].ToString()));
                deviceInfoItems.Add(new DeviceInfoItem("Foto pletizmogramma", deviceEEG5.DeviceInformation.Spo2Info[7].ToString()));
                deviceInfoItems.Add(new DeviceInfoItem("Testing check", deviceEEG5.DeviceInformation.Spo2Info[8].ToString()));
                deviceInfoItems.Add(new DeviceInfoItem("CHECK", deviceEEG5.DeviceInformation.Spo2Info[9] == 0 ? "OK" : "Alarm"));
            }
        }

        /// <summary>
        /// Устанавливает информацию для НС-1,2,3,4,4/П
        /// </summary>
        /// <param name="deviceEEG4"></param>
        private void SetEEG4Information(EEG4Device deviceEEG4)
        {
            int numOfKeys = 0;
            switch (EEG4Scripts.GetDeviceType(deviceEEG4))
            {
                case EEG4Scripts.NeuronSpectrumTypes.NS_1:
                case EEG4Scripts.NeuronSpectrumTypes.NS_2:
                case EEG4Scripts.NeuronSpectrumTypes.NS_3:
                case EEG4Scripts.NeuronSpectrumTypes.NS_4:
                    numOfKeys = 6;
                    break;
                case EEG4Scripts.NeuronSpectrumTypes.NS_4P:
                case EEG4Scripts.NeuronSpectrumTypes.NS_4EP:
                    numOfKeys = 9;
                    break;
                case EEG4Scripts.NeuronSpectrumTypes.EMG_Micro2:
                    numOfKeys = 5;
                    break;
                case EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4:
                    numOfKeys = 7;
                    break;
            }
            
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ADCModuleInformation, ""));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceName, deviceEEG4.DeviceInformation.DeviceName));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceSerialNumber, (int)deviceEEG4.DeviceInformation.DeviceNumber, int.Parse(environment.DeviceSerialNumber) - 1, int.Parse(environment.DeviceSerialNumber) + 1));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceVersion, (int)deviceEEG4.DeviceInformation.DeviceVersion, 0, 100));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceType, deviceEEG4.DeviceInformation.DeviceType, 0, 9));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceFullName, deviceEEG4.DeviceInformation.FullName));
            if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0149) == 0)
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, 0));
            else
            {
                if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0001) == 1)
                    deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, Properties.Resource1.ErrorStackOverFlow));
                if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0008) == 8)
                    deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, Properties.Resource1.ErrorProcRestart));
                if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0040) == 64)
                    deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, Properties.Resource1.ErrorSwitchesTest));
                if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0100) == 256)
                    deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, Properties.Resource1.ErrorSupplyAbsent));
            }
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.KeysNumber, (int)((deviceEEG4.DeviceInformation.DeviceAlarm & 0xFE00) >> 9), numOfKeys - 1, numOfKeys + 1));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RefVoltageCode, deviceEEG4.DeviceInformation.U0, 24400, 24800));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ReferenceVoltage, Math.Round(deviceEEG4.DeviceInformation.U0 * 6.66 / 65535, 3).ToString() + " " + Properties.Resource1.V,
                                                   deviceEEG4.DeviceInformation.U0, 24400, 24800));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.FrontPanelHeadOrientation, (deviceEEG4.DeviceInformation.reserve & 0x01) == 0 ? Properties.Resource1.A1Right : Properties.Resource1.A1Left));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.EEGAmplifiersType, (deviceEEG4.DeviceInformation.reserve & 0x02) == 0 ? "INA118" : "LT1168"));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.TestVoltageCodePlus, deviceEEG4.DeviceInformation.Uhplus, 2100, 4100));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.TestVoltageCodeMinus, deviceEEG4.DeviceInformation.Uhminus, -4100, -2100));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ImpedanceMeasureVoltage, "(U+) - (U-) = " + 
                                                   Math.Round((deviceEEG4.DeviceInformation.Uhplus - deviceEEG4.DeviceInformation.Uhminus) * 6.66 * 1e3 / 65535 / 2, 2).ToString() + " " + Properties.Resource1.mV,
                                                   (int)Math.Round((deviceEEG4.DeviceInformation.Uhplus - deviceEEG4.DeviceInformation.Uhminus) * 6.66 * 1e3 / 65535 / 2, 2), 353, 393));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVoltageCodePlus, deviceEEG4.DeviceInformation.Ulplus.ToString()/*, -5000, 5000*/));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVoltageCodeMinus, deviceEEG4.DeviceInformation.Ulminus.ToString()/*, -5000, 5000*/));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVolatage, "(u+) - (u-) = " +
                                                   Math.Round((deviceEEG4.DeviceInformation.Ulplus - deviceEEG4.DeviceInformation.Ulminus) * 6.66 * 1e6 / 65535 / 102, 1).ToString() + " " + Properties.Resource1.mkV,
                                                   (int)Math.Round((deviceEEG4.DeviceInformation.Ulplus - deviceEEG4.DeviceInformation.Ulminus) * 6.66 * 1e6 / 65535 / 102, 1), 800, 1200));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DC1Voltage, Math.Round(deviceEEG4.DeviceInformation.Temperature * 6.66 * 1e3 / 65535).ToString() + " " + Properties.Resource1.mV));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.BreathChannelCode, deviceEEG4.DeviceInformation.Spread, int.MinValue, int.MaxValue));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.GndChannelCode, deviceEEG4.DeviceInformation.E1, int.MinValue, int.MaxValue));
        }

        /// <summary>
        /// Устанавливает информацию для ЭМГ-Микро 2, ЭМГ-Микро 4
        /// </summary>
        /// <param name="deviceEEG4"></param>
        private void SetEmgMicroInformation(EEG4Device deviceEEG4)
        {
            int numOfKeys = 0;
            switch (EEG4Scripts.GetDeviceType(deviceEEG4))
            {
                case EEG4Scripts.NeuronSpectrumTypes.EMG_Micro2:
                    numOfKeys = 5;
                    break;
                case EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4:
                    numOfKeys = 7;
                    break;
            }

            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ADCModuleInformation, ""));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceName, deviceEEG4.DeviceInformation.DeviceName));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceSerialNumber, (int)deviceEEG4.DeviceInformation.DeviceNumber, int.Parse(environment.DeviceSerialNumber) - 1, int.Parse(environment.DeviceSerialNumber) + 1));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceVersion, (int)deviceEEG4.DeviceInformation.DeviceVersion, 0, 100));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceType, deviceEEG4.DeviceInformation.DeviceType, 0, 9));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DeviceFullName, deviceEEG4.DeviceInformation.FullName));
            if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0149) == 0)
                deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, 0));
            else
            {
                if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0001) == 1)
                    deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, Properties.Resource1.ErrorStackOverFlow));
                if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0008) == 8)
                    deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, Properties.Resource1.ErrorProcRestart));
                if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0040) == 64)
                    deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, Properties.Resource1.ErrorSwitchesTest));
                if ((deviceEEG4.DeviceInformation.DeviceAlarm & 0x0100) == 256)
                    deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.MalfunctionInDevice, Properties.Resource1.ErrorSupplyAbsent));
            }
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.KeysNumber, (int)((deviceEEG4.DeviceInformation.DeviceAlarm & 0xFE00) >> 9), numOfKeys - 1, numOfKeys + 1));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.RefVoltageCode, deviceEEG4.DeviceInformation.U0, 24400, 24800));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ReferenceVoltage, Math.Round(deviceEEG4.DeviceInformation.U0 * 6.66 / 65535, 3).ToString() + " " + Properties.Resource1.V,
                                                   deviceEEG4.DeviceInformation.U0, 24400, 24800));
            //deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.FrontPanelHeadOrientation, (deviceEEG4.DeviceInformation.reserve & 0x01) == 0 ? Properties.Resource1.A1Right : Properties.Resource1.A1Left));
            //deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.EEGAmplifiersType, (deviceEEG4.DeviceInformation.reserve & 0x02) == 0 ? "INA118" : "LT1168"));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.TestVoltageCodePlus, deviceEEG4.DeviceInformation.Uhplus, 2100, 4100));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.TestVoltageCodeMinus, deviceEEG4.DeviceInformation.Uhminus, -4100, -2100));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.ImpedanceMeasureVoltage, "(U+) - (U-) = " +
                                                   Math.Round((deviceEEG4.DeviceInformation.Uhplus - deviceEEG4.DeviceInformation.Uhminus) * 6.66 * 1e3 / 65535 / 2, 2).ToString() + " " + Properties.Resource1.mV,
                                                   (int)Math.Round((deviceEEG4.DeviceInformation.Uhplus - deviceEEG4.DeviceInformation.Uhminus) * 6.66 * 1e3 / 65535 / 2, 2), 353, 393));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVoltageCodePlus, deviceEEG4.DeviceInformation.Ulplus.ToString()/*, -5000, 5000*/));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVoltageCodeMinus, deviceEEG4.DeviceInformation.Ulminus.ToString()/*, -5000, 5000*/));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.CalibrVolatage, "(u+) - (u-) = " +
                                                   Math.Round((deviceEEG4.DeviceInformation.Ulplus - deviceEEG4.DeviceInformation.Ulminus) * 6.66 * 1e6 / 65535 / 102, 1).ToString() + " " + Properties.Resource1.mkV,
                                                   (int)Math.Round((deviceEEG4.DeviceInformation.Ulplus - deviceEEG4.DeviceInformation.Ulminus) * 6.66 * 1e6 / 65535 / 102, 1), 800, 1200));
            //deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.DC1Voltage, Math.Round(deviceEEG4.DeviceInformation.Temperature * 6.66 * 1e3 / 65535).ToString() + " " + Properties.Resource1.mV));
            //deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.BreathChannelCode, deviceEEG4.DeviceInformation.Spread, int.MinValue, int.MaxValue));
            deviceInfoItems.Add(new DeviceInfoItem(Properties.Resource1.GndChannelCode, deviceEEG4.DeviceInformation.E1, int.MinValue, int.MaxValue));
        }

        /// <summary>
        /// Обработчик события от модуля SpO2
        /// </summary>
        /// <param name="spo2"> Значение SpO2 </param>
        /// <param name="photoPlethysmogram"> Фотоплетизмограмма </param>
        void SpO2Event(int spo2, int[] photoPlethysmogram)
        {
            if (isSpO2Testing && (deviceInfoItems.Count != 0) && deviceInfoItems[0].ParamName.Contains("Информация с модуля SpO2"))
            {
                for (int i = 0; i < (environment.Device as EEG5Device).SpO2Information.Length; i++)
                {
                    if (deviceInfoItems[i + 1].ParamName.Equals("CHECK"))
                    {
                        deviceInfoItems[i + 1].ParamValue = (environment.Device as EEG5Device).SpO2Information[i] == 0 ? "OK" : "Alarm";
                        continue;
                    }
                    deviceInfoItems[i + 1].ParamValue = (environment.Device as EEG5Device).SpO2Information[i].ToString();
                }
            }
        }

        /// <summary>
        /// Проверяет, соответствуют ли значения требованиям. Если не соответствует, возвращает сообщение об ошибке
        /// </summary>
        /// <param name="strinErroreMessage"> Сообщение об ошибке </param>
        /// <returns> Результат оценки значений </returns>
        public bool? CheckDeviceInformationValid(out string strinErroreMessage)
        {
            foreach (DeviceInfoItem devInfItem in deviceInfoItems)
            {
                if (!devInfItem.IsValid)
                {
                    strinErroreMessage = "Значение '" + devInfItem.ParamName + "' не соответствует требованиям.";
                    return null;
                }
            }
            strinErroreMessage = "";
            return true;
        }

        public void StopSpO2Reading()
        {
            if (environment.Device is EEG5Device)
            {
                (environment.Device as EEG5Device).NeedSPO2 = false;
                (environment.Device as EEG5Device).SPO2 -= new SPO2EventFunc(SpO2Event);
            }
        }

        public void Dispose()
        {
            StopSpO2Reading();
        }
    }

    /// <summary>
    /// Класс для элементов информации о приборе
    /// </summary>
    public class DeviceInfoItem : INotifyPropertyChanged
    {
        #region Конструкторы

        /// <summary>
        /// Конструктор для параметров со строковым значением
        /// </summary>
        /// <param name="paramName"> Имя параметра </param>
        /// <param name="paramValue"> Значение параметра </param>
        public DeviceInfoItem(string paramName, string paramValue)
        {
            ParamName = paramName;
            ParamValue = paramValue;
            IsValid = true;
        }
        /// <summary>
        /// Конструктор для значений, которые имеют ограниченное значение
        /// </summary>
        /// <param name="paramName"> Имя параметра </param>
        /// <param name="paramValue"> Значение параметра </param>
        /// <param name="minValue"> Минимальное значение </param>
        /// <param name="maxValue"> Максимальное значение </param>
        public DeviceInfoItem(string paramName, int paramValue, int minValue, int maxValue)
        {
            ParamName = paramName;
            this.minValue = minValue;
            this.maxValue = maxValue;
            ParamValue = paramValue.ToString();
            RangedValue<int> rangeParamValue = new RangedValue<int>(paramValue, minValue, maxValue);
            if (!rangeParamValue.IsValidValue)
                IsValid = false;
            else
                IsValid = true;
        }
        /// <summary>
        /// Конструктор для значений, которые имеют ограниченное но строковое значение
        /// </summary>
        /// <param name="paramName"> Имя параметра </param>
        /// <param name="paramValueStr"> Строковое значение параметра </param>
        /// <param name="paramValue"> Значение параметра </param>
        /// <param name="minValue"> Минимальное значение </param>
        /// <param name="maxValue"> Максимальное значение </param>
        public DeviceInfoItem(string paramName, string paramValueStr, int paramValue, int minValue, int maxValue)
        {
            ParamName = paramName;
            ParamValue = paramValueStr;
            RangedValue<int> rangeParamValue = new RangedValue<int>(paramValue, minValue, maxValue);
            if (!rangeParamValue.IsValidValue)
                IsValid = false;
            else
                IsValid = true;
        }
        /// <summary>
        /// Конструктор для параметров со строковым значением
        /// </summary>
        /// <param name="paramName"> Имя параметра </param>
        /// <param name="paramValue"> Значение параметра </param>
        public DeviceInfoItem(string paramName, int paramValue)
        {
            ParamName = paramName;
            ParamValue = paramValue.ToString("X");
            IsValid = true;
        }

        #endregion

        /// <summary>
        /// Минимальноез начение параметра
        /// </summary>
        private int minValue = int.MinValue;
        /// <summary>
        /// Максимальное значение параметра
        /// </summary>
        private int maxValue = int.MaxValue;

        /// <summary>
        /// Имя параметра информации о приборе
        /// </summary>
        private string paramName;
        /// <summary>
        /// Имя параметра информации о приборе
        /// </summary>
        public string ParamName 
        {
            get { return paramName; }
            private set
            {
                paramName = value;
                OnPropertyChanged("ParamName");
            }
        }
        /// <summary>
        /// Строковое значение параметра информации о приборе
        /// </summary>
        private string paramValue;
        /// <summary>
        /// Строковое значение параметра информации о приборе
        /// </summary>
        public string ParamValue 
        {
            get { return paramValue; }
            /*private*/ set
            {
                paramValue = value;
                int intValue;
                if (int.TryParse(paramValue, out intValue))
                {
                    RangedValue<int> rangeParamValue = new RangedValue<int>(int.Parse(paramValue), minValue, maxValue);
                    if (!rangeParamValue.IsValidValue)
                        IsValid = false;
                    else
                        IsValid = true;
                }
                OnPropertyChanged("ParamValue");
            }
        }
        /// <summary>
        /// Валидность значения
        /// </summary>
        private bool isValid;

        public bool IsValid
        {
            get { return isValid; }
            private set
            {
                isValid = value;
                OnPropertyChanged("IsValid");
            }
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Событие на изменение свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Уведомление об изменении свойства (все объекты представления привязаные к этому свойству автоматически обновят себя)
        /// </summary>
        /// <param name="propertyName">Имя свойства принимающего новое значение</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
