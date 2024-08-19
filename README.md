# Parkstation

<p align="center"><img src="https://raw.githubusercontent.com/Simple-Station/Parkstation-Friendly-Chainsaw/master/Resources/Textures/Logo/logo.png" width="1080px" /></p>


## Links

  [Website](https://simplestation.org)
| [Discord](https://discord.gg/49KeKwXc8g)
| [Steam (SSMV)](https://store.steampowered.com/app/2585480/Space_Station_Multiverse/)
| [Standalone (SSMV)](https://spacestationmultiverse.com/downloads/)
| [Steam (WizDen)](https://store.steampowered.com/app/1255460/Space_Station_14/)
| [Standalone (WizDen)](https://spacestation14.io/about/nightlies/)


## Contributing

We are happy to accept contributions from anybody.
Get in Discord if you want to help.
We've got a [list of issues](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/issues) that need to be done and anybody can pick them up.
Don't be afraid to ask for help in Discord either!

If you make any contributions, please make sure to read the markers section in [MARKERS](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/blob/master/MARKERS.md).
Any changes made to files belonging to our upstream should be properly marked in accordance to what is specified there.

Before making a PR here, consider if the feature is specific to us, or if it would generally improve the codebase.
Broader changes should be made on our upstream, [Einstein Engines](https://github.com/Simple-Station/Einstein-Engines), where many of the same maintainers manage the code.
If you're familiar with someone here, feel free to request a review from them in particular over there.

[Einstein Engines](https://github.com/Simple-Station/Einstein-Engines) is also accepting translations.
If you would like to translate the game into another language check the #contributor-general channel in their Discord.


## Building

Refer to [the Space Wizards' guide](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) on setting up a development environment for general information, but keep in mind that Parkstation is not the same and many things may not apply.
We provide some scripts shown below to make the job easier.

### Build Dependencies

> - DOTNET SDK 8.0 or higher

> - Git
> - .NET SDK 8.0.100

> - Git (needed)
> - Python 3.7 or higher
> - Visual Studio Code (**NOT** Visual Studio)

### Windows

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/bat/buildAllDebug.bat` after making any changes to the source
> 4. Run `Scripts/bat/runQuickAll.bat` to launch the client and the server
> 5. Connect to localhost in the client and play

### Linux

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` in a terminal to download the engine
> 3. Run `Scripts/sh/buildAllDebug.sh` after making any changes to the source
> 4. Run `Scripts/sh/runQuickAll.sh` to launch the client and the server
> 5. Connect to localhost in the client and play

### MacOS

> I don't know anybody using MacOS to test this, but it's probably roughly the same steps as Linux.


## License

Content contributed to this repository after commit 87c70a89a67d0521a56388e6b1c3f2cb947943e4 (`17 February 2024 23:00:00 UTC`) is licensed under the GNU Affero General Public License version 3.0 unless otherwise stated.
See [LICENSE-AGPLv3](./LICENSE-AGPLv3.txt).

Content contributed to this repository before commit 87c70a89a67d0521a56388e6b1c3f2cb947943e4 (`17 February 2024 23:00:00 UTC`) is licensed under the MIT license unless otherwise stated.
See [LICENSE-MIT](./LICENSE-MIT.txt).

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file.
[Example](./Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.
