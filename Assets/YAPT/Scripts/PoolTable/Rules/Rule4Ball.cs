using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Game;

namespace YAPT.PoolTable.Rules.FourBall
{
    public enum BallTypeE : int
    {
        CUE_WHITE = 0,
        CUE_YELLOW,
        RED
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Rule4Ball : RuleBase
    {
        [SerializeField] private Texture2D forceTexture;
        [SerializeField] protected GameObject pocketFillers;
        private bool isKorean = false;

        #region UdonSharpBehaviour
        void Start()
        {
            MIN_BALL_ID = 12;
            MAX_BALL_ID = 15;
            SetBallTypes();
            team0 = TeamTypeE.LEFT;
            team1 = TeamTypeE.RIGHT;
            SetInitialBallPositions();
        }
        #endregion // UdonSharpBehaviour

        #region RuleBase
        // Game initialization
        public override void SetVisuals(bool state)
        {
            pocketFillers.SetActive(state);

        }
        protected override void SetInitialBallPositions()
        {
            // override everything, we don't use default setup for 4 ball
            ballInitialPositions[MIN_BALL_ID + 0] = new Vector3(-0.735f, 0f, 0f);
            ballInitialPositions[MIN_BALL_ID + 1] = new Vector3(0.695f, 0f, 0f);
            ballInitialPositions[MIN_BALL_ID + 2] = new Vector3(-0.32f, 0f, 0f);
            ballInitialPositions[MIN_BALL_ID + 3] = new Vector3(0.36f, 0f, 0f);
        }
        public override Texture2D GetTexture() { return forceTexture; }
        protected override void SetBallTypes()
        {
            ballsType[MIN_BALL_ID + 0] = (int)BallTypeE.CUE_WHITE;
            ballsType[MIN_BALL_ID + 1] = (int)BallTypeE.CUE_YELLOW;
            ballsType[MIN_BALL_ID + 2] = (int)BallTypeE.RED;
            ballsType[MIN_BALL_ID + 3] = (int)BallTypeE.RED;
        }

        // Check if the action was valid
        public override bool OnCollisionValid(TeamTypeE activeTeam, int ball1, int ball2) { return true; }
        public override bool OnPocketedValid(TeamTypeE activeTeam, int ball) { return true; }
        #endregion // RuleBase

        #region Rule4Ball
        public void SetKorean(bool isKr) { isKorean = isKr; }
        #endregion
    }
}