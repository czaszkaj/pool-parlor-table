
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CueRackManager : UdonSharpBehaviour
{
    [SerializeField] public Texture2D[] cueSkins;
    [SerializeField] public CueController[] cueControllers;
    
    // TODO: Move maybe to a CueController?
    // Or inside Game to work with active cue.
    // What devhit do?
    [SerializeField] public GameObject guideline,
                                       devhit;
    void Start()
    {
        
    }
}
