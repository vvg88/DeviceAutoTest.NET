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
using System.Collections.ObjectModel;
using System.ComponentModel;
using NeuroSoft.Hardware.Devices;
using System.Numerics;
using NeuroSoft.DeviceAutoTest.Common.Controls;

namespace NeuroSoft.DeviceAutoTest.NewDevicesTest.Controls
{
    /// <summary>
    /// Interaction logic for ImpedancesControl.xaml
    /// </summary>
    public partial class ImpedancesView : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public ImpedancesView()
        {            
            InitializeComponent();            
        }
        
        /// <summary>
        /// Частота на которой измеряется импеданс электродов. Если меньше нуля, то измерение импеденса не поддерживается.
        /// </summary>
        private double MeasureFreq;

        private ObservableCollection<ImpedanceInfo> impedances = new ObservableCollection<ImpedanceInfo>();

        /// <summary>
        /// Коллекция импедансов
        /// </summary>
        public ObservableCollection<ImpedanceInfo> Impedances
        {
            get { return impedances; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="impedances"></param>
        public void UpdateImpedaces(ElectrodeImpedance[] impedances)
        {
            if (impedances == null)
                return;
            foreach (var impedance in impedances)
            {
                var found = Impedances.FirstOrDefault(imp => imp.Index == impedance.Index);
                if (found != null)
                {
                    found.Impedance = impedance.Impedance;
                }
            }
        }

        public void UpdateImpedaces(IEnumerable<Complex> impedances)
        {
            if (impedances == null)
                return;
            var impsWithIndxs = impedances.Select((imp, i) => new { impedance = imp, Index = i});
            foreach (var impWithIndx in impsWithIndxs)
            {
                var found = Impedances.FirstOrDefault(imp => imp.Index == impWithIndx.Index);
                if (found != null)
                    found.Impedance = impWithIndx.impedance;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ImpedanceInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="title"></param>
        /// <param name="measureFreq"></param>
        /// <param name="minResistance"></param>
        /// <param name="maxResistance"></param>
        /// <param name="minCapacity"></param>
        /// <param name="maxCapacity"></param>
        public ImpedanceInfo(int index, string title, double measureFreq, double minResistance, double maxResistance, double minCapacity, double maxCapacity)
        {
            resistance = new RangedValue<double>(0, minResistance, maxResistance);
            capacity = new RangedValue<double>(0, minCapacity, maxCapacity);
            Title = title;
            Index = index;
            MeasureFreq = measureFreq;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="title"></param>
        /// <param name="measureFreq"></param>
        public ImpedanceInfo(int index, string title, double measureFreq)
        {
            resistance = new RangedValue<double>(0, 0, 0) { IgnoreRange = true };
            capacity = new RangedValue<double>(0, 0, 0) { IgnoreRange = true };
            Title = title;
            Index = index;
            MeasureFreq = measureFreq;
        }
        
        private double measureFreq;
        /// <summary>
        /// Частота на которой измеряется импеданс электродов. Если меньше нуля, то измерение импеденса не поддерживается.
        /// </summary>
        public double MeasureFreq
        {
            get { return measureFreq; }
            private set { measureFreq = value; }
        }

        private int index;
        /// <summary>
        /// Индекс импеданса
        /// </summary>
        public int Index
        {
            get { return index; }
            private set { index = value; }
        }

        private string title;
        /// <summary>
        /// Текстовое описание
        /// </summary>
        public string Title
        {
            get { return title; }
            set 
            { 
                title = value;
                OnPropertyChanged("Title");
            }
        }

        private Complex impedance;
        /// <summary>
        /// Значение импеданса
        /// </summary>
        public Complex Impedance
        {
            get { return impedance; }
            set 
            {
                if (IsValid.HasValue)
                    return;
                impedance = value;
                UpdateCapacity();
                UpdateResistance();                
                OnPropertyChanged("Impedance");                
                OnPropertyChanged("Phi");
                OnPropertyChanged("Real");
                OnPropertyChanged("Abs");
                OnPropertyChanged("Image");                
            }
        }

        private RangedValue<double> capacity;
        /// <summary>
        /// Емкость
        /// </summary>
        public RangedValue<double> Capacity
        {
            get
            {
                return capacity;
            }
        }

        private void UpdateCapacity()
        {
            double temp = Impedance.Real * Impedance.Real + Impedance.Imaginary * Impedance.Imaginary;
            double k = 1e12 / (2 * System.Math.PI * MeasureFreq);
            Capacity.Value = Impedance.Imaginary * k / temp;
        }
        /// <summary>
        /// 
        /// </summary>
        public double Phi
        {
            get
            {
                return Impedance.Phase * 180 / System.Math.PI;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public double Abs
        {
            get
            {
                return Impedance.Magnitude;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public double Image
        {
            get
            {
                return Impedance.Imaginary;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Real
        {
            get
            {
                return Impedance.Real;
            }
        }

        private RangedValue<double> resistance = null;
        /// <summary>
        /// 
        /// </summary>
        public RangedValue<double> Resistance
        {
            get
            {
                return resistance;
            }
        }

        private bool? isValid = null;

        /// <summary>
        /// 
        /// </summary>
        public bool? IsValid
        {
            get { return isValid; }
            set
            {
                if (isValid != value)
                {
                    isValid = value;
                    OnPropertyChanged("IsValid");
                }
            }
        }

        private void UpdateResistance()
        {
            double temp = Impedance.Real * Impedance.Real + Impedance.Imaginary * Impedance.Imaginary;
            Resistance.Value = temp / Impedance.Real;
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
