using System.Net;

using Avalonia;
using Avalonia.Controls;

namespace Nonius.Controls;

public partial class HttpStatusCodeControl : UserControl
{
    public static readonly StyledProperty<HttpStatusCode> StatusCodeProperty =
            AvaloniaProperty.Register<HttpStatusCodeControl, HttpStatusCode>(nameof(StatusCode));

    public HttpStatusCode StatusCode
    {
        get => GetValue(StatusCodeProperty);
        set => SetValue(StatusCodeProperty, value);
    }

    public HttpStatusCodeControl()
    {
        InitializeComponent();
    }
}
