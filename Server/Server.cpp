/*
  Main class for running the spreadsheet server through socket connections.
 */
//socket class
#include <sys/socket.h>
#include <iostream>
#include <netinet/in.h>
#include <vector>
#include <json/value.h>
#include <list>
#include "clientObject.cpp"
#include "SpreadsheetObject.cpp"

#define PORT 1100



std::list<SpreadsheetObject> allSpreadsheets;




/**********
Opens the spreadsheet and broadcasts the information to the requesting client.
If the spreadsheet doesn't exist, we create it. Returns a string if there is an error.
*************/
std::string openSpreadsheet(ClientObject client, std::string spreadsheet){
	//Check if spreadsheet exists
	
	//If it does, send the data to the asking client
	//If it doesn't, create it
	
	//If there is an error, we will return a string containing the error
	return null;
}







/*****************
Updates a cell in the spreadsheet, and broadcasts to all the users 
Returns a string if there is an error.
*****************/
std::string updateCell(*SpreadsheetObject spreadsheet, std::string cell, std::string update){
	//Update the cell in the spreadsheet
	spreadsheet->addCell(cell, update);
	
	std::string message = "{\"messageType\":\"editCell\", \"cellName\": \""+cell+"\",\"contents\":\""+update+"\"}";
	//Push this onto the "actions" stack
	spreadsheet->actions.push_back(message);
	
	//Broadcast to everyone
	std::string returnString = spreadsheet->broadcastToAll(message);
	
	//Return any errors
	return returnString;
}







/*****************
Updates a cell in the spreadsheet, and broadcasts to all the users 
Returns a string if there is an error.
*****************/
std::string highlightUpdate(*SpreadsheetObject spreadsheet, std::string cellName, int selectorId, std::string username){
	std::string message = "{\"messageType\":\"cellSelected\", \"cellName\": \""+cellName+"\", \"selector\": \""+selectorId+"\",\"selectorName\":\""+username+"\"}";
	
	//Broadcast to everyone
	std::string returnString = spreadsheet->broadcastToAll(message);
	
	//Return any errors
	return returnString;
}












/*****************
Updates a cell in the spreadsheet, and broadcasts to all the users 
Returns a string if there is an error.
*****************/
std::string broadcastToClient(int clientID, std::string message){
	//Broadcast to one person
}












int main()
{
  int server_socket;
  int result;
  int opt = 1;
  std::vector<clientObject> clients;
  
  //build address struct to specify server settings
  struct sockaddr_in address;
  address.sin_family = AF_INET;
  address.sin_addr.s_addr = INADDR_ANY;
  address.sin_port = htons(PORT);
  
  //get length of address
  int addrlen = sizeof(address);

  //create an streaming, IPv4 socket
  server_socket = socket(AF_INET, SOCK_STREAM, 0);
  //set options to remember the port and address
  setsockopt(server_socket, SOL_SOCKET, SO_REUSEPORT, &opt, sizeof(opt)); 
  setsockopt(server_socket, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt)); 
  bind(server_socket, (struct sockaddr *)&address, sizeof(address));

  //begin listening
  listen(server_socket, 5);
  
  //accept incoming client
  int client_socket = accept(server_socket, (struct sockaddr *)&address, (socklen_t*)&addrlen);

  clients.push_back(new ClientObject(client_socket));
  
  
    do {
        result = recv(server_socket, recvbuf, 6000, 0);
		
		
        if ( result > 0 ){
			//We successfully recieved the result
			//TODO: We need to interprit the JSON that comes in
			ClientObject currentClient = //Search for the client's data
			string workingSpreadsheet = currentClient->clientSpreadsheet;
			string messageType = //Get this from json
			if(/*"User" in json*/){
				currentClient->clientName = //User field
				//Send the spreadsheets to the user
			}
			else if(/*Spreadsheet*/){
				//Open and create spreadsheet
				//Send data to user
				string result = openSpreadsheet(currentClient, /*Get spreadsheet from json*/);
			}
			else if(requestType=="editCell"){
				//Update cell in spreadsheet
				string result = updateCell(workingSpreadsheet, cell, update);//Get these from json
			}
			else if(requestType=="selectCell"){
				//Open and create spreadsheet
				highlightUpdate(workingSpreadsheet, currentClient->clientName); //Get these from json
			}
			else if(requestType=="undo"){
				//Open and create 
				spreadsheet->undo();
			}
			else if(requestType=="revertCell"){
				//Open and create 
				spreadsheet->revert();
			}
			else{
				//Data is unreadable. Send a connectionerrror message
			}
		}
		
		
        else if ( result == 0 ){
			//The client closed the connection
		}
        else{
			//The recieve failed
		}

    } while( result > 0 );
  return 0;
}
