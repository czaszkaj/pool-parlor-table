
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YAPT.PoolTable.Game;
using YAPT.PoolTable.Rules;
using YAPT.PoolTable.Rules.FourBall;

namespace YAPT.PoolTable.Balls
{
    public enum EightBallTextureE : int
    {
        HT = 0,
        UK = 1,
        US = 2
    }
    public enum NineBallTextureE : int
    {
        HT = 3
    }
    public enum FourBallTextureE : int
    {
        HT = 3
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BallManager : UdonSharpBehaviour
    {
        public const int MAX_BALLS = 17; // ballsObj.Length
        [SerializeField] private Texture2D[] textures;
        [SerializeField] private RuleBase[] rules;
        [SerializeField] private GameObject[] ballsObj;

        // Default ball values (7ft)
        private Vector3 cueBallPosition = new Vector3(-0.555f, 0, 0);
        private float triangleBallPosition = 0.385f;

        private const float BALL_RADIUS = 0.03f,
                            BALL_DIAMETRE = 0.06f,
                            BALL_PL_X = 0.03f, // break placement X
                            BALL_PL_Y = 0.05196152422f, // sin(60) * 0.06
                            RANDOMIZE_F = 0.0001f,
                            SPOT_POSITION_X = 0.5334f, // First X position of the racked balls
                            SPOT_CAROM_X = 0.8001f; // Spot position for carom mode
        private RuleBase activeRule;
        Texture2D forceTexture = null;

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

        public void StartGame(GameModeType gameType)
        {
            SetRule(gameType);

            forceTexture = activeRule.GetTexture();
            if (forceTexture != null)
            {
                SetTexture(forceTexture);
            }

            activeRule.ActivateBalls(ballsObj);
            activeRule.SetBallsPosition(ballsObj);
        }

        public void EndGame(GameModeType gameType)
        {
            forceTexture = null;
        }

        public void SetTexture(EightBallTextureE type)
        {
            SetTexture(textures[(int)type]);
        }

        public void SetTexture(Texture2D texture)
        {
            if (forceTexture != null) return;
            //Update texture for each ball
            for (int i = 0; i < ballsObj.Length; i++)
            {
                ballsObj[i].GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", texture);
            }
        }

        private void SetRule(GameModeType gameType)
        {
            switch (gameType)
            {
                case GameModeType.EIGHT_BALL:
                    activeRule = rules[0];
                    break;
                case GameModeType.NINE_BALL:
                    activeRule = rules[1];
                    break;
                case GameModeType.FOUR_BALL:
                    activeRule = rules[2];
                    ((Rule4Ball)activeRule).SetKorean(false);
                    break;
                case GameModeType.FOUR_BALL_KR:
                    activeRule = rules[2];
                    ((Rule4Ball)activeRule).SetKorean(true);
                    break;
                case GameModeType.SIX_REDS:
                    activeRule = rules[3];
                    break;
            }

        }
        #endregion // BallManager

        #region Testing
        public void TestTexture0()
        {
            SetTexture(EightBallTextureE.HT);
        }
        public void TestTexture1()
        {
            SetTexture(EightBallTextureE.US);
        }
        #endregion
    }
}