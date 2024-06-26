
/// <summary>
/// SplitOption class to hold the split options
/// </summary>
public class SplitOption
{
    public SplitOption()
    {
        SplitBy = SplitBy.Page;
        Enable = false;
        Size = 0;
    }

    public SplitBy SplitBy { get; set; }
    public bool Enable { get; set; }
    public int Size { get; set; }
    public bool AddReferencePage { get; set; }

    public bool IsValid()
    {
        return Enable && Size > 0;
    }

}

/// <summary>
/// SplitBy enum to hold the split by options
/// </summary>
public enum SplitBy
{
    Page,
    Size
}