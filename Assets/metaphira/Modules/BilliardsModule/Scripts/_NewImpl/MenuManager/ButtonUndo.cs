
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ButtonUndo : UdonSharpBehaviour
{
    [SerializeField] BilliardsModule table;

    public override void Interact()
    {
        table.managers.practiceManager._Undo();
    }
}
