using XIV.PoolSystem;

namespace XIV.TweenSystem
{
    public abstract class CallbackTween : ITween, IPoolable
    {
        IPool pool;
        
        protected abstract void OnComplete();
        protected abstract void OnCanceled();

        void ITween.Update(float deltaTime)
        {
            
        }
        
        bool ITween.IsDone() => true;
        
        void ITween.Complete()
        {
            OnComplete();
            pool?.Return(this);
        }

        void ITween.Cancel()
        {
            OnCanceled();
            pool?.Return(this);
        }

        void IPoolable.OnPoolCreate(IPool pool)
        {
            this.pool = pool;
        }

        void IPoolable.OnPoolReturn()
        {
            
        }
    }
}