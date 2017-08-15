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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using NeuroSoft.WPFComponents.NSGridView;
using NeuroSoft.WPFComponents.WPFToolkit;
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.Common;
using System.Runtime.Serialization;
using NeuroSoft.WPFComponents.CommandManager;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.Prototype.Database;
using System.ComponentModel;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.DeviceAutoTest.Common;
using NeuroSoft.Prototype.Interface.Database;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for TestingHistoryListDialog.xaml
    /// </summary>
    public partial class TestingHistoryListDialog : DATDialogWindow
    {
        private ObservableCollection<CheckupInfoWrapper> testingsInfo = new ObservableCollection<CheckupInfoWrapper>();
        /// <summary>
        /// Список наладок
        /// </summary>
        public ObservableCollection<CheckupInfoWrapper> TestingsInfo
        {
            get { return testingsInfo; }
        }

        private ICollectionView testingsInfoView;
        /// <summary>
        /// 
        /// </summary>
        public ICollectionView TestingsInfoView
        {
            get { return testingsInfoView; }
            private set { testingsInfoView = value; }
        }
        private CheckupInfoWrapper selectedTesting;

        /// <summary>
        /// Выделенный набор тестов
        /// </summary>
        public CheckupInfoWrapper SelectedTesting
        {
            get { return selectedTesting; }
            set
            {
                if (selectedTesting != value)
                {                    
                    selectedTesting = value;                    
                    OnPropertyChanged("SelectedTesting");
                }
            }
        }
        private ObservableCollection<DeviceTypeInfo> deviceTypes = new ObservableCollection<DeviceTypeInfo>() { new DeviceTypeInfo("none", Properties.Resources.NoDeviceTypeFilter) };
        /// <summary>
        /// Список возможных типов устройств
        /// </summary>
        public ObservableCollection<DeviceTypeInfo> DeviceTypes
        {
            get { return deviceTypes; }
        }

        private DeviceTypeInfo deviceTypeFilter;
        /// <summary>
        /// 
        /// </summary>
        public DeviceTypeInfo DeviceTypeFilter
        {
            get { return deviceTypeFilter; }
            set
            {
                if (deviceTypeFilter != value)
                {
                    deviceTypeFilter = value;
                    if (TestingsInfoView != null)
                    {
                        string deviceType = deviceTypeFilter != null ? deviceTypeFilter.DeviceType : null;
                        TestingsInfoView.Filter = ch =>
                        {
                            if (deviceType == "none" || string.IsNullOrWhiteSpace(deviceType))
                                return true;
                            var chInfo = ch as CheckupInfoWrapper;
                            if (chInfo == null || chInfo.CheckupInfo.PacientInfo == null)
                                return false;
                            return chInfo.CheckupInfo.PacientInfo.LastName == deviceType;
                        };
                    }
                    OnPropertyChanged("DeviceTypeFilter");
                }
            }
        }

        private JournalFormModel JournalFormModel;
        private DataConnection currentConnection;
        /// <summary>
        /// 
        /// </summary>
        public DataConnection CurrentConnection
        {
            get
            {
                if (currentConnection == null)
                {
                    currentConnection = Globals.CurrentConnection.Connection;
                }
                return currentConnection;
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>        
        public TestingHistoryListDialog(JournalFormModel JournalFormModel)
        {
            InitializeComponent();
            this.JournalFormModel = JournalFormModel;
            InitTestsInfoList();
            TestingsInfoView = CollectionViewSource.GetDefaultView(TestingsInfo);
            if (DeviceTypes.Count > 0)
                DeviceTypeFilter = DeviceTypes[0];
            DataContext = this;
        }

        private void InitTestsInfoList()
        {
            TestingsInfo.Clear();
            var patients = CurrentConnection.GetPacientInfoList();
            foreach (var patient in patients)
            {
                if (patient.Species == DeviceTestCheckupAncestor.TestSpecies)
                {
                    var checkups = CurrentConnection.GetCheckupInfoList(false, patient);
                    foreach (var checkup in checkups)
                    {
                        TestingsInfo.Add(new CheckupInfoWrapper(checkup));
                        if (checkup.PacientInfo != null && !DeviceTypes.Any(dti => dti.DeviceType == checkup.PacientInfo.LastName))
                        {
                            DeviceTypeInfo deviceTypeInfo = DevicesManipulation.AvailableDevices.FirstOrDefault(dti => dti.DeviceType == checkup.PacientInfo.LastName);
                            if (deviceTypeInfo == null)
                            {
                                deviceTypeInfo = new DeviceTypeInfo(checkup.PacientInfo.LastName, checkup.PacientInfo.LastName);
                            }                            
                            DeviceTypes.Add(deviceTypeInfo);
                        }
                    }
                }
            }
        }

        private void DetailsBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowDetails();
        }

        private void NSGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowDetails();
        }
        
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {            
            using (BasesListForm form = new BasesListForm())
            {
                form.ShowDialog();
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                JournalFormModel.SetConnection(form.Connection);
                MainModel.OnCurrentConnectionChanged();
                currentConnection = null;
                OnPropertyChanged("CurrentConnection");
                InitTestsInfoList();
            }
        }

        private void ShowDetails()
        {
            if (SelectedTesting == null)
                return;                
            var ancestor = LoadCheckupAncestor(SelectedTesting.CheckupInfo);
            if (ancestor == null)
                return;
            TestingHistoryDialog dialog = new TestingHistoryDialog(ancestor, null);
            dialog.ShowDialog();            
        }

        /// <summary>
        /// Загружает данные обследования
        /// </summary>
        /// <param name="checkupInfo"></param>
        /// <returns></returns>
        private DeviceTestCheckupAncestor LoadCheckupAncestor(CheckupInfo checkupInfo)
        {
            DeviceTestCheckupAncestor result = null;
            NeuroSoft.Prototype.DataModel.CheckupDataStorage storage = new NeuroSoft.Prototype.Interface.DataBaseStorage(checkupInfo);
            try
            {
                checkupInfo.PacientInfo.Restore();

                result = NeuroSoft.Prototype.DataModel.CheckupAncestor.Load(storage) as DeviceTestCheckupAncestor;
                if (result != null) ((BaseCheckupAncestor)result).CheckupInfo = checkupInfo;
            }
            finally
            {
                storage.CloseStreams();
            }
            return result;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CheckupInfoWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkupInfo"></param>
        public CheckupInfoWrapper(CheckupInfo checkupInfo)
        {
            CheckupInfo = checkupInfo;
        }

        private CheckupInfo checkupInfo;
        /// <summary>
        /// 
        /// </summary>
        public CheckupInfo CheckupInfo
        {
            get { return checkupInfo; }
            private set
            {
                checkupInfo = value;
            }
        }

        private bool usersInitialized = false;
        private string users = null;
        /// <summary>
        /// Наладчики, участвующие в наладке
        /// </summary>
        public string Users
        {
            get 
            {
                if (users == null)
                {
                    InitUsersInfo();
                }
                return users; 
            }            
        }
        private TimeSpan? testingTime = null;
        /// <summary>
        /// Оцененное время наладки
        /// </summary>
        public TimeSpan? TestingTime
        {
            get
            {
                if (testingTime == null)
                {
                    InitUsersInfo();
                }
                return testingTime;
            }
        }

        private void InitUsersInfo()
        {
            if (usersInitialized)
                return;
            users = string.Empty;
            string usersInfoString = checkupInfo.ReadParam(DeviceTestCheckupAncestor.EditUsersInfoParam, null) as string;
            if (usersInfoString != null)
            {
                var usersInfo = new TestingUserInfoList(usersInfoString);
                usersInfo.Sort((user1, user2) => string.Compare(user1.UserName, user2.UserName));
                long testingTicks = 0;
                for (int i = 0; i < usersInfo.Count; i++)
                {
                    users += usersInfo[i].UserName + (i < usersInfo.Count - 1 ? ", " : "");
                    testingTicks += usersInfo[i].TestingTime;
                }
                testingTime = new TimeSpan(testingTicks);
            }
            usersInitialized = true;
        }

        private DateTime? lastEditDate = null;
        /// <summary>
        /// Время последнего изменения
        /// </summary>
        public DateTime? LastEditDate
        {
            get 
            {
                if (lastEditDate == null)
                {
                    long lastEditDateTicks = Convert.ToInt64(CheckupInfo.ReadParam(DeviceTestCheckupAncestor.LastEditDateParam, 0));
                    if (lastEditDateTicks == 0)
                    {
                        lastEditDateTicks = CheckupInfo.LastEditDate.ToUniversalTime().Ticks;
                    }
                    lastEditDate = new DateTime(lastEditDateTicks, DateTimeKind.Utc).ToLocalTime();
                }
                return lastEditDate; 
            }            
        }

        private string templateName;
        /// <summary>
        /// Название инструкции по наладке
        /// </summary>
        public string TemplateName
        {
            get 
            {
                if (templateName == null)
                {
                    var patientInfo = CheckupInfo.PacientInfo;                    
                    var template = DATTemplate.TestTemplateDescriptors.FirstOrDefault(d => d.GUID == new Guid(patientInfo.Polis) && (d.Version == System.Convert.ToInt32(patientInfo.Height) || patientInfo.Height == 0));
                    if (template == null)
                        templateName = patientInfo.Guid.ToString();
                    templateName = patientInfo.Height == 0 ? template.Name : template.FullName;
                }
                return templateName; 
            }
        }

        private string deviceTypeName;
        /// <summary>
        /// Тип прибора
        /// </summary>
        public string DeviceTypeName
        {
            get
            {
                if (deviceTypeName == null)
                {
                    string deviceType = CheckupInfo.PacientInfo.LastName;
                    var deviceTypeInfo = DevicesManipulation.AvailableDevices.FirstOrDefault(dti => dti.DeviceType == deviceType);
                    deviceTypeName = deviceTypeInfo != null ? deviceTypeInfo.Name : deviceType;
                }
                return deviceTypeName;
            }
        }

        private bool? finished = null;
        /// <summary>
        /// Признак завершенности наладки
        /// </summary>
        public bool Finished
        {
            get 
            {
                if (finished == null)
                {
                    bool isFinished = false;
                    string finishedStr = checkupInfo.ReadParam(DeviceTestCheckupAncestor.IsFinishedTesingParam, null) as string;
                    if (finishedStr != null)
                    {                        
                        bool.TryParse(finishedStr, out isFinished);
                    }                    
                    finished = isFinished;
                }
                return finished.Value; 
            }            
        }
        
    }
    /// <summary>
    /// 
    /// </summary>
    internal class DeviceTypeNameConverter : IValueConverter
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
            string deviceType = System.Convert.ToString(value);
            var deviceTypeInfo = DevicesManipulation.AvailableDevices.FirstOrDefault(dti => dti.DeviceType == deviceType);
            return deviceTypeInfo != null ? deviceTypeInfo.Name : deviceType;
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
