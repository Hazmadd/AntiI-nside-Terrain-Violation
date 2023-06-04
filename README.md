# Anti-Inside Terrain Violation

Handles antihack InsideTerrain violations by teleporting players to a specified coordinate. Useful for custom maps / bad procgen. 

## Features

- Teleports players to a safe location upon InsideTerrain violation.
- Sends chat messages to players notifying them of the violation.
- Logs the violation to the server console.
~~- Supports integration with Discord webhooks to send violation reports.~~

## Installation

1. Download the latest release.
2. Extract the contents into your `oxide/plugins` directory of your Rust server.
3. Start or restart your Rust server.
4. **IMPORTANT** Configure the SafeLocation coordinate in the newly generated configuration file and make any other desired modifications.
5. Reload the plugin in the server console using the command `o.reload AntiInsideTerrainViolation` for the changes to take effect.

## Configuration

The plugin can be configured by editing the file located in your `oxide\config` directory of your Rust server.

## Permissions

- `antiinsideterrainviolation.bypass`: Players with this permission will not be affected by the AntiInsideTerrainViolation plugin, meaning they will be killed and kicked from the server on violation.

## Support

If you encounter any issues or have any questions or suggestions, please create an issue on the GitHub repository.
