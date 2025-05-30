using Core;
using Core.CommandLine;
using Core.Meta;

namespace Stats;

public class PieChart
{
    public static string Generate(List<ProxyNode> proxies)
    {
        string charts = string.Empty;
        CommandLineOptions options = Context.Options;

        // 延迟分布
        if (options.DelayTestEnable ?? true)
        {
            List<double> delays = proxies.Select(proxy => proxy.DelayResult.Result()).ToList();
            List<KeyValuePair<string, int>> delayFrequency = Frequency(delays, range => range.start, range => $"{(int)range.start}-{(int)range.end}ms");
            charts = $"{charts}{Environment.NewLine}{PieContent("延迟分布", delayFrequency)}";
        }
        // 下载速度分布
        if (options.SpeedTestEnable ?? false)
        {
            List<double> speeds = proxies.Select(proxy => proxy.SpeedResult.Result()).ToList();
            List<KeyValuePair<string, int>> speedFrequency = Frequency(speeds, range => -range.start, range => $"{range.start / 8 / 1024 / 1024:0.00}-{range.end / 8 / 1024 / 1024:0.00}MB/s");
            charts = $"{charts}{Environment.NewLine}{PieContent("下载速度分布", speedFrequency)}";
        }
        // 地域分布
        if (options.GeoLookup ?? true)
        {
            List<KeyValuePair<string, int>> regionFrequency = proxies.GroupBy(proxy => proxy.GeoInfo.Country).Select(group => new KeyValuePair<string, int>(group.Key, group.Count())).ToList();
            charts = $"{charts}{Environment.NewLine}{PieContent("地域分布", regionFrequency)}";
        }
        // 协议分布
        List<KeyValuePair<string, int>> typeFrequency = proxies.GroupBy(proxy => proxy.Type.ToLower()).Select(group => new KeyValuePair<string, int>(group.Key, group.Count())).ToList();
        charts = $"{charts}{Environment.NewLine}{PieContent("协议分布", typeFrequency)}";

        return charts;
    }

    private static string PieContent(string title, ICollection<KeyValuePair<string, int>> frequency)
    {
        if (frequency.Count > 0)
        {
            string stats = string.Join(Environment.NewLine, frequency.Select(entry => $"\"{entry.Key}\" : {entry.Value}").ToList());
            return string.Join(Environment.NewLine, ["```mermaid", "pie showData", $"title {title}", stats, "```"]);
        }
        return string.Empty;
    }

    private static List<KeyValuePair<string, int>> Frequency(IEnumerable<double> numbers, Func<Range, object>? orderBy, Func<Range, string>? formatter)
    {
        List<Range> ranges = GetRanges(numbers);
        return numbers
            .GroupBy(number => ranges.FirstOrDefault(range => number >= range.start && number <= range.end))
            .Where(group => group.Key != null)
            .Select(group =>
            {
                group.Key.frequency = group.Count();
                return group.Key;
            })
            .OrderBy(range => (orderBy ??= (r) => r.frequency)(range))
            .Select(range =>
            {
                range.label = (formatter ??= (r) => $"{r.start}-{r.end}")(range);
                return new KeyValuePair<string, int>(range.label, range.frequency);
            }).ToList();
    }
    private static List<Range> GetRanges(IEnumerable<double> numbers)
    {
        double min = numbers.Min();
        double max = numbers.Max();
        // Scott's normal reference rule
        int width = (int)(Math.Ceiling(3.5 * STDEV(numbers) * Math.Pow(numbers.Count(), -1 / 3.0) / 10) * 10);
        List<Range> ranges = [];
        if (width > 0)
        {
            for (double current = min; current < max; current += width)
            {
                ranges.Add(new Range { start = current, end = current + width });
            }
        }
        else
        {
            ranges.Add(new Range { start = numbers.ElementAt(0), end = numbers.ElementAt(0) + 1 });
        }
        return ranges;
    }

    private static double STDEV(IEnumerable<double> numbers)
    {
        double sum = numbers.Sum();
        double mean = sum / numbers.Count();
        double standardDeviation = 0;
        foreach (double number in numbers)
        {
            standardDeviation += Math.Pow(number - mean, 2);
        }
        standardDeviation = Math.Sqrt(standardDeviation / numbers.Count());
        return standardDeviation;
    }

    class Range
    {
        public double start;
        public double end;
        public string label = string.Empty;
        public int frequency;
    }
}