using System.ComponentModel;
using PerfectionHandbook.Integration;

namespace PerfectionHandbook.GUI.Shared;

public abstract class AbstractSpinBoxViewModel<T>(Func<T> backingGetter, Func<T, bool> backingSetter)
    : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(this, args);
    }

    public T Value
    {
        get => ValueGetter();
        set => ValueSetter(value);
    }
    private static readonly PropertyChangedEventArgs ValuePCEA = new(nameof(Value));
    public string ValueLabel => ValueLabelGetter();
    private static readonly PropertyChangedEventArgs ValueLabelPCEA = new(nameof(ValueLabel));

    public virtual T ValueGetter() => backingGetter();

    public virtual void ValueSetter(T newValue)
    {
        if (backingSetter(newValue))
        {
            OnPropertyChanged(ValuePCEA);
            OnPropertyChanged(ValueLabelPCEA);
        }
    }

    public virtual string ValueLabelGetter() => Value?.ToString() ?? string.Empty;

    public abstract bool Decrease();

    public abstract bool Increase();

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
    Func<int, bool> backingSetter,
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

    public override bool Decrease()
    {
        Value -= step;
        return true;
    }

    public override bool Increase()
    {
        Value += step;
        return true;
    }
}

public sealed class StringSpinBoxViewModel(
    Func<string> backingGetter,
    Func<string, bool> backingSetter,
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

    private bool ChangeIndex(int change)
    {
        int idx = ValidValues.IndexOf(Value);
        idx += change;
        if (idx < 0)
            idx = ValidValues.Length - 1;
        else if (idx >= ValidValues.Length)
            idx = 0;
        Value = ValidValues[idx];
        return true;
    }

    public override bool Decrease() => ChangeIndex(-1);

    public override bool Increase() => ChangeIndex(1);

    public override string ValueLabelGetter() => I18n.GetByKey(string.Concat(i18nPrefix, Value));
}
