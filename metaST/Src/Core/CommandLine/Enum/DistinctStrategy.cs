namespace Core.CommandLine.Enum;
public enum DistinctStrategy
{
    type_server,        // 协议类型+主机
    type_server_port,   // 协议类型+主机+端口号
    server,             // 主机
    server_port,        // 主机+端口号
}