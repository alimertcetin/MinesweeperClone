using System;

namespace XIV.PoolSystem
{
    public interface IPool
    {
        Type StoredType { get; }
        void Return(IPoolable item);
    }

    public interface IPool<out T> : IPool where T : IPoolable
    {
        T GetItem();
    }
}