using System;

namespace XIV.TweenSystem
{
    internal sealed class OnCompleteCallbackTween : CallbackTween
    {
        Action action;

        public OnCompleteCallbackTween Set(Action action)
        {
            this.action = action;
            return this;
        }
        
        protected override void OnComplete()
        {
            action.Invoke();
        }

        protected override void OnCanceled()
        {
            
        }
    }
}