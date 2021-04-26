/*
  Main class for running the spreadsheet server through socket connections.
 */
//socket class
#include <sys/socket.h>
#include <iostream>
#include <netinet/in.h>
#include <vector>
#include <json/value.h>
#include "clientObject.cpp"

#define PORT 1100






string openSpreadsheet(ClientObject client, std::string spreadsheet){
	//Check if spreadsheet exists
	
	//If it does, send the data to the asking client
	//If it doesn't, create it
	
	//If there is an error, we will return a string containing the error
	return null;
}

string updateCell(string spreadsheet, string cell, string update){
	//Update the cell in the spreadsheet
	//Push this onto the "undo" stack
	
	//Broadcast to everyone
	
	//Return any errors
	return null;
}

string highlightUpdate(string spreadsheet, string username){
	//Broadcast the highlight to everyone
	return null;
}

void undo(string spreadsheet){
	//Pop from the undo stack
	//Broadcast to everyone
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
			else if(/*"User" in json*/){
				currentClient->clientName = //User field
			}
			else if(/*Spreadsheet*/){
				//Open and create spreadsheet
				//Send data to user
				string result = openSpreadsheet(currentClient, /*Get spreadsheet from json*/);
			}
			else if(/*CellUpdate*/){
				//Update cell in spreadsheet
				string result = updateCell(workingSpreadsheet, string cell, string update);//Get these from json
			}
			else if(/*HighlightUpdate*/){
				//Open and create spreadsheet
				highlightUpdate(workingSpreadsheet, currentClient->clientName); //Get these from json
			}
			else if(/*Undo*/){
				//Open and create 
				undo(workingSpreadsheet){
			}
			else if(/*Revert*/){
				//Open and create spreadsheet
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
