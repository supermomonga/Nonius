using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Nonius.ViewModels;

namespace Nonius;

public class CachedViewLocator : IDataTemplate
{
    private static readonly object LockObj = new();

    private static readonly Dictionary<object, Control> CachedViews = [];

    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            lock (LockObj)
            {
                if (CachedViews.TryGetValue(data, out var res))
                {
                    return res;
                }
                var control = (Control)Activator.CreateInstance(type)!;
                control.DataContext = data;
                CachedViews[data] = control;
                return control;
            }
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public static bool Remove(object data)
    {
        lock (LockObj)
        {
            if (CachedViews.TryGetValue(data, out var res))
            {
                res.DataContext = null;
                return CachedViews.Remove(data);
            }
            else
            {
                return false;
            }
        }
    }

    public bool Match(object? data)
        => data is ViewModelBase;
}
