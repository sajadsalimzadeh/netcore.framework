namespace Devor.Framework.Entities.Abstractions
{
    public interface IModificationAudit : IModificationTime
    {
        long? ModifierId { get; set; }
        string ModifierName { get; set; }
    }
}
