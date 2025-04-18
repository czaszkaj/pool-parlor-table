#if UNITY_ANDROID
#define HT_QUEST
#endif

#if !HT_QUEST || true
#define HT8B_DEBUGGER
#endif

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using System;
using Metaphira.Modules.CameraOverride;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class BilliardsModule : UdonSharpBehaviour
{
    // TODO: To be used in debugger
    // Base version before fork: 1.0.4
    [NonSerialized] public readonly string VERSION = "1.0.4"; // "1.1.0"; // After refactor

    // Non-Serialized Dependecies
    [NonSerialized]  public GameModule game;
    [NonSerialized]  public Logger logger;
    [NonSerialized]  public PlayerManager playerManager;
    [NonSerialized]  public AudioManager audio;

    // Serialized parameters
    [SerializeField] Text ltext;    // DebbugerText : TODO: rename
    // Somethign related with reflection.
    // TODO: analyze why we need this
    [SerializeField] ReflectionProbe reflection_main;

    #region ParametersMovedToOtherClasses
    // TODO: Remove
    // TODO: check why we get error for serialization if this is removed.
    // If too obnoxious just redo This class after everything
    // Maybe we need to remove This asset from Unity before removing serialized data
    // ONLY!!! validate after chnages are moved to GIT

    [SerializeField] [HideInInspector] public Color k_colour_foul,        // v1.6: ( 1.2, 0.0, 0.0, 1.0 )
                                                    k_colour_default,     // v1.6: ( 1.0, 1.0, 1.0, 1.0 )
                                                    k_colour_off = new Color(0.01f, 0.01f, 0.01f, 1.0f);

    [SerializeField] [HideInInspector] public Color k_teamColour_spots,   // v1.6: ( 0.00, 0.75, 1.75, 1.0 )
                                                    k_teamColour_stripes; // v1.6: ( 1.75, 0.25, 0.00, 1.0 )

    [SerializeField] [HideInInspector] public Color k_colour4Ball_team_0, // v1.6: ( )
                                                    k_colour4Ball_team_1; // v1.6: ( 2.0, 1.0, 0.0, 1.0 )

    [SerializeField] [HideInInspector] public Color k_fabricColour_8ball, // v1.6: ( 0.3, 0.3, 0.3, 1.0 )
                                                    k_fabricColour_9ball, // v1.6: ( 0.1, 0.6, 1.0, 1.0 )
                                                    k_fabricColour_4ball; // v1.6: ( 0.15, 0.75, 0.3, 1.0 )

    [SerializeField] [HideInInspector] public Texture[] textureSets;
    [SerializeField] [HideInInspector] public ModelData[] tableModels;
    [SerializeField] [HideInInspector] public Texture2D[] tableSkins;
    [SerializeField] [HideInInspector] public Texture snookerTexture;
    [SerializeField] [HideInInspector] public Transform[] coloredPositions;
    [SerializeField] [HideInInspector] public UdonSharpBehaviour cameraModule;
    [SerializeField] [HideInInspector] public AudioClip snd_Intro,
                                                        snd_Sink,
                                                        snd_NewTurn,
                                                        snd_PointMade,
                                                        snd_btn,
                                                        snd_spin,
                                                        snd_spinstop,
                                                        snd_hitball;
    [SerializeField] [HideInInspector] public Texture2D[] cueSkins;
    [SerializeField] [HideInInspector] public GameObject guideline,
                                                         devhit,
                                                         markerObj,
                                                         marker9ball;
    [SerializeField] [HideInInspector] public CueController[] cueControllers;
    [SerializeField] [HideInInspector] public GameObject[] balls;
    [NonSerialized] public uint ballsPocketedLocal,
                                fourBallCueBallLocal;
    [SerializeField] Text infReset; // Text under reset button
    #endregion
    #region ParametersMovedToOtherClassesButNoClueWhatItIs

    #endregion
    #region ToeBeMovedNotSureWhere
    [NonSerialized] public const int PERF_MAX = 6;
    [NonSerialized] public float repoMaxX;
    #endregion
    #region ToeBeRemoved
    [NonSerialized] public ManagerController managers;
    #endregion


    // Most likely it is possible to just delete those
    #region NonSerialized
    // Move to rules
    // Validate indexing

    [NonSerialized] public uint gameModeLocal,
                                timerLocal,
                                teamIdLocal,
                                teamColorLocal,
                                winningTeamLocal,
                                previewWinningTeamLocal;

    [NonSerialized] public int activeCueSkin,
                               tableSkinLocal,
                               PERF_MAIN = 0,
                               PERF_PHYSICS_MAIN = 1,
                               PERF_PHYSICS_VEL = 2,
                               PERF_PHYSICS_BALL = 3,
                               PERF_PHYSICS_CUSHION = 4,
                               PERF_PHYSICS_POCKET = 5;

    // table model properties
    [NonSerialized] public float k_TABLE_WIDTH, // horizontal span of table
                                 k_TABLE_HEIGHT, // vertical span of table
                                 k_CUSHION_RADIUS, // The roundess of colliders
                                 k_POCKET_RADIUS, // Full diameter of pockets
                                 k_INNER_RADIUS; // Pocket 'hitbox' cylinder

    [NonSerialized] public Vector3 k_vE, // corner pocket data
                                   k_vF, // side pocket data
                                   k_rack_position = new Vector3(),
                                   k_rack_direction = new Vector3();

    [NonSerialized] public GameObject[] pockets;
    [NonSerialized] public GameObject auto_pocketblockers;
    [NonSerialized] public Transform table;
    [NonSerialized] public AudioSource aud_main;
    [NonSerialized] public UdonBehaviour callbacks;
    [NonSerialized] public CueController activeCue;
    [NonSerialized] public CameraOverrideModule cameraOverrideModule;
    [NonSerialized] public Vector3[][] initialPositions = new Vector3[5][];
    #endregion

    #region Private
    private readonly Color k_aimColour_aim = new Color(0.7f, 0.7f, 0.7f, 1.0f),
                           k_aimColour_locked = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private const int LOG_MAX = 32;

    private const float k_BALL_RADIUS = 0.03f,
                        k_BALL_DIAMETRE = 0.06f,
                        k_BALL_PL_X = 0.03f, // break placement X
                        k_BALL_PL_Y = 0.05196152422f, // sin(60) * 0.06
                        k_RANDOMIZE_F = 0.0001f,
                        k_SPOT_POSITION_X = 0.5334f, // First X position of the racked balls
                        k_SPOT_CAROM_X = 0.8001f; // Spot position for carom mode

    private GameObject auto_rackPosition;
    private GameObject auto_colliderBaseVFX;

    private uint[] initialBallsPocketed = new uint[5];

    private string[] LOG_LINES = new string[32];
    private string[] perfNames = new string[] {
      "main",
      "physics",
      "physicsVel",
      "physicsBall",
      "physicsCushion",
      "physicsPocket"
   };

    private float[] perfCounters = new float[PERF_MAX],
                    perfTimings = new float[PERF_MAX],
                    perfStart = new float[PERF_MAX];

    private byte gameStateLocal = byte.MaxValue,
                 turnStateLocal = byte.MaxValue;

    private int timerStartLocal,
                tableModelLocal,
                firstHit = 0,
                secondHit = 0,
                thirdHit = 0;

    private uint repositionStateLocal;

    private bool isLocalSimulationOurs = false,
                 fbMadePoint = false,
                 fbMadeFoul = false;
    #endregion


    // Here/GameManager?
    // TODO: verify, simplify, explain
    private void OnEnable()
    {
        playerManager.Init(this);
        logger.Init(this, managers.networkingManager);
        logger._LogInfo("initializing billiards module");

        cameraOverrideModule = (CameraOverrideModule)_GetModule(nameof(CameraOverrideModule));

        initializeRack();

        resetCachedData();

        managers.updateCurrentPhysics();

        setTableModel(0, false);

        aud_main = this.GetComponent<AudioSource>();

        for (int i = 0; i < game.table.balls.balls.Length; i++)
            game.table.balls.balls[i].GetComponentInChildren<Repositioner>(true)._Init(this, i);

        managers._Init(this);

        managers.currentPhysicsManager.SendCustomEvent("_InitConstants");


#if HT8B_DEBUGGER
        this.transform.Find("debugger").gameObject.SetActive(true);
#endif

        this.transform.Find("intl.balls/guide/guide_display").GetComponent<MeshRenderer>().material.SetMatrix("_BaseTransform", this.transform.worldToLocalMatrix);

        reflection_main.RenderProbe();

#if UNITY_EDITOR
        managers.graphicsManager._OnGameStarted();
        managers.menuManager.menuSettings.transform.localScale = Vector3.zero;
#endif
    }

    // GameManager?
    // TODO: verify
    private void FixedUpdate()
    {
        managers.currentPhysicsManager.SendCustomEvent("_FixedTick");
    }

    // GameManager?
    // TODO: verify, simplify, explain
    private void Update()
    {
        managers.networkingManager._Tick();

        managers.desktopManager._Tick();
        managers.menuManager._Tick();

        logger._BeginPerf(PERF_MAIN);
        managers.practiceManager._Tick();
        managers.repositionManager._Tick();
        managers.cameraManager._Tick();
        managers.graphicsManager._Tick();
        tickTimer();
        _Update9BallMarker();

        managers.networkingManager._FlushBuffer();
        logger._EndPerf(PERF_MAIN);

        if (perfCounters[PERF_MAIN] % 500 == 0) logger._RedrawDebugger();
    }

    // GameManager?
    // TODO: verify
    // Looks like we can just call it right away where we need it
    // Its a "global" function (?)
    // public override void OnPlayerLeft(VRCPlayerApi player)
    // {
    //     playerManager.OnPlayerLeft(player);
    // }

    // wat?
    // TODO: verify
    public UdonSharpBehaviour _GetModule(string type)
    {
        string[] parts = game.table.cameraModule.GetUdonTypeName().Split('.');
        if (parts[parts.Length - 1] == type)
        {
            return game.table.cameraModule;
        }
        return null;
    }

    #region Triggers
    // GameManager?
    // TODO: verify
    public void _TriggerLobbyOpen()
    {
        if (game.lobbyOpen) return;

        managers.networkingManager._OnLobbyOpened();
    }

    // GameManager?
    // TODO: verify
    public void _TriggerLobbyClosed()
    {
        managers.networkingManager._OnLobbyClosed();
    }

    // GameManager?
    // TODO: verify
    public void _TriggerTeamsChanged(bool teamsEnabled)
    {
        managers.networkingManager._OnTeamsChanged(teamsEnabled);
    }

    // GameManager?
    // TODO: verify
    public void _TriggerNoGuidelineChanged(bool noGuidelineEnabled)
    {
        managers.networkingManager._OnNoGuidelineChanged(noGuidelineEnabled);
    }

    // GameManager?
    // TODO: verify
    public void _TriggerNoLockingChanged(bool noLockingEnabled)
    {
        managers.networkingManager._OnNoLockingChanged(noLockingEnabled);
    }

    // GameManager?
    // TODO: verify
    public void _TriggerTimerChanged(uint timerSelected)
    {
        managers.networkingManager._OnTimerChanged(timerSelected);
    }

    // GameManager?
    // TODO: verify
    public void _TriggerGameModeChanged(uint newGameMode)
    {
        managers.networkingManager._OnGameModeChanged(newGameMode);
    }

    // SettingsManager?
    // TODO: verify
    public void _TriggerGlobalSettingsUpdated(string newTournamentReferee, int newTableModel, int newTableSkin)
    {
        managers.networkingManager._OnGlobalSettingsChanged(newTournamentReferee, (byte)newTableModel, (byte)newTableSkin);
    }

    // CueManager/BallManager/PhysicsManager?
    // TODO: verify
    public void _TriggerCueBallHit()
    {
        if (playerManager.localTeamId != teamIdLocal && !game.table.isPracticeMode) return; // is there a better way to do this?

        logger._LogWarn("trying to propagate cue ball hit, linear velocity is " + game.table.balls.ballsV[0].ToString("F4") + " and angular velocity is " + game.table.balls.ballsW[0].ToString("F4"));

        if (float.IsNaN(game.table.balls.ballsV[0].x) || float.IsNaN(game.table.balls.ballsV[0].y) || float.IsNaN(game.table.balls.ballsV[0].z) || float.IsNaN(game.table.balls.ballsW[0].x) || float.IsNaN(game.table.balls.ballsW[0].y) || float.IsNaN(game.table.balls.ballsW[0].z))
        {
            game.table.balls.ballsV[0] = Vector3.zero;
            game.table.balls.ballsW[0] = Vector3.zero;
            return;
        }

        _TriggerCueDeactivate();

        managers.networkingManager._OnHitBall(game.table.balls.ballsV[0], game.table.balls.ballsW[0]);
    }

    // CueManager?
    // TODO: verify
    public void _TriggerCueActivate()
    {
        if (!playerManager.isOurTurn()) return;

        if (Vector3.Distance(activeCue._GetCuetip().transform.position, game.table.balls.ballsP[0]) < k_BALL_RADIUS)
        {
            _TriggerCueDeactivate();
            return;
        }

        game.table.canHitCueBall = true;
        this._TriggerOnPlayerPrepareShoot();

#if !HT_QUEST
        this.transform.Find("intl.balls/guide/guide_display").GetComponent<MeshRenderer>().material.SetColor("_Colour", k_aimColour_locked);
#endif
    }

    // CueManager?
    // TODO: verify
    public void _TriggerCueDeactivate()
    {
        game.table.canHitCueBall = false;

#if !HT_QUEST
        game.table.cues.guideline.gameObject.transform.Find("guide_display").GetComponent<MeshRenderer>().material.SetColor("_Colour", k_aimColour_aim);
#endif
    }

    // CueManager?
    // TODO: verify
    public void _OnPickupCue()
    {
        if (!Networking.LocalPlayer.IsUserInVR()) managers.desktopManager._OnPickupCue();
    }

    // CueManager?
    // TODO: verify
    public void _OnDropCue()
    {
        if (!Networking.LocalPlayer.IsUserInVR()) managers.desktopManager._OnDropCue();
    }

    // GameManager?
    // TODO: verify
    public void _TriggerOnPlayerPrepareShoot()
    {
        managers.networkingManager._OnPlayerPrepareShoot();
    }

    // GameManager?
    // TODO: verify
    public void _OnPlayerPrepareShoot()
    {
        managers.cameraManager._OnPlayerPrepareShoot();
    }

    // GameManager?
    // TODO: verify
    public void _TriggerPlaceBall(int idx)
    {
        if (!game.table.canPlayLocal) return; // in case player was forced to drop ball since someone else took the shot

        // practiceManager._Record();

        bool consumeReposition = false;
        if (idx == 0)
        {
            managers.currentPhysicsManager.SendCustomEvent("_IsCueBallTouching");
            bool isTouching = (bool)managers.currentPhysicsManager.GetProgramVariable("outIsTouching");

            consumeReposition = !isTouching;
        }

        managers.networkingManager._OnRepositionBalls(game.table.balls.ballsP, consumeReposition);
    }

    // GameManager?
    // TODO: verify
    public void _TriggerGameStart()
    {
        logger._LogYes("starting game");

        managers.networkingManager._OnGameStart(initialBallsPocketed[gameModeLocal], initialPositions[gameModeLocal]);
    }

    // GameManager/PlayerManager?
    // TODO: verify, simplify
    public void _TriggerJoinTeam(int teamId)
    {
        if (playerManager.localPlayerId != -1) return;

        logger._LogInfo("joining team " + teamId);

        playerManager.localPlayerId = managers.networkingManager._OnJoinTeam(teamId);
        if (playerManager.localPlayerId != -1)
        {
            playerManager.localTeamId = (uint)(playerManager.localPlayerId & 0x1u);

            playerManager.playerNamesLocal[playerManager.localPlayerId] = Networking.LocalPlayer.displayName;
            managers.menuManager._RefreshLobbyOpen();
            managers.menuManager._RefreshPlayerList();
        }
        else
        {
            logger._LogWarn("failed to join team " + teamId + ", did someone else beat you to it?");
        }
    }

    // GameManager?
    // TODO: verify
    public void _TriggerLeaveLobby()
    {
        if (playerManager.localPlayerId == -1) return;

        logger._LogInfo("leaving lobby");
        
        managers.networkingManager._OnLeaveLobby(playerManager.localPlayerId);
        playerManager.playerNamesLocal[playerManager.localPlayerId] = "";
        playerManager.localPlayerId = -1;
        playerManager.localTeamId = 0;
        managers.menuManager._RefreshLobbyOpen();
        managers.menuManager._RefreshPlayerList();
    }

    // GameManager?
    // TODO: verify, simplify
    public void _TriggerGameReset()
    {
        string self = Networking.LocalPlayer.displayName;

        if (!game.gameLive)
        {
            if (game.lobbyOpen && playerManager._IsModerator(Networking.LocalPlayer))
            {
                managers.networkingManager._OnLobbyClosed();
            }
            return;
        }

        string[] allowedPlayers = playerManager.playerNamesLocal;
        if (!string.IsNullOrEmpty(playerManager.tournamentRefereeLocal))
        {
            allowedPlayers = new string[] { playerManager.tournamentRefereeLocal };
        }

        bool allPlayersOffline = true;
        bool isAllowedPlayer = false;
        foreach (string allowedPlayer in allowedPlayers)
        {
            if (allPlayersOffline && playerManager._GetPlayerByName(allowedPlayer) != null) allPlayersOffline = false;

            if (allowedPlayer == self) isAllowedPlayer = true;
        }

        if (allPlayersOffline || isAllowedPlayer || playerManager._IsModerator(Networking.LocalPlayer))
        {
            logger._LogInfo("force resetting game");

            managers.networkingManager._OnGameReset();
        }
        else
        {
            string playerStr = "";
            bool has = false;
            foreach (string allowedPlayer in allowedPlayers)
            {
                if (string.IsNullOrEmpty(allowedPlayer)) continue;
                if (has) playerStr += ", ";
                has = true;

                playerStr += managers.graphicsManager._FormatName(allowedPlayer);
            }

            game.table.infReset.text = "Only these players may reset:\n" + playerStr;
        }
    }
    #endregion

    #region NetworkingClient
    // NetworkManager?
    // TODO: verify, explain
    // the order is important, unfortunately
    public void _OnRemoteDeserialization()
    {
        logger._LogInfo("processing latest remote state (packet=" + managers.networkingManager.packetIdSynced + ", state=" + managers.networkingManager.stateIdSynced + ")");
        Debug.Log("[BilliardsModule] latest game state is " + managers.networkingManager._EncodeGameState());

        // propagate game settings first
        onRemoteGlobalSettingsUpdated(
            managers.networkingManager.tournamentRefereeSynced,
            managers.networkingManager.tableModelSynced,
            managers.networkingManager.tableSkinSynced
        );
        onRemoteGameSettingsUpdated(
            managers.networkingManager.gameModeSynced,
            managers.networkingManager.timerSynced,
            managers.networkingManager.teamsSynced,
            managers.networkingManager.noGuidelineSynced,
            managers.networkingManager.noLockingSynced
        );

        // propagate valid players second
        onRemotePlayersChanged(managers.networkingManager.playerNamesSynced);

        // apply state transitions if needed
        onRemoteGameStateChanged(managers.networkingManager.gameStateSynced);

        // now update game state
        onRemoteBallPositionsChanged(managers.networkingManager.ballsPSynced);
        onRemoteTeamIdChanged(managers.networkingManager.teamIdSynced);
        onRemoteFourBallCueBallChanged(managers.networkingManager.fourBallCueBallSynced);
        onRemoteBallsPocketedChanged(managers.networkingManager.ballsPocketedSynced);
        onRemoteFourBallScoresUpdated(managers.networkingManager.fourBallScoresSynced);
        onRemoteRepositionStateChanged(managers.networkingManager.repositionStateSynced);
        onRemoteIsTableOpenChanged(managers.networkingManager.isTableOpenSynced, managers.networkingManager.teamColorSynced);
        onRemoteTurnStateChanged(managers.networkingManager.turnStateSynced);
        onRemotePreviewWinningTeamChanged(managers.networkingManager.previewWinningTeamSynced);

        // finally, take a snapshot
        managers.practiceManager._Record();

        logger.redrawDebugger();
    }

    // SettingsManager?
    // TODO: verify, simplify
    private void onRemoteGlobalSettingsUpdated(string tournamentRefereeSynced, byte tableModelSynced, byte tableSkinSynced)
    {
        if (game.gameLive) return;

        if (
            playerManager.tournamentRefereeLocal == tournamentRefereeSynced &&
            tableModelLocal == tableModelSynced &&
            tableSkinLocal == tableSkinSynced
        )
        {
            return;
        }
        logger._LogInfo($"onRemoteGlobalSettingsUpdated tournamentReferee={tournamentRefereeSynced} tableModel={tableModelSynced} tableSkin={tableSkinSynced}");

        if (playerManager.tournamentRefereeLocal != tournamentRefereeSynced)
        {
            playerManager.tournamentRefereeLocal = tournamentRefereeSynced;
        }

        if (tableModelLocal != tableModelSynced)
        {
            setTableModel(tableModelSynced, true);
        }

        if (tableSkinLocal != tableSkinSynced)
        {
            tableSkinLocal = tableSkinSynced;
            managers.graphicsManager._UpdateTableColorScheme();
        }
    }

    // SettingsManager?
    // TODO: verify, simplify
    private void onRemoteGameSettingsUpdated(uint gameModeSynced, uint timerSynced, bool teamsSynced, bool noGuidelineSynced, bool noLockingSynced)
    {
        if (gameModeLocal == gameModeSynced &&
            timerLocal == timerSynced &&
            game.table.teamsLocal == teamsSynced &&
            game.table.common.guideLineEnabledLocal == noGuidelineSynced &&
            game.table.common.lockingEnabledLocal == noLockingSynced) return;

        logger._LogInfo($"onRemoteGameSettingsUpdated gameMode={gameModeSynced} timer={timerSynced} teams={teamsSynced} guideline={!noGuidelineSynced} locking={!noLockingSynced}");

        if (gameModeLocal != gameModeSynced)
        {
            gameModeLocal = gameModeSynced;

            game.table.is8Ball = gameModeLocal == 0u;
            game.table.is9Ball = gameModeLocal == 1u;
            game.table.isJp4Ball = gameModeLocal == 2u;
            game.table.isKr4Ball = gameModeLocal == 3u;
            game.table.isSnooker6Red = gameModeLocal == 4u;
            game.table.is4Ball = game.table.isJp4Ball || game.table.isKr4Ball;

            managers.menuManager._RefreshGameMode();
        }

        if (timerLocal != timerSynced)
        {
            timerLocal = timerSynced;

            managers.menuManager._RefreshTimer();
        }

        bool refreshToggles = false;
        setToggle(ref game.table.teamsLocal, teamsSynced, ref refreshToggles);
        setToggle(ref game.table.common.guideLineEnabledLocal, noGuidelineSynced, ref refreshToggles);
        setToggle(ref game.table.common.lockingEnabledLocal, noLockingSynced, ref refreshToggles);

        if (refreshToggles)
            managers.menuManager._RefreshToggleSettings();
    }
    private void setToggle(ref bool localBool, bool syncedBool, ref bool refreshToggles )
    {
        if (localBool == syncedBool) return;

        localBool = syncedBool;
        refreshToggles = true;
    }

    // GameManager?
    // TODO: verify
    private void onRemotePlayersChanged(string[] playerNamesSynced)
    {
        playerManager.onRemotePlayersChanged(playerNamesSynced);
    }

    // GameManager?
    // TODO: verify
    private void onRemoteGameStateChanged(byte gameStateSynced)
    {
        if (gameStateLocal == gameStateSynced) return;

        gameStateLocal = gameStateSynced;
        logger._LogInfo($"onRemoteGameStateChanged newState={gameStateSynced}");
        switch (gameStateSynced)
        {
            case 0:
                onRemoteLobbyClosed();
                break;
            case 1:
                onRemoteLobbyOpened();
                break;
            case 2:
                onRemoteGameStarted();
                break;
            case 3:
                onRemoteGameEnded(managers.networkingManager.winningTeamSynced);
                break;
        }
    }

    // GameManager?
    // TODO: verify
    private void onRemoteLobbyOpened()
    {
        logger._LogInfo($"onRemoteLobbyOpened");

        game.lobbyOpen = true;
        managers.graphicsManager._OnLobbyOpened();
        managers.menuManager._RefreshLobbyOpen();
        managers.menuManager._RefreshPlayerList();

        if (callbacks != null) callbacks.SendCustomEvent("_OnLobbyOpened");
    }

    // GameManager?
    // TODO: verify
    private void onRemoteLobbyClosed()
    {
        logger._LogInfo($"onRemoteLobbyClosed");

        game.lobbyOpen = false;
        playerManager.localPlayerId = -1;
        managers.graphicsManager._OnLobbyClosed();
        managers.menuManager._RefreshLobbyOpen();

        resetCachedData();

        if (callbacks != null) callbacks.SendCustomEvent("_OnLobbyClosed");
    }

    // GameManager/RuleManager
    // TODO: verify, simplify
    private void onRemoteGameStarted()
    {
        logger._LogInfo($"onRemoteGameStarted");

        game.lobbyOpen = false;
        game.gameLive = true;

        Array.Clear(perfCounters, 0, PERF_MAX);
        Array.Clear(perfStart, 0, PERF_MAX);
        Array.Clear(perfTimings, 0, PERF_MAX);

        game.table.isPracticeMode = playerManager.playerNamesLocal[1] == "" && playerManager.playerNamesLocal[3] == "";

        managers.menuManager._DisableMenu();

        managers.graphicsManager._OnGameStarted();
        managers.desktopManager._OnGameStarted();
        applyCueAccess(false);
        managers.practiceManager._Clear();
        managers.repositionManager._OnGameStarted();
        if (game.table.isPracticeMode)
            cueControllers[1].gameObject.SetActive(false);

        Array.Clear(game.table.rule4Ball.scoresLocal, 0, 2);
        auto_pocketblockers.SetActive(game.table.is4Ball);
        game.table.balls.marker9ball.SetActive(game.table.is9Ball);

        managers.graphicsManager._ShowBalls();

        // Reflect game state
        managers.graphicsManager._UpdateScorecard();
        game.table.isReposition = false;
        game.table.balls.markerObj.SetActive(false);

        // Effects
        managers.graphicsManager._PlayIntroAnimation();
        aud_main.PlayOneShot(audio.snd_Intro, 1.0f);

        managers.graphicsManager._SetScorecardPlayers(playerManager.playerNamesLocal);

        game.table.timerRunning = false;

        reflection_main.RenderProbe();

        activeCue = cueControllers[0];
    }

    // TableManager/PhysicManager
    // TODO: verify, simplify
    private void onRemoteBallPositionsChanged(Vector3[] ballsPSynced)
    {
        if (game.table.balls.ballsP.Equals(ballsPSynced)) return;

        logger._LogInfo($"onRemoteBallPositionsChanged");

        Array.Copy(ballsPSynced, game.table.balls.ballsP, game.table.balls.ballsP.Length);
    }


    // GameManager/RuleManager
    // TODO: verify, simplify
    private void onRemotePreviewWinningTeamChanged(uint previewWinningTeamSynced)
    {
        if (!game.gameLive) return;
        if (string.IsNullOrEmpty(playerManager.tournamentRefereeLocal)) return;

        if (previewWinningTeamLocal == previewWinningTeamSynced) return;

        logger._LogInfo($"onRemotePreviewWinningTeamChanged winningTeam={previewWinningTeamSynced}");
        previewWinningTeamLocal = previewWinningTeamSynced;

        if (previewWinningTeamSynced == 2)
            managers.graphicsManager._ResetWinners();
        else
            managers.graphicsManager._SetWinners(game.table.isPracticeMode ? 0u : previewWinningTeamSynced, playerManager.playerNamesLocal);
    }

    // TableManager/RuleManager
    // TODO: verify, simplify
    private void onRemoteGameEnded(uint winningTeamSynced)
    {
        logger._LogInfo($"onRemoteGameEnded winningTeam={winningTeamSynced}");

        game.isLocalSimulationRunning = false;

        if (playerManager.IsLocalPlayerReferee())
        {
            // tournament mode has some special logic
            if (winningTeamSynced != 2u)
            {
                return;
            }

            winningTeamLocal = previewWinningTeamLocal;
        }
        else
        {
            winningTeamLocal = winningTeamSynced;
        }

        if (winningTeamLocal == 2)
        {
            winningTeamLocal = 0;

            game.table.isTableOpenLocal = true;
            logger._LogWarn("game reset");
            managers.graphicsManager._OnGameReset();
        }
        else
        {
            logger._LogWarn("game over, team " + winningTeamLocal + " won (" + playerManager.playerNamesLocal[winningTeamLocal] + " and " + playerManager.playerNamesLocal[winningTeamLocal + 2] + ")");
            managers.graphicsManager._SetWinners(game.table.isPracticeMode ? 0u : winningTeamLocal, playerManager.playerNamesLocal);
        }

        game.gameLive = false;

        managers.graphicsManager._UpdateTeamColor(winningTeamSynced);
        managers.graphicsManager._UpdateScorecard();
        managers.graphicsManager._RackBalls();

        disablePlayComponents();

        this.transform.Find("intl.controls/undo").gameObject.SetActive(false);
        this.transform.Find("intl.controls/redo").gameObject.SetActive(false);
        this.transform.Find("intl.controls/skipturn").gameObject.SetActive(false);

        // Remove any access rights
        playerManager.localPlayerId = -1;
        playerManager.localTeamId = 0;
        applyCueAccess(true);

        resetCachedData();

        cueControllers[1].gameObject.SetActive(true);

        managers.menuManager._EnableMenu();

        game.table.infReset.text = "Reset";
    }

    // TableManager
    // TODO: verify
    private void onRemoteBallsPocketedChanged(uint ballsPocketedSynced)
    {
        if (!game.gameLive) return;

        // todo: actually use a separate variable to track local modifications to balls pocketed
        if (game.table.balls.ballsPocketedLocal != ballsPocketedSynced) logger._LogInfo($"onRemoteBallsPocketedChanged ballsPocketed={ballsPocketedSynced:X}");

        game.table.balls.ballsPocketedLocal = ballsPocketedSynced;

        managers.graphicsManager._UpdateScorecard();
        managers.graphicsManager._RackBalls();

        refreshBallPickups();
    }

    // TableManager/RuleManager
    // TODO: verify
    private void onRemoteFourBallScoresUpdated(int[] fbScoresSynced)
    {
        if (!game.gameLive) return;

        if (game.table.rule4Ball.scoresLocal[0] == fbScoresSynced[0] && game.table.rule4Ball.scoresLocal[1] == fbScoresSynced[1]) return;

        logger._LogInfo($"onRemoteFourBallScoresUpdated team1={fbScoresSynced[0]} team2={fbScoresSynced[1]}");

        Array.Copy(fbScoresSynced, game.table.rule4Ball.scoresLocal, 2);
        managers.graphicsManager._UpdateScorecard();
    }

    // PlayerManager
    // TODO: verify
    private void onRemoteTeamIdChanged(uint teamIdSynced)
    {
        if (!game.gameLive) return;

        if (teamIdLocal == teamIdSynced) return;

        logger._LogInfo($"onRemoteTeamIdChanged newTeam={teamIdSynced}");
        teamIdLocal = teamIdSynced;

        aud_main.PlayOneShot(audio.snd_NewTurn, 1.0f);

        managers.graphicsManager._UpdateTeamColor(teamIdLocal);

        // always use first cue if practice mode
        activeCue = cueControllers[game.table.isPracticeMode ? 0 : (int)teamIdLocal];
    }

    // TableManager/RuleManager
    // TODO: verify
    private void onRemoteFourBallCueBallChanged(uint fourBallCueBallSynced)
    {
        if (!game.gameLive) return;
        if (!game.table.is4Ball) return;

        if (game.table.balls.fourBallCueBallLocal == fourBallCueBallSynced) return;

        logger._LogInfo($"onRemoteFourBallCueBallChanged cueBall={fourBallCueBallSynced}");
        game.table.balls.fourBallCueBallLocal = fourBallCueBallSynced;

        managers.graphicsManager._UpdateFourBallCueBallTextures(game.table.balls.fourBallCueBallLocal);
    }

    // TableManager
    // TODO: verify
    private void onRemoteIsTableOpenChanged(bool isTableOpenSynced, uint teamColorSynced)
    {
        if (!game.gameLive) return;

        if (teamColorLocal == teamColorSynced && game.table.isTableOpenLocal == isTableOpenSynced) return;

        logger._LogInfo($"onRemoteIsTableOpenChanged isTableOpen={isTableOpenSynced} teamColor={teamColorSynced}");
        game.table.isTableOpenLocal = isTableOpenSynced;
        teamColorLocal = teamColorSynced;

        if (!game.table.isTableOpenLocal)
        {
            string color = (teamIdLocal ^ teamColorLocal) == 0 ? "blues" : "oranges";
            logger._LogInfo($"table closed, team {teamIdLocal} is {color}");
        }

        managers.graphicsManager._UpdateTeamColor(teamIdLocal);
        managers.graphicsManager._UpdateScorecard();
    }

    // TableManager
    // TODO: verify
    private void onRemoteColorTurnChanged(bool ColorTurnSynced)
    {
        if (!game.gameLive) return;

        if (game.table.colorTurnLocal == ColorTurnSynced) return;

        logger._LogInfo($"onRemoteColorTurnChanged colorTurn={ColorTurnSynced}");
        game.table.colorTurnLocal = ColorTurnSynced;
    }
    // TableManager/PhysicsManager
    // TODO: verify, it's way too complicated for maintenace
    private void onRemoteRepositionStateChanged(uint repositionStateSynced)
    {
        if (!game.gameLive) return;

        if (repositionStateLocal == repositionStateSynced) return;

        logger._LogInfo($"onRemoteRepositionStateChanged repositionState={repositionStateSynced}");
        repositionStateLocal = repositionStateSynced;

        if (!playerManager.isOurTurn() || repositionStateLocal == 0)
        {
            game.table.isReposition = false;
            setFoulPickupEnabled(false);
            return;
        }

        if (repositionStateLocal == 1 || repositionStateLocal == 2)
        {
            game.table.isReposition = true;
            if (repositionStateLocal == 1)
            {
                repoMaxX = -k_SPOT_POSITION_X;
            }
            else
            {
                Vector3 k_pR = (Vector3)managers.currentPhysicsManager.GetProgramVariable("k_pR");
                repoMaxX = k_pR.x;
            }
            setFoulPickupEnabled(true);
        }
    }

    // TableManager
    // TODO: verify
    private void onRemoteTurnBegin(int timerStartSynced)
    {
        logger._LogInfo("onRemoteTurnBegin");
        game.table.canPlayLocal = true;
        timerStartLocal = timerStartSynced;

        enablePlayComponents();
        Array.Clear(game.table.balls.ballsV, 0, game.table.balls.ballsV.Length);
        Array.Clear(game.table.balls.ballsW, 0, game.table.balls.ballsW.Length);
    }

    // TableManager/BallManager/PhysicsManager
    // TODO: verify, it's way too complicated for maintenace
    private void onRemoteTurnSimulate(Vector3 cueBallV, Vector3 cueBallW, string simulationOwner)
    {
        logger._LogInfo($"onRemoteTurnSimulate cueBallV={cueBallV.ToString("F4")} cueBallW={cueBallW.ToString("F4")} owner={simulationOwner}");

        game.table.balls.balls[0].GetComponent<AudioSource>().PlayOneShot(audio.snd_hitball, 1.0f);

        game.table.canPlayLocal = false;
        disablePlayComponents();

        if (!playerManager._IsPlayer(Networking.LocalPlayer) && !table.GetComponent<MeshRenderer>().isVisible)
        {
            // don't bother simulating if the table isn't even visible
            logger._LogWarn("skipping simulation");
            return;
        }

        game.isLocalSimulationRunning = true;
        firstHit = 0;
        secondHit = 0;
        thirdHit = 0;
        fbMadePoint = false;
        fbMadeFoul = false;
        game.isBreak = false;
        game.table.balls.ballsPocketedOrig = game.table.balls.ballsPocketedLocal;
        if (Networking.LocalPlayer.displayName == simulationOwner)
        {
            isLocalSimulationOurs = true;
        }

        for (int i = 0; i < game.table.balls.ballsV.Length; i++)
        {
            game.table.balls.ballsV[i] = Vector3.zero;
            game.table.balls.ballsW[i] = Vector3.zero;
        }
        game.table.balls.ballsV[0] = cueBallV;
        game.table.balls.ballsW[0] = cueBallW;

        auto_colliderBaseVFX.SetActive(true);
    }

    // TableManager
    private void onRemoteTurnStateChanged(byte turnStateSynced)
    {
        if (!game.gameLive) return;

        if (turnStateSynced == turnStateLocal) return;

        logger._LogInfo($"onRemoteTurnStateChanged newState={turnStateSynced}");
        turnStateLocal = turnStateSynced;

        if (turnStateLocal == 0 || turnStateLocal == 2)
        {
            if (turnStateLocal == 2) turnStateLocal = 0; // synthetic state

            onRemoteTurnBegin(managers.networkingManager.timerStartSynced);
            // practiceManager._Record();
        }
        else if (turnStateLocal == 1)
        {
            onRemoteTurnSimulate(managers.networkingManager.cueBallVSynced, managers.networkingManager.cueBallWSynced, managers.networkingManager.simulationOwnerSynced);
            // practiceManager._Record();
        }
        else
        {
            game.table.canPlayLocal = false;
            disablePlayComponents();
        }
    }
    #endregion

    #region PhysicsEngineCallbacks
    // TableManager/RulesManager/PhysicsManager
    // TODO: verify, it's way too complicated for maintenace
    public void _TriggerCollision(int srcId, int dstId)
    {
        if (dstId < srcId)
        {
            int tmp = dstId;
            dstId = srcId;
            srcId = dstId;
        }
        if (srcId != 0) return;

        switch (gameModeLocal)
        {
            case 0:
            case 1:
                if (firstHit == 0) firstHit = dstId;
                break;
            case 2:
                if (firstHit == 0)
                {
                    firstHit = dstId;
                    break;
                }
                if (secondHit == 0)
                {
                    if (dstId != firstHit)
                    {
                        secondHit = dstId;
                        handle4BallHit(game.table.balls.ballsP[dstId], true);
                    }
                    break;
                }
                if (thirdHit == 0)
                {
                    if (dstId != firstHit && dstId != secondHit)
                    {
                        thirdHit = dstId;
                        handle4BallHit(game.table.balls.ballsP[dstId], true);
                    }
                    break;
                }
                break;
            case 3:
                if (dstId == 13)
                {
                    handle4BallHit(game.table.balls.ballsP[dstId], false);
                    break;
                }
                if (firstHit == 0)
                {
                    firstHit = dstId;
                    break;
                }
                if (secondHit == 0)
                {
                    if (dstId != firstHit)
                    {
                        secondHit = dstId;
                        handle4BallHit(game.table.balls.ballsP[dstId], true);
                    }
                    break;
                }
                break;
            case 4:
                if (firstHit == 0)
                    firstHit = dstId;
                break;
        }
    }

    // TableManager/RulesManager
    // TODO: verify, it's way too complicated for maintenace
    public void _TriggerPocketBall(int id)
    {
        uint total = 0U;

        // Get total for X positioning
        int count_extent = game.table.is9Ball ? 10 : (game.table.isSnooker6Red ? 13: 16);
        for (int i = 1; i < count_extent; i++)
        {
            total += (game.table.balls.ballsPocketedLocal >> i) & 0x1U;
        }

        // place ball on the rack
        game.table.balls.ballsP[id] = k_rack_position + (float)total * k_BALL_DIAMETRE * k_rack_direction;

        game.table.balls.ballsPocketedLocal ^= 1U << id;

        uint bmask = 0x1FCU << ((int)(teamIdLocal ^ teamColorLocal) * 7);


        if (game.table.isSnooker6Red)
        {
            bool foulCondition = false;
            int pocketedBallTypes = sixRedCheckBallTypesPocketed(),
                nextColor = sixRedFindLowestUnpocketedColor(game.table.balls.ballsPocketedLocal);
            bmask = game.table.colorTurnLocal ? (!game.table.rule6Reds.redsOnTable ? (uint)(1 << game.table.rule6Reds.ballOrder[nextColor]) : 0x1AE) : (!game.table.rule6Reds.redsOnTable ? (uint)(1 << game.table.rule6Reds.ballOrder[nextColor]) : 0x1E50u);
            foulCondition = ((game.table.rule6Reds.redsOnTable && pocketedBallTypes == 0 && game.table.colorTurnLocal) ||
                (game.table.rule6Reds.redsOnTable && pocketedBallTypes > 0 && !game.table.colorTurnLocal) ||
                (!game.table.rule6Reds.redsOnTable && firstHit != game.table.rule6Reds.ballOrder[nextColor]) ||
                (!game.table.rule6Reds.redsOnTable && (game.table.balls.ballsPocketedOrig & 0x1AE) < (game.table.balls.ballsPocketedLocal & (0x1AE - bmask))));
            if (!foulCondition)
                managers.graphicsManager._FlashTableLight();
            else
                managers.graphicsManager._FlashTableError();
        }
        else
        {
            if (((0x1U << id) & ((bmask) | (game.table.isTableOpenLocal ? 0xFFFCU : 0x0000U) | ((bmask & game.table.balls.ballsPocketedLocal) == bmask ? 0x2U : 0x0U))) > 0)
            {
                managers.graphicsManager._FlashTableLight();
            }
            else
            {
                managers.graphicsManager._FlashTableError();
            }
        }
        aud_main.PlayOneShot(audio.snd_Sink, 1.0f);

#if !HT_QUEST

        // VFX ( make ball move )
        Rigidbody body = game.table.balls.balls[id].GetComponent<Rigidbody>();
        body.isKinematic = false;
        body.velocity = this.transform.TransformVector(new Vector3(
           game.table.balls.ballsV[id].x,
           0.0f,
           game.table.balls.ballsV[id].z
        ));

#else
        game.table.balls.balls[id].transform.localPosition = game.table.balls.ballsP[id];
#endif
    }

    // TableManager
    // TODO: verify, it's way too complicated for maintenace
    public void _TriggerSimulationEnded(bool forceScratch)
    {
        if (!game.isLocalSimulationRunning) return;
        game.isLocalSimulationRunning = false;

        logger._LogInfo("local simulation completed");
        managers.cameraManager._OnLocalSimEnd();
        auto_colliderBaseVFX.SetActive(false);

        // Make sure we only run this from the client who initiated the move
        if (isLocalSimulationOurs)
        {
            isLocalSimulationOurs = false;

            uint bmask = 0xFFFCu;
            uint emask = 0x0u;

            // Quash down the mask if table has closed
            if (!game.table.isTableOpenLocal)
            {
                bmask = bmask & (0x1FCu << ((int)(teamIdLocal ^ teamColorLocal) * 7));
                emask = 0x1FCu << ((int)(teamIdLocal ^ teamColorLocal ^ 0x1U) * 7);
            }

            // Common informations
            bool isSetComplete = (game.table.balls.ballsPocketedLocal & bmask) == bmask;
            bool isScratch = (game.table.balls.ballsPocketedLocal & 0x1U) == 0x1U || forceScratch;

            game.table.balls.ballsPocketedLocal = game.table.balls.ballsPocketedLocal & ~(0x1U);
            if (isScratch) game.table.balls.ballsP[0] = Vector3.zero;
            // Append black to mask if set is done
            if (isSetComplete)
            {
                bmask |= 0x2U;
            }

            // These are the resultant states we can set for each mode
            // then the rest is taken care of
            bool
               isObjectiveSink,
               isOpponentSink,
               winCondition,
               foulCondition,
               deferLossCondition;

            if (game.table.is8Ball)
            {
                isObjectiveSink = (game.table.balls.ballsPocketedLocal & bmask) > (game.table.balls.ballsPocketedOrig & bmask);
                isOpponentSink = (game.table.balls.ballsPocketedLocal & emask) > (game.table.balls.ballsPocketedOrig & emask);

                // Calculate if objective was not hit first
                bool isWrongHit = ((0x1U << firstHit) & bmask) == 0;
                bool is8Sink = (game.table.balls.ballsPocketedLocal & 0x2U) == 0x2U;

                if (is8Sink && game.table.isPracticeMode)
                {
                    is8Sink = false;

                    game.table.balls.ballsPocketedLocal = game.table.balls.ballsPocketedLocal & ~(0x2U);
                    game.table.balls.ballsP[1] = Vector3.zero;
                }

                winCondition = isSetComplete && is8Sink;
                foulCondition = isScratch || isWrongHit;

                deferLossCondition = is8Sink;
            }
            else if (game.table.is9Ball)
            {
                // Rules are from: https://www.youtube.com/watch?v=U0SbHOXCtFw

                // Rule #1: Cueball must strike the lowest number ball, first
                bool isWrongHit = !(findLowestUnpocketedBall(game.table.balls.ballsPocketedOrig) == firstHit);

                // Rule #2: Pocketing cueball, is a foul

                // Win condition: Pocket 9 ball ( at anytime )
                winCondition = (game.table.balls.ballsPocketedLocal & 0x200u) == 0x200u;

                // this video is hard to follow so im just gonna guess this is right
                isObjectiveSink = (game.table.balls.ballsPocketedLocal & 0x3FEu) > (game.table.balls.ballsPocketedOrig & 0x3FEu);

                isOpponentSink = false;
                deferLossCondition = false;

                foulCondition = isWrongHit || isScratch;

                // TODO: Implement rail contact requirement
            }
            else if (game.table.is4Ball)
            {
                isObjectiveSink = fbMadePoint;
                isOpponentSink = fbMadeFoul;
                foulCondition = false;
                deferLossCondition = false;

                winCondition = game.table.rule4Ball.scoresLocal[teamIdLocal] >= 10;
            }
            else /* if (isSnooker) */
            {
                game.table.rule6Reds.redsOnTable = sixRedCheckIfRedOnTable(game.table.balls.ballsPocketedOrig);

                bool redOnTableOrColorTurn = game.table.rule6Reds.redsOnTable || game.table.colorTurnLocal,
                     allBallsPocketed = ((game.table.balls.ballsPocketedLocal & 0x1FFEu) == 0x1FFEu),
                     myTeamWinning;

                int numBallsPocketed = 0,
                    ballScore = 0,
                    highestPocketedBallScore = 0,
                    nextColor = sixRedFindLowestUnpocketedColor(game.table.balls.ballsPocketedOrig);

                uint objective = game.table.colorTurnLocal ? 0x1AE : (game.table.rule6Reds.redsOnTable ? 0x1E50u : (uint)(1 << game.table.rule6Reds.ballOrder[nextColor]));

                isOpponentSink = false;

                isObjectiveSink = (game.table.balls.ballsPocketedLocal & (objective)) > (game.table.balls.ballsPocketedOrig & (objective));

                sixRedScoreBallsPocketed(ref ballScore, ref numBallsPocketed, ref highestPocketedBallScore);

                foulCondition = isSixRedFoul(objective, isScratch, nextColor);

                bool isMyScoreValid = game.table.rule4Ball.scoresLocal[teamIdLocal] <= 200;
                bool isEnemyScoreValid = game.table.rule4Ball.scoresLocal[1 - teamIdLocal] <= 200;
                if (foulCondition)
                    game.table.rule4Ball.scoresLocal[1 - teamIdLocal] += isEnemyScoreValid ? Math.Max(ballScore, 4) : 0;
                else
                    game.table.rule4Ball.scoresLocal[teamIdLocal] += isMyScoreValid ? ballScore : 0;

                if (redOnTableOrColorTurn || foulCondition)
                    sixRedReturnColoredBalls(foulCondition ? nextColor : 6);

                if (isScratch)
                    game.table.balls.ballsP[0] = initialPositions[4][0];

                game.table.colorTurnLocal = (game.table.rule6Reds.redsOnTable && isObjectiveSink && !foulCondition) ? !game.table.colorTurnLocal : false;
                game.table.rule6Reds.redsOnTable = sixRedCheckIfRedOnTable(game.table.balls.ballsPocketedLocal);

                myTeamWinning = game.table.rule4Ball.scoresLocal[teamIdLocal] > game.table.rule4Ball.scoresLocal[1 - teamIdLocal];

                winCondition = myTeamWinning && allBallsPocketed;
                if (winCondition) foulCondition = false;
                deferLossCondition = allBallsPocketed && !myTeamWinning;

                logger._LogInfo($"6RED: TeamScore 0: {game.table.rule4Ball.scoresLocal[0]}\n6RED: TeamScore 1: {game.table.rule4Ball.scoresLocal[1]}");
            }
            managers.networkingManager._OnSimulationEnded(game.table.balls.ballsP, game.table.balls.ballsPocketedLocal, game.table.rule4Ball.scoresLocal, game.table.colorTurnLocal, game.table.rule6Reds.redsOnTable);

            if (winCondition)
            {
                if (foulCondition)
                {
                    // Loss
                    onLocalTeamWin(teamIdLocal ^ 0x1U);
                }
                else
                {
                    // Win
                    onLocalTeamWin(teamIdLocal);
                }
            }
            else if (deferLossCondition)
            {
                // Loss
                onLocalTeamWin(teamIdLocal ^ 0x1U);
            }
            else if (foulCondition)
            {
                // Foul
                onLocalTurnFoul();
            }
            else if (isObjectiveSink && !isOpponentSink)
            {
                // Continue
                onLocalTurnContinue();
            }
            else
            {
                // Pass
                onLocalTurnPass();
            }
        }
    }
    #endregion

    #region sixRedSnooker
    // RuleManager
    private bool isSixRedFoul(uint objective, bool isScratch, int nextColor)
    {
        int firsthittype = sixRedCheckFirstHit(firstHit),
            pocketedBallTypes = sixRedCheckBallTypesPocketed();

        return (game.table.rule6Reds.redsOnTable && firsthittype == 0 && game.table.colorTurnLocal) ||
                (game.table.rule6Reds.redsOnTable && firsthittype == 1 && !game.table.colorTurnLocal) ||
                (game.table.rule6Reds.redsOnTable && pocketedBallTypes == 0 && game.table.colorTurnLocal) ||
                (game.table.rule6Reds.redsOnTable && pocketedBallTypes > 0 && !game.table.colorTurnLocal) ||
                (!game.table.rule6Reds.redsOnTable && firstHit != game.table.rule6Reds.ballOrder[nextColor] && !game.table.colorTurnLocal) ||
                (!game.table.rule6Reds.redsOnTable && (game.table.balls.ballsPocketedOrig & 0x1AE) < (game.table.balls.ballsPocketedLocal & (0x1AE - objective)) && !game.table.colorTurnLocal) ||
                (firsthittype == -1) ||
                (isScratch);
    }
    // RuleManager
    public string sixRedNumberToColor(int ball)
    {
        string[] ballColors = new string[]
        {
            "Yellow",
            "Green",
            "Brown",
            "Blue",
            "Pink",
            "Black",
            "Red"
        };
        return (ball - 6) >= 0 && (ball - 6) < ballColors.Length ? ballColors[ball - 6] : "Red";
    }
    // RuleManager
    public int sixRedFindLowestUnpocketedColor(uint field)
    {
        for (int i = 6; i < game.table.rule6Reds.ballOrder.Length; i++)
        {
            bool isLowestUnpocketed = ((field >> game.table.rule6Reds.ballOrder[i]) & 0x1U) == 0x00U;
            if (isLowestUnpocketed)
                return i;
        }
        return -1;
    }
    // RuleManager
    public bool sixRedCheckIfRedOnTable(uint field)
    {
        for (int i = 0; i < 6; i++)
            if (((field >> game.table.rule6Reds.ballOrder[i]) & 0x1U) == 0x00U)
                return true;
        return false;
    }
    // RuleManager
    public int sixRedCheckFirstHit(int firstHit)
    {
        uint firstHitball = (uint)1 << firstHit;
        if ((firstHitball & 0x1E50u) > 0)
            return 0;
        if ((firstHitball & 0x1AE) > 0)
            return 1;
        return -1;
    }
    // RuleManager
    public void sixRedReturnColoredBalls(int from)
    {
        for (int i = from; i < game.table.rule6Reds.ballOrder.Length; i++)
        {
            bool isBallInBreakOrder = (game.table.balls.ballsPocketedLocal & (1 << game.table.rule6Reds.ballOrder[i])) > 0;

            if (!isBallInBreakOrder)
                continue;

            sixRedMoveBallUntilNotTouching(game.table.rule6Reds.ballOrder[i]);
            game.table.balls.ballsPocketedLocal = game.table.balls.ballsPocketedLocal ^ (1u << game.table.rule6Reds.ballOrder[i]);
        }
    }
    // RuleManager
    public void sixRedScoreBallsPocketed(ref int ballScore, ref int highestScoringBall, ref int numBallsPocketed)
    {
        for (int i = 1; i < 13; i++)
        {
            bool isBallPocketted =  (game.table.balls.ballsPocketedLocal & (1 << i)) > (game.table.balls.ballsPocketedOrig & (1 << i));

            if (!isBallPocketted)
                continue;

            if (highestScoringBall < game.table.rule6Reds.ballPoints[i])
                highestScoringBall = game.table.rule6Reds.ballPoints[i];

            ballScore += game.table.rule6Reds.ballPoints[i];
            numBallsPocketed++;
        }
    }
    // RuleManager/BallManager
    public int sixRedCheckBallTypesPocketed()
    {
        bool redBallPocket = (game.table.balls.ballsPocketedOrig & 0x1E50u) <  (game.table.balls.ballsPocketedLocal & 0x1E50u);
        bool coloredBallPocket = (game.table.balls.ballsPocketedOrig & 0x1AE) <  (game.table.balls.ballsPocketedLocal & 0x1AE);
        bool bothPocket = (coloredBallPocket && redBallPocket);

        return bothPocket ? 2 : (coloredBallPocket ? 1 : (redBallPocket ? 0 : -1));
    }
    // RuleManager/BallManager
    private void sixRedMoveBallUntilNotTouching(int Ball)
    {
        game.table.balls.ballsP[Ball] = initialPositions[4][Ball];
        int blockingBall = CheckIfBallTouchingBall(Ball);
        if (CheckIfBallTouchingBall(Ball) < 0)
            return;

        for (int i = game.table.rule6Reds.ballOrder.Length - 1; i > 5; i--)
        {
            game.table.balls.ballsP[Ball] = initialPositions[4][game.table.rule6Reds.ballOrder[i]];
            if (CheckIfBallTouchingBall(Ball) < 0)
                return;
        }

        game.table.balls.ballsP[Ball] = initialPositions[4][Ball];
        Vector3 moveDir = game.table.balls.ballsP[Ball] - game.table.balls.ballsP[blockingBall];
        moveDir.y = 0;
        if (moveDir.sqrMagnitude == 0)
            moveDir = Vector3.left;

        moveDir = moveDir.normalized;
        moveBallInDirUntilNotTouching(Ball, moveDir * k_BALL_RADIUS * .051f);
    }
    // BallManager
    private void moveBallInDirUntilNotTouching(int Ball, Vector3 Dir)
    {
        while (CheckIfBallTouchingBall(Ball) > 0)
            game.table.balls.ballsP[Ball] += Dir;
    }
    // BallManager
    private int CheckIfBallTouchingBall(int Input)
    {
        bool isNotCorrectBall;
        bool isBallTouching;
        float ballDiameter = k_BALL_RADIUS * 2f;
        float k_BALL_DSQR = ballDiameter * ballDiameter;
        for (int i = 1; i < 16; i++)
        {
            isNotCorrectBall = ( (game.table.balls.ballsPocketedLocal >> i) & 0x1u) == 0x1u || i == Input;
            if (isNotCorrectBall)
                continue;

            isBallTouching = (game.table.balls.ballsP[Input] - game.table.balls.ballsP[i]).sqrMagnitude < k_BALL_DSQR;
            if (isBallTouching)
                return i;
        }
        return -1;
    }
    #endregion

    #region GameLogic
    // TODO: verify TableManager?
    private void initializeRack()
    {
        _RackStart();
        _8BallRackLogic();
        _9BallRackLogic();
        _4BallRackLogic(true);
        _4BallRackLogic(false);
        _SnookerRackLogic();
    }

    // TableManager/RuleManager?
    private void _RackStart()
    {
        for (int i = 0; i < 5; i++)
        {
            initialPositions[i] = new Vector3[16];
            for (int j = 0; j < 16; j++)
            {
                initialPositions[i][j] = Vector3.zero;
            }

            // cue ball always starts here (unless four ball, but we override below)
            initialPositions[i][0] = new Vector3(-k_SPOT_POSITION_X, 0.0f, 0.0f);
        }
    }

    // TableManager/RuleManager?
    private void _4BallRackLogic(bool isJP)
    {
        if (isJP)
        {
            initialBallsPocketed[2] = 0x1FFEu;
            initialPositions[2][0] = new Vector3(-k_SPOT_CAROM_X, 0.0f, 0.0f);
            initialPositions[2][13] = new Vector3(k_SPOT_CAROM_X, 0.0f, 0.0f);
            initialPositions[2][14] = new Vector3(k_SPOT_POSITION_X, 0.0f, 0.0f);
            initialPositions[2][15] = new Vector3(-k_SPOT_POSITION_X, 0.0f, 0.0f);
        }
        else
        {
            initialBallsPocketed[3] = initialBallsPocketed[2];
            initialPositions[3] = initialPositions[2];
        }
    }

    // TableManager/RuleManager?
    private void _8BallRackLogic()
    {
        // 8 ball
        initialBallsPocketed[0] = 0x00u;
        makeTriangle(k_SPOT_POSITION_X, 0, game.table.rule8Ball.ballOrder, 5);
    }

    // TableManager/RuleManager?
    private void _9BallRackLogic()
    {
        // 9 ball
        initialBallsPocketed[1] = 0xFC00u;
        int rown;
        for (int i = 0, k = 0; i < 5; i++)
        {
            rown = game.table.rule9Ball.breakRows[i];
            for (int j = 0; j <= rown; j++)
            {
                initialPositions[1][game.table.rule9Ball.ballOrder[k++]] = new Vector3
                (
                   k_SPOT_POSITION_X + i * k_BALL_PL_Y + UnityEngine.Random.Range(-k_RANDOMIZE_F, k_RANDOMIZE_F),
                   0.0f,
                   (-rown + j * 2) * k_BALL_PL_X + UnityEngine.Random.Range(-k_RANDOMIZE_F, k_RANDOMIZE_F)
                );
            }
        }
    }

    // TableManager/RuleManager?
    private void _SnookerRackLogic()
    {
        // Snooker
        initialBallsPocketed[4] = 0xE000u;

        initialPositions[4][1] = game.table.balls.getSnookerPositionV3(SnookerPositionE.BLACK);
        initialPositions[4][5] = game.table.balls.getSnookerPositionV3(SnookerPositionE.PINK);
        initialPositions[4][2] = game.table.balls.getSnookerPositionV3(SnookerPositionE.YELLOW);
        initialPositions[4][7] = game.table.balls.getSnookerPositionV3(SnookerPositionE.GREEN);
        initialPositions[4][8] = game.table.balls.getSnookerPositionV3(SnookerPositionE.BROWN);
        initialPositions[4][0] = game.table.balls.getSnookerPositionV3(SnookerPositionE.CUE);
                                                                                                                            //triangle
        float rackStartSnooker = game.table.balls.getSnookerPositionV3(SnookerPositionE.CUE).x;
        makeTriangle(rackStartSnooker, 4, game.table.rule6Reds.ballOrder, 3);
    }

    // TableManager/RuleManager?
    private void makeTriangle(float initialPosition, int gameMode, int[] breakOrder, int rows)
    {
        for (int i = 0, k = 0; i < rows; i++)// change 3 to 5 for 15 balls (rows)
        {
            for (int j = 0; j <= i; j++)
            {
                initialPositions[gameMode][breakOrder[k++]] = new Vector3
                (
                   initialPosition + i * k_BALL_PL_Y,
                   0.0f,
                   (-i + j * 2) * k_BALL_PL_X
                );
            }
        }
    }

    // TableManager?
    private void resetCachedData()
    {
        for (int i = 0; i < 4; i++)
        {
            playerManager.playerNamesLocal[i] = "";
        }
        repositionStateLocal = 0;
        gameModeLocal = uint.MaxValue;
        turnStateLocal = byte.MaxValue;
        previewWinningTeamLocal = 2;
    }

    // MenuManager?
    private void setTransform(Transform src, Transform dest, float sf)
    {
        dest.position = src.position;
        dest.rotation = src.rotation;
        dest.localScale = src.localScale * sf;
    }

    // TableModule/TableConfigurationData?
    private void setTableModel(int newTableModel, bool update)
    {
        tableModels[tableModelLocal].gameObject.SetActive(false);
        tableModels[newTableModel].gameObject.SetActive(true);

        tableModelLocal = newTableModel;

        ModelData data = tableModels[tableModelLocal];
        k_TABLE_WIDTH = data.tableWidth;
        k_TABLE_HEIGHT = data.tableHeight;
        k_CUSHION_RADIUS = data.cushionRadius;
        k_POCKET_RADIUS = data.pocketRadius;
        k_INNER_RADIUS = data.innerRadius;
        k_vE = data.cornerPocket;
        k_vF = data.sidePocket;
        pockets = data.pockets;

        Transform table_base = _GetTableBase().transform;
        auto_pocketblockers = table_base.Find(".4BALL_FILL").gameObject;
        auto_rackPosition = table_base.Find(".RACK").gameObject;
        auto_colliderBaseVFX = table_base.Find("collision.vfx").gameObject;

        Transform transformSurface = (Transform)managers.currentPhysicsManager.GetProgramVariable("transform_Surface");
        k_rack_position = transformSurface.InverseTransformPoint(auto_rackPosition.transform.position);
        k_rack_direction = transformSurface.InverseTransformDirection(auto_rackPosition.transform.up);

        table = table_base.Find("table");
        if (update)
        {
            managers.currentPhysicsManager.SendCustomEvent("_InitConstants");
            managers.graphicsManager._InitializeTable();
        }

        Transform menu_transform = this.transform.Find("intl.menu");
        setTransform(table_base.Find(".MENU"), menu_transform, 1.0f);
        menu_transform.transform.position += menu_transform.right * 0.4f;

        Transform score_info_root = this.transform.Find("intl.scorecardinfo");
        setTransform(table_base.Find(".NAME_0"), score_info_root.Find("player0-name").gameObject.GetComponent<RectTransform>(), 1F / 200F);
        setTransform(table_base.Find(".NAME_1"), score_info_root.Find("player1-name").gameObject.GetComponent<RectTransform>(), 1F / 200F);

        // todo: reposition cues
    }

    // TableModule/TableConfigurationData?
    public GameObject _GetTableBase()
    {
        return tableModels[tableModelLocal].transform.Find("table_artwork").gameObject;
    }

    // TableModule/RulesModule?
    private void handle4BallHit(Vector3 loc, bool good)
    {
        if (good)
            handle4BallHitGood(loc);
        else
            handle4BallHitBad(loc);
        managers.graphicsManager._SpawnFourBallPoint(loc, good);
        managers.graphicsManager._UpdateScorecard();
    }

    // TableModule/RulesModule?
    private void handle4BallHitGood(Vector3 p)
    {
        fbMadePoint = true;
        aud_main.PlayOneShot(audio.snd_PointMade, 1.0f);

        game.table.rule4Ball.scoresLocal[teamIdLocal]++;
        if (game.table.rule4Ball.scoresLocal[teamIdLocal] > 10) game.table.rule4Ball.scoresLocal[teamIdLocal] = 10;
    }

    // TableModule/RulesModule?
    private void handle4BallHitBad(Vector3 p)
    {
        if (fbMadeFoul) return;
        fbMadeFoul = true;

        game.table.rule4Ball.scoresLocal[teamIdLocal]--;
        if (game.table.rule4Ball.scoresLocal[teamIdLocal] < 0) game.table.rule4Ball.scoresLocal[teamIdLocal] = 0;
    }

    // TableModule?
    private void onLocalTeamWin(uint winner)
    {
        logger._LogInfo($"onLocalTeamWin {(winner)}");

        if (string.IsNullOrEmpty(playerManager.tournamentRefereeLocal))
            managers.networkingManager._OnGameWin(winner);
        else
            managers.networkingManager._OnPreviewWinner(winner);
    }

    // TableModule?
    private void onLocalTurnPass()
    {
        logger._LogInfo($"onLocalTurnPass");

        managers.networkingManager._OnTurnPass(teamIdLocal ^ 0x1u);
    }

    // TableModule?
    private void onLocalTurnFoul()
    {
        logger._LogInfo($"onLocalTurnFoul");

        managers.networkingManager._OnTurnFoul(teamIdLocal ^ 0x1u);
    }

    // TableModule?
    private void onLocalTurnContinue()
    {
        logger._LogInfo($"onLocalTurnContinue");

        // try and close the table if possible
        if (game.table.is8Ball && game.table.isTableOpenLocal)
        {
            uint sink_orange = 0;
            uint sink_blue = 0;
            uint pmask = game.table.balls.ballsPocketedLocal >> 2;

            for (int i = 0; i < 7; i++)
            {
                if ((pmask & 0x1u) == 0x1u)
                    sink_blue++;

                pmask >>= 1;
            }
            for (int i = 0; i < 7; i++)
            {
                if ((pmask & 0x1u) == 0x1u)
                    sink_orange++;

                pmask >>= 1;
            }

            if (sink_blue != sink_orange)
            {
                teamColorLocal = (sink_blue > sink_orange) ? teamIdLocal : teamIdLocal ^ 0x1u;
                managers.networkingManager._OnTableClosed(teamColorLocal);
            }
        }

        managers.networkingManager._OnTurnContinue();
    }

    // TODO: verify TableModule?
    private void onLocalTimerEnd()
    {
        game.table.timerRunning = false;

        logger._LogWarn("out of time!");

        managers.graphicsManager._HideTimers();

        if (!playerManager.IsLocalPlayerReferee())
        {
            // no one is allowed to play
            game.table.canPlayLocal = false;

            if (playerManager.isOurTurn())
            {
                // everyone on the current team propagates the change
                onLocalTurnFoul();
            }
        }
    }

    // CueManager?
    private void applyCueAccess(bool gameOver)
    {
        switch (playerManager.localPlayerId)
        {
            case -1:
                cueControllers[0]._Disable(gameOver);
                cueControllers[1]._Disable(gameOver);
                break;
            case 0:
                cueControllers[0]._Enable(gameOver);
                cueControllers[1]._Disable(gameOver);
                break;
            default:
                cueControllers[1]._Enable(gameOver);
                cueControllers[0]._Disable(gameOver);
                break;
        }
    }

    // TableManager?
    // turn on any game elements that are enabled when someone is taking a shot
    private void enablePlayComponents()
    {
        bool isOurTurnVar = playerManager.isOurTurn();

        if ((isOurTurnVar && game.table.isPracticeMode) || playerManager.IsLocalPlayerReferee())
        {
            this.transform.Find("intl.controls/undo").gameObject.SetActive(true);
            this.transform.Find("intl.controls/redo").gameObject.SetActive(true);
            this.transform.Find("intl.controls/skipturn").gameObject.SetActive(true);
        }

        if (game.table.is9Ball)
        {
            game.table.balls.marker9ball.SetActive(true);
            _Update9BallMarker();
        }

        refreshBallPickups();

        if (isOurTurnVar)
        {
            // Update for desktop
            managers.desktopManager._AllowShoot();
        }
        else
        {
            managers.desktopManager._DenyShoot();
        }

        if (timerLocal > 0)
        {
            game.table.timerRunning = true;
            managers.graphicsManager._ShowTimers();
        }
    }

    // TableManager?
    public void _SkipTurn()
    {
        if (game.table.isPracticeMode || playerManager.IsLocalPlayerReferee())
        {
            onLocalTurnFoul();
        }
    }

    // BallManager?
    public void _Update9BallMarker()
    {
        if (game.table.balls.marker9ball.activeSelf)
        {
            int target = findLowestUnpocketedBall (game.table.balls.ballsPocketedLocal);
            game.table.balls.marker9ball.transform.localPosition = game.table.balls.ballsP[target];
        }
    }

    // TODO: verify
    // turn off any game elements that are enabled when someone is taking a shot
    private void disablePlayComponents()
    {
        game.table.balls.marker9ball.SetActive(false);
        setFoulPickupEnabled(false);
        refreshBallPickups();
        game.table.cues.devhit.SetActive(false);
        game.table.cues.guideline.SetActive(false);
        game.table.common.isGuidelineValid = false;
        game.table.isReposition = false;

        managers.desktopManager._DenyShoot();
        managers.graphicsManager._HideTimers();
    }

    // BallManager?
    public int findLowestUnpocketedBall(uint field)
    {
        for (int i = 2; i <= 8; i++)
        {
            if (((field >> i) & 0x1U) == 0x00U)
                return i;
        }

        if (((field) & 0x2U) == 0x00U)
            return 1;

        for (int i = 9; i < 16; i++)
        {
            if (((field >> i) & 0x1U) == 0x00U)
                return i;
        }

        // ??
        return 0;
    }

    // TableManager?
    private void setBallPickupActive(int ballId, bool active)
    {
        Transform pickup =  game.table.balls.balls[ballId].transform.GetChild(0);

        pickup.gameObject.SetActive(active);
        pickup.GetComponent<SphereCollider>().enabled = active;
        ((VRC_Pickup)pickup.GetComponent(typeof(VRC_Pickup))).pickupable = active;
        if (!active) ((VRC_Pickup)pickup.GetComponent(typeof(VRC_Pickup))).Drop();
    }

    // TableManager?
    private void refreshBallPickups()
    {
        bool canUsePickup = playerManager.IsPickupAllowed();

        uint ball_bit = 0x1u;
        for (int i = 0; i < game.table.balls.balls.Length; i++)
        {
            if (game.gameLive && (canUsePickup || (i == 0 && game.table.isReposition)) && game.table.canPlayLocal &&  (game.table.balls.ballsPocketedLocal & ball_bit) == 0x0u)
            {
                setBallPickupActive(i, true);
            }
            else
            {
                setBallPickupActive(i, false);
            }
            ball_bit <<= 1;
        }
    }

    // TableManager?
    private void setFoulPickupEnabled(bool enabled)
    {
        game.table.balls.markerObj.SetActive(enabled);
        if (enabled)
        {
            setBallPickupActive(0, true);
        }
        else if (!game.table.isPracticeMode && !playerManager.IsLocalPlayerReferee())
        {
            setBallPickupActive(0, false);
        }
    }

    // TableManager?
    private void tickTimer()
    {
        if (game.gameLive && game.table.timerRunning && game.table.canPlayLocal)
        {
            float timeRemaining = timerLocal - (Networking.GetServerTimeInMilliseconds() - timerStartLocal) / 1000.0f;
            float timePercentage = timeRemaining >= 0.0f ? 1.0f - (timeRemaining / timerLocal) : 0.0f;

            managers.graphicsManager._SetTimerPercentage(timePercentage);

            if (timeRemaining < 0.0f)
            {
                onLocalTimerEnd();
            }
        }
    }

    // TableManager?
    public void _IndicateError()
    {
        game.table.flashFoul();
    }

    // TableManager?
    public void _IndicateSuccess()
    {
        // nothing, for now
    }

    // TODO: analyze
    public string _SerializeGameState()
    {
        return managers.networkingManager._EncodeGameState();
    }

    public void _LoadSerializedGameState(string gameState)
    {
        if (string.IsNullOrEmpty(playerManager.tournamentRefereeLocal))
        {
            // no loading on top of other people's games
            if (!playerManager._IsPlayer(Networking.LocalPlayer)) return;

            // no loading outside of practice
            if (!game.table.isPracticeMode) return;
        }
        else
        {
            // only host can load on top of tournament
            if (!playerManager.IsLocalPlayerReferee()) return;
        }

        managers.networkingManager._OnLoadGameState(gameState);
        // practiceManager._Record();
    }

    // TODO: read about in memory states, what it is and how it works
    // This looks to be Pratice mode related. To redo. Make it clear what and how it works
    public object[] _SerializeInMemoryState()
    {
        Vector3[] positionClone = new Vector3[game.table.balls.ballsP.Length];
        Array.Copy(game.table.balls.ballsP, positionClone, game.table.balls.ballsP.Length);
        int[] scoresClone = new int[game.table.rule4Ball.scoresLocal.Length];
        Array.Copy(game.table.rule4Ball.scoresLocal, scoresClone, game.table.rule4Ball.scoresLocal.Length);
        return new object[15]
        {
            positionClone,
            game.table.balls.ballsPocketedLocal,
            scoresClone,
            gameModeLocal,
            teamIdLocal,
            repositionStateLocal,
            game.table.isTableOpenLocal,
            teamColorLocal,
            game.table.balls.fourBallCueBallLocal,
            turnStateLocal,
            managers.networkingManager.cueBallVSynced,
            managers.networkingManager.cueBallWSynced,
            managers.networkingManager.previewWinningTeamSynced,
            managers.networkingManager.colorTurnSynced,
            managers.networkingManager.redsOnTableSynced
        };
    }

    public void _LoadInMemoryState(object[] state, int stateIdLocal)
    {
        managers.networkingManager._ForceLoadFromState(
            stateIdLocal,
            (Vector3[])state[0],
            (uint)state[1],
            (int[])state[2],
            (uint)state[3],
            (uint)state[4],
            (uint)state[5],
            (bool)state[6],
            (uint)state[7],
            (uint)state[8],
            (byte)state[9],
            (Vector3)state[10],
            (Vector3)state[11],
            (byte)state[12],
            (bool)state[13],
            (bool)state[14]
        );
    }

    public bool _AreInMemoryStatesEqual(object[] a, object[] b)
    {
        // Use Equal or SequenceEqual from Array lib
        Vector3[] posA = (Vector3[])a[0];
        Vector3[] posB = (Vector3[])b[0];
        for (int i = 0; i < game.table.balls.ballsP.Length; i++) if (posA[i] != posB[i]) return false;

        int[] scoresA = (int[])a[2];
        int[] scoresB = (int[])b[2];
        for (int i = 0; i < game.table.rule4Ball.scoresLocal.Length; i++) if (scoresA[i] != scoresB[i]) return false;

        for (int i = 0; i < a.Length; i++) if (i != 0 && i != 2 && !a[i].Equals(b[i])) return false;

        return true;
    }

    #endregion
}
