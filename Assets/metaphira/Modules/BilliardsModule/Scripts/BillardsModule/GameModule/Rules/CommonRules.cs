
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class CommonRules : UdonSharpBehaviour
{
    [NonSerialized] [HideInInspector] public bool guideLineEnabledLocal = true; // TODO: Curently used in revers, to change
    [NonSerialized] [HideInInspector] public bool lockingEnabledLocal = true;
    [NonSerialized] [HideInInspector] public bool isGuidelineValid = true;
    void Start()
    {
        
    }
}
