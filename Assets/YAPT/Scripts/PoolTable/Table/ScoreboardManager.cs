
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Table;

public class ScoreboardManager : UdonSharpBehaviour
{
    private const int FONT_SIZE_1_ROW = 12;
    private const int FONT_SIZE_2_ROWS = 6;
    private const int SHADER_COLORS_SIZE = 15;

    [Header("4 ball Colors")]
    // Those values are constant values
    // Could be moved somwhere else
    [SerializeField] private Color colorTeam0;
    [SerializeField] private Color colorTeam1;
    private Color[] colors4Ball = new Color[SHADER_COLORS_SIZE]; // Default black
    [Header("Team names")]
    [SerializeField] private Text team0;
    [SerializeField] private Text team1;
    [Header("Snooker")]
    [SerializeField] private Text score0;
    [SerializeField] private Text score1;
    [SerializeField] private Text ballToSink;
    [Header("Scorecard")]
    [SerializeField] private MeshRenderer scorecard;
    private Renderer objRenderer;
    private MaterialPropertyBlock propertyBlock;
    // Used to initialize scorecard colors
    private Vector4[] colorVectorBlack = new Vector4[SHADER_COLORS_SIZE];
    // Used to convert array to vector
    private Vector4[] colorVector = new Vector4[SHADER_COLORS_SIZE];
    private GameModeType gameType = GameModeType.EIGHT_BALL;

    #region UdonSharpBehaviour
    void Start()
    {
        // Initiate 4 ball colors
        colors4Ball[0] = colorTeam0;
        colors4Ball[1] = colorTeam1;

        // Initialize MaterialPropertyBlock
        // Using MaterialPropertyBlock allows to modify parameters of a material
        // without affecting other GameObjects
        propertyBlock = new MaterialPropertyBlock();
    }

    #endregion // UdonSharpBehaviour

    #region ScoreboardManager

    public void SelectScoreboard(GameModeType gameType)
    {
        // Disable current scoreboard elements
        score0.enabled = false;
        score1.enabled = false;
        ballToSink.enabled = false;

        // Pick new scoreboard
        InitScorecardProperties();
        switch (gameType)
        {
            case GameModeType.EIGHT_BALL:
                SetScorecardGameMode(0);
                break;
            case GameModeType.NINE_BALL:
                SetScorecardGameMode(2);
                ballToSink.enabled = true;
                break;
            case GameModeType.FOUR_BALL:
                SetScorecardGameMode(1);
                SetScorecardColors(colors4Ball);
                break;
            case GameModeType.SIX_REDS:
                SetScorecardGameMode(2);
                score0.enabled = true;
                score1.enabled = true;
                ballToSink.enabled = true;
                break;
        }
    }

    private void InitScorecardProperties()
    {
        propertyBlock.SetVectorArray("_Colors", colorVectorBlack);
        propertyBlock.SetInt("_LeftScore", 0);
        propertyBlock.SetInt("_RightScore", 0);
        // propertyBlock.SetInt("_GameMode", 0); // Keep current
        //Both = 0, Left = 1, Right = 2
        propertyBlock.SetInt("_SolidsMode", 0);
    }

    private void SetScorecardGameMode(int gameMode)
    {
        // Game mode parameters from yapt/Scoreboard.shader
        // 0 = EightBall, 1 = FourBall, 2 = Empty
        propertyBlock.SetInt("_GameMode", gameMode);
        scorecard.SetPropertyBlock(propertyBlock);
    }

    public void SetScorecardColors(Color[] scoreColors)
    {
        // Expect exactly 15 Colors
        if (scoreColors.Length != 15) return;
        // For 4 ball we only use scoreColors[0] and scoreColors[1]
        // For 8 ball we expect specific color order of pocketed balls
        updateColorVector(scoreColors); // set colorVector
        propertyBlock.SetVectorArray("_Color", colorVector);
        scorecard.SetPropertyBlock(propertyBlock);
    }

    public void SetScorecardScore(int team0, int team1)
    {
        propertyBlock.SetInt("_LeftScore", team0);
        propertyBlock.SetInt("_RightScore", team1);
        scorecard.SetPropertyBlock(propertyBlock);
    }

    public void SetScorecardSolids(int team)
    {
        //Both = 0, Left = 1, Right = 2
        propertyBlock.SetInt("_SolidsMode", team);
        scorecard.SetPropertyBlock(propertyBlock);
    }

    private void updateColorVector(Color[] colorArray)
    {
        for (int i = 0; i < SHADER_COLORS_SIZE; i++)
        {
            // Implicit conversion from Color to Vector4
            colorVector[i] = colorArray[i];
        }
    }

    public void Score8Ball(Color[] scoredBallsColors, int team)
    {
    }
    public void Score9Ball(int[] scoredBalls, int team)
    {
    }
    public void Score4Ball(int[] scores, int team)
    {
    }
    public void ScoreSnookerBall(int score, int team)
    {
    }

    public void fillPlayerNames(string[] playerNames)
    {
        // Print first team names
        if (playerNames[1] == "")
        {
            fillSinglePlayer(playerNames[0], team0);
        }
        else
        {
            fillTwoPlayer(playerNames[0], playerNames[1], team0);
        }
        // Print second team names
        if (playerNames[2] == "")
        {
            fillSinglePlayer(playerNames[2], team1);
        }
        else
        {
            fillTwoPlayer(playerNames[2], playerNames[3], team1);
        }
    }

    private void fillSinglePlayer(string player, Text uitext)
    {
        uitext.fontSize = FONT_SIZE_1_ROW;
        uitext.text = player;
    }
    private void fillTwoPlayer(string player0, string player1, Text uitext)
    {
        uitext.fontSize = FONT_SIZE_2_ROWS;
        uitext.text = $"{player0}\n{player1}";
    }

    #endregion // ScoreboardManager

    #region Testing


    public void Fiil3Players()
    {
        fillSinglePlayer("player 0", team0);
        fillTwoPlayer("player 2", "player 3", team1);

    }
    #endregion
}
