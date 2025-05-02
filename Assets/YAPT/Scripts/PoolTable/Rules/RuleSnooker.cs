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
        private const int MAX_BALL = 13;
        [SerializeField] private Texture2D forceTexture;
        [SerializeField] protected Transform cueBallPos;
        [SerializeField] protected Transform trianglePos;

        [SerializeField] protected Transform[] coloredBallsPos;
        [SerializeField] protected GameObject startLine;

        private BallTypeE currentTarget;

        #region UdonSharpBehaviour
        void Start()
        {
            SetBallTypes();
            currentTarget = BallTypeE.RED;
        }
        #endregion // UdonSharpBehaviour

        #region RuleBase
        // Game initialization
        public override void SetVisuals(bool state)
        {
            startLine.SetActive(state);
        }
        public override void SetBallPositions() { }
        public override Texture2D GetTexture() { return forceTexture; }
        protected override void SetBallTypes()
        {
            ballTypes[0] = (int)BallTypeE.CUE;
            ballTypes[1] = (int)BallTypeE.YELLOW;
            ballTypes[2] = (int)BallTypeE.BROWN;
            ballTypes[3] = (int)BallTypeE.GREEN;
            ballTypes[4] = (int)BallTypeE.BLUE;
            ballTypes[5] = (int)BallTypeE.PINK;
            ballTypes[6] = (int)BallTypeE.BLACK;
            ballTypes[7] = (int)BallTypeE.BLUE;
            for (int i = 8; i < MAX_BALL; i++)
            {
                ballTypes[i] = (int)BallTypeE.RED;
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
            if ((int)currentTarget == ballTypes[ball])
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