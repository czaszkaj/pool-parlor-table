
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using System;
using YAPT.PoolTable.Table;
using YAPT.PoolTable.Common;
using YAPT.PoolTable.Players;

// TODO
// Add player validation for public methods, to avoid hackers
namespace YAPT.PoolTable.Ui
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MenuManager : UdonSharpBehaviour
    {
        private MenuData data;
        private PlayerManager playerManager;

        [Header("Game Objects")]
        [SerializeField][HideInInspector] private GameObject gameModule; // Assighned in Start
        [SerializeField] private GameObject gameOwner;

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
            // Set gameModule to the parent of gameOwner
            // This is needed to allow name changes of the parent object (Table1, Table2, TableInDoors, etc.)
            gameModule = gameOwner.transform.parent.gameObject;
        }

        // Handle animated parts of the menu
        void FixedUpdate()
        {
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
            if (!playerManager.isPlayer(player.displayName)) return;
            VRCPlayerApi gameOwnerPlayer = Networking.GetOwner(gameOwner);
            if (gameOwnerPlayer.playerId == player.playerId)
            {
                playerManager.RemovePlayer(player.displayName);
                Networking.SetOwner(null, gameOwner);
                if (playerManager.isPlayer(Networking.LocalPlayer.displayName))
                {
                    RegisterOwner();
                    UpdatePlayerNameObject();
                }

            }
            else if (playerManager.isPlayer(Networking.LocalPlayer.displayName))
            {
                // Local player
                playerManager.RemovePlayer(player.displayName);
                UpdatePlayerNameObject();
            }

        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            Debug.Log($"OnPlayerJoined: name:{player.displayName}, isOwner: {Networking.IsOwner(player, gameOwner)}");
            if (player.isLocal && data.Owner != "")
            {
                // If Owner available update menu state
                menuGame.SetActive(true);
                menuStart.SetActive(false);
                UpdatePlayerNameObject();
                _SwitchFourBallMode(data.Is4BallKr);
                _SwitchGameMode((GameModeType)data.ActiveGameMode);
                if (data.TimerValue != 0)
                {
                    selectedTimer = data.TimerValue;
                    StartSelectedTimerAnimation();
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
            Debug.Log($"UIButton: receivedPressedEvent {inButton.name}, isOwner: {Networking.IsOwner(gameOwner)}");
            if (inButton.name == "StartButton")
            {
                RegisterOwner();
                UpdatePlayerNameObject();
            }
            else if (Networking.IsOwner(gameOwner))
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
            // Add in tests option to register other player as owner
            Debug.Log($"UIButton: AnyPlayer");
            switch (button.name)
            {
                // case "StartButton":
                //     RegisterOwner();
                //     break;
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
            // based on player allocation.
            // Save of parameter synchronization if not needed.

            // TODO: Not handled yet. Need at least RESET to function
            // // menuGame.SetActive(false);

            // Trigger Game Start in game module
            // It will start the game based on MenuData
            //gameModule.SendCustomEvent("StartGame");
        }

        public void RegisterOwner()
        {
            // Check if the owner is already set
            if (data.Owner != "") return;

            // Set the game owner
            Networking.SetOwner(Networking.LocalPlayer, gameOwner);
            playerManager.SetPlayerName(0, Networking.LocalPlayer.displayName);
            data.Owner = Networking.LocalPlayer.displayName;
            data.SyncData();
            // Show the game menu
            menuStart.SetActive(false);
            menuGame.SetActive(true);
            // Inform other players to avoid race condition
            requestedPlay = true;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RegisterOwnerInd");
        }

        /// <summary>
        /// This function is called to update gameOwner to prevent
        /// race condition. It uses currently set gameOwner as host.
        /// </summary>
        public void RegisterOwnerInd()
        {
            int playerNameId = PlayerManager.INVALID_PLAYER_ID;
            bool isOwner = Networking.IsOwner(gameOwner);
            // Check if the owner is already set and allocate player id
            if (requestedPlay && !isOwner)
            {
                data.Owner = Networking.GetOwner(gameOwner).displayName;
                // Shouldn't be needed, we already Sync data
                //playerManager.SetPlayerName(0, data.Owner);
                playerNameId = 1;
            }
            else if (isOwner)
            {
                _PressDefaultGameType();
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                playerNameId = 0;
            }
            requestedPlay = false;

            // Handle rare cases when more then 2 players try to start the game.
            // Assure that local player is registered in MenuData.
            if (playerManager.isPlayer(Networking.LocalPlayer.displayName))
            {
                playerManager.SetPlayerId(playerNameId, Networking.LocalPlayer.playerId);
            }

            // Update game state for all players
            if (!isOwner)
            {
                // Disable setting buttons for other players
                button8Ball.disableInteractions = true;
                button9Ball.disableInteractions = true;
                button4Ball.disableInteractions = true;
                button4BallJP.disableInteractions = true;
                button4BallKR.disableInteractions = true;
                buttonSixReds.disableInteractions = true;
                buttonTeamsToggle.disableInteractions = true;
                buttonGuidelineToggle.disableInteractions = true;
                buttonLockingToggle.disableInteractions = true;
                buttonTimerLeft.disableInteractions = true;
                buttonTimerRight.disableInteractions = true;
            }
            else
            {
                // Local join buttons setup
                buttonJoinOrange.gameObject.SetActive(false);
                buttonJoinBlue.gameObject.SetActive(false);
                buttonLeave.gameObject.SetActive(true);
                buttonPlay.gameObject.SetActive(true);
            }
        }

        public void UnregisterOwner()
        {
            // TODO: handle menu ownership transfer
            // As is it can be problematic, since SetOwner can be called
            // only from the owner or lobby owner.
            // TODO: Switch from Ownership to just player name
            if (data.Owner == "") return;

            Networking.SetOwner(null, gameOwner);
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
            if (newIs4BallKr)
            {
                button4BallJP._ResetPushButton();
                // button4BallKR._SetButtonPushed();
            }
            else
            {
                button4BallKR._ResetPushButton();
                // button4BallJP._SetButtonPushed();
            }
            data.Is4BallKr = newIs4BallKr;
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
            ToggleJoinButtons(!playerManager.isPlayer(Networking.LocalPlayer.displayName));
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
                if (!playerManager.isTeamFull(0))
                {
                    buttonJoinOrange.gameObject.SetActive(true);
                }
                else
                {
                    buttonJoinOrange.gameObject.SetActive(false);
                }
                if (!playerManager.isTeamFull(1))
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
                if (playerManager.isPlayer(Networking.LocalPlayer.displayName))
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
            data.SyncData();
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
            if (playerManager.isTeamFull(team_id))
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
            data.SyncData();
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

        #endregion // Synchronization

        #region Other methods
        #endregion // Other methods

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

        // public void TestListGamePlayers()
        // {
        //     for (int i = 0; i < PlayerManager.MAX_PLAYERS; i++)
        //     {
        //         Debug.Log($"Player: name:{data.PlayerNames[i]} id:{data.PlayerIds[i]}");
        //     }
        // }
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