using System;

namespace Skclusive.Mobx.StateTree
{
    public class GuidType : CoreType<Guid, Guid>, ISimpleType<Guid>
    {
        public GuidType() :
            base("guid", TypeFlags.String, (object value) => value is Guid)
        {
        }
    }
}
