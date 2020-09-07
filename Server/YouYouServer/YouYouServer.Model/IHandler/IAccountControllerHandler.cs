using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YouYouServer.Common;

namespace YouYouServer.Model
{
    public interface IAccountControllerHandler
    {
        public Task<AccountEntity> RegisterAsync(string userName, string pwd, short channelId, string deviceIdentifier, string deviceModel);
        public Task<AccountEntity> LoginAsync(string userName, string pwd, short channelId, string deviceIdentifier, string deviceModel);
        public Task<AccountEntity> GetAccountAsync(string key);
    }
}
