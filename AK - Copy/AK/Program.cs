using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static AK.Models;
//using static AK.MainLogic;
using static AK.PageParser;
using static AK.Extensions;

namespace AK
{
    class Program
    {
        public static string _NotFoundWord = "nera"; //Naudojamas tam, kad neradus zodeliu prie nagrinejamo zodzio irasytu nurodyta reiksme.
        public static string _path = @"C:\Users\Lina\Desktop\VDU\bakalauras\Sutvarkyti sakiniai.xlsx";
        public static string _saveNewFilePath = @"C:\Users\Lina\Desktop\VDU\bakalauras\KlasifikavimoRezultatai1.xlsx";
        public static string _saveNewFilePathLINA = @"C:\Users\Lina\Desktop\VDU\bakalauras\SutvarkytiSakiniaiMorfologiniai1.xlsx";
        public static List<int> _positions = new List<int>() { -3, -2, -1, 1, 2, 3 };
        public static List<string> _Linksniai = new List<string>() { "V.", "K.", "N.", "G.", "Įn.", "Vt." };
        public static string _uri = "http://donelaitis.vdu.lt/main_helper.php?id=4&nr=7_2";
        public static string _pathAttributes = @"C:\Users\Lina\Desktop\VDU\bakalauras\SakiniuAtributai1.xlsx";


        static void Main(string[] args)
        {
            var sentences = ExcelDLL.returnSentences(_path, 1);
            var objects = MainLogic.SplitSentence(sentences);
            var analyzedSentences = MainLogic.AnalyzeSentence(objects, 3);//galutinis rezultatas su isrinktais zodziais po nagrinejimo. Skaiciu nurodome kiek imti zodziu pries ir po nagrinejant zodi.
            List<Sentence> fixedResult = Extensions.FixWordChars(analyzedSentences);
            
            DoMorfology(fixedResult);
            //extra find examWords word and morfology
            ExcelDLL.FindExamWordsExtraDataFromExcel(_path, 1, fixedResult);

            DoMorfologyForExamWords(fixedResult);
            SplitMorfologyInDetail(fixedResult);
            var list = MakeAttributes(fixedResult);
            ExcelDLL.writeAttributesToExcel(list, _pathAttributes);
            var result = ExcelDLL.WriteDataToExcelFile(fixedResult, _saveNewFilePath);
            ExcelDLL.writeDataToExcelOnlyMorfology(fixedResult, _saveNewFilePathLINA);

            //PrintResult(fixedResult);
        }

        public static List<string> SplitMorfologyInDetail(List<Sentence> sentences)
        {
            var toReturn = new List<string>();

            foreach (var i in sentences)
            {
                foreach (var j in i.examinedWords)
                {
                    foreach (var k in j.words)
                    {
                        k.morfologyDetail = returnMorfologyDetail(k.morfology);
                    }

                    j.morfologyDetail = returnMorfologyDetail(j.morfology);
                }
            }
            return toReturn;
        }

        public static MorfologyDetail returnMorfologyDetail(string morfologyFullText)
        {
            var morfologyKalb = "";
            var morfologyGim = "";
            var morfologyLink = "";

            if (!string.IsNullOrWhiteSpace(morfologyFullText))
            {
                var splittedMorfology = morfologyFullText.Split(',').ToList();

                morfologyKalb = splittedMorfology.First();
                if (!morfologyKalb.Contains("nežinomas") || !morfologyKalb.Contains(_NotFoundWord) || splittedMorfology.Count() < 2)
                {
                    if (splittedMorfology.Any(x => x.Contains("vyr. g.") || x.Contains("mot. g.")))
                    {
                        var getGimine = splittedMorfology.Where(x => x.Contains("vyr. g.") || x.Contains("mot. g.")).First();
                        var getNumber = "";
                        if (splittedMorfology.Any(x => x.Contains("vns.") || x.Contains("dgs.")))
                        {
                            getNumber = splittedMorfology.Where(x => x.Contains("vns.") || x.Contains("dgs.")).First();
                        }
                        morfologyGim = getGimine + ", " + getNumber;
                    }
                    else morfologyGim = _NotFoundWord;
                }
                else
                {
                    morfologyGim = _NotFoundWord;
                    morfologyLink = _NotFoundWord;
                    //break;
                }
                if (splittedMorfology.Any(x => _Linksniai.Any(z => z.Contains(x.Replace(" ", "")))))
                {
                    morfologyLink = splittedMorfology.Last().Replace(" ", "");
                }
                else morfologyLink = _NotFoundWord;
            }
            else
            {
                morfologyGim = _NotFoundWord;
                morfologyKalb = _NotFoundWord;
                morfologyLink = _NotFoundWord;
            }
            return new MorfologyDetail(morfologyKalb, morfologyGim, morfologyLink);
        }

        public static List<string> MakeAttributes(List<Sentence> sentences)
        {
            var toReturn = new List<string>();

            var firstAttr = new List<AttributeMorfology>();
            var secAttr = new List<AttributeMorfology>();
            var thirdAttr = new List<AttributeMorfology>();
            var fourthAttr = new List<AttributeMorfology>();
            var fifthAttr = new List<AttributeMorfology>();
            var sixthAttr = new List<AttributeMorfology>();
            var examMorfologies = new List<AttributeMorfology>();
            var firstExaminadeWordMeaning = new List<AttributeMorfology>();
            var nextToExaminadeWordMeaning = new List<AttributeMorfology>();

            foreach (var i in sentences)
            {
                foreach (var j in i.examinedWords)
                {
                    for (var k = 0; k < j.words.Count; k++)
                    {
                        if (k == 0) firstAttr.Add(new AttributeMorfology(j.words[k].word, j.words[k].morfology, new MorfologyDetail(j.words[k].morfologyDetail.morfologyKalb, j.words[k].morfologyDetail.morfologyGim, j.words[k].morfologyDetail.morfologyLink)));
                        if (k == 1) secAttr.Add(new AttributeMorfology(j.words[k].word, j.words[k].morfology, new MorfologyDetail(j.words[k].morfologyDetail.morfologyKalb, j.words[k].morfologyDetail.morfologyGim, j.words[k].morfologyDetail.morfologyLink)));
                        if (k == 2) thirdAttr.Add(new AttributeMorfology(j.words[k].word, j.words[k].morfology, new MorfologyDetail(j.words[k].morfologyDetail.morfologyKalb, j.words[k].morfologyDetail.morfologyGim, j.words[k].morfologyDetail.morfologyLink)));
                        if (k == 3) fourthAttr.Add(new AttributeMorfology(j.words[k].word, j.words[k].morfology, new MorfologyDetail(j.words[k].morfologyDetail.morfologyKalb, j.words[k].morfologyDetail.morfologyGim, j.words[k].morfologyDetail.morfologyLink)));
                        if (k == 4) fifthAttr.Add(new AttributeMorfology(j.words[k].word, j.words[k].morfology, new MorfologyDetail(j.words[k].morfologyDetail.morfologyKalb, j.words[k].morfologyDetail.morfologyGim, j.words[k].morfologyDetail.morfologyLink)));
                        if (k == 5) sixthAttr.Add(new AttributeMorfology(j.words[k].word, j.words[k].morfology, new MorfologyDetail(j.words[k].morfologyDetail.morfologyKalb, j.words[k].morfologyDetail.morfologyGim, j.words[k].morfologyDetail.morfologyLink)));
                    }
                    examMorfologies.Add(new AttributeMorfology(j.checkingWord, j.morfology, j.morfologyDetail));
                }
            }

            firstAttr = firstAttr.GroupBy(x => x.word).Select(x => x.First()).ToList();
            secAttr = secAttr.GroupBy(x => x.word).Select(x => x.First()).ToList();
            thirdAttr = thirdAttr.GroupBy(x => x.word).Select(x => x.First()).ToList();
            fourthAttr = fourthAttr.GroupBy(x => x.word).Select(x => x.First()).ToList();
            fifthAttr = fifthAttr.GroupBy(x => x.word).Select(x => x.First()).ToList();
            sixthAttr = sixthAttr.GroupBy(x => x.word).Select(x => x.First()).ToList();
            firstExaminadeWordMeaning = firstExaminadeWordMeaning.GroupBy(x => x.word).Select(x => x.First()).ToList();
            nextToExaminadeWordMeaning = nextToExaminadeWordMeaning.GroupBy(x => x.word).Select(x => x.First()).ToList();

            var f = generateText(firstAttr, 3, "pries");
            foreach (var i in f) toReturn.Add(i);

            var se = generateText(secAttr, 2, "pries");
            foreach (var i in se) toReturn.Add(i);

            var th = generateText(thirdAttr, 1, "pries");
            foreach (var i in th) toReturn.Add(i);

            var four = generateText(fourthAttr, 1, "po");
            foreach (var i in four) toReturn.Add(i);

            var fi = generateText(fifthAttr, 2, "po");
            foreach (var i in fi) toReturn.Add(i);

            var s = generateText(sixthAttr, 3, "po");
            foreach (var i in s) toReturn.Add(i);

            var ei = generateData(firstExaminadeWordMeaning);
            foreach (var i in firstExaminadeWordMeaning) toReturn.Add(i);

            var nin = generateData(nextToExaminadeWordMeaning);
            foreach (var i in nextToExaminadeWordMeaning) toReturn.Add(i);

            var onlyClassList = generateClass(examMorfologies);
            foreach (var i in onlyClassList) toReturn.Add(i);

            return toReturn;
        }
        public static List<string> generateText(List<AttributeMorfology> list, int position, string text)
        {
            var toReturn = new List<string>();

            var firstrow = "@attribute " + position + "z" + text + " {";
            var firstmorfologyKalb = "@attribute " + position + "z" + text + "morfKalb {";
            var firstmorfologyGim = "@attribute " + position + "z" + text + "morfGim {";
            var firstmorfologyLink = "@attribute " + position + "z" + text + "morfLink {";
            foreach (var i in list)
            {
                i.word = i.word == "" ? "nera" : i.word;
                firstrow += '"' + i.word + '"' + ", ";
            }
            var temp = list.GroupBy(x => x.morfologyDetail.morfologyKalb).Select(x => x.First()).ToList();
            var temp2 = list.GroupBy(x => x.morfologyDetail.morfologyGim).Select(x => x.First()).ToList();
            var temp3 = list.GroupBy(x => x.morfologyDetail.morfologyLink).Select(x => x.First()).ToList();
            foreach (var i in temp)
            {
                i.morfologyDetail.morfologyKalb = i.morfologyDetail.morfologyKalb == "" ? "nera" : i.morfologyDetail.morfologyKalb;
                firstmorfologyKalb += '"' + i.morfologyDetail.morfologyKalb + '"' + ", ";
            }
            foreach (var i in temp2)
            {
                i.morfologyDetail.morfologyGim = i.morfologyDetail.morfologyGim == "" ? "nera" : i.morfologyDetail.morfologyGim;
                firstmorfologyGim += '"' + i.morfologyDetail.morfologyGim + '"' + ", ";
            }
            foreach (var i in temp3)
            {
                i.morfologyDetail.morfologyLink = i.morfologyDetail.morfologyLink == "" ? "nera" : i.morfologyDetail.morfologyLink;
                firstmorfologyLink += '"' + i.morfologyDetail.morfologyLink + '"' + ", ";
            }
            firstrow = firstrow.Substring(0, firstrow.Count() - 2);
            firstrow += "}";
            firstmorfologyKalb = firstmorfologyKalb.Substring(0, firstmorfologyKalb.Count() - 2);
            firstmorfologyKalb += "}";
            firstmorfologyGim = firstmorfologyGim.Substring(0, firstmorfologyGim.Count() - 2);
            firstmorfologyGim += "}";
            firstmorfologyLink = firstmorfologyLink.Substring(0, firstmorfologyLink.Count() - 2);
            firstmorfologyLink += "}";

            toReturn.Add(firstrow);
            toReturn.Add(firstmorfologyKalb);
            toReturn.Add(firstmorfologyGim);
            toReturn.Add(firstmorfologyLink);

            return toReturn;
        }
        public static List<string> generateData(List<ExamWord> list)
        {
            var toReturn = new List<string>();
            var te1 = new List<string>();
            var te2 = new List<string>();
            var temp1 = list.GroupBy(x => x.examinedWordsmeaning).Select(x => x.First()).ToList(); foreach (var i in temp1) te1.Add(i.examinedWordsmeaning);
            var temp2 = list.GroupBy(x => x.nextToExaminedWord).Select(x => x.First()).ToList(); foreach (var i in temp2) te2.Add(i.nextToExaminedWord);

            toReturn.Add(ReturnGeneratedTextForData(te1, 1));
            toReturn.Add(ReturnGeneratedTextForData(te2, 2));

            return toReturn;
        }
        public static List<string> generateClass(List<AttributeMorfology> list)
        {
            var toReturn = new List<string>();
            var te1 = new List<string>();
            var temp1 = list.GroupBy(x => x.morfologyDetail.morfologyKalb).Select(x => x.First()).ToList(); foreach (var i in temp1) te1.Add(i.morfologyDetail.morfologyKalb);
            var te2 = new List<string>();
            var temp2 = list.GroupBy(x => x.morfologyDetail.morfologyGim).Select(x => x.First()).ToList(); foreach (var i in temp2) te2.Add(i.morfologyDetail.morfologyGim);
            var te3 = new List<string>();
            var temp3 = list.GroupBy(x => x.morfologyDetail.morfologyLink).Select(x => x.First()).ToList(); foreach (var i in temp3) te3.Add(i.morfologyDetail.morfologyLink);

            toReturn.Add(ReturnGeneratedTextForClass(te1, 1));
            toReturn.Add(ReturnGeneratedTextForClass(te2, 2));
            toReturn.Add(ReturnGeneratedTextForClass(te3, 3));

            return toReturn;
        }
        private static string ReturnGeneratedTextForData(List<string> temp, int number)
        {
            var data1 = "@attribute data" + number + " {";
            foreach (var i in temp)
            {
                data1 += '"' + i + '"' + ", ";
            }
            data1 = data1.Substring(0, data1.Count() - 2);
            data1 += "}";
            return data1;
        }
        private static string ReturnGeneratedTextForClass(List<string> temp, int number)
        {
            var class1 = "@attribute class"+number+ " {";
            foreach (var i in temp)
            {
                class1 += '"' + i + '"' + ", ";
            }
            class1 = class1.Substring(0, class1.Count() - 2);
            class1 += "}";
            return class1;
        }

        public static void DoMorfologyForExamWords(List<Sentence> sentences)
        {
            var foundsWords = ReturnAllFoundWordsOnlyExamWords(sentences);

            var text = GenerateTextForRequest(foundsWords);

            var resultString = GetResponseFromPage(_uri, text);

            var resultWithMorfologies = ParseHtml(resultString);

            MatchSentenceWithMorfologyOnlyExamWords(sentences, resultWithMorfologies);
        }

        public static void DoMorfology(List<Sentence> sentences)
        {

            var foundsWords = ReturnAllFoundWords(sentences);

            var text = GenerateTextForRequest(foundsWords);

            var resultString = GetResponseFromPage(_uri, text);

            var resultWithMorfologies = ParseHtml(resultString);

            var finalResult = MatchSentenceWithMorfology(sentences, resultWithMorfologies);
        }

        public static void PrintResult(List<Sentence> sentences)
        {
            foreach (var i in sentences)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(i.position + " Sakinys");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(i.sentence);
                Console.ResetColor();
                foreach (var j in i.examinedWords)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Nagrinejamas zodis !--- " + j.checkingWord + " ---! ");
                    Console.ResetColor();
                    foreach (var k in j.words)
                    {
                        if (k.word == _NotFoundWord)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("(" + k.row + " {" + k.word + "}) ");
                            Console.ResetColor();
                        }
                        else Console.Write("(" + k.row + " {" + k.word + "}) ");
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("");
            }

            Console.ReadLine();
        }



    }
}
