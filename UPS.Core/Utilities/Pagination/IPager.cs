namespace UPS.Core.Utilities.Pagination
{
    public interface IPager
    {
        int TotalRecords { get; set; }
        int TotalPages { get; set; }
        int CurrentPage { get; set; }
        int PageSize { get; set; }
    }
}
