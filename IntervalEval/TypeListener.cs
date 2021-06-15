using System;

namespace IntervalEval
{
    /// <summary>
    /// Wrapper around an object to allow a event to be fired on change.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypeListener<T>
    {
        private T _value;
        public EventHandler<T> OnChange { get; set; }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnChange?.Invoke(this, _value); // Fire event when value changed
            }
        }
    }
}