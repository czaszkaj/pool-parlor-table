
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using YAPT;
using YAPT.PoolTable.Players;
using YAPT.PoolTable.Ui;

// Name convenction:
// * Methods starting with _ 
//      Used for networking. They are usually folowed by the same method without _.
//      One exception is _OnButtonPressed which is called from other module.
// * Methods ending with Ind
//      Used for networking. Expected to ba handled by everyone (caller included).
//      Used to avoid race condition. Perform ownership sensitive changes in it.

namespace YAPT.PoolTable.Table
{
    public enum GameModeType : int
    {
        INVALID = -1,
        EIGHT_BALL = 0,
        NINE_BALL = 1,
        FOUR_BALL = 2,
        SIX_REDS = 3
    }

    public class GameModule : UdonSharpBehaviour
    {
        [Header("Table Objects")]
        [SerializeField] private GameObject menu;
        [SerializeField] private GameObject tableModel;
        [SerializeField] private GameObject[] cues;
        [SerializeField] private GameObject[] balls;
        private MenuData menuData;
        // TODO: compiler was using wrong MenuManager.
        // To be fixed in final version. Can stay as is for now.
        // This could be related to methaphira files, that i keep for reference
        private YAPT.PoolTable.Ui.MenuManager menuManager;
        private PlayerManager playerManager;

        #region UdonSharpBehaviour
        void Start()
        {
            menuData = menu.GetComponentInParent<MenuData>();
            menuManager = menu.GetComponentInParent<YAPT.PoolTable.Ui.MenuManager>();
            playerManager = menu.GetComponentInParent<PlayerManager>();
        }
        public void FixedUpdate()
        {
            // Timer aniamation
            // Physics
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            // Update game state data // Done automatically with UdonSync
            // Cue ball position
            // Cue ownership
            // Player turn
            // Player scores
            // Player game mode
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            // SyncData();
        }
        #endregion

        #region Game Handling
        public void StartGame()
        {
            Debug.Log("Start game");
            // menuData.IsGameLive = true; // TODO: Testing: Keep it off to allow player modification
            menuData.SyncData();
            VRCPlayerApi player = Networking.LocalPlayer;
            if (player.displayName == menuData.Owner)
            {
                // Set owner of the table
                // TODO: Check who needs to be an owner during shot
                // To update it to all players
                Networking.SetOwner(player, gameObject);
            }
            // Start game
            // Reset game state

        }

        public void EndGame()
        {
            menuData.IsGameLive = false;
            // Trigger who won
            // Sync data
            // Reset menu to Play state
            menuManager.TriggerGameReset();

        }
        #endregion

        #region Testing

        public void TestSetTwoTeams()
        {
            playerManager.SetPlayer(1, "TestPlayer1", 11);
            playerManager.SetPlayer(2, "TestPlayer2", 12);
            playerManager.SetPlayer(3, "TestPlayer3", 13);
        }
        public void TestSetOneTeam()
        {
            playerManager.SetPlayer(1, "TestPlayer1", 11);
            playerManager.RemovePlayerIndex(2);
            playerManager.RemovePlayerIndex(3);
        }

        #endregion
    }
}