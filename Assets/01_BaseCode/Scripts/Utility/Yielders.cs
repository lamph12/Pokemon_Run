using System.Collections.Generic;
using UnityEngine;

public static class Yielders
{
    private static readonly Dictionary<float, WaitForSeconds>
        _timeInterval = new Dictionary<float, WaitForSeconds>(100);

    public static WaitForEndOfFrame EndOfFrame { get; } = new WaitForEndOfFrame();

    public static WaitForFixedUpdate FixedUpdate { get; } = new WaitForFixedUpdate();

    public static WaitForSeconds Get(float seconds)
    {
        if (!_timeInterval.ContainsKey(seconds))
            _timeInterval.Add(seconds, new WaitForSeconds(seconds));
        return _timeInterval[seconds];
    }
}