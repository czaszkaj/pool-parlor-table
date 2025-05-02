using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Game;

namespace YAPT.PoolTable.Rules.EightBall
{
    public enum BallTypeE : int
    {
        CUE = 0,
        SOLID,
        HALF,
        BLACK
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Rule8Ball : RuleBase
    {
        private const int MAX_BALL = 16;
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
        public override void SetTeams(TeamTypeE _team0, TeamTypeE _team1)
        {
            team0 = _team0;
            team1 = _team1;
        }
        public override void SetVisuals(bool state)
        {
            startLine.SetActive(state);
        }
        public override void SetBallPositions() { }
        protected override void SetBallTypes()
        {
            ballTypes[0] = (int)BallTypeE.CUE;
            ballTypes[8] = (int)BallTypeE.BLACK;
            for (int i = 1; i < 7; i++)
            {
                ballTypes[i] = (int)BallTypeE.SOLID;
            }
            for (int i = 9; i < MAX_BALL; i++)
            {
                ballTypes[i] = (int)BallTypeE.SOLID;
            }
        }

        // Check if the action was valid
        public override bool OnCollisionValid(TeamTypeE activeTeam, int ball1, int ball2) { return true; }
        public override bool OnPocketedValid(TeamTypeE activeTeam, int ball) { return true; }
        #endregion // RuleBase

        #region Rule8Ball
        #endregion // Rule8Ball
    }
}