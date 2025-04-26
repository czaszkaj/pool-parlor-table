
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace YAPT.PoolTable.Cue
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CueGripPrimary : CueGripVR
    {
        #region UdonSharpBehaviour

        public override void Start()
        {
            base.Start();
            gripType = GripTypeE.PRIMARY;
        }

        #endregion // UdonSharpBehaviour

        #region CueGripPrimary

        // Same as CueGripVR

        #endregion // CueGripPrimary
    }
}