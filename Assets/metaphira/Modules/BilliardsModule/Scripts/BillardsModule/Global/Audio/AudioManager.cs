
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AudioManager : UdonSharpBehaviour
{
    [SerializeField] public AudioClip snd_Intro,
                                      snd_Sink,
                                      snd_NewTurn,
                                      snd_PointMade,
                                      snd_btn,
                                      snd_spin,
                                      snd_spinstop,
                                      snd_hitball;
    void Start()
    {
        
    }
}
