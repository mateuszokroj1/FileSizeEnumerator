using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace FileSizeEnumerator
{
    public abstract class ModelBase : INotifyPropertyChanged
    {
        #region Constructor

        protected ModelBase()
        {
            PropertyChangedObservable = Observable.
                FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>
                (
                    handler => PropertyChanged += handler,
                    handler => PropertyChanged -= handler
                )
                .Select(args => args.EventArgs.PropertyName)
                .Where(name => !string.IsNullOrWhiteSpace(name));
        }

        #endregion

        #region Fields

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public IObservable<string> PropertyChangedObservable { get; }

        #endregion

        #region Methods

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if(!string.IsNullOrWhiteSpace(propertyName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T destination, T newValue, [CallerMemberName] string propertyName = "") =>
            SetProperties(ref destination, newValue, propertyName);

        protected void SetProperties<T>(ref T destination, T newValue, params string[] propertyNames)
        {
            if (!Equals(destination, newValue))
            {
                destination = newValue;

                foreach (var propertyName in propertyNames)
                    RaisePropertyChanged(propertyName);
            }
        }

        #endregion
    }
}
