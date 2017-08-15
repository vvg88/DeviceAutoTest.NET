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
using NeuroSoft.DeviceAutoTest.Dialogs;

namespace NeuroSoft.DeviceAutoTest.Controls
{
    /// <summary>
    /// Interaction logic for TestingHistoryControl.xaml
    /// </summary>
    public partial class TestingHistoryControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public TestingHistoryControl()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        public TestingHistoryControl(DeviceTestCheckupAncestor testingAncestor, TestObject test)
        {
            InitializeComponent();
            Ancestor = testingAncestor;
            Test = test;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Title
        {
            get 
            {
                string title = ""; 
                if (Ancestor != null)
                    title = Ancestor.CheckupInfo.PacientInfo.FirstName + " (" + Ancestor.CheckupInfo.PacientInfo.LastName + ")";
                if (Test != null)
                {
                    title += " - " + Test.TemplateItem.Name;                    
                }
                return title; 
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        public DeviceTestCheckupAncestor Ancestor
        {
            get { return (DeviceTestCheckupAncestor)GetValue(AncestorProperty); }
            set { SetValue(AncestorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Ancestor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AncestorProperty =
            DependencyProperty.Register("Ancestor", typeof(DeviceTestCheckupAncestor), typeof(TestingHistoryControl), new UIPropertyMetadata(null, OnAncestorChanged));

        private static void OnAncestorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TestingHistoryControl control = d as TestingHistoryControl;
            control.OnAncestorOrTestChanged();
        }

        /// <summary>
        /// Действие, по которому фильтруется история
        /// </summary>
        public TestObject Test
        {
            get { return (TestObject)GetValue(TestProperty); }
            set { SetValue(TestProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty TestProperty =
            DependencyProperty.Register("Test", typeof(TestObject), typeof(TestingHistoryControl), new UIPropertyMetadata(null, OnTestChanged));

        private static void OnTestChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TestingHistoryControl control = d as TestingHistoryControl;
            control.ShowAllTime = control.Test == null;
            control.InvalidateHistory();
            control.OnAncestorOrTestChanged();
        }

        private void OnAncestorOrTestChanged()
        {
            if (Ancestor == null) return;
            OnPropertyChanged("Title");            
            OnPropertyChanged("HistorySnapshots");
            OnPropertyChanged("EffectiveTime");
            OnPropertyChanged("AllTime");
        }

        internal void InvalidateHistory()
        {            
            history = null;
            OnPropertyChanged("HistorySnapshots");
        }

        private List<TestingSnapshot> history = null;
        /// <summary>
        /// Список снимков в истории
        /// </summary>
        public List<TestingSnapshot> HistorySnapshots
        {
            get
            {
                if (history == null && Ancestor != null)
                {
                    history = new List<TestingSnapshot>();                    
                    history.AddRange(from snapshot in Ancestor.Snapshots where Test == null || Test.TemplateItem.Contains(snapshot.ExecutedTestId) select snapshot);
                }
                return history;
            }
        }

        private TestingSnapshot selectedSnapshot;

        /// <summary>
        /// Выделенный снимок
        /// </summary>
        public TestingSnapshot SelectedSnapshot
        {
            get { return selectedSnapshot; }
            set
            {
                if (selectedSnapshot != value)
                {
                    selectedSnapshot = value;
                    OnPropertyChanged("selectedSnapshot");
                }
            }
        }

        /// <summary>
        /// Оценка общего затраченного на наладку времени (без учета затрат на навигацию и подтверждение тестов)
        /// </summary>
        public TimeSpan EffectiveTime
        {
            get
            {
                long sum = 0;
                if (HistorySnapshots != null)
                {
                    foreach (var snapshot in HistorySnapshots)
                    {
                        if (snapshot.ExecutionTime.HasValue)
                            sum += snapshot.ExecutionTime.Value.Ticks;
                    }
                }
                return new TimeSpan(sum);
            }
        }

        /// <summary>
        /// Время от начала наладки до последнего выполненного действия
        /// </summary>
        public TimeSpan AllTime
        {
            get
            {
                if (HistorySnapshots == null || HistorySnapshots.Count == 0)
                    return new TimeSpan();
                long minTime = HistorySnapshots.First().SnapshotTime;
                long maxTime = HistorySnapshots.Last().SnapshotTime;
                return new TimeSpan(maxTime - minTime);
            }
        }
        private bool showAllTime = true;
        /// <summary>
        /// 
        /// </summary>
        public bool ShowAllTime
        {
            get { return showAllTime; }
            set
            {
                if (showAllTime != value)
                {
                    showAllTime = value;
                    OnPropertyChanged("ShowAllTime");
                }
            }
        }
        private void NSGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowDetails();
        }

        internal void ShowDetails()
        {
            if (SelectedSnapshot == null)
            {
                return;
            }
            TestingHistoryDetailsDialog details = new TestingHistoryDetailsDialog(SelectedSnapshot);
            details.ShowDialog();
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
    }
}
