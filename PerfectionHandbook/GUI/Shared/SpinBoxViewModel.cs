using System.ComponentModel;
using PerfectionHandbook.Integration;

namespace PerfectionHandbook.GUI.Shared;

public abstract class AbstractSpinBoxViewModel<T>(Func<T> backingGetter, Action<T> backingSetter)
    : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new(propName));
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
        RaisePropertyChanged(nameof(Value));
        RaisePropertyChanged(nameof(ValueLabel));
    }

    public virtual string ValueLabelGetter() => Value?.ToString() ?? string.Empty;

    public abstract void Decrease();

    public abstract void Increase();

    public bool Wheel(SDUIDirection direction)
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
        return true;
    }
}

public sealed class IntSpinBoxViewModel(
    Func<int> backingGetter,
    Action<int> backingSetter,
    int minimum,
    int maximum,
    int step
) : AbstractSpinBoxViewModel<int>(backingGetter, backingSetter)
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

public sealed class StringSpinBoxViewModel(
    Func<string> backingGetter,
    Action<string> backingSetter,
    string[] validValues,
    string i18nPrefix
) : AbstractSpinBoxViewModel<string>(backingGetter, backingSetter)
{
    public readonly string[] ValidValues = validValues;

    public override void ValueSetter(string newValue)
    {
        if (!ValidValues.Contains(newValue))
            return;
        base.ValueSetter(newValue);
    }

    private void ChangeIndex(int change)
    {
        int idx = ValidValues.IndexOf(Value);
        idx += change;
        if (idx < 0)
            idx = ValidValues.Length - 1;
        else if (idx >= ValidValues.Length)
            idx = 0;
        Value = ValidValues[idx];
    }

    public override void Decrease() => ChangeIndex(-1);

    public override void Increase() => ChangeIndex(1);

    public override string ValueLabelGetter() => I18n.GetByKey(string.Concat(i18nPrefix, Value));
}
