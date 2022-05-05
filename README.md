DiscordBot
==========
DiscordBot is currently being re-written to support slashcommands and other interactions. Also it's pretty cool to see how far I came from when I frist used this as project to get a job and learn to code :)
A Discord Bot written in C# using the Discord.NET library. I plan to continue to expand this bot with common Discord bot features. If you found a bug, would like to see a command/feature/different database or ORM/etc please open an issues, I love a challenge :)

If you would like to contribute, offer suggestion/criticism or hang out on the Discord server I test the bot in join here: https://discord.gg/2bxgHyfN6A If you would like to invite the bot to your sever please use this link: https://discord.com/api/oauth2/authorize?client_id=970419555400839219&permissions=8&scope=bot%20applications.commands

**Important:** As I re-write the bot it will likely be offline more than online. It's only hosted on my desktop as I debug it at the moment.

I am open to doing custom Discord Bot development as well, if you are interested please join the discord above and we can discuss pricing. Or possibly free/low cost if its something I find interesting.

DiscordBot had the following features, I am currently re-implemnting them during the re-write:

* Auto Roles: Allow Discord roles to be registered with the bot, these registered roles will be given to all members when joining the server.
  * autoroles, addautotole, delautorole, runautorules
* Fun: 
  * 8ball, coinflip, rolldie, RussianRoulette, lmgtfy (Let Me Google That for You), RockPaperScissors, giphy, random
* General commands including:
  * Math, ping, about, owner, echo, info, server, image, help {command_name}, uptime, servers
* Moderation commands including:
  * Ability to enable or disable sending invites to other server (serverinvites)
  * Ability to censor or delete messages with profanity, and allow or disallow words. This is pretty basic and doesn't take too much creativity to bypass :(
  * Ability to warn users and kick or ban them after a given number of warnings (warn, warnaction, getwarnings)
  * ban, unban, kick, purge, prefix, welcome, mute, unmute, slowmode, logs, embedcolor, profanityallow, profanityblock, profanityfilter
* Music Commands
  * search, play, join, skip, pause, resume
* Ranks: Allows discord roles to be registered with the bot as ranks, users can assign these ranks to themselves
  * ranks, addrank, delrank, rank
* Reddit command
  * Show a post from a chosen or random subreddit (reddit) delete known subreddit (subredditremove)
  * subredditlearning can be turned 'on' or 'off' and requires the redditor role to teach the bot new subreddits
* Timzone features:
  * registertimezone, userstime, time (get the time in any timezone), validtimezone (validate a timezone)
* Web
  * google (search google), youtube (search youtube)
* Note Commands
   * note create, note list, note delete, note show
* Pomodoro Timer Commands. Bot DMs you when time is up, and responds in channel the command was used in.
   * pomodoro start, pomodoro shortbreak, pomodoro longbreak

DiscordApi is an API that DiscordBot can integrate with to report log events per server, and command usage. DiscordBotApiWrapper is an API wrapper that makes this intergration easier. The next step is to construct a website to show log events and command usage statistics.

This bot uses Dapper as the ORM and currently uses SQL Server as the database engine. I am unlikely to support any other DBs at this time.

Technologies used in the making of this bot: C#, Discord.NET, Serilog, Dapper, Entity Framework Core, SQL Server, ASP.NET Core Web API

Notable Nuget Packages Used: Victora, Profanity.Detector, TimezoneConverter, Discord.Addons.Interactive 