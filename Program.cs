using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TestTask
{
    public class Program
    {

        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            using (IReadOnlyStream inputStream1 = GetInputStream(args[0]))
            using (IReadOnlyStream inputStream2 = GetInputStream(args[1]))
            {
                IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

                RemoveCharStatsByType(ref singleLetterStats, CharType.Vowel);
                RemoveCharStatsByType(ref doubleLetterStats, CharType.Consonants);

                PrintStatistic(singleLetterStats);
                PrintStatistic(doubleLetterStats);

                Console.ReadKey();
            }
            // TODO : Необжодимо дождаться нажатия клавиши, прежде чем завершать выполнение программы.
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            List<LetterStats> listLetterStats = new List<LetterStats>();

            stream.ResetPositionToStart();
            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                string letter = c.ToString();
                var index = listLetterStats.FindIndex(ls => ls.Letter == letter);
                if (index == -1)
                {
                    LetterStats newLetterStat = new LetterStats { Letter = letter, Count = 1 };
                    listLetterStats.Add(newLetterStat);
                }
                else
                {
                    LetterStats newLetterStat = listLetterStats[index];
                    listLetterStats.Remove(newLetterStat);
                    IncStatistic(ref newLetterStat);
                    listLetterStats.Add(newLetterStat);
                }
            }
            return listLetterStats;
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            List<LetterStats> listLetterStats = new List<LetterStats>();

            stream.ResetPositionToStart();

            char prevChar = char.MaxValue;
            while (!stream.IsEof)
            {
                char currChar = stream.ReadNextChar();
                if (!char.IsLetter(currChar)) continue;

                currChar = char.Parse(currChar.ToString().ToLower());

                if (currChar != prevChar)
                {
                    prevChar = currChar;
                    continue;
                }

                var pair = prevChar.ToString() + currChar.ToString();

                var index = listLetterStats.FindIndex(ls => ls.Letter == pair);
                if (index == -1)
                {
                    LetterStats newLetterStat = new LetterStats { Letter = pair, Count = 1 };
                    listLetterStats.Add(newLetterStat);
                }
                else
                {
                    LetterStats newLetterStat = listLetterStats[index];
                    listLetterStats.Remove(newLetterStat);
                    IncStatistic(ref newLetterStat);
                    listLetterStats.Add(newLetterStat);
                }

                prevChar = currChar;
            }
            return listLetterStats;
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(ref IList<LetterStats> letters, CharType charType)
        {
            List<string> charsToRemove = new List<string>();
            string pattern = "^(?i:[aeiouyаеёиоуыэюя]).*";
            // TODO : Удалить статистику по запрошенному типу букв.
            switch (charType)
            {
                case CharType.Consonants:
                    charsToRemove.AddRange(new string[] { "a", "e", "i", "o", "u" });
                    charsToRemove.AddRange(new string[] { "а", "о", "у", "э", "ы", "я", "ё", "ю", "е", "и" });
                    break;
                case CharType.Vowel:
                    for (char c = 'a'; c <= 'z'; c++)
                    {
                        if (!"aeiou".Contains(c)) charsToRemove.Add(c.ToString());
                    }
                    for (char c = 'а'; c <= 'я'; c++)
                    {
                        if (!"аоуэыяёюеи".Contains(c)) charsToRemove.Add(c.ToString());
                    }
                    break;
            }

            for (int i = letters.Count - 1; i > 0; i--)
            {
                if (Regex.IsMatch(letters[i].Letter, pattern)) letters.Remove(letters[i]);
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            foreach (LetterStats letterStats in letters.OrderBy(l => l.Letter))
            {
                Console.WriteLine($"Char: {letterStats.Letter} Count: {letterStats.Count}.");
            }
        }
        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(ref LetterStats letterStats)
        {
            letterStats.Count++;
        }
    }
}
