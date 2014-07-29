using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Acervuline {

	public static class StringExtention {
		public static string ToCamelCase(this string input) {

			string camelCaseString = "";
			int wordCount = 0;

			string[] split = input.Split(' ');

			foreach(string word in split) {

				if(wordCount == 0) {
					camelCaseString += word.ToLower();
				} else {

					char[] letterArray = word.ToCharArray();

					letterArray[0] = char.ToUpper(letterArray[0]);

					camelCaseString += new String(letterArray);

				}

				wordCount++;

			}


			return camelCaseString;

		}

	}

	// AS LONG AS IT WORKS IT CAN LOOK AS TERRIBLE AS IT WANTS
	// IT WON'T BE THE BEST, BUT IT CAN CERTAINLY BE THE WORST
	class HtmlTidy {

		static string htmlFragment, fragmentTitle;

		public static string FormatHtml(string title, string html) {

			htmlFragment = html;
			fragmentTitle = title.ToCamelCase();

			GeneralPass();
			NormalizeLists();

			switch(fragmentTitle) {

				case "rationale":
					break;
				case "aims":
					break;
				case "learningOutcomes":
					break;
				case "content":
					break;
				case "approachesToTeachingAndLearning":
					break;
				case "assessment":

					// add in missing header for Assessment Submission and Extensions
					// It's not passed through for some reason
					AssessmentPass();
					break;
				case "academicIntegrity":
					break;
				case "resourceMaterials":
					break;
				case "riskAssessmentStatement":
					break;
				default:
					throw new NotImplementedException();

			}

			FinalPass();

			return htmlFragment;

		}

		private static void GeneralPass() {

			// Remove div tags
			htmlFragment = Regex.Replace(htmlFragment, @"<\/?(?:div|p)>", "");
			htmlFragment = htmlFragment.Trim();

			// convert breaks to newline chars
			htmlFragment = Regex.Replace(htmlFragment, @"(<br\s*\/?>\s*)+", "\n");

			// Convert strong and b tags to h3 tags. Both are used as header elements in outlines,
			// and not for inline styling
			htmlFragment = Regex.Replace(htmlFragment, @"<(\/?)(?:b|strong)>", "<$1h3>");

			// Tidy any errant double dashed lists.
			htmlFragment = Regex.Replace(htmlFragment, @"-\s-{1,}", "-");

			// Normalize whitespace
			htmlFragment = Regex.Replace(htmlFragment, @"\s{2,}", " ");

		}

		private static void FinalPass() {

			htmlFragment = htmlFragment.Trim();

			htmlFragment = Regex.Replace(htmlFragment, @"(?<!\w)(<\/\w+>)(?!$)", "\n$1\n", RegexOptions.Multiline);

			htmlFragment = Regex.Replace(htmlFragment, "<h3>", "\n<h3>", RegexOptions.Multiline);

			// Then wrap text that isn't contained within a tag in p tags
			htmlFragment = Regex.Replace(htmlFragment, @"^(?!<)(.*?)$", "<p>$1</p>", RegexOptions.Multiline);

			htmlFragment = Regex.Replace(htmlFragment, @"\n?<p>\s?<\/p>", "");

			htmlFragment = htmlFragment.Trim();

		}

		private static void AssessmentPass() {

			htmlFragment = Regex.Replace(htmlFragment, @"<\/h3>\s?(.*?)\s?(?=$|<h3>)", "</h3>\n$1\n", RegexOptions.Multiline);
			htmlFragment = Regex.Replace(htmlFragment, @"<h3>(.*?):<\/h3>", "<h3>$1</h3>", RegexOptions.Multiline);

			//htmlFragment = Regex.Replace(htmlFragment, "(</h3>\n)^(.*?)$", "$1<p>$2</p>", RegexOptions.Multiline);

			/*
			MatchCollection taglessText = Regex.Matches(htmlFragment, @"<\/h3>\s?(.*?)\s?(?=$|<h3>)", RegexOptions.Multiline);

			foreach(Match textBlock in taglessText) {

				string textItem = textBlock.Groups[1].ToString();

				htmlFragment = htmlFragment.Replace(textItem, "<p>" + textItem + "</p>");
			}
			*/
		}

		/// <summary>
		///		Attempts to normalise all the various list formats into 1 standard.
		///		
		///		Modifies the htmlFragment directly.
		/// </summary>
		private static void NormalizeLists() {

			string dashedList = @"^-\s?(.*?)(?:\n|$)",
				   asteriskList = @"\*\s(.*?)<",
				   singleDigitList = @"\d\.\s(.*?)\n",
				   doubleDigitList = @"(\d\.\d\s.*?)\n";

			if(Regex.IsMatch(htmlFragment, dashedList, RegexOptions.Multiline)) {

				MatchCollection listItems = Regex.Matches(htmlFragment, dashedList, RegexOptions.Multiline);

				FormatAsList(listItems, new string[] { "<ul>", "</ul>" });

			}

			if(Regex.IsMatch(htmlFragment, asteriskList, RegexOptions.Multiline)) {

				MatchCollection listItems = Regex.Matches(htmlFragment, asteriskList, RegexOptions.Multiline);

			}

			if(Regex.IsMatch(htmlFragment, singleDigitList, RegexOptions.Multiline)) {

				MatchCollection listItems = Regex.Matches(htmlFragment, singleDigitList, RegexOptions.Multiline);

				FormatAsList(listItems, new string[] { "<ol>", "</ol>" });

			}
			
			// figure out if this is worth doing later
			/*
			if(Regex.IsMatch(htmlFragment, doubleDigitList, RegexOptions.Multiline)) {

				MatchCollection listItems = Regex.Matches(htmlFragment, doubleDigitList, RegexOptions.Multiline);

				FormatAsList(listItems, new string[] { "<ul>", "</ul>" });

			}
			*/
		} // End NormalizeLists

		/// <summary>
		///		Modifies the htmlFragment to properly format an ordered list
		/// </summary>
		/// <param name="listItems"></param>
		private static void FormatAsList(MatchCollection listItems, string[] listTags) {

			string openingTag = "\n" + listTags[0] + "\n", closingTag = listTags[1] + "\n";

			int startIndex = listItems[0].Index;
			int runningLength = openingTag.Length; // The opening tag is 4 characters + new line, as below

			htmlFragment = htmlFragment.Insert(startIndex, openingTag);

			foreach(Match entry in listItems) {

				string listEntry = String.Format("<li>{0}</li>\n", entry.Groups[1].ToString());

				// Add the total length of the new string to our running offset
				runningLength += listEntry.Length;

				htmlFragment = htmlFragment.Replace(entry.ToString(), listEntry);

			}

			htmlFragment = htmlFragment.Insert(startIndex + runningLength, closingTag);

		} // End CreateOrderedList

	}
}
