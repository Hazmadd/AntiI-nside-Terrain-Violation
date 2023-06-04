using System;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using Oxide.Ext.DiscordWebhooks;

namespace Oxide.Plugins
{
    [Info("AntiInsideTerrainViolation", "Hazmad", "1.0.0")]
    [Description("Teleports players to a safe location when they violate InsideTerrain.")]
    class AntiInsideTerrainViolation : RustPlugin
    {
        private DynamicConfigFile config;
        private Vector3 safeLocation;
        private DiscordWebhooks discord;

        private string chatMessage;
        private string consoleLogMessage;
        private string discordWebhookMessage;

        protected override void LoadDefaultConfig()
        {
            Config["SafeLocation"] = "0 0 0";
            Config["DiscordWebhookURL"] = "";

            Config["ChatMessage"] = "You fell into an unsafe area of the map and have been teleported to a safe location.";
            Config["ConsoleLogMessage"] = "InsideTerrain violation: Player '{player}' ({playerID}) was teleported to a safe location.";
            Config["DiscordWebhookMessage"] = "InsideTerrain Violation\nPlayer: {player} ({playerID})\nViolation Location: {position}";

            SaveConfig();
        }

        void Init()
        {
            config = Interface.Oxide.DataFileSystem.GetFile("antiinsideterrainviolation");
            discord = GetLibrary<DiscordWebhooks>();

            LoadConfig();
        }

        void LoadConfig()
        {
            try
            {
                safeLocation = GetConfig<Vector3>("SafeLocation");

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
                Puts("Attention! Be aware that you have not specified a default secure location in the configuration file. " +
                      "Unless you have a deliberate intention for this, it could lead to players being unexpectedly teleported to hazardous areas. " +
                      "It is essential to input appropriate coordinates for a safe zone to prevent any potential risks.");
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

        void OnEntityEnter(TriggerZone zone, BaseEntity entity)
        {
            var player = entity as BasePlayer;
            if (player == null)
                return;

            if (zone is TerrainTrigger && zone.Check(entity.WorldSpaceBounds().ToBounds()) == Zone.TestResult.FailInside)
            {
                if (player.IsAdmin || permission.UserHasPermission(player.UserIDString, "antiinsideterrainviolation.bypass"))
                    return;

                if (player.lastSentInsideTerrainDistance >= 200f)
                {
                    HandleInsideTerrainViolation(player);
                }
            }
        }

        void HandleInsideTerrainViolation(BasePlayer player)
        {
            player.Teleport(safeLocation);

            player.ChatMessage(chatMessage);

            var logMessage = consoleLogMessage.Replace("{player}", player.displayName).Replace("{playerID}", player.UserIDString);
            LogToConsole(logMessage);

            SendDiscordReport(player);
        }

        void SendDiscordReport(BasePlayer player)
        {
            var webhookURL = GetConfig<string>("DiscordWebhookURL");
            if (string.IsNullOrEmpty(webhookURL))
                return;

            var discordMessage = new DiscordWebhooks.Message
            {
                content = discordWebhookMessage
                    .Replace("{player}", player.displayName)
                    .Replace("{playerID}", player.UserIDString)
                    .Replace("{position}", player.transform.position.ToString())
            };

            discord.SendMessage(webhookURL, discordMessage);
        }
    }
}
