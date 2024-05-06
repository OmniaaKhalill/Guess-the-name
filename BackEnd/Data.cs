using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd
{
    
    
    internal class Data
    {
        public List<Player> Players { get; }
        public List<Room> Rooms { get; }
        //public Dictionary<string, List<char[]>> Words { get; }
        public static Dictionary<string, List<string>> WordsCategories;
        private static Random random = new Random();

        public string[] categories { get;  }

        public Data()
        {
            categories= ["Fruits","Animals","Colors","Sports","Countries"];
            Players = new List<Player>();
            Rooms = new List<Room>();
            WordsCategories = new Dictionary<string, List<string>>();
            //categories = GetCategoires(new DirectoryInfo(""));
            PopulateWords();
        }

        
        private void PopulateWords()
        {
            using (StreamReader sr = new StreamReader("words.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                    {
                        string[] parts = line.Split(' ');
                        string category = parts[0].Substring(1);
                        List<string> words = new List<string>(parts[1..]);
                        WordsCategories.Add(category, words);
                    }
                }
            }
        }
        public string GetRandomWord(string category)
        {
            List<string> categoryWords = WordsCategories[category];
            int randomIndex = random.Next(0, categoryWords.Count);
            return categoryWords[randomIndex];
        }
        public string getRooms()
        {
            if (Rooms.Count == 0)
                return "";
            StringBuilder rooms = new StringBuilder("");
            foreach (var room in Rooms)
            {
                rooms.Append(room.ToString() + ",,");

            }
            rooms.Replace(",,", "", rooms.Length - 2, 2);
            return rooms.ToString();
        }
    }
}
