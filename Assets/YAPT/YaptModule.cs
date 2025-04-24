
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace YAPT
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class YaptModule : UdonSharpBehaviour
    {
        [NonSerialized] public GameObject debugScreen;
        void Start()
        {
            debugScreen = GetComponentInParent<YaptModule>().debugScreen;
            // Initialize debugger
        }
    }
}
