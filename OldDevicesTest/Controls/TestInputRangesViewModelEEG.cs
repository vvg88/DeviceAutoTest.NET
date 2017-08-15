using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.Devices;
using System.Collections.ObjectModel;
using NeuroSoft.DeviceAutoTest.Common.Controls;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    public class TestInputRangesViewModelEEG5 : TestInputRangesViewModelBase
    {
        public TestInputRangesViewModelEEG5(ScriptEnvironment environment, EEG5Device device)
            : base(environment, device)
        {
            SetDeviceInfo(device);
            currentTestInfo = new TestSignalInfo(SamplesRateInSamplesPerSecond, 300.0);
            InputRangeTests = new ObservableCollection<TestInputRangeInfo>();
        }

        #region Свойства

        /// <summary>
        /// Фильтр сетевой помехи
        /// </summary>
        public override bool EnablePowerRejector
        {
            get
            {
                return base.EnablePowerRejector;
            }
            set
            {
                (Device as EEG5Device).SetFilters(/*(float)FiltersInfo[0].MinFreq*/0.0f, /*(float)FiltersInfo[0].MaxFreq*/1e4f, value);
                base.EnablePowerRejector = value;
            }
        }

        /// <summary>
        /// Частота дискретизации
        /// </summary>
        public override float SamplesRateInSamplesPerSecond
        {
            get
            {
                return ((Device as EEG5Device).GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).SampleFrequencyVP;
            }
            set
            {
                (Device as EEG5Device).SetSampleFrequencyVP(value);
            }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Инициализирует DataPlotter
        /// </summary>
        protected override void InitPlotter()
        {
            List<NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams> channelParams = new List<NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams>();
            //string name;
            for (int i = 0; i < base.ChannelsCount; i++)
            {
                //name = (Device as EEG5Device).GetChannelName(i);
                channelParams.Add(new Hardware.Devices.Controls.DataPlotter.ChannelParams(String.Format("E{0}", i + 1), 30, 10, 1));
            }
            DataMonitoringPlotter.ChannelsCount = ChannelsCount;
            DataMonitoringPlotter.InitChannels(channelParams.ToArray(), 20);
            DataMonitoringPlotter.SamplesRateInSamplesPerSecond = SamplesRateInSamplesPerSecond;
            DataMonitoringPlotter.Height = 400.0;
        }

        /// <summary>
        /// Инициализирует информацию, связанную с девайсом
        /// </summary>
        /// <param name="device"></param>
        private void SetDeviceInfo(EEG5Device device)
        {
            (Device as EEG5Device).SetWorkMode(EEGWorkMode.VPTransmit);
            base.SetDeviceInfo();
            for (int i = 0; i < base.ChannelsCount; i++)
            {
                StatisticsCollection.Add(new DataStatistics(String.Format("E{0}", i + 1)));
                ChannelsInfo.Add(new VpChannelInfo(i, Device));
            }
        }

        public override void StartReadData()
        {
            for (int i = 0; i < (device as EEG5Device).Filters.Length; i++)
            {
                for (int j = 0; j < (device as EEG5Device).Filters[i].Length; j++)
                    if ((device as EEG5Device).Filters[i][j].Active && !((device as EEG5Device).Filters[i][j] is NeuroSoft.MathLib.Filters.DeviceNoiseFilter))
                        (device as EEG5Device).Filters[i][j].Active = false;
            }
            for (int i = 0; i < ChannelsCount; i++)
            {
                (device as EEG5Device).SetFilterHiVP(i, 10000.0f);
                (device as EEG5Device).SetFilterLoVP(i, 0.5f);
            }
            base.StartReadData();
        }

        #endregion
    }

    /// <summary>
    /// Класс модели представления данных для контрола проверки диапазонов усиления каналов ВП приборов НС-1...НС-4
    /// </summary>
    public class TestInputRangesViewModelEEG4 : TestInputRangesViewModelBase
    {
        public TestInputRangesViewModelEEG4(ScriptEnvironment environment, EEG4Device device)
            : base (environment, device)
        {
            SetDeviceInfo(device);
            currentTestInfo = new TestSignalInfo(SamplesRateInSamplesPerSecond, 300.0);
            InputRangeTests = new ObservableCollection<TestInputRangeInfo>();
        }

        #region Свойства

        /// <summary>
        /// Фильтр сетевой помехи
        /// </summary>
        public override bool EnablePowerRejector
        {
            get
            {
                return base.EnablePowerRejector;
            }
            set
            {
                (Device as EEG4Device).SetFilters(/*(float)FiltersInfo[0].MinFreq*/0.0f, /*(float)FiltersInfo[0].MaxFreq*/1e4f, value);
                base.EnablePowerRejector = value;
            }
        }

        /// <summary>
        /// Частота дискретизации
        /// </summary>
        public override float SamplesRateInSamplesPerSecond
        {
            get
            {
                return ((Device as EEG4Device).GetDeviceState().GetState(typeof(EEG4AmplifierState)) as EEG4AmplifierState).SampleFrequencyVP;
            }
            set
            {
                (Device as EEG4Device).SetSampleFrequencyVP(value);
            }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Инициализирует DataPlotter
        /// </summary>
        protected override void InitPlotter()
        {
            List<NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams> channelParams = new List<NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams>();
            //string name;
            for (int i = 0; i < base.ChannelsCount; i++)
            {
                //name = (Device as EEG5Device).GetChannelName(i);
                channelParams.Add(new Hardware.Devices.Controls.DataPlotter.ChannelParams(String.Format("E{0}", i + 1), 30, 10, 1));
            }
            DataMonitoringPlotter.ChannelsCount = ChannelsCount;
            DataMonitoringPlotter.InitChannels(channelParams.ToArray(), 20);
            DataMonitoringPlotter.SamplesRateInSamplesPerSecond = SamplesRateInSamplesPerSecond;
            DataMonitoringPlotter.Height = 400.0;
        }

        /// <summary>
        /// Инициализирует информацию, связанную с девайсом
        /// </summary>
        /// <param name="device"></param>
        private void SetDeviceInfo(EEG4Device device)
        {
            (Device as EEG4Device).SetWorkMode(EEGWorkMode.VPTransmit);
            base.SetDeviceInfo();
            for (int i = 0; i < base.ChannelsCount; i++)
            {
                StatisticsCollection.Add(new DataStatistics(String.Format("E{0}", i + 1)));
                ChannelsInfo.Add(new VpChannelInfo(i, Device));
            }
        }

        /// <summary>
        /// Запускает процесс чтения данных
        /// </summary>
        public override void StartReadData()
        {
            for (int i = 0; i < (device as EEG4Device).Filters.Length; i++)
            {
                for (int j = 0; j < (device as EEG4Device).Filters[i].Length; j++)
                    if ((device as EEG4Device).Filters[i][j].Active && !((device as EEG4Device).Filters[i][j] is NeuroSoft.MathLib.Filters.DeviceNoiseFilter))
                        (device as EEG4Device).Filters[i][j].Active = false;
            }
            for (int i = 0; i < ChannelsCount; i++)
            {
                (device as EEG4Device).SetFilterHiVP(i, 10000.0f);
                (device as EEG4Device).SetFilterLoVP(i, 0.7f/*0.5f*/);
            }
            base.StartReadData();
        }

        /// <summary>
        /// Останавливает чтение данных и если опорный электрод был изменен (не REF), то возвращает REF
        /// </summary>
        public override void StopReadData()
        {
            base.StopReadData();
            //Device.ReceiveData -= new ReceiveDataDelegate(DeviceReceiveDataHandler);
        }

        #endregion
    }
}
