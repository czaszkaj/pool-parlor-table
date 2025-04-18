#if UNITY_ANDROID
#define HT_QUEST
#endif

// Redo to const
// defines are dangerous to use
#if !HT_QUEST || true
#define HT8B_DEBUGGER
#endif

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using System;

// TODO: Make it static
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Logger : UdonSharpBehaviour
{
    [SerializeField] public BilliardsModule table;
    [SerializeField] public NetworkingManager networkingManager;
    [SerializeField] public Text ltext;

    [NonSerialized] public const int PERF_MAX = 6;
    [NonSerialized] private const int LOG_MAX = 32;
    [NonSerialized] private int LOG_LEN = 0;
    [NonSerialized] private int LOG_PTR = 0;
    [NonSerialized] private string[] LOG_LINES = new string[32];
    [NonSerialized] private float[] perfCounters = new float[PERF_MAX];
    [NonSerialized] private float[] perfTimings = new float[PERF_MAX];
    [NonSerialized] private float[] perfStart = new float[PERF_MAX];

    // Maybe we could use enums, and some enum to string instead
    // I don't like this construction
    private string[] perfNames = new string[] {
      "main",
      "physics",
      "physicsVel",
      "physicsBall",
      "physicsCushion",
      "physicsPocket"
   };
    #region Debugger
    private const string LOG_LOW = "<color=\"#ADADAD\">";
    private const string LOG_ERR = "<color=\"#B84139\">";
    private const string LOG_WARN = "<color=\"#DEC521\">";
    private const string LOG_YES = "<color=\"#69D128\">";
    private const string LOG_END = "</color>";

    // Why Init and not a CTOR? Maybe redo
    public void Init(BilliardsModule table_, NetworkingManager networkingManager_)
    {
        table = table_;
        networkingManager = networkingManager_;
    }

#if HT8B_DEBUGGER
    public void _Log(string msg)
    {
        _log(LOG_WARN + msg + LOG_END);
    }
    public void _LogYes(string msg)
    {
        _log(LOG_YES + msg + LOG_END);
    }
    public void _LogWarn(string msg)
    {
        _log(LOG_WARN + msg + LOG_END);
    }
    public void _LogError(string msg)
    {
        _log(LOG_ERR + msg + LOG_END);
    }
    public void _LogInfo(string msg)
    {
        _log(LOG_LOW + msg + LOG_END);
    }
    public void _RedrawDebugger()
    {
        redrawDebugger();
    }
#else
    public void _Log(string msg) { }
    public void _LogYes(string msg) { }
    public void _LogInfo(string msg) { }
    public void _LogWarn(string msg) { }
    public void _LogError(string msg) { }
    public void _RedrawDebugger() { }
#endif

    public void _BeginPerf(int id)
    {
        perfStart[id] = Time.realtimeSinceStartup;
    }

    public void _EndPerf(int id)
    {
        perfTimings[id] += Time.realtimeSinceStartup - perfStart[id];
        perfCounters[id]++;
    }

    private string colorText(string text, string color)
    {
        //ex. "<color=\"#95a2b8\">net(</color>"
        return "<color=\""+color+"\">"+text+"(</color>";
    }
    private string colorTextState(string key, string colorKey, string value, string colorValue = "#FFFFFF")
    {
        //ex. "<color=\"#95a2b8\">net(</color> <color=\"#4287F5\">OWNER</color> <color=\"#95a2b8\">)</color> " :
        return colorText(key+"(", colorKey) + " " + colorText(value, colorValue) + colorText(")", colorKey);
    }

    private void _log(string ln)
    {
        Debug.Log("[<color=\"#B5438F\">BilliardsModule</color>] " + ln);

        LOG_LINES[LOG_PTR++] = "[<color=\"#B5438F\">BilliardsModule</color>] " + ln + "\n";
        LOG_LEN++;

        if (LOG_PTR >= LOG_MAX)
        {
            LOG_PTR = 0;
        }

        if (LOG_LEN > LOG_MAX)
        {
            LOG_LEN = LOG_MAX;
        }

        redrawDebugger();
    }

    // Arguments to be accessible through managers
    // So init of a logger should have reference to network manager and other expected classes
    public void redrawDebugger(bool isLocalSimulationRunning = false, uint teamIdLocal = 0)
    {
        string output = "BilliardsModule ";

        // Add information about game state:
        output += Networking.IsOwner(Networking.LocalPlayer, networkingManager.gameObject) ?
            colorTextState("net", "#95a2b8", "OWNER", "#4287F5"):
            colorTextState("net", "#95a2b8", "RECVR", "#678AC2");

        output += isLocalSimulationRunning ?
            colorTextState("sim", "#95a2b8", "ACTIVE", "#4287F5"):
            colorTextState("sim", "#95a2b8", "PAUSED", "#678AC2");

        VRCPlayerApi currentOwner = Networking.GetOwner(networkingManager.gameObject);
        // TODO: Replace value with function from OwnershipManager (new)
        output += colorTextState(
            "owner",
            "#95a2b8",
            currentOwner != null ? currentOwner.displayName + ":" + currentOwner.playerId : "[null]" + "/" + teamIdLocal,
            "#4287F5");

        output += "\n---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n";

        // TODO: Replace value with function from OwnershipManager
        for (int i = 0; i < PERF_MAX; i++)
        {
            output += colorTextState(
                perfNames[i],
                 "#95a2b8",
                 (perfCounters[i] > 0 ? perfTimings[i] * 1e6 / perfCounters[i] : 0).ToString("F2") + "µs");
        }

        output += "\n---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n";

        // Update display 
        for (int i = 0; i < LOG_LEN; i++)
        {
            output += LOG_LINES[(LOG_MAX + LOG_PTR - LOG_LEN + i) % LOG_MAX];
        }

        ltext.text = output;
    }
    #endregion
}
