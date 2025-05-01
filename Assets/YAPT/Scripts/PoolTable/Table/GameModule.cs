
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using YAPT;
using YAPT.PoolTable.Balls;
using YAPT.PoolTable.Cue;
using YAPT.PoolTable.Players;
using YAPT.PoolTable.Scoreboard;
using YAPT.PoolTable.Ui;

// Name convenction:
// * Methods starting with _ 
//      Used for networking. They are usually folowed by the same method without _.
//      One exception is _OnButtonPressed which is called from other module.
// * Methods ending with Ind
//      Used for networking. Expected to ba handled by everyone (caller included).
//      Used to avoid race condition. Perform ownership sensitive changes in it.

namespace YAPT.PoolTable.Game
{
    // Similar to YAPT.PoolTable.Scoreboard.SolidTeamE
    public enum TeamTypeE : int
    {
        INVALID = 0,
        LEFT = 1,
        RIGHT = 2
    }

    public enum GameModeType : int
    {
        INVALID = -1,
        EIGHT_BALL = 0,
        NINE_BALL = 1,
        FOUR_BALL = 2, // Default JP
        FOUR_BALL_KR = 3,
        SIX_REDS = 4
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GameModule : UdonSharpBehaviour
    {
        [Header("Table Objects")]
        [SerializeField] private GameObject tableModel;
        [SerializeField] private GameObject menuObj;
        [SerializeField] private GameObject fourBallFiller;
        [SerializeField] private ScoreboardManager scoreboard;

        [SerializeField] private CueManager[] cuesMgr;
        [SerializeField] private BallManager ballsMgr;
        // Menu data should be constat throughout single match
        private MenuData menuData;
        private GameData gameData;
        private GameModeType activeGameType;
        // TODO: compiler was using wrong MenuManager.
        // To be fixed in final version. Can stay as is for now.
        // This could be related to methaphira files, that i keep for reference
        private YAPT.PoolTable.Ui.MenuManager menuMgr;
        private PlayerManager playersMgr;

        #region UdonSharpBehaviour
        void Start()
        {
            // Get local access to components
            gameData = GetComponentInParent<GameData>();
            menuData = menuObj.GetComponentInParent<MenuData>();
            menuMgr = menuObj.GetComponentInParent<YAPT.PoolTable.Ui.MenuManager>();
            playersMgr = menuObj.GetComponentInParent<PlayerManager>();
            ballsMgr = GetComponentInParent<BallManager>();
        }
        public void FixedUpdate()
        {
            // Timer aniamation
            // Physics
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            // Update game state data // Done automatically with UdonSync
            if (player == Networking.LocalPlayer)
            {
                SetupTableConfiguration();
                // Player scores
                SetScoreboard();
                // Player turn
            }
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
            TestStartGame();
            gameData.IsGameLive = true;
            // Reset game state
            SetupTableConfiguration();
        }

        public void EndGame()
        {
            gameData.IsGameLive = false;
            // Reset cuesMgr
            cuesMgr[0].Disable();
            cuesMgr[1].Disable();
            cuesMgr[0].ResetCuePosition();
            cuesMgr[1].ResetCuePosition();
            // Trigger who won
            // Sync data
            // Reset menu to Play state
            menuMgr.TriggerGameReset();

        }

        private void SetupTableConfiguration()
        {
            activeGameType = (GameModeType)menuData.ActiveGameMode;
            SetCuesAccess();
            SetScoreboard();
            // Set locking mode
            // Set pratice mode
            // Set 4 ball fillers
            fourBallFiller.SetActive(IsFourBall());
            // rulesManager.SetPraticeMode(playersMgr.IsSingleTeam());
            // Start game
            // ballsManager.SetActimeGameMode(activeGameType);
            // ballsManager.SetPosition();
            // Start timer
        }

        private bool IsFourBall()
        {
            return (activeGameType == GameModeType.FOUR_BALL || activeGameType == GameModeType.FOUR_BALL_KR);
        }

        private void SetCuesAccess()
        {
            cuesMgr[0].SetAuthorizedOwners(playersMgr.GetTeamNames(0));
            cuesMgr[1].SetAuthorizedOwners(playersMgr.GetTeamNames(1));
            cuesMgr[0].Enable();
            cuesMgr[1].Enable();
        }

        private void SetScoreboard()
        {
            switch (activeGameType)
            {
                case GameModeType.EIGHT_BALL:
                    // scoreboard.SetScore8Ball(ballsManager.ScoredBallsColors, gameData.SolidTeam);
                    break;
                case GameModeType.NINE_BALL:
                    // scoreboard.SetScore9Ball(ballsManager.CurrentBallTarget);
                    break;
                case GameModeType.FOUR_BALL:
                    scoreboard.SetScore4Ball(gameData.TeamScore0, gameData.TeamScore1);
                    break;
                case GameModeType.SIX_REDS:
                    // scoreboard.SetScoreSnooker(gameData.TeamScore0, gameData.TeamScore1, ballsManager.CurrentBallTarget);
                    break;
            }
            scoreboard.FillPlayerNames(menuData.PlayerNames);
        }
        #endregion

        #region Testing
        public void TestStartGame()
        {
            TestSetTwoTeams();
        }

        public void TestSetTwoTeams()
        {
            playersMgr.SetPlayer(1, "TestPlayer1", 11);
            playersMgr.SetPlayer(2, "TestPlayer2", 12);
            playersMgr.SetPlayer(3, "TestPlayer3", 13);
        }
        public void TestSetOneTeam()
        {
            playersMgr.SetPlayer(1, "TestPlayer1", 11);
            playersMgr.RemovePlayerIndex(2);
            playersMgr.RemovePlayerIndex(3);
        }

        #endregion
    }
}