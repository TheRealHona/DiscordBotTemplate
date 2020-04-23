﻿using System;
using System.IO;
using System.Linq;
using Discord;
using DiscordBotTemplate.Constants;
using DiscordBotTemplate.Discord;
using DiscordBotTemplate.Logging;
using DiscordBotTemplate.Utilities;

namespace DiscordBotTemplate
{
    public static class Program
    {
        public static void Main()
        {
            // Get the current environment
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            
            // Make sure environment is valid
            if (!Enum.TryParse(typeof(Models.Environment), environment, true, out _))
            {
                Logger.LogError($"'{environment}' is not a valid environment, values are: '{string.Join("', '", ((Models.Environment[])Enum.GetValues(typeof(Models.Environment))).Select(x => x.ToString()))}'");
                ApplicationHelper.AnnounceAndExit();
            }

            // Load the bot token from file, and clean the line endings that can change between Windows/Unix
            var discordBotToken = File.ReadAllText(PathConstants.DiscordBotTokenFile).CleanLineEndings();

            // Validate token early
            try
            {
                TokenUtils.ValidateToken(TokenType.Bot, discordBotToken);
            }
            catch
            {
                Logger.LogError("The discord bot token was invalid, please check the value in file: " + PathConstants.DiscordBotTokenFile);
                ApplicationHelper.AnnounceAndExit();
            }

            var templateBot = new TemplateBot(discordBotToken);

            // Start the bot in async context from a sync context
            var closingException = templateBot.RunAsync().GetAwaiter().GetResult();

            if (closingException == null)
            {
                ApplicationHelper.AnnounceAndExit();
            }
            else
            {
                Logger.LogError("Caught crashing exception");
                Logger.LogException(closingException);
                Console.WriteLine();
                ApplicationHelper.AnnounceAndExit();
            }
        }
    }
}
