using SS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Controller;
using NetworkUtil;

namespace SpreadsheetGUI
{

    public delegate void TextBoxContentsChangedHandler(SpreadsheetPanel sender);


    public partial class SpreadsheetForm : Form
    {
        private SpreadsheetController controller;
        private bool error = false;

        // public delegate void UpdateSelection(string cellName);
        //public event UpdateSelection ssSelection;
        //public delegate void EditSelection(string cellContents);
        //public event EditSelection ssEdit;

        /// <summary>
        /// Creates a new window displaying an empty spreadsheet
        /// </summary>
        public SpreadsheetForm(SpreadsheetController ssCtrl, string ssName)
        {
            controller = ssCtrl;

            // Event listeners
            ssCtrl.Error += ShowError;
            ssCtrl.ssUpdate += UpdateSpreadsheet;
            ssCtrl.ssUpdateError += UpdateError;
            ssCtrl.ssConnected += InitialSelection;
            // ssCtrl.SelectionUpdate += OnSelectionChanged;


            // Client Spreadsheet

            // initialize form
            InitializeComponent(ssCtrl);

            // the name of the form
            this.Text = ssName;

            // highlights 
            this.ActiveControl = textBoxCellContents;

            // set up listener for panel selection changed
            spreadsheetPanel1.SelectionChanged += OnSelectionChanged;

            // set initial selection to A1, 1
            spreadsheetPanel1.SetSelection(0, 0);

            // Select Spreadsheet from server
            ssCtrl.selectSpreadsheet(ssName);
        }

        /// <summary>
        /// Handler for the controller's Error event
        /// </summary>
        private void ShowError(string errorMessage)
        {
            MessageBox.Show(errorMessage);
        }

        /// <summary>
        /// Updates the clients current selection, handles cell updates, and redraws
        /// the spreadsheet panel.
        /// </summary>
        private void UpdateSpreadsheet()
        {

            lock (controller)
            {
                // Update client's selection if necessary
                MethodInvoker updateSelection = delegate
                {
                    string newSel = controller.getClientSelection(controller.getThisID());
                    spreadsheetPanel1.GetSelection(out int col, out int row);
                    string crntSel = spreadsheetPanel1.ConvertCellName(col, row);

                    //if (newSel != crntSel)
                    {
                        spreadsheetPanel1.SetSelection(col, row);

                        
                        // Update name, value, and contents textBoxs
                        textBoxCellName.Text = (this.spreadsheetPanel1.ConvertCellName(col, row));

                        spreadsheetPanel1.GetValue(col, row, out string val);
                        textBoxCellValue.Text = val;

                        textBoxCellContents.Text = spreadsheetPanel1.GetContents(col, row);

                        // Focus the input onto the contents textbox
                        textBoxCellContents.Focus();

                    }
                };

                BeginInvoke(updateSelection);

                // Update the cell info from server
                MethodInvoker updateCell = delegate
                {
                    lock (controller.getCellsToUpdate())
                    {
                        List<KeyValuePair<string, string>> cellsToUpdate = controller.getCellsToUpdate();
                        List<KeyValuePair<string, string>> cellsToRemove = new List<KeyValuePair<string, string>>(cellsToUpdate);
                        foreach (KeyValuePair<string, string> cellUpdate in cellsToUpdate)
                        {
                            // Update the spreadsheet
                            spreadsheetPanel1.SetContents(cellUpdate.Key, cellUpdate.Value);

                            // Update the textbox
                            if (cellUpdate.Key == controller.getClientSelection(controller.getThisID()))
                            {
                                textBoxCellContents.Text = cellUpdate.Value;
                                spreadsheetPanel1.GetValue(cellUpdate.Key, out string cellValue);
                                textBoxCellValue.Text = cellValue;
                            }
                        }
                        foreach (KeyValuePair<string, string> updatedCell in cellsToRemove)
                            controller.cellUpdated(updatedCell);
                    }
                };

                BeginInvoke(updateCell);

                // Redrawn in case clients disconnect
                spreadsheetPanel1.Invalidate();
            }
        }

        /// <summary>
        /// Shows an error from the server/controller.
        /// </summary>
        /// <param name="message"></param>
        private void UpdateError(string message)
        {
            error = true;
            MessageBox.Show(message);
        }

        /// <summary>
        /// When the selection of a spreadsheet is changed, sends a message to the server to
        /// request an update.
        /// </summary>
        /// <param name="sender"></param>
        private void OnSelectionChanged(SpreadsheetPanel sender)
        {
            lock (controller)
            {
                // Get where we are in the spreadsheet
                sender.GetSelection(out int col, out int row);

                // Send selection changed command to server
                controller.setCellContents("");
                controller.setCellName(sender.ConvertCellName(col, row));
                controller.ProcessInputs();
            }
        }

        /// <summary>
        /// Starts up the first selection after client recieves ID.
        /// </summary>
        private void InitialSelection()
        {
            OnSelectionChanged(spreadsheetPanel1);
        }

        /// <summary>
        /// When a key is pressed in the contents box
        /// If it is an enter key, input that data as the contents of the cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCellContents_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                TextBox t = (TextBox)sender;
                string contents = t.Text.ToString();
                spreadsheetPanel1.GetSelection(out int col, out int row);

                lock (controller)
                {
                    controller.setCellContents(contents);
                    controller.setCellName(this.spreadsheetPanel1.ConvertCellName(col, row));

                    controller.ProcessInputs();
                }
                //spreadsheetPanel1.SetContents(col, row, contents);
                // Update();
                e.Handled = true;
            }
        }

        /// <summary>
        /// This method is called anytime the form closes
        /// checks to see if there is any unsaved progress,
        /// and prompts user accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //  private void SpreadsheetForm_FormClosing(object sender, FormClosingEventArgs e)
        private void SpreadsheetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Close the socket and let the server know that the client has been disconnected
            Networking.SendAndClose(controller.theServer.TheSocket, "");
        }

        /// <summary>
        /// Displays a dialogue box with information of how to operate a spreadsheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            string helpText = "This is a client spreadsheet. To edit a cell, click on it with the mouse. " +
                "The server you are connected to will send updates for your selections. You can then edit" +
                "the contents of the cell by typing in the contents and hitting enter. The value of the " +
                "cell will be shown as well as the contents of that cell.\n\nOther clients selections will" +
                " be visable to you, as will their display names. Updates to cell contents will occur " +
                "after the server processes them.\n\nFeatures: Undo, Redo\nTo undo your last action (i.e." +
                "your last edit) press the undo button. To revert a selected cell, press the revert button.";

            MessageBox.Show(helpText, "Help");
        }

        /// <summary>
        /// Handler for pressing the night mode button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripNightModeButton_Click(object sender, EventArgs e)
        {
            NightMode();
        }

        /// <summary>
        /// Enables/Disables Night Mode for this form.
        /// </summary>
        private void NightMode()
        {
            spreadsheetPanel1.NightMode(toolStripNightModeButton.Checked);

            if (toolStripNightModeButton.Checked)
            {
                labelCellContents.ForeColor = Color.Black;
                labelCellName.ForeColor = Color.Black;
                labelCellValue.ForeColor = Color.Black;
                this.BackColor = Color.LightGray;
                toolStripNightModeButton.Checked = false;
            }
            else
            {
                labelCellContents.ForeColor = Color.LightGray;
                labelCellName.ForeColor = Color.LightGray;
                labelCellValue.ForeColor = Color.LightGray;
                this.BackColor = ColorTranslator.FromHtml("#303030");
                toolStripNightModeButton.Checked = true;
            }
        }

        /// <summary>
        /// Handler for pressing the undo button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripUndoButton_Click(object sender, EventArgs e)
        {
            lock (controller)
            {
                controller.undoAction();
                controller.ProcessInputs();
            }
        }

        /// <summary>
        /// Handler for pressing the revert button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripRevertButton_Click(object sender, EventArgs e)
        {
            lock (controller)
            {
                controller.revertCell(controller.getClientSelection(controller.getThisID()));
                controller.ProcessInputs();
            }
        }
    }
}