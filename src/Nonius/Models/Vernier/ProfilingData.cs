using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;
/// <summary>
/// プロファイリングデータのルートクラス
/// </summary>
public class ProfilingData
{
    /// <summary>
    /// メタ情報
    /// </summary>
    [JsonPropertyName("meta")]
    public required Meta Meta { get; init; }

    /// <summary>
    /// ライブラリ情報のリスト
    /// </summary>
    [JsonPropertyName("libs")]
    public required List<object> Libs { get; init; }

    /// <summary>
    /// スレッド情報のリスト
    /// </summary>
    [JsonPropertyName("threads")]
    public required List<ThreadInfo> Threads { get; init; }
}
