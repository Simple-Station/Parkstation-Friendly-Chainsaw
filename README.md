# Parkstation

## Links

[Discord](https://discord.gg/49KeKwXc8g) | [Steam](https://store.steampowered.com/app/2585480/Space_Station_Multiverse/) | [Standalone](https://spacestationmultiverse.com/downloads/)

## Contributing

We are happy to accept contributions from anybody. Get in Discord if you want to help. We've got a [list of issues](https://github.com/orgs/Park-Station/projects/1/views/1) that need to be done and anybody can pick them up. Don't be afraid to ask for help either!

## Building

Refer to [the Space Wizards' guide](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) on setting up a development environment for general information, but keep in mind that Parkstation is a distant fork of Space Wizards' SS14 and not everything applies. We provide some scripts for making the job easier.

### Build dependencies

> - Git
> - .NET SDK 7.0 or higher
> - python 3.7 or higher


### Windows

> 1. Clone this repository.
> 2. Run `RUN_THIS.py` to init submodules and download the engine, or run `git submodule update --init --recursive` in a terminal.
> 3. Run the `Scripts/bat/run1buildDebug.bat`
> 4. Run the `Scripts/bat/run2configDev.bat` if you need other configurations run other config scripts.
> 5. Run both the `Scripts/bat/run3server.bat` and `Scripts/bat/run4client.bat`
> 6. Connect to localhost and play.

### Linux

> 1. Clone this repository.
> 2. Run `RUN_THIS.py` to init submodules and download the engine, or run `git submodule update --init --recursive` in a terminal.
> 3. Run the `Scripts/sh/run1buildDebug.sh`
> 4. Run the `Scripts/sh/run2configDev.sh` if you need other configurations run other config scripts.
> 5. Run both the `Scripts/sh/run3server.bat` and `scripts/sh/run4client.sh`
> 6. Connect to localhost and play.

## License

All code for the content repository is licensed under [MIT](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/blob/master/LICENSE.TXT).

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/Simple-Station/Parkstation-Friendly-Chainsaw/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.
