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
        private const int MAX_BALL = 10;
        [SerializeField] private Texture2D forceTexture;
        [SerializeField] protected Transform cueBallPos;
        [SerializeField] protected Transform trianglePos;
        [SerializeField] protected GameObject startLine;
        #region UdonSharpBehaviour
        void Start()
        {
            SetBallTypes();

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
            for (int i = 1; i < MAX_BALL; i++)
            {
                ballTypes[i] = i;
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