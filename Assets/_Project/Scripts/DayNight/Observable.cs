using System;
using System.Collections.Generic;

[Serializable]
public class Observable<T> {
    private T value;
    public event Action<T> ValueChanged;

    public T Value {
        get => value;
        set => Set(value);
    }

    public static implicit operator T(Observable<T> observable) => observable.value;

    public Observable(T value, Action<T> onValueChanged = null) {
        this.value = value;

        if (onValueChanged != null)
            ValueChanged += onValueChanged;
    }

    public void Set(T value) {
        if (EqualityComparer<T>.Default.Equals(this.value, value))
            return;
        this.value = value;
        Invoke();
    }
    
    public void Invoke() {
        ValueChanged?.Invoke(value);
    }

    public void AddListener(Action<T> handler) {
        ValueChanged += handler;
    }

    public void RemoveListener(Action<T> handler) {
        ValueChanged -= handler;
    }

    public void Dispose() {
        ValueChanged = null;
        value = default;
    }
}
