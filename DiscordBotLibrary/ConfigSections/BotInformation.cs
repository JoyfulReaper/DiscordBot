/*
MIT License

Copyright(c) 2022 Kyle Givler
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


namespace DiscordBotLibrary.ConfigSections;

public class BotInformation
{
    public string BotName { get; set; } = "DiscordBot";
    public string BotWebsite { get; set; } = "https://github.com/JoyfulReaper/DiscordBot";
    public string DefaultPrefix { get; set; } = "!";
    public int PrefixMaxLength { get; set; } = 8;
    public string WelcomeMessage { get; set; } = "just spawned in!";
    public string PartMessage { get; set; } = "disappeared forever :(";
    public bool ShowBotJoinMessages { get; set; } = false;
    public int MaxUserNotes { get; set; } = 10;
    public int MaxWelcomeMessages { get; set; } = 10;
    public string InviteLink { get; set; } = "https://discord.com/api/oauth2/authorize?client_id=832404891379957810&permissions=268443670&scope=bot";
    public string DefaultBannerImage { get; set; } = "https://images.unsplash.com/photo-1500829243541-74b677fecc30?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=2555&q=80";
    public string SupportServer { get; set; } = "https://discord.gg/2bxgHyfN6A";
}
