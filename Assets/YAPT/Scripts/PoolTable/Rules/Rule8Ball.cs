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
        [SerializeField] protected GameObject startLine;

        #region UdonSharpBehaviour
        void Start()
        {
            MIN_BALL_ID = 0;
            MAX_BALL_ID = 16;
            TRIANGLE_BALL_ID = 1;
            TRIANGLE_BALL_COUNT = 15;
            SetBallTypes();
            SetInitialBallPositions();
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
        protected override void SetBallTypes()
        {
            ballsType[0] = (int)BallTypeE.CUE;
            ballsType[8] = (int)BallTypeE.BLACK;
            for (int i = 1; i < 7; i++)
            {
                ballsType[i] = (int)BallTypeE.SOLID;
            }
            for (int i = 9; i <= MAX_BALL_ID; i++)
            {
                ballsType[i] = (int)BallTypeE.SOLID;
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