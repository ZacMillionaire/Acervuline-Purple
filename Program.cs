using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Acervuline {
    class Program {

        static string unitSearchBaseURL = "https://www.qut.edu.au/study/unit-search/?prefix=";

        static Dictionary<string, dynamic> unitData, summerData;
        static Dictionary<string, dynamic> scrapeData = new Dictionary<string, dynamic>();

		public static string RootFolder = ".\\Saved Pages";
		public static string CurrentFolderPath;

		static void Main(string[] args) {

			int choice;
			bool valid = false;

			do {

				PrintMenuBanner(
					new List<string>(){
						"Acervuline v26.10.2014",
						"",
						"Select an option"
					},
					ConsoleColor.White
				);

				Console.WriteLine("1) Rebuild Data From Web");
				Console.WriteLine("2) Rebuild Data From Local");

				if(Int32.TryParse(Console.ReadLine(), out choice)) {

					switch(choice) {
						case 1:
							UnitCrawler.CrawlFromBase();
							//loadStreamFromWeb("blank");
							break;
						case 2:
							loadStreamFromLocal(RootFolder);
							break;
					}

				}

			} while(!valid);

			return;

			#region Old Code
			/*
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
			*/
			#endregion

		} // End Main

		public static void loadStreamFromWeb(string url) {
			Console.WriteLine("Load from web NYI");
		}

		public static void loadStreamFromLocal(string filepath) {

			new LocalDocHandler(filepath);

		}

		#region Console Banner Code
		/// <summary>
		/// 
		/// Prints a console banner, given a list of strings,
		/// expands to the max width of the default windows console (not powershell).
		/// 
		/// <para>
		///     List Strings greater than 78 will not wrap neatly. So manually break them if needed.
		/// </para>
		/// 
		/// <conditions>
		///     <para>Conditions</para>
		///     <para>
		///         Pre: Given a List of strings, optionally ConsoleColor type for foreground
		///         and a width for the banner.
		///     </para>
		///     <para>
		///         Post: Outputs a distinct 'banner' of the given strings in the colour you chose.
		///     </para>
		/// </conditions>
		///
		/// <author>
		///     <para>
		///         Author: Scott J. Schultz
		///     </para>
		///     <para>
		///         Date: April 2014
		///     </para>
		/// </author>
		/// 
		/// </summary>
		/// 
		/// <parameters>
		///     <param name="menuLines">
		///         A List of items. Each entry is its own line within the banner.
		///         Does not break lines to wrap.
		///     </param>
		///     <param name="foregroundColour">
		///         Foreground colour. Must be set, has no default.
		///     </param>
		///     <param name="bannerWidth">
		///         [Optional] Defines the width of the banner. Defaults to 80, best results with 80.
		///     </param>
		/// </parameters>
		public static void PrintMenuBanner(List<string> menuLines,
										   ConsoleColor foregroundColour,
										   int bannerWidth = 80) {

			// Border offset accounts for the +'s and |'s used for corners and borders respectively.
			const int BORDER_OFFSET = 2,
					  ODD_NUMBER_OFFSET = 1;
			// So realistically there's only bannerwidth - offset available space for characters
			int offsetBannerWidth = bannerWidth - BORDER_OFFSET,
				stringLength,
				paddingLength,
				paddingLeft,
				difference;

			string consoleLine;

			// --- End variable declarations --- //

			Console.ForegroundColor = foregroundColour;

			// Pad our borders and whitespace lines based on our adjusted offsets
			Console.Write("\n+{0}+", "".PadLeft(offsetBannerWidth, '-'));
			Console.Write("|{0}|", "".PadLeft(offsetBannerWidth, ' '));

			// Loop through each menu line
			foreach(var itemString in menuLines) {

				stringLength = itemString.Length;
				// Figure out how much whitespace we'll need to pad between the borders
				// To center each string within the window.
				// paddingLeft will always be the largest value

				paddingLength = (offsetBannerWidth - stringLength) / 2;
				paddingLeft = paddingLength; // Applied only to the right hand side of strings

				// Do an odd number check. Odd number strings cut the length short
				// due to lazy rounding methods above. Odd number length strings will always be one off
				// the length of the bannerWidth, if left at 80
				if((stringLength + paddingLength) != offsetBannerWidth) {
					paddingLeft += ODD_NUMBER_OFFSET;
				} // End if

				// Check that the length hasn't exceeded the max offset width after adjustments.
				// If it has, subtract the difference from paddingLength
				if((stringLength + paddingLength + paddingLeft) > offsetBannerWidth) {
					difference = (stringLength + paddingLength + paddingLeft) - offsetBannerWidth;
					paddingLength -= difference;
				} // End if

				consoleLine = String.Format(
					"|{0}{1}{2}|",
					"".PadLeft(paddingLeft, ' '),
					itemString,
					"".PadRight(paddingLength, ' ')
				);
				Console.Write(consoleLine);

			} // End foreach(item in menuLines)

			// Close our banner off and reset the console text colour to the defined default
			// (Not the OS default)
			Console.Write("|{0}|", "".PadLeft(offsetBannerWidth, ' '));
			Console.Write("+{0}+\n", "".PadLeft(offsetBannerWidth, '-'));
			Console.ForegroundColor = ConsoleColor.White;

		} // End PrintMenuBanner
		#endregion


		// Probably dead code below
		#region Old Code
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

		/*
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
		*/
        /// <summary>
        ///     Placeholder function, not quite sure if there are variants
        ///     outside of, Unit Outline: Semester 1/2 YYYY
        /// </summary>
        /// <param name="preformattedString"></param>
        /// <returns></returns>
        public static string ParseUnitOverviewTitle(string preformattedString) {

            return preformattedString;

        }
		/*
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
		*/
		/*
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
		*/
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
		#endregion

	} // End Class

} // End Namespace




// It might be the dumbest thing you've ever done, but it won't be the dumbest thing you'll ever do.