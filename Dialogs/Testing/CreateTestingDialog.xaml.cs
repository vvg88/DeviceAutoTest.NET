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
using System.ComponentModel;
using NeuroSoft.Prototype.Database;
using System.Collections.ObjectModel;
using NeuroSoft.Prototype.Interface;
using NeuroSoft.Prototype.Interface.Database;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateTestingDialog.xaml
    /// </summary>
    public partial class CreateTestingDialog : DATDialogWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public CreateTestingDialog(CardInfo cardInfo, JournalFormModel journalFormModel)
        {
            InitializeComponent();
            JournalFormModel = journalFormModel;
            CardInfo = cardInfo ?? Globals.GetCurrentCardInfo();

            if (AvailableTemplates.Count > 0)
            {
                SelectedTemplate = AvailableTemplates[0];
            }
            DataContext = this;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            SerialNumberTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            SerialNumberTextBox.Focus();
        }

        #region Properties
        
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

        private CardInfo cardInfo;
        /// <summary>
        /// Путь сохранения нового обследования
        /// </summary>
        public CardInfo CardInfo
        {
            get
            {
                return cardInfo;
            }
            set
            {
                if (cardInfo != value)
                {
                    cardInfo = value;
                    OnPropertyChanged("CardInfo");
                    UpdateExistingSerialNumbers();
                }
            }
        }
        private string serialNumber;
        /// <summary>
        /// Серийный номер устройства
        /// </summary>
        public string SerialNumber
        {
            get { return serialNumber; }
            set
            {
                if (serialNumber != value)
                {
                    serialNumber = value;
                    OnPropertyChanged("SerialNumber");                    
                    Validate();
                }
            }
        }

        private List<string> existingSerialNumbers = new List<string>();
        private void UpdateExistingSerialNumbers()
        {
            existingSerialNumbers = new List<string>();
            if (SelectedTemplate != null)
            {
                List<PacientInfo> list = CurrentConnection.GetPacientInfoList();
                foreach (PacientInfo patient in list)
                {
                    if (patient.Polis == SelectedTemplate.GUID.ToString() && patient.CardExist(CardInfo))
                    {
                        existingSerialNumbers.Add(patient.FirstName);
                    }
                }
            }
            Validate();
        }

        /// <summary>
        /// Список серийных номеров устройств типа SelectedDeviceType, для которых были созданы наборы тестов.
        /// </summary>
        internal List<string> ExistingSerialNumbers
        {
            get 
            {
                return existingSerialNumbers;
            }            
        }

        private DATTemplateDescriptor selectedTemplate;

        /// <summary>
        /// Тип устройства
        /// </summary>
        public DATTemplateDescriptor SelectedTemplate
        {
            get { return selectedTemplate; }
            set
            {
                if (selectedTemplate != value)
                {
                    selectedTemplate = value;
                    OnPropertyChanged("SelectedTemplate");
                    OnPropertyChanged("TemplateIsSelected");
                    if (selectedTemplate != null && !string.IsNullOrEmpty(selectedTemplate.DefaultCardPath))
                    {
                        var defaultCard = Globals.GetCardInfoByPath(Globals.CurrentConnection.Connection, selectedTemplate.DefaultCardPath);
                        if (defaultCard != null)
                        {
                            CardInfo = defaultCard;
                        }
                    }
                    UpdateExistingSerialNumbers();
                    SerialNumberTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                }
            }
        }

        private void UpdateAvailableTemplates()
        {
            availableTemplates = null;
            OnPropertyChanged("AvailableTemplates");
        }

        /// <summary>
        /// Признак выбранного сценария тестирования
        /// </summary>
        public bool TemplateIsSelected
        {
            get
            {
                return SelectedTemplate != null;
            }
        }
        private List<DATTemplateDescriptor> availableTemplates = null;
        /// <summary>
        /// Список шаблонов тестирования для заданного типа устройств
        /// </summary>
        public List<DATTemplateDescriptor> AvailableTemplates
        {
            get
            {
                if (availableTemplates == null)
                {
                    availableTemplates = new List<DATTemplateDescriptor>();
                    foreach (var descr in DATTemplate.TestTemplateDescriptors)
                    {                        
                        availableTemplates.Add(descr);
                    }
                    OnPropertyChanged("AvailableTemplates");
                }
                return availableTemplates;
            }
        }

        private bool repeatTesting;
        /// <summary>
        /// Создать повторное обследование устройства
        /// </summary>
        public bool RepeatTesting
        {
            get { return repeatTesting; }
            set
            {
                if (repeatTesting != value)
                {
                    repeatTesting = value;
                    OnPropertyChanged("RepeatTesting");
                    Validate();
                }
            }
        }

        private bool lastSerialNumberExists;
        /// <summary>
        /// Признак существования обследования для прибора с текущим серийным номером
        /// </summary>
        public bool SerialNumberExists
        {
            get 
            {
                bool newSNExists = ExistingSerialNumbers.Contains(SerialNumber);
                if (newSNExists != lastSerialNumberExists)
                {
                    lastSerialNumberExists = newSNExists;
                    OnPropertyChanged("ExistingDevice");
                }
                return newSNExists;
            }
        }

        private bool canCreate;
        /// <summary>
        /// 
        /// </summary>
        public bool CanCreate
        {
            get { return canCreate; }
            private set
            {
                if (canCreate != value)
                {
                    canCreate = value;
                    OnPropertyChanged("CanCreate");
                }
            }
        }

        
        private string errorText;

        /// <summary>
        /// Описание ошибки
        /// </summary>
        public string ErrorText
        {
            get { return errorText; }
            private set
            {
                if (errorText != value)
                {
                    errorText = value;
                    OnPropertyChanged("ErrorText");
                    OnPropertyChanged("ShowErrorText");
                }
            }
        }
      
        /// <summary>
        /// 
        /// </summary>
        public PacientInfo ExistingDevice
        {
            get
            {
                if (SelectedTemplate != null)
                {
                    List<PacientInfo> list = CurrentConnection.GetPacientInfoList();
                    foreach (PacientInfo patient in list)
                    {
                        if (patient.Polis == SelectedTemplate.GUID.ToString() && patient.FirstName == SerialNumber && patient.CardExist(CardInfo))
                        {
                            return patient;                            
                        }
                    }
                }
                return null;
            }
        }

        private bool isRepeatCheck;
        /// <summary>
        /// Признак необходимости проведения только повторной проверки
        /// </summary>
        public bool IsRepeatCheck
        {
            get { return isRepeatCheck; }
            set 
            { 
                isRepeatCheck = value;
                OnPropertyChanged("IsRepeatCheck");
            }
        }
        

        #endregion 
        
        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(SerialNumber))
            {
                ErrorText = Properties.Resources.SerialNumberMustBeNotEmpty;
                CanCreate = false;
            }
            else if (SerialNumberExists)
            {
                ErrorText = Properties.Resources.SerialNumberAlreadyExists;
                CanCreate = RepeatTesting;
            }
            else
            {
                ErrorText = null;
                CanCreate = true;
            }
            OnPropertyChanged("SerialNumberExists");
        }
        /// <summary>
        /// Объект устройства (пациента) на основе серийного номера и типа устройства.
        /// </summary>
        public PacientInfo GeneratePatientInfo(PacientInfo basePatient = null)
        {
            PacientInfo patientInfo = basePatient;
            if (RepeatTesting && SerialNumberExists) //При повторной наладке устройства найдем устройство из списка существующих
            {
                patientInfo = ExistingDevice;
            }
            if (patientInfo == null)
            {
                if (CardInfo != null)
                    patientInfo = new PacientInfo(CardInfo);
                else
                    patientInfo = new PacientInfo(CurrentConnection);
            }
            patientInfo.FirstName = SerialNumber;
            patientInfo.Polis = SelectedTemplate.GUID.ToString();
            patientInfo.LastName = SelectedTemplate.DeviceType;
            patientInfo.Height = SelectedTemplate.Version;
            patientInfo.Species = DeviceTestCheckupAncestor.TestSpecies;
            if (isRepeatCheck)
                patientInfo.Sex = 1;
            return patientInfo;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cardInfo = CardInfo;
            var connection = cardInfo != null ? cardInfo.DataConnection : CurrentConnection;
            string id = "";
            if (cardInfo != null) id = cardInfo.Id;
            NeuroSoft.Prototype.Interface.Database.SelectCardForm form = new NeuroSoft.Prototype.Interface.Database.SelectCardForm(connection, id);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CardInfo newCardInfo = connection.GetCardInfoById(form.CurrentCardId);
                CardInfo = newCardInfo;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string cardPath = Globals.GetCardInfoPath(CardInfo);
            using (BasesListForm form = new BasesListForm())
            {
                form.ShowDialog();
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                JournalFormModel.SetConnection(form.Connection);
                MainModel.OnCurrentConnectionChanged();                
                currentConnection = null;
                OnPropertyChanged("CurrentConnection");

                var cardInfo = Globals.GetCardInfoByPath(CurrentConnection, cardPath);
                if (cardInfo == null && SelectedTemplate != null && !string.IsNullOrEmpty(SelectedTemplate.DefaultCardPath))
                {
                    cardInfo = Globals.GetCardInfoByPath(CurrentConnection, SelectedTemplate.DefaultCardPath);
                }
                CardInfo = cardInfo ?? Globals.GetCurrentCardInfo();
                SerialNumberTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }            
        }
    }
}
