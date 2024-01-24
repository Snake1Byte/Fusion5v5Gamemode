﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using BoneLib.BoneMenu.Elements;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Server;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Bonelab;
using SLZ.Marrow.SceneStreaming;
using TMPro;
using UnityEngine;
using static Fusion5vs5Gamemode.Commons;
using static Fusion5vs5Gamemode.Fusion5vs5CustomModule;

namespace Fusion5vs5Gamemode
{
    public class Fusion5vs5Gamemode : Gamemode
    {
        public override string GamemodeCategory => "Snake1Byte's Gamemodes";

        public override string GamemodeName => "5 vs 5";

        public override bool PreventNewJoins => EnableLateJoining;

        public override bool AutoHolsterOnDeath => false;
        public override bool DisableManualUnragdoll => true;
        public override bool AutoStopOnSceneLoad => false;

        // Avatars
        private string _DefaultAvatar = BoneLib.CommonBarcodes.Avatars.FordBL;

        // Debug
        public override bool DisableDevTools => !_Debug;
        public override bool DisableSpawnGun => !_Debug;

        private float _DebugTextUpdateTimePassed = 0;
        private GameObject _DebugText = null;
        private bool _Debug = true;

        // Music 
        public override bool MusicEnabled => _EnableMusic;
        private bool _EnableMusic = true;

        // UI
        private Timer _UITimer;
        private int currentTimeLimit;
        private int elapsedTime = 0;

        // Internal
        public static Fusion5vs5Gamemode Instance { get; private set; }
        public Server.Server Server { get; private set; }
        private Fusion5vs5GamemodeTeams _DefendingTeam;
        private Fusion5vs5GamemodeTeams _LocalTeam;

        private MenuCategory _Menu;
        private FunctionElement _DefendingTeamSelection;
        private FunctionElement _AttackingTeamSelection;
        private FunctionElement _JoinSpectatorSelection;
        private IntElement _MaxRoundsSetting;
        private BoolElement _EnableHalfTimeSetting;
        private BoolElement _EnableLateJoiningSetting;
        private BoolElement _AllowAvatarChangingSetting;

        // Settings
        public int MaxRounds { get; private set; } = 15;
        public bool HalfTimeEnabled { get; private set; } = true;
        public bool EnableLateJoining { get; private set; } = true;
        public bool AllowAvatarChanging { get; private set; } = true;

        public Dictionary<GameStates, int> TimeLimits { get; } = new Dictionary<GameStates, int>();

        public override void OnBoneMenuCreated(MenuCategory category)
        {
            Log(category);
            base.OnBoneMenuCreated(category);

            _Menu = category;

            //TODO Add custom settings for this gamemode 
            _DefendingTeamSelection =
                _Menu.CreateFunctionElement($"Join Defending Team", Color.white, RequestJoinDefenders);

            _AttackingTeamSelection =
                _Menu.CreateFunctionElement("Join Attacking Team", Color.white, RequestJoinAttackers);

            _JoinSpectatorSelection = _Menu.CreateFunctionElement("Join Spectators", Color.white, RequestJoinSpectator);

            _MaxRoundsSetting = category.CreateIntElement("Maximum rounds", Color.white, MaxRounds, 1, 1, 1000000,
                i =>
                {
                    if (IsActive())
                        _MaxRoundsSetting.SetValue(MaxRounds);
                    else
                    {
                        int maxRounds = _MaxRoundsSetting.GetValue();
                        if (maxRounds % 2 == 1 && maxRounds > MaxRounds)
                        {
                            _MaxRoundsSetting.SetValue(maxRounds + 1);
                        }
                        else if (maxRounds % 2 == 1 && maxRounds < MaxRounds)
                        {
                            _MaxRoundsSetting.SetValue(maxRounds - 1);
                        }

                        MaxRounds = _MaxRoundsSetting.GetValue();
                    }
                });

            _EnableHalfTimeSetting = category.CreateBoolElement("Enable Half-Time", Color.white, HalfTimeEnabled, b =>
            {
                if (IsActive())
                {
                    _EnableHalfTimeSetting.SetValue(HalfTimeEnabled);
                }
                else if (b)
                {
                    _MaxRoundsSetting.SetIncrement(2);
                    int maxRounds = _MaxRoundsSetting.GetValue();
                    if (maxRounds % 2 == 1)
                    {
                        _MaxRoundsSetting.SetValue(maxRounds + 1);
                    }

                    MaxRounds = _MaxRoundsSetting.GetValue();
                }
                else
                {
                    _MaxRoundsSetting.SetIncrement(1);
                }
            });

            _EnableLateJoiningSetting = category.CreateBoolElement("Enable late joining", Color.white,
                EnableLateJoining, b =>
                {
                    if (IsActive())
                        _EnableLateJoiningSetting.SetValue(EnableLateJoining);
                    else
                        EnableLateJoining = b;
                });

            _AllowAvatarChangingSetting = category.CreateBoolElement("Allow avatar changing", Color.white,
                AllowAvatarChanging, b =>
                {
                    if (IsActive())
                        _AllowAvatarChangingSetting.SetValue(AllowAvatarChanging);
                });

            category.CreateBoolElement("Enable round music", Color.white, _EnableMusic, b => _EnableMusic = b);

            category.CreateBoolElement("Debug", Color.white, _Debug, e => _Debug = e);

            // Only show these while the game is running, until I add a better way to switch teams
            category.Elements.RemoveInstance(_DefendingTeamSelection);
            category.Elements.RemoveInstance(_AttackingTeamSelection);
            category.Elements.RemoveInstance(_JoinSpectatorSelection);
        }

        public override void OnGamemodeRegistered()
        {
            Log();
            base.OnGamemodeRegistered();
            MelonLogger.Msg("5vs5 Mode: OnGameModeRegistered Called.");
            Instance = this;

            TimeLimits.Add(GameStates.Warmup, 60);
            TimeLimits.Add(GameStates.BuyPhase, 15);
            TimeLimits.Add(GameStates.PlayPhase, 135);
            TimeLimits.Add(GameStates.RoundEndPhase, 10);
            TimeLimits.Add(GameStates.MatchHalfPhase, 15);
            TimeLimits.Add(GameStates.MatchEndPhase, 30);
        }

        public override void OnGamemodeUnregistered()
        {
            Log();
            base.OnGamemodeUnregistered();
            MelonLogger.Msg("5vs5 Mode: OnGameModeUnRegistered Called.");
            if (Instance == this)
            {
                Instance = null;
            }
        }

        protected override void OnStartGamemode()
        {
            Log();
            base.OnStartGamemode();
            MelonLogger.Msg("5vs5 Mode: OnStartGamemode Called.");
            _Menu.Elements.Insert(0, _JoinSpectatorSelection);
            _Menu.Elements.Insert(0, _DefendingTeamSelection);
            _Menu.Elements.Insert(0, _AttackingTeamSelection);
            _Menu.Elements.RemoveInstance(_EnableHalfTimeSetting);
            _Menu.Elements.RemoveInstance(_MaxRoundsSetting);

            Fusion5vs5GamemodeDescriptor descriptor = null;
            foreach (var firstComponent in Fusion5vs5GamemodeDescriptor.Cache.Components)
            {
                descriptor = firstComponent;
                break;
            }

            if (descriptor == null)
            {
                Notify("Map does not support game mode!", "The current map does not support the Fusion5vs5Gamemode.");
                StopGamemode();
                return;
            }

            _UITimer = new Timer();
            _UITimer.AutoReset = true;
            _UITimer.Interval = 1000;
            _UITimer.Elapsed += (a, b) => UpdateUITimer();

#pragma warning disable CS0472
            _DefendingTeam = descriptor.DefendingTeam == null
                ? Fusion5vs5GamemodeTeams.CounterTerrorists
                : descriptor.DefendingTeam;
#pragma warning restore CS0472
            _DefaultAvatar = descriptor.DefaultAvatar == null
                ? _DefaultAvatar
                : descriptor.DefaultAvatar._barcode.ToString();
            if (NetworkInfo.IsServer)
            {
                Server = new Server.Server(
                    new ServerOperationsImpl(this),
                    TimeLimits,
                    _DefendingTeam,
                    MaxRounds,
                    HalfTimeEnabled,
                    PreventNewJoins,
                    AllowAvatarChanging
                );
            }

            FusionPlayer.SetPlayerVitality(1.0f);
            SceneStreamer.Reload();
        }

        protected override void OnStopGamemode()
        {
            Log();
            base.OnStopGamemode();
            MelonLogger.Msg("5vs5 Mode: OnStopGamemode Called.");

            UnFreeze();
            
            if (NetworkInfo.IsServer && Server != null)
            {
                Server.Dispose();
            }

            _Menu.Elements.RemoveInstance(_JoinSpectatorSelection);
            _Menu.Elements.RemoveInstance(_DefendingTeamSelection);
            _Menu.Elements.RemoveInstance(_AttackingTeamSelection);
            _Menu.Elements.Insert(0, _EnableHalfTimeSetting);
            _Menu.Elements.Insert(0, _MaxRoundsSetting);

            FusionPlayer.ClearPlayerVitality();
            _DebugText = null;
            Dump();
        }

        protected override void OnMetadataChanged(string key, string value)
        {
            Log(key, value);
            base.OnMetadataChanged(key, value);

            if (_Debug)
            {
                MelonLogger.Msg($"5vs5: OnMetadataChanged called: {key} {value}");
                UpdateDebugText();
            }
            else if (key.StartsWith(Commons.Metadata.TeamNameKey))
            {
                string teamName = key.Split('.')[3];
                Fusion5vs5GamemodeTeams team = GetTeamFromValue(teamName);
                TeamRepresentation rep = new TeamRepresentation { Team = team, DisplayName = value };
                OnTeamNameChanged(rep);
            }
            else if (key.StartsWith(Commons.Metadata.TeamKey))
            {
                string _player = key.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                Fusion5vs5GamemodeTeams _team = GetTeamFromValue(value);
                TeamRepresentation team = GetTeamRepresentation(_team);
                OnTeamChanged(player, team);
            }
            else if (key.Equals(Commons.Metadata.RoundNumberKey))
            {
                int newScore = int.Parse(value);
                OnRoundNumberChanged(newScore);
            }
        }

        protected override void OnEventTriggered(string eventName)
        {
            Log(eventName);
            if (eventName.StartsWith(Events.PlayerKilledPlayer))
            {
                string _killer = eventName.Split('.')[1];
                string _killed = eventName.Split('.')[2];
                PlayerId killer = GetPlayerFromValue(_killer);
                PlayerId killed = GetPlayerFromValue(_killed);
                killer.TryGetDisplayName(out string killerName);
                killed.TryGetDisplayName(out string killedName);
                SDKIntegration.InvokePlayerKilledAnotherPlayer(killerName, killedName, killed.IsSelf);
            }
            else if (eventName.StartsWith(Events.PlayerSuicide))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                player.TryGetDisplayName(out string playerName);
                SDKIntegration.InvokePlayerSuicide(playerName, player.IsSelf);
            }
            else if (eventName.StartsWith(Events.KillPlayer))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);

                KillPlayer(player);
            }
            else if (eventName.StartsWith(Events.RevivePlayer))
            {
                string _player = eventName.Split('.')[1];
                string _team = eventName.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                Fusion5vs5GamemodeTeams team = GetTeamFromValue(_team);

                RevivePlayer(player, team);
            }
            else if (eventName.StartsWith(Events.RespawnPlayer))
            {
                string _player = eventName.Split('.')[1];
                string _team = eventName.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                Fusion5vs5GamemodeTeams team = GetTeamFromValue(_team);

                RespawnPlayer(player, team);
            }
            else if (eventName.StartsWith(Events.SetSpectator))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);
                SetSpectator(player);
            }
            else if (eventName.StartsWith(Events.TeamWonRound))
            {
                Fusion5vs5GamemodeTeams _team = GetTeamFromValue(eventName.Split('.')[1]);
                TeamRepresentation team = GetTeamRepresentation(_team);
                OnTeamWonRound(team);
            }
            else if (eventName.StartsWith(Events.TeamWonGame))
            {
                Fusion5vs5GamemodeTeams _team = GetTeamFromValue(eventName.Split('.')[1]);
                TeamRepresentation team = GetTeamRepresentation(_team);
                OnTeamWonGame(team);
            }
            else if (eventName.Equals(Events.GameTie))
            {
                OnGameTie();
            }
            else if (eventName.Equals(Events.Fusion5vs5Started))
            {
                if (_Debug)
                {
                    if (FusionSceneManager.Level._barcode.Equals("Snek.csoffice.Level.Csoffice"))
                    {
                        _DebugText = GameObject.Find("debugText");
                    }
                }
            }
            else if (eventName.Equals(Events.Fusion5vs5Aborted))
            {
                StopGamemode();
            }
            else if (eventName.Equals(Events.Fusion5vs5Over))
            {
                StopGamemode();
            }
            else if (eventName.StartsWith(Events.NewGameState))
            {
                string _newState = eventName.Split('.')[1];
                GameStates newState = (GameStates)Enum.Parse(typeof(GameStates), _newState);
                OnStateChanged(newState);
            }
            else if (eventName.StartsWith(Events.PlayerLeft))
            {
                string _player = eventName.Split('.')[1];
                string _team = eventName.Split('.')[2];
                PlayerId player = GetPlayerFromValue(_player);
                Fusion5vs5GamemodeTeams team = GetTeamFromValue(_team);

                OnTeamRemoved(player, team);
            }
            else if (eventName.StartsWith(Events.PlayerSpectates))
            {
                string _player = eventName.Split('.')[1];
                PlayerId player = GetPlayerFromValue(_player);

                SetSpectator(player);
            }

            UpdateDebugText(eventName);
        }

        protected override void OnUpdate()
        {
            if (_Debug)
            {
                _DebugTextUpdateTimePassed += Time.deltaTime;
                if (_DebugTextUpdateTimePassed >= 1)
                {
                    _DebugTextUpdateTimePassed -= 1;
                    UpdateDebugText();
                }
            }
        }

        private void OnStateChanged(GameStates state)
        {
            MelonLogger.Msg($"New game state {state}.");

            _UITimer.Stop();
            elapsedTime = 0;
            currentTimeLimit = TimeLimits[state];
            _UITimer.Start();

            switch (state)
            {
                case GameStates.Warmup:
                    StartWarmupPhase();
                    break;
                case GameStates.BuyPhase:
                    StartBuyPhase();
                    break;
                case GameStates.PlayPhase:
                    StartPlayPhase();
                    break;
                case GameStates.RoundEndPhase:
                    StartRoundEndPhase();
                    break;
                case GameStates.MatchHalfPhase:
                    StartMatchHalfPhase();
                    break;
                case GameStates.MatchEndPhase:
                    StartMatchEndPhase();
                    break;
                default:
                    MelonLogger.Warning($"Could not execute the state {state}!");
                    break;
            }

            Log(state);
        }

        private void StartWarmupPhase()
        {
            Log();
            // TODO decide how to implement team switching
            Notify("Warmup begun", "Select a team from UI component");

            SDKIntegration.InvokeWarmupPhaseStarted();
        }

        private void StartBuyPhase()
        {
            Log();
            // TODO decide how to implement weapon buying
            Notify("Buy Phase", "Buy weapons from <UI component>");

            Freeze();
            
            SDKIntegration.InvokeBuyPhaseStarted();
        }

        private void StartPlayPhase()
        {
            Log();

            UnFreeze();

            if (_LocalTeam == _DefendingTeam)
            {
                // TODO change notification
                Notify("Round start", "Do defending team stuff...");
            }
            else
            {
                Notify("Round start", "Do attacking team stuff...");
            }

            SDKIntegration.InvokePlayPhaseStarted();
        }

        private void StartRoundEndPhase()
        {
            Log();

            SDKIntegration.InvokeRoundEndPhaseStarted();
        }

        private void StartMatchHalfPhase()
        {
            Log();

            Freeze();
            
            Fusion5vs5GamemodeTeams _team = _LocalTeam == Fusion5vs5GamemodeTeams.Terrorists
                ? Fusion5vs5GamemodeTeams.CounterTerrorists
                : Fusion5vs5GamemodeTeams.Terrorists;
            Notify("Switching sides", $"Switching to team {GetTeamDisplayName(_team)}.");

            OnSwapTeams();

            SDKIntegration.InvokeMatchHalfPhaseStarted();
        }

        private void StartMatchEndPhase()
        {
            Log();

            Freeze();

            SDKIntegration.InvokeMatchEndPhaseStarted();
        }

        private void RequestJoinDefenders()
        {
            Log();
            Fusion5vs5GamemodeTeams team = _DefendingTeam;
            RequestJoinTeam(team);
        }

        private void RequestJoinAttackers()
        {
            Log();
            Fusion5vs5GamemodeTeams team =
                _DefendingTeam == Fusion5vs5GamemodeTeams.Terrorists
                    ? Fusion5vs5GamemodeTeams.CounterTerrorists
                    : Fusion5vs5GamemodeTeams.Terrorists;
            RequestJoinTeam(team);
        }

        private void RequestJoinTeam(Fusion5vs5GamemodeTeams team)
        {
            Log(team);
            PlayerId player = PlayerIdManager.LocalId;
            string request = $"{ClientRequest.ChangeTeams}.{player?.LongId}.{team.ToString()}";
            RequestToServer(request);
        }

        private void RequestJoinSpectator()
        {
            Log();
            PlayerId player = PlayerIdManager.LocalId;
            string request = $"{ClientRequest.JoinSpectator}.{player?.LongId}";
            RequestToServer(request);
        }

        private void OnTeamChanged(PlayerId player, TeamRepresentation team)
        {
            Log(player, team);
            if (player.IsSelf)
            {
                _LocalTeam = team.Team;
                Notify($"Joined {team.DisplayName}", "");
            }

            // TODO Implement UI changes

            player.TryGetDisplayName(out string name);
            if (team.Team == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                SDKIntegration.InvokeCounterTerroristTeamJoined(name, player.IsSelf);
            }
            else
            {
                SDKIntegration.InvokeTerroristTeamJoined(name, player.IsSelf);
            }
        }

        private void OnTeamRemoved(PlayerId player, Fusion5vs5GamemodeTeams team)
        {
            Log(player, team);
            // TODO Implement UI changes
        }

        private void OnTeamNameChanged(TeamRepresentation rep)
        {
            Log(rep);
            // TODO Implement UI changes
        }

        private void OnTeamWonRound(TeamRepresentation team)
        {
            Log(team);
            // TODO Implement UI changes
            Notify("Round over.", $"Team {team.DisplayName} wins.");

            Metadata.TryGetValue(GetTeamScoreKey(team.Team), out string value);
            int totalScore = int.Parse(value);
            if (team.Team == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                SDKIntegration.InvokeCounterTerroristTeamScored(totalScore,
                    _LocalTeam == Fusion5vs5GamemodeTeams.CounterTerrorists);
            }
            else
            {
                SDKIntegration.InvokeTerroristTeamScored(totalScore, _LocalTeam == Fusion5vs5GamemodeTeams.Terrorists);
            }
        }

        private void OnTeamWonGame(TeamRepresentation team)
        {
            Log(team);
            Notify("Game over.", $"Team {team.DisplayName} wins.");
            // TODO implement UI
        }
        
        private void OnGameTie()
        {
            Log();
            Notify("Game over.", "Game tied.");
            // TODO implement UI
        }

        private void OnSwapTeams()
        {
            Log();
            // TODO Implement UI changes
        }

        private TeamRepresentation GetTeamRepresentation(Fusion5vs5GamemodeTeams team)
        {
            Log(team);
            return new TeamRepresentation
                { Team = team, DisplayName = GetTeamDisplayName(team) };
        }

        private string GetTeamNameKey(Fusion5vs5GamemodeTeams team)
        {
            Log(team);
            return $"{Commons.Metadata.TeamNameKey}.{team.ToString()}";
        }

        private string GetTeamDisplayName(Fusion5vs5GamemodeTeams team)
        {
            Log(team);
            Metadata.TryGetValue(GetTeamNameKey(team), out string displayName);
            return displayName;
        }

        private void OnRoundNumberChanged(int newScore)
        {
            Log(newScore);
            // TODO Implement UI changes

            SDKIntegration.InvokeNewRoundStarted(newScore);
        }

        private void RevivePlayer(PlayerId player, Fusion5vs5GamemodeTeams team)
        {
            Log(player, team);
            // TODO change avatar from spectator avatar, place player in spawn, give back interactability and visibility
        }

        private void KillPlayer(PlayerId player)
        {
            Log(player);
            // TODO spawn player's ragdoll where the player avatar is without killing player
            SetSpectator(player);
        }

        private void RespawnPlayer(PlayerId player, Fusion5vs5GamemodeTeams team)
        {
            Log(player, team);
            // TODO Simply teleport player to his team's spawn
        }

        private void SetSpectator(PlayerId player)
        {
            Log(player);
            // TODO change to spectator avatar, remove interactibility and visibility
        }
        
        private void Freeze()
        {
            // TODO Either set their agility avatar stat to 0 or use Harmony Patching to prevent player from walking
        }
        
        private void UnFreeze()
        {
            // TODO Undo all changes from Freeze()
        }

        private void Notify(string header, string body)
        {
            Log(header, body);
            FusionNotification notif = new FusionNotification()
            {
                title = header,
                showTitleOnPopup = true,
                message = body,
                isMenuItem = false,
                isPopup = true,
            };
            FusionNotifier.Send(notif);
        }

        private void UpdateUITimer()
        {
            ++elapsedTime;
        }

        private void UpdateDebugText()
        {
            if (_DebugText != null)
            {
                TextMeshProUGUI metadataText =
                    _DebugText.transform.Find("MetadataText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI playerListText =
                    _DebugText.transform.Find("PlayerListText").GetComponent<TextMeshProUGUI>();

                StringBuilder sb = new StringBuilder();
                sb.Append("Metadata:\n");
                foreach (var kvPair in Metadata)
                {
                    sb.Append(kvPair.Key + "\t\t");
                    sb.Append(kvPair.Value + "\n");
                }

                metadataText.text = sb.ToString();

                sb = new StringBuilder();
                sb.Append("Player List:");

                int timeLeft = currentTimeLimit - elapsedTime;
                sb.Append("\t");
                if (timeLeft >= 0)
                {
                    int minutes = timeLeft / 60;
                    int seconds = timeLeft - 60 * minutes;
                    string time = $"{minutes}:{seconds:00}";
                    sb.Append(time);
                }
                else
                {
                    sb.Append("0:00");
                }

                sb.Append("\n");
                foreach (var playerId in PlayerIdManager.PlayerIds)
                {
                    playerId.TryGetDisplayName(out string name);
                    sb.Append(name + "\t\t");
                    sb.Append(playerId.LongId + "\n");
                }

                playerListText.text = sb.ToString();
            }
        }

        private void UpdateDebugText(string eventTriggerName)
        {
            TextMeshProUGUI eventTriggerText =
                _DebugText.transform.Find("EventTriggerText").GetComponent<TextMeshProUGUI>();
            eventTriggerText.text = eventTriggerText.text + "\n" + eventTriggerName;
        }
    }
}