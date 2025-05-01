
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Game;

namespace YAPT.PoolTable.Scoreboard
{
    // [KeywordEnum(Both, Left, Right)] _SolidsMode("Solids", Int)
    public enum SolidTeamE : int
    {
        BOTH = 0,
        LEFT = 1,
        RIGHT = 2
    }

    // [KeywordEnum(EightBall, FourBall, Empty)] _GameMode("Gamemode", Int)
    public enum ScorecardTypeE : int
    {
        EIGHT_BALL = 0,
        FOUR_BALL = 1,
        EMPTY = 2
    }

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
        private MaterialPropertyBlock propertyBlock;
        // Used to initialize scorecard colors
        private Vector4[] colorVectorBlack = new Vector4[SHADER_COLORS_SIZE];
        // Used to convert array to vector
        private Vector4[] colorVector = new Vector4[SHADER_COLORS_SIZE];

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
            // InitScorecardProperties(); // TODO: Not sure about this one in regards to new players
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
                    SetScorecardGameMode((int)ScorecardTypeE.EIGHT_BALL);
                    break;
                case GameModeType.NINE_BALL:
                    SetScorecardGameMode((int)ScorecardTypeE.EMPTY);
                    ballToSink.enabled = true;
                    break;
                case GameModeType.FOUR_BALL:
                    SetScorecardGameMode((int)ScorecardTypeE.FOUR_BALL);
                    SetScorecardColors(colors4Ball);
                    break;
                case GameModeType.SIX_REDS:
                    SetScorecardGameMode((int)ScorecardTypeE.EMPTY);
                    score0.enabled = true;
                    score1.enabled = true;
                    ballToSink.enabled = true;
                    break;
            }
            scorecard.SetPropertyBlock(propertyBlock);
        }

        public void SetScore8Ball(Color[] scoredBallsColors, SolidTeamE solidTeam)
        {
            SetScorecardSolids((int)solidTeam);
            SetScorecardColors(scoredBallsColors);
            scorecard.SetPropertyBlock(propertyBlock);
        }
        public void SetScore9Ball(string currentBallColor)
        {
            ballToSink.text = $"Ball to sink: {currentBallColor}";
        }
        public void SetScore4Ball(int team0, int team1)
        {
            score0.text = $"{team0}";
            score1.text = $"{team1}";
            scorecard.SetPropertyBlock(propertyBlock);
        }
        public void SetScoreSnooker(int team0, int team1, string currentBallColor)
        {
            score0.text = $"{team0}";
            score1.text = $"{team1}";
            ballToSink.text = $"Ball to sink: {currentBallColor}";
            scorecard.SetPropertyBlock(propertyBlock);
        }

        private void InitScorecardProperties()
        {
            // Set basic values
            ballToSink.text = "";
            score0.text = "0";
            score1.text = "0";
            // Set material properties
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
        }

        private void SetScorecardColors(Color[] scoreColors)
        {
            // Expect exactly 15 Colors
            if (scoreColors.Length != 15) return;
            // For 4 ball we only use scoreColors[0] and scoreColors[1]
            // For 8 ball we expect specific color order of pocketed balls
            updateColorVector(scoreColors); // set colorVector
            propertyBlock.SetVectorArray("_Color", colorVector);
        }

        private void SetScorecardScore(int team0, int team1)
        {
            propertyBlock.SetInt("_LeftScore", team0);
            propertyBlock.SetInt("_RightScore", team1);
        }

        private void SetScorecardSolids(int team)
        {
            //Both = 0, Left = 1, Right = 2
            propertyBlock.SetInt("_SolidsMode", team);
        }

        private void updateColorVector(Color[] colorArray)
        {
            for (int i = 0; i < SHADER_COLORS_SIZE; i++)
            {
                // Implicit conversion from Color to Vector4
                colorVector[i] = colorArray[i];
            }
        }

        public void FillPlayerNames(string[] playerNames)
        {
            // Print first team names
            if (playerNames[1] == "")
            {
                FillSinglePlayer(playerNames[0], team0);
            }
            else
            {
                FillTwoPlayer(playerNames[0], playerNames[1], team0);
            }
            // Print second team names
            if (playerNames[2] == "")
            {
                FillSinglePlayer(playerNames[2], team1);
            }
            else
            {
                FillTwoPlayer(playerNames[2], playerNames[3], team1);
            }
        }

        private void FillSinglePlayer(string player, Text uitext)
        {
            uitext.fontSize = FONT_SIZE_1_ROW;
            uitext.text = player;
        }
        private void FillTwoPlayer(string player0, string player1, Text uitext)
        {
            uitext.fontSize = FONT_SIZE_2_ROWS;
            uitext.text = $"{player0}\n{player1}";
        }

        #endregion // ScoreboardManager

        #region Testing


        public void Fiil3Players()
        {
            FillSinglePlayer("player 0", team0);
            FillTwoPlayer("player 2", "player 3", team1);

        }
        #endregion
    }
}