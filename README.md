# metaST

> A <a href="https://github.com/MetaCubeX/mihomo">Clash.Meta</a>-based CLI program for proxy testing.

## Features

- Supports proxies delay and speed testing, filtering and sorting.
- Supports proxies renaming using GEO lookup result.
- Supports grouping proxies by region.
- Region emojis and flags provided.
- Built-in rulesets provided.
- Windows and Linux platform supported.

## Usage

### Basic

```bash
metaST-<platform> --config D:/demo.yaml
```

### Example

```bash
metaST-<platform> --config D:/demo.yaml --dt 500 --se true --sf 4096000 --sort delay --top 10 --output E:/demo_result.yaml
```

## CLI Arguments

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

--help    Display this help screen.

--version Display version information.
```
