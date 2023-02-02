public class PagedResult<T>
{
    public int TotalCount { get; set; } //Total number of results in that table I.e 23 of 55
    public int PageNumber { get; set; }//Page theyre on(Results)
    public int RecordNumber { get; set; }//How many records are coming back IE 23

    public List<T> Items { get; set; }
}
