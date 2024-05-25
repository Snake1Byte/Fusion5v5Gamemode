using System;
using System.Collections.Generic;
using Fusion5vs5Gamemode.SDK.Internal;
using SLZ.Marrow.Warehouse;
using UnityEngine;
#if MELONLOADER
using LabFusion.Utilities;
#endif

namespace Fusion5vs5Gamemode.SDK
{
#if UNITY_EDITOR
    [AddComponentMenu("Fusion 5vs5 Gamemode/REQUIRED/Fusion5vs5GamemodeDescriptor")]
    [DisallowMultipleComponent]
#endif
    public class Fusion5vs5GamemodeDescriptor : Fusion5vs5GamemodeBehaviour
    {
        public Fusion5vs5GamemodeTeams DefendingTeam = Fusion5vs5GamemodeTeams.CounterTerrorists;
        public string CounterTerroristTeamName = "Sabrelake";
        public string TerroristTeamName = "Lava Gang";
#if UNITY_EDITOR
        [Space(20)] 
        [Header("BUY ZONES")]
#endif
        public Collider CounterTerroristBuyZone;
        public Collider TerroristBuyZone;
#if UNITY_EDITOR
        [Space(20)] 
        [Header("SPAWN POINTS")]
#endif
        public List<Transform> CounterTerroristSpawnPoints = new List<Transform>();
        public List<Transform> TerroristSpawnPoints = new List<Transform>();


#if UNITY_EDITOR
        [Space(20)] 
        [Header("OPTIONAL")]
#endif
        public AvatarCrateReference DefaultAvatar;

#if MELONLOADER
        public Fusion5vs5GamemodeDescriptor(IntPtr intPtr) : base(intPtr)
        {
        }

        public static readonly FusionComponentCache<GameObject, Fusion5vs5GamemodeDescriptor?> Cache =
            new FusionComponentCache<GameObject, Fusion5vs5GamemodeDescriptor?>();

        private void Awake()
        {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            Cache.Remove(gameObject);
        }

        public static class Defaults
        {
            public static readonly Fusion5vs5GamemodeTeams DefendingTeam = Fusion5vs5GamemodeTeams.CounterTerrorists;
            public static readonly string CounterTerroristTeamName = "Sabrelake";
            public static readonly string TerroristTeamName = "Lava Gang";

            public static readonly AvatarCrateReference DefaultAvatar = new("SLZ.BONELAB.Content.Avatar.CharFord");
        }
#else
        public void OnBuyZoneExited() { }
        
        public void OnBuyZoneEntered() { }
        
        public override string Comment =>
            "A script mandatory for making your map compatible with Fusion5vs5Gamemode. This script is required to start the game mode on this map.\n\n" + BuyZoneComment + SpawnPointComment +
            "The Fusion5vs5Gamemode also has events that can trigger your custom UltEvents. To be able to listen to these events, add the \"Invoke5vs5UltEvent\" component to a GameObject.";

        public const string BuyZoneComment =
            "Buy Zones define the zones where a Team will be able to buy weapons in. This is required since we don't want either team to be able to buy weapons from anywhere within the map, " +
            "but rather within a small area inside of the spawnpoints of either teams. Use Colliders to define the buy zones for each team and set them to \"Is Trigger\". " +
            "When you're done, don't forget to add them to their respective slots below.\n\n";

        public const string SpawnPointComment =
            "Spawn points designate the 10 spawn points that are required to be on this map. Add 5 Transforms to each team below. Make sure to place the spawn point Transforms inside of a BuyZone " +
            "Collider, since you want the members of a team to be able to buy weapons after they spawned during Buy Phase.\n\n";
#endif
    }

    public enum Fusion5vs5GamemodeTeams
    {
        CounterTerrorists,
        Terrorists
    }
}