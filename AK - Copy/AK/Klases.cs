using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AK
{
    public class Klases
    {
        public class Word
        {
            //sukuriama zodziu klase: renkami duomenys eile, zodis ir morfologine reiksme.
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
                return row + " " + word;
            }
        }
        public class ExamWord
        {
            //tikrinamas sakinys, kiek turi zodziu, surasomi patys zodziai ir ju kiekis
            public ExamWord(string checkingWord, List<Word> words, string numberClass, string morfology)
            {
                this.checkingWord = checkingWord;
                this.words = words;
                this.numberClass = numberClass;
                this.morfology = morfology;
            }
            public string checkingWord { get; set; }
            public List<Word> words { get; set; }
            public string numberClass { get; set; }
            public string morfology { get; set; }
            public override string ToString()
            {
                return checkingWord + " " + words.Count();
            }
        }
        public class Sentence
        {
            //sukurta sakinio klase. 
            // sukuriama sakinio reikalavimai: viena, koks sakinys, iskaidytas sakinys po zodi, is kiek zodziu susidaro sakinys, NEZINAU KAS TAS EXAMINEDWORDS 
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
    }
}
