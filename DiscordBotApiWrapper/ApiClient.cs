/*
MIT License

Copyright(c) 2021 Kyle Givler
https://github.com/JoyfulReaper

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotApiWrapper
{
    public class ApiClient
    {
        private readonly HttpClient _client = new HttpClient();
        private Uri _baseAddress = new Uri("https://localhost:44383/api/");

        public Uri BaseAddress
        {
            get { return _baseAddress; }
            set 
            { 
                _baseAddress = value;
                _client.BaseAddress = _baseAddress;
            }
        } 


        public ApiClient(string userName, string password)
        {
            _client.BaseAddress = _baseAddress;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.UserAgent.Clear();
            _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DiscordBotApiWrapper", "0.0.1"));

            var authArray = Encoding.ASCII.GetBytes($"{userName}:{password}");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authArray));
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var response = await _client.GetFromJsonAsync<T>(uri);

            return response;
        }
    }
}
