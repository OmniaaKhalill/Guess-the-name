using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;
using System.CodeDom.Compiler;

namespace FrontEnd
{
    enum Action
    {
        Connect, CreateRoom, UpdateLobby, RequestJoin,CancelJoin, ReviewJoin, AcceptJoin, JoinRoom, StartGame, GuessCharacter, WatchGame, StopWatching, AskPlayAgain,ResponseToPlayAgain
    }
    class Message
    {
        public Action action { get; set; }
        public Dictionary<string, string> parameters { get; }
        public Message()
        {
            parameters = new Dictionary<string, string>();
        }

        public Message(Action action, Dictionary<string, string> parameters)
        {
            this.action = action;
            this.parameters = parameters;
        }
        public string SendMessage()
        {
            string temp = "";
            temp += action.ToString() + "$";
            foreach (var keyValues in parameters)
            {
                temp += keyValues.Key + "=" + keyValues.Value + "$";
            }

            return temp;
        }
        static public Message InterpretMessage(string msg)
        {
            string[] values = msg.Trim('$').Split("$");
            Message temp = new Message();
            temp.action = (Action)Enum.Parse(typeof(Action), values[0]);
            for (int i = 1; i < values.Length; i++)
            {
                string[] keyPair = values[i].Split("=");
                temp.parameters.Add(keyPair[0], keyPair[1]);
            }
            return temp;
        }
    }
    public partial class Client : Form
    {
        #region member variables
        public string ClientName { get; set; }
        public string secondPlayerName { get; set; }
        public int id { get; private set; }
        public int secondPlayerId { get; private set; }
        public int roomId { get; private set; }
        
        TcpClient client;
        IPEndPoint connectoin;
        public NetworkStream stream { get; private set; }
        BackgroundWorker clientWorker;
        Message request;
        Message response;
        string requestText;
        public delegate void watcherLeave(Object sender, EventArgs e);
        public event watcherLeave LeaveListener;
        #endregion
        #region constructor
        public Client()
        {
            roomId = 0;
            id = -1;
            InitializeComponent();
            Config();
            connectoin = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 3000);
            client = new TcpClient();
            clientWorker = new BackgroundWorker();
            clientWorker.WorkerSupportsCancellation = true;
            clientWorker.DoWork += listenToServer;
            request = new Message();
            response = new Message();
            UiDrawer.DrawStartScreen(this);
            UiDrawer.connectButton.Click += connectButtonClick;
            
        }
        private void Config()
        {
            this.Text = "Guess The Name";
            this.Width = 900;
            this.Height = 500;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            //this.AutoScroll = true;
        }
        #endregion
        #region button events

        private void connectButtonClick(object? sender, EventArgs e)
        {
            try
            {
                ClientName = request.parameters["userName"] = UiDrawer.nameTextBox.Text;
                if (ClientName == "")
                    MessageBox.Show("please Enter a name");
                else
                {
                    client.Connect(connectoin);
                    stream = client.GetStream();
                    clientWorker.RunWorkerAsync();
                    request.action = Action.Connect;
                    requestText = request.SendMessage();
                    stream.Write(Encoding.UTF8.GetBytes(requestText));
                    stream.Flush();
                    Controls.Clear();
                }        
            }
            catch
            {
                MessageBox.Show("server not running");
            }
            //id = int.Parse(response.parameters["id"]);            
        }
        private void createRoomButtonClick(object? sender, EventArgs e)
        {
            //request.parameters["id"] = id.ToString();
            request.action = Action.CreateRoom;
            requestText = request.SendMessage();
            stream.Write(Encoding.UTF8.GetBytes(requestText));
            stream.Flush();
            Controls.Clear();
        }
        private void joinRoomButtonClick(object? sender, EventArgs e)
        {
            GameButton temp = (GameButton)sender;
            roomId = int.Parse(temp.RoomId);
            request.parameters["roomId"] = temp.RoomId;
            request.action = Action.RequestJoin;
            requestText = request.SendMessage();
            stream.Write(Encoding.UTF8.GetBytes(requestText));
            stream.Flush();
            UiDrawer.DrawJoinDialog(roomId);
            
            
        }
        private void PlayerReviewButtonClick(object? sender, EventArgs e)
        {
            GameButton tempButton = (GameButton)sender;
            request.action = Action.AcceptJoin;
            Controls.Remove(UiDrawer.info);
            Controls.Remove(UiDrawer.acceptPlayer);
            Controls.Remove(UiDrawer.refusePlayer);
            if (tempButton.Text == "Accept")
            {
                if (UiDrawer.categoriesListBox.SelectedIndex != -1)
                {
                    UiDrawer.startGame.Enabled = true;
                }
                UiDrawer.categoriesListBox.SelectedIndexChanged += (obj, events) => { UiDrawer.startGame.Enabled = true; };
                request.parameters["accepted"] = true.ToString();
                request.parameters["roomId"] = roomId.ToString();
                UiDrawer.RoomSecondPlayerLabel = new GameLabel(secondPlayerName, 2);
                UiDrawer.RoomSecondPlayerLabel.Location = new Point(200, 300);
                Controls.Add(UiDrawer.RoomSecondPlayerLabel);
            }
            else
            {
                request.parameters["roomId"] = roomId.ToString();
                request.parameters["accepted"] = false.ToString();
                secondPlayerName = string.Empty;
            }

            Controls.Remove(UiDrawer.playerDialog);
            request.parameters["accepted"] = tempButton.Text == "Accept" ? true.ToString() : false.ToString();
            request.parameters["playerId"] = secondPlayerId.ToString();
            requestText = request.SendMessage();
            stream.Write(Encoding.UTF8.GetBytes(requestText));
            stream.Flush();


        }
        private void startGameClick(object? sender, EventArgs e)
        {
            
            request.parameters["roomId"] = roomId.ToString();
            request.parameters["category"] = UiDrawer.categoriesListBox.SelectedItem.ToString();
            request.action = Action.StartGame;
            requestText = request.SendMessage();
            stream.Write(Encoding.UTF8.GetBytes(requestText));
            stream.Flush();
        }
        private void keyClick(object? sender, EventArgs e)
        {
            GameButton temp = (GameButton)sender;
            request.parameters["roomId"] = roomId.ToString();
            request.parameters["key"] = temp.Text;
            request.action = Action.GuessCharacter;
            requestText = request.SendMessage();
            if (!String.IsNullOrEmpty(secondPlayerName))
                stream.Write(Encoding.UTF8.GetBytes(requestText));
            stream.Flush();
        }
        private void watchRoomButtonClick(object? sender, EventArgs e)
        {
            GameButton temp = (GameButton)sender;
            roomId = int.Parse(temp.RoomId);
            request.parameters["roomId"] = temp.RoomId;
            request.action = Action.WatchGame;
            requestText = request.SendMessage();
            stream.Write(Encoding.UTF8.GetBytes(requestText));
            stream.Flush();

        }
        private void leaveButtonClick(object? sender, EventArgs e)
        {
            request.action = Action.StopWatching;
            requestText = request.SendMessage();
            request.parameters["roomId"] = roomId.ToString();
            roomId = 0;
            stream.Write(Encoding.UTF8.GetBytes(requestText));
            stream.Flush();
            Controls.Clear();
        }
        private void playAgainButtonClick(object? sender, EventArgs e)
        {
            GameButton temp = (GameButton)sender;
            request.parameters["roomId"] = roomId.ToString();
            if (temp.Text == "Yes")         
                request.parameters["isAccepted"] = true.ToString();         
            else           
                request.parameters["isAccepted"] = false.ToString();        
            request.action = Action.ResponseToPlayAgain;
            requestText = request.SendMessage();
            stream.Write(Encoding.UTF8.GetBytes(requestText));
            stream.Flush();

        }
        #endregion
        #region server listener methods
        private void listenToServer(object? sender, DoWorkEventArgs e)
        {
            byte[] buffer = new byte[1024];
            int bytes;           
            string responseText;
            while ((bytes = stream.Read(buffer)) != 0)
            {
                responseText  = Encoding.UTF8.GetString(buffer, 0, bytes);               
                response=Message.InterpretMessage(responseText);
                switch (response.action)
                {
                    case Action.Connect:
                        if (id==-1)
                        {
                            id = int.Parse(response.parameters["id"]);
                            RenderLobby();
                        }
                                             
                        break;
                    case Action.CreateRoom:
                        if (!string.IsNullOrEmpty(secondPlayerName))
                        {
                            UiDrawer.playAgainDialog.Close();
                            UiDrawer.playerDialog.Close();
                        }
                            
                        if (roomId==0)
                            roomId = int.Parse(response.parameters["roomId"]);
                        secondPlayerId = 0;
                        secondPlayerName = "";
                        RenderRoom();
                        break;
                    case Action.UpdateLobby:
                        if(!string.IsNullOrEmpty(secondPlayerName))
                            UiDrawer.playAgainDialog.Close();
                        
                        secondPlayerId = 0;
                        secondPlayerName = "";
                        roomId = 0;
                        RenderLobby();
                        break;
                    case Action.ReviewJoin:                       
                        secondPlayerId = int.Parse(response.parameters["playerId"]);
                        secondPlayerName= response.parameters["playerName"];
                        if (this.InvokeRequired)
                            this.Invoke(() => { UiDrawer.DrawAcceptPLayer(this, secondPlayerName,secondPlayerId); });
                        else
                            UiDrawer.DrawAcceptPLayer(this, secondPlayerName, secondPlayerId);
                        UiDrawer.acceptPlayer.Click += PlayerReviewButtonClick;
                        UiDrawer.refusePlayer.Click += PlayerReviewButtonClick;
                        break;
                    case Action.JoinRoom:
                        if (this.InvokeRequired)
                            this.Invoke(checkJoinRquest);
                        else
                            checkJoinRquest();                       
                        break;
                    case Action.StartGame:
                        RenderGame();
                        break;
                    case Action.GuessCharacter:
                        RenderGame();
                        break;
                    case Action.WatchGame:
                        RenderGame();
                        break;
                    case Action.AskPlayAgain:
                        if (this.InvokeRequired)          
                            this.Invoke(() => { UiDrawer.RenderAskToPlayAgain(this,bool.Parse(response.parameters["won"]), response.parameters["word"]); });
                        else
                            UiDrawer.RenderAskToPlayAgain(this,bool.Parse(response.parameters["won"]), response.parameters["word"]);
                        //UiDrawer.yesButton.Click += playAgainButtonClick;
                        //UiDrawer.noButton.Click += playAgainButtonClick;
                        break;
                }                
            }
        }
        private void RenderLobby()
        {
            if (this.InvokeRequired)
                this.Invoke(() => { UiDrawer.DrawLobby(this, ProcessRooms(response.parameters["rooms"])); });
            else
                UiDrawer.DrawLobby(this, ProcessRooms(response.parameters["rooms"]));
            UiDrawer.createRoom.Click += createRoomButtonClick;
            for (int i = 0; i < UiDrawer.joinRoom.Length; i++)
            {
                UiDrawer.joinRoom[i].Click += joinRoomButtonClick;
                UiDrawer.watchRoom[i].Click += watchRoomButtonClick;
            }
        }
        private void RenderGame()
        {
            string word = response.parameters["word"];
            string guessedChars = response.parameters["guessedChars"];
            string turnName = response.parameters["turnName"];
            int turnId = int.Parse(response.parameters["turnId"]);
            bool watcher = bool.Parse(response.parameters["watcher"]);
            string category = response.parameters["category"];
            if (this.InvokeRequired)
                this.Invoke(() => { UiDrawer.DrawGame(this, category, word, guessedChars,turnName, turnId, watcher); });
            else
                UiDrawer.DrawGame(this, category, word, guessedChars,turnName, turnId, false);
            foreach (var key in UiDrawer.keyboard)
            {
                key.Click += keyClick;
            }
            if(watcher)
            {
                UiDrawer.leaveButton.Click += leaveButtonClick;
            }
        }
        private void checkJoinRquest()
        {
            if (response.parameters["roomId"] == "-1")
            {
                roomId = 0;
                UiDrawer.JoinDialog.ControlBox = true;
                UiDrawer.JoinDialog.MinimizeBox = false;
                UiDrawer.JoinDialog.MaximizeBox = false;
                UiDrawer.JoinDialog.Controls[0].Text = "Player Refused";
                //UiDrawer.JoinDialog.Controls.RemoveAt(1);
            }
            else
            {
                UiDrawer.JoinDialog.Close();
                Controls.Clear();
                roomId = int.Parse(response.parameters["roomId"]);
                secondPlayerName = response.parameters["playerName"];
                secondPlayerId =int.Parse(response.parameters["playerId"]);
                UiDrawer.DrawRoom(this, response.parameters["categories"].Split(","));
            }
        }      
        private void RenderRoom()
        {
            //MessageBox.Show(response.parameters["categories"]);
            if (this.InvokeRequired)
                this.Invoke(() => { UiDrawer.DrawRoom(this, response.parameters["categories"].Split(",")); });
            else
                UiDrawer.DrawRoom(this, response.parameters["categories"].Split(","));
            UiDrawer.startGame.Click += startGameClick; 
        }
        #endregion
        private string[][] ProcessRooms(string rooms)
        {
            if (rooms=="")
            {                 
                return new string[0][];   
            }
            string[] noOfRoom = rooms.Split(",,");
            string[][] RoomsArr = new string[noOfRoom.Length][];
            for (int i = 0; i < noOfRoom.Length; i++)
            {
                RoomsArr[i] = noOfRoom[i].Split(",");
            }

            for (int i = 0; i < RoomsArr.Length; i++)
            {
                for (int j = 0; j < RoomsArr[i].Length; j++)
                {
                    RoomsArr[i][j] = RoomsArr[i][j].Replace("{", "");
                    RoomsArr[i][j] = RoomsArr[i][j].Replace("}", "");

                    
                }

            }
            return RoomsArr;
        }
    }
}
