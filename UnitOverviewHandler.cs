using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acervuline {
	class UnitOverviewHandler {

		static Dictionary<string, dynamic> outlineDict;
		static HtmlDocument workingFile;

		public static Dictionary<string, dynamic> UnitOverviewInit(string outlineFile) {

			outlineDict = new Dictionary<string, dynamic>();
			workingFile = LocalDocHandler.LoadFileForReading(Program.CurrentFolderPath + outlineFile);

			string padder = ">".PadLeft(Program.CurrentFolderPath.Length, '-');
			Console.WriteLine(padder + outlineFile);

			ParseOverview();

			return outlineDict;

		}

		public static void ParseOverview() {

			GetUnitOutlines();

		}



		private static void GetUnitOutlines() {

			Dictionary<string, dynamic> overviewDict = new Dictionary<string, dynamic>();
			Dictionary<string, dynamic> outlineHeader = ParseOutlineHeader();

			overviewDict.Add("header", outlineHeader);

			// parse the main body
			foreach(HtmlNode headerNode in workingFile.DocumentNode.SelectNodes("//div[@id='unit']//h3[position()>1]")) {

				string sectionTitle = headerNode.InnerText;
				HtmlNodeCollection sectionContent = headerNode.SelectNodes("./following-sibling::*");

				string sectionOutline = FormatSectionData(sectionTitle, sectionContent);

				overviewDict.Add(sectionTitle, sectionOutline);

			}


		} // End GetUnitOutlines

		//assessment regex <strong>Assessment name:<\/strong>(.*?)(?=<br><strong>A|$)/gmi

		// for business units
		//learning outcome regex first pass <strong>(.*?)<\/strong>(?:<br>|\n)*(.*?)(?=(<br>+)<strong>|$)
		// 2nd pass (\d)\.(\d)(.*?)<br>


		private static Dictionary<string, dynamic> ParseOutlineHeader() {

			Dictionary<string, dynamic> headerDict = new Dictionary<string, dynamic>();

			foreach(HtmlNode tableData in workingFile.DocumentNode.SelectNodes("//table/tr")) {

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

			//Console.WriteLine(sectionTitle);

			foreach(HtmlNode sectionNode in sectionContent) {

				if(sectionNode.Name == "h3") {
					break;
				}

				//Console.WriteLine(sectionNode.InnerHtml.Trim());

				sectionDetails.Add(sectionNode.InnerHtml.Trim());

			}

			details = string.Join("", sectionDetails.ToArray());
			string innerHtml = HtmlTidy.FormatHtml(sectionTitle, details.Trim());

			return details;

		}

	}

}
