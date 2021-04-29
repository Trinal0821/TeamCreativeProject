#include <algorithm>
#include <cstdlib>
#include <deque>
#include <iostream>
#include <ofstream>
#include <fstream>
#include <list>
#include <set>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include "rapidxml/rapidxml.hpp"
#include "actionNodes.cpp"

class spreadsheet_instance
{
public:

	/**
		Creates an instance of the spreadsheet
	*/
	spreadsheet_instance(std::string filename){
		this->filename="Spreadsheet/"+filename+".sprd";
		std::list<std::string> fileList;
    std::string path = "Spreadsheet";
    for (const auto & entry : std::filesystem::directory_iterator(path)){
        fileList.push_back(entry.path());
    }

		bool foundSheet = false;
		for(sheet = fileList.begin; sheet!=fileList.end(); ++sheet){
			if(sheet == filename){
				foundSheet=true;
				break;
			}
		}

		//If it doesn't exist, create it
		if(!foundSheet){
			ofstream spreadsheetFile("Spreadsheet/"+filename+".sprd");
			spreadsheetFile <<"<Spreadsheet>\n";
			spreadsheetFile <<"</Spreadsheet>\n";
			spreadsheetFile.close();
		}

		//If it does, put the cells in the actions newNode
		else{
			rapidxml::file<> xmlFile("Spreadsheet/"+filename+".sprd"); // Default template is char
	    rapidxml::xml_document<> doc;
	    doc.parse<0>(xmlFile.data());

			rapidxml::xml_node<> * root_node = doc.first_node("Spreadsheet");
			rapidxml::xml_node<> * cell = NULL;
			rapidxml::xml_node<> * contents = NULL;
			for (rapidxml::xml_node<> * node = root_node->first_node("Cell"); node; node = node->next_sibling()){
				cell = node->first_node("Cell");
				contents = node->first_node("Contents");
				this->actions.push_back(new ActionNode(cell,contents));
			}

		}

		//TODO: Send the data to the client

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
		Update a cell in the file
	*/
	void addCell(std::string cell, std::string value){

		//This is the worst way to do this, but I'm running out of time
		remove(this.filename);

		ofstream spreadsheetFile(this.filename);
		spreadsheetFile << "<Spreadsheet>\n";
		for(node = actions.begin; node!=actions.end(); ++node){
			spreadsheetFile << "<Name>" << node->cell << "</Name>\n";
			spreadsheetFile << "<Contents>" << node->contents << "</Contents>\n";
		}
		spreadsheetFile << "</Spreadsheet>\n";

		spreadsheetFile.close();

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
	std::string filename;
};

typedef std::deque<chat_message> message_queue;
