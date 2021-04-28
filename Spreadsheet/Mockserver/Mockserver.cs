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
        string lastCellName = null;
        private static Dictionary<SocketState, string> clientsdictionary;
        private List<AbstractSpreadsheet> spreadsheetlist = new List<AbstractSpreadsheet>();
        private SocketState clients = null;
        private string name;
        static void Main(string[] args)
        {
            Mockserver server = new Mockserver();
            server.StartServer();

            Stopwatch watch = new Stopwatch();

            while(true)
            {
                while(watch.ElapsedMilliseconds < 100)
                {
                    watch.Restart();
                }
            }
        }

        public Mockserver()
        {
            clientsdictionary = new Dictionary<SocketState, string>();
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

            clientsdictionary.Add(state, null);

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

            //A list that stores all of the data that was sent from the client
            List<string> list = ProcessMessage(clients);

            //A set used to store all of the disconnected clients
            HashSet<long> disconnectedClients = new HashSet<long>();

            //Gets the player's name from the list
             name = list[0];

            //Remove the name from the list
            list.Remove(list[0]);

            state.OnNetworkAction = SendSpreadsheets;

            // Continue the event loop that receives messages from this client
            Networking.GetData(state);
        }

        private void SendSpreadsheets(SocketState state)
        {
            List<string> list = ProcessMessage(clients);

            //store the new spreadsheetname into a variable
            if (list.Count > 0)
            {
                string spreadsheetname = list[0];

                //Remove the spreadsheetname from the list
                list.Remove(list[0]);
            }

            //If the spreadsheet list is empty then create a new one
            if (!spreadsheetlist.Any())
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                spreadsheetlist.Add(sheet);
            }
            else
            {
                StringBuilder stringbuilder = new StringBuilder();

                foreach (Spreadsheet s in spreadsheetlist)
                {
                    stringbuilder.Append(s + "\n");
                }
                stringbuilder.Append("\n");
                Networking.Send(clients.TheSocket, stringbuilder.ToString());
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(state.ID);
            System.Console.WriteLine("about to send id");

            //Send the startup info to the client. If the data cannot be sent then it will add them to a list of disconnected clients to be remove
            if (!Networking.Send(clients.TheSocket, builder.ToString()))
            {
                //disconnectedClients.Add(state.ID);
                System.Console.WriteLine("Error with sending client ID");
            }

            System.Console.WriteLine("sent ID");

            Console.WriteLine("Client " + state.ID + " is connected");

            state.OnNetworkAction = ProcessInput;

            // Continue the event loop that receives messages from this client
            Networking.GetData(state);
        }

        ///// <summary>
        ///// Sends the client ID after sending the spreadsheets
        ///// </summary>
        ///// <param name="state"></param>
        //private void SendID(SocketState state)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    builder.Append(state.ID);
        //    System.Console.WriteLine("about to send id");

        //    //Send the startup info to the client. If the data cannot be sent then it will add them to a list of disconnected clients to be remove
        //    if (!Networking.Send(clients.TheSocket, builder.ToString()))
        //    {
        //        //disconnectedClients.Add(state.ID);
        //        System.Console.WriteLine("Error with sending client ID");
        //    }

        //    System.Console.WriteLine("sent ID");

        //    Console.WriteLine("Client " + state.ID + " is connected");

        //    state.OnNetworkAction = ProcessInput;

        //    // Continue the event loop that receives messages from this client
        //    Networking.GetData(state);
        //}

        /// <summary>
        /// Process all of the commands that was sent from the client and store them accordingly
        /// </summary>
        /// <param name="state"></param>
        private void ProcessInput(SocketState state)
        {
            //Gets a list of data that was sent from the client
            List<string> list = ProcessMessage(state);
            StringBuilder sb = new StringBuilder();

            
            //FIX THIS
            foreach (string p in list)
            {
                if (p != "")
                {
                    JObject jObj = JObject.Parse(p);
                  //  string deserializedCell = "";

                    if (p.Contains("editCell"))
                    {
                       string deserializedCellContents = jObj["contents"].ToString();
                        string deserilizedCellName = jObj["cellName"].ToString();
                        lastedit = deserializedCellContents;
                        lastCellName = deserilizedCellName;

                        sb.Append(JsonConvert.SerializeObject("messageType: " + "cellUpdated" + "\n"));
                        sb.Append(JsonConvert.SerializeObject("cellName: " + deserilizedCellName + "\n"));
                        sb.Append(JsonConvert.SerializeObject("contents: " + deserializedCellContents + "\n"));
                    }
                    else if (p.Contains("revertCell"))
                    {
                        string deserilizedCellName = jObj["cellName"].ToString();

                        sb.Append(JsonConvert.SerializeObject("messageType: " + "cellUpdated" + "\n"));
                        sb.Append(JsonConvert.SerializeObject("cellName: " + deserilizedCellName + "\n"));
                        sb.Append(JsonConvert.SerializeObject("contents: " + "" + "\n"));

                        lastedit = "";
                        lastCellName = deserilizedCellName;

                    }
                    else if (p.Contains("selectCell"))
                    {
                        string deserializedCellName = jObj["cellName"].ToString();

                        foreach(SocketState s in clientsdictionary.Keys)
                        {
                            if(s.ID == state.ID)
                            {
                                //Set the location of where the client is
                                clientsdictionary[s] = deserializedCellName;

                                sb.Append(JsonConvert.SerializeObject("messageType: " + "cellSelected" + "\n"));
                                sb.Append(JsonConvert.SerializeObject("cellName: " + deserializedCellName + "\n"));
                                sb.Append(JsonConvert.SerializeObject("selector: " + s.ID + "\n"));

                                //NEED TO CHANGE THIS!! THIS IS TEMPORARY
                                sb.Append(JsonConvert.SerializeObject("selectorName" + "sam" + "\n"));
                            }
                        }
                    }
                    else if (p.Contains("undo"))
                    {
                        sb.Append(JsonConvert.SerializeObject("messageType: " + "cellUpdated" + "\n"));
                        sb.Append(JsonConvert.SerializeObject("cellName: " + lastCellName + "\n"));
                        sb.Append(JsonConvert.SerializeObject("contents: " + lastedit + "\n"));
                    }


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
