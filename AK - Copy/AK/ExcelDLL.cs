using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AK.Models;

namespace AK
{
    public class ExcelDLL
    {
        public static List<string> returnSentences(string path, int page)
        {
            var excel = new XLWorkbook(path);
            var ws = excel.Worksheet(page).RowsUsed();

            var toReturn = new List<string>();

            foreach (var i in ws) toReturn.Add(i.Cell(1).Value.ToString());

            return toReturn;
        }

        public static List<List<string>> ExcelWorksheetsLetters()
        {
            var toReturn = new List<List<string>>();

            var firstLetters = new List<string>() { "A", "B", "C", "D" }; toReturn.Add(firstLetters);
            var secondLetters = new List<string>() { "E", "F", "G", "H" }; toReturn.Add(secondLetters);
            var thirdLetters = new List<string>() { "I", "J", "K", "L" }; toReturn.Add(thirdLetters);
            var fourthLetters = new List<string>() { "M", "N", "O", "P" }; toReturn.Add(fourthLetters);
            var fifthLetters = new List<string>() { "Q", "R", "S", "T" }; toReturn.Add(fifthLetters);
            var sixthLetters = new List<string>() { "U", "V", "W", "X" }; toReturn.Add(sixthLetters);
            var seventh = new List<string>() { "Y", "Z" }; toReturn.Add(seventh);
            var eighth = new List<string>() { "X", "AA", "AB" }; toReturn.Add(seventh);

            return toReturn;
        }
        // dar vienas budas turet sarasu sarasa
        // public static List<List<string>> sarasasSarase = new List<List<string>>() { new List<String>() { "a", "b" }, new List<string>() { "e", "h" } };

        public static void writeAttributesToExcel (List<string> attributes, string newFileName)
        {
            var workDocument = new XLWorkbook();
            var workSheet = workDocument.AddWorksheet("Attributai");
            var getLetter = ExcelWorksheetsLetters();
            var m = 1;
            foreach (var i in attributes)
            {
                workSheet.Cell("A" + m).Value = i;
                m++;
            }
            workSheet.Columns().AdjustToContents();
            workDocument.SaveAs(newFileName);
        }


        public static void writeDataToExcelOnlyMorfology(List<Sentence> sentences, string newFileName)
        {
            var workDocument = new XLWorkbook();
            var workSheet = workDocument.AddWorksheet("Rezultatai");
            var getLetter = ExcelWorksheetsLetters();
            var m = 1;
            foreach (var i in sentences)
            {
                foreach (var j in i.examinedWords)
                {
                    var index = 0;
                    foreach (var k in j.words)
                    {
                        workSheet.Cell(getLetter[index][0] + m).Value = k.word;
                        workSheet.Cell(getLetter[index][1] + m).Value = k.morfologyDetail.morfologyKalb;
                        workSheet.Cell(getLetter[index][2] + m).Value = k.morfologyDetail.morfologyGim;
                        workSheet.Cell(getLetter[index][3] + m).Value = k.morfologyDetail.morfologyLink;
                        index++;
                    }
                    workSheet.Cell(getLetter[index][0] + m).Value = j.examinedWordsmeaning;
                    workSheet.Cell(getLetter[index][0] + m).Value = j.nextToExaminedWord;
                    workSheet.Cell(getLetter[index][0] + m).Value = j.morfologyDetail.morfologyKalb;
                    workSheet.Cell(getLetter[index][1] + m).Value = j.morfologyDetail.morfologyGim;
                    workSheet.Cell(getLetter[index][2] + m).Value = j.morfologyDetail.morfologyLink;
                    m++;
                }
            }
            workSheet.Columns().AdjustToContents();
            workDocument.SaveAs(newFileName);
        }

        public static List<Sentence> WriteDataToExcelFile(List<Sentence> sentences, string newFileName)
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
                            if (sentences[i].examinedWords[j].words[k].word == Program._NotFoundWord)
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

            return sentences;
        }

        public static List<int> _klaidos = new List<int>();
        public static void FindExamWordsExtraDataFromExcel(string path, int page, List<Sentence> sentences)
        {
            var excel = new XLWorkbook(path);
            var ws = excel.Worksheet(page); //.Cell("B50");

            foreach ( var i in sentences )
            {
                var index = 0;
                foreach (var j in i.examinedWords)
                {
                        var x = ws.Cell(excelLetters[index] + i.position).Value;
                        var temp = x.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last().ToString();
                        j.numberClass = temp.ToLower();
                   
                    index++;
                }
            }
        }
        public static List<string> excelLetters = new List<string>() { "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD"};

    }
}
