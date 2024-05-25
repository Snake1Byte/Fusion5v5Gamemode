using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using BoneLib;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Utilities;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using MelonLoader;
using SLZ.Props.Weapons;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Fusion5vs5Gamemode.Shared;

public static class Commons
{
    public static class Metadata
    {
        // Metadata
        public const string DefaultPrefix = "Fusion5vs5";
        public const string TeamKey = DefaultPrefix + ".Team";
        public const string TeamScoreKey = DefaultPrefix + ".Score";
        public const string PlayerKillsKey = DefaultPrefix + ".Kills";
        public const string PlayerDeathsKey = DefaultPrefix + ".Deaths";
        public const string PlayerAssistsKey = DefaultPrefix + ".Assists";
        public const string RoundNumberKey = DefaultPrefix + ".RoundNumber";
        public const string GameStateKey = DefaultPrefix + ".GameState";
        public const string SpawnPointKey = DefaultPrefix + ".SpawnPoint";
        public const string PlayerFrozenKey = DefaultPrefix + ".PlayerFrozen";
    }

    public static class Events
    {
        public const string PlayerKilledPlayer = "PlayerKilledPlayer";
        public const string PlayerSuicide = "PlayerSuicide";
        public const string KillPlayer = "KillPlayer";
        public const string RevivePlayer = "RevivePlayer";
        public const string ReviveAndFreezePlayer = "ReviveAndFreezePlayer";
        public const string RespawnPlayer = "RespawnPlayer";
        public const string SetSpectator = "SetSpectator";
        public const string TeamWonRound = "TeamWonRound";
        public const string TeamWonGame = "TeamWonGame";
        public const string GameTie = "GameTie";
        public const string Fusion5vs5Started = "Fusion5vs5Started";
        public const string Fusion5vs5Aborted = "Fusion5vs5Aborted";
        public const string Fusion5vs5Over = "Fusion5vs5Over";
        public const string PlayerJoined = "PlayerJoined";
        public const string PlayerLeft = "PlayerLeft";
        public const string BuyTimeOver = "BuyTimeOver";
        public const string BuyTimeStart = "BuyTimeStart";
        public const string ItemBought = "ItemBought";
    }

    public static class ClientRequest
    {
        public const string ChangeTeams = "ChangeTeams";
        public const string JoinSpectator = "JoinSpectator";
        public const string BuyItem = "BuyItem";
        public const string BuyZoneEntered = "BuyZoneEntered";
        public const string BuyZoneExited = "BuyZoneExited";
    }

    static Commons()
    {
#if DEBUG
        builder.Append("==================================================================\n");
#endif
    }

    public static IEnumerator CoRunUponCondition(Action action, Func<bool> condition)
    {
        while (!condition.Invoke())
        {
            yield return null;
        }

        BoneLib.SafeActions.InvokeActionSafe(action);
    }

    public static IEnumerator CoRunUponConditionDelayFrames(Action action, Func<bool> condition, int delayFrames = 0)
    {
        while (!condition.Invoke())
        {
            yield return null;
        }

        for (int i = 0; i < delayFrames; i++)
        {
            yield return null;
        }

        BoneLib.SafeActions.InvokeActionSafe(action);
    }

    public static IEnumerator RunCoRoutine(Action action, Func<bool>? condition = null, float delaySeconds = 0)
    {
        if (condition != null)
        {
            while (!condition.Invoke())
            {
                yield return null;
            }
        }

        if (delaySeconds > 0)
        {
            yield return new WaitForSeconds(delaySeconds);
        }

        BoneLib.SafeActions.InvokeActionSafe(action);
    }

    public static List<Renderer> EnableDisabledRenderers(GameObject go)
    {
        List<Renderer> enabledRenderers = new();
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            if (!renderer.enabled)
            {
                enabledRenderers.Add(renderer);
                renderer.enabled = true;
            }
        }

        return enabledRenderers;
    }

    public static List<Renderer> DisableEnabledRenderers(GameObject go)
    {
        List<Renderer> disabledRenderers = new();
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            if (renderer.enabled)
            {
                disabledRenderers.Add(renderer);
                renderer.enabled = false;
            }
        }

        return disabledRenderers;
    }

    public static RigReferenceCollection? GetRigReferences(PlayerId player)
    {
        Log(player);

        RigReferenceCollection rigReferences;
        if (player.IsSelf)
        {
            rigReferences = RigData.RigReferences;
        }
        else
        {
            PlayerRepManager.TryGetPlayerRep(player, out PlayerRep playerRep);
            if (playerRep == null) return null;
            rigReferences = playerRep.RigReferences;
        }

        return rigReferences;
    }

    public const string SpectatorAvatar = CommonBarcodes.Avatars.PolyBlank;
    public static FusionDictionary<string, string> _Metadata { get; set; } = new();

    public static Fusion5vs5GamemodeTeams? GetTeam(PlayerId localId)
    {
        Log(localId);

        if (_Metadata.TryGetValue(GetTeamMemberKey(localId), out string team))
        {
            return GetTeamFromValue(team);
        }

        return null;
    }

    public static string GetTeamMemberKey(PlayerId id)
    {
        Log(id);

        return $"{Metadata.TeamKey}.{id.LongId}";
    }

    public static string GetTeamScoreKey(Fusion5vs5GamemodeTeams team)
    {
        Log(team);

        return $"{Metadata.TeamScoreKey}.{team.ToString()}";
    }

    public static Fusion5vs5GamemodeTeams? GetTeamFromValue(string value)
    {
        Log(value);

        try
        {
            return (Fusion5vs5GamemodeTeams)Enum.Parse(typeof(Fusion5vs5GamemodeTeams), value);
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Warning(
                $"Tried to parse an enum of type {nameof(Fusion5vs5GamemodeTeams)} with value \"{value}\" in GetTeamFromValue()!\n{e}");
#endif
            return null;
        }
    }

    public static string GetPlayerKillsKey(PlayerId killer)
    {
        Log(killer);

        return $"{Metadata.PlayerKillsKey}.{killer.LongId}";
    }

    public static string GetPlayerAssistsKey(PlayerId assister)
    {
        Log(assister);

        return $"{Metadata.PlayerAssistsKey}.{assister.LongId}";
    }

    public static string GetPlayerDeathsKey(PlayerId killed)
    {
        Log(killed);

        return $"{Metadata.PlayerDeathsKey}.{killed.LongId}";
    }

    public static PlayerId? GetPlayerFromValue(string player)
    {
        Log(player);

        ulong id = ulong.Parse(player);
        foreach (var playerId in PlayerIdManager.PlayerIds)
        {
            if (playerId.LongId == id)
            {
                return playerId;
            }
        }

        MelonLogger.Warning($"Could not find player with LongId {player} in GetPlayerFromValue()!");
        return null;
    }

    public static int GetPlayerKills(PlayerId killer)
    {
        Log(killer);

        _Metadata.TryGetValue(GetPlayerKillsKey(killer), out string killerScore);
        return int.Parse(killerScore);
    }

    public static int GetPlayerDeaths(PlayerId killed)
    {
        Log(killed);

        _Metadata.TryGetValue(GetPlayerDeathsKey(killed), out string deathScore);
        return int.Parse(deathScore);
    }

    public static int GetRoundNumber()
    {
        Log();

        _Metadata.TryGetValue(Metadata.RoundNumberKey, out string roundNumber);
        return int.Parse(roundNumber);
    }

    public static GameStates? GetGameStateFromValue(string value)
    {
        Log(value);

        try
        {
            return (GameStates)Enum.Parse(typeof(GameStates), value);
        }
        catch (Exception e)
        {
            MelonLogger.Warning(
                $"Tried to parse an enum of type {nameof(GameStates)} with value \"{value}\" in GetGameStateFromValue()!\n{e}");
            return null;
        }
    }

    public static GameStates? GetGameState()
    {
        Log();

        if (_Metadata.TryGetValue(Metadata.GameStateKey, out string gameState))
        {
            return GetGameStateFromValue(gameState);
        }

        MelonLogger.Warning(
            $"Could not find a GameState inside of Metadata dictionary.");
        return null;
    }

    public static SerializedTransform? GetSpawnPointFromValue(string value)
    {
        Log(value);

        try
        {
            string[] split = value.Split(',');
            Vector3 pos = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
            Vector3 rot =
                new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
            SerializedTransform spawnPoint = new SerializedTransform
            {
                position = pos,
                rotation = Quaternion.Euler(rot)
            };
            return spawnPoint;
        }
        catch (Exception e)
        {
            MelonLogger.Warning(
                $"Could not convert {value} to a {nameof(SerializedTransform)} in GetSpawnPointFromValue()!\n{e}");
            return null;
        }
    }

    public static SerializedTransform? GetSpawnPoint(PlayerId player)
    {
        Log(player);

        if (_Metadata.TryGetValue(GetSpawnPointKey(player), out string spawnPointRaw))
        {
            return GetSpawnPointFromValue(spawnPointRaw);
        }

        return null;
    }

    public static string GetSpawnPointKey(PlayerId player)
    {
        Log(player);

        return $"{Metadata.SpawnPointKey}.{player.LongId}";
    }

    public static string GetPlayerFrozenKey(PlayerId player)
    {
        Log(player);

        return $"{Metadata.PlayerFrozenKey}.{player.LongId}";
    }

    public static bool? IsPlayerFrozen(PlayerId player)
    {
        Log(player);

        if (_Metadata.TryGetValue(GetPlayerFrozenKey(player), out string frozen))
        {
            try
            {
                return bool.Parse(frozen);
            }
            catch (Exception e)
            {
                MelonLogger.Warning(
                    $"Could not convert {frozen} to a {nameof(SerializedTransform)} in GetSpawnPointFromValue()!\n{e}");
                return null;
            }
        }

        return null;
    }

    public static void RotateGunPerpendicular(Gun gun, SerializedTransform forwardTransform)
    {
    }

    private static StringBuilder builder = new();
    private static int threadNameCounter;

    public static void Log(params object[] parameters)
    {
#if DEBUG
        try
        {
            StackFrame frame = new StackFrame(1);
            MethodBase method = frame.GetMethod();
            if (method == null)
            {
                return;
            }

            DateTime currentTime = DateTime.Now;
            string formattedTime = $"[{currentTime:yyyy/MM/dd HH:mm:ss.fff}]";
            builder.Append(formattedTime);
            builder.Append("\t");

            string? threadName = Thread.CurrentThread.Name;
            if (threadName == null)
            {
                threadName = $"[Thread {threadNameCounter++}]";
                Thread.CurrentThread.Name = threadName;
            }

            builder.Append(threadName);
            builder.Append("\t");

            builder.Append(method.DeclaringType?.FullName + ".");
            int i = 0;
            builder.Append(method.Name);
            if (method.GetParameters().Length > 0)
            {
                builder.Append("(");
                foreach (var parameter in method.GetParameters())
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    if (parameters != null)
                    {
                        builder.Append(parameter);
                        builder.Append(" = ");
                        if (parameters[i] is string)
                        {
                            builder.Append("\"");
                            builder.Append(parameters[i]);
                            builder.Append("\"");
                        }
                        else if (parameters[i] is Team t)
                        {
                            builder.Append(t.TeamName);
                        }
                        else if (parameters[i] is PlayerId p)
                        {
                            builder.Append(p.LongId);
                        }
                        else if (parameters[i] is RadialMenu.RadialSubMenu r)
                        {
                            builder.Append($"{(r.Parent == null ? "" : $"{r.Parent.Name}/")}{r.Name}");
                        }
                        else
                        {
                            builder.Append(parameters[i]);
                        }

                        ++i;
                    }
                }

                builder.Append(")");
                builder.Append("\n");
            }
            else
            {
                builder.Append("()");
                builder.Append("\n");
            }

            Dump(builder.ToString(), @"D:\Windows User\Desktop\Fusion5vs5GamemodeDump.txt");
            builder.Clear();
        }
        catch (Exception e)
        {
            builder.Append("\n");
            StackFrame frame = new StackFrame(1);
            MethodBase method = frame.GetMethod();
            if (method != null)
            {
                string name = $"{method.DeclaringType?.FullName}.{method.Name}";
                MelonLogger.Warning($"Exception during Log() after {name}: {e}");
            }
            else
            {
                MelonLogger.Warning($"Exception during Log(): {e}");
            }
        }
#endif
    }

    public static void Dump(string content, string path)
    {
#if DEBUG
        string filePath = path;
        try
        {
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.Write(content);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.Write(content);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not dump stack trace: {ex.Message}");
        }
#endif
    }
}
