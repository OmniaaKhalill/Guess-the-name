using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//k

namespace BackEnd
{
    enum Action
    {
        Connect,CreateRoom,UpdateLobby, RequestJoin,CancelJoin, ReviewJoin,AcceptJoin,JoinRoom,  StartGame, GuessCharacter,WatchGame, StopWatching,AskPlayAgain, ResponseToPlayAgain
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
        public  string SendMessage()
        {
            string temp = "";
            temp += action.ToString() + "$";
            foreach (var keyValues in parameters)
            {
                temp += keyValues.Key + "=" + keyValues.Value + "$";
            }

            return temp;
        }
        //"connect userName=hossam rooms=asfdfs"
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
    public partial class Server : Form
    {
        Data dataBase;
        IPEndPoint connection;
        TcpListener listener;      
        BackgroundWorker serverWorker;
        ListBox lb;
        public Server()
        {
            InitializeComponent();
            lb = new ListBox();
            lb.Width = this.Width;
            lb.Height=this.Height;
            Controls.Add(lb);
            dataBase = new Data();
            //start server
            connection = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 3000);
            listener = new TcpListener(connection);
            try
            {
                listener.Start();
            }
            catch
            {
                MessageBox.Show("only one server can run at a time");
                this.Close();
                Application.Exit();
            }
            //set up background Worker to accept players without blocking the ui
            serverWorker = new BackgroundWorker();
            serverWorker.WorkerSupportsCancellation = true;
            serverWorker.DoWork += listenForConnections;          
            serverWorker.RunWorkerAsync();

        }

        private void listenForConnections(object? sender, DoWorkEventArgs e)
        {          
            Player player;
            TcpClient tempClient;
            NetworkStream stream;
            string responseText;
            Message response;
            string requestText;
            Message request;
            byte[] buffer = new byte[1024];
            int bytes;
            //for every player Joining
            while (serverWorker.CancellationPending == false)
            {
                //background worker
                //construct player object
                tempClient = listener.AcceptTcpClient();               
                stream =tempClient.GetStream();
                bytes = stream.Read(buffer);
                requestText = Encoding.UTF8.GetString(buffer, 0, bytes);
                if(lb.InvokeRequired)
                    lb.Invoke(() => { lb.Items.Add("From Client:" + requestText); });
                else
                    lb.Items.Add("From Client:" + requestText);
                request = Message.InterpretMessage(requestText);
                player = new Player(dataBase.Players, request.parameters["userName"], tempClient);             
                //create thread and attach it to each player
                new Thread(() => HandlePlayer(player)).Start();
                //add player to database
                dataBase.Players.Add(player);
                //send rooms to player and his id back
                response = new Message();
                response.action = Action.Connect;                              
                response.parameters["rooms"] = dataBase.getRooms();
                response.parameters["id"] = player.Id.ToString();
                responseText = response.SendMessage();
                //lb.Items.Add("To Client:" + requestText);
                stream.Write(Encoding.UTF8.GetBytes(responseText));
            }
        }

        private void HandlePlayer(Player player)
        {
            NetworkStream playerStream = player.Client.GetStream();

            byte[] buffer = new byte[1024];
            int bytes;
            try
            {


                while ((bytes = playerStream.Read(buffer)) != 0)
                {

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                    //lb.Items.Add("from client:"+msg);
                    Message request = Message.InterpretMessage(msg);
                    Message response = new Message();
                    Room tempRoom;
                    switch (request.action)
                    {
                        // CreateRoom: serverRecieves() ,serverSends(int roomId,string[] categories) && send updateLobby to all idle players
                        case Action.CreateRoom:
                            tempRoom = player.CreateRoom(dataBase.Rooms);
                            dataBase.Rooms.Add(tempRoom);
                            response.action = Action.CreateRoom;
                            response.parameters["roomId"] = tempRoom.Id.ToString();
                            response.parameters["categories"] = String.Join(",", dataBase.categories);
                            // send updateLobby to all idle players
                            BroadcastLobby(request);
                            break;
                        // RequestJoin: serverRecieves[int room id],serverSends[]
                        // send ReviewJoin to owner of roomid and send player id as playerToJoin and his name
                        case Action.RequestJoin:
                            tempRoom = dataBase.Rooms.Find(room => request.parameters["roomId"] == room.Id.ToString());
                            tempRoom.PendingJoin = true;
                            BroadcastLobby(request);
                            Player roomOwner = tempRoom.RoomOwner;
                            request.action = Action.ReviewJoin;
                            request.parameters["playerId"] = player.Id.ToString();
                            request.parameters["playerName"] = player.Name;
                            roomOwner.Client.GetStream().Write(Encoding.UTF8.GetBytes(request.SendMessage()));
                            break;
                        case Action.CancelJoin:
                            tempRoom = dataBase.Rooms.Find(room => request.parameters["roomId"] == room.Id.ToString());
                            tempRoom.PendingJoin = false;
                            BroadcastLobby(request);
                            Player owner = tempRoom.RoomOwner;
                            request.action = Action.CreateRoom;
                            //request.parameters["playerId"] = player.Id.ToString();
                            //request.parameters["playerName"] = player.Name;
                            owner.Client.GetStream().Write(Encoding.UTF8.GetBytes(request.SendMessage()));
                            break;
                        case Action.AcceptJoin:
                            Player secondPlayer = dataBase.Players.Find(p => p.Id == int.Parse(request.parameters["playerId"]));
                            tempRoom = dataBase.Rooms.Find(r => r.Id == int.Parse(request.parameters["roomId"]));
                            request.action = Action.JoinRoom;
                            if (bool.Parse(request.parameters["accepted"]) == true)
                            {
                                
                                secondPlayer.State = PlayerState.SecondPlayer;
                                tempRoom.RoomGuest = secondPlayer;
                                request.parameters["roomId"] = tempRoom.Id.ToString();
                                request.parameters["categories"] = String.Join(",", dataBase.categories);
                                request.parameters["playerId"] = tempRoom.RoomOwner.Id.ToString();
                                request.parameters["playerName"] = tempRoom.RoomOwner.Name;
                                secondPlayer.Client.GetStream().Write(Encoding.UTF8.GetBytes(request.SendMessage()));
                                request.action = Action.UpdateLobby;
                                BroadcastLobby(request);
                            }
                            else
                            {
                                tempRoom.PendingJoin = false;
                                request.parameters["roomId"] = "-1";
                                secondPlayer.Client.GetStream().Write(Encoding.UTF8.GetBytes(request.SendMessage()));
                                BroadcastLobby(request);
                            }
                            break;
                        case Action.StartGame:
                            tempRoom = dataBase.Rooms.Find(r => r.Id == int.Parse(request.parameters["roomId"]));
                            tempRoom.Selectedcategory = request.parameters["category"];
                            string word = dataBase.GetRandomWord(request.parameters["category"]);                          
                            tempRoom.CreateGame(word);
                            response.action = Action.StartGame;                            
                            response.parameters["word"] = new string(tempRoom.Game.CorrectChars);
                            response.parameters["turnName"]= player.Name;
                            response.parameters["turnId"] = player.Id.ToString();
                            response.parameters["guessedChars"]= string.Empty;
                            response.parameters["watcher"] = false.ToString();
                            response.parameters["category"] = tempRoom.Selectedcategory;
                            tempRoom.RoomGuest.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                            BroadcastLobby(request);
                            break;
                        case Action.GuessCharacter:
                            char guessedChar = char.Parse(request.parameters["key"]);
                            
                            tempRoom = dataBase.Rooms.Find(r => r.Id == int.Parse(request.parameters["roomId"]));
                            response.action = Action.GuessCharacter;
                            bool gameRunning= tempRoom.Game.ProcessGuess(guessedChar);



                            response.parameters["category"] = tempRoom.Selectedcategory;
                            response.parameters["watcher"]= false.ToString();
                            response.parameters["word"] = new string(tempRoom.Game.CorrectChars);
                            response.parameters["guessedChars"]=new string(tempRoom.Game.GuessedChars);
                            if(tempRoom.Game.gameState == GameState.FirstPlayerTurn)
                            {
                                response.parameters["turnName"] = tempRoom.RoomOwner.Name;
                                response.parameters["turnId"] = tempRoom.RoomOwner.Id.ToString();
                            }
                            else
                            {
                                response.parameters["turnName"] = tempRoom.RoomGuest.Name;
                                response.parameters["turnId"] = tempRoom.RoomGuest.Id.ToString();
                            }
                            if(player.Id==tempRoom.RoomOwner.Id)
                                tempRoom.RoomGuest.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                            else
                                tempRoom.RoomOwner.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));

                            BroadcastGame(response, tempRoom);
                            if (gameRunning == false)
                            {
                                response.action = Action.AskPlayAgain;
                                response.parameters["word"] = tempRoom.Game.Word;
                                response.parameters["won"] = false.ToString();
                                if (player.Id == tempRoom.RoomOwner.Id)
                                    tempRoom.RoomGuest.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                                else
                                    tempRoom.RoomOwner.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                                response.parameters["won"] = true.ToString();
                            }
                            break;
                        case Action.WatchGame:
                            player.State = PlayerState.Watcher;
                            tempRoom= dataBase.Rooms.Find(r => r.Id == int.Parse(request.parameters["roomId"]));
                            tempRoom.Watchers.Add(player);
                            response.action=Action.WatchGame;
                            response.parameters["category"] = tempRoom.Selectedcategory;
                            response.parameters["watcher"] = true.ToString();
                            response.parameters["word"] = new string(tempRoom.Game.CorrectChars);
                            response.parameters["guessedChars"] = new string(tempRoom.Game.GuessedChars);
                            response.parameters["turnName"] = player.Name;
                            response.parameters["turnId"] = player.Id.ToString();
                            if (tempRoom.Game.gameState == GameState.FirstPlayerTurn)
                            {
                                response.parameters["turnName"] = tempRoom.RoomOwner.Name;
                                response.parameters["turnId"] = tempRoom.RoomOwner.Id.ToString();
                            }
                            else
                            {
                                response.parameters["turnName"] = tempRoom.RoomGuest.Name;
                                response.parameters["turnId"] = tempRoom.RoomGuest.Id.ToString();
                            }
                            break;
                        case Action.ResponseToPlayAgain:
                            tempRoom = dataBase.Rooms.Find(r => r.Id == int.Parse(request.parameters["roomId"]));
                            bool isAccepted = bool.Parse(request.parameters["isAccepted"]);
                            Player player2;
                            if (player.State == PlayerState.FirstPlayer)
                                player2 = tempRoom.RoomGuest;
                            else
                                player2 = tempRoom.RoomOwner;
                            player.PlayAgain = isAccepted ? PlayAgain.Accepted : PlayAgain.Refused;
                            if (isAccepted)
                            {
                                if (player2.PlayAgain == PlayAgain.Accepted)
                                {
                                    player.PlayAgain = PlayAgain.None;
                                    player2.PlayAgain = PlayAgain.None;
                                    string newWord = dataBase.GetRandomWord(tempRoom.Selectedcategory);
                                    tempRoom.CreateGame(newWord); response.action = Action.StartGame;
                                    response.parameters["category"] = tempRoom.Selectedcategory;
                                    response.parameters["word"] = new string(tempRoom.Game.CorrectChars);
                                    response.parameters["turnName"] = tempRoom.RoomOwner.Name;
                                    response.parameters["turnId"] = tempRoom.RoomOwner.Id.ToString();
                                    response.parameters["guessedChars"] = string.Empty;
                                    response.parameters["watcher"] = false.ToString();
                                    player2.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                                }
                            }

                            else
                            {
                                string filePath = "logs.txt";



                                string playerInfo;
                                if (tempRoom.Game.gameState == GameState.FirstPlayerTurn )
                                {
                                    playerInfo = $"{tempRoom.RoomOwner.Name} \"won\", {tempRoom.RoomGuest.Name} \"lose\"";
                                }
                                else
                                {
                                    playerInfo = $"{tempRoom.RoomGuest.Name} \"won\", {tempRoom.RoomOwner.Name} \"lose\"";
                                }
                                File.AppendAllText(filePath, playerInfo+Environment.NewLine);
                                



                                tempRoom.GameRunning = false;
                                tempRoom.PendingJoin = false;
                                
                                
                                //send create room to player
                                // send update lobby to player2

                                //player2 guest  // player owner 

                                if (player == tempRoom.RoomOwner)
                                {

                                    response.action = Action.UpdateLobby;
                                    
                                    player2.State = PlayerState.Idle;
                                    tempRoom.RoomGuest = null;
                                    response.parameters["rooms"] = dataBase.getRooms();
                                    player2.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                                    response.action = Action.CreateRoom;
                                    response.parameters["categories"] = String.Join(",", dataBase.categories);
                                    response.parameters["roomId"] = tempRoom.Id.ToString();
                                }

                                //send create room to player2
                                // send update lobby to player

                                //player2 owner  // player guest 
                                else //guest
                                {
                                    response.action = Action.CreateRoom;
                                    response.parameters["categories"] = String.Join(",", dataBase.categories);
                                    response.parameters["roomId"] = tempRoom.Id.ToString();
                                    player.State = PlayerState.Idle;
                                    player2.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                                    response.action = Action.UpdateLobby;
                                    tempRoom.RoomGuest = null;
                                    response.parameters["rooms"] = dataBase.getRooms();
                                }
                                foreach (var p in tempRoom.Watchers)
                                {
                                    p.State = PlayerState.Idle;
                                }
                                BroadcastLobby(request);
                                tempRoom.Watchers.Clear();

                            }
                            break;
                        case Action.StopWatching:
                            tempRoom = dataBase.Rooms.Find(r => r.Id == int.Parse(request.parameters["roomId"]));
                            tempRoom.Watchers.Remove(player);
                            player.State = PlayerState.Idle;
                            response.action = Action.UpdateLobby;
                            response.parameters["rooms"] = dataBase.getRooms();                           
                            break;
                    }
                    playerStream.Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                    //lb.Items.Add("to Client:" + response.SendMessage());
                }
            }
            catch            
            {
                //switch (player.State)
                //{
                //    case PlayerState.Idle:
                //        break;
                //    case PlayerState.FirstPlayer:
                //        if (player == PlayerState.Idle)
                //}
                
                dataBase.Players.Remove(player);
                if (player.State==PlayerState.FirstPlayer)
                {
                    for (int i = 0; i < dataBase.Rooms.Count; i++) 
                    {
                        if (dataBase.Rooms[i].RoomOwner.Id==player.Id)
                        {
                            dataBase.Rooms[i].GameRunning = false;
                            if (dataBase.Rooms[i].RoomGuest!=null)
                                dataBase.Rooms[i].RoomGuest.State = PlayerState.Idle;                                             
                            foreach (var p in dataBase.Rooms[i].Watchers)
                                p.State = PlayerState.Idle;                    
                            dataBase.Rooms[i].Watchers.Clear();
                            dataBase.Rooms.RemoveAt(i);
                            BroadcastLobby(new Message());
                        }
                    }
                    
                }
                else if (player.State == PlayerState.SecondPlayer)
                {
                    Message response = new Message();
                    for (int i = 0; i < dataBase.Rooms.Count; i++)
                    {
                        if (dataBase.Rooms[i].RoomGuest.Id == player.Id)
                        {
                            dataBase.Rooms[i].PendingJoin = false;
                            dataBase.Rooms[i].GameRunning = false;
                            dataBase.Rooms[i].RoomGuest = null;
                            response.action = Action.CreateRoom;
                            response.parameters["categories"] = String.Join(",", dataBase.categories);
                            response.parameters["roomId"] = dataBase.Rooms[i].Id.ToString();
                            dataBase.Rooms[i].RoomOwner.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
                            foreach (var p in dataBase.Rooms[i].Watchers)
                            {
                                p.State = PlayerState.Idle;
                            }
                            dataBase.Rooms[i].Watchers.Clear();
                            BroadcastLobby(new Message());
                        }
                    }
                }
                
            }
            
            
        }
        private void BroadcastLobby(Message request)
        {
            List<Player> players = dataBase.Players.FindAll(player => player.State == PlayerState.Idle);
            request.action = Action.UpdateLobby;
            request.parameters["rooms"] = dataBase.getRooms();
            foreach (Player player in players)
            {
                player.Client.GetStream().Write(Encoding.UTF8.GetBytes(request.SendMessage()));
            }
            
        }
        private void BroadcastGame(Message response,Room room)
        {
            response.parameters["watcher"]=true.ToString();
            List<Player> players = room.Watchers;
            foreach (Player player in players)
            {
                player.Client.GetStream().Write(Encoding.UTF8.GetBytes(response.SendMessage()));
            }
            response.parameters["watcher"] = false.ToString();
        }
    }
}
