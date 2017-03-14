using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using Scraper;


namespace Kragle
{
    public class JSONReader
    {
       public class Script
       {
          public JsonArray Code;
          public string Scope;
          public string ScopeName;
          public string Location;
          public string ScriptId;
          public string ProgramId;

          public Script(JsonArray code, string scope, string scopeName, string location, string scriptId, string programId)
          {
             Code = code;
             Scope = scope;
             ScopeName = scopeName;
             Location = location;
             ScriptId = scriptId;
             ProgramId = programId;
          }
       }

       public static void ProcessJSON(string path)
       {
          DirectoryInfo d = new DirectoryInfo(path);

          FileInfo[] Files = d.GetFiles();
          int i = 0;

          foreach (FileInfo file in Files)
          {
             int dot = file.Name.IndexOf(".");
             string id = file.Name.Substring(0, dot);

             string filename = file.FullName;

             System.IO.StreamReader fileRead = new System.IO.StreamReader(filename);
             string JSON = fileRead.ReadToEnd();

             var allScripts = new List<Script>();
             var allWaitScripts = new List<Script>();

             try
             {
                dynamic result = JsonValue.Parse(JSON);

                //in scripts we get all scripts at the level of the stage (the background)
                foreach (var script in result.scripts)
                {
                   string location = script[0] + "-" + script[1]; 

                   //as script id we used the number of scripts saved so far.

                   Script s = new Script(script[2], "stage", "stage", location, allScripts.Count().ToString(), id); //we only take the third element, the first two are the location.
                   allScripts.Add(s);

                   string scopeType = s.Scope;
                   string scopeName = s.ScopeName;

                   foreach (var w in allWaits(script[2], s, ref scopeType, ref scopeName))
                   {
                      allWaitScripts.Add(w);
                   }

                   WriteToFile(s, path);
                }

                //the children are all the sprites, each one has its ows name and code
                foreach (var sprite in result.children)
                {
                   string spriteName = sprite.objName; 

                   foreach (var script in sprite.scripts)
                   {
                      string location = script[0] + "-" + script[1]; 

                      Script s = new Script(script[2], "sprite", spriteName, location, allScripts.Count().ToString(), id); //we only take the third element, the first two are the location.
                      allScripts.Add(s);

                      string scopeType = s.Scope;
                      string scopeName = s.ScopeName;

                      foreach (var w in allWaits(script[2], s, ref scopeType, ref scopeName))
                      {
                         allWaitScripts.Add(w);
                      }


                      WriteToFile(s, path);
                   }
                }

                cloneDetect(path, "clones", allScripts, id, false);
                cloneDetect(path, "waitClones", allWaitScripts, id,true);
             }
             catch (Exception E)
             {
                using (System.IO.StreamWriter failFile =
                new System.IO.StreamWriter(path + "output\\fail.csv", true))
                {
                   failFile.WriteLine(id);
                }
             }

             fileRead.Close();
             try
             {
                File.Move(filename, path+ "done\\" + id + ".sb");
             }
             catch (Exception)
             {
                
             }


             Console.WriteLine(i.ToString() + "-" + ((i * 100) / Files.Length).ToString() + "%");
             i++;

          }
          Console.ReadLine();
       }

       private static void cloneDetect(string path, string file, List<Script> allScripts, string id, bool WriteCode)
       {

         var scriptsbyCode = allScripts.GroupBy(x => x.Code.ToString());

         // Loop over groups.
         foreach (var code in scriptsbyCode)
         {
            //if there is a group bigger than 1 (aka A CLONE) 
            //print that thang!
            int numOccurrences = code.Count();

            if (numOccurrences > 1)
            {
               //now group the clones by their location:
               var clonesbyScope = code.GroupBy(x => x.ScopeName);

               string scopesAndOccurrences = "";
               foreach (var scope in clonesbyScope)
               {
                  scopesAndOccurrences += "," + scope.Key + ",";
                  scopesAndOccurrences += scope.Count();
               }

               if (numOccurrences != clonesbyScope.Count())
               {
                  var b = true;
               }

               string toWrite;
               if (WriteCode)
               {
                  toWrite = "\"" + code.First().ScopeName + "" + "\"," + code.First().ScriptId + "," + code.Key; 
               }
               else
               {
                  toWrite = "\"" + code.First().ScopeName + "" + "\"," + code.First().ScriptId; //we write the location of the first clone                                       
               }

               using (System.IO.StreamWriter analysisFile =
                  new System.IO.StreamWriter(path + "output\\" + file + ".csv", true))
               {
                  analysisFile.WriteLine(id + "," + numOccurrences + "," + clonesbyScope.Count() + "," + toWrite +
                                       scopesAndOccurrences );
               }
            }
         }
          
       }


       private static bool AllOneField(JsonArray script)
        {
            //determines whether all subfields of this have only one field

            //if this has more, no deal
            if (script.Count >1)
            {
                return false;
            }


            foreach (var innerScript in script)
            {
                if (innerScript is JsonArray)
                {
                    if (!AllOneField((JsonArray)innerScript))
                    {
                        return false;
                    }
                }

            }

            return true;
        }

       private static ArrayList allWaits(JsonArray script, Script S, ref string scopeType, ref string scopeName)
       {
          //S is the original script the block occur on
         //script is just code so we can recurse
          var result = new ArrayList();

          if (scopeName[0] != '"')
          {
             //not in quotes? add them
             scopeName = "\"" + scopeName + "\"";
          }


          foreach (var innerScript in script)
          {
             if (innerScript is JsonArray)
             {
                if (AllOneField((JsonArray) innerScript))
                {
                   //this is clearly not a wait block
                }
                else
                {
                   //we save all conditions for clone detection:
                   if (innerScript.Count > 0 && innerScript[0].ToString() == "\"doWaitUntil\"")
                   {
                      //create a new scriot to store this block in
                      Script s = new Script((JsonArray)innerScript, scopeType, scopeName, S.Location, S.ScriptId, S.ProgramId);
                      result.Add(s);
                   }

                   foreach (var item in allWaits((JsonArray) (innerScript), S, ref scopeType, ref scopeName))
                   {
                      result.Add(item);
                   }

                }
             }

          }
          return result;
       }


       static ArrayList flatten(ref int order, JsonArray scripts, ref string scopeType, ref string scopeName, ref int indent, string path, string id, ref int maxIndent)
        {
            var result = new ArrayList();

            if (scopeName[0] != '"')
            {
               //not in quotes? add them
               scopeName = "\"" + scopeName + "\"";
            }


            //by default we add the order, type of the scope (scene, sprite, or proc) the name of the scope and the indent
            string toPrint = scopeType + "," + scopeName + "," + indent.ToString();
            bool added = false;

            bool addOrder = true;

            foreach (var innerScript in scripts)
            {

               //if the script is primitive, we just print it.
                if (innerScript is JsonPrimitive)
                {
                   if (addOrder)
                   {
                      toPrint += "," + order + "," + innerScript;
                      order = order + 1;
                      addOrder = false;
                   }
                   else
                   {
                      toPrint += "," + innerScript;
                   }

                   added = true; //it could be that there will be more primitives (arguments) so we only print at the end
                }
                if (innerScript is JsonArray)
                {
                    if (AllOneField((JsonArray)innerScript))
                    {
                        if (innerScript.Count == 0)
                        {
                           //this is an empy array
                           if (addOrder)
                           {
                              toPrint += "," + order + ",[]";
                              order = order + 1;
                              addOrder = false;
                           }
                           else
                           {
                              toPrint += ",[]";
                           }

                        }
                        else
                        {
                           int j = indent + 1;
                           if (j>maxIndent)
	                        {
                              maxIndent = j;
	                        }
                           foreach (var item in flatten(ref order, (JsonArray)(innerScript), ref scopeType, ref scopeName, ref j, id, path, ref maxIndent))
                           {
                              result.Add(item);
                           }
                        }

                    }
                    else
                    {

                        if (innerScript.Count > 0 && innerScript[0].ToString() == "\"procDef\"")
                        {
                            //first save this definition to a separate file
                            string procdef = id + ","+ scopeName+",procDef," + innerScript[1].ToString()+"," + innerScript[2].Count.ToString(); //procdef plus name of the proc plus number of arguments
                            JSONGetter.writeStringToFile(procdef, path+"output\\procedures.csv", true);

                            toPrint += ",procdef";
                            //now set the other blocks to the scope of this proc
                            scopeType = "procDef";
                            scopeName = innerScript[1].ToString();

                            added = true;
                        }
                        else
                        {
                           int j = indent + 1;
                           if (j > maxIndent)
                           {
                              maxIndent = j;
                           }
                           foreach (var item in flatten(ref order, (JsonArray)(innerScript), ref scopeType, ref scopeName, ref j, id, path, ref maxIndent))
                            {
                                result.Add(item);
                            }
                        }
                    }


                }

            }

            if (added)
            {
               result.Add(toPrint);
            }




            return result;

        }

   public static void WriteToFile(Script s, string path)
   {
      string saveLocation = path;

      // scripts have a location that is unique so we use that as an ID.

      int indent = 0;
      string scopeType = s.Scope;
      string scopeName = s.ScopeName;
      int order = 0;

      var maxIndent = 0;

      var allStatements = flatten(ref order, s.Code, ref scopeType, ref scopeName, ref indent, path, s.ProgramId, ref maxIndent);
      foreach (var statement in allStatements)
      {
         using (System.IO.StreamWriter analysisFile =
         new System.IO.StreamWriter(saveLocation+"output\\analysis.csv", true))
         {
            analysisFile.WriteLine(s.ProgramId + "," + s.Location +"," + s.ScriptId + "," + statement);
         }
      };

      //to calculate Long Method smell we need the length of all scripts, which is the number of items in flattenSimple
      //we also save the number of statements at the top level
      //and the maximum depth.

      using (System.IO.StreamWriter scriptsFile = new System.IO.StreamWriter(saveLocation + "output\\scripts.csv", true))
      {
         scriptsFile.WriteLine(s.ProgramId + "," + s.Location + "," + s.ScriptId + "," + scopeType + "," + scopeName + "," + s.Code.Count + "," + allStatements.Count + "," + maxIndent);
      }

   }



            


            


     }


    
}
