using System;

namespace Skclusive.Mobx.StateTree
{
    public class StringType : CoreType<string, string>, ISimpleType<string>
    {
        public StringType() :
            base("string", TypeFlags.String, (object value) => value is string)
        {
        }
    }
}
