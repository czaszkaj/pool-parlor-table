
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerManager : UdonSharpBehaviour
{
    // Serialized
    [NonSerialized] public CueController[] cueControllers;
    [NonSerialized] public BilliardsModule table;

    // Non-Serialized
    [NonSerialized] private string[] moderators = new string[0];
    [NonSerialized] public string tournamentRefereeLocal;
    [NonSerialized] public int localPlayerId = -1;
    [NonSerialized] public string[] playerNamesCached = new string[4],
                                    playerNamesLocal = new string[4];
    [NonSerialized] public uint localTeamId = 0u;

    // Functionality
    public void Init(BilliardsModule table_)
    {
        table = table_;
    }

    public void onRemotePlayersChanged(string[] playerNamesSynced)
    {
        // Info what Cashed means in this context
        // From what i see it probably means it's availabe in Unity
        if (playerNamesCached.Equals(playerNamesSynced)) return;
        Array.Copy(playerNamesSynced, playerNamesCached, playerNamesCached.Length);
        Array.Copy(playerNamesSynced, playerNamesLocal, playerNamesLocal.Length);
        logPlayerNames();

        localPlayerId = Array.IndexOf(playerNamesLocal, Networking.LocalPlayer.displayName);
        if (localPlayerId != -1) localTeamId = (uint)(localPlayerId & 0x1u);

        cueControllers[0]._SetAuthorizedOwners(new string[] { playerNamesLocal[0], playerNamesLocal[2] });
        cueControllers[1]._SetAuthorizedOwners(new string[] { playerNamesLocal[1], playerNamesLocal[3] });

        // Leave commented code in table manager
        // managers.menuManager._RefreshLobbyOpen();
        // managers.menuManager._RefreshPlayerList();
    }

    private void logPlayerNames()
    {
        string[] playerDetails = new string[4];
        for (int i = 0; i < 4; i++)
            playerDetails[i] = playerNamesLocal[i] == "" ? "none" : playerNamesLocal[i];
        table.logger._LogInfo($"onRemotePlayersChanged newPlayers={string.Join(",", playerDetails)}");
    }

    public bool _IsLocalPlayerReferee()
    {
        return _IsReferee(Networking.LocalPlayer);
    }

    public bool _IsModerator(VRCPlayerApi player)
    {
        return Array.IndexOf(moderators, player.displayName) != -1;
    }

    public bool _IsReferee(VRCPlayerApi player)
    {
        if (player == null) return false;

        if (string.IsNullOrEmpty(tournamentRefereeLocal)) return false;

        return player.displayName == tournamentRefereeLocal || _IsModerator(player);
    }

    public bool _IsPlayer(VRCPlayerApi who)
    {
        if (who == null) return false;
        if (who.isLocal && localPlayerId >= 0) return true;

        for (int i = 0; i < 4; i++)
        {
            if (playerNamesLocal[i] == who.displayName)
            {
                return true;
            }
        }

        return false;
    }
}
