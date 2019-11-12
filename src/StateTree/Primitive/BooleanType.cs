namespace Skclusive.Mobx.StateTree
{
    public class BooleanType : CoreType<bool, bool>, ISimpleType<bool>
    {
        public BooleanType() :
            base("boolean", TypeFlags.Boolean, (object value) => value is bool)
        {
        }
    }
}
