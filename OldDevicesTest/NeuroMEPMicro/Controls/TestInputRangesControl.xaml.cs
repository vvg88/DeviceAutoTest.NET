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
using NeuroSoft.Devices;
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for TestInputRangesControl.xaml
    /// </summary>
    public partial class TestInputRangesControl : UserControl
    {
        public TestInputRangesControl(TestInputRangesViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        #region Properties

        private TestInputRangesViewModel viewModel;

        public TestInputRangesViewModel ViewModel
        {
            get { return viewModel; }
            private set
            {
                if (viewModel != value)
                {
                    if (viewModel != null)
                    {
                        viewModel.RecieveData -= new RecieveDataHandler(viewModel_RecieveData);
                    }
                    viewModel = value;
                    MonitoringPlotter.AmplifierCapabilities = new AmplifierCapabilities(true, viewModel.ChannelsCount, null, null, null, null, null, 0, null, false, viewModel.ChannelsCount * 2);//ViewModel.AmplifierCapabilities;
                    MonitoringPlotter.SamplesRateInSamplesPerSecond = ViewModel.SamplesRateInSamplesPerSecond;
                    viewModel.RecieveData += new RecieveDataHandler(viewModel_RecieveData);
                    DataContext = viewModel;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Обработчик события о получении данных в viewModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void viewModel_RecieveData(object sender, ReceiveDataArgs e)
        {
            SamplesStimulsDataEventArgs DataEventArgs = new SamplesStimulsDataEventArgs();
            DataEventArgs.ChannelsSamples = e.Data;
            DataEventArgs.SamplesCount = e.Data[1].Length;
            MonitoringPlotter.WriteData(DataEventArgs);
        }

        /// <summary>
        /// Запуск тестирования
        /// </summary>
        public void StartTest()
        {
            if (viewModel.TestWasLaunched)
                viewModel.InputRangeTests.RemoveAt(viewModel.CurrentTestNumber);
            if (!viewModel.TestWasLaunched && viewModel.CurrentTestNumber == 11)
                viewModel.InputRangeTests.Clear();
            viewModel.StartReadData();
        }

        /// <summary>
        /// Останвка тестирования. Завершает прием данных
        /// </summary>
        public void StopTest()
        {
            viewModel.StopTestProcess();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!viewModel.IsTesting)
                StartTest();
            else
                StopTest();
        }

        #endregion
    }
}
