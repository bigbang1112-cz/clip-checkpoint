# Clip Checkpoint

[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/BigBang1112-cz/clip-checkpoint?include_prereleases&style=for-the-badge)](https://github.com/BigBang1112-cz/clip-checkpoint/releases)
[![GitHub all releases](https://img.shields.io/github/downloads/BigBang1112-cz/clip-checkpoint/total?style=for-the-badge)](https://github.com/BigBang1112-cz/clip-checkpoint/releases)

Powered by [GBX.NET](https://github.com/BigBang1112/gbx-net).

**Clip Checkpoint** is a tool that can extract checkpoint times from replays **and create a MediaTracker visualization as .Clip.Gbx** which can be then imported next to the replay.

The tool currently works on drag and dropping. You can also drag and drop multiple files to process.

A web interface is planned in the future.

Currently the tool can read inputs from these files:
- Replay.Gbx
- Ghost.Gbx
- Clip.Gbx (that contains minimally 1 ghost)

Where can I import the outputted Clip.Gbx?
- TrackMania Sunrise (`Legacy: true`)
- TrackMania United (`Legacy: true`)
- TrackMania United Forever
- TrackMania Nations Forever
- TrackMania 2
- Trackmania Turbo
- Trackmania (2020)

Where should the clip files be copied to?
- TMUF/TMNF: Documents/TrackMania/Tracks
- TMTurbo: Documents/TrackmaniaTurbo/Replays/Clips
- TM2: Documents/ManiaPlanet/Replays/Clips
- TM2020: Documents/Trackmania/Replays/Clips

Legacy versions:
- TMs: TrackMania Sunrise/GameData/Tracks
- TMU: Documents/TrackMania United/Tracks

Feel free to organize the foldering inside these folders.

## Troubleshooting

- **Issue: The program blinks when trying to run**
  - Solution: Install at least [.NET 5 Runtime](https://dotnet.microsoft.com/download/dotnet/6.0/runtime) and choose x64 or x86 depending on the OS you use.

## Settings

You can manage the default values in `Config.yml` file.
