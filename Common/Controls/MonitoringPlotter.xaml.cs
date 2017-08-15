using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// Interaction logic for MonitoringPlotter.xaml
    /// </summary>
    public partial class MonitoringPlotter : UserControl, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public MonitoringPlotter()
        {
            InitializeComponent();
            Plotter.MeasureAmplitudeUnitName = "v";
            SelectedXScaleItem = XScaleItems[9];
            SelectedYScaleItem = YScaleItems[YScaleItems.Length - 1];
        }

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public double SamplesRateInSamplesPerSecond
        {
            get { return Plotter.SamplesRateInSamplesPerSecond; }
            set { Plotter.SamplesRateInSamplesPerSecond = value; }
        }

        private AmplifierCapabilities amplifierCapabilities;

        /// <summary>
        /// 
        /// </summary>
        public AmplifierCapabilities AmplifierCapabilities
        {
            get { return amplifierCapabilities; }
            set
            {
                amplifierCapabilities = value;
                NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[] channelParams = new NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[ChannelsCount];
                for (int i = 0; i < ChannelsCount; i++)
                {
                    string name = i.ToString();                    
                    channelParams[i] = new Hardware.Devices.Controls.DataPlotter.ChannelParams(name, 30, 10, 1);
                }
                InitChannels(channelParams);
                OnPropertyChanged("ChannelsCount");
            }
        }

        private int channelsCount;
        /// <summary>
        /// Колличество каналов усилителя.
        /// При тестировании НейроМВП-МикроМ и всех новых приборов, компоненты для которых написаны Смирновым А.Н. количество каналов устанавливается с использованием
        /// класса AmplifierCapabilities. Для старых приборов, компоненты которых написаны Ивановым А.А., AmplifierCapabilities не актуален. Поэтому свойство channelsCount
        /// устанавливается непосредственно перед инициализацией плоттера.
        /// </summary>
        public int ChannelsCount
        {
            get
            {
                return AmplifierCapabilities != null ? AmplifierCapabilities.ChannelsCount : channelsCount;
            }
            set
            {
                channelsCount = AmplifierCapabilities == null ? value : AmplifierCapabilities.ChannelsCount;
            }
        }
        /// <summary>
        /// Число каналов событий
        /// </summary>
        public int EventsChannelsCount
        {
            get { return Plotter.EventsChannelsCount; }
            //set { Plotter.EventsChannelsCount = value; }
        }

        private bool paused = false;
        /// <summary>
        /// Пауза
        /// </summary>
        public bool Paused
        {
            get { return paused; }
            set
            {
                paused = value;
                OnPropertyChanged("Paused");
            }
        }
        #endregion


        #region Methods

        /// <summary>
        /// Пишет данные для мониторинга. Версия для приборов поддерживающих нумерацию отсчётов.
        /// </summary>
        /// <param name="data"></param>
        //public void WriteData(Hardware.Devices.SamplesEventsCallbackArgs data)
        //{
        //    if (Paused)
        //        return;
        //    Plotter.WriteData(data);
        //}

        /// <summary>
        /// Пишет данные для мониторинга.Версия для приборов неподдерживающих нумерацию отсчётов.
        /// </summary>
        /// <param name="data"></param>
        public void WriteData(Hardware.Devices.SamplesStimulsDataEventArgs data)
        {
            if (Paused)
                return;
            Plotter.WriteData(data);
        }

        #endregion       

        #region Регулировка параметров отображения кривых

        private ScaleItem[] xScaleItems = null;
        /// <summary>
        /// 
        /// </summary>
        public ScaleItem[] XScaleItems
        {
            get
            {
                if (xScaleItems == null)
                {
                    xScaleItems = new ScaleItem[] { new ScaleItem(10f, "10  с/мм"), new ScaleItem(5f, "5 с/мм"), new ScaleItem(2f, "2 с/мм"), new ScaleItem(1f, "1 с/мм"), new ScaleItem(500e-3f, "0,5 с/мм"), 
                        new ScaleItem(200e-3f, "0,2 с/мм"), new ScaleItem(100e-3f, "0,1 с/мм"), new ScaleItem(50e-3f, "50 мс/мм"), new ScaleItem(20e-3f, "20 мс/мм"), new ScaleItem(10e-3f, "10 мс/мм"), 
                        new ScaleItem(5e-3f, "5 мс/мм"), new ScaleItem(2e-3f, "2 мс/мм"), new ScaleItem(1e-3f, "1 мс/мм"), new ScaleItem(500e-6f, "0,5 мс/мм"), new ScaleItem(200e-6f, "0,2 мс/мм"), 
                        new ScaleItem(100e-6f, "0,1 мс/мм"), new ScaleItem(50e-6f, "50 мкс/мм"), new ScaleItem(40e-6f, "40 мкс/мм"), new ScaleItem(20e-6f, "20 мкс/мм"), new ScaleItem(10e-6f, "10 мкс/мм"), 
                        new ScaleItem(5e-6f, "5 мкс/мм"), new ScaleItem(2e-6f, "2 мкс/мм"), new ScaleItem(1e-6f, "1 мкс/мм") };
                }
                return xScaleItems;
            }
        }

        private ScaleItem selectedXScaleItem;

        /// <summary>
        /// 
        /// </summary>
        public ScaleItem SelectedXScaleItem
        {
            get { return selectedXScaleItem; }
            set
            {
                if (selectedXScaleItem != value)
                {
                    selectedXScaleItem = value;
                    if (selectedXScaleItem != null)
                    {
                        Plotter.TimeScaleInSecondsPerMillimeter = selectedXScaleItem.Scale;
                    }
                    OnPropertyChanged("SelectedXScaleItem");
                }
            }
        }

        private ScaleItem[] yScaleItems = null;
        /// <summary>
        /// 
        /// </summary>
        public ScaleItem[] YScaleItems
        {
            get
            {
                if (yScaleItems == null)
                {
                    yScaleItems = new ScaleItem[] { new ScaleItem(10e-9f, "10 нВ/мм"), new ScaleItem(20e-9f, "20 нВ/мм"), new ScaleItem(50e-9f, "50 нВ/мм"), 
                        new ScaleItem(100e-9f, "100 нВ/мм"), new ScaleItem(200e-9f, "200 нВ/мм"), new ScaleItem(500e-9f, "500 нВ/мм"), new ScaleItem(1e-6f, "1 мкВ/мм"), 
                        new ScaleItem(2e-6f, "2 мкВ/мм"), new ScaleItem(5e-6f, "5 мкВ/мм"), new ScaleItem(10e-6f, "10 мкВ/мм"), new ScaleItem(20e-6f, "20 мкВ/мм"), 
                        new ScaleItem(50e-6f, "50 мкВ/мм"), new ScaleItem(100e-6f, "100 мкВ/мм"), new ScaleItem(200e-6f, "200 мкВ/мм"), new ScaleItem(500e-6f, "500 мкВ/мм"), 
                        new ScaleItem(1e-3f, "1 мВ/мм"), new ScaleItem(2e-3f, "2 мВ/мм"), new ScaleItem(5e-3f, "5 мВ/мм"), new ScaleItem(10e-3f, "10 мВ/мм"), 
                        new ScaleItem(20e-3f, "20 мВ/мм"), new ScaleItem(50e-3f, "50 мВ/мм"), new ScaleItem(100e-3f, "100 мВ/мм"), new ScaleItem(200e-3f, "200 мВ/мм"), 
                        new ScaleItem(500e-3f, "500 мВ/мм") };
                }
                return yScaleItems;
            }
        }

        private ScaleItem selectedYScaleItem;
        /// <summary>
        /// 
        /// </summary>
        public ScaleItem SelectedYScaleItem
        {
            get { return selectedYScaleItem; }
            set
            {
                if (selectedYScaleItem != value)
                {
                    selectedYScaleItem = value;
                    SynchronizeYScale();
                    OnPropertyChanged("SelectedYScaleItem");
                }
            }
        }

        private void SynchronizeYScale()
        {
            if (selectedYScaleItem != null)
            {
                for (int i = Plotter.ChannelsCount - 1; i >= 0; i--)
                    Plotter.SetChannelScale(i, SelectedYScaleItem.Scale);
                Plotter.MeasureAmplitudeScaleInUnitsPerMillimeter = SelectedYScaleItem.Scale;
            }
        }
        public void InitChannels(NeuroSoft.Hardware.Devices.Controls.DataPlotter.ChannelParams[] channelParams, int curvesStep = 30, int eventsChannelsCount = 1)
        {
            Plotter.Configure(channelParams, eventsChannelsCount);
            for (int i = 0; i < channelParams.Length; i++)
            {
                Plotter.SetChannelPosition(i, curvesStep * (i + 1));
            }
            SynchronizeYScale();
        }

        #endregion 

        #region IDisposable
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Plotter.Dispose();
            plotterHost.Child = null;
        }

        #endregion

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
