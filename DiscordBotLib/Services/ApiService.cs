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

using DiscordBotApiWrapper;
using Microsoft.Extensions.Logging;
using System;


namespace DiscordBotLib.Services
{
    public class ApiService : IApiService
    {
        public IServerLogItemApi ServerLogItemApi { get; set; }
        public ICommandItemApi CommandItemApi { get; set; }

        public bool ApiIsEnabled { get; set; }

        private readonly ILogger<ApiService> _logger;
        private readonly ISettings _settings;
        private readonly IServerLogItemApi _serverLogItemApi;
        private readonly ICommandItemApi _commandItemApi;
        private readonly IApiClient _apiClient;

        public ApiService(ILogger<ApiService> logger,
            ISettings settings,
            IServerLogItemApi serverLogItemApi,
            ICommandItemApi commandItemApi,
            ApiClient apiClient)
        {
            _logger = logger;
            _settings = settings;
            _serverLogItemApi = serverLogItemApi;
            _commandItemApi = commandItemApi;
            _apiClient = apiClient;

            ServerLogItemApi = _serverLogItemApi;
            CommandItemApi = _commandItemApi;

            ApiIsEnabled = _settings.UseDiscordBotApi;

            _apiClient.BaseAddress = new Uri(_settings.ApiBaseAddress);
        }
    }
}
