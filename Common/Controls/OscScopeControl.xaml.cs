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
using AvalonDock;
using System.ComponentModel;
using Neurosoft.Hardware.Tools.Visa;

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// Interaction logic for OscScopeControl.xaml
    /// </summary>
    public partial class OscScopeControl : DockableContent
    {
        public OscScopeControl()
        {
            InitializeComponent();
            DataContext = this;

            oscScope = new NS_VisaOsc();
            oscScopeSettings = new OscSettings();
        }

        NS_VisaOsc oscScope;
        /// <summary>
        /// 
        /// </summary>
        public NS_VisaOsc OscScope 
        {
            get { return oscScope; }
            private set { oscScope = value; }
        }

        OscSettings oscScopeSettings;

        private string oscScopeInfo;
        /// <summary>
        /// 
        /// </summary>
        public string OscScopeInfo
        {
            get { return oscScopeInfo; }
            set 
            { 
                oscScopeInfo = value;
                NotifyPropertyChanged("OscScopeInfo");
            }
        }

        private bool isOscScopeConnected;
        /// <summary>
        /// 
        /// </summary>
        public bool IsOscScopeConnected
        {
            get { return isOscScopeConnected; }
            set
            {
                isOscScopeConnected = value;
                NotifyPropertyChanged("IsOscScopeConnected");
            }
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (oscScope.SetupRsrcSettings() == System.Windows.Forms.DialogResult.OK)
            {
                IsOscScopeConnected = oscScope.IsRsrcUsed();
                OscScopeInfo = IsOscScopeConnected ? oscScope._idVendor.ToString() + "\n" + oscScope.GetRsrcstring() + "\n" + oscScope.GetRsrcNamestring() : string.Empty;
            }
        }
    }
}
