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
using System.Threading;
using System.Diagnostics;

namespace NeuroSoft.DeviceAutoTest.Views
{
    /// <summary>
    /// Interaction logic for AboutForm.xaml
    /// </summary>
    public partial class AboutForm : UserControl, INotifyPropertyChanged
    {
        public AboutForm()
        {
            InitializeComponent();
            DataContext = this;

            AppNameStr = AppName == null ? System.Windows.Forms.Application.ProductName : AppName;
            if (AppNameFontSize.HasValue)
                AppNameFontSizeNs = AppNameFontSize.Value;
            ReleaseDateStr = ReleaseDate;
            AppVersionStr = AppVersion == null ? System.Windows.Forms.Application.ProductVersion.ToString() : AppVersion;
            LangNs = Lang;
            SupportedOSStr = System.Environment.Is64BitOperatingSystem ? "64" : "32";
            SupportedOSStr += LangNs == lang.russian ? " бит" : " bit";
            SuppDevsStr = SuppDevs;
        }

        #region Properties

        private string extString;
        /// <summary>
        /// Текст дополнительной строки
        /// </summary>
        public string ExtStringNs
        {
            get { return extString; }
            set 
            { 
                extString = value;
                OnPropertyChanged("ExtStringNs");
            }
        }
        public static string ExtString;

        private string releaseDate;
        /// <summary>
        /// Строка с датой релиза
        /// </summary>
        public string ReleaseDateStr
        {
            get { return releaseDate; }
            set 
            {
                releaseDate = value;
                OnPropertyChanged("ReleaseDateStr");
            }
        }
        public static string ReleaseDate;

        private string suppDevs;
        /// <summary>
        /// Строка с поддерживаемыми устройствами
        /// </summary>
        public string SuppDevsStr
        {
            get { return suppDevs; }
            set 
            {
                suppDevs = value;
                OnPropertyChanged("SuppDevsStr");
            }
        }
        public static string SuppDevs;

        private string appName;
        /// <summary>
        /// Название.
        /// Если null, то значение берётся из <see cref="System.Windows.Forms.Application.ProductName"/>.
        /// </summary>
        public string AppNameStr
        {
            get { return appName; }
            set 
            {
                appName = value;
                OnPropertyChanged("AppNameStr");
            }
        }
        public static string AppName;

        private string appVersion;
        /// <summary>
        /// Версия.
        /// Если null, то значение берётся из <see cref="System.Windows.Forms.Application.ProductVersion"/>.
        /// </summary>
        public string AppVersionStr
        {
            get { return appVersion; }
            set 
            {
                appVersion = value;
                OnPropertyChanged("AppVersionStr");
            }
        }
        public static string AppVersion;

        private float appNameFontSize;
        /// <summary>
        /// Позволяет изменить размер шрифта поля названия.
        /// </summary>
        public float AppNameFontSizeNs
        {
            get { return appNameFontSize; }
            set 
            {
                appNameFontSize = value;
                OnPropertyChanged("AppNameFontSizeNs");
            }
        }

        public static float? AppNameFontSize;

        private lang llang;
        /// <summary>
        /// Поле, содеражащее язык формы
        /// </summary>
        public lang LangNs
        {
            get { return llang; }
            set 
            {
                llang = value;
                OnPropertyChanged("LangNs");
            }
        }
        public static lang Lang;

        public string LangVal
        {
            get { return Enum.GetName(typeof(lang), llang); }
        }

        private string supportedOSStr;
        /// <summary>
        /// Текущая версия ОС.
        /// </summary>
        public string SupportedOSStr
        {
            get { return supportedOSStr; }
            set 
            {
                supportedOSStr = value;
                OnPropertyChanged("SupportedOSStr");
            }
        }

        private Window abotWindow;
        
        #endregion

        #region Методы

        public static void ShowAbout()
        {
            AboutForm aForm = new AboutForm();
            aForm.abotWindow = new Window();
            aForm.abotWindow.SizeToContent = SizeToContent.WidthAndHeight;
            aForm.abotWindow.Content = aForm;
            aForm.abotWindow.WindowStyle = WindowStyle.None;
            aForm.abotWindow.ResizeMode = ResizeMode.NoResize;
            aForm.abotWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            aForm.abotWindow.ShowDialog();
        }

        #endregion

        /// <summary>
        /// Перечисление с доступными языками
        /// </summary>
        public enum lang
        {
            /// <summary>
            /// Русский.
            /// </summary>
            russian,
            /// <summary>
            /// Английский.
            /// </summary>
            english,
            /// <summary>
            /// Одновременно английский и русский.
            /// </summary>
            combined
        }

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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string url = "www.neurosoft.com" + "/" + Thread.CurrentThread.CurrentUICulture.ThreeLetterISOLanguageName + "/";
            try
            {
                Process.Start(url);
            }
            catch
            {
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            abotWindow.Close();
        }
    }
}
