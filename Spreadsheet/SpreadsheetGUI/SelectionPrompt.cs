﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Controller;

namespace SpreadsheetGUI
{
    public partial class SelectionPrompt : Form
    {
        public string selection = "";
        public bool errorOccured = false;
        SpreadsheetController controller;

        public SelectionPrompt(SpreadsheetController ssCtrl, string IPaddress, string userName)
        {
            controller = ssCtrl;
            controller.Connected += OnConnect;
            controller.Error += ShowError;
            InitializeComponent();

            this.userInputTextBox.Enabled = false;
            controller.Connect(IPaddress, userName);
        }

        private void OnConnect(string[] ssNames)
        {
            //try
            // {
            //  MethodInvoker invalidator = new MethodInvoker(() => this.Invalidate(true));
            //this.Invoke(invalidator);

            MethodInvoker invalidator = delegate
            {
                foreach (string s in ssNames)
                SpreadsheetNamesLabel.Text = SpreadsheetNamesLabel.Text + s;
            userInputTextBox.Enabled = true;
             };

            BeginInvoke(invalidator);
        }

        private void ShowError(string errorMsg)
        {
            errorOccured = true;
            MessageBox.Show(errorMsg);
        }

        private void SelectionButton_Click(object sender, EventArgs e)
        {
            SelectionButton.Enabled = false;
            userInputTextBox.Enabled = false;

            if(userInputTextBox.Text.Contains("\n"))
            {
                MessageBox.Show("Invalid name, try again");
                SelectionButton.Enabled = true;
                userInputTextBox.Enabled = true;
                return;
            }

            selection = userInputTextBox.Text;
            this.Close();
        }

        private void userInputTextBox_TextChanged(object sender, EventArgs e)
        {
            if (userInputTextBox.TextLength > 0)
                SelectionButton.Enabled = true;
            else
                SelectionButton.Enabled = false;
        }
    }
}
