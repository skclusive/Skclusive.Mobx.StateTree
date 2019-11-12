namespace Skclusive.Mobx.StateTree
{
    public class IntType : CoreType<int, int>, ISimpleType<int>
    {
        public IntType() :
            base("int", TypeFlags.Number, (object value) => value is int)
        {
        }
    }
}
