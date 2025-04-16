
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class GameModule : UdonSharpBehaviour
{
    [NonSerialized] [HideInInspector] public TableManager table;

    void Start()
    {
        
    }
}
