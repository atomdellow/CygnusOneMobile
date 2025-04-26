using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CygnusOneMobile.Models;
using CygnusOneMobile.Services;
using Microsoft.Maui.Controls;

namespace CygnusOneMobile.ViewModels
{
    public class ArticlesViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private bool _hasMore = true;
        private int _currentPage = 1;
        private const int PageSize = 10;
        private string _selectedAuthor;
        private string _selectedTag;

        public ObservableCollection<Article> Articles { get; } = new ObservableCollection<Article>();
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAuthor
        {
            get => _selectedAuthor;
            set
            {
                if (_selectedAuthor != value)
                {
                    _selectedAuthor = value;
                    OnPropertyChanged();
                    ResetAndRefresh();
                }
            }
        }

        public string SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (_selectedTag != value)
                {
                    _selectedTag = value;
                    OnPropertyChanged();
                    ResetAndRefresh();
                }
            }
        }

        public ICommand LoadMoreCommand { get; }
        public ICommand RefreshCommand { get; }

        public ArticlesViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadMoreCommand = new Command(async () => await LoadMoreAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadArticlesAsync(reset: true);
        }

        private async Task LoadMoreAsync()
        {
            if (IsLoading || !_hasMore)
                return;

            _currentPage++;
            await LoadArticlesAsync();
        }

        private async Task RefreshAsync()
        {
            await LoadArticlesAsync(reset: true);
        }

        private void ResetAndRefresh()
        {
            _currentPage = 1;
            _hasMore = true;
            Articles.Clear();
            Task.Run(async () => await LoadArticlesAsync());
        }

        private async Task LoadArticlesAsync(bool reset = false)
        {
            if (reset)
            {
                _currentPage = 1;
                Articles.Clear();
            }

            try
            {
                IsLoading = true;
                
                var response = await _apiService.GetArticlesAsync(
                    _currentPage, 
                    PageSize, 
                    _selectedAuthor, 
                    _selectedTag);

                if (response?.Data != null)
                {
                    foreach (var article in response.Data)
                    {
                        Articles.Add(article);
                    }

                    _hasMore = response.HasMore;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading articles: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}