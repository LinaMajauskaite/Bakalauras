using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AK.Program;

namespace AK
{
    public class PageParser
    {
        public static Dictionary<string, string> ParseHtml(string html)
        {
            var toReturn = new Dictionary<string, string>();
            var parsedHtml = Regex.Split(html, "word");

            var splittedRow = new List<string>();
            foreach (var i in parsedHtml)
            {
                splittedRow = Regex.Split(i, "&quot;").ToList();
                var foundWord = "";
                var foundValue = "";
                for (var j = 0; j < splittedRow.Count; j++)
                {
                    if (splittedRow[j] == "=")
                    {
                        var takeElementIndex = j + 1;
                        foundWord = splittedRow[takeElementIndex];
                    }
                    else if (splittedRow[j].Replace(" ", "") == "type=")
                    {
                        var takeElementIndex = j + 1;
                        foundValue = splittedRow[takeElementIndex];

                        if (!toReturn.ContainsKey(foundWord))
                            toReturn.Add(foundWord, foundValue);
                        break;
                    }
                    else { continue; }
                }
            }

            return toReturn;
        }
    }
}
