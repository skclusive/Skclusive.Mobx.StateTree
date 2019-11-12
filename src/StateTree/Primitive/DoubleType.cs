namespace Skclusive.Mobx.StateTree
{
    public class DoubleType : CoreType<double, double>, ISimpleType<double>
    {
        public DoubleType() :
            base("double", TypeFlags.Number, (object value) => value is double)
        {
        }
    }
}
