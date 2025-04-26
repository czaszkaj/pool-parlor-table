
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Ui;

namespace YAPT.PoolTable.Players
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerManager : UdonSharpBehaviour
    {
        public const int INVALID_PLAYER_ID = -1;
        public const int MAX_PLAYERS = 4;
        // Team 0: player 0, player 1
        // Team 1: player 2, player 3
        private MenuData menuData;

        #region UdonSharpBehaviour
        void Start()
        {
            menuData = GetComponentInParent<MenuData>();
        }
        #endregion

        #region Player Management

        public void Reset()
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                menuData.PlayerNames[i] = "";
                menuData.PlayerIds[i] = INVALID_PLAYER_ID;
            }
        }

        public void SetPlayer(int index, string name, int id)
        {
            if (menuData.IsGameLive) return; // Allow player change only if game is not live
            if (index < 0 || index >= MAX_PLAYERS)
            {
                Debug.LogError($"Index {index} is out of bounds.");
                return;
            }
            if (menuData.PlayerNames[index] == "")
            {
                menuData.PlayerNames[index] = name;
                menuData.PlayerIds[index] = id;
            }
            menuData.SyncData();
        }

        // Possible small optimization if playerId is used
        public bool isPlayer(string name)
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (menuData.PlayerNames[i] == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemovePlayerIndex(int index)
        {
            if (menuData.IsGameLive) return; // Allow player change only if game is not live
            RemovePlayer(menuData.PlayerNames[index]);
        }

        public void RemovePlayer(string name)
        {
            Debug.Log($"PlayerManager:Remove: name:{name}");
            if (menuData.IsGameLive) return; // Allow player change only if game is not live
            if (menuData.PlayerNames[0] == name)
            {
                MovePlayerData(1, 0);
            }
            else if (menuData.PlayerNames[1] == name)
            {
                ClearPlayerData(1);
            }
            else if (menuData.PlayerNames[2] == name)
            {
                MovePlayerData(3, 2);
            }
            else if (menuData.PlayerNames[3] == name)
            {
                ClearPlayerData(3);
            }
            menuData.SyncData();
        }

        public void AddTeamPlayer(int team_id, string name, int id)
        {
            if (!menuData.IsGameLive) return; // Allow player change only if game is not live
            if (team_id < 0 || team_id > 1) return;
            if (isPlayer(name)) return;
            if (isTeamFull(team_id)) return;

            if (menuData.PlayerNames[team_id * 2] == "")
            {
                SetPlayer(team_id * 2, name, id);
            }
            else if (menuData.PlayerNames[team_id * 2 + 1] == "")
            {
                SetPlayer(team_id * 2 + 1, name, id);
            }
        }

        // public void AddPlayer(string name, int id)
        // {
        //     if (!menuData.IsGameLive) return; // Allow player change only if game is not live
        //     if (isPlayer(name)) return;
        //     if (menuData.PlayerNames[0] == "")
        //     {
        //         SetPlayer(0, name, id);
        //     }
        //     else if (menuData.PlayerNames[1] == "")
        //     {
        //         SetPlayer(1, name, id);
        //     }
        //     else if (menuData.PlayerNames[2] == "")
        //     {
        //         SetPlayer(2, name, id);
        //     }
        //     else if (menuData.PlayerNames[3] == "")
        //     {
        //         SetPlayer(3, name, id);
        //     }
        // }

        public bool isTeamFull(int team_id)
        {
            if (menuData.IsTeams)
            {
                return menuData.PlayerNames[team_id * 2] != "" && menuData.PlayerNames[team_id * 2 + 1] != "";
            }
            else
            {
                return menuData.PlayerNames[team_id * 2] != "";
            }
        }

        private void ClearPlayerData(int index)
        {
            menuData.PlayerNames[index] = "";
            menuData.PlayerIds[index] = INVALID_PLAYER_ID;
        }

        private void MovePlayerData(int fromIndex, int toIndex)
        {
            menuData.PlayerNames[toIndex] = menuData.PlayerNames[fromIndex];
            menuData.PlayerIds[toIndex] = menuData.PlayerIds[fromIndex];
            ClearPlayerData(fromIndex);
        }
        #endregion
    }
}