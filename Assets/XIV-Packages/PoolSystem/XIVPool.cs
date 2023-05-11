using System;
using System.Collections.Generic;

namespace XIV.PoolSystem
{
    public class XIVPool<T> : IPool<T> where T : IPoolable
    {
        public Type StoredType { get; }

        readonly Func<T> createFunc;
        Stack<IPoolable> poolables;
        
        public XIVPool(Func<T> createFunc) : this(2, createFunc)
        {
            
        }
        
        public XIVPool(int initialSize, Func<T> createFunc)
        {
            StoredType = typeof(T);
            this.createFunc = createFunc;
            initialSize = initialSize <= 0 ? 2 : initialSize;
            poolables = new Stack<IPoolable>(initialSize);
            for (int i = 0; i < initialSize; i++)
            {
                var item = createFunc.Invoke();
                item.OnPoolCreate(this);
                poolables.Push(item);
            }
        }

        public T GetItem()
        {
            if (poolables.Count > 0) return (T)poolables.Pop();
            
            var item = createFunc.Invoke();
            item.OnPoolCreate(this);
            return item;
        }
        
        public void Return(IPoolable item)
        {
            item.OnPoolReturn();
            poolables.Push(item);
        }
        
    }
}