using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.Prototype.DataModel;
using NeuroSoft.Prototype.Database;
using NeuroSoft.WPFPrototype.Interface;
using NeuroSoft.WPFPrototype.Interface.Common;
using NeuroSoft.WPFComponents.CommandManager;
using System.Windows;
using NeuroSoft.DeviceAutoTest.Dialogs;
using NeuroSoft.WPFComponents.ProtocolPatternMaker;
using NeuroSoft.Devices;
using NeuroSoft.WPFComponents.ScalableWindows;
using NeuroSoft.Prototype.Interface.Database;
using NeuroSoft.DeviceAutoTest.Common;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    /// Класс модели приложения
    /// </summary>
    [Anamnesis(false)]
    [Conclusion(false)]
    public class MainModel : WPFPrototypeModel
    {
        /// <summary>
        /// 
        /// </summary>
        public const string AutoSaveKey = "AutoSave";
        private bool autoSave;
        /// <summary>
        /// Признак автосохранения обследования
        /// </summary>
        public bool AutoSave
        {
            get { return autoSave; }
            set { autoSave = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public MainModel()
        {
            //Зарегистрировать команды, группы и их описания из указанной сборки. Внимание!!!
            //Описание команд д.б. обязательно расположено в папке /Resources/Commands.xaml
            CommandRepository.RegisterCommandsAndDefineDescriptions("NeuroSoft.DeviceAutoTest");

            //Регистрация CheckupManager'а приложения.
            //Здесь же необходимо регистрировать CheckupManager'ы плагинов.
            RegisterPlugIns(new Type[] { typeof(DeviceTestCheckupManager) });

            // Откроем возможно подключенные HID-устройства (клавиатура, кнопка пациента, педаль)
            NSHIDDevices.Open();

            NSHIDDevices.ReceivePad += new Devices.NsHID.ReceivePadDelegate(DeviceTestCheckupManager.NSHIDDevices_ReceivePad);
            AutoSave = (bool)Globals.Depository.Read(NeuroSoft.Prototype.Interface.DepositoryKeys.UserSettingsFolder, AutoSaveKey, false);
            SettingsChangedEvent += new EventHandler(MainModel_SettingsChangedEvent);
            OnCurrentConnectionChanged();
        }

        void MainModel_SettingsChangedEvent(object sender, EventArgs e)
        {
            AutoSave = (bool)Globals.Depository.Read(NeuroSoft.Prototype.Interface.DepositoryKeys.UserSettingsFolder, AutoSaveKey, false);
        }        

        /// <summary>
        /// Перекрытый метод, возвращающий экземпляр CheckupManager, с которым работает приложение.
        /// </summary>
        public override MainCheckupManager CreateMainCheckupManager(CheckupData checkupData, CheckupInfo checkupInfo)
        {
            var manager = new DeviceTestCheckupManager(this, checkupData, checkupInfo);
            var mainWindow = manager.MainWindow as MainWindow;
            mainWindow.UpdateAutoTestingToolBarState(manager);
            return manager;
        }
                        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkupInfo"></param>
        /// <param name="cardInfo"></param>
        /// <param name="newCheckup"></param>
        /// <returns></returns>
        public override bool EditCheckup(CheckupInfo checkupInfo, CardInfo cardInfo, bool newCheckup)
        {
            if (newCheckup)
            {
                if (checkupInfo.PacientInfo != null && checkupInfo.PacientInfo.Species == DeviceTestCheckupAncestor.TestSpecies)
                {
                    TestingDescriptionDialog descriptionDialog = new TestingDescriptionDialog(checkupInfo.PacientInfo);
                    descriptionDialog.ShowDialog();
                    checkupInfo.Description = descriptionDialog.Description;
                    return true;
                }
                CreateTestingDialog dialog = new CreateTestingDialog(cardInfo, CreateJournalModel());
                if (dialog.ShowDialog() == true)
                {
                    PacientInfo pacientInfo = dialog.GeneratePatientInfo();                    
                    checkupInfo.PacientInfo = pacientInfo;

                    TestingDescriptionDialog descriptionDialog = new TestingDescriptionDialog(checkupInfo.PacientInfo);
                    descriptionDialog.ShowDialog();
                    checkupInfo.Description = descriptionDialog.Description;                    
                    return true;
                }
            }
            return false;
        }
        
        internal bool LockClosing = false;
        public override bool CloseCheckup(MainCheckupManager checkup)
        {
            if (LockClosing)
                return false;
            bool close = base.CloseCheckup(checkup);
            if (checkup is WPFMainCheckupManager)
            {
                var mainWindow = (checkup as WPFMainCheckupManager).MainWindow as MainWindow;                
                if (mainWindow != null)
                    mainWindow.UpdateAutoTestingToolBarState();
            }
            return close;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pacientInfo"></param>
        /// <param name="cardInfo"></param>
        /// <param name="newPacient"></param>
        /// <returns></returns>
        public override bool EditPacient(PacientInfo pacientInfo, CardInfo cardInfo, bool newPacient)
        {
            if (newPacient)
            {
                CreateTestingDialog dialog = new CreateTestingDialog(cardInfo, CreateJournalModel());
                if (dialog.ShowDialog() == true)
                {
                    dialog.GeneratePatientInfo(pacientInfo);                    
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkupInfo"></param>
        /// <returns></returns>
        public override bool OpenCheckup(CheckupInfo checkupInfo)
        {
            CloseAllCheckups();
            bool open = base.OpenCheckup(checkupInfo);
            if (CurrentMainCheckupManager != null)
            {
                var mainWindow = CurrentMainCheckupManager.MainWindow as MainWindow;
                mainWindow.UpdateAutoTestingToolBarState();
            }
            return open;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void ShowProtocolTemplates()
        {
            if (DATTemplate.CurrentTemplate == null)
                return;
            if (!Globals.IsAdmin())
            {
                NSMessageBox.Show(Properties.Resources.OnlyAdminError, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ProtocolPattern.UpdateProtocolPatternDescriptorsList();
            UpdateTagDescriptors();
            base.ShowProtocolTemplates();
        }
                
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        public override Guid NewProtocolByTemplate(Guid guid, bool showReportNameDialog = true)
        {
            // Получить переменную с суффиксом серийного номера
            var checkup = CurrentMainCheckupManager.CheckupAncestor as DeviceTestCheckupAncestor;
            var snSuffx = checkup.Variables.FirstOrDefault(var => var.TestVariableID == "SnSuffix");
            if (snSuffx != null)
            {
                if (string.IsNullOrEmpty(snSuffx.VariableValue as string))  // Если она найдена и пустая, сохранить суффикс серийного номера
                {
                    snSuffx.VariableValue = CommonScripts.GetSnSuffix();
                }
            }
            // Получить переменную признак успешного завершения наладки
            var checkUpSuccess = checkup.Variables.FirstOrDefault(var => var.TestVariableID == "IsCheckupSuccess");
            if (checkUpSuccess != null)
            {
                // Установить true, если все тесты пройдены и нет ошибок
                checkUpSuccess.VariableValue = checkup.Tests.All(tst => tst.Finished && tst.FinishingIsConfirmed)
                                               && !checkup.Tests.Any(testObj => testObj.Status == TestObjectStatus.HasErrors);
            }
            UpdateTagDescriptors();            
            return base.NewProtocolByTemplate(guid);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void UpdateTagDescriptors()
        {
            ProtocolPattern.RegisteredTags.Clear();

            if (DATTemplate.CurrentTemplate != null)
            {
                ProtocolPattern.RegisterTags(DeviceTestCheckupManager.GetDATTagDescriptions());
            }
            //base.UpdateTagDescriptors();
        }
                
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Prototype.Interface.Database.JournalFormModel CreateJournalModel()
        {
            return new DATJournalModel(Globals.DatabaseManager) { Options = Prototype.Interface.Database.JournalOptions.EnableAbout };
        }

        /// <summary>
        /// Регистрация типов панелей для обследования
        /// </summary>
        protected override void RegisteredCheckupInfo()
        {
            JournalFormModel.RegisteredCheckupInfo.Add(typeof(JournalInspectorInfo));
            JournalFormModel.RegisteredCheckupInfo.Add(typeof(JournalProtocolInfo));
        }
        /// <summary>
        /// Возвращает массив объектов для отображения в журнале
        /// </summary>
        /// <param name="objectInfo"></param>
        /// <returns></returns>
        public override JournalObjectInfo[] GetJournalObjectInfo(ObjectInfo objectInfo)
        {
            if (objectInfo.Type == ObjectInfoType.Pacient) return new JournalObjectInfo[] { new JournalPacientInfoEx() };
            if (objectInfo.Type == ObjectInfoType.Checkup) return new JournalObjectInfo[] { new JournalInspectorInfo(), new JournalProtocolInfo() };
            else return null;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void SaveSettings()
        {
            Globals.Depository.Write(NeuroSoft.Prototype.Interface.DepositoryKeys.UserSettingsFolder, AutoSaveKey, AutoSave);
            base.SaveSettings();            
        }

        private static DataConnection lastConnection;
        /// <summary>
        /// Действия при изменении подключения к базе данных
        /// </summary>
        public static void OnCurrentConnectionChanged()
        {           
            if (lastConnection == Globals.CurrentConnection.Connection)
                return;
            lastConnection = Globals.CurrentConnection.Connection;
            MainWindow.UpdateNewCorrectionsCount();
        }
    }
}
