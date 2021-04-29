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
		std::for_each(clients_.begin(), clients_.end(),
			boost::bind(&client::deliver, _1, boost::ref(msg)));
	}

	/*
		TODO: Undo a cell action
	*/
	void undo(){

	}

	/*
		TODO: Redo a cell action
	*/
	void redo(){

	}

	/*
		TODO: Update a cell in the file
	*/
	void addCell(std::string cell, std::string value){

	}


private:
	std::set<client_ptr> clients_;
	enum { max_recent_msgs = 100 };
	message_queue messages_;
};

typedef std::deque<chat_message> message_queue;
