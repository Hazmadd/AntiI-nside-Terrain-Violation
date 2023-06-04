using System;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("AntiInsideTerrainViolation", "Hazmad", "1.3.1")]
    [Description("Teleports players to a safe location when they violate antihack InsideTerrain.")]
    class AntiInsideTerrainViolation : RustPlugin
    {
        private DynamicConfigFile config;
        private Vector3 safeLocation;

        private string chatMessage;
        private string consoleLogMessage;

        protected override void LoadDefaultConfig()
        {
            Config["SafeLocation"] = "0 0 0";

            Config["ChatMessage"] =
                "Invalid terrain entry! You have been relocated to a secure area.";
            Config["ConsoleLogMessage"] =
                "Antihack violation: Player '{player}' ({playerID}) was teleported to a safe location. Violation Location: {position}";

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
                chatMessage = GetConfig<string>("ChatMessage");
                consoleLogMessage = GetConfig<string>("ConsoleLogMessage");
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
                        + "This plugin will not function unless you configure a coordinate for player teleport. "
                );
            }
        }

        T GetConfig<T>(string key, T defaultValue = default(T))
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
            {
                return null;
            }

            if (type == AntiHackType.InsideTerrain)
            {
                HandleInsideTerrainViolation(player);
                return true; // Handle default antihack behavior
            }

            return null;
        }

        void HandleInsideTerrainViolation(BasePlayer player)
        {
            if (safeLocation == null)
            {
                Puts("Error: Safe location not set!");
                return;
            }

            player.Teleport(safeLocation);

            player.ChatMessage(chatMessage);

            var violationLocation = player.transform.position.ToString();

            var logMessage = consoleLogMessage
                .Replace("{player}", player.displayName)
                .Replace("{playerID}", player.UserIDString)
                .Replace("{position}", violationLocation);
            LogToConsole(logMessage);
        }
    }
}
