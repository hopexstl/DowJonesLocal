using DowJones.Models;
using DowJones.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;

namespace DJLocal.Helpers
{
    public class AuthHelper
    {
        private readonly AuthService _authService;

        public AuthHelper(HttpClient httpClient)
        {
            TokenRequestModel tokenRequestModel = new TokenRequestModel
            {
                client_id = ConfigurationManager.AppSettings["ClientId"],
                username = ConfigurationManager.AppSettings["Username"],
                password = ConfigurationManager.AppSettings["Password"],
                grant_type = ConfigurationManager.AppSettings["grant_type"],
                device = ConfigurationManager.AppSettings["device"],
                scope = ConfigurationManager.AppSettings["scope"],
                connection = ConfigurationManager.AppSettings["connection"]
            };

            _authService = new AuthService(httpClient, tokenRequestModel);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            TokenResponseModel idTokenResponse = await _authService.GetIdTokenAsync();
            if (idTokenResponse != null && !string.IsNullOrEmpty(idTokenResponse.id_token))
            {
                TokenResponseModel accessTokenResponse = await _authService.GetAccessTokenAsync(idTokenResponse.id_token);
                return accessTokenResponse?.access_token;
            }

            return null;
        }
    }
}
