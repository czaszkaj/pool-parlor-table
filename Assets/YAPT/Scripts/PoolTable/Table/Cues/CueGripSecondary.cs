
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace YAPT.PoolTable.Cue
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CueGripSecondary : CueGripVR
    {
        private Vector3 offset;
        private bool holding;

        #region UdonSharpBehaviour

        public override void Start()
        {
            base.Start();
            gripType = GripTypeE.SECONDARY;
        }

        #endregion // UdonSharpBehaviour

        #region CueGripSecondary

        public override void Show()
        {
            meshRenderer.enabled = true;
            base.Show();
        }
        public override void Hide()
        {
            base.Hide();
            meshRenderer.enabled = false;
        }
        #endregion // CueGripSecondary
    }
}