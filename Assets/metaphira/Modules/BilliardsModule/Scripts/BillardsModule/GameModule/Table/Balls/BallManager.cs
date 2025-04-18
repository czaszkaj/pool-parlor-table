
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

// Enum values used for manual mapping with arrays
public enum BallTextureE : int
{
    EIGHT_BALL      = 0,
    NINE_BALL       = 1,
    SNOOKER         = 2,
    EIGHT_BALL_US   = 3,
}

public enum SnookerPositionE : int
{
    BLACK       = 0,
    PINK        = 1,
    YELLOW      = 2,
    GREEN       = 3,
    BROWN       = 4,
    CUE         = 5,
    TRIANGLE    = 6
}

public enum BallType
{
    
    STRIPE,
    FULL,
    COLORED,
    RED,
    CUE_BALL,
    LAST_BALL
}

// Store all data needed for all rules
public class BallObject
{
    public GameObject unityObject;
    public BallType type;
    // Try to remove/rename those
    public Vector3 ballP, ballV, ballW;
    public Vector3 spawnPosition;
    // Rename
    public BallObject ballHitBy, contactBall;
    public int cushionHitCount;
    public bool pocketed;

}

public class BallManager : UdonSharpBehaviour
{
    // Non Serialized
    // Serialized
    [SerializeField] public Texture[] ballTextures;
    [SerializeField] public Transform[] snookerPositions;
    [SerializeField] public GameObject[] balls;
    // TODO: check what it is for
    [NonSerialized] public Vector3[] ballsP = new Vector3[16], // Position
                                     ballsV = new Vector3[16],
                                     ballsW = new Vector3[16];
    // TODO what does ballsPocekted do?
    [NonSerialized] public uint ballsPocketedLocal;
    [NonSerialized] public uint ballsPocketedOrig;
    [NonSerialized] public uint fourBallCueBallLocal;
    [SerializeField] public GameObject markerObj,
                                       marker9ball;
    [SerializeField] [HideInInspector] public BallTextureE currentTexture = BallTextureE.EIGHT_BALL;

    void Start()
    {

    }

    public Texture getTexture(BallTextureE textureType)
    {
        switch(textureType)
        {
            case BallTextureE.NINE_BALL:
                return ballTextures[1];
            case BallTextureE.SNOOKER:
                return ballTextures[2];
            case BallTextureE.EIGHT_BALL_US:
                return ballTextures[3];
            case BallTextureE.EIGHT_BALL:
            default:
                return ballTextures[0];

        }
    }
    public Texture getTexture()
    {
        return getTexture(currentTexture);
    }

    public void setTexture(BallTextureE textureType)
    {
        currentTexture = textureType;
    }

    // TODO: maybe move to Rule6Reds
    public Transform getSnookerPosition(SnookerPositionE objType)
    {
        switch(objType)
        {
            case SnookerPositionE.BLACK:
                return snookerPositions[0];
            case SnookerPositionE.PINK:
                return snookerPositions[1];
            case SnookerPositionE.YELLOW:
                return snookerPositions[2];
            case SnookerPositionE.GREEN:
                return snookerPositions[3];
            case SnookerPositionE.BROWN:
                return snookerPositions[4];
            case SnookerPositionE.CUE:
                return snookerPositions[5];
            case SnookerPositionE.TRIANGLE:
            default:
                // TODO: Check if this is in use
                return snookerPositions[6];
        }
    }

    // TODO: maybe move to Rule6Reds
    public Vector3 getSnookerPositionV3(SnookerPositionE objType)
    {
        Transform objTransform = getSnookerPosition(objType);
        return new Vector3(objTransform.localPosition.x,
                           objTransform.localPosition.y,
                           objTransform.localPosition.z);
    }

    public Vector3 getBallPosition(int ballId)
    {
        return balls[ballId].transform.position;
    }
    public Vector3 getBallLocalPosition(int ballId)
    {
        return balls[ballId].transform.localPosition;
    }
    public Vector3 getBallP(int ballId)
    {
        return ballsP[ballId];
    }
    public int getBallsLength()
    {
        return balls.Length;
    }

    public void setShadows(bool enabled)
    {
        // TODO GraphicsManager?
    }

}
