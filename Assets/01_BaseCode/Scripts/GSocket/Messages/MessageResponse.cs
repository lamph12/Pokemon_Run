using System;

[Serializable]
public class MessageResponse : MessageBase
{
    public int Status { get; set; }
    public string Msg { get; set; }

    public object Body { get; set; }

    public GError Error { get; set; }

    public bool IsSuccess => Status == 1;

    public virtual void Execute()
    {
    }
}