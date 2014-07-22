using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acervuline {
    class Program {

        static string unitSearchBaseURL = "https://www.qut.edu.au/study/unit-search/?prefix=";

        static Dictionary<string, dynamic> unitData, summerData;
        static Dictionary<string, dynamic> scrapeData = new Dictionary<string, dynamic>();

		static string RootFolder = ".\\Saved Pages";
		static string CurrentFolderPath;

        static void Main(string[] args) {

			// Disipline list to loop over 
            HtmlDocument webDoc = GetWebStream("https://www.qut.edu.au/study/unit-search");

			// Create an empty folder for this dump if it doesn't already exist
			Directory.CreateDirectory(RootFolder);

			// The link to each unit list is stored in a td cell as an anchor tag.
			// Thank fuck for that.
            foreach(HtmlNode node in webDoc.DocumentNode.SelectNodes("//td/a[@*]")) {

				// Create a folder for this discipline using the inner text of the anchor tag.
				// I want innerText, not innerHtml because who knows what the qut designers would sneak in
				CurrentFolderPath = RootFolder + "\\" + node.InnerText.Trim() + "\\";
				Directory.CreateDirectory(CurrentFolderPath);

				// Check for a completed parse file. This file is only created once an ENTIRE
				// unit list has been parsed.
				if(!File.Exists(CurrentFolderPath + node.InnerText.Trim() + ".txt")) {

					Console.WriteLine("Parsing section" + node.InnerText);

					LoadUnitPage(node);

					Console.WriteLine("Finished Parsing section"+node.InnerText);

				} else {

					Console.WriteLine("Skipping {0}, already parsed.", node.InnerText.Trim());

				}

            }

            Console.ReadKey();

        } // End Main

		/// <summary>
		///		Load the given url from a string.
		///		
		///		Converts any symbols into their related html code.
		///		
		///		On time out or any exception, the method will call itself again.
		///		
		///		This is bad because it'd be infinite on 404. Should probably do something about that.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
        public static HtmlDocument GetWebStream(string url) {

            HtmlDocument doc;
            WebRequest webReq;
            Stream stream;

            url = WebUtility.HtmlDecode(url);

            doc = new HtmlDocument();
            webReq = HttpWebRequest.Create(url);

			// Attempt to load the stream, catch any exception then try again.
			try {

				stream = webReq.GetResponse().GetResponseStream();
				doc.Load(stream);
				stream.Close();

			}
			catch(WebException e) {

				Console.WriteLine(e);

				GetWebStream(url);

			}

			// Return the loaded stream as an HtmlDocument
			return doc;

        } // End GetWebStream


		/// <summary>
		///		Loads a unit page, given the raw html element from the disipline list
		/// </summary>
		/// <param name="unitNode"></param>
        private static void LoadUnitPage(HtmlNode unitNode) {

            Dictionary<string, dynamic> unitDict = new Dictionary<string, dynamic>();

            string unitURL = WebUtility.HtmlDecode(unitNode.Attributes["href"].Value);

            HtmlDocument webDoc = GetWebStream(unitURL);

			// Roughly the same format as the disipline list, td cells and anchor tags.
            foreach(HtmlNode node in webDoc.DocumentNode.SelectNodes("//td[@valign='top']/a[@href]")) {

                string unitString = WebUtility.HtmlDecode(node.Attributes["href"].Value);
				string unitCode = node.InnerText;

                Console.WriteLine("Loading Unit Page: " + unitString);

				// Change this to a return, make unit data local to the following method
				GetUnitDetails(unitString, unitCode);

                Console.WriteLine("finished parsing page");

				// Unit data is a global, I should change this to reside only in its method
                unitDict.Add(unitData["unitCode"], unitData);

               // Console.ReadLine();

            }

			// Convert the dictionary to JSON and appends it to a file to finalise the parse
			string output = JsonConvert.SerializeObject(unitDict);
			File.AppendAllText(CurrentFolderPath + unitNode.InnerText.Trim() + ".txt", output);


        } // End LoadUnitPage

        /// <summary>
        ///     Placeholder function, not quite sure if there are variants
        ///     outside of Unit Outline: Semester 1/2 YYYY
        /// </summary>
        /// <param name="preformattedString"></param>
        /// <returns></returns>
        public static string ParseUnitOverviewTitle(string preformattedString) {

            return preformattedString;

        }

        private static void GetUnitDetails(string url, string unitCode) {

            bool summerOffer = false;

            unitData = new Dictionary<string, dynamic>();
            unitData.Add("summerOffer", summerOffer);

			//url = "https://www.qut.edu.au/study/unit-search/unit?unitCode=IAB270&idunit=53795";

            HtmlDocument webDoc = GetWebStream(url);

			webDoc.Save(CurrentFolderPath + unitCode.Trim() + ".html");

			foreach(HtmlNode tableData in webDoc.DocumentNode.SelectNodes("//table/tr")) {

				if(tableData.ChildNodes[1].InnerHtml.Contains("Dates") || tableData.ChildNodes[1].InnerHtml.Contains("Fee Type")) {
                    summerOffer = true;
                    unitData["summerOffer"] = summerOffer;
                    summerData = new Dictionary<string, dynamic>();
                }

                if(!summerOffer) {
                    ParseTableRow(tableData);
                } else {
                    ParseSummerData(tableData);
                }

            }

            if(summerOffer == true) {
                unitData.Add("summerDetails", summerData);
            }

			try {

				bool hasNodes = webDoc.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]").Count <= 0;

			}
			catch(NullReferenceException e) {
				return;
			}

            foreach(HtmlNode optionData in webDoc.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]")) {

                string overviewTitle = ParseUnitOverviewTitle(optionData.NextSibling.InnerText);
                string overviewParticle = optionData.Attributes[0].Value;
                string unitOverviewURL = string.Format(
                    "{0}&unitSynopsisSelection={1}",
                    url,
                    overviewParticle
                );

				Dictionary<string, dynamic> unitOutline = OverviewManager.GetUnitOutlines(unitOverviewURL, CurrentFolderPath, unitCode, overviewParticle);

                unitData.Add(overviewTitle, unitOutline);

            }

			//webDoc.Save();

        } // End GetUnitDetails


        private static void ParseTableRow(HtmlNode tableRow) {

            string rowHeader;
            dynamic rowBody;

            rowHeader = ParseTableHeader(tableRow.ChildNodes[1].InnerHtml);
            rowBody = ParseTableBody(tableRow.ChildNodes[3].InnerHtml, rowHeader);

            unitData.Add(rowHeader, rowBody);

        }

        private static void ParseSummerData(HtmlNode summerRow) {

            string rowHeader;
            dynamic rowBody;

            rowHeader = ParseTableHeader(summerRow.ChildNodes[1].InnerHtml);
            rowBody = ParseTableBody(summerRow.ChildNodes[3].InnerHtml, rowHeader);

            summerData.Add(rowHeader, rowBody);

        }

        /*
            Loading Unit Page: https://www.qut.edu.au/study/unit-search/unit?unitCode=AYB114
            &idunit=48502
            unitCode
            INN342
            prereqs
             INN210 or INN340 or INN122&nbsp;
            antireqs
             ITB239, INB342 &nbsp;
            equivs
             ITN239&nbsp;
            assumed
            Knowledge of IT concepts at the introductory level: Web browsing; Elementary Sta
            tistics; Basic Database Concepts; Finding library resources; and Issues involved
             in aligning business technology and information systems is assumed knowledge.&n
            bsp;
            CP
            12
            timetable

                      <a target="_blank" href="https://qutvirtual3.qut.edu.au/qvpublic/ttab_
            unit_search_p.process_teach_period_search?p_unit_cd=INN342">Details in QUT Virtu
            al</a>, if available
            avail


                        <dl>
                          <dt>
                            <strong>Gardens Point</strong>
                          </dt>
                          <dd>SEM-2</dd>
                        </dl>


            CSP
            $1,076
            DOM
            $2,124
            INT
            $3,048
        */
        public static string ParseTableHeader(string headerName) {

            string category = "";

            int firstIndex = headerName.IndexOf('<');

            if(firstIndex > 0) {
                category = headerName.Remove(firstIndex).Trim();
            } else {
                category = headerName;
            }

            switch(category) {
                case "QUT code:":
                case "QUT code":
                    category = "unitCode";
                    break;
                case "Prerequisite(s):":
                case "Prerequisite(s)":
                    category = "prereqs";
                    break;
                case "Antirequisite(s)":
                    category = "antireqs";
                    break;
                case "Equivalent(s):":
                case "Equivalent(s)":
                    category = "equivs";
                    break;
                case "Assumed knowledge":
                    category = "assumed";
                    break;
                case "Credit points:":
                case "Credit points":
                    category = "CP";
                    break;
                case "Timetable":
                    category = "timetable";
                    break;
                case "Availabilities":
                    category = "avail";
                    break;
                case "CSP student contribution":
                    category = "CSP";
                    break;
                case "Domestic tuition unit fee":
                    category = "DOM";
                    break;
                case "International unit fee":
                    category = "INT";
                    break;
                case "Dates":
                    category = "dates";
                    break;
                case "Fee Type":
                    category = "feeType";
                    break;
                case "Domestic unit fee": // Used for summer courses
                    category = "DOM";
                    break;
                case "Fee Rates":
                    category = "feeRate";
                    break;
                case "Restrictions":
                    category = "restrictions";
                    break;
                case "Notes":
                    category = "note";
                    break;
                case "Coordinator:":
                    category = "coordinator";
                    break;
                case "Phone:":
                    category = "phone";
                    break;
                case "Fax:":
                    category = "fax";
                    break;
                case "Email:":
                    category = "email";
                    break;
            }

            return category;

        }


        public static dynamic ParseTableBody(string bodyContent, string category) {

            bodyContent = bodyContent.Replace("&nbsp;", "");

            if(category == "prereqs" || category == "antireqs" || category == "equivs") {

                Regex multiUnitRegex = new Regex(@"([A-Z]{3,}[0-9]{3,})");
                MatchCollection matches = multiUnitRegex.Matches(bodyContent);

                String[] units = new String[matches.Count];

                for(int i = 0; i < matches.Count; i++) {
                    units[i] = matches[i].Value;
                }

                return units;

            } else if(category == "timetable") {

                string regex = @"href=\""(.*)\"".*>";

                Regex linkRegex = new Regex(regex);
                Match match = linkRegex.Match(bodyContent);

                String timeTableURL = match.Groups[1].Value;

                return timeTableURL;

            } else if(category == "avail") {

                return ParseAvailabilities(bodyContent);

            }

            return bodyContent.Trim();

        }

        private static Dictionary<string, List<string>> ParseAvailabilities(string availabilityData) {
            
            Dictionary<string, List<string>> availDict = new Dictionary<string, List<string>>();

            Regex semesterRegex = new Regex(@"([A-Z]{3}-*[0-9]*)");
            MatchCollection semesterMatches;

            HtmlDocument availDOM = new HtmlDocument();
                         availDOM.LoadHtml(availabilityData);

            // Something something definitionItem is still availDOM, instead of an instance of the current
            // selected node or whatever
            foreach(HtmlNode definitionItem in availDOM.DocumentNode.SelectNodes("//dl")) {

                // ./ is parent node, so in this case, the current position in the stream in the loop
                // of dl items...fucked if I know, just know that this works.
                // Need to research xpath a bit more I guess
                string campus = definitionItem.SelectSingleNode("./dt/strong").InnerText;
                string offers = definitionItem.SelectSingleNode("./dd").InnerText;

                //Console.WriteLine(campus);
                //Console.WriteLine(offers);
                semesterMatches = semesterRegex.Matches(offers);

                availDict.Add(campus, semesterMatches.Cast<Match>().Select(m => m.Value).ToList());
            }

            return availDict;

        }



    }
}




// It might be the dumbest thing you've ever done, but it won't be the dumbest thing you'll ever do.