using System;
using Fusion5vs5Gamemode.SDK.Internal;
using UltEvents;
using UnityEngine;
#if MELONLOADER
using LabFusion.Utilities;
#endif


namespace Fusion5vs5Gamemode.SDK
{
#if UNITY_EDITOR
    [AddComponentMenu("Fusion 5vs5 Gamemode/Optional/5vs5 Invoke Ult Event")]
    [RequireComponent(typeof(UltEventHolder))]
#endif
    public class Invoke5vs5UltEvent : Fusion5vs5GamemodeBehaviour
    {
        public Fusion5vs5GamemodeUltEvents Event;

        public enum Fusion5vs5GamemodeUltEvents
        {
            CounterTerroristTeamJoined,     // Triggered whenever a player joined the Counter Terrorist Team.
            TerroristTeamJoined,            // Triggered whenever a player joins the Terrorist Team.
            CounterTerroristTeamScored,     // Triggered whenever the Counter Terrorist Team won a round and thus scored a point.
            TerroristTeamScored,            // Triggered whenever the Terrorist Team won a round and thus scored a point.
            NewRoundStarted,                // Triggered when the new round has begun and the buy phase begins.
            PlayerKilledAnotherPlayer,      // Triggered whenever a player killed another player. The names of the players can be found within the Properties of this script.
            PlayerSuicide,                  // Triggered whenever a player kills themselves.
            WarmupPhaseStarted,             // Triggered whenever the warmup phase starts. The warmup phase starts right after starting the game mode, once the current map has finished reloading.
            BuyPhaseStarted,                // Triggered whenever the buy phase starts, right after warmup phase ends or right after Round End Phase ends.
            PlayPhaseStarted,               // Triggered whenever the buy phase ends. Buy phase ends once the players can start moving again at the beginning of a round.
            RoundEndPhaseStarted,           // Triggered whenever the play phase ends. The play phase ends once one of the teams scored a point. The Round End Phase gives players a couple seconds time to pick up guns or do whatever before the next buy phase begins.
            MatchHalfPhaseStarted,          // Triggered whenever play phase ends AND the "Enable Half Time" setting is enabled AND half of the rounds have been played. This phase serves as a transitional phase and takes a couple of seconds, where every player's team gets switched, player's movement get locked and enables microphone communication with the enemy team.
            MatchEndPhaseStarted            // Triggered whenever a team's score is maxed out, which means the team won. Similarly to the MatchHalfPhase, players get locked in place and have a couple of seconds to talk to the enemy team over microphone before the game mode ends.
        }

        public string CounterTerroristTeamJoinedValue { get; set; }         // Holds the name of the player that joined the Counter Terrorist team. Gets updated right before the CounterTerroristTeamJoined event gets called. 
        public string TerroristTeamJoinedValue { get; set; }                // Holds the name of the player that joined tje Terrorist team. Gets updated right before the TerroristTeamJoined event gets called.
        public int CounterTerroristTeamScoredValue { get; set; }            // Holds the new total score for the Counter Terrorist Team. Gets updated right before the CounterTerroristTeamScored event gets called.
        public int TerroristTeamScoredValue { get; set; }                   // Holds the new total score for the Terrorist Team. Gets updated right before the TerroristTeamScored event gets called.
        public bool WasLocalTeam { get; set; }                              // This value gets updated alongside CounterTerroristTeamScoredValue or TerroristTeamScoredValue or CounterTerroristTeamJoinedValue or TerroristTeamJoinedValue to indicate whether the local player is part of the winning team / team that has been joined. 
        public int NewRoundStartedValue { get; set; }                       // Holds the new round number based on the total number of rounds played so far. The round number is calculated by adding one to the count of completed rounds
        public string PlayerKilledAnotherPlayerValueKiller { get; set; }    // Holds the name of the player that killed the other player. The other player's name can be found in PlayerKilledAnotherPlayerValueKilled. Gets updated right before the PlayerKilledAnotherPlayer event gets called.
        public string PlayerKilledAnotherPlayerValueKilled { get; set; }    // Holds the name of the player that got killed by PlayerKilledAnotherPlayerValueKiller. Gets updated right before the PlayerKilledAnotherPlayer event gets called.
        public string PlayerSuicideValue { get; set; }                      // Holds the name of the player that killed themself. Gets updated right before the PlayerSuicide event gets called.
        public bool WasLocalPlayer { get; set; }                            // This value gets updated alongside PlayerKilledAnotherPlayerValueKilled and PlayerSuicideValue to indicate whether the player that died was the local player, aka the player that plays the game on the local machine

        // Convenience methods for use in UltEventHolders
        public string ConvertIntToString(int i) => i.ToString();

        public string JoinStrings(string a, string b) => a + b;
        
#if MELONLOADER

        public Invoke5vs5UltEvent(IntPtr intPtr) : base(intPtr)
        {
        }

        public static readonly FusionComponentCache<GameObject, Invoke5vs5UltEvent> Cache =
            new FusionComponentCache<GameObject, Invoke5vs5UltEvent>();

        private void Awake()
        {
            Cache.Add(gameObject, this);
        }

        private void OnDestroy()
        {
            Cache.Remove(gameObject);
        }

        public void Invoke()
        {
            var holder = GetComponent<UltEventHolder>();
            if (holder != null)
                holder.Invoke();
        }
#else
        public override string Comment => "The UltEventHolder attached to this GameObject will be executed whenever the event that can be selected from the dropdown below is triggered. "+
        "Some events also have parameters that they come with. Once triggered, these can be accessed with the properties of this component that end with \"Value\" with the help of UltEvents. For example: the event NewRoundStarted "+
        "comes with an int that contains the new round number. Access this int with the NewRoundStartedValue property. Similar properties have been placed in this script for other events. "+
        "If unsure what some of these properties that end with \"Value\" mean or contain, open this script and read the comments next to these properties.";
#endif
    }
}