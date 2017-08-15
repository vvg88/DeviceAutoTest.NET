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
using NeuroSoft.Hardware.Devices.Base;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.Hardware.Common;
//using System.Drawing;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for DisplayControl.xaml
    /// </summary>
    public partial class DisplayControl : UserControl, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public DisplayControl(NeuroMepBase device)
        {
            InitializeComponent();
            Device = device;
            device.Display.SwitchOn();
            DataContext = this;
        }

        #region Properties

        private NeuroMepBase device;
        /// <summary>
        /// 
        /// </summary>
        public NeuroMepBase Device
        {
            get { return device; }
            private set { device = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public float DisplayContrast
        {
            get { return device.Display.Contrast; }
            set 
            {
                if (value >= 0 && value <= 1)
                {
                    try
                    {
                        device.Display.Contrast = value;
                        contrastChanged = true;
                    }
                    catch(DeviceErrorException e)
                    {
                        contrastChanged = false;
                        CommonScripts.ShowError(e.ErrorDescriptor.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float DisplayBrightness
        {
            get { return device.Display.Brightness; }
            set 
            {
                if (value >= 0 && value <= 1)
                {
                    try
                    {
                        device.Display.Brightness = value;
                        brightnessChanged = true;
                    }
                    catch (DeviceErrorException e)
                    {
                        brightnessChanged = false;
                        CommonScripts.ShowError(e.ErrorDescriptor.Message);
                    }
                }
            }
        }

        private BitmapImage testDisplayImage;
        /// <summary>
        /// 
        /// </summary>
        public BitmapImage TestDisplayImage
        {
            get 
            {
                if (testDisplayImage == null)
                {
                    //testDisplayImage = new BitmapImage(new Uri("pack://application:,,,/NeuroSoft.DeviceAutoTest.Common;component/Resources/TestDisplayImage.jpg"));
                    testDisplayImage = new BitmapImage(new Uri("pack://application:,,,/NeuroSoft.DeviceAutoTest.Common;component/Resources/Impedance.jpg"));
                }
                return testDisplayImage; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double DisplayWidth
        {
            get
            {
                return device.Display.CapSizeInPixels.X;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double DisplayHeight
        {
            get
            {
                return device.Display.CapSizeInPixels.Y;
            }
        }

        private bool whitePressed = false;
        private bool blackPressed = false;
        private bool redPressed = false;
        private bool greenPressed = false;
        private bool bluePressed = false;
        private bool imagePressed = false;
        private bool contrastChanged = false;
        private bool brightnessChanged = false;
        /// <summary>
        /// 
        /// </summary>
        public bool AllChecked
        {
            get
            {
                return whitePressed && blackPressed && redPressed && greenPressed && bluePressed && imagePressed && contrastChanged && brightnessChanged;
            }
        }

        private bool settingsSaved;
        /// <summary>
        /// Признак, что настройки были сохранены
        /// </summary>
        public bool SettingsSaved
        {
            get { return settingsSaved; }
            private set { settingsSaved = value; }
        }
        

        /// <summary>
        /// 
        /// </summary>
        public Brush DisplayBrush
        {
            get { return (Brush)GetValue(DisplayBrushPropertyProperty); }
            set { SetValue(DisplayBrushPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayBrushProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayBrushPropertyProperty =
            DependencyProperty.Register("DisplayBrush", typeof(Brush), typeof(DisplayControl), new UIPropertyMetadata(Brushes.LightGray));
        
        #endregion        
    
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (device != null)
            {
                device.Display.SwitchOff();
                device = null;
            }
        }

        private void FillWhiteBtn_Click(object sender, RoutedEventArgs e)
        {
            DisplayBrush = Brushes.White;
            DisplayFill(System.Drawing.Brushes.White);
            whitePressed = true;
        }

        private void FillBlackBtn_Click(object sender, RoutedEventArgs e)
        {
            DisplayBrush = Brushes.Black;
            DisplayFill(System.Drawing.Brushes.Black);
            blackPressed = true;
        }

        private void FillRedBtn_Click(object sender, RoutedEventArgs e)
        {
            DisplayBrush = Brushes.Red;
            DisplayFill(System.Drawing.Brushes.Red);
            redPressed = true;
        }

        private void FillGreenBtn_Click(object sender, RoutedEventArgs e)
        {
            DisplayBrush = Brushes.Green;
            DisplayFill(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 255, 0)));
            greenPressed = true;
        }

        private void FillBlueBtn_Click(object sender, RoutedEventArgs e)
        {
            DisplayBrush = Brushes.Blue;
            DisplayFill(System.Drawing.Brushes.Blue);
            bluePressed = true;
        }

        private void FillImageBtn_Click(object sender, RoutedEventArgs e)
        {
            DisplayBrush = new ImageBrush(TestDisplayImage);
            //System.Drawing.Image image = NeuroSoft.DeviceAutoTest.Common.Properties.Resources.TestDisplayImage;
            System.Drawing.Image image = NeuroSoft.DeviceAutoTest.Common.Properties.Resources.Impedance;
            device.Display.DrawImage(image, new System.Drawing.Rectangle(new System.Drawing.Point(), image.Size), System.Drawing.GraphicsUnit.Pixel);
            imagePressed = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                device.Display.StoreBrightnessAndContrast();
                SettingsSaved = true;
            }
            catch (DeviceErrorException exp)
            {
                SettingsSaved = false;
                CommonScripts.ShowError(exp.ErrorDescriptor.Message);
            }
        }

        private void DisplayFill(System.Drawing.Brush brush)
        {
            //var rect = System.Drawing.Rectangle.Intersect(device.Display., new Rectangle(new Point(), moduleDisplay.CapSizeInPixels));
            //if (rect.IsEmpty)
            //    return;

            using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(device.Display.CapSizeInPixels.X, device.Display.CapSizeInPixels.Y))
            {
                System.Drawing.Rectangle srcRect = new System.Drawing.Rectangle(new System.Drawing.Point(), device.Display.CapSizeInPixels);

                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm);
                g.FillRectangle(brush, srcRect);
                g.Dispose();

                try
                {
                    device.Display.DrawImage(bm, srcRect, System.Drawing.GraphicsUnit.Pixel);
                }
                catch (DeviceErrorException exc)
                {
                    CommonScripts.ShowError(exc.ErrorDescriptor.Message);
                }
            }
        }
    }
}
