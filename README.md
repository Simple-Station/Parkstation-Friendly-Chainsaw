# Parkstation

<p align="center"><img src="https://raw.githubusercontent.com/Simple-Station/Parkstation-Friendly-Chainsaw/master/Resources/Textures/Logo/logo.png" width="1080px" /></p>

## Links

[Discord](https://discord.gg/49KeKwXc8g) | [Steam](https://store.steampowered.com/app/2585480/Space_Station_Multiverse/) | [Standalone](https://spacestationmultiverse.com/downloads/)

## Contributing

We are happy to accept contributions from anybody. Get in Discord if you want to help. We've got a [list of issues](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/issues) that need to be done and anybody can pick them up. Don't be afraid to ask for help in Discord either!

We are currently accepting translations of the game on our main repository. If you would like to translate the game into another language check the #contribution-help channel in our Discord.

If you make any contributions, please make sure to read the markers section in [MARKERS](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/blob/master/MARKERS.md)
Any changes made to files belonging to our upstream should be properly marked in accordance to what is specified there.

## Building

Refer to [the Space Wizards' guide](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) on setting up a development environment for general information, but keep in mind that Parkstation is a distant fork of Space Wizards' SS14 and not everything applies. We provide some scripts to make the job easier.

### Build dependencies

> - Git
> - DOTNET SDK 7.0 or higher
> - python 3.7 or higher


### Windows

> 1. Clone this repository
> 2. Run `RUN_THIS.py` to init submodules and download the engine, or run `git submodule update --init --recursive` in a terminal
> 3. Run the `Scripts/bat/run1buildDebug.bat`
> 4. Run the `Scripts/bat/run2configDev.bat` if you need other configurations run other config scripts
> 5. Run both the `Scripts/bat/run3server.bat` and `Scripts/bat/run4client.bat`
> 6. Connect to localhost and play

### Linux

> 1. Clone this repository
> 2. Run `RUN_THIS.py` to init submodules and download the engine, or run `git submodule update --init --recursive` in a terminal
> 3. Run the `Scripts/sh/run1buildDebug.sh`
> 4. Run the `Scripts/sh/run2configDev.sh` if you need other configurations run other config scripts
> 5. Run both the `Scripts/sh/run3server.bat` and `scripts/sh/run4client.sh`
> 6. Connect to localhost and play

### MacOS

> I don't know anybody using MacOS to test this, but it's probably roughly the same steps as Linux.

## License

Content contributed to this repository after commit 87c70a89a67d0521a56388e6b1c3f2cb947943e4 (`17 February 2024 23:00:00 UTC`) is licensed under the GNU Affero General Public License version 3.0 unless otherwise stated. See [LICENSE-AGPLv3](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/blob/master/LICENSE-AGPLv3.txt).

Content contributed to this repository before commit 87c70a89a67d0521a56388e6b1c3f2cb947943e4 (`17 February 2024 23:00:00 UTC`) is licensed under the MIT license unless otherwise stated. See [LICENSE-MIT](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/blob/master/LICENSE-MIT.txt).

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.
