using CygnusOneMobile.ViewModels;

namespace CygnusOneMobile.Views;

public partial class DebugPage : ContentPage
{
    private readonly DebugViewModel _viewModel;
    
    public DebugPage(DebugViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}