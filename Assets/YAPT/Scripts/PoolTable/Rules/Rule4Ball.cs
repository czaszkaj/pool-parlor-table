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
        private const int MIN_BALL_ID = 12;
        private const int MAX_BALL = 4;
        [SerializeField] private Texture2D forceTexture;
        [SerializeField] protected Transform cue0BallPos;
        [SerializeField] protected Transform cue1BallPos;
        [SerializeField] protected Transform red0BallPos;
        [SerializeField] protected Transform red1BallPos;
        [SerializeField] protected GameObject pocketFillers;


        private bool isKorean = false;
        #region UdonSharpBehaviour
        void Start()
        {
            SetBallTypes();
            team0 = TeamTypeE.LEFT;
            team1 = TeamTypeE.RIGHT;
        }
        #endregion // UdonSharpBehaviour

        #region RuleBase
        // Game initialization
        public override void SetVisuals(bool state)
        {
            pocketFillers.SetActive(state);
        }
        public override void SetBallPositions() { }
        public override Texture2D GetTexture() { return forceTexture; }
        protected override void SetBallTypes()
        {
            ballTypes[MIN_BALL_ID] = (int)BallTypeE.CUE_WHITE;
            ballTypes[MIN_BALL_ID + 1] = (int)BallTypeE.CUE_YELLOW;
            ballTypes[MIN_BALL_ID + 2] = (int)BallTypeE.RED;
            ballTypes[MIN_BALL_ID + 3] = (int)BallTypeE.RED;
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