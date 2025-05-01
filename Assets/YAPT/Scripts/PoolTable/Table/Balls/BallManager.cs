
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace YAPT.PoolTable.Balls
{
    public enum TextureNameE : int
    {
        HT = 0,
        UK = 1,
        US = 2
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BallManager : UdonSharpBehaviour
    {
        [SerializeField] private Texture2D[] textures;
        [SerializeField] private GameObject[] ballsObj;

        #region UdonSharpBehaviour
        void Start()
        {
            // Init pickup Repositioner index
            for (int i = 0; i < ballsObj.Length; i++)
            {
                // Update index of pickup object Repositioner
                ballsObj[i].transform.GetChild(0).GetComponent<Repositioner>().SetIndex(i);
            }

        }
        #endregion

        #region BallManager
        public void SetTexture(TextureNameE type)
        {
            //Update texture for each ball
            for (int i = 0; i < ballsObj.Length; i++)
            {
                ballsObj[i].GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", textures[(int)type]);
            }
        }

        #endregion

        #region Testing
        public void TestTexture0()
        {
            SetTexture(TextureNameE.HT);
        }
        public void TestTexture1()
        {
            SetTexture(TextureNameE.US);
        }
        #endregion
    }
}