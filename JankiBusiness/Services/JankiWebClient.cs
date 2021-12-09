using JankiTransfer.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace JankiBusiness.Services
{
    public class JankiWebClient
    {
        private string ServerAddress { get; set; } = "http://localhost:5000";

        private readonly HttpClient client = new HttpClient();

        public async Task ImportBundle(Guid id)
        {
            await client.PostAsync($"{ServerAddress}/bundle/import", new StringContent(
                JsonSerializer.Serialize(new ImportData() { Id = id }),
                Encoding.UTF8,
                "application/json"));
        }

        public async Task PublishBundle(IList<Guid> deckIds, string name)
        {
            await client.PostAsync($"{ServerAddress}/bundle/publish", new StringContent(
                JsonSerializer.Serialize(new PublishData() { DeckIds = deckIds, Name = name }),
                Encoding.UTF8,
                "application/json"));
        }

        public Task<List<DeckTreeModel>> GetAllDecks() => GetJsonAsync<List<DeckTreeModel>>($"{ServerAddress}/bundle/decks");

        public Task<List<BundleModel>> GetPublicBundles() => GetJsonAsync<List<BundleModel>>($"{ServerAddress}/bundle/bundles");

        public async Task PostSync(ChangeData data)
        {
            await client.PostAsync($"{ServerAddress}/sync", new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json"));
        }

        public Task<ChangeData> GetSync(DateTime since) => GetJsonAsync<ChangeData>($"{ServerAddress}/sync?since={HttpUtility.UrlEncode(since.ToString())}");

        public void SetBearerToken(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> Register(string username, string password)
        {
            using (HttpResponseMessage response = await client.PostAsync($"{ServerAddress}/account/signup", new StringContent(
                JsonSerializer.Serialize(new Signup() { UserName = username, Password = password }),
                Encoding.UTF8,
                "application/json")))
            {
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            try
            {
                Dictionary<string, string> content = new Dictionary<string, string>()
                {
                    ["grant_type"] = "password",
                    ["username"] = username,
                    ["password"] = password,
                    ["client_id"] = "rop",
                    ["client_secret"] = "SuperSecretClientSecret"
                };

                using (HttpResponseMessage response = await client.PostAsync($"{ServerAddress}/connect/token", new FormUrlEncodedContent(content)))
                {
                    return await GetJsonAsync<LoginResult>(response);
                }
            }
            catch (Exception)
            {
                return new LoginResult();
            }
        }

        private async Task<T> GetJsonAsync<T>(string uri, CancellationToken cancellationToken = default) where T : new()
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    return await GetJsonAsync<T>(response, cancellationToken);
                }
            }
            catch (Exception)
            {
                return new T();
            }
        }

        private async Task<T> GetJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default) where T : new()
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                response.EnsureSuccessStatusCode();

                if (response.Content == null)
                    throw new FormatException("The HTTP request didn't return any data.");

                Stream stream = await response.Content.ReadAsStreamAsync();

                return await Task.Run(async () => await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }, cancellationToken), cancellationToken);
            }
            catch (Exception)
            {
                return new T();
            }
        }
    }
}