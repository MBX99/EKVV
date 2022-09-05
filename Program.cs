using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net;


namespace EKVV_SIK
{
  class Program
  {
    public static string ID_TOKEN = "id\":";
    private static string STARTDATE_TOKEN = "DTSTART;TZID=Europe/Berlin:";
    private static string ENDDATE_TOKEN = "DTEND;TZID=Europe/Berlin:";
    private static string VALUE_TOKE = "VALUE=DATE:";
    private static string Eventstart_Token = "BEGIN:VEVENT";
    private static string Eventend_Token = "END:VEVENT";
    private static string Frequenz_Woche = "RRULE:FREQ=WEEKLY;UNTIL="; //<-- Neu hinzugefügt
    private static string Frequenz_Tag = "RRULE:FREQ=DAILY;UNTIL="; //<-- Neu hinzugefügt
    private static string Frequenz_Intervall_1 = "Z;INTERVAL=1"; //<-- Neu hinzugefügt
    private static string Frequenz_Intervall_2 = "Z;INTERVAL=2"; //<-- Neu hinzugefügt

    static void Main()
    {
      List<Raum> RaumListe = ParseRaumListe(); //läuft schon
      RaumListe.ForEach(raum => raum.AddBuchungen()); //todo
      RaumListe.ForEach(raum => raum.OeffneLueftung(SollteOffenSein(raum))); //todo

      APIWebstring_lesen();
      Id_filtern();
      long Sys_time = Systemzeit();
      string[] Raumid = Raumid_Lesen();
      for (int ctr = 0; ctr < Raumid.Length; ctr++)
      {
        string weburl = Icalurl(Raumid, ctr);
        string ical = Icalfeed(weburl);
        Console.WriteLine($"Hörsaal {Raumid[ctr]}");
        Hoersaal_zeiten(ical, Sys_time);
        Console.WriteLine($"");
        Console.WriteLine($"");
      }

      Console.WriteLine("Durchlaufen");
      Console.ReadKey();
    }

    private static bool SollteOffenSein(Raum raum)
    {
      List<Termin> termine = raum.GetTermine();
      DateTime now = DateTime.Now;
      //vorlauf (aus raum)
      //nachlauf

      termine.ForEach(buchung =>
      {
        //if
        //start - vorlauf ist vor now
        //und
        //end + nachlauf ist nach now
        // return true
      });
      return false;
    }

    private static List<Raum> ParseRaumListe()
    {
      WebClient client = new WebClient();
      string webstring = client.DownloadString("https://ekvv.uni-bielefeld.de/bisapi/v2/raum/all");
      List<RaumJSONObjekt> raumJsonObjekt = JsonSerializer.Deserialize<List<RaumJSONObjekt>>(webstring);
      List<Raum> raumListe = raumJsonObjekt.Select(objekt => new Raum(
          objekt.id.ToString(),
          objekt.raumAttribute[0].wert[0].wert,
          objekt.raumAttribute[0].wert[1].wert))
        .ToList();
      return raumListe;
    }

    public static void APIWebstring_lesen() //JSON String runterladen
    {
      WebClient client = new WebClient();
      string webstring = client.DownloadString("https://ekvv.uni-bielefeld.de/bisapi/v2/raum/all");
      char[] delim = { ',', '{', '}', };
      string[] lines = webstring.Split(delim, StringSplitOptions.RemoveEmptyEntries);
      for (int ctr = 0; ctr < lines.Length; ctr++)
      {
        lines[ctr] = lines[ctr].Remove(0, 1);
        string path = "websitestring.txt";
        File.WriteAllLines(path, lines);
      }
    }

    public static void Id_filtern() //JSON String nach Raumid filtern
    {
      string[] lesen = File.ReadAllLines("websitestring.txt");
      string path2 = "ids.txt";

      for (int ctr = 0; ctr >= lesen.Length; ctr++)
      {
        if (lesen[ctr].StartsWith(ID_TOKEN))
        {
          lesen[ctr] = lesen[ctr].Replace(ID_TOKEN, string.Empty);
          File.AppendAllText(path2, lesen[ctr] + Environment.NewLine);
        }
      }
    }

    public static long Systemzeit() //Systemzeit umrechnen
    {
      DateTime Sys_Time = System.DateTime.Now;
      long Unix_Sys_Time = ((DateTimeOffset)Sys_Time).ToUnixTimeSeconds();

      return Unix_Sys_Time;
    }

    public static string[] Raumid_Lesen() // Raumids lesen
    {
      string all = File.ReadAllText("ids.txt");
      char[] delim = { '\n', };
      string[] ids = all.Split(delim, StringSplitOptions.RemoveEmptyEntries);

      return ids;
    }

    public static string Icalurl(string[] ids, int ctr) //website url erstellen
    {
      string basisurl = "https://ekvv.uni-bielefeld.de/ws/calendar?raumId=";
      string url = basisurl + ids[ctr];
      string[] charsToRemove = new string[] { "\r" };
      foreach (var c in charsToRemove)
      {
        url = url.Replace(c, string.Empty);
      }

      return url;
    }

    public static string Icalfeed(string web) // Icalfeed downloaden als String
    {
      WebClient wClient = new WebClient();
      string str_ical = wClient.DownloadString(web);

      return str_ical;
    }

    //###################################################### Änderung in der folgenden Methode ######################################################
    public static void Hoersaal_zeiten(string strSource, long systemzeit) // Hörsaal Zeiten Auswertung Zeiten
    {
      char[] loesen = { '\n', };
      string[] lines = strSource.Split(loesen, StringSplitOptions.RemoveEmptyEntries);
      bool inevent = false;
      long result_start = 0;
      long result_ende = 0;
      long until_frequenz_day = 0; // <-- Variabel hinzugefügt
      long until_frequenz_week = 0;
      DateTime Eventstart = new DateTime(1970, 01, 01, 0, 0, 0);
      DateTime Eventende = new DateTime(1970, 01, 01, 0, 0, 0);

      for (int e = 0; e < lines.Length; e++)
      {
        int abbruch;
        if (lines[e].StartsWith(Eventstart_Token))
        {
          result_start = 0;
          result_ende = 0;
          inevent = true;
        }
        else if (lines[e].StartsWith(Eventend_Token))
        {
          inevent = false;
        }
        else if (inevent == true)
        {
          if (lines[e].StartsWith(STARTDATE_TOKEN))
          {
            string startTimeStamp = Zeitenstring_verarbeitung(lines, e, STARTDATE_TOKEN);
            Eventstart = Zeitenwandler(startTimeStamp);
            result_start = ((DateTimeOffset)Eventstart).ToUnixTimeSeconds();
          }

          if (lines[e].StartsWith(ENDDATE_TOKEN))
          {
            string endTimeStamp = Zeitenstring_verarbeitung(lines, e, ENDDATE_TOKEN);
            Eventende = Zeitenwandler(endTimeStamp);
            result_ende = ((DateTimeOffset)Eventende).ToUnixTimeSeconds();
          }

          //############################      Änderung ab Hier:       #######################################
          if (lines[e].StartsWith(Frequenz_Woche))
          {
            string last_week = Str_bearb_Frequenz(lines, e, Frequenz_Woche);
            DateTime until_week = Zeitenwandler(last_week);
            until_frequenz_week = ((DateTimeOffset)until_week).ToUnixTimeSeconds();

            for (int fre_zaehler_week = 1; until_frequenz_week >= result_start; fre_zaehler_week++)
            {
              if (systemzeit >= result_start)
              {
                // End Datum nach berechnen
                result_ende = result_ende + (604800 * fre_zaehler_week);
                // Start und Enddatums Vergleich Weitergabe an Hörsaal_ansteuerung
                break;
              }
              else
              {
                result_start = result_start + 604800;
              }
            }
          }

          if (lines[e].StartsWith(Frequenz_Tag))
          {
            string last_Tag = Str_bearb_Frequenz(lines, e, Frequenz_Tag);

            DateTime until_day = Zeitenwandler(last_Tag);
            until_frequenz_day = ((DateTimeOffset)until_day).ToUnixTimeSeconds();
            for (int fre_zaehler_day = 1; until_frequenz_day >= result_start; fre_zaehler_day++)
            {
              if (systemzeit >= result_start)
              {
                // End Datum nach berechnen
                result_ende = result_ende + (86400 * fre_zaehler_day);
                // Start und Enddatums Vergleich Weitergabe an Hörsaal_ansteuerung
                break;
              }
              else
              {
                result_start = result_start + (86400);
              }
            }
          }

          //############################      Änderung Ende       ###################################################
          abbruch = Zeitenauswertung(result_start, result_ende, systemzeit);

          if (abbruch == 1)
          {
            break;
          }
        }

        if (inevent == true)
        {
        }
      }
    }

    private static string Zeitenstring_verarbeitung(string[] lines, int e, string Datumstoken)
    {
      string TimeStamp = lines[e].Replace(Datumstoken, string.Empty).Replace(VALUE_TOKE, string.Empty);

      string[] charsToRemove_csr = new string[] { "\r" };
      foreach (var csr in charsToRemove_csr)
      {
        TimeStamp = TimeStamp.Replace(csr, string.Empty);
      }

      string[] charsToRemove_cst = new string[] { "T" };
      foreach (var cst in charsToRemove_cst)
      {
        TimeStamp = TimeStamp.Replace(cst, string.Empty);
      }

      return TimeStamp;
    }

    private static int Zeitenauswertung(long re_start, long re_ende, long systemzeit)
    {
      int abbruch = 0;
      if (re_start <= systemzeit)
      {
        if (systemzeit <= re_ende)
        {
          //Console.WriteLine($"Hörsaal {nummer}");
          Console.WriteLine($"Hörsaal {System.DateTime.Now}");
          Console.WriteLine($"Hörsaal Öffnet sich!");

          abbruch = 1;
        }
      }

      return abbruch;
    }

    private static DateTime Zeitenwandler(string Zeitstempel)
    {
      string formatString = "yyyyMMddHHmmss";
      DateTime DT_ZEIT = DateTime.ParseExact(Zeitstempel, formatString, null);
      return DT_ZEIT;
    }

    //###################################################### Änderung in der folgenden Methode ############################
    private static string Str_bearb_Frequenz(string[] lines, int e, string Frequenztoken)
    {
      string Frequenz = lines[e].Replace(Frequenztoken, string.Empty).Replace(Frequenz_Intervall_1, string.Empty)
        .Replace(Frequenz_Intervall_2, string.Empty);
      string[] charsToRemove_csr = new string[] { "\r" }; //String bearbeiten: entfernen /r
      foreach (var csr in charsToRemove_csr)
      {
        Frequenz = Frequenz.Replace(csr, string.Empty);
      }

      string[] charsToRemove_cst = new string[] { "T" };
      foreach (var cst in charsToRemove_cst)
      {
        Frequenz = Frequenz.Replace(cst, string.Empty);
      }

      return Frequenz;
    }
    // ############################################## Veränderung Ende  ###################################################
  }
}
