using Skclusive.Mobx.Observable;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Skclusive.Mobx.StateTree
{
    public interface ILateType : IType
    {
        IType SubType { get; }
    }
}
