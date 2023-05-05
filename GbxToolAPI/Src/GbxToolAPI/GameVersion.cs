using GBX.NET;
using GBX.NET.Engines.Game;

namespace GbxToolAPI;

public static class GameVersion
{
    public static bool IsManiaPlanet(CGameCtnGhost ghost)
    {
        return ghost.Chunks.Any(chunk => chunk.Id > 0x03092019 && chunk.Id <= 0x03092FFF);
    }
    
    public static bool IsManiaPlanet(CGameCtnChallenge map)
    {
        return map.Chunks.Any(chunk => chunk.Id > 0x0304302A && chunk.Id <= 0x03043FFF);
    }

    public static bool? IsManiaPlanet(Node node) => node switch
    {
        CGameCtnChallenge map => IsManiaPlanet(map),
        CGameCtnReplayRecord replay => replay.Chunks.Any(chunk => chunk.Id > 0x03093015 && chunk.Id <= 0x03093FFF),
        CGameCtnGhost ghost => IsManiaPlanet(ghost),
        _ => null
    };

    public static bool? IsTM2020(Node node) => node switch
    {
        CGameCtnChallenge map => map.Chunks.Any(chunk => chunk.Id > 0x03043059 && chunk.Id <= 0x03043FFF),
        CGameCtnReplayRecord replay => replay.Chunks.Any(chunk => chunk.Id > 0x03093026 && chunk.Id <= 0x03093FFF),
        CGameCtnGhost ghost => ghost.Chunks.Any(chunk => chunk.Id > 0x03092028 && chunk.Id <= 0x03092FFF),
        _ => null
    };

    public static bool? CanBeTMTurbo(Node node) => node switch
    {
        CGameCtnChallenge map => map.GetChunk<CGameCtnChallenge.Chunk03043040>()?.Version > 2 && !map.Chunks.Any(chunk => chunk.Id > 0x03043055 && chunk.Id <= 0x03043FFF),
        CGameCtnReplayRecord replay => replay.Chunks.Any(chunk => chunk.Id > 0x03093021) && !replay.Chunks.Any(chunk => chunk.Id > 0x03093023 && chunk.Id <= 0x03093FFF),
        CGameCtnGhost ghost => ghost.Chunks.Any(chunk => chunk.Id == 0x03092000) && !ghost.Chunks.Any(chunk => chunk.Id > 0x03092027 && chunk.Id <= 0x03092FFF),
        _ => null
    };

    public static bool? IsTMF(Node node) => node switch
    {
        CGameCtnChallenge map => map.Chunks.Any(chunk => chunk.Id >= 0x03043000) && !map.Chunks.Any(chunk => chunk.Id > 0x0304302A && chunk.Id <= 0x03043FFF),
        CGameCtnReplayRecord replay => replay.Chunks.Any(chunk => chunk.Id >= 0x03093000) && !replay.Chunks.Any(chunk => chunk.Id > 0x03093015),
        CGameCtnGhost ghost => ghost.Chunks.Any(chunk => chunk.Id >= 0x03092000) && !ghost.Chunks.Any(chunk => chunk.Id > 0x03092019),
        _ => null
    };
}
