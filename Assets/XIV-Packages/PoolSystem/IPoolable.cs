namespace XIV.PoolSystem
{
    public interface IPoolable
    {
        void OnPoolCreate(IPool pool);
        void OnPoolReturn();
    }
}