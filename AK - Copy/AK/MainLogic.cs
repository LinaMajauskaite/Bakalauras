using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AK.Models;
using static AK.Program;

namespace AK
{
    public class MainLogic
    {
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
                        if (!splittedWords[moveUp].Contains("<") && !string.IsNullOrWhiteSpace(splittedWords[moveUp]))
                        {
                            toReturn.Add(new Word(+j, splittedWords[moveUp], "", new MorfologyDetail("", "", "" )));
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
                                toReturn.Add(new Word(+j, _NotFoundWord, "", new MorfologyDetail("", "", "")));
                                ++j;
                            }
                            break;
                        }
                        else
                        {
                            toReturn.Add(new Word(+j, _NotFoundWord, "", new MorfologyDetail("", "", "")));
                        }
                    }
                    else
                    {
                        while (j != maxPositionUpDown + 1)
                        {
                            toReturn.Add(new Word(+j, _NotFoundWord, "", new MorfologyDetail("", "", "")));
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
                        if (!splittedWords[moveDown].Contains("<") && !string.IsNullOrWhiteSpace(splittedWords[moveDown]))
                        {
                            toReturn.Add(new Word(-j, splittedWords[moveDown], "", new MorfologyDetail("", "", "")));
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
                                toReturn.Add(new Word(-j, _NotFoundWord, "", new MorfologyDetail("", "", "")));
                                ++j;
                            }
                            break;
                        }
                        else
                        {
                            toReturn.Add(new Word(-j, _NotFoundWord, "", new MorfologyDetail("", "", "")));
                            //++j;
                        }
                    }
                    else
                    {
                        while (j != maxPositionUpDown + 1)
                        {
                            toReturn.Add(new Word(-j, _NotFoundWord, "", new MorfologyDetail("", "", "")));
                            ++j;
                        }
                    }
                }
                isDone = true;
            }

            return toReturn;
        }


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
                var temp = "" ;
                var temp2 = "";
                
                i.splittedWords = i.splittedWords.Where(x => x != "" && x != "–" && x != "-").ToList();

                for (var j = 0; j < i.splittedWords.Count; j++)
                {
                    if (i.splittedWords[j].Contains("<"))
                    {
                        var pickedWords = returnAllWordsByExaminedWord(i.splittedWords, j, maxPositionUpDown);
                        var formattedWord = i.splittedWords[j].Replace("<", "").Replace(">", "");
                        var laikinas = i.splittedWords[j+1].Replace("<", "").Replace(">", "");

                        if (checkIfStringIsNumber(formattedWord)) temp = "sk";
                        else if (i.splittedWords[j].Contains("<")) temp = formattedWord;
                        else temp = _NotFoundWord;

                        if (checkIfStringIsNumber(laikinas)) temp2 = "sk";
                        else if (i.splittedWords[j + 1].Contains("<")) temp2 = laikinas;
                        else temp2 = _NotFoundWord;
                        examinedWordsList.Add(new ExamWord(i.splittedWords[j], pickedWords, "", "", new MorfologyDetail("", "", ""), temp, temp2));
                    }
                }

                i.examinedWords = examinedWordsList;
            }

            return sentences;
        }
        public static bool checkIfStringIsNumber(string text)
        {
            var regex = new Regex(@"^-*[0-9,\.]+$");
            return regex.IsMatch(text);
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

    }
}
