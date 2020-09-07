using YouYouServer.Model;

namespace YouYouServer.Model
{
    public interface IPlayerForWorldClientHandler
    {
        public void Init(PlayerForWorldClient playerForWorldClient);

        public void Dispose();
    }
}