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
using System.Collections.ObjectModel;
using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.DeviceAutoTest.Common.Scripts;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for TestAFCControl.xaml
    /// </summary>
    public partial class TestAFCControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public TestAFCControl(TestAFCViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            MonitoringPlotter.SelectedYScaleItem = MonitoringPlotter.YScaleItems[12];
        }

        private TestAFCViewModel viewModel;

        /// <summary>
        /// Модель представления
        /// </summary>
        public TestAFCViewModel ViewModel
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
                    MonitoringPlotter.AmplifierCapabilities = ViewModel.AmplifierCapabilities;
                    MonitoringPlotter.SamplesRateInSamplesPerSecond = ViewModel.SamplesRateInSamplesPerSecond;
                    DataContext = ViewModel;
                    ViewModel.RecieveData += new RecieveDataHandler(ViewModel_RecieveData);
                    OnPropertyChanged("ViewModel");
                }
            }
        }

        void ViewModel_RecieveData(object sender, SamplesStimulsDataEventArgs e)
        {
            MonitoringPlotter.WriteData(e);
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
            ViewModel.StopOrStart();            
        }

        private void AFCDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AFCDataGrid.ScrollIntoView(AFCDataGrid.SelectedItem);
        }

    }    
}
