using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Acervuline {
	class LocalDocHandler {

		public LocalDocHandler(string directory) {

			IEnumerable<string> dirlist = Directory.EnumerateDirectories(directory);

			foreach(string unitFolder in dirlist) {
				ReadFromUnitFolder(unitFolder);
			}


		} // End Constructor


		/// <summary>
		///		Loops over all folders within the root path
		/// </summary>
		/// <param name="unitDir"></param>
		private static void ReadFromUnitFolder(string unitDir) {

			Console.WriteLine("Reading All HTML files within {0}", unitDir);

			//IEnumerable<string> filelist = Directory.EnumerateFiles(unitDir, "*.html");
			Regex reg = new Regex(@"[A-Z]{3,}[0-9]{3,}\.html");
			IEnumerable<string> filelist = Directory.GetFiles(unitDir, "*.html").Where(Path => reg.IsMatch(Path));

			foreach(string fileName in filelist) {

				Console.WriteLine(fileName);

				Program.CurrentFolderPath = unitDir + "\\";

				HtmlDocument currentFile = LoadFileForReading(fileName);

				HtmlDocumentHandler.ParseCurrentUnitFile(currentFile);

			}

			Console.WriteLine();

		} // End ReadFromUnitFolder


		/// <summary>
		///		Loads a given html file from a given path into a stream, then returns it as
		///		an HtmlDocument
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		public static HtmlDocument LoadFileForReading(string filepath) {

			HtmlDocument doc;
			Stream stream;

			doc = new HtmlDocument();
			stream = File.OpenRead(filepath);
			doc.Load(stream);

			return doc;

		} // End LoadFileForReading

	}
}
