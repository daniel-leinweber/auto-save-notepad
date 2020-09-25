using AutoSaveNotepad.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows.Input;

namespace AutoSaveNotepad.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            bool output = false;

            if (EqualityComparer<T>.Default.Equals(backingStore, value) == false)
            {
                backingStore = value;
                onChanged?.Invoke();
                RaisePropertyChanged(propertyName);

                output = true;
            }

            return output;
        }

        #endregion

        public string PageTitle
        {
            get { return _pageTitle; }
            set { SetProperty(ref _pageTitle, value); }
        }
        private string _pageTitle;
        
    }
}
