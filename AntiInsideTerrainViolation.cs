using System;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("AntiInsideTerrainViolation", "Hazmad", "1.2.2")]
    [Description("Teleports players to a safe location when they violate antihack InsideTerrain.")]
    class AntiInsideTerrainViolation : RustPlugin
    {
        private DynamicConfigFile config;
        private Vector3 safeLocation;

        private string chatMessage;
        private string consoleLogMessage;
        private bool useDiscordWebhook;
        private string discordWebhookURL;
        private string discordWebhookMessage;

        protected override void LoadDefaultConfig()
        {
            Config["SafeLocation"] = "0 0 0";
            Config["UseDiscordWebhook"] = false;

            Config["ChatMessage"] =
                "Invalid terrain entry! You have been relocated to a secure area.";
            Config["ConsoleLogMessage"] =
                "Antihack violation: Player '{player}' ({playerID}) was teleported to a safe location. Violation Location: {position}";
            Config["DiscordWebhookURL"] = "";
            Config["DiscordWebhookMessage"] =
                "Antihack Violation\nPlayer: {player} ({playerID})\nViolation Location: {position}";

            SaveConfig();
        }

        void Init()
        {
            config = Interface.Oxide.DataFileSystem.GetFile("antiinsideterrainviolation");

            LoadConfig();
        }

        void LoadConfig()
        {
            try
            {
                safeLocation = GetConfig<Vector3>("SafeLocation");
                useDiscordWebhook = GetConfig<bool>("UseDiscordWebhook");
                discordWebhookURL = GetConfig<string>("DiscordWebhookURL");

                chatMessage = GetConfig<string>("ChatMessage");
                consoleLogMessage = GetConfig<string>("ConsoleLogMessage");
                discordWebhookMessage = GetConfig<string>("DiscordWebhookMessage");
            }
            catch
            {
                Puts("Error loading the config file. Using default values.");
                LoadDefaultConfig();
            }

            if (safeLocation == Vector3.zero)
            {
                Puts(
                    "Attention! Be aware that you have not specified a default secure location in the configuration file. "
                        + "Unless you have a deliberate intention for this, it could lead to players being unexpectedly teleported to hazardous areas. "
                        + "It is essential to input appropriate coordinates for a safe zone to prevent any potential risks."
                );
            }
        }

        T GetConfig<T>(string key, T defaultValue = default)
        {
            if (config[key] == null)
            {
                config[key] = defaultValue;
                SaveConfig();
            }

            return (T)Convert.ChangeType(config[key], typeof(T));
        }

        void OnServerInitialized()
        {
            permission.RegisterPermission("antiinsideterrainviolation.bypass", this);
        }

        object OnPlayerViolation(BasePlayer player, AntiHackType type)
        {
            if (
                player == null
                || player.IsAdmin
                || permission.UserHasPermission(
                    player.UserIDString,
                    "antiinsideterrainviolation.bypass"
                )
            )
                return null;

            if (type == AntiHackType.InsideTerrain)
            {
                HandleInsideTerrainViolation(player);
                return true; // Handle default antihack behavior
            }

            return null;
        }

        void HandleInsideTerrainViolation(BasePlayer player)
        {
            player.Teleport(safeLocation);

            player.ChatMessage(chatMessage);

            var violationLocation = player.transform.position.ToString();

            var logMessage = consoleLogMessage
                .Replace("{player}", player.displayName)
                .Replace("{playerID}", player.UserIDString)
                .Replace("{position}", violationLocation);
            LogToConsole(logMessage);

            if (useDiscordWebhook && !string.IsNullOrEmpty(discordWebhookURL))
                SendDiscordReport(player, violationLocation);
        }

        void SendDiscordReport(BasePlayer player, string violationLocation)
        {
            Puts("Discord Webhook functionality is not available without DiscordWebhooks.cs plugin.");
        }
    }
}
