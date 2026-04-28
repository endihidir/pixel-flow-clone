namespace Core.Pool.Services
{
    public interface IPooledObject
    { 
        public int PoolKey { get; set; }
        public bool IsActive { get; }
        public void Activate();
        public void Deactivate();
    }
}