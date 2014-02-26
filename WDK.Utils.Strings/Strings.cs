using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WDK.Utils
{
	public static class Strings
	{
		/// Like linq take - takes the first x characters
		public static string Take(this string theString, int count, bool ellipsis = false)
		{
			int lengthToTake = Math.Min(count, theString.Length);
			var cutDownString = theString.Substring(0, lengthToTake);

			if (ellipsis && lengthToTake < theString.Length)
				cutDownString += "...";

			return cutDownString;
		}

		//like linq skip - skips the first x characters and returns the remaining string
		public static string Skip(this string theString, int count)
		{
			int startIndex = Math.Min(count, theString.Length);
			var cutDownString = theString.Substring(startIndex - 1);

			return cutDownString;
		}

		//reverses the string... pretty obvious really
		public static string Reverse(this string input)
		{
			char[] chars = input.ToCharArray();
			Array.Reverse(chars);
			return new String(chars);
		}

		// "a string".IsNullOrEmpty() beats string.IsNullOrEmpty("a string")
		public static bool IsNullOrEmpty(this string theString)
		{
			return string.IsNullOrEmpty(theString);
		}

		//not so sure about this one -
		//"a string {0}".Format("blah") vs string.Format("a string {0}", "blah")
		public static string With(this string format, params object[] args)
		{
			return string.Format(format, args);
		}

		//ditches html tags - note it doesnt get rid of things like &nbsp;
		public static string StripHtml(this string html)
		{
			if (string.IsNullOrEmpty(html))
				return string.Empty;

			return Regex.Replace(html, @"<[^>]*>", string.Empty);
		}

		public static bool Match(this string value, string pattern)
		{
			return Regex.IsMatch(value, pattern);
		}

		//splits string into array with chunks of given size. not really that useful..
		public static string[] SplitIntoChunks(this string toSplit, int chunkSize)
		{
			if (string.IsNullOrEmpty(toSplit))
				return new string[] { "" };

			int stringLength = toSplit.Length;

			int chunksRequired = (int)Math.Ceiling((decimal)stringLength / (decimal)chunkSize);
			var stringArray = new string[chunksRequired];

			int lengthRemaining = stringLength;

			for (int i = 0; i < chunksRequired; i++)
			{
				int lengthToUse = Math.Min(lengthRemaining, chunkSize);
				int startIndex = chunkSize * i;
				stringArray[i] = toSplit.Substring(startIndex, lengthToUse);

				lengthRemaining = lengthRemaining - lengthToUse;
			}

			return stringArray;
		}

		public static string ParseUrls(string source)
		{
			/*
			 * 
#See: http://daringfireball.net/2010/07/improved_regex_for_matching_urls
import re, urllib

GRUBER_URLINTEXT_PAT = re.compile(ur'(?i)\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:\'".,<>?\xab\xbb\u201c\u201d\u2018\u2019]))')

for line in urllib.urlopen("http://daringfireball.net/misc/2010/07/url-matching-regex-test-data.text"):
    print [ mgroups[0] for mgroups in GRUBER_URLINTEXT_PAT.findall(line) ]
			 */

			return "";
		}
	}
}
