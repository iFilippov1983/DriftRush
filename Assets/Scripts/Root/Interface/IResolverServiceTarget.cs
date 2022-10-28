using System;

namespace RaceManager.Root
{
    public interface IResolverServiceTarget<T>
    { 
        Type Type { get; }
        T Target { get; }
    }
}

