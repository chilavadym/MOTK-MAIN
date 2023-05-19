using MOTK.Enums;

namespace MOTK.Helpers;

public class GridSortingColumn
{
    public GridSortingColumn(string? name)
    {
        Name = name;
    }

    public string? Name { get; set; }
    public ESortOrder SortOrder { get; set; }
}