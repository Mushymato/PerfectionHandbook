using StardewValley;

namespace PerfectionHandbook.Models;

public sealed class ReprObject : SObject
{
    public readonly Item innerItem;

    public ReprObject(Item innerItem)
    {
        this.innerItem = innerItem;
        this.CopyFieldsFrom(innerItem);
        this.Category = innerItem.Category;
        this.ResetParentSheetIndex();
    }

    public override string TypeDefinitionId => innerItem.TypeDefinitionId;

    public override int maximumStackSize()
    {
        return int.MaxValue;
    }

    private int reprStack = 0;
    public int ReprStack => reprStack;
    public override int Stack
    {
        get => Math.Min(reprStack, 99999);
        set { }
    }

    internal ReprObject SetReprStack(int stack)
    {
        reprStack = stack;
        return this;
    }

    protected override Item GetOneNew()
    {
        return new ReprObject(innerItem.getOne());
    }
}
