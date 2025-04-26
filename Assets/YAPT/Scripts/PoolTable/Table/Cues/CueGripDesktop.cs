
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace YAPT.PoolTable.Cue
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CueGripDesktop : UdonSharpBehaviour
    {
        protected GripTypeE gripType = GripTypeE.DESKTOP; // Not used
        private CueManager cueManager;

        #region UdonSharpBehaviour

        public void Start()
        {
            // This is needed to allow name changes of the parent object (Cue1, Cue2, CueInDoors, etc.)
            cueManager = transform.parent.gameObject.GetComponent<CueManager>();
        }

        #endregion // UdonSharpBehaviour

        #region CueGripDesktop

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            return cueManager.IsOwnershipTransferAllowed(requester, newOwner);
        }

        #endregion // CueGripDesktop
    }
}