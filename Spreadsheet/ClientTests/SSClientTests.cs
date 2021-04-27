using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Controller;

namespace SSTests
{
    

    class SSClientTests
    {
        static SpreadsheetController controller;
        static ArrayList testResults;
        static string update;
        static int testIndex;
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Out.WriteLine("Invalid number of arguments!\nArguments should be an IP address followed by a username.");
                return;
            }
            testResults = new ArrayList();

            // Setup controller
            controller = new SpreadsheetController();
            controller.Connected += EndConnectionTest;
            controller.testUpdate += ServerUpdate;
            controller.ssUpdateError += ErrorOccured;
            controller.Error += ErrorOccured;

            Console.Out.WriteLine("Starting Client Tests . . .\n");
            RunConnectionTest(args[0], args[1]); // args[0]: IP address, args[1]: name
            RunTests();
            PrintResults();
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
            if (ssNames[0] == "some spreadsheet name" /* && ... */)
                testResults.Add(new KeyValuePair<bool, string>(true, ""));

            else
                testResults.Add(new KeyValuePair<bool, string>(false, "Spreadsheet names did not match expected output.\n" +
                    "Expected: \"some spreadsheet name\", etc...\n" +
                    "Actual:   " + ssNames.ToString()));

            testIndex++;
        }

        private static void ErrorOccured(string message)
        {
            testResults.Add(new KeyValuePair<bool, string>(false, message));
            testIndex++;
        }

        /// <summary>
        /// Starts data retrieval from server.
        /// </summary>
        private static void RunTests()
        {
            // Starts tests on successful connection
            if (testResults.Count > 0 && ((KeyValuePair<bool, string>)testResults[0]).Key)
                controller.selectSpreadsheet("some spreadsheet name");
            else
                Console.Out.WriteLine("Connection test failed. Unable to complete remaining tests.\n");
        }

        private static void Test1()
        {
            if (((KeyValuePair<bool, string>)testResults[0]).Key)
            {

                if (int.TryParse(update, out int id))
                {
                    if (id != 1987)
                    {
                        testResults.Add(new KeyValuePair<bool, string>(true, ""));
                        testIndex++;
                    }
                    else
                    {
                        testResults.Add(new KeyValuePair<bool, string>(false, "Actual ID doens't match expected.\n" +
                            "Expected: 1987\n" +
                            "Actual:   " + id));
                    }
                }
            }
        }

        private static void ServerUpdate(string instruction)
        {
            update = instruction;

            switch(testIndex)
            {
                case 1:
                    Test1();
                    break;
                case 2:
                    break;
                case 3:
                    break;
                // TODO
                // ... and so on
            }
        }

        private static void PrintResults()
        {
            for(int i = 0; i < testResults.Count; i++)
            {
                KeyValuePair<bool, string> result = ((KeyValuePair<bool, string>)testResults[i]);

                if(i == 0)
                    Console.Out.Write("Connection Test : ");
                else
                    Console.Out.Write("Test " + i + ": ");

                if (result.Key)
                    Console.Out.WriteLine("Success!");
                else
                    Console.Out.WriteLine("Failure...\n" + result.Value);

                Console.Read();
            }
        }
    }
}
