using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Devices;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using NeuroSoft.EEG.EEGMontageMaker;
using NeuroSoft.DeviceAutoTest.ScriptExecution;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    public class ReadDataViewModelEEG5 : ReadDataViewModelBase
    {
        /// <summary>
        /// Контсруктор
        /// </summary>
        /// <param name="device"> Ссылка на экземпляр устройства </param>
        /// <param name="workMode"> Режим передачи данных </param>
        public ReadDataViewModelEEG5(ScriptEnvironment environment, EEG5Device device, EEGTestingMode testingMode)
            : base(environment, device)
        {
            TestingMode = testingMode;
            SetDeviceInfo(device);
            InitFilters();
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
        /// Количество каналов в устройстве
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
        /// ССылка на экземпляр устройства НС-5
        /// </summary>
        public EEG5Device EEGDevice
        {
            get { return device as EEG5Device; }
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
                        if (!(EEGDevice.Filters[i][j] is MathLib.Filters.IIRFilterRejector) && !(EEGDevice.Filters[i][j] is MathLib.Filters.DeviceNoiseFilter))
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
                        return (EEGDevice.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).SampleFrequencyEEG;
                    case EEGWorkMode.VPTransmit:
                        return (EEGDevice.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).SampleFrequencyVP;
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
                int chanNumb = 0;
                MontageHead montage = EEGDevice.GetMontage();
                switch (testingMode)
                {
                    case EEGTestingMode.Calibration:
                        WorkMode = EEGWorkMode.Kalibrovka;
                        EEGDevice.SetCalibrationFrequency(3.0f);
                        chanNumb = GetUsedChNames(testingMode).Count();
                        break;
                    case EEGTestingMode.EEGNoiseTransmit:
                    case EEGTestingMode.EEGTransRefA1:
                    case EEGTestingMode.EEGTransmit:
                        WorkMode = EEGWorkMode.EEGTransmit;
                        chanNumb = GetUsedChNames(testingMode).Count();
                        if (value == EEGTestingMode.EEGTransRefA1)
                        {
                            montage.SetNewReferentForAllDerivations(ReferentElectrodeEnum.IpsyEarElectrode);
                            EEGDevice.SetMontage(montage, true);
                        }
                        break;
                    case EEGTestingMode.VPTransmit:
                        WorkMode = EEGWorkMode.VPTransmit;
                        break;
                    case EEGTestingMode.DCTransmit:
                        WorkMode = EEGWorkMode.EEGTransmit;
                        chanNumb = GetUsedChNames(testingMode).Count();
                        break;
                    case EEGTestingMode.BreathTransmit:
                    case EEGTestingMode.ECGNoiseTransmit:
                    case EEGTestingMode.ECGTransmit:
                        WorkMode = EEGWorkMode.EEGTransmit;
                        chanNumb = 1;
                        break;
                }
                ChannelsCount = chanNumb;
            }
        }

        /// <summary>
        /// Флаг, указывающий, что чтение осуществляется с прибора Нейрон-Спектр-5
        /// </summary>
        private bool isNS5Reading
        {
            get { return !((Device as EEG5Device).GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).isNS4MEP; }
        }
        

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
                //EEGDevice.ResetFilters();
                //object filters = EEGDevice.Filters;
                if (TestingMode == EEGTestingMode.VPTransmit)
                {
                    int index = FiltersInfo.IndexOf(filterInfo);
                    for (int i = 0; i < EEGDevice.Filters[index].Length; i++)
                    {
                        if (!(EEGDevice.Filters[index][i] is NeuroSoft.MathLib.Filters.IIRFilterRejector) && !(EEGDevice.Filters[index][i] is NeuroSoft.MathLib.Filters.DeviceNoiseFilter))
                            EEGDevice.Filters[index][i].Active = filterInfo.IsEnabled;
                    }
                }
                else
                {
                    for (int i = 0; i < EEGDevice.Filters.Length; i++)
                    {
                        for (int j = 0; j < EEGDevice.Filters[i].Length; j++)
                        {
                            if (!(EEGDevice.Filters[i][j] is NeuroSoft.MathLib.Filters.IIRFilterRejector) && !(EEGDevice.Filters[i][j] is NeuroSoft.MathLib.Filters.DeviceNoiseFilter))
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
                    EEGDevice.ImposeFilters(true);
                    for (int i = 0; i < EEGDevice.Filters[index].Length; i++)
                    {
                        if (!(EEGDevice.Filters[index][i] is NeuroSoft.MathLib.Filters.IIRFilterRejector) && !(EEGDevice.Filters[index][i] is NeuroSoft.MathLib.Filters.DeviceNoiseFilter))
                            EEGDevice.Filters[index][i].Active = filterInfo.IsEnabled;
                    }
                }
                else
                {
                    EEGDevice.SetFilters((float)filterInfo.MinFreq, (float)filterInfo.MaxFreq, enablePowerRejector);
                    EEGDevice.ImposeFilters(false);
                }
            }
            //EEGDevice.ImposeFilters(false);
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
                    CopyEEGData(ref newData, ref e.Data, TestingMode);
                    break;
                case EEGTestingMode.EEGNoiseTransmit:
                case EEGTestingMode.EEGTransRefA1:
                case EEGTestingMode.EEGTransmit:
                    CopyEEGData(ref newData, ref e.Data, TestingMode);
                    break;
                case EEGTestingMode.DCTransmit:
                    newData[0] = e.Data[26];
                    newData[1] = e.Data[27];
                    break;
                case EEGTestingMode.BreathTransmit:
                    newData[0] = e.Data[28];
                    break;
                case EEGTestingMode.ECGTransmit:
                case EEGTestingMode.ECGNoiseTransmit:
                    newData[0] = e.Data[21];
                    break;
            }
            if (TestingMode != EEGTestingMode.VPTransmit)
                e.Data = newData;
            base.DeviceReceiveDataHandler(sender, e);
        }

        private void CopyEEGData(ref float[][] targArray, ref float[][] srcData, EEGTestingMode testMode)
        {
            int j = 0;
            Array.Copy(srcData, targArray, j += 21);    //Каналы ЭЭГ
            
            if (testMode == EEGTestingMode.Calibration)
                targArray[j++] = srcData[21];       // ECG
            targArray[j++] = srcData[22];   // A1
            targArray[j++] = srcData[23];   // A2
            if (isNS5Reading)
            {
                var xChansLen = 11;
                Array.Copy(srcData, 32, targArray, j, xChansLen);   // X1...X11
                j += xChansLen;
            }
            if (testMode == EEGTestingMode.Calibration)
            {
                Array.Copy(srcData, 44, targArray, j, 4);   // E1...E4
            }
        }

        /// <summary>
        /// Инициализирует коллекцию фильтров
        /// </summary>
        private void InitFilters()
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                
                var filterInfo = new FilterInfo(TestingMode == EEGTestingMode.VPTransmit ? String.Format("E{0}", i + 1) : "All", SamplesRateInSamplesPerSecond);
                filterInfo.FilterChanged += new RoutedEventHandler(FiltersChanged);
                FiltersInfo.Add(filterInfo);
                filterInfo.ApplyFilterSettings();
                if (TestingMode != EEGTestingMode.VPTransmit)
                    break;
            }
        }

        /// <summary>
        /// Инициализирует DataPlotter
        /// </summary>
        protected override void InitPlotter()
        {
            List<Hardware.Devices.Controls.DataPlotter.ChannelParams> channelParams = new List<Hardware.Devices.Controls.DataPlotter.ChannelParams>();
            foreach (var chName in GetUsedChNames(TestingMode))
            {
                channelParams.Add(new Hardware.Devices.Controls.DataPlotter.ChannelParams(chName, 30, 10, 1));
            }
            
            DataMonitoringPlotter.ChannelsCount = ChannelsCount;
            DataMonitoringPlotter.InitChannels(channelParams.ToArray(), (TestingMode == EEGTestingMode.VPTransmit || TestingMode == EEGTestingMode.DCTransmit) ? 20 : 10);
            DataMonitoringPlotter.SamplesRateInSamplesPerSecond = SamplesRateInSamplesPerSecond;
            DataMonitoringPlotter.Height = (TestingMode == EEGTestingMode.Calibration || TestingMode == EEGTestingMode.EEGTransmit
                                            || TestingMode == EEGTestingMode.EEGTransRefA1 || TestingMode == EEGTestingMode.EEGNoiseTransmit) ? 500.0 : 400.0;
        }

        /// <summary>
        /// Инициализирует информацию, связанную с девайсом
        /// </summary>
        /// <param name="device"></param>
        private void SetDeviceInfo(EEG5Device device)
        {
            EEGDevice = device;
            base.SetDeviceInfo();

            foreach (var chName in GetUsedChNames(TestingMode))
            {
                StatisticsCollection.Add(new DataStatistics(chName));
            }
            foreach (var i in Enumerable.Range(0, 4))
            {
                ChannelsInfo.Add(new VpChannelInfo(i, EEGDevice));
            }
        }

        /// <summary>
        /// Применят настройки
        /// </summary>
        /// <param name="settings"> Настройки контрола </param>
        public override void SetSavedSettings(ReadDataSettings settings)
        {
            base.SetSavedSettings(settings);
            // При проверке сигнала относительно A1 необходимо изменить пределы значний для этой ячейки
            if (TestingMode == EEGTestingMode.EEGTransRefA1)
            {
                foreach (DataStatistics dataStatisticItem in StatisticsCollection)
                {
                    if (dataStatisticItem.ChannelName == "A1")
                    {
                        dataStatisticItem.Swing.MinValue = 0.0;
                        dataStatisticItem.Swing.MaxValue = 10e-6;
                    }
                }
            }
            if (TestingMode == EEGTestingMode.VPTransmit && (settings.MinFreqFilter.HasValue || settings.MaxFreqFilter.HasValue))
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
            if (TestingMode == EEGTestingMode.EEGNoiseTransmit || TestingMode == EEGTestingMode.ECGNoiseTransmit)
            {
                EEG5AmplifierState amplifierState = Device.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState;
                if (amplifierState != null)
                    (Device as EEG5Device).SetMontage(amplifierState.CurrentMontage, true);
            }
            if (TestingMode == EEGTestingMode.EEGTransRefA1)
            {
                MontageHead currentMontage = EEGDevice.GetMontage();
                if (currentMontage.ReferentElectrode != ReferentElectrodeEnum.Ref)
                {
                    currentMontage.SetNewReferentForAllDerivations(ReferentElectrodeEnum.Ref);
                    EEGDevice.SetMontage(currentMontage, true);
                }
            }
        }

        /// <summary>
        /// Получить имена используемых каналов в зависимости от режима работы
        /// </summary>
        /// <param name="tstMode"> Режим работы </param>
        /// <returns> Перечисление имен </returns>
        private IEnumerable<string> GetUsedChNames(EEGTestingMode tstMode)
        {
            var isNs5 = isNS5Reading;   // Признак работы с нс5
            var chNames = Enumerable.Range(0, base.ChannelsCount).Select(i => EEGDevice.GetChannelName(i));     // Получить имена ВСЕХ каналов

            // Объявить коллекции с именами, которые следует исключать
            var exraNames = new[] { "", "DC1", "DC2", "PG", "PPG" };
            var xNames = new[] { "X1", "X2", "X3", "X4", "X5", "X6", "X7", "X8", "X9", "X10", "X11" };
            var eNames = new[] { "E1", "E2", "E3", "E4", "ECG" };

            // Выбрать только нужные имена в зависимости от режима тестирования
            switch (TestingMode)
            {
                case EEGTestingMode.Calibration:
                    var excludedNames = isNs5 ? exraNames : exraNames.Concat(xNames);
                    return chNames.Where(chn => !excludedNames.Contains(chn));
                case EEGTestingMode.EEGNoiseTransmit:
                case EEGTestingMode.EEGTransRefA1:
                case EEGTestingMode.EEGTransmit:
                    excludedNames = isNs5 ? exraNames.Concat(eNames) : exraNames.Concat(xNames).Concat(eNames);
                    return chNames.Where(chn => !excludedNames.Contains(chn));
                case EEGTestingMode.VPTransmit:
                    return eNames.Take(4);
                case EEGTestingMode.DCTransmit:
                    return new[] { "DC1", "DC2" };
                case EEGTestingMode.BreathTransmit:
                    return new[] { "PG" };
                case EEGTestingMode.ECGTransmit:
                case EEGTestingMode.ECGNoiseTransmit:
                    return new[] { "ECG" };
            }
            return Enumerable.Empty<string>();
        }
        
        #endregion
    }

    public enum EEGTestingMode { EEGTransmit, VPTransmit, Calibration, EEGTransRefA1, DCTransmit, BreathTransmit, ECGTransmit, EEGNoiseTransmit, ECGNoiseTransmit, VPcalibration }

    /// <summary>
    /// Информация о каналах ВП энцефалографа
    /// </summary>
    public class VpChannelInfo : ChannelInfoBase
    {
        public VpChannelInfo(int chanNumb, NSDevice device) : base(chanNumb, device)
        { }

        #region Properties

        private List<float> highFreqPassBands;
        /// <summary>
        /// Поддерживаемые частоты среза ФНЧ
        /// </summary>
        public override List<float> HighFreqPassBands
        {
            get
            {
                if (highFreqPassBands == null)
                {
                    float[] highFreqsPass;
                    if (device is EEG5Device || (device is EEG4Device && (device as EEG4Device).DeviceInformation.DeviceVersion < 14))
                        highFreqsPass = new float[] { 20000.0f, 10000.0f, 250.0f };
                    else
                        highFreqsPass = new float[] { 10000.0f, 250.0f };
                    highFreqPassBands = highFreqsPass.ToList();
                }
                return highFreqPassBands;
            }
        }

        /// <summary>
        /// Индекс верхней частоты полосы пропускания сигнала
        /// </summary>
        public override float SelectedHighFreq
        {
            get
            {
                if (device == null || HighFreqPassBands.Count == 0)
                {
                    return -1;
                }
                if (device is EEG5Device)
                {
                    PolyChannelsFilterHi5[] filters = (device.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).FiltersHiVP;
                    return HighFreqPassBands[(int)(device.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).FiltersHiVP[channelNumber]];
                }
                if (device is EEG4Device)
                {
                    PolyChannelsFilterHi[] filters = (device.GetDeviceState().GetState(typeof(EEG4AmplifierState)) as EEG4AmplifierState).FiltersHiVP;
                    return HighFreqPassBands[(int)(device.GetDeviceState().GetState(typeof(EEG4AmplifierState)) as EEG4AmplifierState).FiltersHiVP[channelNumber]];
                }
                return -1;
            }
            set
            {
                if (device == null || HighFreqPassBands.Count == 0)
                    return;
                if (device is EEG5Device)
                {
                    (device as EEG5Device).SetFilterHiVP(channelNumber, value);
                    (device as EEG5Device).SetChannelsUsing();
                }
                if (device is EEG4Device)
                {
                    (device as EEG4Device).SetFilterHiVP(channelNumber, value);
                }
                OnPropertyChanged("SelectedHiFreq");
            }
        }

        private List<float> lowFreqPassBands = null;
        /// <summary>
        /// Поддерживаемые частоты среза ФВЧ
        /// </summary>
        public override List<float> LowFreqPassBands
        {
            get
            {
                if (lowFreqPassBands == null)
                {
                    float[] lowFreqsPass;
                    if (device is EEG5Device || (device is EEG4Device && (device as EEG4Device).DeviceInformation.DeviceVersion >= 14))
                        lowFreqsPass = new float[] { 0.212f, 0.7f, 160f };
                    else
                        lowFreqsPass = new float[] { 0.05f, 0.5f, 160f };
                    lowFreqPassBands = lowFreqsPass.ToList();
                }
                return lowFreqPassBands;
            }
        }

        /// <summary>
        /// Ниняя частота полосы пропускания сигнала
        /// </summary>
        public override float SelectedLowFreq
        {
            get
            {
                if (device == null || LowFreqPassBands.Count == 0)
                {
                    return -1;
                }
                if (device is EEG5Device)
                    return LowFreqPassBands[(int)(device.GetDeviceState().GetState(typeof(EEG5AmplifierState)) as EEG5AmplifierState).FiltersLoVP[channelNumber]];
                if (device is EEG4Device)
                    return LowFreqPassBands[(int)(device.GetDeviceState().GetState(typeof(EEG4AmplifierState)) as EEG4AmplifierState).FiltersLoVP[channelNumber]];
                return -1;
            }
            set
            {
                if (device == null || LowFreqPassBands.Count == 0)
                    return;
                if (device is EEG5Device)
                {
                    (device as EEG5Device).SetFilterLoVP(channelNumber, value);
                    (device as EEG5Device).SetChannelsUsing();
                }
                if (device is EEG4Device)
                {
                    (device as EEG4Device).SetFilterLoVP(channelNumber, value);
                }
                OnPropertyChanged("SelectedLowFreq");
            }
        }

        private List<float> channelSupportedRanges;

        /// <summary>
        /// Поддерживаемые коэффициенты усиления каналов ВП
        /// </summary>
        public override List<float> ChannelSupportedRanges
        {
            get
            {
                if (channelSupportedRanges == null)
                {
                    float[] suppurtedRanges = { 0.0002f, 0.0005f, 0.001f, 0.002f, 0.005f, 0.01f, 0.02f, 0.05f, 0.1f };
                    channelSupportedRanges = suppurtedRanges.ToList();
                }
                return channelSupportedRanges;
            }
        }

        /// <summary>
        /// Индекс диапазона входного сигнала
        /// </summary>
        public override float SelectedChannelRange
        {
            get
            {
                if (device == null || ChannelSupportedRanges.Count == 0)
                {
                    return -1;
                }
                return device.GetChannelState(channelNumber).InputRange;
            }
            set
            {
                if (device == null || ChannelSupportedRanges.Count == 0)
                    return;
                if (device is EEG5Device)
                {
                    (device as EEG5Device).SetGainFactorVP(channelNumber, (PolyChannelsGainFactor)channelSupportedRanges.IndexOf(value));
                    (device as EEG5Device).SetChannelsUsing();
                }
                if (device is EEG4Device)
                {
                    (device as EEG4Device).SetGainFactorVP(channelNumber, (PolyChannelsGainFactor)channelSupportedRanges.IndexOf(value));
                    //(device as EEG4Device).SetChannelsUsing();
                }
                OnPropertyChanged("SelectedChannelRange");
            }
        }

        #endregion
    }
}
