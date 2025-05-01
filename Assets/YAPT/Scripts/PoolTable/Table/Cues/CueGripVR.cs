
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace YAPT.PoolTable.Cue
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CueGripVR : UdonSharpBehaviour
    {
        protected GripTypeE gripType = GripTypeE.INVALID;
        protected CueManager cueManager;

        protected VRC_Pickup pickup;
        protected MeshRenderer meshRenderer;
        protected SphereCollider sphereCollider;

        #region UdonSharpBehaviour

        public virtual void Start()
        {
            // Dynamic assigment is needed to allow name changes of the parent object (Cue1, Cue2, CueInDoors, etc.)
            cueManager = transform.parent.gameObject.GetComponent<CueManager>();

            pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
            meshRenderer = GetComponent<MeshRenderer>();
            sphereCollider = GetComponent<SphereCollider>();

            Hide();
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            return cueManager.IsOwnershipTransferAllowed(requester, newOwner);
        }

        public override void OnPickup()
        {
            cueManager.OnGripPickup(gripType);
        }

        public override void OnDrop()
        {
            cueManager.OnGripDrop(gripType);
        }

        public override void OnPickupUseDown()
        {
            meshRenderer.enabled = false;
            cueManager.OnGripUseDown(gripType);
        }

        public override void OnPickupUseUp()
        {
            meshRenderer.enabled = true;
            cueManager.OnGripUseUp(gripType);
        }

        #endregion // UdonSharpBehaviour

        #region CueGripVR

        public virtual void Show()
        {
            sphereCollider.enabled = true;
            pickup.pickupable = true;
        }

        public virtual void Hide()
        {
            pickup.Drop();
            pickup.pickupable = false;
            sphereCollider.enabled = false;
        }

        #endregion // CueGripVR
    }
}