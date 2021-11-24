using System;
using System.Collections.Generic;

public class Week
{
    public int WeekID;
    public List<Game> Games;

    public Week()
    {
        this.Games = new List<Game>();
    }
}