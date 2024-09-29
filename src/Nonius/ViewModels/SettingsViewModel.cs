using CommunityToolkit.Mvvm.Input;

namespace Nonius.ViewModels;

public partial class SettingsViewModel(ApplicationContextViewModel context) : ViewModelBase
{
    private readonly ApplicationContextViewModel _Context = context;

    public BindableReactiveProperty<int> ViewerPaneWidthRatio => _Context.ViewerPaneWidthRatio;

    [RelayCommand]
    public async Task SaveSettingsAsync()
        => await _Context.SaveSettingsAsync(showNotification: true);
}
