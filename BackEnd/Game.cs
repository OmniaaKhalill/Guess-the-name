using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BackEnd
{

    enum GameState
    {
        FirstPlayerTurn,SecondPlayerTurn
    }
    internal class Game
    {
        public string Word {  get; }

        private int tries = 0;
        public GameState gameState { get; set; }

        public char[] GuessedChars { get; }
        
        public char[] CorrectChars { get; }
        public Game(string word)
        {          
            gameState= GameState.FirstPlayerTurn;
            Word = word.ToUpper();
            GuessedChars = new char[26];
            CorrectChars= new char[word.Length];
            for(int i=0; i < word.Length; i++)
            {
                CorrectChars[i]= '-';
            }
            
                
            
        }
        public bool ProcessGuess(char ch)
        {
            bool gameRunning = true;
            int count = 0;
            for (int i = 0; i < Word.Length; i++)
                if (ch == Word[i])
                {
                    CorrectChars[i]=ch;
                    count++;
                }
            if(!CorrectChars.Contains('-'))
                gameRunning= false;
                    
                                                 
            GuessedChars[tries]=ch;
            tries++;
            if (count == 0)
            {
                gameState = gameState == GameState.FirstPlayerTurn ? GameState.SecondPlayerTurn : GameState.FirstPlayerTurn;
            }
            return gameRunning;
        }
    }  
}
