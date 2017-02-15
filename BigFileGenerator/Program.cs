using System;
using System.Collections.Generic;
using System.IO;

namespace BigFileGenerator
{
	class MainClass
	{
		private static Random rnd = new Random ();
		private static int minWordSize = 3;
		private static int maxWordSize = 5;

		public static void Main (string[] args)
		{
			int wordsCount;
			int eachWordCount;
			int wordsPerLine;

			try{
				wordsCount = Int32.Parse(args[0]);
				eachWordCount = Int32.Parse(args[1]);
				wordsPerLine = Int32.Parse(args[2]);
			}
			catch(Exception e){
				Console.WriteLine ("Invalid number of agruments. Sould be:");
				Console.WriteLine ("Different_Number_Count");
				Console.WriteLine ("Each_Word_Count");
				Console.WriteLine ("Words_Per_Line");
				return;
			}


			string filePath = "/tmp/BIGFILE";



			List<string> words = generateWords (wordsCount);


			Console.WriteLine ("Output: " + filePath);
			Console.WriteLine ("Total words: " + wordsCount * eachWordCount);

			Console.WriteLine ("Words: ");
			foreach(string word in words){
				Console.WriteLine (" - " + word);
			}

			Console.WriteLine ("Running...");


			Dictionary<string, int> wordsCounter = new Dictionary<string, int> ();
			foreach(string word in words){
				wordsCounter.Add (word, eachWordCount);
			}

			int totalWordCount = eachWordCount * words.Count;


			//using (var fileStream = File.Open (filePath, FileMode.Open)) {
			using (var fileStream = new StreamWriter(filePath)) {
				int currentWordInLine = 0;


				for (int i = 0; i < totalWordCount; i++) {
					currentWordInLine %= wordsPerLine;

					int wordIndex = rnd.Next (words.Count);
					string word = words [wordIndex];

					if (currentWordInLine == (wordsPerLine - 1)) {
						fileStream.WriteLine (word);
					} else {
						fileStream.Write (word + " ");
					}


					wordsCounter [word]--;
					if(wordsCounter[word] == 0){
						wordsCounter.Remove (word);
						words.RemoveAt (wordIndex);
					}

					currentWordInLine++;
				}
			}

			Console.WriteLine ("Finished.");
		}

		static List<string> generateWords (int wordsCount)
		{
			List<string> words = new List<string> (wordsCount);

			for (int i = 0; i < wordsCount; i++) {
				words.Add (generateRandomWord());
			}

			return words;
		}

		static string generateRandomWord ()
		{
			int wordSize = rnd.Next (minWordSize, maxWordSize + 1);
			string word = "";

			for (int i = 0; i < wordSize; i++) {
				char a = (char) rnd.Next(97, 123);
				word += a;
			}

			return word;
		}
	}
}
