using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ObservableCollection<SearchItemViewModel> SearchResult { get; private set; }

        public SearchPreviewViewModel(ISearchService searchService)
        {
            _searchService = searchService;
            SearchResult = new ObservableCollection<SearchItemViewModel>();
        }

        public override Task Initialised()
        {
            _doSearch();
            return base.Initialised();
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
    }
}
