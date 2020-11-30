using UnityEngine;

/// <summary>
/// Debug Plus Manager v1.0.0
/// Made by Yukon Wainczak. Please credit me if you use any of my scripts!
/// Special thanks to Gavin Camlin for the base code!
/// </summary>
public class DebugPlusManager : Singleton<DebugPlusManager>
{
    [Header("Properties")]
    [SerializeField]
    private LogType debugLogType = LogType.All;

    public override void Awake()
    {
        base.Awake();

        SetLogType(debugLogType);
    }

    private void Start()
    {
        SetLogType(debugLogType);
    }

    private void Update()
    {
        SetLogType(debugLogType);
    }

    public void SetLogType(LogType _logType)
    {
        DebugPlus.SetLogType(_logType);
    }
}

public static class DebugPlus
{
    private static LogType currentLogType = LogType.All;

    public static void Log(string _log, LogType _logType)
    {
        if (currentLogType == LogType.All || currentLogType == _logType)
        {
            Debug.Log(_log);
        }
    }

    public static void Log(string _log, LogType _logType, GameObject _gameObject)
    {
        if (currentLogType == LogType.All || currentLogType == _logType)
        {
            Debug.Log("<b>" + _gameObject.name + "</b>\n" + _log);
        }
    }

    public static void SetLogType(LogType _logType)
    {
        currentLogType = _logType;
    }
}

public enum LogType
{
    All,
    System,
    Gameplay,
    UI
}