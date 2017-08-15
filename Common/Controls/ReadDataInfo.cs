using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuroSoft.DeviceAutoTest.Common.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class Range<T>
    {
        /// <summary>
        /// Конструктор
        /// </summary>        
        public Range(T min, T max)
        {
            Min = min;
            Max = max;
        }


        private T min = default(T);
        /// <summary>
        /// Нижняя граница значения
        /// </summary>
        public T Min
        {
            get { return min; }
            set { min = value; }
        }

        private T max = default(T);
        /// <summary>
        /// Нижняя граница значения
        /// </summary>
        public T Max
        {
            get { return max; }
            set { max = value; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RangedValue<T> : DATBaseViewModel where T : IComparable
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public RangedValue(T value, T minValue, T maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            Value = value;
        }

        private T value;
        /// <summary>
        /// Значение
        /// </summary>
        public T Value
        {
            get { return value; }
            set
            {
                if (Compare(this.value, value) != 0)
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                    OnPropertyChanged("IsValidValue");
                }
            }
        }

        private T maxValue;
        /// <summary>
        /// Нижняя граница значения
        /// </summary>
        public T MaxValue
        {
            get { return maxValue; }
            set
            {
                if (Compare(maxValue, value) != 0)
                {
                    maxValue = value;
                    OnPropertyChanged("MaxValue");
                    OnPropertyChanged("IsValidValue");
                }
            }
        }

        private T minValue;
        /// <summary>
        /// Нижняя граница значения
        /// </summary>
        public T MinValue
        {
            get { return minValue; }
            set
            {
                if (Compare(minValue, value) != 0)
                {
                    minValue = value;
                    OnPropertyChanged("MinValue");
                    OnPropertyChanged("IsValidValue");
                }
            }
        }

        /// <summary>
        /// Признак допустимости значения
        /// </summary>
        public bool IsValidValue
        {
            get
            {
                if (ignoreRange)
                    return true;
                return Compare(Value, MaxValue) < 0 && Compare(Value, MinValue) > 0;
            }
        }


        private int Compare(T left, T right)
        {
            if (left == null)
            {
                return right == null ? 0 : -1;
            }
            return left.CompareTo(right);
        }

        private bool ignoreRange = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IgnoreRange
        {
            get { return ignoreRange; }
            set { ignoreRange = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is RangedValue<T>)
            {
                var right = obj as RangedValue<T>;
                return Compare(Value, right.Value) == 0 && Compare(MaxValue, right.MaxValue) == 0 && Compare(MinValue, right.MinValue) == 0;
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DataStatistics : DATBaseViewModel
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="channel"></param>
        public DataStatistics(string channel)
        {
            ChannelName = channel;
        }

        private string channelName;
        /// <summary>
        /// Имя канала
        /// </summary>
        public string ChannelName
        {
            get { return channelName; }
            private set
            {
                if (channelName != value)
                {
                    channelName = value;
                    OnPropertyChanged("ChannelName");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="average"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="rms"></param>
        public void UpdateStatistics(double average, double min, double max, double rms)
        {
            Average.Value = average;
            Swing.Value = max - min;
            RMS.Value = rms;
            MaxSignal = max;
            MinSignal = min;
        }

        private RangedValue<double> average = new RangedValue<double>(0, double.MinValue, double.MaxValue);
        /// <summary>
        /// Среднее значение сигнала (В)
        /// </summary>
        public RangedValue<double> Average
        {
            get { return average; }
            set
            {
                if (average != value)
                {
                    average = value;
                    OnPropertyChanged("Average");
                }
            }
        }

        private RangedValue<double> swing = new RangedValue<double>(0, 0, double.MaxValue);
        /// <summary>
        /// Размах сигнала (В)
        /// </summary>
        public RangedValue<double> Swing
        {
            get { return swing; }
            set
            {
                if (swing != value)
                {
                    swing = value;
                    OnPropertyChanged("Amplitude");
                }
            }
        }

        private RangedValue<double> rms = new RangedValue<double>(0, 0, double.MaxValue);
        /// <summary>
        /// RMS (В)
        /// </summary>
        public RangedValue<double> RMS
        {
            get { return rms; }
            set
            {
                if (rms != value)
                {
                    rms = value;
                    OnPropertyChanged("RMS");
                }
            }
        }

        private double minSignal;

        /// <summary>
        /// Минимальное значение сигнала
        /// </summary>
        public double MinSignal
        {
            get { return minSignal; }
            set
            {
                minSignal = value;
                OnPropertyChanged("MinSignal");
            }
        }

        private double maxSignal;

        /// <summary>
        /// Максимальное значение сигнала
        /// </summary>
        public double MaxSignal
        {
            get { return maxSignal; }
            set
            {
                maxSignal = value;
                OnPropertyChanged("MaxSignal");
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class ScaleItem
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="displayValue"></param>
        public ScaleItem(float scale, string displayValue)
        {
            Scale = scale;
            DisplayValue = displayValue;
        }
        private float scale;
        /// <summary>
        /// Масштаб
        /// </summary>
        public float Scale
        {
            get { return scale; }
            private set { scale = value; }
        }

        private string displayValue;
        /// <summary>
        /// Отображаемое значение
        /// </summary>
        public string DisplayValue
        {
            get { return displayValue; }
            private set { displayValue = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DisplayValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ScaleItem)
            {
                return Scale == (obj as ScaleItem).Scale;
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}
