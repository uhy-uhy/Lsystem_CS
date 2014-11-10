using System.Collections;
using System.Collections.Generic;

public class DelList{

    public static Queue<Cellstate> queue = new Queue<Cellstate>();
    public static void add(Cellstate c)
    {
        queue.Enqueue(c);
    }
    public static void delete()
    {
        while (queue.Count > 0)
        {
            queue.Dequeue().dead();
        }
    }
    public static void clear()
    {
        queue.Clear();
    }
}
