public class exemple
{
    public static int function(String[] args)
    {
        int best = 0;
        for (String item : args)
        {
            for (int i = 0; i < item.length() - 1; i++)
            {
                for (int j = i + 1; j <= item.length(); j++)
                {
                    var sub = item.substring(i, j);
                    boolean has = true;
                    for (String other : args)
                    {
                        if (!other.contains(sub))
                            has = false;
                    }
                    if (!has)
                        continue;
                    int size = j - i;
                    if (size > best)
                        best = size;
                }
            }
        }
        return best;
    }
}