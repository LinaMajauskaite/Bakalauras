using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AK.Models;
using static AK.Program;

namespace AK
{
    public class Extensions
    {
        public static List<string> _doesNotMatchedWords = new List<string>();

        public static void MatchSentenceWithMorfologyOnlyExamWords(List<Sentence> sentences, Dictionary<string, string> morfologyDic)
        {
            foreach (var i in sentences)
            {
                foreach (var j in i.examinedWords)
                {
                        if (j.numberClass != _NotFoundWord)
                        {
                            try
                            {
                                j.morfology = morfologyDic[j.numberClass];
                            }
                            catch (Exception ex)
                            {
                                _doesNotMatchedWords.Add(j.numberClass);
                            }
                        }
                }
            }
        }
        public static List<Sentence> MatchSentenceWithMorfology(List<Sentence> sentences, Dictionary<string, string> morfologyDic)
        {
            foreach (var i in sentences)
            {
                foreach (var j in i.examinedWords)
                {
                    foreach (var k in j.words)
                    {
                        if (k.word != _NotFoundWord)
                        {
                            //k.word = RemoveBadChars(k.word);
                            try
                            {
                                k.morfology = morfologyDic[k.word];
                            }
                            catch (Exception ex)
                            {
                                _doesNotMatchedWords.Add(k.word);
                            }
                        }
                    }
                }
            }

            return sentences;
        }
        //nereikalingu simboliu isvalymas is string 
        public static string RemoveBadChars(string word)
        {
            Regex reg = new Regex("[^a-zA-ZĄČĘĖĮŠŲŪŽąčęėįšųūž']");
            return reg.Replace(word, string.Empty);
        }



        public static string GenerateTextForRequest(Dictionary<string, string> dictionaries)
        {
            var text = "";

            foreach (var i in dictionaries.Keys) text += " " + i;

            return text;
        }
        public static Dictionary<string, string> ReturnAllFoundWordsOnlyExamWords (List<Sentence> sentences)
        {
            var toReturn = new Dictionary<string, string>();

            foreach (var i in sentences)
                foreach (var j in i.examinedWords)
                {
                     if (!toReturn.ContainsKey(j.numberClass) && !j.numberClass.Contains(_NotFoundWord)) toReturn.Add(j.numberClass, "not found");
                }

            return toReturn;
        }
        public static Dictionary<string, string> ReturnAllFoundWords(List<Sentence> sentences)
        {
            var toReturn = new Dictionary<string, string>();

            foreach (var i in sentences)
                foreach (var j in i.examinedWords)
                    foreach (var k in j.words)
                    {
                        //k.word = RemoveBadChars(k.word);
                        if (!toReturn.ContainsKey(k.word) && !k.word.Contains(_NotFoundWord)) toReturn.Add(k.word, "not found");
                    }

            return toReturn;
        }

        public static string GetResponseFromPage(string URI, string generatedText)
        {
            var request = (HttpWebRequest)WebRequest.Create(URI);
            var values = "tekstas=" + generatedText;

            //FinalAdd
            values += "&tipas=anotuoti&pateikti=M&veiksmas=Rezultatas+puslapyje";

            var data = Encoding.UTF8.GetBytes(values);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            return responseString;
        }

        public static List<Sentence> FixWordChars(List<Sentence> sentences)
        {
            foreach (var i in sentences)
                foreach (var j in i.examinedWords)
                    foreach (var k in j.words)
                        if (!string.IsNullOrWhiteSpace(k.word))
                        {
                            k.word = RemoveBadChars(k.word);//istrina nereikalingus simbolius.
                            k.word = k.word.ToLower();//visus simbolius vercia i mazasias raides.
                        }
                        else k.word = _NotFoundWord; //Gali buti ir taip, kad jeigu blogai aprasyti sakiniai, tai paims vietoj zodelio tuscia tarpeli, tai reikia dar karta pasitikrinti ir irasyti reiksme is _notfoundword
            return sentences;
        }

    }
}
