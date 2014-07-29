using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acervuline {
    class OverviewManager {

		// legacy shit that I can't remember if I still need

		/*
		public static void ParseUnitOutline(HtmlDocument overviewData) {



		}

		public static Dictionary<string, dynamic> GetUnitOutlines(string url, string saveDir, string unitCode, string overviewParticle) {

			Dictionary<string, dynamic> overviewDict = new Dictionary<string, dynamic>();

			HtmlDocument webDoc = Program.GetWebStream(url);

			Console.WriteLine("Loading {0} Unit Outline: " + url, unitCode);

			Dictionary<string, dynamic> outlineHeader = ParseOutlineHeader(webDoc);
			overviewDict.Add("header", outlineHeader);

			webDoc.Save(saveDir + unitCode.Trim() + "_" + overviewParticle.Replace('|', '-') + ".html");

			// parse the main body
			foreach(HtmlNode headerNode in webDoc.DocumentNode.SelectNodes("//div[@id='unit']//h3[position()>1]")) {

				string sectionTitle = headerNode.InnerText;
				HtmlNodeCollection sectionContent = headerNode.SelectNodes("./following-sibling::*");

				string sectionOutline = GroupSectionData( sectionContent);

				overviewDict.Add(sectionTitle, sectionOutline);

			}

			return overviewDict;


		} // End GetUnitOutlines


		private static Dictionary<string, dynamic> ParseOutlineHeader(HtmlDocument webDoc) {

			Dictionary<string, dynamic> headerDict = new Dictionary<string, dynamic>();

			foreach(HtmlNode tableData in webDoc.DocumentNode.SelectNodes("//table/tr")) {

				string rowHeader;
				dynamic rowBody;

				//rowHeader = Program.ParseTableHeader(tableData.ChildNodes[1].InnerHtml);
				//rowBody = Program.ParseTableBody(tableData.ChildNodes[3].InnerHtml, rowHeader);

				//headerDict.Add(rowHeader, rowBody);

			}

			return headerDict;

		}


		private static string GroupSectionData(HtmlNodeCollection sectionContent) {

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

			return details;

		}

		 * */

    }
}
