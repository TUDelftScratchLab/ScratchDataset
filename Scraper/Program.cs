using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Scraper;

namespace Kragle
{
   class MainApp
   {
      static void Main(string[] args)
      {
         string param1 = "-p";//args[0];

         string path = @"C:\Users\Felienne\Dropbox\Code\KragleData\testData\";//args[1];

         if (param1=="-p")
         {
            JSONReader.ProcessJSON(path);
         }
         else
         {
            if (param1 == "-s")
            {
                 //split the shared and non-shared files
                 Split(path);
            }
            else
            {
                 JSONPropertiesReader.writeProperties(path);
            }

      }

      }

      private static void Split(string path)
      {
         //merge all local list scraped files:


         var from = @"C:\ScratchScrapeData\Random_RunLocal\files";

         DirectoryInfo d = new DirectoryInfo(from);

          FileInfo[] Files = d.GetFiles();
          int i = 0;

          foreach (FileInfo file in Files)
          {
             //get the id:
             string id = Path.GetFileNameWithoutExtension(file.Name);

             string filenameTo = @"C:\ScratchScrapeData\ScrapedFromList\files\" + id + ".sb";

             try
             {
                File.Move(file.FullName, filenameTo);
                Console.WriteLine((i * 100) / Files.Length);
             }
             catch (Exception E)
             {
                Console.WriteLine(E.Message);
                var x = 5;
             }



             i++;

          }



         
      //   string filenameNotShared = path+"properties\\notshared.sb";

      //   System.IO.StreamReader fileRead = new System.IO.StreamReader(filenameNotShared);

      //   while (!fileRead.EndOfStream)
      //   {
      //      string ID = fileRead.ReadLine();

      //      string filenameFrom = path + ID + ".sb";
      //      string filenameTo = path + "notShared\\" + ID + ".sb";

      //      File.Move(filenameFrom, filenameTo);
      //   }


      }


   }
}
