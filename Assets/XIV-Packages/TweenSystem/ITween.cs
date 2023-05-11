namespace XIV.TweenSystem
{
    public interface ITween
    {
        void Update(float deltaTime);
        bool IsDone();
        void Complete();
        void Cancel();
    }
}