
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace YAPT.PoolTable.Common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerSlot : UdonSharpBehaviour
    {
        [UdonSynced][NonSerialized] public string owner = "";

        private NetworkingManager networkingManager;

        // public void _Init(NetworkingManager networkingManager_)
        // {
        //     networkingManager = networkingManager_;
        // }

        public bool _Register()
        {
            VRCPlayerApi player = Networking.LocalPlayer;

            if (owner != "" && owner != player.displayName) return false;

            owner = player.displayName;

            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            this.RequestSerialization();

            return true;
        }

        public void _Reset()
        {
            if (owner == "") return;

            owner = "";

            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            this.RequestSerialization();
            // this.OnDeserialization();
        }

        // public override void OnDeserialization()
        // {
        //     if (networkingManager == null) return;

        //     // networkingManager._OnPlayerSlotChanged(this);
        // }
        void Start()
        {
            // networkingManager = GetComponentInParent<NetworkingManager>();
            // if (networkingManager == null)
            // {
            //     // Debug.LogError("PlayerSlot: NetworkingManager not found in parent.");
            // }
        }
    }
}