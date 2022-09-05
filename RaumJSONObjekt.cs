using System.Collections.Generic;

namespace EKVV_SIK
{
  public class RaumJSONObjekt
  {
    public string name{get; set;}
    public int id{get; set;}
    public string standort_kuerzel{get; set;}
    public int standort_id{get; set;}
    public string name_gelesen{get; set;}
    public bool experimentalausstattung{get; set;}
    public List<raumAttribut> raumAttribute{get; set;}
    public string kategorie{get; set;}
    public string kommentar{get; set;}
    public int plaetze{get; set;}
  }

  public class raumAttribut
  {
    public string id{get; set;}
    public List<Laufzeit> wert{get; set;}
    public string beschreibung{get; set;}
  }

  public class Laufzeit
  {
    public string id{get; set;}
    public int wert{get; set;}
    public string beschreibung{get; set;}

  }
}
