/*Boost library organization code based off of: https://www.boost.org/doc/libs/1_63_0/doc/html/boost_asio/example/cpp03/chat/chat_server.cpp */
/*Working version*/
#include <algorithm>
#include <cstdlib>
#include <deque>
#include <iostream>
#include <fstream>
#include <list>
#include <set>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include <boost/filesystem.hpp>
#include "rapidxml/rapidxml.hpp"
#include "actionNode.cpp"
#include "client.hpp"
#include <map>
#include <mutex> 

class spreadsheet_instance
{

private:
	std::list<client_ptr> clients_;
	enum { max_recent_msgs = 100 };
  std::list<ActionNode*> actions;
  std::list<ActionNode*> undone;
	std::string filename;
	int clientNumCounter = -1;
	bool newlyCreated;
  std::mutex sheetLock;
public:

	/*
		Creates an instance of the spreadsheet
	*/
	spreadsheet_instance(std::string filename){
		this->filename="Spreadsheet/"+filename+".sprd";
		std::list<std::string> fileList;
    std::string path = "Spreadsheet";
    for (const auto & entry : boost::filesystem::directory_iterator(path)){
      fileList.push_back(entry.path().leaf().string());
    }

		bool foundSheet = false;
		std::list<std::string>::iterator sheet;
		for(sheet = fileList.begin(); sheet!=fileList.end(); ++sheet){
		  if(sheet->c_str() == filename){
				foundSheet=true;
				break;
			}
		}

		//If it doesn't exist, create it
		if(!foundSheet){
		  std::ofstream spreadsheetFile("Spreadsheet/"+filename+".sprd");
			spreadsheetFile << "<Spreadsheet>\n";
			spreadsheetFile << "</Spreadsheet>\n";
			spreadsheetFile.close();
			bool newlyCreated = true;
		}

		//If it does, put the cells in the actions newNode
		else{
		  std::ifstream file("Spreadsheet/"+filename+".sprd");
		  std::stringstream buffer;
		  buffer << file.rdbuf();
		  file.close();
		  std::string content(buffer.str());
	    rapidxml::xml_document<> doc;
	    doc.parse<0>(&content[0]);

			rapidxml::xml_node<> * root_node = doc.first_node("Spreadsheet");
			rapidxml::xml_node<> * cell = NULL;
			rapidxml::xml_node<> * contents = NULL;
			for (rapidxml::xml_node<> * node = root_node->first_node("Cell"); node; node = node->next_sibling()){
				cell = node->first_node("Cell");
				contents = node->first_node("Contents");
				this->actions.push_back(new ActionNode(cell->value(),contents->value()));
			}
			bool newlyCreated = false;
		}

	}




	/*
	* Assigns the client to the instance of the server, for output purposes
	*/
	int join(client_ptr clientJoin)
	{
		//partially-done: send empty string with two newlines if new spreadsheet
		if (this->newlyCreated)
		{
			clientJoin->deliver("\n");
		}
		else
		{
		  sheetLock.lock();
			//TODO: send spreadsheet as combination of edits
			sheetLock.unlock();
		}
		this->clientNumCounter++;
		clients_.push_back(clientJoin);
	        std::list<ActionNode*>::iterator node;
	       	for(const auto& node : actions){
		  clientJoin->deliver("{\"messageType\":\"editCell\", \"cellName\":\""+node->cell+"\", \"contents\":\""+node->value+"\"}");
		}
		return this->clientNumCounter;
	}




	/*
	* Remove a client from the list of connected clients
	*/
	void leave(client_ptr clientLeave)
	{
		int clientNum = getID(clientLeave);
		std::list<client_ptr>::iterator remove;
		for(remove = clients_.begin();remove != clients_.end(); ++remove){
		  if(&(*remove) == &clientLeave){
		    clients_.erase(remove);
		  }
		}
		deliver("{\"messageType\":\"disconnected\", \"user\":\"" + std::to_string(clientNum) + "\"}");
	}




	/*
	* Send a message to all clients
 	*/
	void deliver(const std::string& msg)
	{
	  sheetLock.lock();
		std::for_each(clients_.begin(), clients_.end(),
			boost::bind(&client::deliver, _1, boost::ref(msg)));
		sheetLock.unlock();
	}




	/*
		Undo a cell action
	*/
	void undo(){
		try {
			//DONE: Wrap in try/catch
			ActionNode* removingNode = actions.back();
			actions.pop_back();
			std::list<ActionNode*>::iterator node;
			for(const auto& node : actions){
				if (removingNode->cell == node->cell) {
					addCell(removingNode->cell, removingNode->value);
					undone.push_back(new ActionNode(removingNode->cell, removingNode->value));
					return;
				}
		
			}
			addCell("", removingNode->value);
			undone.push_back(new ActionNode(removingNode->cell, removingNode->value));
		}
		catch (...)
		{
			deliver("{\"messageType\":\"requestError\",\"message\":\"No undo's possible.\"}\n");
		}
	}




	/*
		Redo a cell action
	*/
	void revert(){
		try {
			ActionNode* removingNode = undone.back();
			undone.pop_back();
			addCell(removingNode->cell, removingNode->value);
		}
		catch (...)
		{
			deliver("{\"messageType\":\"requestError\",\"message\":\"No reverts possible.\"}\n");
		}
	}





	/*
		Update a cell in the file
	*/
	void addCell(std::string cell, std::string value){
	  sheetLock.lock();
		//This is the worst way to do this, but I'm running out of time
	  int l = this->filename.length();
	  char file[l+1];
	  strcpy(file, this->filename.c_str());
	  std::remove(file);

		std::ofstream spreadsheetFile(this->filename);
		spreadsheetFile << "<Spreadsheet>\n";

		for(const auto& node : actions){
			spreadsheetFile << "<Name>" << node->cell << "</Name>\n";
			spreadsheetFile << "<Contents>" << node->value << "</Contents>\n";
		}

		spreadsheetFile << "</Spreadsheet>\n";

		spreadsheetFile.close();

		actions.push_back(new ActionNode(cell, value));
		std::string message = "{\"messageType\":\"editCell\", \"cellName\": \""+cell+"\",\"contents\":\""+value+"\"}\n";
		deliver(message);
		sheetLock.unlock();
	}

	int getID(client_ptr clientFind)
	{
	  int counter = 0;
		std::list<client*>::iterator clientNode;
		for(const auto& clientNode : clients_){
		  if(&clientNode == &clientFind)
		    return counter;
		  counter++;
		}
		return -1;
	}


};
