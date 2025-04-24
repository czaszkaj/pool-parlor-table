
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT;
using System;

namespace YAPT.PoolTable.Table
{
    public enum GameModeType : int
    {
        INVALID = -1,
        EIGHT_BALL = 0,
        NINE_BALL = 1,
        FOUR_BALL = 2,
        SIX_REDS = 3
    }

    public class GameModule : UdonSharpBehaviour
    {
        void Start()
        {

        }
    }
}