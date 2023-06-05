using System;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Configuration;

namespace Oxide.Plugins
{
    [Info("AntiInsideTerrainViolation", "Hazmad", "1.4.3")]
    [Description("Teleports players to a safe location when they violate antihack InsideTerrain.")]
    class AntiInsideTerrainViolation : RustPlugin
    {
        private DynamicConfigFile config;
        private Vector3 safeLocation;
        private string chatMessage;
        private string consoleLogMessage;
        private const string permissionName = "antiinsideterrainviolation.allowed";

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
            permission.RegisterPermission(permissionName, this);
        }

        void LoadConfig()
        {
            try
            {
                safeLocation = ParseVector3(Config["SafeLocation"].ToString());
                chatMessage = Config["ChatMessage"].ToString();
                consoleLogMessage = Config["ConsoleLogMessage"].ToString();
            }
            catch
            {
                Puts("Error loading the config file. Using default values.");
                LoadDefaultConfig();
            }

            if (safeLocation == Vector3.zero)
            {
                Puts(
                    "Attention! Be aware that you have not specified a default secure location in the configuration file. This plugin will not function unless you configure a coordinate for player teleport."
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

        object OnPlayerViolation(BasePlayer player, AntiHackType type)
        {
            if (type == AntiHackType.InsideTerrain)
            {
                if (
                    player != null
                    && !player.IsAdmin
                    && !permission.UserHasPermission(player.UserIDString, permissionName)
                )
                {
                    // Player does not have the required permission, allow the default antihack behavior
                    return null;
                }

                HandleInsideTerrainViolation(player);
                return false; // Nullify the default antihack behavior
            }

            return null;
        }

        void HandleInsideTerrainViolation(BasePlayer player)
        {
            if (safeLocation == Vector3.zero)
            {
                Puts("Error: Safe location not set!");
                return;
            }

            // Set the unHostileTimestamp to 0
            player.State.unHostileTimestamp = 0;
            player.DirtyPlayerState();
            player.ClientRPCPlayer<float>(null, player, "SetHostileLength", 0f);

            // Put the player to sleep
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Sleeping, true);

            // Teleport the player to the safe location
            player.MovePosition(safeLocation);

            // Wake up the player after a short delay
            timer.Once(
                2f,
                () =>
                {
                    if (player.IsSleeping())
                    {
                        player.SetPlayerFlag(BasePlayer.PlayerFlags.Sleeping, false);
                    }
                }
            );

            player.ChatMessage(chatMessage);

            var violationLocation = player.transform.position.ToString();

            var logMessage = consoleLogMessage
                .Replace("{player}", player.displayName)
                .Replace("{playerID}", player.UserIDString)
                .Replace("{position}", violationLocation);
            Puts(logMessage);
        }

        Vector3 ParseVector3(string input)
        {
            var parts = input.Split(' ');
            if (parts.Length != 3)
                return Vector3.zero;

            float x,
                y,
                z;
            if (
                !float.TryParse(parts[0], out x)
                || !float.TryParse(parts[1], out y)
                || !float.TryParse(parts[2], out z)
            )
                return Vector3.zero;

            return new Vector3(x, y, z);
        }
    }
}