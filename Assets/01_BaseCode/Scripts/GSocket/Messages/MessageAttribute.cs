using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = true)]
public class MessageAttribute : Attribute
{
    public MessageAttribute(string name, string Route = "msg", bool log = true)
    {
        Name = name;
        Log = log;
        this.Route = Route;
    }

    public string Name { set; get; }
    public bool Log { get; set; }
    public string Route { get; set; }
}