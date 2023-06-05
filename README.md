# Anti Inside Terrain Violation

For Oxide / Umod Rust Servers - Alternative solution for antihack InsideTerrain.

## Summary
- The AntiInsideTerrainViolation plugin is designed for Rust servers and handles InsideTerrain violations triggered by the antihack system.
- When a player violates InsideTerrain, they are automatically teleported to a safe location defined by the server administrators.
- The plugin provides customizable chat messages and console log messages to inform players and admins about the violation and teleportation.
- To prevent fall damage the plugin will force the player into a sleeping state and wake them.
- The plugin also resets any hostility timers for players who are teleported into Outpost / Bandit.

## Requirements

- The safe location coordinate must be set in the plugin's configuration file.
- Players / groups must also have the `antiinsideterrainviolation.allowed` permission for the plugin to have any effect on them.
- Server admins need to set the convar `antihack.terrain_kill` to `false` **otherwise players will still die by antihack.**

## Installation

1. Place the `AntiInsideTerrainViolation.cs` file in the `oxide/plugins` directory on your Rust server.

2. Start the server and the plugin will generate a configuration file in `oxide/config`.

3. Set the SafeLocation coordinate in the configuration file and reload the plugin with `o.reload AntiInsideTerrainViolation`.

4. You must grant players or groups the permission `antiinsideterrainviolation.allowed`. Without this permission the server will default to the vanilla antihack handler.

## Configuration

- **SafeLocation (IMPORTANT! Set valid safe location coordinate)**: Specify the coordinates of the safe location where players will be teleported when they violate InsideTerrain. Set the coordinates in the format `x y z` (e.g., `100 0 -50`). You can easily get your current coordinates in-game using `printpos` in the F1 console.

- **ChatMessage (Alert message sent to player on violation)**: Customize the message that will be displayed to players when they are teleported to the safe location.

- **ConsoleLogMessage (Alert message logged to console on violation)**: Customize the message that will be logged to the server console when a player violates InsideTerrain. The placeholders `{player}`, `{playerID}`, and `{position}` will be replaced with the player's display name, SteamID, and the violation location, respectively.

## License

This plugin is licensed under the [MIT License](LICENSE).
