
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TableManager : UdonSharpBehaviour
{
    [NonSerialized] [HideInInspector] public GraphicsManager graphics;

    // TODO: Move to rules
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

    public void flashFoul()
    {
        graphics._FlashTableColor(k_colour_foul);
    }
}