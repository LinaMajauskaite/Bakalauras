using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AK
{
    public class Models
    {
        public class Word
        {
            public Word(int row, string word, string morfology, MorfologyDetail morfologyDetail)
            {
                this.row = row;
                this.word = word;
                this.morfology = morfology;
                this.morfologyDetail = morfologyDetail;
            }

            public int row { get; set; }
            public string word { get; set; }
            public string morfology { get; set; }
            public MorfologyDetail morfologyDetail { get; set; }
            public override string ToString()
            {
                return row + " " + word + " Morfology: " + morfology;
            }
        }
        public class ExamWord
        {
            public ExamWord(string checkingWord, List<Word> words, string numberClass, string morfology, MorfologyDetail morfologyDetail, string examinedWordsmeaning, string nextToExaminedWord)
            {
                this.checkingWord = checkingWord;
                this.words = words;
                this.numberClass = numberClass;
                this.morfology = morfology;
                this.morfologyDetail = morfologyDetail;
                this.examinedWordsmeaning = examinedWordsmeaning;
                this.nextToExaminedWord = nextToExaminedWord;
            }

            public string checkingWord { get; set; }
            public List<Word> words { get; set; }
            public string numberClass { get; set; }
            public string morfology { get; set; }
            public MorfologyDetail morfologyDetail { get; set; }
            public string examinedWordsmeaning { get; set; }
            public string nextToExaminedWord { get; set; }

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

        public class MorfologyDetail
        {
            public MorfologyDetail(string morfologyKalb, string morfologyGim, string morfologyLink)
            {
                this.morfologyKalb = morfologyKalb;
                this.morfologyGim = morfologyGim;
                this.morfologyLink = morfologyLink;
            }

            public string morfologyKalb { get; set; }
            public string morfologyGim { get; set; }
            public string morfologyLink { get; set; }
            public override string ToString()
            {
                return morfologyKalb + " " + morfologyGim + " " + morfologyLink;
            }
        }

        public class AttributeMorfology
        {
            public AttributeMorfology(string word, string morfology, MorfologyDetail morfologyDetail)
            {
                this.word = word;
                this.morfology = morfology;
                this.morfologyDetail = morfologyDetail;
            }

            public string word { get; set; }
            public string morfology { get; set; }
            public MorfologyDetail morfologyDetail { get; set; }

            public override string ToString()
            {
                return word + " " + morfology;
            }
        }

    }
}
