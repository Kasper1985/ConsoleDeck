namespace ConsoleDeck;

public static class GraphicsPathExtensions
{
    public static Rectangle Resize(this Rectangle rect, int widthChange, int heightChange)
    {
        var rectangle = new Rectangle(rect.X, rect.Y, rect.Width + widthChange, rect.Height + heightChange);
        return rectangle;
    }

    public static bool Pop(this IList<string> list, string value)
    {
        int index = list.IndexOf(value);
        if (index >= 0)
        {
            list.RemoveAt(index);
            return true;
        }
        return false;
    }

    public static bool SetContainsAll<T>(this HashSet<T> set, params T[] items)
    {
        foreach (var item in items)
        {
            if (!set.Contains(item))
                return false;
        }
        return true;
    }
}