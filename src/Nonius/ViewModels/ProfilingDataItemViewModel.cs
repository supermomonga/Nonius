using System.Net;
using System.Net.Http;

using Nonius.Models.Vernier;

namespace Nonius.ViewModels;

public partial class ProfilingDataItemViewModel : ViewModelBase, IComparable<ProfilingDataItemViewModel>
{
    public required string DataFilePath { get; init; }

    public required ProfilingData Data { get; init; }

    private string? _ResourceName;
    public string ResourceName =>
        _ResourceName ??= (
            ActionControllerMarker is MarkerDataItem marker &&
            marker.Controller is string controller &&
            marker.Action is string action
        ) ? $"{controller}#{action}" : "Unknown";

    private HttpMethod? _HttpMethod;
    public HttpMethod? HttpMethod =>
        _HttpMethod ??= (
            ActionControllerMarker is MarkerDataItem marker &&
            marker.Method is string method
        ) ? HttpMethod.Parse(method) : null;

    private HttpStatusCode? _HttpStatusCode;
    public HttpStatusCode? HttpStatusCode =>
        _HttpStatusCode ??= (
            ActionControllerMarker is MarkerDataItem marker &&
            marker.Status is int status
        ) ? (HttpStatusCode)status : null;

    private DateTimeOffset? _ProfiledAt;
    public DateTimeOffset ProfiledAt =>
        _ProfiledAt ??= DateTimeOffset.FromUnixTimeMilliseconds((long)Data.Meta.StartTime).ToLocalTime();

    private DateTimeOffset? _StartedAt;
    public DateTimeOffset StartedAt =>
        _StartedAt ??= DateTimeOffset.FromUnixTimeMilliseconds((long)Data.Meta.StartTime).ToLocalTime();

    private TimeOnly? _Duration;
    public TimeOnly Duration
    {
        get
        {
            if (_Duration is TimeOnly duration)
            {
                return duration;
            }
            // TODO: Use Markers if Samples is unavailable.
            // https://github.com/tenderlove/profiler/blob/979dca8b83a9bde8afb1636a847d4eda6882ee28/src/profile-logic/profile-data.js#L1076
            // https://github.com/tenderlove/profiler/blob/c679708f08d9015c2b5f905163d9099cf74ffc7a/src/profile-logic/committed-ranges.js#L167
            // https://github.com/tenderlove/profiler/blob/c679708f08d9015c2b5f905163d9099cf74ffc7a/src/utils/format-numbers.js#L323
            var minStart = Data.Threads.Min(t => t.Samples.Time.FirstOrDefault(double.MaxValue));
            var maxEnd = Data.Threads.Max(t => t.Samples.Time.LastOrDefault());
            var time = maxEnd - minStart + Data.Meta.Interval;
            return _Duration ??= TimeOnly.FromTimeSpan(TimeSpan.FromMilliseconds(time));
        }
    }

    private int? _DurationInMilliseconds;
    public int DurationInMilliseconds => _DurationInMilliseconds ??= (int)Duration.ToTimeSpan().TotalMilliseconds;

    private MarkerDataItem? _ActionControllerMarker;
    public MarkerDataItem? ActionControllerMarker
    {
        get
        {
            return _ActionControllerMarker ??=
                Data.Threads.Select(
                    t => t.Markers.Data.FirstOrDefault(m => m.Type == "process_action.action_controller")
                ).Compact().FirstOrDefault();
        }
    }

    public ProfilingDataItemViewModel() { }

    #region IComparable

    public int CompareTo(ProfilingDataItemViewModel? other)
    {
        // If other is not a valid object reference, this instance is greater.
        return other == null ? 1 : ProfiledAt.CompareTo(other.ProfiledAt);
    }

    #endregion IComparable
}
