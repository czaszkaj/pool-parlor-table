
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Rule6Reds : UdonSharpBehaviour
{
    [NonSerialized] [HideInInspector] public readonly int[] ballOrder =  {4, 6, 9, 10, 11, 12, 2, 7, 8, 3, 5, 1};
    [NonSerialized] [HideInInspector] public readonly int[] ballPoints = {0, 7, 2, 5, 1, 6, 1, 3, 4, 1, 1, 1, 1};
    [NonSerialized] [HideInInspector] public bool redsOnTable = true;
    
    void Start()
    {
        
    }
}
