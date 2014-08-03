using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acervuline {
	class HtmlDocumentHandler {

		static Dictionary<string, dynamic> unitData, summerData;
		static List<string> unitSemesters;
		static HtmlDocument workingFile;
		static bool unitHasSemesters;


		/// <summary>
		///		Begins parsing the current given HtmlDocument
		/// </summary>
		/// <param name="currentFile"></param>
		public static Dictionary<string, dynamic> ParseCurrentUnitFile(HtmlDocument currentFile) {

			workingFile = currentFile;
			unitData = new Dictionary<string, dynamic>();
			unitData.Add("semesters", new Dictionary<string, dynamic>());

			ParseHeaderData();
			GetUnitSynopsis();
			unitHasSemesters = GetUnitSemesters();

			if(unitHasSemesters) {

				foreach(string overviewFile in unitSemesters){

					Dictionary<string,dynamic> parsedOutline = UnitOverviewHandler.UnitOverviewInit(overviewFile);

					Regex overviewTitleRegex = new Regex(@"(?:(?:SEM-(1|2))|([0-9][a-zA-Z]+[0-9])|(SUM))-(\d+)");
					MatchCollection overviewTitle = overviewTitleRegex.Matches(overviewFile);

					unitData["semesters"].Add(overviewTitle[0].Groups[0].ToString(), parsedOutline);

				}


			}

			return unitData;

			//disciplineData.Add(unitData["unitCode"],unitData);

			//Console.ReadLine();

		} // End ParseCurrentUnitFile

		private static void ParseHeaderData() {

			bool summerOffer = false;
			summerData = new Dictionary<string, dynamic>();

			foreach(HtmlNode tableData in workingFile.DocumentNode.SelectNodes("//table/tr")) {

				if(tableData.ChildNodes[1].InnerHtml.Contains("Dates") || tableData.ChildNodes[1].InnerHtml.Contains("Fee Type")) {
					summerOffer = true;
					unitData["summerOffer"] = summerOffer;
				}

				if(!summerOffer) {
					ParseTableRow(tableData);
				} else {

					Dictionary<string, dynamic> parsedData = ParseSummerData(tableData);

					summerData.Add(parsedData.Keys.ElementAt(0), parsedData.Values.ElementAt(0));
				}

			}
			if(summerOffer == true) {
				unitData.Add("summerDetails", summerData);
			}

		}

		private static void GetUnitSynopsis() {

			try {
				HtmlNodeCollection sectionContent = workingFile.DocumentNode.SelectNodes("//div[@class='content']//h3/following-sibling::div[1]");

				string unitSynopsis = sectionContent[0].InnerText;

				unitData.Add("synopsis", unitSynopsis);
			}
			catch {
				unitData.Add("synopsis", "This unit has no synopsis");
			}

		}

		private static bool GetUnitSemesters() {

			unitSemesters = new List<string>();

			try {

				HtmlNodeCollection selectNodes = workingFile.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]");
				foreach(HtmlNode optionData in workingFile.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]")) {

					string overviewTitle = ParseUnitOverviewTitle(optionData.NextSibling.InnerText);
					string overviewParticle = optionData.Attributes[0].Value;
					string semesterFile = unitData["unitCode"] + "_" + overviewParticle.Replace('|', '-') + ".html";

					unitSemesters.Add(semesterFile);

				}

				return true;

			}
			catch {
				return false;
			}
			

		}

		private static void ParseTableRow(HtmlNode tableRow) {

			string rowHeader;
			dynamic rowBody;

			rowHeader = ParseTableHeader(tableRow.ChildNodes[1].InnerHtml);
			rowBody = ParseTableBody(tableRow.ChildNodes[3].InnerHtml, rowHeader);

			unitData.Add(rowHeader, rowBody);

		}

		private static Dictionary<string, dynamic> ParseSummerData(HtmlNode summerRow) {

			Dictionary<string, dynamic> parsedSummerData = new Dictionary<string, dynamic>();
			string rowHeader;
			dynamic rowBody;

			rowHeader = ParseTableHeader(summerRow.ChildNodes[1].InnerHtml);
			rowBody = ParseTableBody(summerRow.ChildNodes[3].InnerHtml, rowHeader);

			parsedSummerData.Add(rowHeader, rowBody);

			return parsedSummerData;

		}

		public static string ParseTableHeader(string headerName) {

			string category;

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
				case "Antirequisite(s):":
				case "Antirequisite(s)":
					category = "antireqs";
					break;
				case "Equivalent(s):":
				case "Equivalent(s)":
					category = "equivs";
					break;
				case "Assumed knowledge:":
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
				case "Other requisite(s):":
					category = "otherreqs";
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

		public static string ParseUnitOverviewTitle(string preformattedString) {

			return preformattedString;

		}

		//<strong>Assessment name:<\/strong>(.*?)(?=<br><strong>A|$)/gmi

		/*
		private static void GetUnitDetails(string url, string unitCode) {

			bool summerOffer = false;
			Dictionary<string,dynamic> unitData = new Dictionary<string, dynamic>();

			unitData.Add("summerOffer", summerOffer);

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

				bool hasNodes = weebDoc.DocumentNode.SelectNodes("//select[@id='unitSynopsisSelection']/option[position()>1]").Count <= 0;

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
	}
}
