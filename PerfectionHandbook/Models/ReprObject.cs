using StardewValley;

namespace PerfectionHandbook.Models;

public sealed class ReprObject : SObject
{
    public readonly Item innerItem;

    public ReprObject(Item innerItem)
    {
        this.innerItem = innerItem;
        this.CopyFieldsFrom(innerItem);
        this.ResetParentSheetIndex();
    }

    public override string TypeDefinitionId => innerItem.TypeDefinitionId;

    public override int maximumStackSize()
    {
        return int.MaxValue;
    }

    private int reprStack = 0;
    public override int Stack
    {
        get => reprStack;
        set { }
    }

    internal void SetReprStack(int stack)
    {
        reprStack = stack;
    }

    protected override Item GetOneNew()
    {
        return new ReprObject(innerItem.getOne());
    }
}
