using System;
using System.Collections.Generic;
using System.Linq;
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

	class HtmlTidy {

		static string htmlFragment, fragmentTitle;

		public static string FormatHtml(string title, string html) {

			htmlFragment = html;
			fragmentTitle = title.ToCamelCase();

			GeneralPass();

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

			//html = Regex.Replace(html, @"-\s-{1,}", "-");

			return htmlFragment;

		}

		private static void GeneralPass() {

			htmlFragment = Regex.Replace(htmlFragment, @"<div>|<\/div>", "");
			htmlFragment = Regex.Replace(htmlFragment, @"(<br\s*\/?>\s*)+", "\n");
			htmlFragment = Regex.Replace(htmlFragment, @"<(\/?)b>", "<$1strong>");

		}

		private static void NormaliseLists() {

			htmlFragment = Regex.Replace(htmlFragment, @"-\s-{1,}", "-");

		}

	}
}
