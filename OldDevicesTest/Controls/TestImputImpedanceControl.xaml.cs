using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using NeuroSoft.Devices;
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for TestImputImpedanceControl.xaml
    /// </summary>
    public partial class TestImputImpedanceControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        public TestImputImpedanceControl(TestInputImpedanceViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            MonitoringPlotter.SelectedYScaleItem = MonitoringPlotter.YScaleItems[12];
        }

        private TestInputImpedanceViewModel viewModel;

        /// <summary>
        /// Модель представления
        /// </summary>
        public TestInputImpedanceViewModel ViewModel
        {
            get { return viewModel; }
            private set
            {
                if (viewModel != value)
                {
                    if (viewModel != null)
                    {
                        viewModel.RecieveData -= new RecieveDataHandler(ViewModel_RecieveData);
                    }
                    viewModel = value;
                    MonitoringPlotter.AmplifierCapabilities = new AmplifierCapabilities(true, viewModel.ChannelsCount, null, null, null, null, null, 0, null, false, viewModel.ChannelsCount * 2);
                    MonitoringPlotter.SamplesRateInSamplesPerSecond = ViewModel.SamplesRateInSamplesPerSecond;
                    DataContext = ViewModel;
                    ViewModel.RecieveData += new RecieveDataHandler(ViewModel_RecieveData);
                    OnPropertyChanged("ViewModel");
                }
            }
        }

        void ViewModel_RecieveData(object sender, ReceiveDataArgs e)
        {
            SamplesStimulsDataEventArgs DataEventArgs = new SamplesStimulsDataEventArgs();
            DataEventArgs.ChannelsSamples = e.Data;
            DataEventArgs.SamplesCount = e.Data[1].Length;
            MonitoringPlotter.WriteData(DataEventArgs);
        }

        #region IDisposable
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (viewModel != null)
            {
                viewModel.RecieveData -= new RecieveDataHandler(ViewModel_RecieveData);
                viewModel.StopReadData();
            }
            MonitoringPlotter.Dispose();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StopStart();
        }
    }
}
