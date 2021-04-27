using Controller;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpreadsheetGUI;
using SS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mockserver
{
    class Mockserver
    {
        string lastedit = null;
        private static Dictionary<long, SocketState> clientsdictionary;
        private List<AbstractSpreadsheet> spreadsheetlist = new List<AbstractSpreadsheet>();
        private SocketState clients = null;
        static void Main(string[] args)
        {
            Mockserver server = new Mockserver();
            server.StartServer();

            Stopwatch watch = new Stopwatch();

            while(true)
            {
                while(watch.ElapsedMilliseconds < 100000)
                {
                    watch.Restart();
                }
            }
        }

        public Mockserver()
        {
            clientsdictionary = new Dictionary<long, SocketState>();
        }

        private void StartServer()
        {
            Networking.StartServer(NewClientConnected, 1100);
            Console.WriteLine("Server is Running");
        }

        /// <summary>
        /// Splits all of the data that was recived and split them by new line. Return these string as a list
        /// and process it.
        /// </summary>
        /// <param name="state"></param>
        private List<string> ProcessMessage(SocketState state)
        {
            //Gets all of the data and split them based on new line
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            List<string> newMessages = new List<string>();
            string temp;

            // Loop until we have processed all messages.
            // We may have received more than one.
            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                if(p.Contains("\n"))
                {
                    temp = p.TrimEnd('\n');
                    newMessages.Add(temp);
                }
                else
                {
                    newMessages.Add(p);
                }

                

                // Remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);

            }
            return newMessages;
        }

        /// <summary>
        /// Method to be invoked by the networking library
        /// when a new client connects
        /// </summary>
        /// <param name="obj"></param>
        private void NewClientConnected(SocketState state)
        {
            clients = state;

            if (state.ErrorOccured)
                return;

            // change the state's network action to the 
            // receive handler so we can process data when something
            // happens on the network
            state.OnNetworkAction = ReceiveMessage;

            //Get more data
            Networking.GetData(state);
        }

        /// <summary>
        /// Method to be invoked by the networking library
        /// when a network action occurs
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            //A string builder that is used to append all of the json that need to be sent
            StringBuilder builder = new StringBuilder();
            builder.Append(state.ID);

            //// Remove the client if they aren't still connected
            //if (state.ErrorOccured)
            //{
            //    RemoveClient(state.ID);
            //    return;
            //}

            //A list that stores all of the data that was sent from the client
            List<string> list = ProcessMessage(clients);

            //A set used to store all of the disconnected clients
            HashSet<long> disconnectedClients = new HashSet<long>();

            //Gets the player's name from the list
            string name = list[0];

            //Remove the name from the list
            list.Remove(list[0]);

            System.Console.WriteLine("about to send id");

            //Send the startup info to the client. If the data cannot be sent then it will add them to a list of disconnected clients to be remove
            if (!Networking.Send(clients.TheSocket, builder.ToString()))
            {
                disconnectedClients.Add(state.ID);
            }

            System.Console.WriteLine("sent ID");

            ////loop through all of the disconnected clients and remove them from the world
            //foreach (long id in disconnectedClients)
            //{
            //    RemoveClient(id);
            //}
            //builder.Clear();


            //// Save the client state
            //// Need to lock here because clients can disconnect at any time
            //lock (clients)
            //{
            //    clients[state.ID] = state;
            //}

            Console.WriteLine("Client " + state.ID + " is connected");

            state.OnNetworkAction = ProcessInput;

            // Continue the event loop that receives messages from this client
            Networking.GetData(state);
        }
        /// <summary>
        /// Process all of the commands that was sent from the client and store them accordingly
        /// </summary>
        /// <param name="state"></param>
        private void ProcessInput(SocketState state)
        {
            //Gets a list of data that was sent from the client
            List<string> list = ProcessMessage(state);
            StringBuilder sb = new StringBuilder();

            //store the new spreadsheetname into a variable
            string spreadsheetname = list[0];

            //Remove the spreadsheetname from the list
            list.Remove(list[0]);

            //If the spreadsheet list is empty then create a new one
            if (!spreadsheetlist.Any())
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                spreadsheetlist.Add(sheet);

             //   SpreadsheetForm form = new SpreadsheetForm(new SpreadsheetController(), spreadsheetname);
            }
            else
            {
                StringBuilder stringbuilder = new StringBuilder();

                foreach(Spreadsheet s in spreadsheetlist)
                {
                    stringbuilder.Append(s + "\n");
                }
                sb.Append("\n");
                Networking.Send(clients.TheSocket, stringbuilder.ToString());
            }

            foreach (string p in list)
            {
                if (p != "")
                {
                    JObject jObj = JObject.Parse(p);
                    string deserializedCell = "";

                    if (p.Contains("editCell"))
                    {
                        deserializedCell = jObj["contents"].ToString();
                        lastedit = deserializedCell;
                    }
                    else if (p.Contains("revertCell"))
                    {
                        //deserializedCell = jObj["cellName"].ToString();
                        deserializedCell = "";

                    }
                    else if (p.Contains("selectCell"))
                    {
                        deserializedCell = jObj["cellName"].ToString();
                    }
                    else if (p.Contains("undo"))
                    {
                        deserializedCell = lastedit;
                    }

                    sb.Append(JsonConvert.SerializeObject(deserializedCell) + "\n");

                }
            }
            
            if(!Networking.Send(clients.TheSocket, sb.ToString()))
            {
                System.Console.WriteLine("Mockserver send error");
            }

            Networking.GetData(state);
        }
    }
}
