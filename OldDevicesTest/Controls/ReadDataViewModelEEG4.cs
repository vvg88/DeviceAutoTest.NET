using System;
using System.Collections.Generic;
using NeuroSoft.Devices;
using System.Windows;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using NeuroSoft.DeviceAutoTest.OldDevicesTest.Scripts;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.MathLib.Filters;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    public class ReadDataViewModelEEG4 : ReadDataViewModelBase
    {
        /// <summary>
        /// Контсруктор
        /// </summary>
        /// <param name="device"> Ссылка на экземпляр устройства </param>
        /// <param name="testingMode"> Режим тестирования </param>
        public ReadDataViewModelEEG4(ScriptEnvironment environment, EEG4Device device, EEGTestingMode testingMode)
            : base(environment, device)
        {
            TestingMode = testingMode;
            InitChannelsCount();
            SetDeviceInfo(device);
            InitFilters();
            UseRangeForA1A2 = false;
        }

        #region Fields

        /// <summary>
        /// Режим передачи данных
        /// </summary>
        protected EEGWorkMode WorkMode
        {
            get { return EEGDevice.GetWorkMode(); }
            set { EEGDevice.SetWorkMode(value); }
        }

        #endregion

        #region Properties

        private int channelsCount;
        /// <summary>
        /// Количество каналов в устройстве, которые используются
        /// </summary>
        public override int ChannelsCount
        {
            get
            {
                return TestingMode == EEGTestingMode.VPTransmit ? base.ChannelsCount : channelsCount;
            }
            protected set { channelsCount = value; }
        }

        /// <summary>
        /// Число каналов в устройстве
        /// </summary>
        private int ChannelsInDevice
        {
            get
            {
                switch (EEG4Scripts.GetDeviceType(device as EEG4Device))
                {
                    case EEG4Scripts.NeuronSpectrumTypes.NS_1:
                        return base.ChannelsCount - 20;
                    case EEG4Scripts.NeuronSpectrumTypes.NS_2:
                        return base.ChannelsCount - 12;
                    case EEG4Scripts.NeuronSpectrumTypes.NS_3:
                        return base.ChannelsCount - 9;
                    case EEG4Scripts.NeuronSpectrumTypes.NS_4:
                        return base.ChannelsCount - 7;
                    case EEG4Scripts.NeuronSpectrumTypes.NS_4P:
                    case EEG4Scripts.NeuronSpectrumTypes.NS_4EP:
                        return base.ChannelsCount - 2;
                    case EEG4Scripts.NeuronSpectrumTypes.EMG_Micro2:
                    case EEG4Scripts.NeuronSpectrumTypes.EMG_Micro4:
                        return base.ChannelsCount;
                    default:
                        return 0;
                }
            }
        }
        
        /// <summary>
        /// ССылка на экземпляр устройства НС-1...НС-4
        /// </summary>
        public EEG4Device EEGDevice
        {
            get { return device as EEG4Device; }
            set { device = value; }
        }
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
                EEGDevice.SetFilters((float)FiltersInfo[0].MinFreq, (float)FiltersInfo[0].MaxFreq, value);
                for (int i = 0; i < EEGDevice.Filters.Length; i++)
                {
                    for (int j = 0; j < EEGDevice.Filters[i].Length; j++)
                    {
                        if (!(EEGDevice.Filters[i][j] is IIRFilterRejector) && !(EEGDevice.Filters[i][j] is DeviceNoiseFilter))
                            EEGDevice.Filters[i][j].Active = FiltersInfo[TestingMode == EEGTestingMode.VPTransmit ? i : 0].IsEnabled;
                    }
                }
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
                switch (WorkMode)
                {
                    case EEGWorkMode.EEGTransmit:
                    case EEGWorkMode.Kalibrovka:
                        return (EEGDevice.GetDeviceState().GetState(typeof(EEG4AmplifierState)) as EEG4AmplifierState).SampleFrequencyEEG;
                    case EEGWorkMode.VPTransmit:
                        return (EEGDevice.GetDeviceState().GetState(typeof(EEG4AmplifierState)) as EEG4AmplifierState).SampleFrequencyVP;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (WorkMode)
                {
                    case EEGWorkMode.EEGTransmit:
                    case EEGWorkMode.Kalibrovka:
                        EEGDevice.SetSampleFrequencyEEG(value);
                        break;
                    case EEGWorkMode.VPTransmit:
                        EEGDevice.SetSampleFrequencyVP(value);
                        break;
                    default:
                        return;
                }
            }
        }

        private EEGTestingMode testingMode;
        /// <summary>
        /// Режим проверки
        /// </summary>
        public EEGTestingMode TestingMode
        {
            get { return testingMode; }
            set 
            { 
                testingMode = value;
                switch (testingMode)
                {
                    case EEGTestingMode.Calibration:
                    case EEGTestingMode.VPcalibration:
                        WorkMode = EEGWorkMode.Kalibrovka;
                        break;
                    case EEGTestingMode.EEGTransRefA1:
                    case EEGTestingMode.EEGTransmit:
                        WorkMode = EEGWorkMode.EEGTransmit;
                        break;
                    case EEGTestingMode.VPTransmit:
                        WorkMode = EEGWorkMode.VPTransmit;
                        break;
                    case EEGTestingMode.DCTransmit:
                        WorkMode = EEGWorkMode.EEGTransmit;
                        break;
                    case EEGTestingMode.BreathTransmit:
                        WorkMode = EEGWorkMode.EEGTransmit;
                        break;
                }
            }
        }

        /// <summary>
        /// Флаг, указывающий, применять ли устанавливаемый диапазон размаха для канала A1-A2
        /// </summary>
        public bool UseRangeForA1A2 { get; set; }
             
        #endregion

        #region Methods

        /// <summary>
        /// Обработчик события изменения фильтров
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FiltersChanged(object sender, RoutedEventArgs e)
        {
            FilterInfo filterInfo = sender as FilterInfo;
            // Изменяется разрешение фильтров. В режиме ЭЭГ надо запрещать все сразу, а в режиме ВП только для определенного канала
            if (filterInfo.IsEnabledChanged && !filterInfo.IsEnabled)
            {
                if (TestingMode == EEGTestingMode.VPTransmit)
                {
                    int index = FiltersInfo.IndexOf(filterInfo);
                    for (int i = 0; i < EEGDevice.Filters[index].Length; i++)
                    {
                        if (!(EEGDevice.Filters[index][i] is IIRFilterRejector) && !(EEGDevice.Filters[index][i] is DeviceNoiseFilter))
                            EEGDevice.Filters[index][i].Active = filterInfo.IsEnabled;
                    }
                }
                else
                {
                    for (int i = 0; i < EEGDevice.Filters.Length; i++)
                    {
                        for (int j = 0; j < EEGDevice.Filters[i].Length; j++)
                        {
                            if (!(EEGDevice.Filters[i][j] is IIRFilterRejector) && !(EEGDevice.Filters[i][j] is DeviceNoiseFilter))
                                EEGDevice.Filters[i][j].Active = filterInfo.IsEnabled;
                        }
                    }
                }
            }
            if (filterInfo.IsEnabled)
            {
                if (TestingMode == EEGTestingMode.VPTransmit)
                {
                    int index = FiltersInfo.IndexOf(filterInfo);
                    EEGDevice.SetFilterHiVP(index, (float)filterInfo.MaxFreq);
                    EEGDevice.SetFilterLoVP(index, (float)filterInfo.MinFreq);
                    for (int i = 0; i < EEGDevice.Filters[index].Length; i++)
                    {
                        if (!(EEGDevice.Filters[index][i] is IIRFilterRejector) && !(EEGDevice.Filters[index][i] is DeviceNoiseFilter))
                            EEGDevice.Filters[index][i].Active = filterInfo.IsEnabled;
                    }
                }
                else
                {
                    EEGDevice.SetFilters((float)filterInfo.MinFreq, (float)filterInfo.MaxFreq, enablePowerRejector);
                    EEGDevice.ImposeFilters(false);
                }
            }
        }
        /// <summary>
        /// Обработчик события о получении данных с прибора
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void DeviceReceiveDataHandler(object sender, ReceiveDataArgs e)
        {
            float[][] newData = new float[ChannelsCount][];
            switch (TestingMode)
            {
                case EEGTestingMode.Calibration:
                case EEGTestingMode.EEGNoiseTransmit:
                case EEGTestingMode.EEGTransmit:
                    CopyEssentEEGData(ref newData, ref e.Data, TestingMode);
                    break;
                case EEGTestingMode.EEGTransRefA1:
                    newData[0] = e.Data[10];
                    break;
                case EEGTestingMode.DCTransmit:
                    newData[0] = e.Data[29];
                    newData[1] = e.Data[30];
                    break;
                case EEGTestingMode.BreathTransmit:
                    newData[0] = e.Data[27];
                    break;
                case EEGTestingMode.VPcalibration:
                    newData[0] = e.Data[22];
                    newData[1] = e.Data[23];
                    newData[2] = e.Data[24];
                    newData[3] = e.Data[25];
                    break;
            }
            if (TestingMode != EEGTestingMode.VPTransmit)
                e.Data = newData;
            base.DeviceReceiveDataHandler(sender, e);
        }

        private void CopyEssentEEGData(ref float[][] dataTarg, ref float[][] dataSrc, EEGTestingMode testMode)
        {
            int i = 0;
            EEG4Scripts.NeuronSpectrumTypes eegDevType = EEG4Scripts.GetDeviceType(Device as EEG4Device);
            switch (eegDevType)
            {
                case EEG4Scripts.NeuronSpectrumTypes.NS_1:
                    dataTarg[i++] = dataSrc[0];
                    dataTarg[i++] = dataSrc[2];
                    dataTarg[i++] = dataSrc[4];
                    dataTarg[i++] = dataSrc[6];
                    dataTarg[i++] = dataSrc[10];
                    dataTarg[i++] = dataSrc[11];
                    dataTarg[i++] = dataSrc[13];
                    dataTarg[i++] = dataSrc[15];
                    dataTarg[i++] = dataSrc[17];
                    break;
                case EEG4Scripts.NeuronSpectrumTypes.NS_2:
                    Array.Copy(dataSrc, 0, dataTarg, i, 8);
                    i += 8;
                    Array.Copy(dataSrc, 10, dataTarg, i, 9);
                    i += 9;
                    break;
                case EEG4Scripts.NeuronSpectrumTypes.NS_3:
                    Array.Copy(dataSrc, 0, dataTarg, i, 19);
                    i += 19;
                    dataTarg[i++] = dataSrc[20];
                    break;
                case EEG4Scripts.NeuronSpectrumTypes.NS_4:
                case EEG4Scripts.NeuronSpectrumTypes.NS_4P:
                case EEG4Scripts.NeuronSpectrumTypes.NS_4EP:
                    Array.Copy(dataSrc, 0, dataTarg, 0, 22);
                    i += 22;
                    break;
                default:
                    break;
            }
            if (testMode == EEGTestingMode.Calibration)
            {
                dataTarg[i++] = dataSrc[22];
                if (eegDevType == EEG4Scripts.NeuronSpectrumTypes.NS_4P || eegDevType == EEG4Scripts.NeuronSpectrumTypes.NS_4EP)
                {
                    dataTarg[i++] = dataSrc[23];
                    dataTarg[i++] = dataSrc[24];
                    dataTarg[i++] = dataSrc[25];
                }
            }
        }

        /// <summary>
        /// Имена каналов НС-1
        /// </summary>
        private string[] channelsNamesNS1 = new string[] { "FP1", "C3", "O1", "T3", "A1", "FP2", "C4", "O2", "T4", "E1", "PG"};
        /// <summary>
        /// Имена каналов НС-2
        /// </summary>
        private string[] channelsNamesNS2 = new string[] { "FP1", "F3", "C3", "P3", "O1", "F7", "T3", "T5", "A1", "FP2", "F4", "C4", "P4", "O2", "F8", "T4", "T6", "E1", "PG" };
        /// <summary>
        /// Имена каналов НС-3
        /// </summary>
        private string[] channelsNamesNS3 = new string[] { "FP1", "F3", "C3", "P3", "O1", "F7", "T3", "T5", "FZ", "PZ", "A1", "FP2", "F4", "C4", "P4", "O2", "F8", "T4", "T6", "CZ", "E1", "PG" };

        /// <summary>
        /// Возвращает название канала для энцефалографов НС-1...НС-3, у которых урезанный набор ЭЭГ каналов.
        /// Метод создан потому, что каналы идут не по порядку, предусмотренному массивом UsingChannels. Для других приборов вернет null.
        /// </summary>
        /// <param name="chanNumber"> Номер канала </param>
        /// <returns> Название канала </returns>
        private string GetChannelName(int chanNumber)
        {
            switch (EEG4Scripts.GetDeviceType(Device as EEG4Device))
            {
                case EEG4Scripts.NeuronSpectrumTypes.NS_1:
                    return channelsNamesNS1[chanNumber];
                case EEG4Scripts.NeuronSpectrumTypes.NS_2:
                    return channelsNamesNS2[chanNumber];
                case EEG4Scripts.NeuronSpectrumTypes.NS_3:
                    return channelsNamesNS3[chanNumber];
                default:
                    return EEGDevice.GetChannelName(chanNumber);
            }
        }

        /// <summary>
        /// Инициализируем количество используемых каналов в устройстве
        /// </summary>
        private void InitChannelsCount()
        {
            switch (TestingMode)
            {
                case EEGTestingMode.BreathTransmit:
                    ChannelsCount = 1;
                    break;
                case EEGTestingMode.Calibration:
                    ChannelsCount = ChannelsInDevice - (EEG4Scripts.GetDeviceType(Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.NS_4P || 
                                                        EEG4Scripts.GetDeviceType(Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.NS_4EP ? 3 : 1);
                    break;
                case EEGTestingMode.DCTransmit:
                    ChannelsCount = 2;
                    break;
                case EEGTestingMode.EEGTransRefA1:
                    ChannelsCount = 1;
                    break;
                case EEGTestingMode.EEGTransmit:
                case EEGTestingMode.EEGNoiseTransmit:
                    ChannelsCount = ChannelsInDevice - (EEG4Scripts.GetDeviceType(Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.NS_4P || 
                                                        EEG4Scripts.GetDeviceType(Device as EEG4Device) == EEG4Scripts.NeuronSpectrumTypes.NS_4EP ? 7 : 2);
                    break;
                case EEGTestingMode.VPTransmit:
                    ChannelsCount = ChannelsInDevice;
                    break;
                case EEGTestingMode.VPcalibration:
                    ChannelsCount = 4;
                    break;
                default:
                    ChannelsCount = 0;
                    break;
            }
        }

        /// <summary>
        /// Инициализирует коллекцию фильтров
        /// </summary>
        private void InitFilters()
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                
                var filterInfo = new FilterInfo(TestingMode == EEGTestingMode.VPTransmit || TestingMode == EEGTestingMode.VPcalibration ? String.Format("E{0}", i + 1) : "All", SamplesRateInSamplesPerSecond);
                filterInfo.FilterChanged += new RoutedEventHandler(FiltersChanged);
                FiltersInfo.Add(filterInfo);
                filterInfo.ApplyFilterSettings();
                if (TestingMode != EEGTestingMode.VPTransmit && TestingMode != EEGTestingMode.VPcalibration)
                    break;
            }
        }

        /// <summary>
        /// Инициализирует DataPlotter
        /// </summary>
        protected override void InitPlotter()
        {
            List<Hardware.Devices.Controls.DataPlotter.ChannelParams> channelParams = new List<Hardware.Devices.Controls.DataPlotter.ChannelParams>();
            string name;
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (TestingMode == EEGTestingMode.DCTransmit)
                    name = EEGDevice.GetChannelName(i + 29);
                else
                    if (TestingMode == EEGTestingMode.BreathTransmit)
                        name = EEGDevice.GetChannelName(i + 27);
                    else
                        if (TestingMode == EEGTestingMode.EEGTransRefA1)
                            name = EEGDevice.GetChannelName(i + 10);
                        else
                            name = GetChannelName(i);
                switch (TestingMode)
                {
                    case EEGTestingMode.Calibration:
                            channelParams.Add(new Hardware.Devices.Controls.DataPlotter.ChannelParams(name, 30, 10, 1));
                        break;
                    case EEGTestingMode.EEGNoiseTransmit:
                    case EEGTestingMode.EEGTransRefA1:
                    case EEGTestingMode.EEGTransmit:
                        if (name == "A1")
                            name = "A1-A2";
                            channelParams.Add(new Hardware.Devices.Controls.DataPlotter.ChannelParams(name, 30, 10, 1));
                        break;
                    case EEGTestingMode.VPTransmit:
                    case EEGTestingMode.VPcalibration:
                            channelParams.Add(new Hardware.Devices.Controls.DataPlotter.ChannelParams(String.Format("E{0}", i + 1), 30, 10, 1));
                        break;
                    case EEGTestingMode.DCTransmit:
                        channelParams.Add(new Hardware.Devices.Controls.DataPlotter.ChannelParams(name, 30, 10, 1));
                        break;
                    case EEGTestingMode.BreathTransmit:
                        channelParams.Add(new Hardware.Devices.Controls.DataPlotter.ChannelParams(name, 30, 10, 1));
                        break;
                }
                
            }
            DataMonitoringPlotter.ChannelsCount = ChannelsCount;
            DataMonitoringPlotter.InitChannels(channelParams.ToArray(), (TestingMode == EEGTestingMode.VPTransmit || TestingMode == EEGTestingMode.DCTransmit || TestingMode == EEGTestingMode.EEGTransRefA1 || TestingMode == EEGTestingMode.VPcalibration) ? 20 : 10);
            DataMonitoringPlotter.SamplesRateInSamplesPerSecond = SamplesRateInSamplesPerSecond;
            DataMonitoringPlotter.Height = (TestingMode == EEGTestingMode.Calibration || TestingMode == EEGTestingMode.EEGTransmit
                                            || TestingMode == EEGTestingMode.EEGNoiseTransmit) ? 500.0 : 400.0;
        }

        /// <summary>
        /// Инициализирует информацию, связанную с девайсом
        /// </summary>
        /// <param name="device"></param>
        private void SetDeviceInfo(EEG4Device device)
        {
            this.EEGDevice = device;
            base.SetDeviceInfo();
            string name;
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (TestingMode == EEGTestingMode.DCTransmit)
                    name = EEGDevice.GetChannelName(i + 29);
                else
                    if (TestingMode == EEGTestingMode.BreathTransmit)
                        name = EEGDevice.GetChannelName(i + 27);
                    else
                        if (TestingMode == EEGTestingMode.EEGTransRefA1)
                            name = EEGDevice.GetChannelName(i + 10);
                        else
                            name = GetChannelName(i);
                switch (TestingMode)
                {
                    case EEGTestingMode.Calibration:
                        StatisticsCollection.Add(new DataStatistics(name));
                        break;
                    case EEGTestingMode.EEGNoiseTransmit:
                    case EEGTestingMode.EEGTransRefA1:
                    case EEGTestingMode.EEGTransmit:
                        StatisticsCollection.Add(new DataStatistics(name));
                        break;
                    case EEGTestingMode.VPTransmit:
                    case EEGTestingMode.VPcalibration:
                        StatisticsCollection.Add(new DataStatistics(String.Format("E{0}", i + 1)));
                        ChannelsInfo.Add(new VpChannelInfo(i, EEGDevice));
                        break;
                    case EEGTestingMode.DCTransmit:
                        StatisticsCollection.Add(new DataStatistics(name));
                        break;
                    case EEGTestingMode.BreathTransmit:
                        StatisticsCollection.Add(new DataStatistics(name));
                        break;
                }
            }
        }

        /// <summary>
        /// Применяет настройки
        /// </summary>
        /// <param name="settings"> Настройки контрола </param>
        public override void SetSavedSettings(ReadDataSettings settings)
        {
            base.SetSavedSettings(settings);
            // При проверке сигнала относительно A1 необходимо изменить пределы значний для этой ячейки
            if (TestingMode == EEGTestingMode.EEGTransmit || TestingMode == EEGTestingMode.Calibration)
            {
                foreach (DataStatistics dataStatisticItem in StatisticsCollection)
                {
                    if (dataStatisticItem.ChannelName == "A1")
                    {
                        if (UseRangeForA1A2)
                        {
                            dataStatisticItem.Swing.MinValue = settings.SwingRange.Min.Value;
                            dataStatisticItem.Swing.MaxValue = settings.SwingRange.Max.Value;
                        }
                        else
                        {
                            dataStatisticItem.Swing.MinValue = 0.0;
                            dataStatisticItem.Swing.MaxValue = 10e-6;
                        }
                    }
                }
            }
            else
                if (TestingMode == EEGTestingMode.EEGTransRefA1)
                {
                    foreach (DataStatistics dataStatisticItem in StatisticsCollection)
                    {
                        if (dataStatisticItem.ChannelName == "A1")
                        {
                            dataStatisticItem.Swing.MinValue = settings.SwingRange.Min.Value;
                            dataStatisticItem.Swing.MaxValue = settings.SwingRange.Max.Value;
                        }
                    }
                }
            if ((TestingMode == EEGTestingMode.VPTransmit || TestingMode == EEGTestingMode.VPcalibration) && (settings.MinFreqFilter.HasValue || settings.MaxFreqFilter.HasValue))
            {
                for (int i = 0; i < ChannelsCount; i++)
                {
                    if (settings.RangeIndex != null)
                    {
                        ChannelsInfo[i].SelectedChannelRange = ChannelsInfo[i].ChannelSupportedRanges[settings.RangeIndex.Value];
                    }
                    if (settings.MinFreqFilter.HasValue)
                    {
                        if (ChannelsInfo[i].LowFreqPassBands.Contains((float)settings.MinFreqFilter.Value))
                        {
                            ChannelsInfo[i].SelectedLowFreq = (float)settings.MinFreqFilter.Value;
                            FiltersInfo[i].IsEnabled = false;
                            FiltersInfo[i].ApplyFilterSettings();
                        }
                        else
                        {
                            if (settings.MinFreqFilter.HasValue && settings.MaxFreqFilter.HasValue)
                            {
                                FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, settings.MaxFreqFilter);
                                FiltersInfo[i].IsEnabled = true;
                                continue;
                            }
                            else
                            {
                                FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, 20000f);
                                FiltersInfo[i].IsEnabled = true;
                            }
                        }
                    }
                    if (settings.MaxFreqFilter.HasValue)
                    {
                        if (ChannelsInfo[i].HighFreqPassBands.Contains((float)settings.MaxFreqFilter.Value))
                        {
                            ChannelsInfo[i].SelectedHighFreq = (float)settings.MaxFreqFilter.Value;
                            FiltersInfo[i].IsEnabled = false;
                            FiltersInfo[i].ApplyFilterSettings();
                        }
                        else
                        {
                            if (!settings.MinFreqFilter.HasValue)
                                FiltersInfo[i].SetMinMaxFreq(0.0, settings.MaxFreqFilter);
                            else
                                FiltersInfo[i].SetMinMaxFreq(settings.MinFreqFilter, settings.MaxFreqFilter);
                            FiltersInfo[i].IsEnabled = true;
                        }
                    }
                }
            }
            if (TestingMode != EEGTestingMode.VPTransmit)
            {
                if (settings.MinFreqFilter.HasValue && settings.MaxFreqFilter.HasValue)
                    FiltersInfo[0].SetMinMaxFreq(settings.MinFreqFilter.Value, settings.MaxFreqFilter.Value);
                else
                    if (settings.MinFreqFilter.HasValue)
                        FiltersInfo[0].SetMinMaxFreq(settings.MinFreqFilter.Value, 10000.0);
                    else
                        if (settings.MaxFreqFilter.HasValue)
                            FiltersInfo[0].SetMinMaxFreq(0.0, settings.MaxFreqFilter.Value);
                        else
                            FiltersInfo[0].IsEnabled = false;
                FiltersInfo[0].IsEnabled = true;
            }
        }

        /// <summary>
        /// Запускает чтение данных. Если идет проверка уровня шума, то вызывает отдельный метод BeginTransmitNoise()
        /// </summary>
        public override void StartReadData()
        {
            if (TestingMode == EEGTestingMode.EEGNoiseTransmit || TestingMode == EEGTestingMode.ECGNoiseTransmit)
            {
                if (isReading)
                    return;
                isReading = true;
                (Device as EEG5Device).BeginTransmitNoise();
            }
            else
                base.StartReadData();
        }

        /// <summary>
        /// Останавливает чтение данных и если опорный электрод был изменен (не REF), то возвращает REF
        /// </summary>
        public override void StopReadData()
        {
            base.StopReadData();
            Device.ReceiveData -= new ReceiveDataDelegate(DeviceReceiveDataHandler);
        }

        #endregion
    }
}
