using System.ComponentModel;
using PerfectionHandbook.Integration;

namespace PerfectionHandbook.GUI.Shared;

public class AbstractSpinBoxViewModel<T>(Func<T> backingGetter, Action<T> backingSetter) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void InvokePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(sender, e);
    }

    public T Value
    {
        get => ValueGetter();
        set => ValueSetter(value);
    }
    public string ValueLabel => ValueLabelGetter();

    public virtual T ValueGetter() => backingGetter();

    public virtual void ValueSetter(T newValue)
    {
        backingSetter(newValue);
        PropertyChanged?.Invoke(this, new(nameof(Value)));
        PropertyChanged?.Invoke(this, new(nameof(ValueLabel)));
    }

    public virtual string ValueLabelGetter() => Value?.ToString() ?? string.Empty;

    public virtual void Decrease() { }

    public virtual void Increase() { }

    public void Wheel(SDUIDirection direction)
    {
        switch (direction)
        {
            case SDUIDirection.North:
                Decrease();
                break;
            case SDUIDirection.South:
                Increase();
                break;
        }
    }
}

public class IntSpinBoxViewModel(Func<int> backingGetter, Action<int> backingSetter, int minimum, int maximum, int step)
    : AbstractSpinBoxViewModel<int>(backingGetter, backingSetter)
{
    public override void ValueSetter(int newValue)
    {
        if (newValue < minimum || newValue > maximum)
            return;
        base.ValueSetter(newValue);
    }

    public override void Decrease() => Value -= step;

    public override void Increase() => Value += step;
}
