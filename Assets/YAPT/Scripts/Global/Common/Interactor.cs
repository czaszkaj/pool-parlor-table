using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace YAPT.Global.Common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Interactor : UdonSharpBehaviour
    {
        // Select first (from top to bottom) UdonBehaviour from target
        [SerializeField] private UdonBehaviour target;
        [SerializeField] private string eventName;

        public override void Interact()
        {
            if (!target) return;

            target.SendCustomEvent(eventName);
        }
    }
}