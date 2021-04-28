#include <algorithm>
#include <cstdlib>
#include <deque>
#include <iostream>
#include <list>
#include <set>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>

class spreadsheet_instance
{
public:

	/**
		Creates an instance of the spreadsheet
	*/
	spreadsheet_instance(std::string filename){
		std::list<std::string> fileList;
    std::string path = "Spreadsheet";
    for (const auto & entry : std::filesystem::directory_iterator(path))
        fileList.push_back(entry.path());

		*SpreadsheetObject foundSheet = null;
		for(sheet = fileList.begin; sheet!=fileList.end(); ++sheet){
			if(sheet == filename){
				foundSheet=&sheet;
				break;
			}
		}

		//If it doesn't exist, create it
		if(foundSheet==null){
			ofstream spreadsheetFile("Spreadsheet/"+filename+".xls");
			foundSheet = new Spreadsheet(spreadsheet)
			allSpreadsheets.push_back(foundSheet);
			spreadsheetStrings.push_back(spreadsheet);
		}

		//TODO: Send the data to the client

		//If there is an error, we will return a string containing the error
		return null;
	}




	/*
	* Assigns the client to the instance of the server, for output purposes
	*/
	void join(client_ptr client)
	{
		clients_.insert(client);
		client.deliver();
	}




	/*
	* Remove a client from the list of connected clients
	*/
	void leave(client_ptr client)
	{
		clients_.erase(client);
	}




	/*
	* Send a message to all clients
	*/
	void deliver(const std::string& msg)
	{
		std::for_each(participants_.begin(), participants_.end(),
			boost::bind(&chat_participant::deliver, _1, boost::ref(msg)));
	}




	/*
		Undo a cell action
	*/
	void undo(){
		ActionNode removingNode = actions.pop();
		for(node = actions.begin; node!=actions.end(); ++node){
			if(removingNode.cell = node.cell){
				addCell(removingNode.cell, removingNode.value);
				undone.push_back(new ActionNode(removingNode.cell, removingNode.value));
				return;
			}
		}
		addCell("", removingNode.value);
		undone.push_back(new ActionNode(removingNode.cell, removingNode.value));
	}




	/*
		Redo a cell action
	*/
	void redo(){
		ActionNode newNode = undone.pop();
		addCell(newNode.cell, newNode.value);
	}





	/*
		TODO: Update a cell in the file
	*/
	void addCell(std::string cell, std::string value){

		actions.push_back(new ActionNode(cell, value));
		std::string message = "{\"messageType\":\"editCell\", \"cellName\": \""+cell+"\",\"contents\":\""+update+"\"}";
		broadcastToAll(message);
	}





	/*
		TODO: Broadcast a message to all the users
	*/
	void broadcastToAll(std::string message){

	}


private:
	std::set<client_ptr> clients_;
	enum { max_recent_msgs = 100 };
	message_queue messages_;
	list<ActionNode> actions;
	list<ActionNode> undone;
};

typedef std::deque<chat_message> message_queue;
