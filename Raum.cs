using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using Ical.Net;

namespace EKVV_SIK
{
  public class Raum
  {
    private static string Ruletoken = "RRULE";
    //einigermaßen funktionale klasse, getter setter müssen noch korrekt eingeführt werden, methode buchungen wichtig (termine beachten)
    private string id { get; set; }
    private int lueftungsVorlauf { get; set; }
    private int lueftungsNachlauf { get; set; }
    private List<Buchung> buchungen { get; set; }
    private List<Termin> termine { get; set; }

    public Raum(string id, int lueftungsVorlauf, int lueftungsNachlauf)
    {
      this.id = id;
      this.lueftungsVorlauf = lueftungsVorlauf;
      this.lueftungsNachlauf = lueftungsNachlauf;
    }
    
    private bool open = false;

    public void OeffneLueftung(bool oeffne)
    {
      this.open = oeffne;
    }

        public void AddBuchungen()
        {

            //webclient erstellen
            string basisurl = "https://ekvv.uni-bielefeld.de/ws/calendar?raumId=";
            string url = basisurl + id;

            //download icall feed
            WebClient wClient = new WebClient();
            string ical = wClient.DownloadString(url);

            //parse zu Buchungen
            var manyCalenders = Calendar.Load(ical);

            //add Buchungen zu List: buchungen
            //List<Buchung> Buchungs = manyCalenders.Select(objekt => new Buchung(manyCalenders.Events.DtStart, manyCalenders.Events.DtEnd).ToList());
            Console.WriteLine(($"Kalendereintrag 1: {manyCalenders.Events[1].DtStart}"));
            if (manyCalenders.Events[1].Properties[8].Value == Ruletoken)
            {
                Console.WriteLine($"Hat geklappt!");
                
            }
        }




        //Liste echter Termine zusammenstellen und speichern (vorlauf und nachlauf mit in den termin) ungefähr so:
        //if (rule.frequency.eqauls(Frequency.DAILY))
        //{
        //  start.Add(TimeSpan.FromDays(1));
        //}
        // return raumListe;

        public List<Buchung> GetBuchungen()
    {
      return buchungen;
    }
    public List<Termin> GetTermine()
    {
      return termine;
    }
  }
}
