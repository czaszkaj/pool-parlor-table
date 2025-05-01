
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

// <summary>
// This script is part of the YAPT (Yet Another Pool Table) project.
// It is used to manage the menu data for the pool table.
// It also allows for game players handling
// <summary>
namespace YAPT.PoolTable.Ui
{
    /// <summary>
    /// This class is used to store the menu data for the pool table.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MenuData : UdonSharpBehaviour
    {
        public const uint TIMER_INF = 999;
        private const int MAX_PLAYERS = YAPT.PoolTable.Players.PlayerManager.MAX_PLAYERS;
        private const int INVALID_PLAYER_ID = YAPT.PoolTable.Players.PlayerManager.INVALID_PLAYER_ID;
        [UdonSynced][HideInInspector] private uint timerValue = TIMER_INF;
        [UdonSynced][HideInInspector] private string owner = "";
        [UdonSynced][HideInInspector] private int activeGameMode = -1; // GameModeType.INVALID;
        [UdonSynced][HideInInspector] private bool is4BallKr = false;
        [UdonSynced][HideInInspector] private bool isTeams = false;
        [UdonSynced][HideInInspector] private bool isLocking = false;
        [UdonSynced][HideInInspector] private bool isGuideline = false;
        // Team 1: player 0, player 1
        // Team 2: player 2, player 3
        [UdonSynced][HideInInspector] private string[] playerNames = new string[MAX_PLAYERS];
        [UdonSynced][HideInInspector] private int[] playerIds = new int[MAX_PLAYERS];
        // Set it to default false for players that join later
        // and need to sync the data.
        private bool isDataSynced = true;

        #region UdonSharpBehaviour
        /// <summary>
        /// This method is called when the script is loaded.
        /// </summary>
        void Start()
        {

        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            // SyncData();
        }
        #endregion // UdonSharpBehaviour

        #region Data Handling
        /// <summary>
        /// Resets only player information.
        /// Keep the game mode and other settings.
        /// </summary>
        public void Reset()
        {
            owner = "";
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                playerNames[i] = "";
                playerIds[i] = INVALID_PLAYER_ID;
            }
            isDataSynced = false;
        }

        public string Owner
        {
            get => owner;
            set
            {
                if (owner != value)
                {
                    isDataSynced = false;
                    owner = value;
                }
            }
        }

        public int ActiveGameMode
        {
            get => activeGameMode;
            set
            {
                if (activeGameMode != value)
                {
                    isDataSynced = false;
                    activeGameMode = value;
                }
            }
        }

        public bool Is4BallKr
        {
            get => is4BallKr;
            set
            {
                if (is4BallKr != value)
                {
                    isDataSynced = false;
                    is4BallKr = value;
                }
            }
        }

        public bool IsTeams
        {
            get => isTeams;
            set
            {
                if (isTeams != value)
                {
                    isDataSynced = false;
                    isTeams = value;
                }
            }
        }

        public bool IsLocking
        {
            get => isLocking;
            set
            {
                if (isLocking != value)
                {
                    isDataSynced = false;
                    isLocking = value;
                }
            }
        }

        public bool IsGuideline
        {
            get => isGuideline;
            set
            {
                if (isGuideline != value)
                {
                    isDataSynced = false;
                    isGuideline = value;
                }
            }
        }

        public uint TimerValue
        {
            get => timerValue;
            set
            {
                if (timerValue != value)
                {
                    isDataSynced = false;
                    timerValue = value;
                }
            }
        }

        public string[] PlayerNames
        {
            get => playerNames;
        }
        public int[] PlayerIds
        {
            get => playerIds;
        }

        #endregion

        #region Networking
        // <summary>
        // Synchronizes the data to all players if there was a local change.
        // </summary>
        public void SyncData()
        {
            if (!isDataSynced)
            {
                // Take ownership to update the data
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                // Sync all data
                isDataSynced = true;
                RequestSerialization();
            }
        }
        #endregion // Networking

        #region Testing
        public void TestPrintData()
        {
            Debug.Log($"-------- MenuData ----------\n" +
                      $"TimerValue: {timerValue}\n" +
                      $"Owner: {owner}\n" +
                      $"ActiveGameMode: {activeGameMode}\n" +
                      $"Is4BallKr: {is4BallKr}\n" +
                      $"IsTeams: {isTeams}\n" +
                      $"IsLocking: {isLocking}\n" +
                      $"IsGuideline: {isGuideline}\n" +
                      $"PlayerNames[0]: {playerNames[0]}\n" +
                      $"PlayerNames[1]: {playerNames[1]}\n" +
                      $"PlayerNames[2]: {playerNames[2]}\n" +
                      $"PlayerNames[3]: {playerNames[3]}\n");
        }
        #endregion
    }
}