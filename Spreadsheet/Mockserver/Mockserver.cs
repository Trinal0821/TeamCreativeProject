using ClientCommands;
using Controller;
using MockserverCommands;
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
using SpreadsheetUtilities;

namespace Mockserver
{
    class Mockserver
    {
        string lastedit = null;
        string lastCellName = null;
        private static Dictionary<SocketState, KeyValuePair<string, string>> clientsdictionary; // KeyValuePair<string clientName, ssName(ss connection)>
        // private List<AbstractSpreadsheet> spreadsheetlist = new List<AbstractSpreadsheet>();
        private Dictionary<string, Spreadsheet> spreadsheetList = new Dictionary<string, Spreadsheet>();
        private Dictionary<Spreadsheet, Dictionary<int, KeyValuePair<string, string>>> selectionList =
            new Dictionary<Spreadsheet, Dictionary<int, KeyValuePair<string,string>>>();
        // ^^Note: Dictionary<int selector, KeyValuePair<string cellName, string selectorName>>
        private SocketState clients = null;
        private string name;
        private StringBuilder sb = new StringBuilder();

        static void Main(string[] args)
        {
            Mockserver server = new Mockserver();
            server.StartServer();

            Stopwatch watch = new Stopwatch();

            while (true)
            {
                while (watch.ElapsedMilliseconds < 100)
                {
                    watch.Restart();
                }
            }
        }

        public Mockserver()
        {
            clientsdictionary = new Dictionary<SocketState, KeyValuePair<string, string>>();
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
            ////Gets all of the data and split them based on new line
            ////  sb.Append(state.GetData());
            //string totalData = state.GetData();
            //string temp2 = totalData.Replace(@"\", "") ;


            ////string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            //string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            //List<string> newMessages = new List<string>();
            //string temp;

            //// Loop until we have processed all messages.
            //// We may have received more than one.
            //foreach (string p in parts)
            //{
            //    // Ignore empty strings added by the regex splitter
            //    if (p.Length == 0)
            //        continue;

            //    // The regex splitter will include the last string even if it doesn't end with a '\n',
            //    // So we need to ignore it if this happens. 
            //    if (p[p.Length - 1] != '\n')
            //        break;

            //   // sb.Append(newMessages);


            //    if (p.Contains("\n"))
            //    {
            //      //  temp = p.TrimEnd('\n');
            //        //newMessages.Add(temp);
            //        //sb.Remove(0, p.Length);
            //        newMessages.Add(p);

            //        state.RemoveData(0, p.Length);

            //    }

            //    // Remove it from the SocketState's growable buffer


            //}
            //return newMessages;

            //Gets all of the data and split them based on new line
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            List<string> newMessages = new List<string>();

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

                newMessages.Add(p);

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

            clientsdictionary.Add(state, new KeyValuePair<string, string>());

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
            List<string> list = ProcessMessage(state);

            if (!state.ErrorOccured)
            {
                //A set used to store all of the disconnected clients
                HashSet<long> disconnectedClients = new HashSet<long>();

                //Gets the player's name from the list
                name = list[0];

                //Remove the name from the list
                list.Remove(list[0]);

                //Update clientdictionary
                clientsdictionary.Remove(state);
                clientsdictionary.Add(state, new KeyValuePair<string, string>(name, null));


                StringBuilder stringbuilder = new StringBuilder();

                if (spreadsheetList.Count != 0)
                {
                    foreach (string s in spreadsheetList.Keys)
                    {
                        stringbuilder.Append(s + "\n");
                    }
                    stringbuilder.Append("\n");
                }
                else
                {
                    stringbuilder.Append("\n\n");
                }

                Networking.Send(state.TheSocket, stringbuilder.ToString());

                state.OnNetworkAction = SendSpreadsheet;
                Networking.GetData(state);
            }
        }

        private string[] getCellRowAndColumn(string cellName)
        {
            List<int> rowAndColumn = new List<int>();

            //Source: https://stackoverflow.com/questions/3650118/how-to-split-a-string-on-numbers-and-it-substrings
            string[] output = Regex.Matches(cellName, "[0-9]+|[^0-9]+")
            .Cast<Match>()
            .Select(match => match.Value)
            .ToArray();

            return output;
        }


        private void SendSpreadsheet(SocketState state)
        {
            List<string> list = ProcessMessage(state);

            string spreadsheetName = list[0];
            list.Remove(list[0]);

            //Update clientdictionary
            if (clientsdictionary.TryGetValue(state, out KeyValuePair<string, string> client))
            {
                clientsdictionary.Remove(state);
                clientsdictionary.Add(state, new KeyValuePair<string, string>(client.Key, spreadsheetName));
            }

            if (spreadsheetList.TryGetValue(spreadsheetName, out Spreadsheet sheet))
            {
                //Create a stringbuilder
                StringBuilder sb = new StringBuilder();

                //go through and update each of the values in the cell
                IEnumerable<string> nonemptyCells = sheet.GetNamesOfAllNonemptyCells();

                foreach (string s in nonemptyCells)
                {
                    // Cell Updates
                    object val = sheet.GetCellContents(s);
                    CellUpdate update;
                    if (val is double)
                        update = new CellUpdate("cellUpdated", s, ((double) val).ToString());
                    else if (val is Formula)
                        update = new CellUpdate("cellUpdated", s, "=" +((Formula)val).ToString());
                    else
                        update = new CellUpdate("cellUpdated", s, (string)val);

                    sb.Append(JsonConvert.SerializeObject(update) + "\n");
                }


                if (selectionList.TryGetValue(sheet, out Dictionary<int, KeyValuePair<string,string>> selList))
                {
                    foreach(KeyValuePair<int, KeyValuePair<string, string>> sel in selList)
                    {
                        int selID = sel.Key;
                        string selCell = sel.Value.Key;
                        string selName = sel.Value.Value;

                        // Selection Updates
                        CellSelected selection = new CellSelected("cellSelected", selCell, selID, selName);
                        sb.Append(JsonConvert.SerializeObject(selection) + "\n");
                    }
                }

                Networking.Send(state.TheSocket, sb.ToString());
            }
            else
            {
                // Create a new spreadsheet
                spreadsheetList.Add(spreadsheetName, new Spreadsheet());
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(state.ID + "\n");
            System.Console.WriteLine("about to send id");

            //Send the startup info to the client. If the data cannot be sent then it will add them to a list of disconnected clients to be remove
            if (!Networking.Send(state.TheSocket, builder.ToString()))
            {
                //disconnectedClients.Add(state.ID);
                System.Console.WriteLine("Error with sending client ID");
            }

            System.Console.WriteLine("sent ID");

            clientsdictionary.TryGetValue(state, out KeyValuePair<string, string> clientName);
            Console.WriteLine("Client " + state.ID + " is connected. Name: " + clientName.Key);

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


            foreach (string p in list)
            {
                if (p != "")
                {
                    JObject jObj = JObject.Parse(p);
                    JToken edit = jObj["editCell"];
                    JToken select = jObj["selectCell"];
                    JToken revert = jObj["revertCell"];
                    JToken undo = jObj["undoCell"];

                    if (edit != null)
                    {
                        EditCell editcell = JsonConvert.DeserializeObject<EditCell>(p);

                        CellUpdate update = new CellUpdate("cellUpdated", editcell.cellName, editcell.contents);
                        sb.Append(JsonConvert.SerializeObject(update) + "\n");

                        clientsdictionary.TryGetValue(state, out KeyValuePair<string, string> clientInfo);
                        spreadsheetList.TryGetValue(clientInfo.Value, out Spreadsheet ss);
                        ss.SetContentsOfCell(editcell.cellName, editcell.contents);
                    }
                    else if (select != null)
                    {
                        SelectCell selectCell = JsonConvert.DeserializeObject<SelectCell>(p);

                        clientsdictionary.TryGetValue(state, out KeyValuePair<string,string> client);

                        CellSelected selected = new CellSelected("cellSelected", selectCell.cellName, (int)state.ID, client.Key);
                        sb.Append(JsonConvert.SerializeObject(selected) + "\n");

                        // Store seleciton info
                        spreadsheetList.TryGetValue(client.Value, out Spreadsheet ss);
                        if (selectionList.TryGetValue(ss, out Dictionary<int, KeyValuePair<string, string>> sel))
                        {
                            // Selection needs to be rewritten
                            if(sel.ContainsKey(selected.selector))
                            {
                                sel.Remove(selected.selector);
                            }
                            // Add Selection
                            sel.Add(selected.selector, new KeyValuePair<string, string>(selected.cellName, selected.selectorName));
                        }
                        // Dictionary needs to be added for the spreadsheet
                        else
                        {
                            KeyValuePair<string, string> selection = new KeyValuePair<string, string>(selected.cellName, selected.selectorName);
                            Dictionary<int, KeyValuePair<string, string>> newDict = new Dictionary<int, KeyValuePair<string, string>>();

                            newDict.Add(selected.selector, selection);
                            selectionList.Add(ss, newDict);
                        }
                    }
                    else if (revert != null)
                    {
                        RevertCell revertCell = JsonConvert.DeserializeObject<RevertCell>(p);
                    }
                    else if (undo != null)
                    {
                        UndoCell undoCell = JsonConvert.DeserializeObject<UndoCell>(p);
                    }
                    else
                    {
                        // Recieved invalid request

                        // Get client seleciton
                        string selection = "null";

                        clientsdictionary.TryGetValue(state, out KeyValuePair<string, string> client);
                        spreadsheetList.TryGetValue(client.Value, out Spreadsheet ss);
                        if (selectionList.TryGetValue(ss, out Dictionary<int, KeyValuePair<string, string>> sel))
                        {
                            sel.TryGetValue((int)state.ID, out KeyValuePair<string, string> selInfo);
                            selection = selInfo.Key;
                        }

                                RequestError error = new RequestError("requestError", selection, "Your request \"" + p + "\" is invalid.");
                        sb.Append(JsonConvert.SerializeObject(error) + "\n");
                    }
                }
            }
            // Send updates to each client
            List<SocketState> disconnectedClients = new List<SocketState>();
            foreach (SocketState s in clientsdictionary.Keys)
                if (!Networking.Send(s.TheSocket, sb.ToString()) || !s.TheSocket.Connected)
                {
                    System.Console.WriteLine("Client " + s.ID + " disconnected.");

                    disconnectedClients.Add(s);
                }
            foreach (SocketState d in disconnectedClients)
            {
                // Remove clients selections from spreadsheet dictionary
                clientsdictionary.TryGetValue(d, out KeyValuePair<string, string> clientInfo);
                spreadsheetList.TryGetValue(clientInfo.Value, out Spreadsheet ss);
                selectionList.TryGetValue(ss, out Dictionary<int, KeyValuePair<string, string>> sel);

                sel.Remove((int)d.ID);

                // Remove client
                clientsdictionary.Remove(d);
            }
            foreach (SocketState s in clientsdictionary.Keys)
            {
                foreach (SocketState d in disconnectedClients)
                {
                    Disconnected disconnect = new Disconnected("disconnected", (int)d.ID);

                    Networking.Send(s.TheSocket, JsonConvert.SerializeObject(disconnect) + "\n");
                }
            }
            if (!state.ErrorOccured)
                Networking.GetData(state);
        }
    }
}
