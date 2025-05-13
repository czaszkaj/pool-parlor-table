using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Game;

namespace YAPT.PoolTable.Rules.Snooker
{
    public enum BallTypeE
    {
        CUE = 0,
        RED = 1,
        YELLOW = 2,
        GREEN = 3,
        BROWN = 4,
        BLUE = 5,
        PINK = 6,
        BLACK = 7,
        ANY_COLOR = 8
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RuleSnooker : RuleBase
    {
        private const int NUMBER_OF_REDS = 6;
        [SerializeField] private Texture2D forceTexture;
        [SerializeField] protected GameObject startLine;

        private BallTypeE currentTarget;

        #region UdonSharpBehaviour
        void Start()
        {
            MIN_BALL_ID = 1;
            MAX_BALL_ID = 13;
            TRIANGLE_BALL_ID = 8; // first with RED type
            TRIANGLE_BALL_COUNT = 6; // Default 6 reds, to optimize gameplay
            SetBallTypes();
            currentTarget = BallTypeE.RED;
            SetInitialBallPositions();
        }
        #endregion // UdonSharpBehaviour

        #region RuleBase
        // Game initialization
        public override void SetVisuals(bool state)
        {
            startLine.SetActive(state);
        }
        protected override void SetInitialBallPositions()
        {
            base.SetInitialBallPositions();
            ballInitialPositions[1] = new Vector3(-0.715f, 0f, 0f); // shifted white ball
            ballInitialPositions[2] = new Vector3(-0.555f, 0f, -0.226f);
            ballInitialPositions[3] = new Vector3(-0.555f, 0f, 0.226f);
            ballInitialPositions[4] = new Vector3(-0.555f, 0f, 0f);
            ballInitialPositions[5] = new Vector3(0f, 0f, 0f);
            ballInitialPositions[6] = new Vector3(0.323f, 0f, 0f);
            ballInitialPositions[7] = new Vector3(0.782f, 0f, 0f);
        }
        public override Texture2D GetTexture() { return forceTexture; }
        protected override void SetBallTypes()
        {
            ballsType[1] = (int)BallTypeE.CUE;
            ballsType[2] = (int)BallTypeE.YELLOW;
            ballsType[3] = (int)BallTypeE.GREEN;
            ballsType[4] = (int)BallTypeE.BROWN;
            ballsType[5] = (int)BallTypeE.BLUE;
            ballsType[6] = (int)BallTypeE.PINK;
            ballsType[7] = (int)BallTypeE.BLACK;
            for (int i = 8; i <= MAX_BALL_ID; i++)
            {
                ballsType[i] = (int)BallTypeE.RED;
            }
        }

        // Check if the action was valid
        public override bool OnCollisionValid(TeamTypeE activeTeam, int ball1, int ball2) { return true; }
        public override bool OnPocketedValid(TeamTypeE activeTeam, int ball)
        {
            if (ball == 0)  // Pocketed CueBall
            {
                return false;
            }
            if ((int)currentTarget == ballsType[ball])
            {
                if (currentTarget == BallTypeE.RED)
                {
                    currentTarget = BallTypeE.ANY_COLOR;
                }
                else
                {
                    currentTarget = BallTypeE.RED;
                }
            }
            else
            {
                currentTarget = BallTypeE.RED;
                return false;
            }
            return true;
        }
        #endregion // RuleBase

        #region RuleSnooker
        #endregion
    }
}