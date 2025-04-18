
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Rule8Ball : UdonSharpBehaviour
{
    // I would like to move 8ball from index 1 to 8 in balls[]
    [NonSerialized] [HideInInspector] public readonly int[] ballOrder = {9, 2, 10, 11, 1, 3, 4, 12, 5, 13, 14, 6, 15, 7, 8};

    void Start()
    {
        
    }
}
