using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontEnd
{
    
    internal class GameButton:Button
    {
        public string RoomId {  get; set; }
        
        
        public GameButton(string Text)
        {
            this.Text = Text;

            this.Font = new Font("Arial", 12, FontStyle.Bold);
            this.ForeColor = Color.DarkCyan;

            this.BackColor = Color.White;

            this.Size = new Size(120, 40);

            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 2;

            this.FlatAppearance.BorderColor = Color.Black;

            this.FlatAppearance.MouseOverBackColor = Color.HotPink;
        }
        public GameButton(char ch)
        {
            this.Font = new Font("Arial", 12, FontStyle.Bold);
            this.Text = ch.ToString();
            ForeColor = Color.DarkCyan;
            BackColor = Color.White;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Size = new Size(65, 40);
        }
        
        
    }
}
