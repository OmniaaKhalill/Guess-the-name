using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FrontEnd
{
    internal static class UiDrawer
    {
        static public GameButton connectButton { get; private set; }
        static public Label playerInfo { get; private set; }
        static public Label roomInfo { get; private set; }
        static public Label category { get; private set; }
        static public Label[] FirstPlayerLabel { get; private set; }
        static public Label[] SecondPlayerLabel { get; private set; }
        static public Label RoomFirstPlayerLabel { get; private set; }
        static public Label RoomSecondPlayerLabel { get;  set; }
        static public Label info { get; private set; }
        static public GameButton[] joinRoom {  get; private set; }
        static public GameButton[] watchRoom { get; private set; }
        static public GameButton createRoom { get; private set; }
        static public Form playerDialog { get; private set; }
        static public GameButton cancelJoin { get; set; }
        static public Form playAgainDialog { get; private set; }
        static public GameButton acceptPlayer { get; private set; }
        static public GameButton refusePlayer { get; private set; }
        static public Form JoinDialog { get; private set; }
        static public ListBox categoriesListBox { get; private set; }
        static public GameButton startGame { get; private set; }
        static public GameLabel wordLabel {  get; set; }
        static public GameLabel turnLabel { get; set; }
        static public GameButton[] keyboard { get; set; }
        static public GameButton leaveButton { get; set; }
        static public GameButton yesButton { get; set; }
        static public GameButton noButton { get; set; }
        static public TextBox nameTextBox { get; private set; }
        static public Client clientToDraw { get; private set; }
        static public void DrawStartScreen(Client client)
        {
            if (System.IO.File.Exists("bac.jpg"))
            {
                Image backgroundImage = Image.FromFile("bac.jpg");
                client.BackgroundImage = backgroundImage;
            }
            nameTextBox = new TextBox();
            nameTextBox.Location= new Point(400, 200);
            nameTextBox.Size = new Size(120, 40);
            client.Controls.Add(nameTextBox);
            
            connectButton = new GameButton("Connect");
            
            
            connectButton.Location = new Point(400, 240);
            
            client.Controls.Add(connectButton);
            
            
        }
        static public void DrawLobby(Client client, string[][] rooms)
        {
            client.AutoScroll = true;
            
            client.BackgroundImage = null;
            
            client.Controls.Clear();
            playerInfo = new Label();
            playerInfo.Text = $"Username: {client.ClientName} | ID: {client.id}" ;
            playerInfo.Size = new Size(400, 50);
            playerInfo.ForeColor = Color.White;
            playerInfo.Font = new Font("arial", 12, FontStyle.Bold);
            playerInfo.BackColor = Color.Transparent;
            playerInfo.Location = new Point(40, 40);
            client.Controls.Add(playerInfo);
            createRoom = new GameButton("Create Room");
            createRoom.Location = new Point(400, 100);
            client.Controls.Add(createRoom);
            joinRoom= new GameButton[rooms.Length];
            watchRoom = new GameButton[rooms.Length];
            FirstPlayerLabel = new Label[rooms.Length];
            SecondPlayerLabel= new Label[rooms.Length];
            int y = 200;
            if (rooms.Length > 0)
                for(int i = 0; i < rooms.Length; i++)
                {
                    // if two player make second label and check for game status for watch button
                    joinRoom[i] = new GameButton("Join");
                    watchRoom[i] = new GameButton("Watch");                    
                    if (bool.Parse(rooms[i][1])==false)
                        watchRoom[i].Enabled = false;
                    if (rooms[i].Length>3)
                    {
                        try
                        {
                            bool pending=bool.Parse(rooms[i][3]);
                            joinRoom[i].Enabled = pending ? false : true;
                        }
                        catch
                        {
                            joinRoom[i].Enabled = false;
                            SecondPlayerLabel[i] = new GameLabel(rooms[i][3], 2);
                            SecondPlayerLabel[i].Location = new Point(300, y);
                            client.Controls.Add(SecondPlayerLabel[i]);
                        }
                        
                    }
                    FirstPlayerLabel[i]= new GameLabel(rooms[i][2],1);
                    FirstPlayerLabel[i].Location = new Point(200, y);                  
                    watchRoom[i].RoomId = joinRoom[i].RoomId = rooms[i][0];                  
                    joinRoom[i].Location = new Point(600, y);
                    watchRoom[i].Location = new Point(500, y);
                    y += 80;
                    client.Controls.Add(watchRoom[i]);
                    client.Controls.Add(joinRoom[i]);
                    client.Controls.Add(FirstPlayerLabel[i]);
                }
            client.BackColor = ColorTranslator.FromHtml("#0b0b0b");
        }
        static public void DrawRoom(Client client, string[] categories)
        {
            client.Controls.Clear();
            client.AutoScroll = false;
            roomInfo = new Label();
            roomInfo.Text = $"Room ID: {client.roomId}";
            roomInfo.Size = new Size(400, 50);
            roomInfo.ForeColor = Color.White;
            roomInfo.Font = new Font("arial", 12, FontStyle.Bold);
            roomInfo.BackColor = Color.Transparent;
            roomInfo.Location = new Point(40, 40);
            client.Controls.Add(roomInfo);
            //client.Width = 700;
            
            categoriesListBox = new ListBox();
            categoriesListBox.BackColor= Color.White;
            categoriesListBox.ForeColor= Color.DarkCyan;
            categoriesListBox.Location = new Point(650, 100);
            categoriesListBox.Size = new Size(110,150);
            categoriesListBox.Font = new Font("arial", 14);
            categoriesListBox.Items.AddRange(categories);
            categoriesListBox.SelectionMode = SelectionMode.One;
            
            startGame = new GameButton("Start Game");
            startGame.Location = new Point(370, 100);
            startGame.Enabled = false;
            
            if (string.IsNullOrEmpty(client.secondPlayerName))
            {
                client.Controls.Add(categoriesListBox);
                client.Controls.Add(startGame);
                RoomFirstPlayerLabel = new GameLabel(client.ClientName, 1);
                RoomFirstPlayerLabel.Location = new Point(200, 211);
                RoomSecondPlayerLabel= new GameLabel("", 2);
                RoomSecondPlayerLabel.Location = new Point(460, 260);
                client.Controls.Add(RoomFirstPlayerLabel);
                client.Controls.Add(RoomSecondPlayerLabel);
            }
            else
            {
                Label indicator = new Label();
                
                indicator.Text = $"Waiting For {client.secondPlayerName} To Start Game";
                indicator.Size = new Size(400, 50);
                indicator.ForeColor = Color.White;
                indicator.Font = new Font("arial", 12, FontStyle.Bold);
                indicator.BackColor = Color.Transparent;
                indicator.Location = new Point(300, 100);
                client.Controls.Add(indicator);
                categoriesListBox.Enabled = false;
                RoomFirstPlayerLabel = new GameLabel(client.secondPlayerName, 1);
                RoomFirstPlayerLabel.Location = new Point(200, 211);
                RoomSecondPlayerLabel= new GameLabel(client.ClientName, 2);
                RoomSecondPlayerLabel.Location= new Point(460, 260);
                client.Controls.Add(RoomFirstPlayerLabel);
                client.Controls.Add(RoomSecondPlayerLabel);
            }
            if (System.IO.File.Exists("RoomVersus.png"))
            {
                PictureBox pb = new PictureBox();
                pb.Image = Image.FromFile("RoomVersus.png");
                pb.Location = new System.Drawing.Point(150, 150);

                pb.Size = new System.Drawing.Size(500, 500);
                client.Controls.Add(pb);
                //Image backgroundImage = Image.FromFile("RoomVersus.png");
                //client.BackgroundImage = backgroundImage;
                //client.BackgroundImage.Size = new Size(400,200);
                //client.BackgroundImage.
            }
        }
        static public void DrawAcceptPLayer(Client client, string name,int id)
        {
            clientToDraw = client;
            playerDialog = new Form();
            playerDialog.StartPosition = FormStartPosition.CenterParent;
            if (System.IO.File.Exists("reviewJoin.jpg"))
            {
                Image backgroundImage = Image.FromFile("reviewJoin.jpg");
                playerDialog.BackgroundImage = backgroundImage;
                playerDialog.Size = new Size(backgroundImage.Width, backgroundImage.Height);
            }

            Label info = new Label();
            info.Size = new Size(200, 50);
            info.Location = new Point(120, 20);
            info.Text = $"{name} With Id {id} Wants To Join";
            info.ForeColor = Color.White;
            info.BackColor = Color.Black;
            playerDialog.Controls.Add(info);
            playerDialog.ControlBox = false;
            
            playerDialog.ShowInTaskbar = false;
            playerDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            acceptPlayer = new GameButton("Accept");
            refusePlayer = new GameButton("Refuse");
            refusePlayer.Click += PlayerReviewButtonClick;
            acceptPlayer.Click += PlayerReviewButtonClick;
            playerDialog.Controls.Add(info);
            acceptPlayer.Location = new Point(80, 200);
            refusePlayer.Location = new Point(220, 200);
            playerDialog.Controls.Add(acceptPlayer);
            playerDialog.Controls.Add(refusePlayer);
            playerDialog.ShowDialog(client); 
            //
            //info = new Label();
            //info.Size = new Size(200, 50);
            //info.Text = $"{name} With Id {id} Wants To Join";
            //info.ForeColor = Color.White;
            //info.Size = new Size(300, 40);
            //info.Location = new Point(200, 300);
            //acceptPlayer = new GameButton("Accept");
            //refusePlayer = new GameButton("Refuse");
            //client.Controls.Add(info);
            //acceptPlayer.Location = new Point(200, 350);
            //refusePlayer.Location= new Point(400,350);
            //client.Controls.Add(acceptPlayer);
            //client.Controls.Add(refusePlayer);

        }
        private static void PlayerReviewButtonClick(object? sender, EventArgs e)
        {
            GameButton tempButton = (GameButton)sender;
            Message request = new Message();
            request.action = Action.AcceptJoin;
            playerDialog.Close();
            if (tempButton.Text == "Accept")
            {
                if (UiDrawer.categoriesListBox.SelectedIndex != -1)
                {
                    UiDrawer.startGame.Enabled = true;
                }
                UiDrawer.categoriesListBox.SelectedIndexChanged += (obj, events) => { UiDrawer.startGame.Enabled = true; };
                request.parameters["accepted"] = true.ToString();
                request.parameters["roomId"] = clientToDraw.roomId.ToString();
                UiDrawer.RoomSecondPlayerLabel.Text = "Player 2: " + clientToDraw.secondPlayerName;
                //UiDrawer.RoomSecondPlayerLabel.Location = new Point(200, 300);
                //clientToDraw.Controls.Add(UiDrawer.RoomSecondPlayerLabel);
            }
            else
            {
                request.parameters["roomId"] = clientToDraw.roomId.ToString();
                request.parameters["accepted"] = false.ToString();
                clientToDraw.secondPlayerName = string.Empty;
            }

            //Controls.Remove(UiDrawer.playerDialog);
            request.parameters["accepted"] = tempButton.Text == "Accept" ? true.ToString() : false.ToString();
            request.parameters["playerId"] = clientToDraw.secondPlayerId.ToString();
            //requestText = request.SendMessage();
            clientToDraw.stream.Write(Encoding.UTF8.GetBytes(request.SendMessage()));
            clientToDraw.stream.Flush();


        }

        internal static void DrawJoinDialog(int roomId)
        {
            JoinDialog = new Form();
            JoinDialog.StartPosition = FormStartPosition.CenterParent;
            //JoinDialog.BackColor = ColorTranslator.FromHtml("#4B4444");
            //clientToDraw = client;
            if (System.IO.File.Exists("loading.png"))
            {
                Image backgroundImage = Image.FromFile("loading.png");
                JoinDialog.BackgroundImage = backgroundImage;
                JoinDialog.Size = new Size(backgroundImage.Width,backgroundImage.Height);
            }
            
            Label info = new Label();
            info.Size = new Size(200, 50);
            info.Location = new Point(120, 20);
            info.Text = "Waiting For Player to Accept";
            info.ForeColor = Color.White;
            info.BackColor = Color.Black;
            JoinDialog.Controls.Add(info);
            JoinDialog.ControlBox = false;
            
            JoinDialog.ShowInTaskbar= false;
            JoinDialog.FormBorderStyle = FormBorderStyle.FixedSingle;
            cancelJoin = new GameButton("Cancel");
            cancelJoin.Location = new Point(300, 20);
            cancelJoin.RoomId= roomId.ToString();
            cancelJoin.Click += (s, e) => { JoinDialog.Close(); };
                
            JoinDialog.Controls.Add(cancelJoin);
            JoinDialog.ShowDialog();
        }

        private static void cancelButtonClick(object? sender, EventArgs e)
        {
            
                GameButton temp = (GameButton)sender;
                Message request = new Message();
                request.parameters["roomId"] = temp.RoomId.ToString();
                request.action = Action.CancelJoin;

                clientToDraw.stream.Write(Encoding.UTF8.GetBytes(request.SendMessage()));
                clientToDraw.stream.Flush();
                JoinDialog.Close();
            
        }

        internal static void DrawGame(Client client,string gameCategory, string word,string guessedChars,string turnName,int turnId,bool watcher)
        {
            //client.Width = 637;
            client.AutoScroll = false;
            
            
            keyboard = new GameButton[26];
            string chars = "QWERTYUIOPASDFGHJKLZXCVBNM";
            client.Controls.Clear();
            int x = 100;
            int y = 250;
            for (int i = 0; i < keyboard.Length; i++)
            {
                y = i == 10 ? y + 48 : i==19?y+48:y;
                x = i == 10 ?  130: i == 19 ? 180 : x;
                keyboard[i] = new GameButton(chars[i]);
                keyboard[i].Location = new Point(x, y);
                if (guessedChars.Contains(chars[i]) )
                {
                    keyboard[i].Enabled = false;
                }
                
                client.Controls.Add(keyboard[i]);
                x += 70;
                if(watcher)
                {
                    
                    keyboard[i].FlatAppearance.MouseOverBackColor = Color.White;
                    keyboard[i].FlatAppearance.MouseDownBackColor = Color.White;
                }
            }
            category = new Label();
            category.Text = $"Category: {gameCategory}";
            category.Size = new Size(300, 50);
            category.ForeColor = Color.White;
            category.Font = new Font("arial", 12, FontStyle.Bold);
            category.BackColor = Color.Transparent;
            category.Location = new Point(40, 40);
            client.Controls.Add(category);
            wordLabel = new GameLabel();
            wordLabel.Text = word;           
            wordLabel.TextAlign = ContentAlignment.MiddleCenter;
            wordLabel.Size = new Size(550, 60);
            wordLabel.Font = new Font("arial", 32);
            wordLabel.ForeColor = Color.White;
            wordLabel.Location = new Point(170, 150);
            client.Controls.Add(wordLabel);
            turnLabel= new GameLabel(turnName);
            turnLabel.Size = new Size(180, 40);
            turnLabel.Location = new Point(350, 50);
            client.Controls.Add(turnLabel);
            if (turnId!=client.id&&watcher==false)
            {
                foreach (var key in keyboard)
                {
                    key.Enabled = false;
                }
            }
            
            if (watcher)
            {
                leaveButton = new GameButton("Stop Watching");
                leaveButton.Location = new Point(700, 50);
                client.Controls.Add(leaveButton);

            }
            //GameLabel turnlabel = new GameLabel();
            //wordLabel = new Label();
            //wordLabel.Text = word;
            //wordLabel.Size = new Size(150, 40);
            //wordLabel.Location = new Point(450,300);
            //wordLabel.BackColor = ColorTranslator.FromHtml("#D9D9D9");
            
        }

       internal static void RenderAskToPlayAgain(Client client,bool won, string word)
        {
            clientToDraw = client;
            playAgainDialog = new Form();
            playAgainDialog.ControlBox = true;
            playAgainDialog.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            playAgainDialog.StartPosition = FormStartPosition.CenterScreen;
            //playAgainDialog.BackColor = ColorTranslator.FromHtml("#4B4444");
            Label info = new Label();
            info.Location = new Point(10, 10);
            info.Size = new Size(200, 50);
            if (won)
            {
                if (System.IO.File.Exists("congrats.png"))
                {
                    Image backgroundImage = Image.FromFile("congrats.png");
                    playAgainDialog.BackgroundImage = backgroundImage;
                    playAgainDialog.Size = new Size(backgroundImage.Width, backgroundImage.Height);
                    info.Text = $"word was {word} Play Again?";
                }
            }
            else
            {
                if (System.IO.File.Exists("hardLuck.png"))
                {
                    Image backgroundImage = Image.FromFile("hardLuck.png");
                    playAgainDialog.BackgroundImage = backgroundImage;
                    playAgainDialog.Size = new Size(backgroundImage.Width, backgroundImage.Height);
                    info.Text = $"word was {word} Play Again?";
                }
            }
            
            info.ForeColor = Color.White;
            info.BackColor = Color.Transparent;
            playAgainDialog.Controls.Add(info);
            //client.Controls.Add(info);
            yesButton = new GameButton("Yes");
            yesButton.Location = new Point(250, 10);

            noButton = new GameButton("No");
            noButton.Location = new Point(400, 10);
            noButton.Click += playAgainButtonClick;
            yesButton.Click += playAgainButtonClick;
            playAgainDialog.Controls.Add(yesButton);
            playAgainDialog.Controls.Add(noButton);
            playAgainDialog.Show();
            //client.Controls.Add(yesButton);
            //client.Controls.Add(noButton);
            //client.ControlBox = true;
            //client.ShowInTaskbar = false;
            //client.FormBorderStyle = FormBorderStyle.FixedSingle;
            //client.ShowDialog();
        }
        private static void playAgainButtonClick(object? sender, EventArgs e)
        {
            playAgainDialog.Close();
            GameButton temp = (GameButton)sender;
            Message request = new Message();
            request.parameters["roomId"] = clientToDraw.roomId.ToString();
            if (temp.Text == "Yes")
                request.parameters["isAccepted"] = true.ToString();
            else
                request.parameters["isAccepted"] = false.ToString();
            request.action = Action.ResponseToPlayAgain;
            
            clientToDraw.stream.Write(Encoding.UTF8.GetBytes(request.SendMessage()));
            clientToDraw.stream.Flush();

        }
    }
}
