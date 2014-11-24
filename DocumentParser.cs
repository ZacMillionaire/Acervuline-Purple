using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Acervuline {

	public class UnitClass {
		public string UnitCode { get; set; }
		public string Title { get; set; }
		public string Synopsis { get; set; }
		public int CreditPoints { get; set; }
		public SemesterClass[] Semesters { get; set; }
		public FeeClass Fees { get; set; }
		public bool IsSummerUnit { get; set; }
		public SummerHeaderClass SummerHeader { get; set; }
		public HeaderClass Header { get; set; }
	}

	public class FeeClass {
		public int CSP { get; set; }
		public int DOM { get; set; }
		public int INT { get; set; }

	}

	public class UnitRequirementClass {
		public string[] Prereqs { get; set; }
		public string[] Antireqs { get; set; }
		public string[] Equivs { get; set; }
		public string[] Assumed { get; set; }
		public string[] Other { get; set; }

	}

	public class HeaderClass {
		public string UnitCode { get; set; }
		// Generally the same as the synopis listed credit points, but here just incase it changes,
		// as it is listed on each semester page.
		public int CreditPoints { get; set; }
		public UnitRequirementClass UnitRequirements { get; set; }
		// A href link, I'll find away to scrape it's internals soon enough but it's complex
		public string Timetable { get; set; }
		public AvailabilityClass[] Availability { get; set; }
		// A title stating it's type
		public StaffClass Coordinator { get; set; }
	}

	public class StaffClass {
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Title { get; set; }
		public string Phone { get; set; }
		public string Fax { get; set; }
		public string Email { get; set; }

	}

	public class AvailabilityClass {
		public string Campus { get; set; }
		public string Semester { get; set; }
	}

	public class SemesterClass {
		public HeaderClass Header { get; set; }
	}

	public class SummerHeaderClass {
		public string[] Dates { get; set; }
		public string FeeType { get; set; }
		public int DomesticFee { get; set; }
		public int InternationalFee { get; set; }
		// Often a hyperlink
		public string FeeRate { get; set; }
		// Relates to units offered to visitors and cross institute students (UQ, Griffith, TAFEs, etc)
		public string Restrictions { get; set; }
		// Relates to summer and international offered units not available to domestic students
		public string Note { get; set; }
	}

	class DocumentParser {

		//private static ParseToDB dbSystem = new ParseToDB();
		private static HtmlDocument _currentDocument;
		private bool _summerSemester = false;

		public static void ParseDocument(HtmlDocument currentFile) {

			_currentDocument = currentFile;
			GetDocumentHeader();

		}

		private static void GetDocumentHeader() {

			string title = _currentDocument.DocumentNode.SelectSingleNode("//div[@id='unit']//h2").InnerText;
			HeaderClass headerData = new HeaderClass();

			Regex unitTitleRegex = new Regex(@"[A-Z]{3,}[0-9]{3,}\s+([a-zA-Z\s\S]+)\n");
			MatchCollection titleMatches = unitTitleRegex.Matches(title);
			string UnitTitle = titleMatches[0].Groups[1].ToString();

			//unitData.Add("title", whatever);
			foreach(HtmlNode tableData in _currentDocument.DocumentNode.SelectNodes("//table/tr")) {

				if(tableData.ChildNodes[1].InnerHtml.Contains("Dates") || tableData.ChildNodes[1].InnerHtml.Contains("Fee Type")) {
					GetSummerSemesterDetails();
					break;
				}

			}
			GetSemesterDetails();
			// after if summer exists insert into

		}

		private static void GetSemesterDetails() {
			throw new NotImplementedException();
		}

		private static void GetSummerSemesterDetails() {
			throw new NotImplementedException();
		}

		private static void ParseHeaderString() {

		}
	}
}
