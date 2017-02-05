using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Autofac;
using Contracts.Entity;
using Contracts.Interfaces;
using XCoreLite.View;

namespace SmartFaceAligner.View.Search
{
    public class SearchPreviewViewModel : ViewModel
    {
        private readonly ISearchService _searchService;
        public string SearchQuery { get; set; }

        private string _downloadCap;

        public ObservableCollection<SearchItemViewModel> SearchResult { get; private set; }
        public ICommand DownloadCommand => Command(_doDownload);

        

        public SearchPreviewViewModel(ISearchService searchService)
        {
            DownloadCap = "50";
            _searchService = searchService;
            SearchResult = new ObservableCollection<SearchItemViewModel>();
        }

        public override Task Initialised()
        {
            _doSearch();
            return base.Initialised();
        }

        async void _doDownload()
        {
            int dlCap = 0;
            if (!int.TryParse(DownloadCap, out dlCap))
            {
                System.Windows.MessageBox.Show("Invalid download cap");
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            var path = dialog.SelectedPath;

            var messageBoxResult = System.Windows.MessageBox.Show($"Download to {path}?", "Download Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            var dlResult = await _searchService.SearchAndDownload(SearchQuery, path,  dlCap, 0);

            System.Windows.MessageBox.Show($"Download complete. {dlResult} items downloaded. ");
            await NavigateBackTo<HomeViewModel>();
        }

        async void _doSearch()
        {
            var result = await _searchService.SearchImages(SearchQuery, 25, 0);

            SearchItemViewModel Wrap(Value value)
            {
                var searchResultVm = Scope.Resolve<SearchItemViewModel>();
                searchResultVm.SearchResult = value;
                return searchResultVm;
            }

            result.value.ForEach(_ => SearchResult.Add(Wrap(_)));
        }

        public string DownloadCap
        {
            get { return _downloadCap; }
            set
            {
                _downloadCap = value; 
                OnPropertyChanged();
            }
        }
    }
}
