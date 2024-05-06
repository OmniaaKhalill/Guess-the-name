using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontEnd
{
    internal class GameLabel:Label
    {
       
        public GameLabel()
        {
            TextAlign = ContentAlignment.MiddleCenter;
            Size = new Size(150, 60);
            Font = new Font("Times New Roman", 32);
            ForeColor = Color.White;
        }
        
        public GameLabel(string playerName,int i)
        {
             
            Text = i==1?"Player 1: " + playerName:"Player 2: "+playerName;
            Size = new Size(200, 40);
            ForeColor = Color.DarkCyan;
            TextAlign=ContentAlignment.MiddleLeft;
            BackColor = Color.White;
            
        }
        public GameLabel(string playerName) 
        {
            Text = "Turn: " + playerName;
            ForeColor = Color.DarkCyan;

            TextAlign = ContentAlignment.MiddleCenter;
            Size = new Size(250, 40);
            BackColor = Color.White;
            Location = new Point(100, 100);
        }

    }
}
