using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.Storage;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
//using Script.Common.PlayFab;

public struct DataField
{
    public string Key;
    public object Val;
}

public struct EventStreamData
{
    public string Event_Name;
    public Dictionary<string, object> Data;
}

public class UserDataServices : SingletonClass<UserDataServices>, IService
{
    private const string PRIVATE_DATAQUEUE_NEW = "PRIVATE_DATAQUEUE_V1";
    private static SortedDictionary<string, DataField> privateDataQueue_new;

    private static bool private_change = true;
    private static bool allowUpload;


    private static readonly HashSet<Type> IntegerType = new HashSet<Type>
    {
        typeof(int),
        typeof(long),
        typeof(decimal)
    };

    private static readonly HashSet<Type> FloatTypes = new HashSet<Type>
    {
        typeof(float),
        typeof(double)
    };

    private static HashSet<Type> StringType = new HashSet<Type>
    {
        typeof(string)
    };

    private readonly Subject<Unit> OnDataChange = new Subject<Unit>();
    public static string ReasonUpdateData { get; set; }


    private SortedDictionary<string, DataField> PrivateDataQueueNew
    {
        get
        {
            if (privateDataQueue_new == null)
            {
                if (!string.IsNullOrEmpty(ObscuredPrefs.GetString(PRIVATE_DATAQUEUE_NEW)))
                    privateDataQueue_new =
                        JsonConvert.DeserializeObject<SortedDictionary<string, DataField>>(
                            ObscuredPrefs.GetString(PRIVATE_DATAQUEUE_NEW));
                if (privateDataQueue_new == null)
                    privateDataQueue_new = new SortedDictionary<string, DataField>();
            }

            return privateDataQueue_new;
        }
        set => privateDataQueue_new = value;
    }


    public static int DataChangeCount
    {
        get => PlayerPrefs.GetInt("DATA_CHANGE_COUNT");
        set
        {
            PlayerPrefs.SetInt("DATA_CHANGE_COUNT", value);
            PlayerPrefs.Save();
        }
    }


    // Use this for initialization
    public void Init()
    {
        OnDataChange.ThrottleFrame(1)
            .Subscribe(_ => { UploadPrivateData(); });

        OnStreamEventChange.ThrottleFrame(1)
            .Subscribe(_ => { });
    }


    private void SaveCachedData()
    {
        try
        {
            if (private_change)
            {
                var privateValueNew = JsonConvert.SerializeObject(PrivateDataQueueNew);
                ObscuredPrefs.SetString(PRIVATE_DATAQUEUE_NEW, privateValueNew);
                ObscuredPrefs.Save();
                private_change = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Save Error" + ex.Message);
        }
    }

    private void UploadPrivateData()
    {
    }

    public void ClearPrivateQueue(string location)
    {
    }

    public bool NonternetConnection()
    {
        return Application.internetReachability == NetworkReachability.NotReachable;
    }


    public void HandlePrivateData(Dictionary<string, object> Data, List<string> ListKey, int DataVersion,
        string localtion)
    {
#if TEST_DATA
            return;
#endif
    }

    public void HandleDataChanged(string msg, Dictionary<string, object> Data, List<string> ListKey, int DataVersion,
        string localtion)
    {
    }


    public void AllowUpload(bool allow, string location)
    {
    }


    public static bool IsInteger(Type type)
    {
        return IntegerType.Contains(type) ||
               IntegerType.Contains(Nullable.GetUnderlyingType(type));
    }

    public static bool IsNumericFloat(Type type)
    {
        return FloatTypes.Contains(type) ||
               FloatTypes.Contains(Nullable.GetUnderlyingType(type));
    }

    #region Event Stream

    private const string EVENT_STREAM_CACHED = "EVENT_STREAM_CACHED";

    private static SortedDictionary<string, EventStreamData> event_queue;

    private SortedDictionary<string, EventStreamData> Event_queue
    {
        get
        {
            if (event_queue == null)
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(EVENT_STREAM_CACHED)))
                    event_queue =
                        JsonConvert.DeserializeObject<SortedDictionary<string, EventStreamData>>(
                            PlayerPrefs.GetString(EVENT_STREAM_CACHED));
                if (event_queue == null)
                    event_queue = new SortedDictionary<string, EventStreamData>();
            }

            return event_queue;
        }
        set => event_queue = value;
    }

    private readonly Subject<Unit> OnStreamEventChange = new Subject<Unit>();

    public static int DataEventCount
    {
        get => PlayerPrefs.GetInt("DATA_EVENT_COUNT");
        set
        {
            PlayerPrefs.SetInt("DATA_EVENT_COUNT", value);
            PlayerPrefs.Save();
        }
    }

    #endregion
}