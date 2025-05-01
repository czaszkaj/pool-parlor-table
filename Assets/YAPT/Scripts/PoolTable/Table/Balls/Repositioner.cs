using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

// Probably to be removed and moved to manager

namespace YAPT.PoolTable.Balls
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class Repositioner : UdonSharpBehaviour
    {
        [SerializeField] private BallManager ballsMgr;
        private VRC_Pickup pickup;
        private int index;


        #region UdonSharpBehaviour
        void Start()
        {
            pickup = GetComponent<VRC_Pickup>();
        }

        public override void OnPickup()
        {
            // table.repositionManager._BeginReposition(this);
        }

        public override void OnDrop()
        {
            // table.repositionManager._EndReposition(this);
        }
        #endregion

        #region UdonSharpBehaviour
        public void SetIndex(int _index)
        {
            index = _index;
        }

        public void _Drop()
        {
            // pickup.Drop();
        }

        public void _Reset()
        {
            // this.transform.localPosition = Vector3.zero;
            // this.transform.localRotation = Quaternion.identity;
        }
        #endregion
    }
}