namespace PlayerStates
{
    public interface IPlayerState
    {
        void HandleInput(PlayerController heroine);
        void Update(PlayerController heroine);
    }
}