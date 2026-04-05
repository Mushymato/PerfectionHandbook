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

    public override int Stack
    {
#pragma warning disable AvoidNetField // Avoid Netcode types when possible
        get => base.stack.Value;
        set => base.stack.Value = value;
#pragma warning restore AvoidNetField // Avoid Netcode types when possible
    }

    protected override Item GetOneNew()
    {
        return new ReprObject(innerItem.getOne());
    }
}
