namespace Skclusive.Mobx.StateTree
{
    public class NullType : CoreType<object, object>, ISimpleType<object>
    {
        public NullType() :
            base("null", TypeFlags.Null, (object value) => value == null)
        {
        }

        object IType.Create(object snapshot, IEnvironment environment)
        {
            return Create(snapshot, environment);
        }

        object IType<object, object>.Create(object snapshot, IEnvironment environment)
        {
            return Create(snapshot, environment);
        }
    }
}
