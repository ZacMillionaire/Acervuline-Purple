using System.Diagnostics;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acervuline {
	class UnitOverviewHandler {

		static Dictionary<string, dynamic> outlineDict, overviewDict;
		static HtmlDocument workingFile;

		static Dictionary<string, int> wordFrequency = new Dictionary<string, int>();
		static Dictionary<string, double> tagWeights;

		public static Dictionary<string, dynamic> UnitOverviewInit(string outlineFile) {

			outlineDict = new Dictionary<string, dynamic>();
			workingFile = LocalDocHandler.LoadFileForReading(Program.CurrentFolderPath + outlineFile);

			string padder = ">".PadLeft(Program.CurrentFolderPath.Length, '-');
			Console.WriteLine(padder + outlineFile);

			ParseOverview(outlineFile);

			return outlineDict;

		}

		public static void ParseOverview(string outlineFile) {

			GetUnitOutlines();

			outlineDict = overviewDict;
			/*
			tagWeights = TagBuilder.TFIFD();

			tagWeights = tagWeights.OrderBy(x => x.Value).Reverse().ToDictionary(x => x.Key, x => x.Value);
			string pressXtoJSON = JsonConvert.SerializeObject(tagWeights);
			 * */
			

		}



		private static void GetUnitOutlines() {

			overviewDict = new Dictionary<string, dynamic>();
			Dictionary<string, dynamic> outlineHeader = ParseOutlineHeader();

			overviewDict.Add("header", outlineHeader);

			// parse the main body
			foreach(HtmlNode headerNode in workingFile.DocumentNode.SelectNodes("//div[@id='unit']//h3[position()>1]")) {

				string sectionTitle = headerNode.InnerText.ToCamelCase();
				HtmlNodeCollection sectionContent = headerNode.SelectNodes("./following-sibling::*");

				string sectionOutline = FormatSectionData(sectionTitle, sectionContent);
				//wordFrequency = TagBuilder.BuildTagDictionary(sectionOutline);
				
				overviewDict.Add(sectionTitle, sectionOutline);

			}

			Debug.WriteLine("");

		} // End GetUnitOutlines


		private static Dictionary<string, dynamic> ParseOutlineHeader() {

			Dictionary<string, dynamic> headerDict = new Dictionary<string, dynamic>();

			foreach(HtmlNode tableData in workingFile.DocumentNode.SelectNodes("//table//tr")) {

				string rowHeader;
				dynamic rowBody;

				rowHeader = HtmlDocumentHandler.ParseTableHeader(tableData.ChildNodes[1].InnerHtml);
				rowBody = HtmlDocumentHandler.ParseTableBody(tableData.ChildNodes[3].InnerHtml, rowHeader);

				headerDict.Add(rowHeader, rowBody);

			}

			return headerDict;

		}

		private static string FormatSectionData(string sectionTitle, HtmlNodeCollection sectionContent) {

			List<string> sectionDetails = new List<string>();
			string details;

			foreach(HtmlNode sectionNode in sectionContent) {

				if(sectionNode.Name == "h3") {
					break;
				}

				sectionDetails.Add(sectionNode.OuterHtml.Trim());

			}

			details = string.Join("", sectionDetails.ToArray());
			string innerHtml = HtmlTidy.FormatHtml(sectionTitle, details.Trim());

			return innerHtml;

		}

	}

}
