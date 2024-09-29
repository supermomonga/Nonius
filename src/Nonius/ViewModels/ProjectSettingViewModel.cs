using System.Runtime.InteropServices;

namespace Nonius.ViewModels;

public partial class ProjectViewModel(
    Guid? id = null,
    string? name = null,
    string? profileDataDir = null,
    string? sourceCodeDir = null,
    TimeSpan? displayDuration = null) : ViewModelBase
{
    public Guid Id { get; init; } = id ?? Guid.NewGuid();

    public BindableReactiveProperty<string> Name { get; } =
        new(string.IsNullOrEmpty(name) ? "New Project" : name);

    public BindableReactiveProperty<string> ProfileDataDir { get; } =
        new(string.IsNullOrEmpty(profileDataDir) ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) : profileDataDir);

    public BindableReactiveProperty<string?> SourceCodeDir { get; } =
        new(sourceCodeDir);

    public BindableReactiveProperty<TimeSpan?> DisplayDuration { get; } =
        new(displayDuration);
}
