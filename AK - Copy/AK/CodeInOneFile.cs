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

namespace LinosPvz
{
    class CodeInOneFile //Jeigu norima naudoti tik vienam faile.
    {
        private static string _NotFoundWord = "nera"; //Naudojamas tam, kad neradus zodeliu prie nagrinejamo zodzio irasytu nurodyta reiksme.
        private static string _path = @"C:\Users\Admin\Downloads\Sutvarkyti-sakiniai.xlsx";
        private static string _saveNewFilePath = "KlasifikavimoRezultatai(" + DateTime.Now.ToString("yyyy-MM-dd HH mm") + ").xlsx";

        //static void Main(string[] args)
        //{
        //    var sentences = returnSentences(_path, 1);
        //    var objects = SplitSentence(sentences);
        //    var analyzedSentences = AnalyzeSentence(objects, 3);//galutinis rezultatas su isrinktais zodziais po nagrinejimo. Skaiciu nurodome kiek imti zodziu pries ir po nagrinejant zodi.
        //    var fixedResult = FixWordChars(analyzedSentences);

        //    DoMorfology(fixedResult);
        //    WriteDataToExcelFile(fixedResult, _saveNewFilePath);
        //    //PrintResult(fixedResult);
        //}

        public static List<List<string>> ExcelWorksheetsLetters()
        {
            var toReturn = new List<List<string>>();

            var firstLetters = new List<string>() { "A", "B" }; toReturn.Add(firstLetters);
            var secondLetters = new List<string>() { "C", "D" }; toReturn.Add(secondLetters);
            var thirdLetters = new List<string>() { "E", "F" }; toReturn.Add(thirdLetters);
            var fourthLetters = new List<string>() { "G", "H" }; toReturn.Add(fourthLetters);
            var fifthLetters = new List<string>() { "I", "J" }; toReturn.Add(fifthLetters);
            var sixthLetters = new List<string>() { "K", "L" }; toReturn.Add(sixthLetters);

            return toReturn;
        }

        public static void WriteDataToExcelFile(List<Sentence> sentences, string newFileName)
        {
            var workBook = new XLWorkbook();
            var workSheet = workBook.AddWorksheet("Rezultatai");

            var rowNumber = 1;
            for (var i = 0; i < sentences.Count; i++)
            {
                workSheet.Cell("A" + rowNumber).Value = sentences[i].position + ". " + sentences[i].sentence;
                workSheet.Cell("A" + rowNumber).Style.Font.SetBold();
                workSheet.Range("A" + rowNumber + ":" + "O" + rowNumber).Merge();
                rowNumber++;
                for (var j = 0; j < sentences[i].examinedWords.Count; j++)
                {
                    workSheet.Cell("A" + rowNumber).Value = sentences[i].examinedWords[j].checkingWord;
                    workSheet.Cell("A" + rowNumber).Style.Font.FontColor = XLColor.Green;
                    workSheet.Range("A" + rowNumber + ":" + "O" + rowNumber).Merge();
                    rowNumber++;
                    for (var k = 0; k < sentences[i].examinedWords[j].words.Count; k++)
                    {
                        var getLetter = ExcelWorksheetsLetters()[k];
                        var value1 = sentences[i].examinedWords[j].words[k].row + " (" + sentences[i].examinedWords[j].words[k].word + ")";
                        workSheet.Cell(getLetter[0] + rowNumber).Value = value1;

                        var value2 = sentences[i].examinedWords[j].words[k].morfology;
                        workSheet.Cell(getLetter[1] + rowNumber).Value = value2;
                        if (string.IsNullOrWhiteSpace(value2))
                        {
                            if (sentences[i].examinedWords[j].words[k].word == _NotFoundWord)
                            {
                                workSheet.Cell(getLetter[0] + rowNumber).Style.Border.SetOutsideBorder(XLBorderStyleValues.Double).Border.SetOutsideBorderColor(XLColor.Red);
                                workSheet.Cell(getLetter[1] + rowNumber).Style.Border.SetOutsideBorder(XLBorderStyleValues.Double).Border.SetOutsideBorderColor(XLColor.Red);
                            }
                            else
                            {
                                workSheet.Cell(getLetter[0] + rowNumber).Style.Border.SetOutsideBorder(XLBorderStyleValues.Double).Border.SetOutsideBorderColor(XLColor.Blue);
                                workSheet.Cell(getLetter[1] + rowNumber).Style.Border.SetOutsideBorder(XLBorderStyleValues.Double).Border.SetOutsideBorderColor(XLColor.Blue);

                            }
                        }
                    }
                    rowNumber++;
                }
                workSheet.Cell("A" + rowNumber).Value = "";
                rowNumber++;
            }
            workSheet.Columns().AdjustToContents();
            workBook.SaveAs(newFileName);
        }

        public static void DoMorfology(List<Sentence> sentences)
        {
            var uri = "http://donelaitis.vdu.lt/main_helper.php?id=4&nr=7_2";

            var foundsWords = ReturnAllFoundWords(sentences);

            var text = GenerateTextForRequest(foundsWords);

            //var resultString = GetResponseFromWeb(uri, text);
            var resultString = GetResponseFromPage(uri, text);

            var resultWithMorfologies = ParseHtml(resultString);

            var finalResult = MatchSentenceWithMorfology(sentences, resultWithMorfologies);
        }
        private static List<string> _doesNotMatchedWords = new List<string>();
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

        public static string RemoveBadChars(string word)
        {
            Regex reg = new Regex("[^a-zA-ZĄČĘĖĮŠŲŪŽąčęėįšųūž1']");
            return reg.Replace(word, string.Empty);
        }

        public static Dictionary<string, string> ParseHtml(string html)
        {
            var toReturn = new Dictionary<string, string>();
            var ahtml = Regex.Split(html, "word");

            var splittedRow = new List<string>();
            foreach (var i in ahtml)
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

        public static string GenerateTextForRequest(Dictionary<string, string> dictionaries)
        {
            var text = "";

            foreach (var i in dictionaries.Keys) text += " " + i;

            return text;
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


        public static string GetResponseFromWeb(string URI, string generatedText)
        {
            var toReturn = "";
            var myParameters = "pateikti=M&tekstas=";
            myParameters += generatedText;
            myParameters += "&tipas=anotuoti&veiksmas=Rezultatas+puslapyje";

            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var HtmlResult = wc.UploadString(URI, myParameters);

                byte[] bytes = Encoding.Default.GetBytes(HtmlResult);
                toReturn = Encoding.UTF8.GetString(bytes);
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

        public static List<Word> goUpToSearch(List<string> splittedWords, int foundPosition, int maxPositionUpDown)
        {
            var toReturn = new List<Word>();

            var isDone = false;
            var isNormalWord = false;

            while (isDone == false)
            {
                for (var j = 1; j <= maxPositionUpDown; j++)
                {
                    var moveUp = foundPosition + j;
                    if (moveUp < splittedWords.Count)
                    {
                        if (!splittedWords[moveUp].Contains("<"))
                        {
                            toReturn.Add(new Word(+j, splittedWords[moveUp], ""));
                            isNormalWord = true;
                        }
                        else if (isNormalWord == false && splittedWords[moveUp].Contains("<"))
                        {
                            ++maxPositionUpDown;
                        }
                        else if (isNormalWord == true)
                        {
                            while (j != maxPositionUpDown + 1)
                            {
                                toReturn.Add(new Word(+j, _NotFoundWord, ""));
                                ++j;
                            }
                            break;
                        }
                    }
                    else
                    {
                        while (j != maxPositionUpDown + 1)
                        {
                            toReturn.Add(new Word(+j, _NotFoundWord, ""));
                            ++j;
                        }
                    }
                }
                isDone = true;
            }

            return toReturn;
        }


        public static List<Word> goDownToSearch(List<string> splittedWords, int foundPosition, int maxPositionUpDown)
        {
            var toReturn = new List<Word>();

            var isDone = false;
            var isNormalWord = false;

            while (isDone == false)
            {
                for (var j = 1; j <= maxPositionUpDown; j++)
                {
                    var moveDown = foundPosition - j;
                    if (moveDown > -1)
                    {
                        if (!splittedWords[moveDown].Contains("<"))
                        {
                            toReturn.Add(new Word(-j, splittedWords[moveDown], ""));
                            isNormalWord = true;
                        }
                        else if (isNormalWord == false && splittedWords[moveDown].Contains("<"))
                        {
                            ++maxPositionUpDown;
                        }
                        else if (isNormalWord == true)
                        {
                            while (j != maxPositionUpDown + 1)
                            {
                                toReturn.Add(new Word(-j, _NotFoundWord, ""));
                                ++j;
                            }
                            break;
                        }
                    }
                    else
                    {
                        while (j != maxPositionUpDown + 1)
                        {
                            toReturn.Add(new Word(-j, _NotFoundWord, ""));
                            ++j;
                        }
                    }
                }
                isDone = true;
            }

            return toReturn;
        }

        private static List<int> _positions = new List<int>() { -3, -2, -1, 1, 2, 3 };
        public static List<Word> returnAllWordsByExaminedWord(List<string> splittedWords, int foundPosition, int maxPositionUpDown)
        {
            var toReturn = new List<Word>();

            var maxWordsBeforeAfter = maxPositionUpDown * 2;

            var wordsDown = goDownToSearch(splittedWords, foundPosition, maxPositionUpDown);
            var wordsUp = goUpToSearch(splittedWords, foundPosition, maxPositionUpDown);

#if (DEBUG)
            if (wordsDown.Count != maxPositionUpDown) throw new Exception("Neatitinka ieskant PRIES");
            if (wordsUp.Count != maxPositionUpDown) throw new Exception("Neatitinka ieskant PO");
#endif
            foreach (var i in wordsDown) toReturn.Add(i);
            foreach (var i in wordsUp) toReturn.Add(i);

            toReturn = toReturn.OrderBy(x => x.row).ToList();
            if (toReturn.Count != maxWordsBeforeAfter)
            {
                throw new Exception("Neatitinka kiekis pagal reikalavimus! " + toReturn.Count + " != " + maxPositionUpDown);
            }

            for (var i = 0; i < toReturn.Count; i++) toReturn[i].row = _positions[i];

            return toReturn;
        }

        public static List<Sentence> AnalyzeSentence(List<Sentence> sentences, int maxPositionUpDown)
        {
            foreach (var i in sentences)
            {
                var examinedWordsList = new List<ExamWord>();

                for (var j = 0; j < i.splittedWords.Count; j++)
                {
                    if (i.splittedWords[j].Contains("<"))
                    {
                        var pickedWords = returnAllWordsByExaminedWord(i.splittedWords, j, maxPositionUpDown);

                        examinedWordsList.Add(new ExamWord(i.splittedWords[j], pickedWords));
                    }
                }

                i.examinedWords = examinedWordsList;
            }

            return sentences;
        }

        public static List<Sentence> SplitSentence(List<string> sentences)
        {
            var toReturn = new List<Sentence>();

            var pos = 1;
            foreach (var i in sentences)
            {
                var splittedWords = i.Split(' ').ToList();
                //var splittedWords = i.Split(new char[] { ' ', '.', ',', ';', '-', '_' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                toReturn.Add(new Sentence(pos, i, splittedWords, splittedWords.Count, new List<ExamWord>()));
                ++pos;
            }

            return toReturn;
        }

        public class Word
        {
            public Word(int row, string word, string morfology)
            {
                this.row = row;
                this.word = word;
                this.morfology = morfology;
            }
            public int row { get; set; }
            public string word { get; set; }
            public string morfology { get; set; }
            public override string ToString()
            {
                return row + " " + word + " Morfology: " + morfology;
            }
        }
        public class ExamWord
        {
            public ExamWord(string checkingWord, List<Word> words)
            {
                this.checkingWord = checkingWord;
                this.words = words;
            }
            public string checkingWord { get; set; }
            public List<Word> words { get; set; }
            public override string ToString()
            {
                return checkingWord + " " + words.Count();
            }
        }

        public class Sentence
        {
            public Sentence(int position, string sentence, List<string> splittedWords, int count, List<ExamWord> examinedWords)
            {
                this.position = position;
                this.sentence = sentence;
                this.splittedWords = splittedWords;
                this.count = count;
                this.examinedWords = examinedWords;
            }
            public int position { get; set; }
            public string sentence { get; set; }
            public List<string> splittedWords { get; set; }
            public int count { get; set; }
            public List<ExamWord> examinedWords { get; set; }

            public override string ToString()
            {
                return position + ". - (" + splittedWords.Count + ") - " + sentence;
            }
        }

        public static List<string> returnSentences(string path, int page)
        {
            var excel = new XLWorkbook(path);
            var ws = excel.Worksheet(page).RowsUsed();

            var toReturn = new List<string>();

            foreach (var i in ws) toReturn.Add(i.Cell(1).Value.ToString());

            return toReturn;
        }
    }
}
