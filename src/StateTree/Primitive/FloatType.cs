namespace Skclusive.Mobx.StateTree
{
    public class FloatType : CoreType<float, float>, ISimpleType<float>
    {
        public FloatType() :
            base("float", TypeFlags.Number, (object value) => value is float)
        {
        }
    }
}
