using YouYouServer.Model.ServerManager;

namespace YouYouServer.Model.IHandler
{
    public interface IPlayerForWorldClientHandler
    {
        public void Init(PlayerForWorldClient playerForWorldClient);

        public void Dispose();
    }
}