using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SS;

namespace Controller
{
    /// <summary>
    /// Mediates the passing of information between the GUI and the spreadsheet 
    /// server. 
    /// </summary>
    public class SpreadsheetController
    {
        // Controller events that the view can subscribe to
        public delegate void UpdateFromServer();
        public event UpdateFromServer ssUpdate;
        public delegate void UpdateError(string message);
        public event UpdateError ssUpdateError;
        // public delegate void SelectionChanged();
        //public event SelectionChanged SelectionUpdate;
        public delegate void ConnectedHandler(string[] ssNames);
        public event ConnectedHandler Connected;
        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        // For testing purposes
        public delegate void UpdateTest(string message);
        public event UpdateTest testUpdate;

        // private variables
        private String userName;
        private int clientID = int.MinValue;
        private StringBuilder jsonInfo;
        private Dictionary<int, KeyValuePair<string, string>> clientList =
            new Dictionary<int, KeyValuePair<string, string>>(); // <clientID, <cellName(position), clientName>
        private KeyValuePair<string, string> cellToUpdate =
            new KeyValuePair<string, string>("", ""); // <cellName, cellContents>

        // User input variables
        private string contents = "";
        private string cellName = "";
        private bool doUndo = false;
        private bool doRevert = false;

        // state representing the connection to the server
        public SocketState theServer = null;

        /// <summary>
        /// Atttemps to connect to a server from a given address.
        /// </summary>
        /// <param name="address">Address of the server to connect to </param>
        /// <param name="name">The user's input name</param>
        public void Connect(string address, string name)
        {
            // save the name of the client connecting
            userName = name;

            // Attempt to connect to the server
            Networking.ConnectToServer(OnConnect, address, 1100);
        }

        /// <summary>
        /// Callback for the connect method
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Error("Error connecting to server");
                state.OnNetworkAction = (SocketState) => { };
                return;
            }
            else
            {
                // set our server to this new connection
                theServer = state;

                // tell the server who we are
                Networking.Send(theServer.TheSocket, this.userName + "\n");

                // assign the network action event to our recieve data event
                state.OnNetworkAction = receiveStartUpData;

                // start receiving start up data
                jsonInfo = new StringBuilder();
                Networking.GetData(state);
            }


        }

        /// <summary>
        /// Callback for the OnConnect method
        /// Recieves initial data about spreadsheet names in the server
        /// </summary>
        /// <param name="state"></param>
        private void receiveStartUpData(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Error("Lost connection to server");
                return;
            }

            // get the Json information
            jsonInfo.Append(state.GetData());

            // Check if all spreadsheet name data has been gathered
            if (jsonInfo.ToString().Contains("\n\n"))
            {
                // split it into actual messages
                // send the spreadsheet names to the GUI
                Connected(Regex.Split(jsonInfo.ToString(), @"(?<=[\n])"));
            }
            // Continue gathering startup data
            else
            {
                Networking.GetData(state);
            }
        }

        /// <summary>
        /// Attepts to access a specifed spreadsheet from the server.
        /// </summary>
        /// <param name="ssName">Name of the selected spreadsheet</param>
        public void selectSpreadsheet(string ssName)
        {
            if (theServer == null)
            {
                Error("Must have connection to server to select spreadsheets. Make call to Connect first.");
                return;
            }
            SocketState state = theServer;
            state.OnNetworkAction = receiveUpdate;

            // Send the selected spreadsheet name
            Networking.Send(state.TheSocket, ssName + "\n");
        }

        /// <summary>
        /// Recieves an update from the server.
        /// Continues the update event loop.
        /// </summary>
        /// <param name="state"></param>
        private void receiveUpdate(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Error("Error while receiving update from server.");
                return;
            }

            string clientID = state.GetData();

            // remove the data we just processed from the state's buffer
            state.RemoveData(0, clientID.Length);

            processData(state);

            // Continue event loop
            Networking.GetData(state);
        }

        /// <summary>
        /// Parses the instructions recieved from the server
        /// </summary>
        /// <param name="state"></param>
        private void processData(SocketState state)
        {
            // get the Json information
            string jsonInfo = state.GetData();

            // split it into actual messages
            string[] updates = Regex.Split(jsonInfo, @"(?<=[\n])");

            foreach (string instruction in updates)
            {
                // Ignore empty strings from Regex Splitter
                if (instruction.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (instruction[instruction.Length - 1] != '\n')
                    break;

                UpdateSpreadsheet(instruction);

                // remove the data we just processed from the state's buffer
                state.RemoveData(0, instruction.Length);
            }

            //What user inputs happened during the last frame, process them
            ProcessInputs();

            Networking.GetData(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        private void UpdateSpreadsheet(string instruction)
        {
            if (clientID == int.MinValue)
            {
                // Assign client ID
                if (int.TryParse(instruction, out int ID))
                    clientID = ID;
            }

            JObject jObj = JObject.Parse(instruction);

            // string deserializedMessageType = JsonConvert.DeserializeObject<string>(instruction);

            string deserializeMessage = jObj["messageType"].ToString();
            if (instruction.Contains("cellUpdate"))
            {
                string deserializedName = jObj["cellName"].ToString();
                string deserializedcontents = jObj["contents"].ToString();

                cellToUpdate = new KeyValuePair<string, string>(deserializedName, deserializedcontents);
            }
            else if (instruction.Contains("cellSelected"))
            {
                string deserializedName = jObj["cellName"].ToString();
                string deserializedSelector = jObj["selector"].ToString();
                string deserializedSelectorName = jObj["selectorName"].ToString();

                //Add the client ID to the client list
                if (int.TryParse(instruction, out int ID))
                {
                    addClients(ID, new KeyValuePair<string, string>(deserializedName, deserializedSelectorName));
                }
            }
            else if (instruction.Contains("disconnected"))
            {
                string deserializedID = jObj["user"].ToString();
                if (int.TryParse(deserializedID, out int ID))
                {
                    removeClients(ID);
                }

            }
            else if (instruction.Contains("requestError"))
            {
                string deserializedMessage = jObj["message"].ToString();
                ssUpdateError(deserializedMessage);
            }
            else if (instruction.Contains("serverError"))
            {
                string deserializedMessage = jObj["message"].ToString();
                ssUpdateError(deserializedMessage);
            }

            if (ssUpdate != null)
            {
                ssUpdate();
            }
            if (testUpdate != null)
            {
                testUpdate(instruction);
            }
        }

        public void ProcessInputs()
        {
            StringBuilder sb = new StringBuilder();
            if (doUndo)
            {
                sb.Append(JsonConvert.SerializeObject("requestType:" + "undo" + "\n"));
                doUndo = false;
            }
            else if (doRevert)
            {

                sb.Append(JsonConvert.SerializeObject("requestType:" + "revertCell" + "\n"));
                sb.Append(JsonConvert.SerializeObject("cellName:" + cellName + "\n"));
                doUndo = false;
            }
            else if (!contents.Equals(""))
            {
                sb.Append(JsonConvert.SerializeObject("requestType:" + "editCell" + "\n"));
                sb.Append(JsonConvert.SerializeObject("cellName:" + cellName + "\n"));
                sb.Append(JsonConvert.SerializeObject("contents:" + contents + "\n"));
            }
            else if (!cellName.Equals(""))
            {
                sb.Append(JsonConvert.SerializeObject("requestType:" + "selectCell" + "\n"));
                sb.Append(JsonConvert.SerializeObject("cellName:" + cellName + "\n"));
            }

            Networking.Send(theServer.TheSocket, sb.ToString());
        }

        public int getThisID()
        {
            return this.clientID;
        }

        /// <summary>
        /// Adds/Updates the client info into the clientlist
        /// </summary>
        /// <param name="clientID"></param>
        private void addClients(int clientID, KeyValuePair<string, string> userInfo)
        {
            if (clientList.ContainsKey(clientID))
            {
                clientList.Remove(clientID);
            }

            clientList.Add(clientID, new KeyValuePair<string, string>(userInfo.Key, userInfo.Value));
        }

        /// <summary>
        /// Removes the client info from the clientlist
        /// </summary>
        /// <param name="clientID"></param>
        private void removeClients(int clientID)
        {
            clientList.Remove(clientID);
        }

        /// <summary>
        /// Get a list of clients that are currently connected to the server
        /// </summary>
        /// <returns></returns>
        public List<int> getClientIDList()
        {
            List<int> idList = new List<int>();
            foreach (int i in clientList.Keys)
                idList.Add(i);
            return idList;
        }

        /// <summary>
        /// Returns the selected cell for a specified client.
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public string getClientSelection(int clientID)
        {
            if (clientList.TryGetValue(clientID, out KeyValuePair<string, string> clientInfo))
                return clientInfo.Key;
            return null;
        }

        /// <summary>
        /// Set the cell Contents
        /// </summary>
        /// <param name="contents"></param>
        public void setCellContents(string contents)
        {
            this.contents = contents;
        }

        /// <summary>
        /// Set the cell name
        /// </summary>
        /// <param name="cellName"></param>
        public void setCellName(string cellName)
        {
            this.cellName = cellName;
        }

        public void undoAction()
        {
            doUndo = true;
        }

        public void revertCell(string cellName)
        {
            this.cellName = cellName;
            doRevert = true;
        }

        public KeyValuePair<string, string> getCellToUpdate()
        {
            return cellToUpdate;
        }

        public void cellUpdated()
        {
            cellToUpdate = new KeyValuePair<string, string>("", "");
        }
    }
}