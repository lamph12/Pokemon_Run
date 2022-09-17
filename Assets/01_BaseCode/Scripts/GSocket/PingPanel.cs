using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PingPanel : MonoBehaviour
{
    private static PingPanel _instance;

    public static int PingMs;
    public static int segment_id = -3;

    public Text infoText;
    public Text infoText_2;

    private Coroutine _coroutine;
    private string _osName;

    private int _version;

    private Ping newPing;

    private void Awake()
    {
        if (_instance)
        {
            DestroyImmediate(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _version = Config.versionCode;
#if UNITY_ANDROID
        _osName = "A.";
#else
        _osName = "I.";
#endif
        GSocket.OnLoginSuccess.Subscribe(LoginSuccessEvent).AddTo(this);
        if (_coroutine == null) _coroutine = StartCoroutine(InitPing());
    }


    private void LoginSuccessEvent(Unit unit)
    {
        StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(InitPing());
    }

    private IEnumerator InitPing()
    {
        while (true)
        {
            yield return Yielders.Get(0.5f);
            newPing = new Ping("8.8.8.8");

            while (!newPing.isDone)
            {
                PingMs = -1;
                if (!GSocket.IntenetAvaiable)
                {
                    infoText.text = $"{PingMs} ms-{_osName}{_version}.{segment_id}";
                    StopCoroutine(_coroutine);
                    yield break;
                }

                yield return null;
            }

            infoText.text = $"{newPing.time} ms-{_osName}{_version}.{segment_id}";
            PingMs = newPing.time;
        }
    }
}