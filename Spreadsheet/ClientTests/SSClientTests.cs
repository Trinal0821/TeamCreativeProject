using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Controller;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ClientCommands;
using MockserverCommands;

namespace SSTests
{


    class SSClientTests
    {
        static SpreadsheetController controller;
        static ArrayList testResults;
        static ArrayList errors;
        static int testIndex;
        static void Main(string[] args)
        {
            //if (args.Length != 2)
            //{
            //    Console.Out.WriteLine("Invalid number of arguments!\nArguments should be an IP address followed by a username.");
            //    return;
            //}
            testResults = new ArrayList();

            // Setup controller
            controller = new SpreadsheetController();
            controller.Connected += EndConnectionTest;
            controller.testUpdate += ServerUpdate;
            controller.ssUpdateError += ErrorOccured;
            controller.Error += ErrorOccured;

            // Wait a few seconds for server
            {
                for (int i = 0; i < int.MaxValue; i++)
                { }
            }

            Console.Out.WriteLine("Starting Client Tests . . .\n");
            RunConnectionTest("127.0.0.1", "Tester"); // Run test on local server
            //RunTests();
            //PrintResults();
        }

        /// <summary>
        /// Tests a connection with a server.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="name"></param>
        private static void RunConnectionTest(string address, string name)
        {
            controller.Connect(address, name);
        }

        /// <summary>
        /// Checks names of available spreadsheets, to ensure all correct data was captured
        /// on client connection.
        /// </summary>
        /// <param name="ssNames"></param>
        private static void EndConnectionTest(string[] ssNames)
        {
            // TODO
            if (ssNames.Length == 0)
                testResults.Add(new KeyValuePair<bool, string>(true, ""));

            else
                testResults.Add(new KeyValuePair<bool, string>(false, "Spreadsheet names did not match expected output.\n" +
                    "Expected: \n" +
                    "Actual:   " + ssNames.ToString()));

            testIndex++;

            // Continue tests
            RunTests();
        }

        private static void ErrorOccured(string message)
        {
            errors.Add(message);
            //testIndex++;
            PrintResults();
        }

        /// <summary>
        /// Starts data retrieval from server.
        /// </summary>
        private static void RunTests()
        {
            // Starts tests on successful connection
            if (testResults.Count > 0 && ((KeyValuePair<bool, string>)testResults[0]).Key)
                controller.selectSpreadsheet("new_test");
            else
                Console.Out.WriteLine("Connection test failed. Unable to complete remaining tests.\n");
        }

        private static void Test1(string instruction)
        {
            if (int.TryParse(instruction, out int id))
            {
                // ID check
                if (id != 0)
                {
                    testResults.Add(new KeyValuePair<bool, string>(true, ""));
                    testIndex++;
                }
                else
                {
                    testResults.Add(new KeyValuePair<bool, string>(false, "Actual ID doens't match expected.\n" +
                        "Expected: 0\n" +
                        "Actual:   " + id));
                }
            }
            // Send initial selection
            controller.setCellName("A1");
            controller.ProcessInputs();

        }

        private static void Test2(string instruction)
        {
            //Check instruction has correct info
            JObject jObj = JObject.Parse(instruction);
            JToken cellupdate = jObj["cellUpdated"];
            JToken cellselected = jObj["cellSelected"];
            JToken disconnected = jObj["disconnected"];
            if (cellselected != null)
            {

                CellSelected select = JsonConvert.DeserializeObject<CellSelected>(instruction);

                if (select.cellName != "A1")
                {
                    testResults.Add(new KeyValuePair<bool, string>(false, "CellSelected[cellName] does not match expected.\n" +
                        "Expected: A1\n" +
                        "Actual:   " + select.cellName));

                    ServerUpdate("Finished");
                    return;
                }
                if (select.selector != 0)
                {
                    testResults.Add(new KeyValuePair<bool, string>(false, "CellSelected[selector] does not match expected.\n" +
                        "Expected: 0\n" +
                        "Actual:   " + select.selector));

                    ServerUpdate("Finished");
                    return;
                }

                if (select.selectorName != "Tester")
                {
                    testResults.Add(new KeyValuePair<bool, string>(false, "CellSelected[selectorName] does not match expected.\n" +
                        "Expected: Tester\n" +
                        "Actual:   " + select.selectorName));

                    ServerUpdate("Finished");
                    return;
                }

            }
            else
            {
                string instType = "invalid instruction";
                if (cellupdate != null)
                {
                    instType = "cellUpdated";
                }
                else if (disconnected != null)
                {
                    instType = "disconnected";
                }
                testResults.Add(new KeyValuePair<bool, string>(false, "Instruction does not match expected\n" +
                        "Expected: cellSelected\n" +
                        "Actual:   " + instType));

                ServerUpdate("Finished");
                return;
            }

            //Check if our client is added
            if (controller.getClientIDList().Count != 1)
            {
                testResults.Add(new KeyValuePair<bool, string>(false, "ClientIDList.Count does not match expected.\n" +
                        "Expected: 1\n" +
                        "Actual:   " + controller.getClientIDList().Count));

                ServerUpdate("Finished");
                return;
            }


            //Check if selection was updated
            if (controller.getClientSelection(0) == "A1")
            {
                testResults.Add(new KeyValuePair<bool, string>(true, ""));
                testIndex++;
            }
            else
            {
                testResults.Add(new KeyValuePair<bool, string>(false, "Client[0] selection does not match expected\n" +
                        "Expected: A1\n" +
                        "Actual:   " + controller.getClientSelection(0)));


                ServerUpdate("Finished");
                return;
            }
        }

        private static void ServerUpdate(string instruction)
        {

            switch (testIndex)
            {
                case 1:
                    Test1(instruction);
                    break;
                case 2:
                    Test2(instruction);
                    break;
                case 3:
                    //Test3(instruction);
                    break;
                    // TODO
                    // ... and so on
            }

            if (instruction == "Finished")
                PrintResults();
        }

        private static void PrintResults()
        {
            for (int i = 0; i < testResults.Count; i++)
            {
                KeyValuePair<bool, string> result = ((KeyValuePair<bool, string>)testResults[i]);

                if (i == 0)
                    Console.Out.Write("Connection Test : ");
                else
                    Console.Out.Write("Test " + i + ": ");

                if (result.Key)
                    Console.Out.WriteLine("Success!");
                else
                    Console.Out.WriteLine("Failure...\n" + result.Value);

                Console.Read();
            }

            Console.Out.WriteLine("Reported Errors:");
            foreach (string errMsg in errors)
            {
                Console.Out.WriteLine(errMsg);
            }
        }
    }
}
