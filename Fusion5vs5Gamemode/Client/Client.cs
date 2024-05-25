using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using BoneLib.BoneMenu.Elements;
using Fusion5vs5Gamemode.Client.Combat;
using Fusion5vs5Gamemode.Client.UI;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Server;
using Fusion5vs5Gamemode.Shared;
using Fusion5vs5Gamemode.Shared.Modules;
using Fusion5vs5Gamemode.Utilities;
using Fusion5vs5Gamemode.Utilities.Extensions;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Bonelab;
using SLZ.Marrow.SceneStreaming;
using SLZ.Rig;
using SwipezGamemodeLib.Spawning;
using SwipezGamemodeLib.Utilities;
using TMPro;
using UnityEngine;
using static Fusion5vs5Gamemode.Shared.Commons;
using static Fusion5vs5Gamemode.Client.ModuleRequests;
using Object = UnityEngine.Object;

namespace Fusion5vs5Gamemode.Client;

public class Client : Gamemode
{
    public override string GamemodeCategory => "Snake1Byte";

    public override string GamemodeName => "5 vs 5";

    public override bool PreventNewJoins => EnableLateJoining;

    public override bool AutoHolsterOnDeath => false;
    public override bool DisableManualUnragdoll => true;
    public override bool AutoStopOnSceneLoad => false;

    // Debug
    public override bool DisableDevTools => !_Debug;
    public override bool DisableSpawnGun => !_Debug;

    private float _DebugTextUpdateTimePassed;
    private GameObject? _DebugText;

#if DEBUG
    private bool _Debug = true;
#else
        private bool _Debug = false;
#endif

    // Music 
    public override bool MusicEnabled => _EnableMusic;
    private bool _EnableMusic = true;

    // UI
    private Timer? _UITimer;
    private int _CurrentTimeLimit;
    private int _ElapsedTime;

    // Internal
    public static Client? Instance { get; private set; }
    private Fusion5vs5GamemodeDescriptor? _Descriptor;
    public Server.Server? Server { get; private set; }
    private Fusion5vs5GamemodeTeams? _DefendingTeam;
    private string? _CounterTerroristTeamName;
    private string? _TerroristTeamName;

    private float? _LocalPlayerVelocity;
    private string? _LastLocalAvatar;

    private MenuCategory? _Menu;
    private IntElement? _MaxRoundsSetting;
    private BoolElement? _EnableHalfTimeSetting;
    private BoolElement? _EnableLateJoiningSetting;
    private BoolElement? _AllowAvatarChangingSetting;

    private readonly object _FreezeLock = new();

    private bool _InsideTBuyZone;
    private bool _InsideCTBuyZone;
    private bool _IsBuyTime;

    // Settings
    public int MaxRounds { get; private set; } = 15;
    public bool HalfTimeEnabled { get; private set; } = true;
    public bool EnableLateJoining { get; private set; } = true;
    public bool AllowAvatarChanging { get; private set; } = true;

    public Dictionary<GameStates, int> TimeLimits { get; } = new();

    public override void OnBoneMenuCreated(MenuCategory category)
    {
        Log(category);

        base.OnBoneMenuCreated(category);

        _Menu = category;

        _MaxRoundsSetting = category.CreateIntElement("Maximum rounds", Color.white, MaxRounds, 1, 1, 1000000,
            i =>
            {
                if (_MaxRoundsSetting == null) return;
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
            if (_EnableHalfTimeSetting == null) return;
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
                if (_EnableLateJoiningSetting == null) return;
                if (IsActive())
                    _EnableLateJoiningSetting.SetValue(EnableLateJoining);
                else
                    EnableLateJoining = b;
            });

        _AllowAvatarChangingSetting = category.CreateBoolElement("Allow avatar changing", Color.white,
            AllowAvatarChanging, b =>
            {
                if (_AllowAvatarChangingSetting == null) return;
                if (IsActive())
                    _AllowAvatarChangingSetting.SetValue(b);
            });

        category.CreateBoolElement("Enable round music", Color.white, _EnableMusic, b => _EnableMusic = b);

#if DEBUG
        category.CreateBoolElement("Debug", Color.white, _Debug, e => _Debug = e);
#endif
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
        _Metadata = Metadata;
        _Menu!.Elements.RemoveInstance(_EnableHalfTimeSetting);
        _Menu.Elements.RemoveInstance(_MaxRoundsSetting);

        Fusion5vs5GamemodeDescriptor? descriptor = Fusion5vs5GamemodeDescriptor.Cache.Components.FirstOrDefault();

        if (descriptor == null)
        {
            Notify("Map does not support game mode!",
                "The current map does not have a Fusion5vs5GamemodeDescriptor script attached to it.",
                10f);
            StopGamemode();
            return;
        }

        _Descriptor = descriptor;

        Collider counterTerroristBuyZoneGo = _Descriptor.CounterTerroristBuyZone;
        Collider terroristBuyZoneGo = _Descriptor.TerroristBuyZone;

        if (counterTerroristBuyZoneGo == null || terroristBuyZoneGo == null)
        {
            Notify("Map does not support game mode!", "The current map does not have a buy zone for each team.",
                10f);
            StopGamemode();
            return;
        }

        if (_Descriptor.CounterTerroristSpawnPoints.Count < 5 || _Descriptor.CounterTerroristSpawnPoints.Count < 5)
        {
            Notify("Map does not support game mode!",
                "The current map does not have a minimum of 10 Spawn Points for 5vs5.",
                10f);
            StopGamemode();
            return;
        }

        _UITimer = new Timer();
        _UITimer.AutoReset = true;
        _UITimer.Interval = 1000;
        _UITimer.Elapsed += (_, _) => UpdateUITimer();

#pragma warning disable CS0472
        if (_Descriptor.DefendingTeam == null)
#pragma warning restore CS0472
        {
            _Descriptor.DefendingTeam = Fusion5vs5GamemodeDescriptor.Defaults.DefendingTeam;
        }

        _DefendingTeam = _Descriptor.DefendingTeam;

        List<SerializedTransform> counterTerroristSpawnPoints = new List<SerializedTransform>();
        List<SerializedTransform> terroristSpawnPoints = new List<SerializedTransform>();
        foreach (var spawnPoint in _Descriptor.CounterTerroristSpawnPoints)
        {
            SerializedTransform t = new SerializedTransform
            {
                position = spawnPoint.position,
                rotation = spawnPoint.rotation
            };
            counterTerroristSpawnPoints.Add(t);
        }

        foreach (var spawnPoint in _Descriptor.TerroristSpawnPoints)
        {
            SerializedTransform t = new SerializedTransform
            {
                position = spawnPoint.position,
                rotation = spawnPoint.rotation
            };
            terroristSpawnPoints.Add(t);
        }

        if (NetworkInfo.IsServer)
        {
            Server = new Server.Server(
                new FusionServerOperationsImpl(this),
                _DefendingTeam.Value,
                counterTerroristSpawnPoints,
                terroristSpawnPoints,
                MaxRounds,
                HalfTimeEnabled,
                PreventNewJoins,
                AllowAvatarChanging,
                TimeLimits
            );
        }

        _LastLocalAvatar = RigData.GetAvatarBarcode();

        FusionPlayer.SetPlayerVitality(1.0f);

        SceneStreamer.Reload();
    }

    protected override void OnStopGamemode()
    {
        Log();

        base.OnStopGamemode();
        MelonLogger.Msg("5vs5 Mode: OnStopGamemode Called.");

        FusionPlayerExtended.worldInteractable = true;
        FusionPlayerExtended.canSendDamage = true;
        FusionPlayer.ClearAvatarOverride();
        RigManager rm = RigData.RigReferences.RigManager;
        rm.SwapAvatarCrate(_LastLocalAvatar, true);
        UnFreeze();
        _IsBuyTime = false;

        if (NetworkInfo.IsServer && Server != null)
        {
            Server.Dispose();
        }

        _UITimer?.Dispose();

        _Menu?.Elements.Insert(0, _EnableHalfTimeSetting);
        _Menu?.Elements.Insert(0, _MaxRoundsSetting);

        try
        {
            if (_Descriptor != null)
            {
                Object.Destroy(_Descriptor.TerroristBuyZone.gameObject.GetComponent<TriggerLasers>());
                Object.Destroy(_Descriptor.CounterTerroristBuyZone.gameObject.GetComponent<TriggerLasers>());
            }
        }
        catch
        {
            // ignored
        }

        TriggerLasersEvents.OnTriggerEntered -= OnTriggerEntered;
        TriggerLasersEvents.OnTriggerExited -= OnTriggerExited;

        BuyMenu.RemoveBuyMenu();
        BuyMenu.OnBuyMenuItemClicked -= OnBuyMenuItemClicked;

        TeamSelectionMenu.RemoveTeamsMenu();
        TeamSelectionMenu.OnDefendersSelected -= RequestJoinDefenders;
        TeamSelectionMenu.OnAttackersSelected -= RequestJoinAttackers;
        TeamSelectionMenu.OnSpectatorsSelected -= RequestJoinSpectator;

        FusionPlayer.ClearPlayerVitality();

        _Descriptor = default;
        _CounterTerroristTeamName = default;
        _TerroristTeamName = default;
        _LastLocalAvatar = default;
        lock (_FreezeLock)
        {
            _LocalPlayerVelocity = default;
        }

        _DebugText = default;
    }

    private void OnFusion5vs5Started()
    {
        Log();

        SetSpectator();

        if (_Debug)
        {
            if (FusionSceneManager.Level._barcode.Equals("Snek.csoffice.Level.Csoffice"))
            {
                _DebugText = GameObject.Find("debugText");
            }
        }

        foreach (var firstComponent in Fusion5vs5GamemodeDescriptor.Cache.Components)
        {
            _Descriptor = firstComponent;
            break;
        }

        if (String.IsNullOrEmpty(_Descriptor!.CounterTerroristTeamName))
        {
            _Descriptor.CounterTerroristTeamName = Fusion5vs5GamemodeDescriptor.Defaults.CounterTerroristTeamName;
        }

        if (String.IsNullOrEmpty(_Descriptor.TerroristTeamName))
        {
            _Descriptor.TerroristTeamName = Fusion5vs5GamemodeDescriptor.Defaults.TerroristTeamName;
        }

        if (_Descriptor.DefaultAvatar == null)
        {
            _Descriptor.DefaultAvatar = Fusion5vs5GamemodeDescriptor.Defaults.DefaultAvatar;
        }

        _CounterTerroristTeamName = _Descriptor.CounterTerroristTeamName;
        _TerroristTeamName = _Descriptor.TerroristTeamName;

        TriggerLasers ctBuyZoneTrigger =
            _Descriptor.CounterTerroristBuyZone.gameObject.AddComponent<TriggerLasers>();
        ctBuyZoneTrigger.LayerFilter = 134217728;
        ctBuyZoneTrigger.onlyTriggerOnPlayer = true;

        TriggerLasers tbuyZoneTrigger = _Descriptor.TerroristBuyZone.gameObject.AddComponent<TriggerLasers>();
        tbuyZoneTrigger.LayerFilter = 134217728;
        tbuyZoneTrigger.onlyTriggerOnPlayer = true;

        TriggerLasersEvents.OnTriggerEntered += OnTriggerEntered;
        TriggerLasersEvents.OnTriggerExited += OnTriggerExited;

        BuyMenu.OnBuyMenuItemClicked += OnBuyMenuItemClicked;

        TeamSelectionMenu.AddTeamsMenu();
        TeamSelectionMenu.OnDefendersSelected += RequestJoinDefenders;
        TeamSelectionMenu.OnAttackersSelected += RequestJoinAttackers;
        TeamSelectionMenu.OnSpectatorsSelected += RequestJoinSpectator;

        Utilities.Resources.Initialize(new FusionSpawning(0));
    }

    protected override void OnMetadataChanged(string key, string value)
    {
        Log(key, value);

        base.OnMetadataChanged(key, value);

#if DEBUG
        MelonLogger.Msg($"OnMetadataChanged(): {key} {value}");
        UpdateDebugText();
#endif

        if (key.StartsWith(Commons.Metadata.TeamKey))
        {
            string playerRaw = key.Split('.')[2];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            Fusion5vs5GamemodeTeams? teamFromValue = GetTeamFromValue(value);
            if (teamFromValue == null) return;
            TeamRepresentation team = GetTeamRepresentation(teamFromValue.Value);
            OnTeamChanged(player, team);
        }
        else if (key.Equals(Commons.Metadata.RoundNumberKey))
        {
            int newScore = int.Parse(value);
            OnRoundNumberChanged(newScore);
        }
        else if (key.StartsWith(Commons.Metadata.GameStateKey))
        {
            GameStates? state = GetGameStateFromValue(value);
            if (state == null) return;
            OnStateChanged(state.Value);
        }
        else if (key.StartsWith(Commons.Metadata.SpawnPointKey))
        {
            string playerRaw = key.Split('.')[2];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            SerializedTransform? transform = GetSpawnPointFromValue(value);
            if (transform == null) return;
            SetFusionSpawnPoint(player, transform.position,
                transform.rotation.eulerAngles);
        }
        else if (key.StartsWith(Commons.Metadata.PlayerFrozenKey))
        {
            string playerRaw = key.Split('.')[2];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            if (player.IsSelf)
            {
                bool frozen = bool.Parse(value);
                if (frozen)
                {
                    Freeze(true);
                }
                else
                {
                    UnFreeze(true);
                }
            }
        }
    }

    protected override void OnMetadataRemoved(string key)
    {
        Log(key);

        if (key.StartsWith(Commons.Metadata.TeamKey))
        {
            string playerRaw = key.Split('.')[2];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            OnPlayerJoinedSpectators(player);
        }
    }

    protected override void OnEventTriggered(string eventName)
    {
        Log(eventName);

        if (eventName.StartsWith(Events.PlayerKilledPlayer))
        {
            string killerRaw = eventName.Split('.')[1];
            string killedRaw = eventName.Split('.')[2];
            PlayerId? killer = GetPlayerFromValue(killerRaw);
            PlayerId? killed = GetPlayerFromValue(killedRaw);
            if (killer == null || killed == null) return;
            killer.TryGetDisplayName(out string killerName);
            killed.TryGetDisplayName(out string killedName);
            if (killed.IsSelf && GetGameState() != GameStates.Warmup)
            {
                RigManager rm = RigData.RigReferences.RigManager;
                SetFusionSpawnPoint(rm.physicsRig.m_pelvis);
            }

            SDKIntegration.InvokePlayerKilledAnotherPlayer(killerName, killedName, killed.IsSelf);
        }
        else if (eventName.StartsWith(Events.PlayerSuicide))
        {
            string playerRaw = eventName.Split('.')[1];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            player.TryGetDisplayName(out string playerName);
            if (player.IsSelf && GetGameState() != GameStates.Warmup)
            {
                RigManager rm = RigData.RigReferences.RigManager;
                SetFusionSpawnPoint(rm.physicsRig.m_pelvis);
            }

            SDKIntegration.InvokePlayerSuicide(playerName, player.IsSelf);
        }
        else if (eventName.StartsWith(Events.KillPlayer))
        {
            string playerRaw = eventName.Split('.')[1];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;

            if (player.IsSelf)
            {
                Kill();
            }
        }
        else if (eventName.StartsWith(Events.RevivePlayer))
        {
            string playerRaw = eventName.Split('.')[1];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            if (player.IsSelf)
            {
                Revive();
            }
        }
        else if (eventName.StartsWith(Events.ReviveAndFreezePlayer))
        {
            string playerRaw = eventName.Split('.')[1];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            if (player.IsSelf)
            {
                ReviveAndFreeze();
            }
        }
        else if (eventName.StartsWith(Events.RespawnPlayer))
        {
            string playerRaw = eventName.Split('.')[1];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            if (player.IsSelf)
            {
                Respawn();
            }
        }
        else if (eventName.StartsWith(Events.SetSpectator))
        {
            string playerRaw = eventName.Split('.')[1];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            if (player.IsSelf)
            {
                SetSpectator();
            }
        }
        else if (eventName.StartsWith(Events.TeamWonRound))
        {
            Fusion5vs5GamemodeTeams? teamFromValue = GetTeamFromValue(eventName.Split('.')[1]);
            if (teamFromValue == null) return;
            TeamRepresentation team = GetTeamRepresentation(teamFromValue.Value);
            OnTeamWonRound(team);
        }
        else if (eventName.StartsWith(Events.TeamWonGame))
        {
            Fusion5vs5GamemodeTeams? teamFromValue = GetTeamFromValue(eventName.Split('.')[1]);
            if (teamFromValue == null) return;
            TeamRepresentation team = GetTeamRepresentation(teamFromValue.Value);
            OnTeamWonGame(team);
        }
        else if (eventName.Equals(Events.GameTie))
        {
            OnGameTie();
        }
        else if (eventName.Equals(Events.Fusion5vs5Started))
        {
            MelonCoroutines.Start(CoRunUponCondition(OnFusion5vs5Started, () => SceneStreamer.Session.Status == StreamStatus.DONE));
        }
        else if (eventName.Equals(Events.Fusion5vs5Aborted))
        {
            StopGamemode();
        }
        else if (eventName.Equals(Events.Fusion5vs5Over))
        {
            StopGamemode();
        }
        else if (eventName.StartsWith(Events.PlayerLeft))
        {
            string playerRaw = eventName.Split('.')[1];
            PlayerId? player = GetPlayerFromValue(playerRaw);
            if (player == null) return;
            OnPlayerLeft(player);
        }
        else if (eventName.Equals(Events.BuyTimeOver))
        {
            _IsBuyTime = false;
            BuyMenu.RemoveBuyMenu();
        }
        else if (eventName.Equals(Events.BuyTimeStart))
        {
            _IsBuyTime = true;
            if (IsInsideBuyZone())
            {
                BuyMenu.AddBuyMenu();
            }
        }
        else if (eventName.StartsWith(Events.ItemBought))
        {
            string[] info = eventName.Split('.');
            PlayerId? player = GetPlayerFromValue(info[1]);
            if (player == null) return;
            string barcode = string.Join(".", info.Skip(2));
            BuyMenuSpawning.PlayerBoughtItem(player, barcode);
        }
        else if (eventName.StartsWith(Events.PlayerJoined))
        {
            string[] info = eventName.Split('.');
            PlayerId? player = GetPlayerFromValue(info[1]);
            if (player == null) return;
            if (player.IsSelf)
            {
                MelonCoroutines.Start(CoRunUponCondition(() =>
                {
                    OnStartGamemode();
                    OnFusion5vs5Started();

                    GameStates? currentGameState = GetGameState();
                    if (currentGameState == null) return;
                    TimeLimits.TryGetValue(currentGameState.Value, out _CurrentTimeLimit);
                    // TODO set _ElapsedTime
                }, () => SceneStreamer.Session.Status == StreamStatus.DONE));
            }
        }

        UpdateDebugText(eventName);
    }

    protected override void OnUpdate()
    {
#if DEBUG
        if (_Debug)
        {
            _DebugTextUpdateTimePassed += Time.deltaTime;
            if (_DebugTextUpdateTimePassed >= 10)
            {
                _DebugTextUpdateTimePassed -= 10;
                UpdateDebugText();
            }
        }
#endif
    }

    private void OnStateChanged(GameStates state)
    {
        Log(GetGameState()!);

        MelonLogger.Msg($"New game state {state}.");

        _UITimer?.Stop();
        _ElapsedTime = 0;
        _CurrentTimeLimit = TimeLimits[state];
        _UITimer?.Start();

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
    }

    private void StartWarmupPhase()
    {
        Log();
        // TODO decide how to implement team switching
        Notify("Warmup begun", "You're a spectator. Select a team from UI component", 10f);

        SDKIntegration.InvokeWarmupPhaseStarted();
    }

    private void StartBuyPhase()
    {
        Log();
        // TODO decide how to implement weapon buying
        Notify("Buy Phase", "Buy weapons from <UI component>");

        SDKIntegration.InvokeBuyPhaseStarted();
    }

    private void StartPlayPhase()
    {
        Log();

        Fusion5vs5GamemodeTeams? localTeam = GetTeam(PlayerIdManager.LocalId);

        if (localTeam == null)
        {
            Notify("Round start", "You are spectating.");
        }
        else if (localTeam == _DefendingTeam)
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

        Fusion5vs5GamemodeTeams? localTeam = GetTeam(PlayerIdManager.LocalId);
        if (localTeam == null)
        {
            Notify("Switching sides", "");
        }
        else
        {
            Fusion5vs5GamemodeTeams team = localTeam.Value == Fusion5vs5GamemodeTeams.Terrorists
                ? Fusion5vs5GamemodeTeams.CounterTerrorists
                : Fusion5vs5GamemodeTeams.Terrorists;
            Notify("Switching sides", $"Switching to team {GetTeamDisplayName(team)}.");
        }

        OnSwapTeams();

        SDKIntegration.InvokeMatchHalfPhaseStarted();
    }

    private void StartMatchEndPhase()
    {
        Log();

        SDKIntegration.InvokeMatchEndPhaseStarted();
    }

    private void RequestJoinDefenders()
    {
        Log();

        if (_DefendingTeam != null)
        {
            RequestJoinTeam(_DefendingTeam.Value);
        }
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
        GenericRequestToServer(request);
    }

    private void RequestJoinSpectator()
    {
        Log();

        PlayerId player = PlayerIdManager.LocalId;
        string request = $"{ClientRequest.JoinSpectator}.{player?.LongId}";
        GenericRequestToServer(request);
    }

    private void OnTeamChanged(PlayerId player, TeamRepresentation team)
    {
        Log(player, team);

        if (player.IsSelf)
        {
            if (IsInsideBuyZone())
            {
                OnBuyZoneEntered();
            }
            else
            {
                OnBuyZoneExited();
            }

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

    private void OnPlayerLeft(PlayerId player)
    {
        Log(player);

        GameObject go = GameObject.Find($"Fusion 5vs5 Spawn Point for {player.LongId}");
        if (go != null) Object.Destroy(go);
        // TODO Implement UI changes
    }

    private void OnPlayerJoinedSpectators(PlayerId player)
    {
        Log(player);

        // TODO Implement UI changes
    }


    private void OnTeamWonRound(TeamRepresentation team)
    {
        Log(team);

        // TODO Implement UI changes
        Notify("Round over.", $"Team {team.DisplayName} wins.");

        Metadata.TryGetValue(GetTeamScoreKey(team.Team), out string value);
        Fusion5vs5GamemodeTeams? localTeam = GetTeam(PlayerIdManager.LocalId);
        if (localTeam == null) return;
        int totalScore = int.Parse(value);
        if (team.Team == Fusion5vs5GamemodeTeams.CounterTerrorists)
        {
            SDKIntegration.InvokeCounterTerroristTeamScored(totalScore,
                localTeam.Value == Fusion5vs5GamemodeTeams.CounterTerrorists);
        }
        else
        {
            SDKIntegration.InvokeTerroristTeamScored(totalScore, localTeam.Value == Fusion5vs5GamemodeTeams.Terrorists);
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

    private void OnRoundNumberChanged(int newScore)
    {
        Log(newScore);

        // TODO Implement UI changes

        SDKIntegration.InvokeNewRoundStarted(newScore);
    }

    private TeamRepresentation GetTeamRepresentation(Fusion5vs5GamemodeTeams team)
    {
        Log(team);

        return new TeamRepresentation
            { Team = team, DisplayName = GetTeamDisplayName(team) ?? "None" };
    }

    private string? GetTeamDisplayName(Fusion5vs5GamemodeTeams team)
    {
        Log(team);

        if (team == Fusion5vs5GamemodeTeams.Terrorists)
        {
            return _TerroristTeamName;
        }

        return _CounterTerroristTeamName;
    }

    /// <summary>
    /// Spawns a ragdoll of the local player's avatar where they stand, without actually subtracting their score and
    /// enters the player into the spectating avatar, where they then are taken away their visibility and
    /// interactability, as described in <see cref="SetSpectator"/>. Typically called when player changes their team
    /// mid-round, outside of the buy phase or warmup phase.
    /// </summary>
    private void Kill()
    {
        Log();

        SetSpectator();
        SpawnRagdoll();
    }

    /// <summary>
    /// Used to respawn the local dead player inside of their spawn point. Usually called once the BuyPhase begins.
    /// Dead players leave their spectator avatar, get placed inside the their spawn point and are given back
    /// their visibility to other players as well as their interactability with the world.
    /// </summary>
    private void Revive()
    {
        Log();

        // TODO give back interactability and visibility
        FusionPlayerExtended.worldInteractable = true;
        FusionPlayerExtended.canSendDamage = true;
        FusionPlayer.ClearAvatarOverride();
        RigManager rm = RigData.RigReferences.RigManager;
        rm.SwapAvatarCrate(_LastLocalAvatar, true);
        Respawn();
    }

    private void ReviveAndFreeze()
    {
        Log();

        // TODO give back interactability and visibility
        FusionPlayerExtended.worldInteractable = true;
        FusionPlayerExtended.canSendDamage = true;
        FusionPlayer.ClearAvatarOverride();
        RigManager rm = RigData.RigReferences.RigManager;
        rm.SwapAvatarCrate(_LastLocalAvatar, true, (Action<bool>)(_ =>
        {
            Freeze(true);
            Respawn();
        }));
    }

    /// <summary>
    /// Enters the local player into the spectator avatar, where they're then taken away of their visibility from
    /// other players and their interactability with the world. Typically called by other methods such as
    /// <see cref="Kill"/>, or after the player has left any team and joined the spectators.
    /// </summary>
    private void SetSpectator()
    {
        Log();

        // TODO remove interactability and visibility
        FusionPlayerExtended.worldInteractable = false;
        FusionPlayerExtended.canSendDamage = false;
        string avatarBarcode = RigData.GetAvatarBarcode();
        if (!avatarBarcode.Equals(SpectatorAvatar))
        {
            _LastLocalAvatar = avatarBarcode;
        }
#if DEBUG
        MelonLogger.Msg($"_LastLocalAvatar changed to {_LastLocalAvatar}.");
#endif
        FusionPlayer.SetAvatarOverride(SpectatorAvatar);
    }

    /// <summary>
    /// Simply teleports the local player into their team's spawnpoint. Typically used to re-position all players once
    /// the round ends and buy phase starts. Dead players however wont be respawned, they will be revived with
    /// <see cref="Revive"/>. Using <see cref="Respawn"/> with dead players leads to undefined behaviour.
    /// </summary>
    private void Respawn()
    {
        Log();

        SerializedTransform? localSpawnPoint = GetSpawnPoint(PlayerIdManager.LocalId);
        if (localSpawnPoint == null) return;
        FusionPlayer.Teleport(localSpawnPoint.position,
            localSpawnPoint.rotation.eulerAngles);
    }

    /// <summary>
    /// Uses the SwipezGamemodeLib to spawn a ragdoll of the local player's avatar where they stand and removes the
    /// ragdoll after three seconds.
    /// </summary>
    private void SpawnRagdoll()
    {
        Log();

        RigManager rm = RigData.RigReferences.RigManager;
        Transform transform = rm.physicsRig.m_pelvis;
        SpawnManager.SpawnRagdoll(_LastLocalAvatar, transform.position, transform.rotation,
            rigManager =>
            {
                Timer despawnTimer = new Timer();
                despawnTimer.Interval = 5000;
                despawnTimer.AutoReset = false;
                despawnTimer.Elapsed += (_, _) =>
                {
                    Object.Destroy(rigManager.gameObject);
                    despawnTimer.Dispose();
                };
                despawnTimer.Start();
            });
    }

    /// <summary>
    /// Uses LabFusion's API to set a new multiplayer spawn point using a <see cref="Transform"/>.
    /// </summary>
    /// <param name="transform"><see cref="Transform"/> object where the new spawn point will be</param>
    private void SetFusionSpawnPoint(Transform transform)
    {
        Log(transform);

        FusionPlayer.SetSpawnPoints(transform);
    }

    /// <summary>
    /// Uses LabFusion's API to set a new multiplayer spawn point using a <see cref="Vector3"/> as position and
    /// eulerAngles, spawns a new GameObject to get ahold of a <see cref="Transform"/> object and feeds that into
    /// <see cref="FusionPlayer.SetSpawnPoints"/>.
    /// </summary>
    /// <param name="pos"><see cref="Vector3"/> world space position of the new spawn point</param>
    /// <param name="rot"><see cref="Vector3"/> eulerAngles of the new spawn point</param>
    private void SetFusionSpawnPoint(PlayerId player, Vector3 pos, Vector3 rot)
    {
        Log(player, pos, rot);

        GameObject go = GameObject.Find($"Fusion 5vs5 Spawn Point for {player.LongId}");
        if (go == null)
        {
            go = new GameObject($"Fusion 5vs5 Spawn Point for {player.LongId}");
        }

        go.transform.position = pos;
        go.transform.localEulerAngles = rot;
        if (player.IsSelf) FusionPlayer.SetSpawnPoints(go.transform);
    }

    private void Freeze(bool force = false)
    {
        Log(force);

        lock (_FreezeLock)
        {
            bool? localPlayerFrozen = IsPlayerFrozen(PlayerIdManager.LocalId);
            if ((!localPlayerFrozen.HasValue || localPlayerFrozen.Value) && !force) return;

#if DEBUG
            MelonLogger.Msg(
                $"1: Current avatar on Freeze: {RigData.RigReferences.RigManager.AvatarCrate._barcode} with _LocalPlayerVelocity: {_LocalPlayerVelocity}");
#endif
            RemapRig rig = RigData.RigReferences.RigManager.remapHeptaRig;
            rig.jumpEnabled = false;
            _LocalPlayerVelocity = rig.maxVelocity == 0.001f ? _LocalPlayerVelocity : rig.maxVelocity;
            rig.maxVelocity = 0.001f;
#if DEBUG
            MelonLogger.Msg(
                $"2: Current avatar on Freeze: {RigData.RigReferences.RigManager.AvatarCrate._barcode} with _LocalPlayerVelocity: {_LocalPlayerVelocity}");
#endif
        }
    }

    private void UnFreeze(bool force = false)
    {
        Log(force);

        lock (_FreezeLock)
        {
            bool? localPlayerFrozen = IsPlayerFrozen(PlayerIdManager.LocalId);
            if ((!localPlayerFrozen.HasValue || !localPlayerFrozen.Value) && !force) return;
#if DEBUG
            MelonLogger.Msg(
                $"1: Current avatar on UnFreeze(): {RigData.RigReferences.RigManager.AvatarCrate._barcode} with _LocalPlayerVelocity: {_LocalPlayerVelocity}");
#endif
            if (_LocalPlayerVelocity == null)
            {
                RigManager rm = RigData.RigReferences.RigManager;
                rm.SwapAvatarCrate(rm._avatarCrate._barcode);
            }
            else
            {
                RemapRig rig = RigData.RigReferences.RigManager.remapHeptaRig;
                rig.maxVelocity = _LocalPlayerVelocity.Value;
                rig.jumpEnabled = true;
            }

#if DEBUG
            MelonLogger.Msg(
                $"2: Current avatar on UnFreeze(): {RigData.RigReferences.RigManager.AvatarCrate._barcode} with _LocalPlayerVelocity: {_LocalPlayerVelocity}");
#endif
        }
    }

    private void OnBuyMenuItemClicked(string barcode)
    {
        Log(barcode);

        GenericRequestToServer($"{ClientRequest.BuyItem}.{PlayerIdManager.LocalId.LongId}.{barcode}");
    }

    private bool IsInsideBuyZone()
    {
        Log();

        Fusion5vs5GamemodeTeams? localTeam = GetTeam(PlayerIdManager.LocalId);
        if (!localTeam.HasValue)
        {
            return false;
        }

        if (localTeam.Value == Fusion5vs5GamemodeTeams.Terrorists && _InsideTBuyZone)
        {
            return true;
        }

        if (localTeam.Value == Fusion5vs5GamemodeTeams.CounterTerrorists && _InsideCTBuyZone)
        {
            return true;
        }

        return false;
    }

    private void OnBuyZoneEntered()
    {
        Log();

        MelonLogger.Msg("Buy Zone entered.");
        GenericRequestToServer($"{ClientRequest.BuyZoneEntered}.{PlayerIdManager.LocalId.LongId}");
        if (_IsBuyTime)
        {
            BuyMenu.AddBuyMenu();
        }
    }

    private void OnBuyZoneExited()
    {
        Log();

        MelonLogger.Msg("Buy Zone exited.");
        BuyMenu.RemoveBuyMenu();
        GenericRequestToServer($"{ClientRequest.BuyZoneExited}.{PlayerIdManager.LocalId.LongId}");
    }

    private void OnTriggerEntered(TriggerLasers obj)
    {
        Log(obj);

        if (_Descriptor == null) return;
        TriggerLasers ctTrigger = _Descriptor.CounterTerroristBuyZone.gameObject.GetComponent<TriggerLasers>();
        TriggerLasers tTrigger = _Descriptor.TerroristBuyZone.gameObject.GetComponent<TriggerLasers>();
        if (ctTrigger != null && ctTrigger.GetInstanceID() == obj.GetInstanceID())
        {
            // Prevent a NullReferenceException inside of TriggerLasers component
            ctTrigger.obj_SpecificTrigger = ctTrigger.gameObject;
        }

        if (tTrigger != null && tTrigger.GetInstanceID() == obj.GetInstanceID())
        {
            tTrigger.obj_SpecificTrigger = tTrigger.gameObject;
        }

        Fusion5vs5GamemodeTeams? localTeam = GetTeam(PlayerIdManager.LocalId);
        if (ctTrigger != null && ctTrigger.GetInstanceID() == obj.GetInstanceID())
        {
            _InsideCTBuyZone = true;
            if (localTeam.HasValue && localTeam.Value == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                OnBuyZoneEntered();
            }
        }
        else if (tTrigger != null && tTrigger.GetInstanceID() == obj.GetInstanceID())
        {
            _InsideTBuyZone = true;
            if (localTeam.HasValue && localTeam.Value == Fusion5vs5GamemodeTeams.Terrorists)
            {
                tTrigger.obj_SpecificTrigger = _Descriptor.TerroristBuyZone.gameObject;
                OnBuyZoneEntered();
            }
        }
    }

    private void OnTriggerExited(TriggerLasers obj)
    {
        Log(obj);

        if (_Descriptor == null) return;
        TriggerLasers ctTrigger = _Descriptor.CounterTerroristBuyZone.gameObject.GetComponent<TriggerLasers>();
        TriggerLasers tTrigger = _Descriptor.TerroristBuyZone.gameObject.GetComponent<TriggerLasers>();
        Fusion5vs5GamemodeTeams? localTeam = GetTeam(PlayerIdManager.LocalId);
        if (ctTrigger != null && ctTrigger.GetInstanceID() == obj.GetInstanceID())
        {
            _InsideCTBuyZone = false;
            if (localTeam.HasValue && localTeam.Value == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                OnBuyZoneExited();
            }
        }
        else
        {
            if (tTrigger != null && tTrigger.GetInstanceID() == obj.GetInstanceID())
            {
                _InsideTBuyZone = false;
                if (localTeam.HasValue && localTeam.Value == Fusion5vs5GamemodeTeams.Terrorists)
                {
                    OnBuyZoneExited();
                }
            }
        }
    }

    private void Notify(string header, string body, float popupLength = 2f)
    {
        Log(header, body, popupLength);

        FusionNotification notif = new FusionNotification
        {
            title = header,
            showTitleOnPopup = true,
            message = body,
            isMenuItem = false,
            isPopup = true,
            popupLength = popupLength
        };
        FusionNotifier.Send(notif);
    }

    private void UpdateUITimer()
    {
        ++_ElapsedTime;
#if DEBUG
        UpdateDebugText();
#endif
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

            int timeLeft = _CurrentTimeLimit - _ElapsedTime - 1;
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
        if (_DebugText == null)
        {
            return;
        }

        TextMeshProUGUI eventTriggerText =
            _DebugText.transform.Find("EventTriggerText").GetComponent<TextMeshProUGUI>();
        eventTriggerText.text = eventTriggerText.text + "\n" + eventTriggerName;
    }
}
