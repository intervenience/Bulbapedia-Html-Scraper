using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Net;

namespace PokemonHtmlScraper {
    class Program {
        static void Main (string [] args) {
            //string input = Console.ReadLine ();
            int previousId = -1;
            StreamWriter sw = new StreamWriter ("output/pkmn.csv");
            var Webget = new HtmlWeb ();

            var htmlDoc = Webget.Load ("https://bulbapedia.bulbagarden.net/wiki/List_of_Pok%C3%A9mon_by_National_Pok%C3%A9dex_number");

            HtmlNodeCollection htmlNodes = htmlDoc.DocumentNode.SelectNodes ("//table");
            //nodes 1, 2, 3 and 4 are the nodes we want

            for (int i = 1; i < 5; i++) {
                HtmlNode table = htmlNodes [i];
                //Console.WriteLine ("Found: " + table.Id);
                foreach (HtmlNode row in table.SelectNodes ("tr")) {
                    //Console.WriteLine ("row");

                    //row[0] = local num (unnecessary), row[1] = national pokedex id, row[3] = pokemon name, row[4] = type 1, row[5] (if exists) = type 2
                    HtmlNodeCollection cell = row.SelectNodes ("th|td");

                    string nationalId = new string(cell [1].InnerText.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray());

                    string type1 = new string (cell [4].InnerText.Where (c => char.IsLetter (c) || char.IsDigit (c)).ToArray ());
                    string type2 = "";
                    if (cell.Count == 6) {
                        type2 = new string (cell [5].InnerText.Where (c => char.IsLetter (c) || char.IsDigit (c)).ToArray ());
                    }

                    string pkmnName = new string (cell [3].InnerText.Where (c => char.IsLetter (c) || char.IsDigit (c)).ToArray ());

                    if (pkmnName == "Farfetchd") {
                        pkmnName = "Farfetch'd";
                    }
                    if (pkmnName == "MrMime") {
                        pkmnName = "Mr._Mime";
                    }
                    if (pkmnName == "MimeJr") {
                        pkmnName = "Mime_Jr.";
                    }
                    if (pkmnName == "PorygonZ") {
                        pkmnName = "Porygon-Z";
                    }
                    if (pkmnName == "HoOh") {
                        pkmnName = "Ho-Oh";
                    }

                    Console.Write (pkmnName);
                    Console.Write (nationalId);

                    if (!Directory.Exists ("output")) {
                        Directory.CreateDirectory ("output");
                    }
                    if (!File.Exists ("output/pkmn.csv")) {
                        File.Create ("output/pkmn.csv");
                    }
                    
                    if (nationalId != "Ndex") {
                        if (Convert.ToInt32(nationalId) != previousId) {
                            if (type1 == "Fairy") {
                                type1 = "Normal";
                            }
                            if (cell.Count == 6) {
                                if (type2 == "Fairy") {
                                    type2 = "Normal";
                                }
                                sw.WriteLine (string.Format ("{0}, {1}, {2}, {3}", nationalId, pkmnName, type1, type2));
                            } else {
                                sw.WriteLine (string.Format ("{0}, {1}, {2}", nationalId, pkmnName, type1));
                            }
                            
                            Pokemon temp = GetPokemonDetails (pkmnName);
                            //Console.WriteLine (temp.ToString ());

                            sw.WriteLine (temp.ToString ());
                            previousId = Convert.ToInt32 (nationalId);
                        }
                    }

                    //for testing
                    //if (previousId == 1) {
                    //    goto End;
                    //}
                }
                
            }



        End:
            sw.Close ();
            Console.ReadLine ();
        }

        static Pokemon GetPokemonDetails (string pokemonName) {
            //general information
            string url = "https://bulbapedia.bulbagarden.net/wiki/";
            StringBuilder sb = new StringBuilder ();
            sb.Append (url);
            sb.Append (pokemonName);
            if (pokemonName == "Nidoran") {
                sb.Append ("♂");
            }
            sb.Append ("_(Pokémon)");
            Console.WriteLine (sb.ToString ());

            var webget = new HtmlWeb ();
            var htmlDoc = webget.Load (sb.ToString ());

            Pokemon p = new Pokemon ();

            HtmlNodeCollection h4 = htmlDoc.DocumentNode.SelectNodes ("//h4");

            foreach (HtmlNode label in h4) {
                if (label.FirstChild.InnerText == "Base stats") {
                    //Console.WriteLine ("Found base stats headline");

                    HtmlNode table = label.NextSibling.NextSibling;
                    if (table.InnerText == pokemonName || table.InnerText.StartsWith ("Generation")) {
                        table = label.NextSibling.NextSibling.NextSibling;
                    }

                    HtmlNodeCollection row = table.ChildNodes;

                    foreach (HtmlNode cell in row) {
                        //Console.WriteLine (cell.InnerText.Trim());
                        string [] text = cell.InnerText.Trim ().Split (' ');

                        for (int i = 0; i < text.Length; i++) {
                            string temp = new string (text[i].Where (c => char.IsLetter (c) || char.IsDigit (c)).ToArray ());
                            //Console.WriteLine (temp);
                            switch (temp) {
                                case "HP":
                                    p.hp = Convert.ToInt32 (text [i + 1]);
                                    //Console.WriteLine ("HP = " + p.hp);
                                    break;
                                case "Attack":
                                    p.atk = Convert.ToInt32 (text [i + 1]);
                                    //Console.WriteLine ("Atk = " + p.atk);
                                    break;
                                case "Defense":
                                    p.def = Convert.ToInt32 (text [i + 1]);
                                    //Console.WriteLine ("Def = " + p.def);
                                    break;
                                case "SpAtk":
                                    p.spAtk = Convert.ToInt32 (text [i + 1]);
                                    //Console.WriteLine ("SpAtk = " + p.spAtk);
                                    break;
                                case "SpDef":
                                    p.spDef = Convert.ToInt32 (text [i + 1]);
                                    //Console.WriteLine ("SpDef = " + p.spDef);
                                    break;
                                case "Speed":
                                    p.speed = Convert.ToInt32 (text [i + 1]);
                                    //Console.WriteLine ("Speed = " + p.speed);
                                    goto PostStats;
                            }
                        }                        
                    }
                }
            }

            PostStats:

            string imageLinkForward = "";
            string imageLinkBack = "";

            //Console.WriteLine ("Nodes matching img " + htmlDoc.DocumentNode.Descendants ("img").Count ());
            foreach (HtmlNode imgNode in htmlDoc.DocumentNode.Descendants("img")) {
                if (imgNode.Attributes ["src"].Value.Contains ("Spr_4p") && !imgNode.Attributes ["src"].Value.Contains ("_s.") && !imgNode.Attributes ["src"].Value.Contains ("_b_")) {
                    imageLinkForward = imgNode.Attributes ["src"].Value.TrimStart('/');
                    //Console.WriteLine (imageLinkForward);
                } else if (imgNode.Attributes ["src"].Value.Contains ("Spr_4d") && !imgNode.Attributes ["src"].Value.Contains ("_s.") && !imgNode.Attributes ["src"].Value.Contains ("_b_")) {
                    imageLinkForward = imgNode.Attributes ["src"].Value.TrimStart ('/');
                    //Console.WriteLine (imageLinkForward);
                }
                if (imgNode.Attributes ["src"].Value.Contains ("Spr_b_4p") && !imgNode.Attributes ["src"].Value.Contains ("_s.")) {
                    imageLinkBack = imgNode.Attributes ["src"].Value.TrimStart ('/');
                    //Console.WriteLine (imageLinkBack);
                } else if (imgNode.Attributes ["src"].Value.Contains ("Spr_b_4d") && !imgNode.Attributes ["src"].Value.Contains ("_s.")) {
                    imageLinkBack = imgNode.Attributes ["src"].Value.TrimStart ('/');
                    //Console.WriteLine (imageLinkBack);
                }
            }

            if (imageLinkForward.Length > 0) {
                imageLinkForward = imageLinkForward.Insert (0, "https://");
                //Console.WriteLine (imageLinkForward);
                using (var client = new WebClient ()) {
                    if (!Directory.Exists("Output/Images")) {
                        Directory.CreateDirectory ("Output/Images");
                    }
                    if (!File.Exists (string.Format ("output/Images/{0}.png", pokemonName))) {
                        client.DownloadFile (new System.Uri (imageLinkForward), string.Format ("output/Images/{0}.png", pokemonName));
                    }
                }
            }
            if (imageLinkBack.Length > 0) {
                imageLinkBack = imageLinkBack.Insert (0, "https://");
                //Console.WriteLine (imageLinkBack);
                using (var client = new WebClient ()) {
                    if (!Directory.Exists ("Output/Images")) {
                        Directory.CreateDirectory ("Output/Images");
                    }
                    if (!File.Exists (string.Format ("output/Images/{0}_b.png", pokemonName))) {
                        client.DownloadFile (new System.Uri (imageLinkBack), string.Format ("output/Images/{0}_b.png", pokemonName));
                    }
                    
                }
            }

            //Console.WriteLine ("Done");

            //learnset
            sb.Append ("/Generation_IV_learnset");

            htmlDoc = webget.Load (sb.ToString ());

            //Console.WriteLine ("Loaded: " + sb.ToString ());

            HtmlNodeCollection h4s = htmlDoc.DocumentNode.SelectNodes ("//h4");
            //Console.WriteLine (h4s.Count);

            foreach (HtmlNode nextH4 in h4s) {
                if (nextH4.FirstChild.InnerText.Contains ("leveling")) {
                    //Console.WriteLine ("Found levels");
                    //Console.WriteLine (nextH4.InnerText);
                    //p.learnset = new Dictionary<int, string> ();
                    p.learnset = new List<Tuple<int, string>> ();
                    int n = 0;
                    foreach (HtmlNode node in nextH4.NextSibling.NextSibling.ChildNodes) {
                        //Console.Write (" " + node.HasChildNodes + "\n");
                        if (node.HasChildNodes) {
                            foreach (HtmlNode tblContainer in node.ChildNodes) {
                                if (tblContainer.HasChildNodes) {
                                    foreach (HtmlNode tbl in tblContainer.ChildNodes) {
                                        if (tbl.InnerText.Contains ("Level") && tbl.HasChildNodes) {
                                            //Console.WriteLine ("Yes");
                                            int rowNum = 0;
                                            int j = 0;
                                            foreach (HtmlNode row in tbl.ChildNodes) {
                                                if (rowNum > 0) {

                                                    //actual cells
                                                    HtmlNodeCollection rowData = row.ChildNodes;
                                                    
                                                    if (rowData.Count > 0) {
                                                        if (j > 0) {
                                                            //Console.WriteLine (new string (row.ChildNodes[1].InnerText.Where (c => char.IsLetter (c) || char.IsDigit (c)).ToArray ()));
                                                            int level = Convert.ToInt32 (new string (row.ChildNodes [1].InnerText.Where (c => char.IsLetter (c) || char.IsDigit (c)).ToArray ()).Substring (0, 2));
                                                            //Console.WriteLine (level);

                                                            string moveName = row.ChildNodes [3].InnerText.Trim ();
                                                            //Console.WriteLine (moveName);

                                                            Tuple<int, string> tuple = new Tuple<int, string> (level, moveName);
                                                            p.learnset.Add (tuple);
                                                        }
                                                        j++;
                                                    }
                                                }
                                                
                                                rowNum++;
                                            }
                                        }
                                    }
                                    n++;
                                }
                            }
                        }
                    }
                    

                } else if (nextH4.FirstChild.InnerText.Contains ("TM/HM")) {
                    p.tmHmLearnset = new List<string> ();
                    //Console.WriteLine ("Found Teachables");
                    //Console.WriteLine (nextH4.InnerText);
                    int nodeCount = 0;
                    foreach (HtmlNode node in nextH4.NextSibling.NextSibling.ChildNodes) {
                        //Console.Write (" " + node.HasChildNodes + "\n");

                        if (node.HasChildNodes && nodeCount >= 5) {
                            
                            HtmlNodeCollection rows = node.ChildNodes;
                            if (rows.Count > 4) {
                                //Console.WriteLine (rows [5].InnerText.Trim ());
                                p.tmHmLearnset.Add (rows [5].InnerText.Trim ());
                            }
                        }
                        nodeCount++;
                    }
                }
            }


            //Console.ReadKey ();
            return p;
        }

    }

    class Pokemon {
        public int hp, atk, def, spAtk, spDef, speed;
        public List<Tuple<int, string>> learnset;
        public List<string> tmHmLearnset;

        public override string ToString() {
            string s = string.Format ("{0}, {1}, {2}, {3}, {4}, {5}\n", hp, atk, def, spAtk, spDef, speed);

            foreach (Tuple<int, string> tuple in learnset) {
                s += string.Format ("{0}, {1}, ", tuple.Item1, tuple.Item2);
            }
            s += "\n";

            foreach (string tmHm in tmHmLearnset) {
                s += string.Format ("{0}, ", tmHm);
            }
            return s;
        }
    }
}
