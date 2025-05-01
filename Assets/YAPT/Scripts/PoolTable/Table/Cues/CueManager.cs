
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

// TODO:
// * remove commented out if's that are depended on gameModue
// * think about using or removing functions triggering GameModule.
//   GameModule should just read current state of CueManagers
// Improvement:
// * resize cue: size, width, cuetip, etc.
// * adjust lag force, maybe disable during shot

namespace YAPT.PoolTable.Cue
{
    public enum GripTypeE : int
    {
        DESKTOP = 0,
        PRIMARY = 1,
        SECONDARY = 2,
        INVALID = 3
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CueManager : UdonSharpBehaviour
    {
        private const float GRIP_SIZE = 0.03f;
        [SerializeField] private float LAG_FORCE = 16.0f; // Default 16.0f

        [Header("Cue distance of the table limit")]
        [SerializeField] private Vector3 min = new Vector3(-3.25f, 0f, -2.5f);
        [SerializeField] private Vector3 max = new Vector3(3.25f, 3f, 2.5f);

        [Header("Cue Objects")]
        [SerializeField][HideInInspector] private GameObject primary;
        [SerializeField][HideInInspector] private GameObject secondary;
        [SerializeField][HideInInspector] private GameObject desktop;
        [SerializeField][HideInInspector] private GameObject body;
        [SerializeField][HideInInspector] private GameObject cuetip;

        [Header("Others")]
        // [UdonSynced] private int syncedCueSkin;

        // Ownership
        private string[] authorizedOwners;
        // Cue parameters
        private float cuetipDistance;

        private bool holderIsDesktop;
        [UdonSynced] private bool syncedHolderIsDesktop;

        private bool primaryHolding;
        [UdonSynced] private bool primaryLocked;
        [UdonSynced] private Vector3 primaryLockPos;
        [UdonSynced] private Vector3 primaryLockDir;

        private bool secondaryHolding;
        [UdonSynced] private bool secondaryLocked;
        [UdonSynced] private Vector3 secondaryLockPos;

        private Vector3 secondaryOffset;

        private Vector3 origPrimaryPosition;
        private Vector3 origSecondaryPosition;

        private Vector3 lagPrimaryPosition;
        private Vector3 lagSecondaryPosition;

        private CueGripPrimary primaryController;
        private CueGripSecondary secondaryController;
        private CueGripDesktop desktopController;

        #region UdonSharpBehaviour

        public void Start()
        {

            primaryController = primary.GetComponent<CueGripPrimary>();
            secondaryController = secondary.GetComponent<CueGripSecondary>();
            desktopController = desktop.GetComponent<CueGripDesktop>();

            cuetipDistance = (cuetip.transform.position - primary.transform.position).magnitude;

            // Get values from Unity to alway have the same original value
            origPrimaryPosition = new Vector3(0, 0, 0);
            origSecondaryPosition = new Vector3(0, 0, 0.55f);

            lagPrimaryPosition = origPrimaryPosition;
            lagSecondaryPosition = origSecondaryPosition;

            resetSecondaryOffset();
        }

        public override void OnDeserialization()
        {
            // Trigger after RequestSerialization()
            // Can be used to trigger refreshCueSkin()
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            return IsOwnershipTransferAllowed(requester, newOwner);
        }

        private void FixedUpdate()
        {
            if (primaryHolding)
            {
                // must not be shooting, since that takes control of the cue object
                // if (!table.desktopManager._IsInUI() || !table.desktopManager._IsShooting())
                // if (true) //TODO: check if i want desktpop animation
                // {
                if (!primaryLocked)// || table.noLockingLocal) //TODO
                {
                    moveCueUnlocked();
                }
                else
                {
                    moveCueLocked();
                }

                updateDesktopMarker();
                // }
                // else
                // {
                //     moveDesktopCue();
                // }

                // clamp controllers
                clampControllers();
            }
            else
            {
                // other player has cue
                // if (!syncedHolderIsDesktop)
                // {
                // other player is in vr, use the grips which update faster
                if (!primaryLocked)// || table.noLockingLocal) //TODO
                {
                    moveCueUnlockedSimplified();
                }
                else
                {
                    moveCueLocked();
                }
                // }
                // else
                // {
                //     // other player is on desktop, use the slower synced marker
                //     moveDesktopCue();
                // }
            }
            getNextCueAnimationPosition();
        }

        #endregion // UdonSharpBehaviour

        #region CueManager

        public void Enable()
        {
            if (Array.IndexOf(authorizedOwners, Networking.LocalPlayer.displayName) != -1) return;
            primaryController.Show();
        }

        public void Disable()
        {
            primaryController.Hide();
            secondaryController.Hide();

            lagPrimaryPosition = origPrimaryPosition;
            lagSecondaryPosition = origSecondaryPosition;
        }
        public void ResetPosition()
        {
            takeOwnership();

            primary.transform.position = origPrimaryPosition;
            primary.transform.localRotation = Quaternion.identity;
            secondary.transform.position = origSecondaryPosition;
            secondary.transform.localRotation = Quaternion.identity;
            desktop.transform.position = origPrimaryPosition;
            desktop.transform.localRotation = Quaternion.identity;
            body.transform.position = origPrimaryPosition;
            body.transform.LookAt(origSecondaryPosition);
        }

        private void getNextCueAnimationPosition()
        {
            // TODO: Improvement: Validate different value
            // Check if this can be disabled during locking
            // 

            // todo: ugly ugly hack from legacy 8ball. intentionally smooth/lag the position a bit
            // we can't remove this because this directly affects physics
            // must occur at the end after we've finished updating the transform's position
            // otherwise vrchat will try to change it because it's a pickup
            lagPrimaryPosition = Vector3.Lerp(lagPrimaryPosition, primary.transform.position, Time.fixedDeltaTime * LAG_FORCE);
            if (!secondaryLocked)
            {
                lagSecondaryPosition = Vector3.Lerp(lagSecondaryPosition, secondary.transform.position, Time.fixedDeltaTime * LAG_FORCE);
            }

        }

        private void moveDesktopCue()
        {
            body.transform.position = desktop.transform.position;
            body.transform.rotation = desktop.transform.rotation;
        }

        private void updateDesktopMarker()
        {
            desktop.transform.position = body.transform.position;
            desktop.transform.rotation = body.transform.rotation;
        }

        private void moveCueLocked()
        {
            // locking primary hand. fix cue in line and ignore secondary hand
            Vector3 delta = lagPrimaryPosition - primaryLockPos;
            float distance = Vector3.Dot(delta, primaryLockDir);
            body.transform.position = primaryLockPos + primaryLockDir * distance;
        }

        private void moveCueUnlocked()
        {
            // base of cue goes to primary
            body.transform.position = lagPrimaryPosition;
            // body.transform.position = primary.transform.position;

            // holding in primary hand
            if (!secondaryHolding)
            {
                // nothing in secondary hand. have the second grip track the cue
                secondary.transform.position = primary.transform.TransformPoint(secondaryOffset);
                body.transform.LookAt(lagSecondaryPosition);
            }
            else if (!secondaryLocked)
            {
                // holding secondary hand. have cue track the second grip
                body.transform.LookAt(lagSecondaryPosition);
            }
            else
            {
                // locking secondary hand. lock rotation on point
                body.transform.LookAt(secondaryLockPos);
            }

            // copy z rotation of primary
            float rotation = primary.transform.localEulerAngles.z;
            Vector3 bodyRotation = body.transform.localEulerAngles;
            bodyRotation.z = rotation;
            body.transform.localEulerAngles = bodyRotation;
        }

        private void moveCueUnlockedSimplified()
        {
            // base of cue goes to primary
            body.transform.position = lagPrimaryPosition;

            // holding in primary hand
            if (!secondaryLocked)
            {
                // have cue track the second grip
                body.transform.LookAt(lagSecondaryPosition);
            }
            else
            {
                // locking secondary hand. lock rotation on point
                body.transform.LookAt(secondaryLockPos);
            }
        }

        public void SetAuthorizedOwners(string[] newOwners)
        {
            authorizedOwners = newOwners;
        }

        private void takeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            Networking.SetOwner(Networking.LocalPlayer, primary);
            Networking.SetOwner(Networking.LocalPlayer, secondary);
            Networking.SetOwner(Networking.LocalPlayer, desktop);
        }

        public void OnGripPickup(GripTypeE gripType)
        {
            // Debug.Log($"CueManager: OnGripPickup: gripType={gripType}");
            switch (gripType)
            {
                case GripTypeE.PRIMARY:
                    takeOwnership();

                    holderIsDesktop = !Networking.LocalPlayer.IsUserInVR();
                    syncedHolderIsDesktop = holderIsDesktop;
                    primaryHolding = true;
                    primaryLocked = false;
                    // syncedCueSkin = table.activeCueSkin;
                    RequestSerialization();
                    // table._OnPickupCue();
                    if (!holderIsDesktop) secondaryController.Show();
                    break;
                case GripTypeE.SECONDARY:
                    secondaryHolding = true;
                    secondaryLocked = false;
                    RequestSerialization();
                    break;
            }
            // Debug.Log($"CueManager: OnGripPickup: \n" +
            //           $"holderIsDesktop={holderIsDesktop}" +
            //           $"syncedHolderIsDesktop={syncedHolderIsDesktop}" +
            //           $"primaryHolding={primaryHolding}" +
            //           $"primaryLocked={primaryLocked}");
        }

        public void OnGripDrop(GripTypeE gripType)
        {
            switch (gripType)
            {
                case GripTypeE.PRIMARY:
                    primaryHolding = false;
                    syncedHolderIsDesktop = false;
                    RequestSerialization();
                    CueReleased();
                    break;
                case GripTypeE.SECONDARY:
                    secondaryHolding = false;
                    RequestSerialization();
                    break;
            }
        }

        private void CueReleased()
        {
            // make sure lag position is reset
            lagPrimaryPosition = primary.transform.position;
            lagSecondaryPosition = secondary.transform.position;

            // move cue to primary grip, since it should be bounded
            body.transform.position = primary.transform.position;
            // make sure cue is facing the secondary grip (since it may have flown off)
            body.transform.LookAt(secondary.transform.position);
            // copy z rotation of primary
            float rotation = primary.transform.localEulerAngles.z;
            Vector3 bodyRotation = body.transform.localEulerAngles;
            bodyRotation.z = rotation;
            body.transform.localEulerAngles = bodyRotation;
            // rotate primary grip to face cue, since cue is visual source of truth
            primary.transform.rotation = body.transform.rotation;
            // reset secondary offset
            resetSecondaryOffset();
            // update desktop marker //TODO: wat?
            updateDesktopMarker();

            //TODO: table._OnDropCue();
        }

        public void OnGripUseDown(GripTypeE gripType)
        {
            switch (gripType)
            {
                case GripTypeE.PRIMARY:
                    primaryLocked = true;
                    primaryLockPos = body.transform.position;
                    primaryLockDir = body.transform.forward.normalized;
                    RequestSerialization();
                    // TODO: table._TriggerCueActivate();
                    break;
                case GripTypeE.SECONDARY:
                    secondaryLocked = true;
                    secondaryLockPos = secondary.transform.position;
                    RequestSerialization();
                    break;
            }
        }

        public void OnGripUseUp(GripTypeE gripType)
        {
            switch (gripType)
            {
                case GripTypeE.PRIMARY:
                    primaryLocked = false;
                    //TODO:  table._TriggerCueDeactivate();
                    RequestSerialization();
                    break;
                case GripTypeE.SECONDARY:
                    secondaryLocked = false;
                    RequestSerialization();
                    break;
            }
        }

        public bool IsOwnershipTransferAllowed(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (requester.playerId != newOwner.playerId) return false;
            if (Array.IndexOf(authorizedOwners, newOwner.displayName) == -1) return true; // TODO: false
            return true;
        }

        private void resetSecondaryOffset()
        {
            // Calculate "position" between primary and secondary grip
            // Needed when secondary grip is not hold, to keep cue in the same direction
            Vector3 position = primary.transform.InverseTransformPoint(secondary.transform.position);
            secondaryOffset = position.normalized * Mathf.Clamp(position.magnitude, CueManager.GRIP_SIZE * 2, cuetipDistance);
        }

        private void clampControllers()
        {
            // Set cue limits around the table
            clampTransform(primary.transform);
            clampTransform(secondary.transform);
        }

        private void clampTransform(Transform child)
        {
            Vector3 min = new Vector3(-3.25f, 0f, -2.5f);
            Vector3 max = new Vector3(3.25f, 3f, 2.5f);
            Transform tableT = transform.parent.parent.parent; // Table.TableModel.Cues.Cue
            child.position = tableT.TransformPoint(clamp(tableT.InverseTransformPoint(child.position), min, max));
        }
        private Vector3 clamp(Vector3 input, Vector3 min, Vector3 max)
        {
            input.x = Mathf.Clamp(input.x, min.x, max.x);
            input.y = Mathf.Clamp(input.y, min.y, max.y);
            input.z = Mathf.Clamp(input.z, min.z, max.z);
            return input;
        }

        public void ResetCuePosition()
        {
            takeOwnership();

            primary.transform.position = origPrimaryPosition;
            primary.transform.localRotation = Quaternion.identity;
            secondary.transform.position = origSecondaryPosition;
            secondary.transform.localRotation = Quaternion.identity;
            desktop.transform.position = origPrimaryPosition;
            desktop.transform.localRotation = Quaternion.identity;
            body.transform.position = origPrimaryPosition;
            body.transform.LookAt(origSecondaryPosition);
        }

        // private void refreshCueSkin()
        // {
        //     MeshRenderer renderer = this.transform.Find("body/render").GetComponent<MeshRenderer>();
        //     renderer.materials[1].SetTexture("_MainTex", table.cueSkins[activeCueSkin]);
        // }

        #endregion // CueManager
    }
}