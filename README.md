# Clip Checkpoint [(online)](https://gbx.bigbang1112.cz/tool/clip-checkpoint)

[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/BigBang1112-cz/clip-checkpoint?include_prereleases&style=for-the-badge)](https://github.com/BigBang1112-cz/clip-checkpoint/releases)
[![GitHub all releases](https://img.shields.io/github/downloads/BigBang1112-cz/clip-checkpoint/total?style=for-the-badge)](https://github.com/BigBang1112-cz/clip-checkpoint/releases)

Hosted on [Gbx Web Tools](https://github.com/bigbang1112-cz/gbx), lives on [Gbx Tool API](https://github.com/bigbang1112-cz/gbx-tool-api), internally powered by [GBX.NET](https://github.com/BigBang1112/gbx-net).

**Clip Checkpoint** is a tool that can extract checkpoint times from replays **and create a MediaTracker visualization as .Clip.Gbx** which can be then imported next to the replay.

### Web UI currently does not support the delta feature

Currently the tool can read checkpoints from these files:
- Replay.Gbx
- Ghost.Gbx
- Clip.Gbx (that contains minimally 1 ghost)

### Where can I import the outputted Clip.Gbx?

- TrackMania Sunrise (`Legacy: true`)
- TrackMania Nations ESWC (`Legacy: true`)
- TrackMania United (`Legacy: true`)
- TrackMania United Forever
- TrackMania Nations Forever
- TrackMania 2
- Trackmania Turbo
- Trackmania (2020)

## Settings

Configuration can be managed on the website in the Config component or inside the `Config` folder.

You can now have multiple configs and change between them.

## CLI build

For 100% offline control, you can use the CLI version of Clip Checkpoint. Drag and drop your desired replays onto the ClipCheckpointCLI(.exe).

### Delta mode

To use the Clip Checkpoint delta feature, drag and drop files onto `DeltaMode.bat` or `DeltaMode.sh`. Selection order of the files matter.

### ConsoleOptions.yml

- **NoPause** - If true, All "Press key to continue..." will be skipped.
- **SingleOutput** - If false, dragging multiple files will produce multiple results. If true, multiple files will produce only one result **(makes delta mode work)**.
- **CustomConfig** - Name of the config inside the `Config` folder without the extension.
- **OutputDir** - Forced output directory of produced results.

Location where the game exe is (you will be asked for it if ConsoleOptions.yml does not exist):

- **TrackmaniaForeverInstallationPath**
- **ManiaPlanetInstallationPath**
- **TrackmaniaTurboInstallationPath** 
- **Trackmania2020InstallationPath**

### Update notifications

The tool notifies you about new versions after launching it. You can press U to directly open the web page where you can download the new version. For security reasons, auto-updater is not planned atm.

### Update assets

You can update assets with execution of `UpdateAssets.bat` or `UpdateAssets.sh`. In case of Clip Checkpoint, this includes checkpoint sounds.

### Specific command line arguments

- `-nopause`
- `-singleoutput`
- `-config [ConfigName]`
- `-o [OutputDir]` or `-output [OutputDir]`
- `-updateassets` - Included in UpdateAssets.bat/sh
- `-c:[AnySettingName] [value]` - Force setting through the command line, **currently works only for string values.**
