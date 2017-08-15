using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using NeuroSoft.WPFComponents.ScalableWindows;

namespace NeuroSoft.DeviceAutoTest.Dialogs
{
    /// <summary>
    /// 
    /// </summary>
    public class DATDialogWindow : ScalableWindow, INotifyPropertyChanged
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="owner"></param>
        public DATDialogWindow()
        {
            InitializeOwner(null);
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="owner"></param>
        public DATDialogWindow(object owner)
        {
            InitializeOwner(owner);
        }

        private void InitializeOwner(object owner)
        {
            Window windowOwner = FindOwnerWindow(owner);
            if (windowOwner != null)
            {
                Owner = windowOwner;
            }
            else
            {
                IntPtr win32WindowHandle = FindOwnerIWin32Window(owner);
                if (win32WindowHandle != IntPtr.Zero)
                {
                    WindowInteropHelper helper = new WindowInteropHelper(this);
                    helper.Owner = win32WindowHandle;
                }
            }

            if (Owner != null)
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            else
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private Window FindOwnerWindow(object owner)
        {
            if (owner is Window)
            {
                return owner as Window;
            }
            else if (owner is DependencyObject)
            {
                DependencyObject element = owner as DependencyObject;
                return Window.GetWindow(element);
            }
            if (Application.Current != null)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.IsActive)
                    {
                        return window;
                    }
                }
            }
            return null;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetActiveWindow();

        private IntPtr FindOwnerIWin32Window(object owner)
        {
            if (owner is System.Windows.Forms.IWin32Window)
            {
                return (owner as System.Windows.Forms.IWin32Window).Handle;
            }
            else if (owner is System.Windows.Forms.Control)
            {
                System.Windows.Forms.Control control = owner as System.Windows.Forms.Control;
                System.Windows.Forms.Form form = control.FindForm();
                if (form != null)
                {
                    return form.Handle;
                }
            }
            return GetActiveWindow();
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
