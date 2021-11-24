using System;
using System.Collections.Generic;
public class Bookmaker
{
    public string key { get; set; }
    public string title { get; set; }
    public DateTime last_update { get; set; }
    public List<Market> markets { get; set; }
}