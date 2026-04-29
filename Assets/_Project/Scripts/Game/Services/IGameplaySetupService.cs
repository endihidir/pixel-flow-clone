namespace Game.Services
{
    public interface IGameplaySetupService
    {
        void SetupGameplay();
        void ResetGameplay();
        void ReleaseFactories();
    }
}