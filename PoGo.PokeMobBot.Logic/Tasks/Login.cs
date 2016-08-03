#region using directives

using System;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public interface ILogin
    {
        Task DoLogin();
    }

    public class Login : ILogin
    {
        private readonly Client _client;

        public Login(Client client)
        {
            _client = client;
        }

        public async Task DoLogin()
        {
            try
            {
                await _client.Login.DoLogin();
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten().InnerException;
            }
        }
    }
}