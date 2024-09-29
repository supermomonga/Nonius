using Avalonia.Controls.Metadata;
using Epoxy;

using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace Nonius.ViewModels;
public partial class ProjectsViewModel : ViewModelBase
{
    private readonly ApplicationContextViewModel _Context;
    public INotifyCollectionChangedSynchronizedViewList<ProjectViewModel> ProjectsView { get; }
    public BindableReactiveProperty<ProjectViewModel?> SelectedProject { get; } = new();
    public BindableReactiveProperty<bool> ShowEditPane { get; } = new(false);
    public Well<SplitView> SplitViewWell { get; } = Well.Factory.Create<SplitView>();
    public ProjectsViewModel(ApplicationContextViewModel context)
    {
        _Context = context;
        ProjectsView = _Context.Projects.ToNotifyCollectionChanged();

        SelectedProject.Subscribe(x => ShowEditPane.Value = x != null);

        SplitViewWell.Add(SplitView.PaneClosingEvent, (e) =>
        {
            // 勝手に閉じようとするのを防ぐ
            e.Cancel = ShowEditPane.Value;
            return ValueTask.CompletedTask;
        });
    }

    [RelayCommand]
    public async Task SaveSelectedProjectAsync()
    {
        // TODO: Create temporary state of the project and apply it only when clicked "Save" button
        if (SelectedProject.Value != null)
        {
            await _Context.SaveSettingsAsync(showNotification: true);
        }
    }

    [RelayCommand]
    public async Task AddProjectAsync()
    {
        var project = new ProjectViewModel(name: $"New Project #{ProjectsView.Count + 1}");
        _Context.Projects.Add(project);
        if (SelectedProject.Value == null)
        {
            SelectedProject.Value = project;
        }
        if (_Context.SelectedProject.Value == null && _Context.Projects.Count == 1)
        {
            _Context.SelectedProject.Value = project;
        }
        await _Context.SaveSettingsAsync(showNotification: true);
    }

    [RelayCommand]
    public async Task RemoveProjectAsync(ProjectViewModel project)
    {
        if (project != null)
        {
            if (SelectedProject.Value == project)
            {
                SelectedProject.Value = null;
            }
            if (_Context.SelectedProject.Value == project)
            {
                _Context.SelectedProject.Value = null;
            }
            _Context.Projects.Remove(project);
            await _Context.SaveSettingsAsync(showNotification: true);
        }
    }
}
