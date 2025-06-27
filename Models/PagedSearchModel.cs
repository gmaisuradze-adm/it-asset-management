namespace HospitalAssetTracker.Models
{
    public abstract class PagedSearchModel
    {
        const int maxPageSize = 50;
        public int Page { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }
    }
}
