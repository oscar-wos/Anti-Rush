﻿using AntiRush.Enums;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public partial class AntiRush : BasePlugin
{
    public override void Load(bool isReload)
    {
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventBulletImpact>(OnBulletImpact);
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect);

        AddCommand("css_antirush", "Anti-Rush", CommandAntiRush);
        AddCommand("css_addzone", "Add Zone", CommandAddZone);
        AddCommand("css_viewzones", "View Zones", CommandViewZones);

        if (!isReload)
            return;

        foreach (var controller in Utilities.GetPlayers())
            _playerData.Add(controller, new PlayerData());

        LoadJson(Server.MapName);
        Server.ExecuteCommand("mp_restartgame 1");
    }

    private void BouncePlayer(CCSPlayerController controller)
    {
        var pos = _playerData[controller].LastPosition;
        var vel = _playerData[controller].LastVelocity;
        var speed = Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y);

        vel *= (-350 / (float)speed);
        vel.Z = vel.Z <= 0f ? 150f : Math.Max(vel.Z, 150f);
        controller.PlayerPawn.Value!.Teleport(pos, controller.PlayerPawn!.Value.EyeAngles, vel);
    }

    private void SaveZone(AddZoneMenu menu)
    {
        CsTeam[] teams = [];

        switch (menu.Items[1].Option)
        {
            case 0:
                teams = [CsTeam.Terrorist];
                break;

            case 1:
                teams = [CsTeam.CounterTerrorist];
                break;

            case 2:
                teams = [CsTeam.Terrorist, CsTeam.CounterTerrorist];
                break;
        }

        var minPoint = new Vector(Math.Min(menu.Points[0].X, menu.Points[1].X), Math.Min(menu.Points[0].Y, menu.Points[1].Y), Math.Min(menu.Points[0].Z, menu.Points[1].Z));
        var maxPoint = new Vector(Math.Max(menu.Points[0].X, menu.Points[1].X), Math.Max(menu.Points[0].Y, menu.Points[1].Y), Math.Max(menu.Points[0].Z, menu.Points[1].Z));

        if (!float.TryParse(menu.Items[3].DataString, out var delay))
            delay = 0.0f;

        if (!int.TryParse(menu.Items[4].DataString, out var damage))
            damage = 10;

        var zone = new Zone((ZoneType)menu.Items[0].Option, teams, minPoint, maxPoint, menu.Items[2].DataString, delay, damage);
        _zones.Add(zone);
    }

    private static bool IsValidPlayer(CCSPlayerController? player)
    {
        return player != null && player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, PawnIsAlive: true, IsBot: false };
    }

    private static bool VectorIsZero(Vector vector)
    {
        return vector is { X: 0.0f, Y: 0.0f, Z: 0.0f };
    }
}