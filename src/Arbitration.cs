using System;
using System.Collections.Generic;

public class Arbitration
{
    public double CurrentHomeTeamArbitration;
    public Game OriginalGameSpread;
    public Game CurrentGameSpread;

    public Arbitration(Game originalGameSpread, Game currentGameSpread)
    {
        this.OriginalGameSpread = originalGameSpread;
        this.CurrentGameSpread = currentGameSpread;
        this.CurrentHomeTeamArbitration = this.CurrentGameSpread.HomeTeamSpread - this.OriginalGameSpread.HomeTeamSpread;
    }
}