using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Json;
using System.Collections;


namespace Scraper
{
    class JSONPropertiesReader
    {
       public static bool isShared(string html)
       {
          return !html.Contains("Sorry this project is not shared");
       }
        

       public static void writeProperties(string _path)
       {
          //this method assumes that in you have scraped a number of Scratch files
          //it will then put all the corresponding properties in /properties


          DirectoryInfo d = new DirectoryInfo(_path);

          FileInfo[] Files = d.GetFiles(); //Getting files
          int i = 0;

          foreach (FileInfo file in Files)
          {
             //get the id:
             string id = Path.GetFileNameWithoutExtension(file.Name);

             string projectURL = @"https://scratch.mit.edu/projects/" + id + "/?x=" + DateTime.Now.ToString();
             //we are adding a fake quety string to prevent the browser form loading from the cache and getting old data

             var HTML = JSONGetter.GetJSON(projectURL);

             if (HTML != null)
             {
                if (isShared(HTML))
                {
                   string pathForProperties = _path + "properties\\properties.sb";

                   JSONGetter.writeStringToFile(id + ",", pathForProperties, true, false);

                   FindCountandWritetoFile(HTML, "fav-count", pathForProperties);
                   FindCountandWritetoFile(HTML, "love-count", pathForProperties);

                   FindCountandWritetoFile(HTML, "icon views", pathForProperties);
                   FindCountandWritetoFile(HTML, "icon remix-tree", pathForProperties);

                   FindCountandWritetoFile(HTML, "Shared:", pathForProperties);
                   FindCountandWritetoFile(HTML, "Modified:", pathForProperties);

                   FindUserWritetoFile(HTML, pathForProperties);
                }
                else 
                {
                   string pathForProperties = _path + "properties\\notShared.sb";
                   JSONGetter.writeStringToFile(id, pathForProperties, true, true);
                }
             }

             Console.WriteLine(i.ToString());
             i++;
          }
       }

       private static void FindCountandWritetoFile(string HTML, string toFind, string pathForProperties)
       {
          var found = HTML.IndexOf(toFind);

          if (found != -1)
          {
             var endofSpan = HTML.IndexOf("</span>", found);
             var item = HTML.Substring(found + toFind.Length + 2, endofSpan - found - toFind.Length - 2);

             var itemNoSpacesandComma = item.Replace(" ", "").Replace("&nbsp;", "").Replace("\n", "") + ",";
             if (itemNoSpacesandComma == ",")
             {
                itemNoSpacesandComma = "0,";
             }

             JSONGetter.writeStringToFile(itemNoSpacesandComma, pathForProperties, true, false);
          }
       }

       private static void FindUserWritetoFile(string HTML, string pathForProperties)
       {
          var toFind = "id=\"owner";
          var found = HTML.IndexOf(toFind);

          if (found != -1)
          {
             var endofSpan = HTML.IndexOf("</span>", found);
             var item = HTML.Substring(found + toFind.Length + 2, endofSpan - found - toFind.Length - 2);

             var itemNoSpaces = item.Replace(" ", "").Replace("&nbsp;", "").Replace("\n", "");
             JSONGetter.writeStringToFile(itemNoSpaces, pathForProperties, true, true);
          }
       }
              



        private static void WriteToFile(string file, dynamic item2)
        {
            using (System.IO.StreamWriter analysisFile =
            new System.IO.StreamWriter(file, true))
            {
                analysisFile.WriteLine(item2);
            }
            
        }



            


            


     }


    
}
