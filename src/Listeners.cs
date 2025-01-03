﻿using CounterStrikeSharp.API;
using AntiRush.Enums;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        if (_gameRules is { WarmupPeriod: true } && !Config.Warmup)
            return;

        if (!_minPlayers)
            return;

        if (Config.NoRushTime != 0 && !_bombPlanted)
        {
            var diff = (Config.NoRushTime + _roundStart) - Server.CurrentTime;

            if (diff > 0 && _countdown.Contains(diff))
                Server.PrintToChatAll($"{Prefix}{Localizer["delayRemaining", Localizer["rushDisabled"], diff.ToString("0")]}");
            else if (diff == 0)
                Server.PrintToChatAll($"{Prefix}{Localizer["rushDisabled"]}");
        }

        if (Config.NoCampTime != 0)
        {
            var diff = (Config.NoCampTime + _roundStart) - Server.CurrentTime;

            if (diff > 0 && _countdown.Contains(diff))
                Server.PrintToChatAll($"{Prefix}{Localizer["delayRemaining", Localizer["campEnabled"], diff.ToString("0")]}");
            else if (diff == 0)
                Server.PrintToChatAll($"{Prefix}{Localizer["campEnabled"]}");
        }

        foreach (var controller in Utilities.GetPlayers().Where(c => c.IsValid() && c.PawnIsAlive))
        {
            foreach (var zone in _zones)
            {
                if (((Config.NoRushTime != 0 && Config.NoRushTime + _roundStart < Server.CurrentTime) || _bombPlanted) && Config.RushZones.Contains((int)zone.Type))
                    continue;

                if (Config.NoCampTime != 0 && Config.NoCampTime + _roundStart > Server.CurrentTime && Config.CampZones.Contains((int)zone.Type))
                    continue;

                var isInZone = zone.IsInZone(controller.PlayerPawn.Value!.AbsOrigin!);

                if (!zone.Data.TryGetValue(controller, out _))
                    zone.Data[controller] = new ZoneData();

                if (!isInZone)
                {
                    if (zone.Data[controller].Entry != 0)
                    {
                        zone.Data[controller].Entry = 0;
                        zone.Data[controller].Exit = Server.CurrentTime;
                    }
                    
                    continue;
                }

                if (zone.Data[controller].Entry == 0)
                {
                    zone.Data[controller].Entry = Server.CurrentTime;
                    zone.Data[controller].Exit = 0;
                }

                if (!zone.Teams.Contains(controller.Team))
                    continue;

                if (zone.Delay != 0)
                {
                    var diff = (zone.Data[controller].Entry + zone.Delay) - Server.CurrentTime;

                    if (diff > 0)
                    {
                        var diffString = diff % 1;

                        if (diffString.ToString("0.00") is ("0.00" or "0.01") && diff >= 1)
                            controller.PrintToChat($"{Prefix}{Localizer["delayRemaining", zone.ToString(Localizer), diff.ToString("0")]}");
                    }
                    else
                        DoAction(controller, zone);

                    continue;
                }

                DoAction(controller, zone);
            }
        }

        return;
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }

    private void OnClientPutInServer(int playerSlot)
    {
        var controller = Utilities.GetPlayerFromSlot(playerSlot);

        if (controller == null || !controller.IsValid())
            return;

        _playerData[controller] = new PlayerData();
    }
}