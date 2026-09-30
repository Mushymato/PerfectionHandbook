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
        base.ValueSetter(Math.Clamp(newValue, minimum, maximum));
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

public sealed class EnumSpinBoxViewModel<TEnum>(
    Func<TEnum> backingGetter,
    Func<TEnum, bool> backingSetter,
    List<TEnum> validValues,
    string i18nPrefix
) : AbstractSpinBoxViewModel<TEnum>(backingGetter, backingSetter)
{
    public readonly List<TEnum> ValidValues = validValues;

    public override void ValueSetter(TEnum newValue)
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
            idx = ValidValues.Count - 1;
        else if (idx >= ValidValues.Count)
            idx = 0;
        Value = ValidValues[idx];
        return true;
    }

    public override bool Decrease() => ChangeIndex(-1);

    public override bool Increase() => ChangeIndex(1);

    public override string ValueLabelGetter() => I18n.GetByKey(string.Concat(i18nPrefix, Value));
}
