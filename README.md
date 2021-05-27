DiscordBot
==========
A Discord Bot written in C# using the Discord.NET library. I plan to continue to expland this bot with common Discord bot features. If you would like to see a command/feature/different database or ORM/etc please open an issues, I love a challenge :)

If you would like to contribute, offer suggestion/criticism or hang out on the Discord server I test the bot in join here: https://discord.gg/2bxgHyfN6A

DiscordBot has the following features:

* Auto Roles: Allow Discord roles to be registered with the bot, these registered roles will be given to all members when joing the server.
* Fun: 8ball command
* General commands including:
  * Math, ping, about, echo, info, server, image, help
* Moderation commands including:
  * purge, prefix, welcome, mute, unmute, slowmode, logs, embedcolor
* Music Commands
  * search, play, join, skip, pause, resume
* Ranks: Allows discord roles to be registered with the bot as ranks, users can assign these ranks to themselves
* Reddit command
  * Show a post from a chosen or random subreddit
  * subredditlearning can be turned 'on' or 'off' and requires the redditor role to teach the bot new subreddits

This bot uses Dapper as the ORM and currently uses SQLite as the database engine. 
If you find this bot useful, but would perfer a different database engine please let me know as I would like to look into adding support for other databases later.
