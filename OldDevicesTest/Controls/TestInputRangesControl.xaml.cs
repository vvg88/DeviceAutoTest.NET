using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NeuroSoft.Devices;
using NeuroSoft.Hardware.Devices;

namespace NeuroSoft.DeviceAutoTest.OldDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for TestInputRangesControl.xaml
    /// </summary>
    public partial class TestInputRangesControl : UserControl, IDisposable
    {
        public TestInputRangesControl(TestInputRangesViewModelBase viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ViewModel.DataMonitoringPlotter = MonitoringPlotter;
            for (int i = 2; i < dataGrid.Columns.Count; i++)
            {
                var column = dataGrid.Columns[i];
                Binding binding = new Binding("ChannelsCount");
                binding.Source = viewModel;
                binding.Converter = new IsVisibleChannelConverter();
                binding.ConverterParameter = i-1;
                BindingOperations.SetBinding(column, DataGridColumn.VisibilityProperty, binding);
            }
        }

        #region Properties

        private TestInputRangesViewModelBase viewModel;

        public TestInputRangesViewModelBase ViewModel
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
            DataEventArgs.SamplesCount = e.Data[0].Length;
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
            viewModel.StopTestProcess(true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!viewModel.IsTesting)
                StartTest();
            else
                StopTest();
        }

        public void Dispose()
        {
            if (viewModel is IDisposable)
                viewModel.Dispose();
        }

        #endregion
    }

    internal class IsVisibleChannelConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int channelNum = System.Convert.ToInt32(parameter);
            int channelsCount = System.Convert.ToInt32(value);
            return channelNum <= channelsCount ? Visibility.Visible : Visibility.Collapsed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
