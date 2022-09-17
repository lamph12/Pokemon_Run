using System;

[Serializable]
public class MessageData : MessageBase
{
    public MessageData()
    {
    }

    public MessageData(MessageBase messageBody)
    {
        Body = messageBody;
        Name = messageBody.Name;
        Route = messageBody.Route;
    }

    public object Body { get; set; }
}