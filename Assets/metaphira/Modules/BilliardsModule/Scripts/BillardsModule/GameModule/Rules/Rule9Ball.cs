
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Rule9Ball : UdonSharpBehaviour
{
    [NonSerialized] [HideInInspector] public readonly int[] ballOrder = {2, 3, 4, 5, 9, 6, 7, 8, 1};
    [NonSerialized] [HideInInspector] public readonly int[] breakRows = {0, 1, 2, 1, 0};

    void Start()
    {
        
    }
}
