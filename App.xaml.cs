using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.WPFPrototype.Interface.Common;
using System.Windows.Markup;
using System.Globalization;
using System.Windows.Interop;
using System.Windows.Input;
using System.Threading;
using System.Windows.Controls;
using NeuroSoft.WPFComponents.CommandManager;
using System.Windows.Threading;
using NeuroSoft.Components.Depository;
using System.IO;


namespace NeuroSoft.DeviceAutoTest
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // By default, WPF menus will acquire Win32 focus for the HwndSource in which they are contained. 
            // This flag prevents WPF menus from acquiring Win32 focus when they receive WPF focus. 
            HwndSource.DefaultAcquireHwndFocusInMenuMode = false;

            // Ensure that the MainWindow's HwndSource and all subsequently-created 
            // HwndSources on the UI thread have their RestoreFocusMode set appropriately. 
            Keyboard.DefaultRestoreFocusMode = RestoreFocusMode.None;
            
            // Установим для XAML (в том числе для биндинга) текущий язык из CultureInfo
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            //Добавление поддержки темы Aero для AvalonDock (не переносить в App.xaml - находит ошибки, если есть прямые ссылки на используемые проекты)
            var rd = new ResourceDictionary();
            rd.Source = new Uri("/AvalonDock;component/themes/aero.normalcolor.xaml", UriKind.RelativeOrAbsolute);
            Resources.MergedDictionaries.Add(rd);

            // Размер иконок по умолчанию
            //CommandDescription.ImageSizeProperty.OverrideMetadata(typeof(Control), new FrameworkPropertyMetadata(30.0, FrameworkPropertyMetadataOptions.Inherits));

            System.Windows.Forms.Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            
            Globals.UpdateDepositoryNeeded += (UpdateDepositoryArgs ee) => { UpdateDepositoryNeeded(ee); };

            // Обработчики, вызываемые при необработанных исключениях
            // Навешиваем только если работа без отладчика
            if (!System.Diagnostics.Debugger.IsAttached)    // Изменено определение режима запуска под отладкой
            {
                AppDomain.CurrentDomain.UnhandledException += (object sndr, UnhandledExceptionEventArgs ee) => { if (ee.ExceptionObject is Exception) ShowException((Exception)ee.ExceptionObject); };
                System.Windows.Forms.Application.ThreadException += (object sndr, System.Threading.ThreadExceptionEventArgs ee) => { ShowException(ee.Exception); };
                DispatcherUnhandledException += (object sndr, DispatcherUnhandledExceptionEventArgs ee) =>
                {
                    ShowException(ee.Exception);
                    ee.Handled = true;
                };
                if (!Globals.SelectCurrentUser()) { Shutdown(); return; }
            }
            else
            {
                // Это для того, чтобы после остановки в отладчике прибор не генерировал критическую ошибку
                //NeuroSoft.Hardware.Devices.Base.NsDevice.IsDebugMode = true;
                Globals.InitDepository();
                string userName = Globals.Depository.Read("Common", DepositoryKeys.CurrentUser, null) as string;
                if (userName != null)
                {
                    Globals.UserName = userName; //если текущий пользователь является администратором, то в режиме отладки войдем без запроса пароля                    
                    Globals.Password = Globals.GetPassword(userName);
                    if (!Globals.IsAdmin())
                    {
                        if (!Globals.SelectCurrentUser()) { Shutdown(); return; }
                    }
                }
            }

            SplashForm.ShowSplash();
            Globals.UpdateSettings();
            
            Settings.Load();

            CommandSettings.CommandToolTipStyle = CommandToolTipStyle.AlwaysWithGesture;

            MainWindow mainWindow = new MainWindow(new MainModel());
            mainWindow.Show();
        }

        /// <summary>
        /// Вызывается в случае изменения пользователя после update программы
        /// </summary>
        /// <param name="e"></param>
        public static void UpdateDepositoryNeeded(UpdateDepositoryArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)     // Изменено определение режима запуска под отладкой
                return;
            // Файл с настройками пользователя
            // Если файла еще нет, то это просто новый пользователь, ничего делать не надо, новые настройки для него 
            // создадутся автоматически
            string oldFileName = Globals.GetConfigFolder() + Path.DirectorySeparatorChar + Globals.UserName + ".Default.NSConfig";
            if (!File.Exists(oldFileName))
                return;
            string defaultFileName = Globals.GetConfigFolder() + "\\" + "DefaultOptions.NSConfig";
			if(File.Exists(defaultFileName))
                Globals.UpgradeSettings(defaultFileName, 1);
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (Thread.CurrentThread == Dispatcher.Thread) // без этой проверки метод Shutdown вызывается после печати и программа падает
            {
                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        /// <summary>
        /// Выдача сообщения об Exception
        /// </summary>
        /// <param name="e"></param>
        private static void ShowException(Exception e)
        {
            if (e != null)
            {
                NeuroSoft.WPFComponents.ScalableWindows.NSMessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
