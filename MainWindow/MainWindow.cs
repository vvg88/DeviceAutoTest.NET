using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.WPFPrototype.Interface;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using NeuroSoft.WPFPrototype.Interface.Common;
using System.Windows.Media.Imaging;
using NeuroSoft.DeviceAutoTest.Commands;
using NeuroSoft.WPFComponents.CommandManager;
using NeuroSoft.WPFPrototype.Interface.Commands;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using NeuroSoft.Devices;
using System.Windows.Interop;
using NeuroSoft.DeviceAutoTest.TestTemplateEditor;
using NeuroSoft.Hardware;
//using NeuroSoft.DeviceAutoTest.Views
using NeuroSoft.Prototype.Database;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using NeuroSoft.DeviceAutoTest.Common.Controls;
using AvalonDock;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Главное окно приложения
    /// </summary>
    public class MainWindow : ProtoMainWindow, INotifyPropertyChanged
    {
        internal ToolBar AutoTestingToolBar;

        /// <summary>
        /// Конструктор главного окна приложения
        /// </summary>
        /// <param name="model">Модель приложения</param>
        public MainWindow(WPFPrototypeModel model)
            : base(model)
        {            
            ResourceDictionary dictionary = new ResourceDictionary();
            dictionary.Source = new Uri("pack://application:,,,/NeuroSoft.DeviceAutoTest;component/MainWindow/MainWindowDictionary.xaml");
            RegisterMenu((Menu)dictionary["AdditionalMainMenu"]);
            //Изменим текст команды открытия обследования (начала тестирования)
            var newCheckupCommandDescriptor = CommandRepository.GetCommandDescription(CheckupCommands.NewCommand);
            newCheckupCommandDescriptor.Text = Properties.Resources.NewTesting;
            newCheckupCommandDescriptor.ToolTip = Properties.Resources.NewTestingTooltip;

            var openCommandDescriptor = CommandRepository.GetCommandDescription(CheckupCommands.OpenCommand);
            //openCommandDescriptor.Text = Properties.Resources.StartTestingText;
            //openCommandDescriptor.ToolTip = Properties.Resources.StartTestingToolTip;
            var closeCommandDescriptor = CommandRepository.GetCommandDescription(CheckupCommands.CloseCommand);
            closeCommandDescriptor.Text = Properties.Resources.CloseTesting;
            closeCommandDescriptor.ToolTip = Properties.Resources.CloseTesting;
            var saveCommandDescriptor = CommandRepository.GetCommandDescription(CheckupCommands.SaveCommand);
            saveCommandDescriptor.Text = Properties.Resources.SaveTesting;
            saveCommandDescriptor.ToolTip = Properties.Resources.SaveTesting;

            //Заменим имя группы Обследование на Тестирование
            CommandRepository.GetGroupDescription(CommandGroupNames.CheckupCommandsGroup).Text = Properties.Resources.TestingCheckupGroupName;
            //Инициализация CommandBindings
            InitializeCommandBindings();
            //Model.UseWord = true;
            UpdateProtocolPatternList();            
            //ProtocolPattern.DoubleSignificantDigitsCount = -1;
            ActiveChildContentChanged += new ActiveChildContentChangedEventHandler(MainWindow_ActiveChildContentChanged);
            Icon = Services.ToImageSource(new System.Drawing.Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("NeuroSoft.DeviceAutoTest.DATIcon.ico")));                       
            Prototype.Interface.Database.JournalFormModel.ObjectInfoImported += new Prototype.Interface.Database.JournalFormModel.ObjectInfoImportedDelegate(JournalFormModel_ObjectInfoImported);
            Prototype.Interface.Database.JournalFormModel.BeforeExportArchiveClosing += new Prototype.Interface.Database.JournalFormModel.BeforeAcrhiveClosing(JournalFormModel_BeforeExportArchiveClosing);
            Title = string.Format("DeviceAutoTest.NET v.{0}", Assembly.GetExecutingAssembly().GetName().Version);

            ResourceDictionary commonResources = new ResourceDictionary();
            commonResources.Source = new Uri("NeuroSoft.DeviceAutoTest;Component/Resources/Common.xaml", UriKind.Relative);
            AutoTestingToolBar = (ToolBar)commonResources["AutoTestingDefaultToolBar"];
            AutoTestingToolBar.Visibility = Visibility.Collapsed;
            //TestDefaultToolBar.DataContext = DataContext;
            AutoTestingToolBar.Name = "AutoTestingDefaultToolBar";
            RegisterToolBar(AutoTestingToolBar);

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        internal void UpdateAutoTestingToolBarState(WPFMainCheckupManager manager = null)
        {
            if (manager == null)
                manager = CurrentMainCheckupManager;
            if (AutoTestingToolBar != null)
            {
                AutoTestingToolBar.Visibility = manager == null || manager.CheckupAncestor is DeviceTestCheckupAncestor && 
                    (manager.CheckupAncestor as DeviceTestCheckupAncestor).AutoTestGroups.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                AutoTestingToolBar.DataContext = manager;
            }
        }

        void JournalFormModel_BeforeExportArchiveClosing(Prototype.Database.DataConnection archive)
        {
            List<string> templateKeys = new List<string>();
            foreach (var patient in archive.GetPacientInfoList())
            {
                foreach (var checkup in archive.GetCheckupInfoList(false, patient))
                {
                    string testTemplateKey = checkup.ReadParam(DeviceTestCheckupAncestor.TemplateKeyParam, null) as string;
                    if (testTemplateKey != null && !templateKeys.Contains(testTemplateKey))
                    {
                        byte[] templateData = Globals.CurrentConnection.Connection.ReadData(testTemplateKey);
                        if (templateData != null)
                        {
                            if (templateData != null)
                            {
                                archive.WriteData(testTemplateKey, templateData);
                            }
                        }
                        templateKeys.Add(testTemplateKey);
                    }
                }
            }
        }

        void JournalFormModel_ObjectInfoImported(Prototype.Database.ObjectInfo objectInfo)
        {
            var checkupInfo = objectInfo as CheckupInfo;
            if (checkupInfo != null)
            {
                string testTemplateKey = checkupInfo.ReadParam(DeviceTestCheckupAncestor.TemplateKeyParam, null) as string;
                if (testTemplateKey != null)
                {
                    //Импортируем инструкцию, если её нет в текущей базе
                    if (Globals.CurrentConnection.Connection.ReadData(testTemplateKey) == null)
                    {
                        byte[] data = objectInfo.DataConnection.ReadData(testTemplateKey);
                        if (data != null)
                        {
                            Globals.CurrentConnection.Connection.WriteData(testTemplateKey, data);
                        }
                    }
                }
            }
        }

        void MainWindow_ActiveChildContentChanged(ChildContent OldChildContent, ChildContent NewChildContent)
        {
            if (OldChildContent is TestDockableContent)
            {
                (OldChildContent as TestDockableContent).OnDeactivated();
            }
            if (NewChildContent is TestDockableContent)
            {
                (NewChildContent as TestDockableContent).OnActivated();
            }            
        }

        OscScopeControl oscScopeCntrl;
        /// <summary>
        /// Контрол со списком осциллографов
        /// </summary>
        public OscScopeControl OscScopeCntrl
        {
            get { return oscScopeCntrl; }
            private set { oscScopeCntrl = value; }
        }

        private UsbDevicesListControl usbDevsListControl;
        /// <summary>
        /// Контрол со списком подключенных приборов
        /// </summary>
        public UsbDevicesListControl UsbDevsListControl
        {
            get { return usbDevsListControl; }
            private set { usbDevsListControl = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            oscScopeCntrl = new OscScopeControl();
            ShowOscScope();

            usbDevsListControl = new UsbDevicesListControl();
            ShowUsbDevsList();
        }

        private void ShowOscScope()
        {
            try
            {
                object obj = Globals.Depository.Read(Properties.Resources.OscScopeControlSettings, "State", null);
                string strXML = obj as string;
                if (!string.IsNullOrEmpty(strXML))
                    DockableContentStateInfo.SetContentStateFromXMLString(DockingManager, strXML, oscScopeCntrl);
                else
                {
                    DockableContentStateInfo dockStateInfo = new DockableContentStateInfo();
                    dockStateInfo.TopMost = true;
                    DockableContentStateInfo.SetContentState(DockingManager, dockStateInfo, oscScopeCntrl);
                    DockingManager.Show(oscScopeCntrl, DockableContentState.AutoHide, AnchorStyle.Right);
                    Globals.Depository.CurrentPath = Properties.Resources.OscScopeControlSettings;
                    Globals.Depository.DeleteNode("State");
                }
            }
            catch
            {
                DockingManager.Show(oscScopeCntrl, DockableContentState.AutoHide, AnchorStyle.Right);
                Globals.Depository.CurrentPath = Properties.Resources.OscScopeControlSettings;
                Globals.Depository.DeleteNode("State");
            }
        }

        private void ShowUsbDevsList()
        {
            try
            {
                var strXML = Globals.Depository.Read(Properties.Resources.UsbDevsListControlSettings, "State", null) as string;
                if (!string.IsNullOrEmpty(strXML))
                    DockableContentStateInfo.SetContentStateFromXMLString(DockingManager, strXML, usbDevsListControl);
                else
                {
                    var dockStateInfo = new DockableContentStateInfo();
                    dockStateInfo.TopMost = true;
                    DockableContentStateInfo.SetContentState(DockingManager, dockStateInfo, usbDevsListControl);
                    DockingManager.Show(usbDevsListControl, DockableContentState.AutoHide, AnchorStyle.Right);
                    Globals.Depository.CurrentPath = Properties.Resources.UsbDevsListControlSettings;
                    Globals.Depository.DeleteNode("State");
                }
            }
            catch
            {
                DockingManager.Show(usbDevsListControl, DockableContentState.AutoHide, AnchorStyle.Right);
                Globals.Depository.CurrentPath = Properties.Resources.UsbDevsListControlSettings;
                Globals.Depository.DeleteNode("State");
            }
        }

        protected override void SaveSettings()
        {
            try
            {
                string oscControlState = DockableContentStateInfo.GetContentStateAsXMLString(oscScopeCntrl);
                if (!string.IsNullOrEmpty(oscControlState))
                    Globals.Depository.Write(Properties.Resources.OscScopeControlSettings, "State", oscControlState);

                var usbDevsListState = DockableContentStateInfo.GetContentStateAsXMLString(usbDevsListControl);
                if (!string.IsNullOrEmpty(usbDevsListState))
                    Globals.Depository.Write(Properties.Resources.UsbDevsListControlSettings, "State", usbDevsListState);
            }
            catch { }
            base.SaveSettings();
        }

        #region Properties
        IntPtr handle;

        /// <summary>
        /// Хэндл окна
        /// </summary>
        public IntPtr Handle
        {
            get
            { 
                if (handle == IntPtr.Zero)
                    handle = new WindowInteropHelper(this).Handle;
                return handle;
            }
        }

        int newCorrectionsCount = -1;
        /// <summary>
        /// 
        /// </summary>
        public int NewCorrectionsCount
        {
            get
            {
                if (!Globals.IsAdmin())
                    return 0;
                if (newCorrectionsCount < 0)
                {
                    newCorrectionsCount = 0;
                    foreach (var descr in DATTemplate.TestTemplateDescriptors)
                    {
                        descr.CheckForNewCorrections();
                        newCorrectionsCount += descr.NewCorrectionsCount;
                    }
                }
                return newCorrectionsCount;
            }
        }
        #endregion

        #region Commands
        private void InitializeCommandBindings()
        {
            //Удаление неиспользуемых команд, унаследованных от прототипа
            List<RoutedCommand> commandsToRemove = new List<RoutedCommand>() 
            { 
                CheckupCommands.AnamnesisCommand, CheckupCommands.ConclusionCommand, CheckupCommands.CopyToCommand, 
                CheckupCommands.CopyToFolderCommand, CheckupCommands.CopyToSelectFolderCommand, 
                CheckupCommands.ExistingPacientCommand, CheckupCommands.InfoCommand, CheckupCommands.SaveAllCommand,
                CheckupCommands.SendByMailCommand, SettingsCommands.TouchStyleCommand, HelpCommands.AboutCommand
            };   
            RemoveCommands(commandsToRemove);
            // Команда редактирования сценариев тестирования
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.OpenTestTemplateCommand,
                (object sender, ExecutedRoutedEventArgs e) =>
                {
                    DATTemplate.OpenTestTemplate();
                }, 
                (object sender, CanExecuteRoutedEventArgs e) =>
                {
                    e.CanExecute = Globals.IsAdmin();
                }));
            // Команда редактирования используемой в данный момент инструкции по наладке
            CommandBindings.Add(new CommandBinding(DATTemplateCommands.EditUsedTestTemplateCommand,
                (object sender, ExecutedRoutedEventArgs e) =>
                {
                    var DATCheckupManager = CurrentMainCheckupManager as DeviceTestCheckupManager;
                    MainModel model = DATCheckupManager.Model as MainModel;
                    model.LockClosing = true;
                    try
                    {
                        foreach (var form in DATCheckupManager.GetForms())
                        {
                            if (form is TestDockableContent)
                            {
                                form.Close();
                            }
                        }
                        Services.DoEvents();
                    }
                    finally
                    {
                        model.LockClosing = false;
                    }                    
                    DATCheckupManager.TestsAncestor.EditCurrentTestTemplate();
                },
                (object sender, CanExecuteRoutedEventArgs e) =>
                {
                    var DATCheckupManager = CurrentMainCheckupManager as DeviceTestCheckupManager;
                    e.CanExecute = Globals.IsAdmin() && DATCheckupManager != null && DATCheckupManager.TestsAncestor != null && DATCheckupManager.TestsAncestor.TestTemplate != null;
                }));
            // Команда просмотра истории тестирования
            CommandBindings.Add(new CommandBinding(DATTestingCommands.ShowTestingHistoryCommand,
                (object sender, ExecutedRoutedEventArgs e) =>
                {                    
                    new TestingHistoryListDialog(CurrentMainWindow.Model.CreateJournalModel()).ShowDialog();
                }));

            CommandBindings.Add(new CommandBinding(DATTestingCommands.StartAutoTestCommand,
                (object sender, ExecutedRoutedEventArgs e) =>
                {
                    var DATCheckupManager = CurrentMainCheckupManager as DeviceTestCheckupManager;
                    DATCheckupManager.CurrentAutoTestManager.Start();
                },
                (object sender, CanExecuteRoutedEventArgs e) =>
                {
                    var DATCheckupManager = CurrentMainCheckupManager as DeviceTestCheckupManager;
                    e.CanExecute = DATCheckupManager != null && DATCheckupManager.CurrentAutoTestManager != null
                        && DATCheckupManager.CurrentAutoTestManager.CanStart;
                }));

            CommandBindings.Add(new CommandBinding(DATTestingCommands.StopAutoTestCommand,
                (object sender, ExecutedRoutedEventArgs e) =>
                {
                    var DATCheckupManager = CurrentMainCheckupManager as DeviceTestCheckupManager;
                    DATCheckupManager.CurrentAutoTestManager.Stop(true);
                },
                (object sender, CanExecuteRoutedEventArgs e) =>
                {
                    var DATCheckupManager = CurrentMainCheckupManager as DeviceTestCheckupManager;
                    e.CanExecute = DATCheckupManager != null && DATCheckupManager.CurrentAutoTestManager != null
                        && DATCheckupManager.CurrentAutoTestManager.CanStop;
                }));

            CommandBindings.Add(new CommandBinding(DATSettingsCommands.ToggleAutoSaveCommand,
                (object sender, ExecutedRoutedEventArgs e) =>
                {
                    MainModel model = Model as MainModel;
                    model.AutoSave = !model.AutoSave;
                },
                (object sender, CanExecuteRoutedEventArgs e) =>
                {
                    MainModel model = Model as MainModel;
                    CommandHelper.UpdateCheckedCommandSource(e.Parameter, model.AutoSave);
                    e.CanExecute = true;
                }));

            CommandBindings.Add(new CommandBinding(HelpCommands.AboutCommand, (object sender, ExecutedRoutedEventArgs e) =>
                {

                    AboutFormWPF.AppNameFontSize = 29;
                    AboutFormWPF.Lang = AboutFormWPF.lang.combined;
                    AboutFormWPF.ReleaseDate = File.GetLastWriteTime(System.Windows.Forms.Application.ExecutablePath).ToShortDateString();
                    AboutFormWPF.SuppDevs = null;
                    for (int i = 0; i < Common.DevicesManipulation.AvailableDevices.Count; i++)
                    {
                        AboutFormWPF.SuppDevs += (AboutFormWPF.Lang == AboutFormWPF.lang.combined) ?
                            (Common.DevicesManipulation.AvailableDevices[i].DeviceType + " (" + Common.DevicesManipulation.AvailableDevices[i].Name + ")") : 
                            ((AboutFormWPF.Lang == AboutFormWPF.lang.english) ? Common.DevicesManipulation.AvailableDevices[i].DeviceType : Common.DevicesManipulation.AvailableDevices[i].Name);
                        if (i == Common.DevicesManipulation.AvailableDevices.Count - 1)
                            break;
                        AboutFormWPF.SuppDevs += "\n";
                    }
                    AboutFormWPF.ShowAbout();
                }));
            
        }

        private void RemoveCommands(List<RoutedCommand> commands)
        {
            if (commands == null)
                return;
            foreach (var command in commands)
            {
                CommandRepository.RemoveCommand(command);
                CommandBinding commandBinding = null;
                foreach (CommandBinding cb in CommandBindings)
                {
                    if (cb.Command == command)
                    {
                        commandBinding = cb;
                        break;
                    }
                }
                if (commandBinding != null)
                {
                    CommandBindings.Remove(commandBinding);
                }
                foreach (var toolbar in RegisteredToolbars)
                {
                    for (int i = toolbar.Items.Count-1; i >= 0; i--)
                    {
                        Button btn = toolbar.Items[i] as Button;
                        if (btn != null && btn.Command == command)
                        {
                            toolbar.Items.RemoveAt(i);
                        }
                    }
                }                
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public static void UpdateNewCorrectionsCount()
        {
            var mainWindow = MainWindow.CurrentMainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.newCorrectionsCount = -1;
                mainWindow.OnPropertyChanged("NewCorrectionsCount");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // Расширение оконной функции 
            HwndSource source = HwndSource.FromHwnd(Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_DEVICECHANGE = 537;

            // Переинициализация HID-устройств, если они менялись
            if (msg == WM_DEVICECHANGE)
            {
                NSHIDDevices.MainFrmDeviceChanges();
                handled = true;
            }

            return IntPtr.Zero;
        }

        internal void NSHIDDevices_ReceivePad(uint nsPadState, uint nsPadChange, int syncValue)
        {
            if (nsPadState == 1)
            {
                DATTestingCommands.PressButton1Command.Execute(null, this);
            }
            else if (nsPadState == 2)
            {
                DATTestingCommands.PressButton2Command.Execute(null, this);
            }
            else if (nsPadState == 4)
            {
                DATTestingCommands.PressButton3Command.Execute(null, this);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Закроем HID-устройства
            NSHIDDevices.Close();            
        }

        internal void UpdateProtocolPatternList()
        {
            //ProtocolPattern.CurrentGroup = DATTemplate.CurrentTemplate != null ? DATTemplate.CurrentTemplate.GUID.ToString() : "None";
            ProtocolPattern.UpdateProtocolPatternDescriptorsList();
            UpdateTemplateList();
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
