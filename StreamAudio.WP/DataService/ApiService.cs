using Newtonsoft.Json;
using StreamAudio.WP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StreamAudio.WP.DataService
{
    public interface IApiService
    {
        Task<List<AudioModel>> GetListAudio();
    }

    public class ApiService : IApiService
    {
        public async Task<List<AudioModel>> GetListAudio()
        {
            return await DoGetAsync<List<AudioModel>>(ApiUrls.SongList);
        }



        #region Base Handle
        protected async Task<TResponse> DoGetAsync<TResponse>(string url, string accessToken = "")
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //SessionManager.Instance.LoadRecentSession();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    //var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", SessionManager.Instance.UserId, SessionManager.Instance.AccessToken));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer ", accessToken);
                }
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    var repponseMessage = await httpClient.SendAsync(requestMessage);
                    string responseString = await repponseMessage.Content.ReadAsStringAsync();
                    var reponseObj = JsonConvert.DeserializeObject<TResponse>(responseString);
                    return reponseObj;
                }
            }
        }


        protected async Task<TResponse> DoPostAsync<TRequest, TResponse>(string url, TRequest requestArgs)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    var jsonContent = JsonConvert.SerializeObject(requestArgs, Formatting.Indented);
                    FormUrlEncodedContent postData = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string,string> ("data",jsonContent),});
                    var reponse = await httpClient.PostAsync(url, postData);
                    string responseString = await reponse.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TResponse>(responseString);
                }
            }
        }
        #endregion
    }
}
