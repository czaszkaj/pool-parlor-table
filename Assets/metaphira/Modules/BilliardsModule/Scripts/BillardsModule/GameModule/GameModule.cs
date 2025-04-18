
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class GameModule : UdonSharpBehaviour
{
    [NonSerialized] [HideInInspector] public TableManager table;
    [NonSerialized][HideInInspector]  public bool lobbyOpen,
                                                  gameLive,
                                                  isBreak,
                                                  isLocalSimulationRunning;

    void Start()
    {
        
    }
}
