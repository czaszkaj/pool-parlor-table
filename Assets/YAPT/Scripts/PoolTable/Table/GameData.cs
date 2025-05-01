
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace YAPT.PoolTable.Game
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GameData : UdonSharpBehaviour
    {
        [UdonSynced] private bool isGameLive = false;
        [UdonSynced] private int teamScore0 = 0;
        [UdonSynced] private int teamScore1 = 0;
        [UdonSynced] private int solidTeam = 0; // TeamTypeE
        [UdonSynced] private int activeTeam = 0; // TeamTypeE
        private bool isDataSynced = true;


        #region UdonSharpBehaviour
        void Start()
        {

        }
        #endregion

        #region Networking

        public void Reset()
        {
            isGameLive = false;
            teamScore0 = 0;
            teamScore1 = 0;
            solidTeam = 0;
            activeTeam = 0;
            isDataSynced = true;
        }

        public bool IsGameLive
        {
            get => isGameLive;
            set
            {
                if (isGameLive != value)
                {
                    isDataSynced = false;
                    isGameLive = value;
                }
            }
        }
        public int TeamScore0
        {
            get => teamScore0;
            set
            {
                if (teamScore0 != value)
                {
                    isDataSynced = false;
                    teamScore0 = value;
                }
            }
        }
        public int TeamScore1
        {
            get => teamScore1;
            set
            {
                if (teamScore1 != value)
                {
                    isDataSynced = false;
                    teamScore1 = value;
                }
            }
        }
        public int SolidTeam
        {
            get => solidTeam;
            set
            {
                if (solidTeam != value)
                {
                    isDataSynced = false;
                    solidTeam = value;
                }
            }
        }
        public int ActiveTeam
        {
            get => activeTeam;
            set
            {
                if (activeTeam != value)
                {
                    isDataSynced = false;
                    activeTeam = value;
                }
            }
        }
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
        #endregion

    }
}