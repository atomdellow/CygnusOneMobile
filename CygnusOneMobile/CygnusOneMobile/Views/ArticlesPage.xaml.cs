using CygnusOneMobile.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace CygnusOneMobile.Views
{
    public partial class ArticlesPage : ContentPage
    {
        private ArticlesViewModel _viewModel;

        public ArticlesPage(ArticlesViewModel viewModel)
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

        private async void OnFilterByAuthorClicked(object sender, EventArgs e)
        {
            string? result = await DisplayPromptAsync("Filter Articles", "Enter author name or ID:", "OK", "Cancel");
            
            if (!string.IsNullOrWhiteSpace(result))
            {
                _viewModel.SelectedAuthor = result;
                _viewModel.SelectedTag = null;
            }
        }

        private async void OnFilterByTagClicked(object sender, EventArgs e)
        {
            string? result = await DisplayPromptAsync("Filter Articles", "Enter tag name:", "OK", "Cancel");
            
            if (!string.IsNullOrWhiteSpace(result))
            {
                _viewModel.SelectedTag = result;
                _viewModel.SelectedAuthor = null;
            }
        }
    }
}