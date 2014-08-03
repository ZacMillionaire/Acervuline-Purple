using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acervuline {
	class TagBuilder {

		static Dictionary<string, int> wordFrequency = new Dictionary<string,int>();
		static Regex wordReg = new Regex(@"(?<!<\/|<)([a-zA-Z]+)\s?(?!>)");

		public static Dictionary<string, int> BuildTagDictionary(string textInput) {
		
			MatchCollection words = wordReg.Matches(textInput);

			foreach(Match match in words) {

				string word = match.Value;
				word = word.Trim().ToLower();

				if(wordFrequency.ContainsKey(word)){
					wordFrequency[word]++;
				} else {
					wordFrequency.Add(word, 1);
				}

			}

			return wordFrequency;

		}
		public static Dictionary<string, double> TFIFD() {

			Dictionary<string, double> TFIFDict = new Dictionary<string, double>();
			int totalWords = wordFrequency.Sum(x => x.Value);

			foreach(KeyValuePair<string, int> entry in wordFrequency) {

				float TF = (float)entry.Value / totalWords;
				//float IFD = Math.Log(6 / entry.Value);
				double TFIFDSum = TF;

				TFIFDict.Add(entry.Key, TFIFDSum);

			}

			return TFIFDict;

		}

	}
}
