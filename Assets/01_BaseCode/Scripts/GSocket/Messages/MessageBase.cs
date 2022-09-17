using System;

[Serializable]
public class MessageBase
{
    public MessageBase()
    {
        Route = "msg";
        var attrs = GetType().GetCustomAttributes(typeof(MessageAttribute), false);
        foreach (var attr in attrs)
        {
            var message = (MessageAttribute)attr;
            Name = message.Name;
            Log = message.Log;
            Route = message.Route;
            return;
        }
    }

    public string Name { get; set; }
    public bool Log { get; set; }
    public string Route { get; set; }
}