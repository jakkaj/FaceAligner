using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using XCoreLite.View;

namespace SmartFaceAligner.View.Search
{
    public class SearchItemViewModel : ViewModel
    {
        public Value SearchResult { get; set; }

        public string ThumbnailUrl => SearchResult.thumbnailUrl;
    }
}
