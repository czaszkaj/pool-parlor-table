
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Ui;

// Important
// * teamId [0, 1]

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
        public bool IsPlayer(string name)
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
            RemovePlayer(menuData.PlayerNames[index]);
        }

        public void RemovePlayer(string name)
        {
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

        public void AddTeamPlayer(int teamId, string name, int id)
        {
            if (teamId < 0 || teamId > 1) return;
            if (IsPlayer(name)) return;
            if (IsTeamFull(teamId)) return;

            if (menuData.PlayerNames[teamId * 2] == "")
            {
                SetPlayer(teamId * 2, name, id);
            }
            else if (menuData.PlayerNames[teamId * 2 + 1] == "")
            {
                SetPlayer(teamId * 2 + 1, name, id);
            }
        }

        // public void AddPlayer(string name, int id)
        // {
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

        public bool IsTeamFull(int teamId)
        {
            if (menuData.IsTeams)
            {
                return menuData.PlayerNames[teamId * 2] != "" && menuData.PlayerNames[teamId * 2 + 1] != "";
            }
            else
            {
                return menuData.PlayerNames[teamId * 2] != "";
            }
        }

        public bool IsSingleTeam()
        {
            return menuData.PlayerNames[2] == "" && menuData.PlayerNames[3] == "";
        }

        public string[] GetTeamNames(int teamId)
        {
            switch (teamId)
            {
                case 0:
                    return new string[] { menuData.PlayerNames[0], menuData.PlayerNames[1] };
                case 1:
                    return new string[] { menuData.PlayerNames[2], menuData.PlayerNames[3] };
                default:
                    return new string[] { "", "" };
            }
        }

        public string[] PlayerNames
        {
            get => menuData.PlayerNames;
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