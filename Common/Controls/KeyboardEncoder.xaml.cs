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

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// Interaction logic for KeyboardEncoder.xaml
    /// </summary>
    public partial class KeyboardEncoder : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public KeyboardEncoder()
        {
            InitializeComponent();
        }

        /// <summary>
        /// угол поворота энкодера
        /// </summary>
        public double EncoderAngle
        {
            get { return (double)GetValue(EncoderAngleProperty); }
            set { SetValue(EncoderAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EncoderAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EncoderAngleProperty =
            DependencyProperty.Register("EncoderAngle", typeof(double), typeof(KeyboardEncoder), new UIPropertyMetadata(0d));

        /// <summary>
        /// Цвет энкодера
        /// </summary>
        public Brush EncoderBrush
        {
            get { return (Brush)GetValue(EncoderBrushProperty); }
            set { SetValue(EncoderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EncoderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EncoderBrushProperty =
            DependencyProperty.Register("EncoderBrush", typeof(Brush), typeof(KeyboardEncoder), new UIPropertyMetadata(Brushes.Transparent));

        
    }
}
