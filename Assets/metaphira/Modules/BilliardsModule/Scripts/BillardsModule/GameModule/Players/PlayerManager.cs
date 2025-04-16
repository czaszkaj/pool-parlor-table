
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

    private bool _IsLocalPlayerReferee()
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

    public bool _AllPlayersOffline()
    {
        for (int i = 0; i < 4; i++)
        {
            if (playerNamesLocal[i] == "") continue;

            VRCPlayerApi player = _GetPlayerByName(playerNamesLocal[i]);
            if (Utilities.IsValid(player))
                return false;
        }

        return true;
    }

    public VRCPlayerApi _GetPlayerByName(string name)
    {
        VRCPlayerApi[] onlinePlayers = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
        for (int playerId = 0; playerId < onlinePlayers.Length; playerId++)
            if (onlinePlayers[playerId].displayName == name)
                return onlinePlayers[playerId];
        return null;
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == null) return;

        if (!table.lobbyOpen) return;

        VRCPlayerApi gameHost = _GetPlayerByName(playerNamesLocal[0]);
        if (!Utilities.IsValid(gameHost))
        {
            // host left. if they were the only ones in-game, instance master tries to close the lobby. otherwise, everyone in-lobby tries
            int otherPlayers = 0;
            for (int i = 0; i < 4; i++)
            {
                if (playerNamesLocal[i] == "") continue;

                VRCPlayerApi possiblePlayer = _GetPlayerByName(playerNamesLocal[i]);
                if (!Utilities.IsValid(possiblePlayer)) continue;

                otherPlayers++;
            }

            if ((otherPlayers == 0 && Networking.LocalPlayer.isMaster) || (otherPlayers > 0 && localPlayerId != -1))
            {
                table.managers.networkingManager._OnLobbyClosed();
            }
        }
        else if (gameHost.isLocal)
        {
            // only host updates player list
            for (int i = 0; i < 4; i++)
            {
                if (playerNamesLocal[i] == "") continue;

                VRCPlayerApi possiblePlayer = _GetPlayerByName(playerNamesLocal[i]);
                if (Utilities.IsValid(possiblePlayer)) continue;

                table.managers.networkingManager._OnKickLobby(i);
            }
        }
    }

    public bool isAnyPlayerActive()
    {
        return _IsPlayer(Networking.LocalPlayer)
            || IsLocalPlayerReferee()
            || _AllPlayersOffline();
    }

    public bool isOurTurn()
    {
        // TODO: verify what this actually checks
        return localPlayerId >= 0 && (localTeamId == table.teamIdLocal || table.isPracticeMode);
    }

    public bool IsPickupAllowed()
    {
        return isOurTurn() && table.isPracticeMode || IsLocalPlayerReferee();
    }

    public bool IsLocalPlayerReferee()
    {
        return !string.IsNullOrEmpty(tournamentRefereeLocal)
            && _IsLocalPlayerReferee();
    }
}
