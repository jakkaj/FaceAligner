using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Contracts.Interfaces;
using XCoreLite.View;

namespace SmartFaceAligner.View.Search
{
    public class SearchViewModel : ViewModel
    {
        private readonly ISearchService _searchService;

        private string _searchQuery;
        
        public SearchViewModel(ISearchService searchService)
        {
            _searchService = searchService;
        }

        async void _onSearch()
        {
            await NavigateTo<SearchPreviewViewModel>(async (vm) => vm.SearchQuery = _searchQuery);
        }

        public ICommand SearchCommand => Command(_onSearch);

        public string SearchQuery
        {
            get { return _searchQuery; }
            set
            {
                _searchQuery = value;
                
                OnPropertyChanged();
            }
        }
    }
}
