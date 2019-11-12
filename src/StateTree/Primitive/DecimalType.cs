namespace Skclusive.Mobx.StateTree
{
    public class DecimalType : CoreType<decimal, decimal>, ISimpleType<decimal>
    {
        public DecimalType() :
            base("decimal", TypeFlags.Number, (object value) => value is decimal)
        {
        }
    }
}
