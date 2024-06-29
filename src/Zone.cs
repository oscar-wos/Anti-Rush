﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using AntiRush.Enums;

namespace AntiRush;

public class Zone(ZoneType type, CsTeam[] teams, Vector minPoint, Vector maxPoint, string name, float delay, int damage)
{
    public ZoneType Type { get; init; } = type;
    public CsTeam[] Teams { get; init; } = teams;
    public Vector MinPoint { get; init; } = minPoint;
    public Vector MaxPoint { get; init; } = maxPoint;
    public string Name { get; init; } = name;
    public float Delay { get; init; } = delay;
    public int Damage { get; init; } = damage;
    public Dictionary<CCSPlayerController, ZoneData> Data { get; } = [];

    public bool IsInZone(Vector point)
    {
        return point.X >= MinPoint.X && point.X <= MaxPoint.X && point.Y >= MinPoint.Y && point.Y <= MaxPoint.Y && point.Z + 36 >= MinPoint.Z && point.Z + 36 <= MaxPoint.Z;
    }

    public string ToString(IStringLocalizer localized)
    {
        return Type switch
        {
            ZoneType.Bounce => $"{ChatColors.Yellow}{localized["zone.Bounce"]}{ChatColors.White}",
            ZoneType.Hurt => $"{ChatColors.Orange}{localized["zone.Hurt"]}{ChatColors.White}",
            ZoneType.Kill => $"{ChatColors.Red}{localized["zone.Kill"]}{ChatColors.White}",
            ZoneType.Teleport => $"{ChatColors.Magenta}{localized["zone.Teleport"]}{ChatColors.White}",
            _ => ""
        };
    }
}