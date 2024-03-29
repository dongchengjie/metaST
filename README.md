 <h1><img src="./metaST/Resources/icon.ico" alt="M" width="24"/><span>etaST</span></h1>

> 一个基于<a href="https://github.com/MetaCubeX/mihomo">Clash.Meta</a>内核的节点测试 CLI 程序

## 功能

- 支持节点延迟测试、下载速度测试，筛选、过滤
- 支持根据节点地域归属重命名
- 支持按照节点地域归属分组
- 节点、代理组提供地域归属国旗 Icon
- 内置规则集 [ACL4SSR](https://github.com/ACL4SSR/ACL4SSR/tree/master)、[Loyalsoldier](https://github.com/Loyalsoldier/clash-rules) 可供选择
- 支持 Windows、Linux 系统

## 用法

### 基础用法

```bash
# 使用本地配置文件
metaST-<platform> --config D:/demo.yaml
# 使用远程配置文件
metaST-<platform> --config https://example.com/config.yaml
```

### 进阶用法

```bash
# 进行下载测试,延迟不超过500ms
# 进行下载测试,速度不小于500KB/s
# 按照延迟升序,取前20条结果
# 输出到E:/example_result.yaml
metaST-<platform> --config D:/demo.yaml --dt 500 --se true --sf 4096000 --sort delay --top 20 --output E:/example_result.yaml
```

## 命令行参数

```bash
--config  Required. clash配置文件路径(或链接地址)

--de      (Default: true) 是否进行延迟测试

--du      (Default: https://www.google.com/gen_204) 延迟测试链接

--dt      (Default: 1000) 延迟测试超时(ms)

--dn      (Default: 16) 延迟测试线程数量

--dr      (Default: 4) 延迟测试轮数

--df      (Default: 1000) 延迟测试过滤阈值(ms)

--se      (Default: false) 是否进行下载测试

--su      (Default: https://cdn.cloudflare.steamstatic.com/steam/apps/256843155/movie_max.mp4) 下载测试链接

--st      (Default: 5000) 下载测试连接超时(ms)

--sd      (Default: 10000) 下载测试时长(ms)

--sr      (Default: 1) 下载测试测试轮数

--sf      (Default: 8192000) 下载测试过滤阈值(bps)

--ff      (Default: 0.5) 快速失败比率(测试一定比率仍无结果,直接失败)

--sort    (Default: delay) 结果排序偏好 Valid values: delay, speed

--top     结果截选前若干条

--tag     节点命名前缀

--geo     (Default: true) 是否GEO查询并重命名

--gt      (Default: 10000) GEO查询超时(ms)

--group   (Default: regieon) 代理组类型 Valid values: standard, regieon

--ruleset (Default: acl4ssr) 结果配置文件使用的规则集 Valid values: acl4ssr, loyalsoldier

--mirror  (Default: https://mirror.ghproxy.com/) Github资源镜像加速地址

--output  输出路径/文件名

--verbose (Default: false) 显示详细输出

--pause   (Default: false) 程序结束后等待

--help    打印帮助信息

--version 打印版本信息
```
