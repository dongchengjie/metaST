using System.Net;
using Core.Geo;

namespace Core.Meta;

public class ProxyNode(Dictionary<dynamic, dynamic> info)
{
    public int Id { get; } = info.GetHashCode();

    public Dictionary<dynamic, dynamic> Info { get; set; } = info;

    public string Name
    {
        get { return Info["name"]; }
        set { Info["name"] = value; }
    }

    public string Type
    {
        get { return Info["type"]; }
        set { Info["type"] = value; }
    }
    public string Server
    {
        get { return Info["server"]; }
        set { Info["server"] = value; }
    }

    public int Port
    {
        get { return int.Parse(Info["port"]); }
        set { Info["port"] = value; }
    }

    public IWebProxy? Mixed { get; set; }

    public GeoInfo GeoInfo { get; set; } = new GeoInfo();
}