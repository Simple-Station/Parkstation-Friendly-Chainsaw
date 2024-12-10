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

> - Git
> - .NET SDK 8.0.100

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

Please read the [LEGAL.md](./LEGAL.md) file for information on the licenses of the code and assets in this repository.
