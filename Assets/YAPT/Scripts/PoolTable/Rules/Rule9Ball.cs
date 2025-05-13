using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Game;

namespace YAPT.PoolTable.Rules.NineBall
{
    public enum BallTypeE : int
    {
        CUE = 0
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Rule9Ball : RuleBase
    {
        [SerializeField] private Texture2D forceTexture;
        [SerializeField] protected GameObject startLine;

        #region UdonSharpBehaviour
        void Start()
        {
            MIN_BALL_ID = 0;
            MAX_BALL_ID = 10;
            TRIANGLE_BALL_ID = 1;
            TRIANGLE_BALL_COUNT = 9;
            SetBallTypes();
            SetInitialBallPositions();
        }
        #endregion // UdonSharpBehaviour

        #region RuleBase
        // Game initialization
        public override void SetVisuals(bool state)
        {
            startLine.SetActive(state);
        }
        public override Texture2D GetTexture() { return forceTexture; }
        protected override void SetBallTypes()
        {
            ballsType[0] = (int)BallTypeE.CUE;
            for (int i = 1; i <= MAX_BALL_ID; i++)
            {
                ballsType[i] = i;
            }
        }

        // Check if the action was valid
        public override bool OnCollisionValid(TeamTypeE activeTeam, int ball1, int ball2) { return true; }
        public override bool OnPocketedValid(TeamTypeE activeTeam, int ball) { return true; }
        #endregion // RuleBase

        #region RuleSnooker
        #endregion
    }
}