using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Acervuline {
	class UnitCrawler {

		static string unitSearchBaseURL = "https://www.qut.edu.au/study/unit-search/?prefix=";
		static Dictionary<string, dynamic> unitData, summerData;
		static Dictionary<string, dynamic> scrapeData = new Dictionary<string, dynamic>();
		static string RootFolder = ".\\Saved Pages";
		static string CurrentFolderPath;
		static HtmlDocument currentFile;
		static string currentURL;
		static long scrapeTime = 0;

		public static void CrawlFromBase() {

			// Disipline list to loop over
			HtmlDocument webDoc;


			// Create an empty folder for this dump if it doesn't already exist
			Directory.CreateDirectory(RootFolder);

			bool unitList = File.Exists(RootFolder + "\\" + "unit-list.html");

			if(!unitList) {
				webDoc = GetWebStream("https://www.qut.edu.au/study/unit-search");
				webDoc.Save(RootFolder + "\\" + "unit-list.html");
			} else {
				Stream stream;

				webDoc = new HtmlDocument();
				stream = File.OpenRead(RootFolder + "\\" + "unit-list.html");
				webDoc.Load(stream);
			}

			// The link to each unit list is stored in a td cell as an anchor tag.
			// Thank fuck for that.
			foreach(HtmlNode node in webDoc.DocumentNode.SelectNodes("//td/a[@*]")) {

				// Create a folder for this discipline using the inner text of the anchor tag.
				// I want innerText, not innerHtml because who knows what the qut designers would sneak in

				CurrentFolderPath = RootFolder + "\\" + WebUtility.HtmlDecode(node.InnerText.Trim()) + "\\";
				Directory.CreateDirectory(CurrentFolderPath);

				Console.WriteLine("Parsing section" + node.InnerText);
				LoadUnitPage(node);
				Console.WriteLine("Finished Parsing section" + node.InnerText);

				// Check for a completed parse file. This file is only created once an ENTIRE
				// unit list has been parsed.
				/*
				if (!File.Exists(CurrentFolderPath + node.InnerText.Trim() + ".txt")) {

					Console.WriteLine("Parsing section" + node.InnerText);
					LoadUnitPage(node);
					Console.WriteLine("Finished Parsing section" + node.InnerText);

				} else {

					Console.WriteLine("Skipping {0}, already parsed.", node.InnerText.Trim());

				}
				 * */
			}

			Console.WriteLine("Crawling finished. Total elapsed time: {0}ms", scrapeTime);
			Console.ReadKey();
		} // End CrawlFromBase


		/// <summary>
		/// Load the given url from a string.
		///
		/// Converts any symbols into their related html code.
		///
		/// On time out or any exception, the method will call itself again.
		///
		/// This is bad because it'd be infinite on 404. Should probably do something about that.
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

			Stopwatch sw = new Stopwatch();

			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("Getting web stream...");
			Console.WriteLine(url);

			sw.Start();

			// Attempt to load the stream, catch any exception then try again.
			try {
				stream = webReq.GetResponse().GetResponseStream();
				doc.Load(stream);
				stream.Close();

			} catch(WebException e) {

				Console.WriteLine(e);
				GetWebStream(url);

			}
			// Return the loaded stream as an HtmlDocument
			sw.Stop();
			Console.ForegroundColor = ConsoleColor.Green;
			scrapeTime += (sw.ElapsedMilliseconds/1000);
			Console.WriteLine("Document loaded (" + scrapeTime + "ms)");
			return doc;
		} // End GetWebStream


		/// <summary>
		/// Loads a unit page, given the raw html element from the disipline list
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
				if(!File.Exists(CurrentFolderPath + unitCode.Trim() + ".html")) {
					Console.WriteLine("Loading Unit Page: " + unitString);
					// Change this to a return, make unit data local to the following method
					GetUnitDetails(unitString, unitCode);
					Console.WriteLine("File Saved ({0}.html)", unitCode);
				} else {
					Console.WriteLine(unitCode + " already saved.");
				}

				// Unit data is a global, I should change this to reside only in its method
				//unitDict.Add(unitData["unitCode"], unitData);
				// Console.ReadLine();
			}

			// Convert the dictionary to JSON and appends it to a file to finalise the parse
			// string output = JsonConvert.SerializeObject(unitDict);
			// File.AppendAllText(CurrentFolderPath + unitNode.InnerText.Trim() + ".txt", output);

		} // End LoadUnitPage


		private static void GetUnitDetails(string url, string unitCode) {

			// bool summerOffer = false;
			// unitData = new Dictionary<string, dynamic>();
			// unitData.Add("summerOffer", summerOffer);
			// url = "https://www.qut.edu.au/study/unit-search/unit?unitCode=IAB270&idunit=53795";

			HtmlDocument webDoc = GetWebStream(url);
			currentURL = url;
			webDoc.Save(CurrentFolderPath + unitCode.Trim() + ".html");

			GetUnitSemesters(webDoc, unitCode.Trim());
			/*

			foreach (HtmlNode tableData in webDoc.DocumentNode.SelectNodes("//table/tr")) {

				if (tableData.ChildNodes[1].InnerHtml.Contains("Dates") || tableData.ChildNodes[1].InnerHtml.Contains("Fee Type")) {

					summerOffer = true;
					unitData["summerOffer"] = summerOffer;
					summerData = new Dictionary<string, dynamic>();
				}

				if (!summerOffer) {
					ParseTableRow(tableData);
				} else {
					ParseSummerData(tableData);
				}

			}

			if (summerOffer == true) {
				unitData.Add("summerDetails", summerData);
			}

			try {
				bool hasNodes = webDoc.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]").Count <= 0;
			} catch (NullReferenceException e) {
				return;
			}

			foreach (HtmlNode optionData in webDoc.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]")) {

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
			
			*/

		} // End GetUnitDetails
		private static bool GetUnitSemesters(HtmlDocument webDoc, string unitCode) {


			try {

				HtmlNodeCollection selectNodes = webDoc.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]");
				foreach(HtmlNode optionData in webDoc.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]")) {

					//string overviewTitle = ParseUnitOverviewTitle(optionData.NextSibling.InnerText);
					string overviewParticle = optionData.Attributes[0].Value;
					string semesterFile = unitCode + "_" + overviewParticle.Replace('|', '-') + ".html";

					Debug.WriteLine(currentURL);
					HtmlDocument semesterDoc = GetWebStream(currentURL + "&unitSynopsisSelection=" + overviewParticle);

					semesterDoc.Save(CurrentFolderPath + semesterFile);

					//unitSemesters.Add(semesterFile);

				}

				return true;

			} catch {
				return false;
			}


		}
	}
}
