using System.ComponentModel;

namespace Core.CommandLine.Enum;
public enum RuleSet
{
    // Github Repo: https://github.com/ACL4SSR/ACL4SSR/tree/master
    [DefaultValue("template.rulesets.acl4ssr.yaml")]
    acl4ssr,
    // Github Repo: https://github.com/Loyalsoldier/clash-rules
    [DefaultValue("template.rulesets.loyalsoldier.yaml")]
    loyalsoldier
}