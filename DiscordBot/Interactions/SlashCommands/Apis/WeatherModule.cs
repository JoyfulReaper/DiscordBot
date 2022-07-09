using Discord;
using Discord.Interactions;
using DiscordBot.Interactions.Infrastructure;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace DiscordBot.Interactions.SlashCommands.Apis;

public class WeatherModule : DiscordBotModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;
    private readonly IHttpClientFactory _httpClient;
    private readonly IConfiguration _config;

    public WeatherModule(IGuildService guildService, IHttpClientFactory httpClient, IConfiguration config) : base(guildService)
    {
        _guildService = guildService;
        _httpClient = httpClient;

        _config = config;
    }




    [SlashCommand("weather", "Get current weather")]
    public async Task GetWeather()
    {
        await Context.Channel.TriggerTypingAsync();

        var weatherForecast = new WeatherForecast();

        var client = _httpClient.CreateClient();
        client.BaseAddress = new Uri($"http://api.weatherapi.com/");
        var test = await client.GetFromJsonAsync<WeatherForecast>($"v1/current.json?key={_config["weatherApi_com_key"]}");
    }
}
