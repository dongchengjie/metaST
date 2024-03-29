 <h1><img src="./metaST/Resources/icon.ico" alt="M" width="24"/><span>etaST</span></h1>

> 一个基于<a href="https://github.com/MetaCubeX/mihomo">Clash.Meta</a>内核的节点测试 CLI 程序

## 功能

- 支持节点延迟测试、下载速度测试，筛选、过滤
- 支持根据节点地域归属重命名
- 支持按照节点地域归属分组
- 节点、代理组提供地域归属国旗 Icon
- 内置规则集 [ACL4SSR](https://github.com/ACL4SSR/ACL4SSR/tree/master)、[Loyalsoldier](https://github.com/Loyalsoldier/clash-rules) 可供选择
- 支持 Windows、Linux 系统

## 快速方法

### 快速使用

```bash
# 使用本地配置文件
metaST-<platform> --config D:/demo.yaml
# 使用远程配置文件
metaST-<platform> --config https://example.com/config.yaml
```

### 进阶使用

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

## 常见问题

1. 程序运行有什么要求吗？

   1. 程序将单独为节点开启 mixed 监听端口，不受系统代理影响，保证系统处于**非 TUN**模式即可。
   2. 测试期间降低网络使用，减少误差。

2. 配置文件支持除 Clash 配置以外（如 v2ray）的配置吗？

   不支持。仅支持 Clash 配置、proxies 节点池配置(以及前两者 Base64 编码后的配置)。

3. 支持测试的协议有哪些？

   基于 Clash.Meta 内核，支持协议详见[文档](https://wiki.metacubex.one/config/proxies/ss/)。

4. 为什么不建议将延迟测试线程数设置得过大？

   受线程调度、系统资源的限制，不可能每个线程都得到公平响应。线程数量过多，可能出现某些线程因高于平均时间才得到响应，导致测试结果偏大。

5. 为什么要为`Cloudflare`节点单独分组？

   1. `Cloudflare`的节点可能为边缘节点，存在跳 IP 的情况，测试结果仅是当时的 IP 归属。
   2. `Cloudflare`的节点没有经过反代，无法访问某些托管在`Cloudflare`上的服务。

6. 下载测试的链接有什么要求吗？

   如果需要测试`Cloudflare`节点的下载速度，需要是`Cloudflare CDN`托管的服务，详见[可用的 Cloudflare 下载测速地址](https://github.com/XIU2/CloudflareSpeedTest/issues/6)。

7. 程序的工作目录位于哪里？

   Windows：%temp%/.metaST

   Linux：tmp/.metaST
