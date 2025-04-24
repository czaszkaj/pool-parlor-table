
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
        [UdonSynced][HideInInspector] private uint timerValue = TIMER_INF;
        [UdonSynced][HideInInspector] private string owner = "";
        [UdonSynced][HideInInspector] private int activeGameMode = -1; // GameModeType.INVALID;
        [UdonSynced][HideInInspector] private bool is4BallKr = false;
        [UdonSynced][HideInInspector] private bool isTeams = false;
        [UdonSynced][HideInInspector] private bool isLocking = false;
        [UdonSynced][HideInInspector] private bool isGuideline = false;
        public const int INVALID_PLAYER_ID = -1;
        public const int MAX_PLAYERS = 4;
        // Team 1: player 0, player 1
        // Team 2: player 2, player 3
        [UdonSynced][HideInInspector] private string[] playerNames = new string[MAX_PLAYERS];
        [UdonSynced][HideInInspector] private int[] playerIds = new int[MAX_PLAYERS];
        // Set it to default false for players that join later
        // and need to sync the data.
        private bool isDataSynced = false;

        #region UdonSharpBehaviour
        /// <summary>
        /// This method is called when the script is loaded.
        /// </summary>
        void Start()
        {
            Reset();
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
        public void SetPlayerName(int index, string name)
        {
            if (index < 0 || index >= MAX_PLAYERS)
            {
                Debug.LogError($"Index {index} is out of bounds.");
                return;
            }
            if (playerNames[index] == "")
            {
                isDataSynced = false;
                playerNames[index] = name;
            }
        }

        public int[] PlayerIds
        {
            get => playerIds;
        }
        public void SetPlayerId(int index, int id)
        {
            if (index < 0 || index >= MAX_PLAYERS)
            {
                Debug.LogError($"Index {index} is out of bounds.");
                return;
            }
            if (playerIds[index] == INVALID_PLAYER_ID)
            {
                isDataSynced = false;
                playerIds[index] = id;
            }
        }

        public void SetPlayer(int index, string name, int id)
        {
            if (index < 0 || index >= MAX_PLAYERS)
            {
                Debug.LogError($"Index {index} is out of bounds.");
                return;
            }
            if (playerNames[index] == "")
            {
                isDataSynced = false;
                playerNames[index] = name;
                playerIds[index] = id;
            }
        }

        public bool isPlayer(string name)
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (playerNames[i] == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemovePlayer(int id)
        {
            RemovePlayer(playerNames[id]);
        }

        public void RemovePlayer(string name)
        {
            if (playerNames[0] == name)
            {
                MovePlayerData(1, 0);
            }
            else if (playerNames[1] == name)
            {
                ClearPlayerData(1);
            }
            else if (playerNames[2] == name)
            {
                MovePlayerData(3, 2);
            }
            else if (playerNames[3] == name)
            {
                ClearPlayerData(3);
            }
        }

        private void ClearPlayerData(int index)
        {
            isDataSynced = false;
            playerNames[index] = "";
            playerIds[index] = INVALID_PLAYER_ID;
        }

        private void MovePlayerData(int fromIndex, int toIndex)
        {
            isDataSynced = false;
            playerNames[toIndex] = playerNames[fromIndex];
            playerIds[toIndex] = playerIds[fromIndex];
            ClearPlayerData(fromIndex);
        }

        #endregion

        #region Networking
        /// <summary>
        /// Synchronizes the data to all players.
        /// </summary>
        public void SyncData()
        {
            if (!isDataSynced)
            {
                isDataSynced = true;
                RequestSerialization();
            }
        }
        #endregion // Networking
    }
}