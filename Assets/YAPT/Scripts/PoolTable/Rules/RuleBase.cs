using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Game;
using YAPT.PoolTable.Balls;

namespace YAPT.PoolTable.Rules
{
    public class RuleBase : UdonSharpBehaviour
    {
        // [Improvement: Medium]
        // Divide rules to data and logic
        // Would allow for single initalization of const values for multiple tables
        // Currently data is initiated per table
        // [Improvement: Low]
        // Move Rules to Balls, there is too much overlaping data, to pretend it's separate
        protected GameModeType ruleType = GameModeType.INVALID;
        protected int MIN_BALL_ID = 0;
        protected int MAX_BALL_ID = 0;
        protected int TRIANGLE_BALL_ID = 0;
        protected int TRIANGLE_BALL_COUNT = 0;
        protected int[] ballsType = new int[BallManager.MAX_BALLS];
        protected Vector3[] ballInitialPositions = new Vector3[BallManager.MAX_BALLS];
        protected bool firstCollision = true;
        protected TeamTypeE team0 = TeamTypeE.INVALID;
        protected TeamTypeE team1 = TeamTypeE.INVALID;
        // Default ball values (7ft)
        private Vector3 cueBallPosition = new Vector3(-0.555f, 0, 0);
        private float triangleBallPosition = 0.385f;

        private const float BALL_PL_X = 0.03f, // break placement X
                            BALL_PL_Y = 0.05196152422f; // sin(60) * 0.06

        #region RuleBase
        // Unified functionalitya
        public void startShot() { firstCollision = false; }

        // Game initialization
        public virtual void SetVisuals(bool state) { }
        public virtual void ActivateBalls(GameObject[] balls)
        {
            // For rules we use consecutive sub array of ball objects
            // Disable balls before cue ball
            for (int i = 0; i < MIN_BALL_ID; i++)
            {
                balls[i].SetActive(false);
            }
            // Enable required balls
            for (int i = MIN_BALL_ID; i <= MAX_BALL_ID; i++)
            {
                balls[i].SetActive(true);
            }
            // Disable balls after the last required
            for (int i = MAX_BALL_ID + 1; i < balls.Length; i++)
            {
                balls[i].SetActive(false);
            }
        }
        public virtual void SetBallsPosition(GameObject[] balls)
        {
            for (int i = MIN_BALL_ID; i <= MAX_BALL_ID; i++)
            {
                balls[i].transform.localPosition = ballInitialPositions[i];
            }
        }
        public virtual Texture2D GetTexture() { return null; }

        // Game active
        public virtual void SetTeams(TeamTypeE _team0, TeamTypeE _team1) { }
        public virtual void UpdateScore() { }

        // Check if the action was valid
        public virtual bool OnCollisionValid(TeamTypeE activeTeam, int ball1, int ball2) { return true; }
        public virtual bool OnPocketedValid(TeamTypeE activeTeam, int ball) { return true; }

        // Uniform protected methods
        // Type depends on texture (or vice versa)
        protected virtual void SetBallTypes() { }
        // Get ball id from list, which starts a triangle
        protected virtual void SetInitialBallPositions()
        {
            // Default: Use first ID as cue ball
            ballInitialPositions[MIN_BALL_ID] = cueBallPosition;
            InitTrianglePositions();
        }

        protected virtual void InitTrianglePositions()
        {
            int maxRows = 5;
            int startIndex = TRIANGLE_BALL_ID;
            int stopIndex = startIndex + TRIANGLE_BALL_COUNT;
            int ballsId = startIndex;
            for (int row = 0; row < maxRows; row++)
            {
                for (int col = 0; col <= row; col++)
                {
                    // Force end if we reached last ball
                    if (ballsId >= stopIndex) return;
                    // TODO: have better understanding of what this do
                    ballInitialPositions[ballsId] =
                        new Vector3(
                            triangleBallPosition + row * BALL_PL_Y,
                            0.0f,
                            (-row + col * 2) * BALL_PL_X
                        );
                    ballsId++;
                }
            }
        }
        #endregion // RuleBase
    }
}