
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using System;
using YAPT.PoolTable.Game;
using YAPT.PoolTable.Players;

// Interface with other modules description.
// Requires:
// * MenuData - store all information from menu interaction.
// * GameModule.StartGame() - "transfer" MenuData to GameModule.
// * TriggerGameReset() - used by GameModule to reset Menu.
// * [NonSerialized] public UIButton inButton; - required by _OnButtonPressed().
// * _OnButtonPressed() - used by UIButton module.
// Nice to have:
// * PlayerManager - easier manipulation of players information.

// Name convenction:
// * Methods starting with _ 
//      Used for networking. They are usually folowed by the same method without _.
//      One exception is _OnButtonPressed which is called from other module.
// * Methods ending with Ind
//      Used for networking. Expected to ba handled by everyone (caller included).
//      Used to avoid race condition. Perform ownership sensitive changes in it.

namespace YAPT.PoolTable.Ui
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MenuManager : UdonSharpBehaviour
    {
        private MenuData data;
        private PlayerManager playerManager;

        [Header("Game Objects")]
        [SerializeField][HideInInspector] private GameModule gameModule; // Assighned in Start

        [Header("Menu Objects")]
        [SerializeField] public GameObject menuGame;
        [SerializeField] private GameObject menuStart;
        [SerializeField] private Text[] playerNames;

        [SerializeField] private GameObject teamCover;
        [SerializeField] private GameObject timelimitDisplay;

        [Header("Menu Buttons")]
        [SerializeField] public UIButton button8Ball;
        [SerializeField] public UIButton button9Ball;
        [SerializeField] public UIButton button4Ball;
        [SerializeField] public UIButton button4BallJP;
        [SerializeField] public UIButton button4BallKR;
        [SerializeField] public UIButton buttonSixReds;
        [SerializeField] public UIButton buttonTimerLeft;
        [SerializeField] public UIButton buttonTimerRight;
        [SerializeField] public UIButton buttonTeamsToggle;
        [SerializeField] public UIButton buttonGuidelineToggle;
        [SerializeField] public UIButton buttonLockingToggle;

        [SerializeField] public UIButton buttonLeave;
        [SerializeField] public UIButton buttonPlay;
        [SerializeField] public UIButton buttonJoinOrange;
        [SerializeField] public UIButton buttonJoinBlue;

        private bool requestedPlay = false;

        private const uint TIMER_MAX_ID = 3;
        private bool isTimeSelectAnimating = false;
        [UdonSynced, FieldChangeCallback("SelectedTimer")]
        [HideInInspector] private uint selectedTimer = TIMER_MAX_ID;
        private float targetPosition;
        private readonly uint[] TIMER_VALUES = { 15, 30, 60, MenuData.TIMER_INF };
        private bool timerSpinPlaying;

        #region UdonSharpBehaviour

        void Start()
        {
            menuGame.SetActive(false);
            menuStart.SetActive(true);
            data = GetComponent<MenuData>();
            playerManager = GetComponent<PlayerManager>();
            // Set gameModule in the Start.
            // This is needed to allow name changes of the parent object (Table1, Table2, TableInDoors, etc.)
            gameModule = transform.parent.gameObject.GetComponent<GameModule>();
        }

        // Handle animated parts of the menu
        void FixedUpdate()
        {
            // Timer selection animation
            if (isTimeSelectAnimating)
            {
                Vector3 position = timelimitDisplay.transform.localPosition;
                position.x = Mathf.Lerp(position.x, targetPosition, Time.deltaTime * 5.0f); // TODO: Magic number
                timelimitDisplay.transform.localPosition = position;
                if (Mathf.Abs(position.x - targetPosition) < 0.01f)
                {
                    isTimeSelectAnimating = false;
                }
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            // Check if missing player is part of the game
            if (!playerManager.IsPlayer(player.displayName)) return;
            if (isMenuOwner(player))
            {
                // Game owner
                // Reset game state
                UnregisterOwner();

            }
            else if (playerManager.IsPlayer(Networking.LocalPlayer.displayName))
            {
                // Local player
                // Remove player from the menu selection
                // playerManager.RemovePlayer(player.displayName);
                // UpdatePlayerNameObject();
            }

        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal && data.Owner != "")
            {
                // Owner is set.
                // Menu selection is active.
                menuGame.SetActive(true);
                menuStart.SetActive(false);
                // Update locally menu state
                UpdatePlayerNameObject();
                SwitchFourBallMode(data.Is4BallKr);
                SwitchGameMode();
                if (data.TimerValue != 0)
                {
                    selectedTimer = data.TimerValue;
                    SetSelectedTimerPosition();
                }
            }
        }
        public uint SelectedTimer
        {
            get => selectedTimer;
            set
            {
                if (selectedTimer != value)
                {
                    if (value >= 0 && value <= TIMER_MAX_ID)
                    {
                        selectedTimer = value;
                        data.TimerValue = TIMER_VALUES[selectedTimer];
                        StartSelectedTimerAnimation();
                    }
                }
            }
        }

        private void SetSelectedTimerPosition()
        {
            // Set timer position
            Vector3 position = timelimitDisplay.transform.localPosition;
            position.x = -0.128f * selectedTimer;
            timelimitDisplay.transform.localPosition = position;
        }

        private void StartSelectedTimerAnimation()
        {
            isTimeSelectAnimating = true;
            targetPosition = -0.128f * selectedTimer;
        }

        #endregion // UdonSharpBehaviour

        #region ButtonUi
        [NonSerialized] public UIButton inButton;
        public void _OnButtonPressed()
        {
            if (inButton.name == "StartButton")
            {
                // Any player can start the game
                data.Reset();
                data.SyncData();
                _RegisterOwner();
                UpdatePlayerNameObject();
            }
            else if (isMenuOwner(Networking.LocalPlayer))
            {
                OnButtonPressedOwner(inButton);
            }
            else
            {
                OnButtonPressed(inButton);
            }
        }

        private void OnButtonPressed(UIButton button)
        {
            // Update functionality for other players
            switch (button.name)
            {
                case "JoinOrange":
                    _JoinTeam0();
                    break;
                case "JoinBlue":
                    _JoinTeam1();
                    break;
                case "LeaveButton":
                    _LeaveTeam();
                    break;
                case "TeamsToggle":
                    // Test teams toggle
                    _ToggleTeamsObject();
                    break;
            }
        }

        private void OnButtonPressedOwner(UIButton button)
        {
            switch (button.name)
            {
                case "LeaveButton":
                    UnregisterOwner();
                    break;
                case "PlayButton":
                    _StartGame();
                    return;
                case "8Ball":
                    _SwitchGameMode(GameModeType.EIGHT_BALL);
                    break;
                case "9Ball":
                    _SwitchGameMode(GameModeType.NINE_BALL);
                    break;
                case "4Ball":
                    _SwitchGameMode(GameModeType.FOUR_BALL);
                    break;
                case "4BallJP":
                    _SwitchFourBallMode(false);
                    break;
                case "4BallKR":
                    _SwitchFourBallMode(true);
                    break;
                case "Snooker6Red":
                    _SwitchGameMode(GameModeType.SIX_REDS);
                    break;
                case "TeamsToggle":
                    _ToggleTeamsObject();
                    break;
                case "GuidelineToggle":
                    _ToggleGuidelineObject();
                    break;
                case "LockingToggle":
                    _ToggleLockingObject();
                    break;
                case "TimeRight":
                    SelectedTimer += 1;
                    RequestSerialization();
                    break;
                case "TimeLeft":
                    SelectedTimer -= 1;
                    RequestSerialization();
                    break;
            }
        }
        #endregion // ButtonUi

        #region Time select

        #endregion // Time select

        #region Networking

        // Sets default button during first game start
        private void _PressDefaultGameType()
        {
            if (data.ActiveGameMode == (int)GameModeType.INVALID)
            {
                // Default game type
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PressDefaultGameType");
                data.ActiveGameMode = (int)GameModeType.EIGHT_BALL;
                // From this point it should be valid for all players
            }
        }

        public void PressDefaultGameType()
        {
            button8Ball._SetButtonPushed();
            button4BallKR._SetButtonPushed();
        }

        private void _StartGame()
        {
            data.SyncData();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartGame");
        }
        public void StartGame()
        {
            // Practice mode to be validated and selected by the game
            // based on player team allocation.

            menuGame.SetActive(false);
            gameModule.StartGame();
        }

        public void _RegisterOwner()
        {
            // Check if the owner is already set
            if (data.Owner != "") return;

            // Set the game owner
            VRCPlayerApi player = Networking.LocalPlayer;
            _SetMenuOwner(player);
            // Show the game menu
            menuStart.SetActive(false);
            menuGame.SetActive(true);
            // Inform other players to avoid race condition
            requestedPlay = true;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RegisterOwnerInd");
        }

        public void RegisterOwnerInd()
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            bool isOwner = Networking.IsOwner(gameObject);
            // Check if the owner is already set and allocate player id
            if (requestedPlay && !isOwner)
            {
                playerManager.SetPlayer(1, player.displayName, player.playerId);
            }
            else if (isOwner)
            {
                _PressDefaultGameType(); // Only one call required
                playerManager.SetPlayer(0, player.displayName, player.playerId);
            }
            requestedPlay = false;

            // Functionality after onwership validation

            // Update game state for all players
            SetButtonInteractionLimits(isOwner);
            if (isOwner)
            {
                SetOwnerJoinButtons();
            }
        }

        private void SetButtonInteractionLimits(bool isOwner)
        {
            // Disable setting buttons for other players
            button8Ball.disableInteractions = !isOwner;
            button9Ball.disableInteractions = !isOwner;
            button4Ball.disableInteractions = !isOwner;
            button4BallJP.disableInteractions = !isOwner;
            button4BallKR.disableInteractions = !isOwner;
            buttonSixReds.disableInteractions = !isOwner;
            buttonTeamsToggle.disableInteractions = !isOwner;
            buttonGuidelineToggle.disableInteractions = !isOwner;
            buttonLockingToggle.disableInteractions = !isOwner;
            buttonTimerLeft.disableInteractions = !isOwner;
            buttonTimerRight.disableInteractions = !isOwner;
        }

        private void SetOwnerJoinButtons()
        {
            // Local join buttons setup
            buttonJoinOrange.gameObject.SetActive(false);
            buttonJoinBlue.gameObject.SetActive(false);
            buttonLeave.gameObject.SetActive(true);
            buttonPlay.gameObject.SetActive(true);
        }

        public void UnregisterOwner()
        {
            menuGame.SetActive(false);
            menuStart.SetActive(true);
            data.Reset();
            data.SyncData();
        }

        [UdonSynced][HideInInspector] private int newGameType;
        private void _SwitchGameMode(GameModeType gameType)
        {
            newGameType = (int)gameType;
            RequestSerialization();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SwitchGameMode");
        }

        public void SwitchGameMode()
        {
            switch ((GameModeType)data.ActiveGameMode)
            {
                case GameModeType.EIGHT_BALL:
                    button8Ball._ResetPushButton();
                    break;
                case GameModeType.NINE_BALL:
                    button9Ball._ResetPushButton();
                    break;
                case GameModeType.FOUR_BALL:
                    button4Ball._ResetPushButton();
                    break;
                case GameModeType.SIX_REDS:
                    buttonSixReds._ResetPushButton();
                    break;
            }
            data.ActiveGameMode = newGameType;
        }

        [UdonSynced][HideInInspector] private bool newIs4BallKr;
        private void _SwitchFourBallMode(bool is4BallKr)
        {
            newIs4BallKr = is4BallKr;
            RequestSerialization();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SwitchFourBallMode");
        }

        public void SwitchFourBallMode()
        {
            SwitchFourBallMode(newIs4BallKr);
        }
        public void SwitchFourBallMode(bool is4BallKr)
        {
            if (is4BallKr)
            {
                button4BallJP._ResetPushButton();
                // button4BallKR._SetButtonPushed();
            }
            else
            {
                button4BallKR._ResetPushButton();
                // button4BallJP._SetButtonPushed();
            }
            data.Is4BallKr = is4BallKr;
        }

        private void _UpdatePlayerNameObject()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdatePlayerNameObject");
        }
        public void UpdatePlayerNameObject()
        {
            // Player Names
            // Important! Indexing between stored names and displayed names Text are not the same.
            teamCover.SetActive(!data.IsTeams);
            if (data.IsTeams)
            {
                // Team 1
                playerNames[2].GetComponent<Text>().text = data.PlayerNames[0];
                playerNames[0].GetComponent<Text>().text = data.PlayerNames[1];
                // Team 2
                playerNames[1].GetComponent<Text>().text = data.PlayerNames[2];
                playerNames[3].GetComponent<Text>().text = data.PlayerNames[3];

            }
            else
            {
                // Team 1
                playerNames[0].GetComponent<Text>().text = data.PlayerNames[0];
                // Team 2
                playerNames[1].GetComponent<Text>().text = data.PlayerNames[2];
            }
        }

        private void _ToggleGuidelineObject()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleGuidelineObject");
        }
        public void ToggleGuidelineObject()
        {
            data.IsGuideline = !data.IsGuideline;
        }

        private void _ToggleLockingObject()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleLockingObject");
        }
        public void ToggleLockingObject()
        {
            data.IsLocking = !data.IsLocking;
        }
        private void _ToggleTeamsObject()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToggleTeamsObject");
        }
        public void ToggleTeamsObject()
        {
            data.IsTeams = !data.IsTeams;
            // Remove 2nd players from teams
            playerManager.RemovePlayerIndex(1);
            playerManager.RemovePlayerIndex(3);
            // Refresh menu view
            ToggleJoinButtons(!playerManager.IsPlayer(Networking.LocalPlayer.displayName));
            // Update player names
            _UpdatePlayerNameObject();
        }

        // <summary>
        // [LOCAL functionality, not synchronized]
        // Show either join or leave buttons
        // It depends if the player is already in the game
        // and if the teams are full.
        // </summary>
        private void ToggleJoinButtons(bool show_join)
        {
            if (show_join)
            {
                if (!playerManager.IsTeamFull(0))
                {
                    buttonJoinOrange.gameObject.SetActive(true);
                }
                else
                {
                    buttonJoinOrange.gameObject.SetActive(false);
                }
                if (!playerManager.IsTeamFull(1))
                {
                    buttonJoinBlue.gameObject.SetActive(true);
                }
                else
                {
                    buttonJoinBlue.gameObject.SetActive(false);
                }
                buttonLeave.gameObject.SetActive(false);
            }
            else
            {
                if (playerManager.IsPlayer(Networking.LocalPlayer.displayName))
                {
                    buttonLeave.gameObject.SetActive(true);
                }
                buttonJoinBlue.gameObject.SetActive(false);
                buttonJoinOrange.gameObject.SetActive(false);
            }
        }

        private void _JoinTeam(int team_id)
        {
            playerManager.AddTeamPlayer(team_id, Networking.LocalPlayer.displayName, Networking.LocalPlayer.playerId);
            ToggleJoinButtons(false);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, $"JoinTeam{team_id}");
        }

        private void _JoinTeam0()
        {
            _JoinTeam(0);
        }

        private void _JoinTeam1()
        {
            _JoinTeam(1);
        }

        public void JoinTeam(int team_id)
        {
            if (playerManager.IsTeamFull(team_id))
            {
                buttonJoinBlue.gameObject.SetActive(false);
            }
            UpdatePlayerNameObject();
        }

        public void JoinTeam0()
        {
            JoinTeam(0);
        }

        public void JoinTeam1()
        {
            JoinTeam(1);
        }

        public void _LeaveTeam()
        {
            playerManager.RemovePlayer(Networking.LocalPlayer.displayName);
            // ToggleJoinButtons(true);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LeaveTeam");
        }
        public void LeaveTeam()
        {
            ToggleJoinButtons(true);
            UpdatePlayerNameObject();
        }

        public void TriggerGameReset()
        {
            data.Reset();
            data.SyncData();
            menuGame.SetActive(false);
            menuStart.SetActive(true);
        }

        public void _SetMenuOwner(VRCPlayerApi player)
        {
            Networking.SetOwner(player, gameObject);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetMenuOwnerInd");
        }

        public void SetMenuOwnerInd()
        {
            // Later validation of the owner.
            // Confirm who got the ownership, to avoid race condition
            if (Networking.IsOwner(gameObject))
            {
                data.Owner = Networking.LocalPlayer.displayName;
                data.SyncData();
            }
        }

        public bool isMenuOwner(VRCPlayerApi player)
        {
            // Use data to check current owner.
            // Otherwise if the players leaves the game instance master would take ownership
            return data.Owner == player.displayName;
        }

        #endregion // Synchronization

        #region Other functions
        #endregion // Other functions

        #region Testing
        // Can't validate Netwworking because not every VRCPlayerApi object is a VRCObjectSync
        // Switching Onwership and checking not owner only functionality requires code modification
        // [Header("Test")]

        // public void TestListLobbyPlayers()
        // {
        //     foreach (VRCPlayerApi player in VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]))
        //     {
        //         Debug.Log($"Player: name:{player.displayName} id:{player.playerId} isLocal:{player.isLocal}");
        //     }
        // }

        public void TestListGamePlayers()
        {
            for (int i = 0; i < PlayerManager.MAX_PLAYERS; i++)
            {
                Debug.Log($"Player: name:{data.PlayerNames[i]} id:{data.PlayerIds[i]}");
            }
        }
        // private string testplayerName2 = "[2] Remote Player";
        // private int testplayerId2 = 2;

        // public void TestSimOtherPlayer()
        // {
        //     // Simulate other player joining
        //     data.PlayerNames[0] = testplayerName2;
        //     data.PlayerIds[0] = testplayerId2;
        //     buttonPlay.gameObject.SetActive(false);
        //     ToggleJoinButtons(true);
        //     _UpdatePlayerNameObject();
        // }

        // public void TestAddPlayer1()
        // {
        //     playerManager.SetPlayer(1, "TestPlayer1", 11);
        //     _UpdatePlayerNameObject();
        // }
        // public void TestAddPlayer2()
        // {
        //     playerManager.SetPlayer(2, "TestPlayer2", 12);
        //     _UpdatePlayerNameObject();
        // }
        // public void TestAddPlayer3()
        // {
        //     playerManager.SetPlayer(3, "TestPlayer3", 13);
        //     _UpdatePlayerNameObject();
        // }
        // public void TestRemovePlayer1()
        // {
        //     playerManager.RemovePlayer("TestPlayer1");
        //     _UpdatePlayerNameObject();
        // }
        // public void TestRemovePlayer2()
        // {
        //     playerManager.RemovePlayer("TestPlayer2");
        //     _UpdatePlayerNameObject();
        // }
        // public void TestRemovePlayer3()
        // {
        //     playerManager.RemovePlayer("TestPlayer3");
        //     _UpdatePlayerNameObject();
        // }
        #endregion
    }
}