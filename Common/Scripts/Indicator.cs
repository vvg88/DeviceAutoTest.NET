using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using System.Windows.Threading;
using System.Windows;
using System.ComponentModel;

namespace NeuroSoft.DeviceAutoTest.Common.Scripts
{
    /// <summary>
    /// Индикатор, отображающий считываемые данные
    /// </summary>
    public class Indicator : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="content"></param>
        public Indicator(ScriptEnvironment environment, UIElement content)
        {
            Environment = environment;
            Content = content;
            timer.Tick += new EventHandler(timer_Tick);
        }        

        /// <summary>
        /// Переменная окружения
        /// </summary>
        protected ScriptEnvironment Environment;
        /// <summary>
        /// Таймер
        /// </summary>
        protected DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
        private UIElement content;

        /// <summary>
        /// Отображаемое содержимое
        /// </summary>
        protected virtual UIElement Content
        {
            get { return content; }
            set { content = value; }
        }

        /// <summary>
        /// Старт таймера
        /// </summary>
        public virtual void Start()
        {           
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            OnTimerTick();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnTimerTick()
        {
        }

        /// <summary>
        /// Остановка таймера
        /// </summary>
        public virtual void Stop()
        {
            timer.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            Stop();
            Environment = null;
            timer = null;
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
