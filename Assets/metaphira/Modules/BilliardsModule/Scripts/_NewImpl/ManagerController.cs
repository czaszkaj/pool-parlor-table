using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ManagerController : UdonSharpBehaviour
{
    // Move it to the table in the end
    // It's an obsulete middle class, it dies what the table doeas
    // Fine for now while the whole implementation is just a mess

    [Header("Managers")]
    [SerializeField] public NetworkingManager networkingManager;
    [SerializeField] public PracticeManager practiceManager;
    [SerializeField] public RepositionManager repositionManager;
    [SerializeField] public DesktopManager desktopManager;
    [SerializeField] public CameraManager cameraManager;
    [SerializeField] public GraphicsManager graphicsManager;
    [SerializeField] public StandardPhysicsManager standardPhysicsManager;
    [SerializeField] public MenuManager menuManager;
    [NonSerialized] public UdonSharpBehaviour currentPhysicsManager;

    public void _Init(BilliardsModule table_)
    {
        networkingManager._Init(table_);
        practiceManager._Init(table_);
        repositionManager._Init(table_);
        desktopManager._Init(table_);
        cameraManager._Init(table_);
        graphicsManager._Init(table_);
        standardPhysicsManager._Init(table_);
        menuManager._Init(table_);
    }

    public void updateCurrentPhysics()
    {
        currentPhysicsManager = standardPhysicsManager;
    }
}