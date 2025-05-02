using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Game;

namespace YAPT.PoolTable.Rules
{
    public class RuleBase : UdonSharpBehaviour
    {
        protected GameModeType ruleType = GameModeType.INVALID;
        protected int[] ballTypes;
        protected bool firstCollision = true;
        protected TeamTypeE team0 = TeamTypeE.INVALID;
        protected TeamTypeE team1 = TeamTypeE.INVALID;

        #region RuleBase
        // Unified functionalitya
        public void startShot() { firstCollision = false; }

        // Game initialization
        public virtual void SetVisuals(bool state) { }
        public virtual void SetBallPositions() { }
        public virtual Texture2D GetTexture() { return null; }

        // Game active
        public virtual void SetTeams(TeamTypeE _team0, TeamTypeE _team1) { }
        public virtual void UpdateScore() { }

        // Check if the action was valid
        public virtual bool OnCollisionValid(TeamTypeE activeTeam, int ball1, int ball2) { return true; }
        public virtual bool OnPocketedValid(TeamTypeE activeTeam, int ball) { return true; }

        // Uniform protected methods
        // Type depends on texture
        protected virtual void SetBallTypes() { }
        #endregion // RuleBase
    }
}