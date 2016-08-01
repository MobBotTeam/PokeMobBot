using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Login;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;

namespace PokemonGo.RocketAPI.Rpc
{
    public delegate void GoogleDeviceCodeDelegate(string code, string uri);
    public class Login : BaseRpc
    {
        //public event GoogleDeviceCodeDelegate GoogleDeviceCodeEvent;

        public Login(Client client) : base(client)
        {
        }

        public async Task DoGoogleLogin(string username,string password)
        {
            _client.AuthType = AuthType.Google;

            _client.AuthToken = GoogleLoginGPSOAuth.DoLogin(username, password);
            await SetServer();

            /*
            * This is our old authentication method
            * Keeping this around in case we might need it later on
            *
            GoogleLogin.TokenResponseModel tokenResponse = null;
            if (_client.Settings.GoogleRefreshToken != string.Empty)
            {
                tokenResponse = await GoogleLogin.GetAccessToken(_client.Settings.GoogleRefreshToken);
                _client.AuthToken = tokenResponse?.id_token;
            }

            if (_client.AuthToken == null)
            {
                var deviceCode = await GoogleLogin.GetDeviceCode();
                if(deviceCode?.user_code == null || deviceCode?.verification_url == null)
                    throw new GoogleOfflineException();

                GoogleDeviceCodeEvent?.Invoke(deviceCode.user_code, deviceCode.verification_url);
                tokenResponse = await GoogleLogin.GetAccessToken(deviceCode);
                _client.Settings.GoogleRefreshToken = tokenResponse?.refresh_token;
                _client.AuthToken = tokenResponse?.id_token;
            }

            await SetServer();
            */
        }

        public async Task DoPtcLogin(string username, string password)
        {
            _client.AuthToken = await PtcLogin.GetAccessToken(username, password);
            _client.AuthType = AuthType.Ptc;

            await SetServer();
        }

        private async Task SetServer()
        {
            #region Standard intial request messages in right Order

            var getPlayerMessage = new GetPlayerMessage();
            var getHatchedEggsMessage = new GetHatchedEggsMessage();
            var getInventoryMessage = new GetInventoryMessage
            {
                LastTimestampMs = DateTime.UtcNow.ToUnixTime()
            };
            var checkAwardedBadgesMessage = new CheckAwardedBadgesMessage();
            var downloadSettingsMessage = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            #endregion

            var serverRequest = RequestBuilder.GetInitialRequestEnvelope(
                new Request
                {
                    RequestType = RequestType.GetPlayer,
                    RequestMessage = getPlayerMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetHatchedEggs,
                    RequestMessage = getHatchedEggsMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetInventory,
                    RequestMessage = getInventoryMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.CheckAwardedBadges,
                    RequestMessage = checkAwardedBadgesMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.DownloadSettings,
                    RequestMessage = downloadSettingsMessage.ToByteString()
                });


            var serverResponse = await PostProto<Request>(Resources.RpcUrl, serverRequest);

            if (serverResponse.AuthTicket == null)
            {
                _client.AuthToken = null;
                throw new AccessTokenExpiredException();
            }

            _client.AuthTicket = serverResponse.AuthTicket;
            _client.ApiUrl = serverResponse.ApiUrl;
        }

    }
}
