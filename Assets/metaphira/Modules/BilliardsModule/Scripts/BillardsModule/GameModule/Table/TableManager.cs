
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TableManager : UdonSharpBehaviour
{
    // Non Serialized
    [NonSerialized] [HideInInspector] public GraphicsManager graphics;
    [NonSerialized] [HideInInspector] public CueRackManager cues;
    [NonSerialized] [HideInInspector] public BallManager balls;
    // Rules, to be replaced with active_rule
    [NonSerialized] [HideInInspector] public Rule8Ball rule8Ball;
    [NonSerialized] [HideInInspector] public Rule9Ball rule9Ball;
    [NonSerialized] [HideInInspector] public Rule6Reds rule6Reds;
    [NonSerialized] [HideInInspector] public Rule4Ball rule4Ball;
    [NonSerialized] [HideInInspector] public PracticeManager practice;
    [NonSerialized] [HideInInspector] public CommonRules common;
    // TODO refactor, better allocation
    // For now it make sense to be here, will update together with functions
    // For example redo ifXBall to enum
    [NonSerialized] public bool teamsLocal,
                                isTableOpenLocal,
                                canPlayLocal,
                                colorTurnLocal,
                                canHitCueBall = false,
                                isReposition = false,
                                is8Ball = false,
                                is9Ball = false,
                                is4Ball = false,
                                isJp4Ball = false,
                                isKr4Ball = false,
                                isSnooker6Red = false,
                                isPracticeMode = false,
                                timerRunning = false;

    // Serialized
    [SerializeField] public ModelData[] tableModels;
    [SerializeField] public Texture2D[] tableSkins;
    [SerializeField] public UdonSharpBehaviour cameraModule;
    [SerializeField] public Text infReset; // Text under reset button

    // TODO: Move/Rename???
    [SerializeField] [HideInInspector] public Color k_colour_foul,        // v1.6: ( 1.2, 0.0, 0.0, 1.0 )
                                                    k_colour_default,     // v1.6: ( 1.0, 1.0, 1.0, 1.0 )
                                                    k_colour_off = new Color(0.01f, 0.01f, 0.01f, 1.0f);

    [SerializeField] [HideInInspector] public Color k_teamColour_spots,   // v1.6: ( 0.00, 0.75, 1.75, 1.0 )
                                                    k_teamColour_stripes; // v1.6: ( 1.75, 0.25, 0.00, 1.0 )

    [SerializeField] [HideInInspector] public Color k_colour4Ball_team_0, // v1.6: ( )
                                                    k_colour4Ball_team_1; // v1.6: ( 2.0, 1.0, 0.0, 1.0 )

    [SerializeField] [HideInInspector] public Color k_fabricColour_8ball, // v1.6: ( 0.3, 0.3, 0.3, 1.0 )
                                                    k_fabricColour_9ball, // v1.6: ( 0.1, 0.6, 1.0, 1.0 )
                                                    k_fabricColour_4ball; // v1.6: ( 0.15, 0.75, 0.3, 1.0 )

    void Start()
    {
        
    }

    public void Init(GraphicsManager _graphics)
    {
        graphics = _graphics;
    }

    // Flash the table
    public void flashFoul()
    {
        graphics._FlashTableColor(k_colour_foul);
    }
}