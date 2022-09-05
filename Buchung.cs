using System;

namespace EKVV_SIK
{
  public class Buchung
  {
    //unvollständige klasse, mindestens getter und setter
    private DateTime Start { get; set; }
    private DateTime End { get; set; }
    private Rule Rule { get; set; }


    public Buchung(DateTime start, DateTime end, Rule rule)
    {
        this.Start = start;
        this.End = end;
        this.Rule = rule;
    }
  }

}
