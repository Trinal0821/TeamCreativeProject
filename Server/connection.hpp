/*Boost library organization code based off of: https://www.boost.org/doc/libs/1_63_0/doc/html/boost_asio/example/cpp03/chat/chat_server.cpp */
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
#include "Json/single_include/nlohmann/json.hpp"
#include <string>
#include "spreadsheet_instance.hpp"

using boost::asio::ip::tcp;



class connection
	: public client,
	public boost::enable_shared_from_this<connection>
{

public:






	/*
	* Constructor; instant assign to remove chance for incomplete connection object
	*/
	connection(boost::asio::io_service& io_service) :socket_(io_service)
	{}







	/*
	* Getter for the socket object
	*/
	tcp::socket& socket()
	{
		return socket_;
	}







	/*
	* Method called by the server to begin the client connection process
	*/
	void start()
	{
		//begin by reading from the buffer and seeing if the name has been sent
		boost::asio::async_read_until(socket_, input_message_, "\n\n",
			boost::bind(&connection::process_name, shared_from_this(),
				boost::asio::placeholders::error));
	}









	/*
	* First step of the handshake.
	* After reading, the name is recorded, to be used for spreadsheet details.
	*/
	void process_name(const boost::system::error_code& error)
	{
		if (!error)
		{
			std::ostringstream ss;
			ss << input_message_;
			std::string name = ss.str();
			name_ = name;

			//build up list of available spreadsheets
			std::string spreadlist;
			std::string path = "Spreadsheet";
			for (const auto& entry : std::filesystem::directory_iterator(path)) {
				spreadlist += entry.path() + "\n";
			}

			spreadlist += "\n";
			//send list of spreadsheets to client, move onto the next part of the handshake
			boost::asio::async_write(socket,
				boost::asio::buffer(spreadlist),
				boost::bind(&connection::read_filename, shared_from_this(),
					boost::asio::placeholders::error));
		}
	}






	/*
	* Intermediate call to get the desired spreadsheet name from the client
	*/
	void read_filename(const boost::system::error_code& error)
	{
		if (!error)
		{
			//next call to read the incoming filename selection
			boost::asio::async_read_until(socket, input_message_, "\n\n",
				boost::bind(&connection::process_filename, shared_from_this(),
					boost::asio::placeholders::error));
		}
	}







	/*
	* Retreives the filename from client input.
	* Begins the loop to start reading and writing to the client
	* upon spreadsheet update.
	*
	* DONE: spreadsheet part not yet implemented
	*/
	void process_filename(const boost::system::error_code& error)
	{
		if (!error)
		{
			std::ostringstream ss;
			ss << input_message_;
			std::string filename = ss.str();
			filename_ = filename;

			spreadsheet_instance instance(filename_);
			workingSheet = instance;
			int clientNum = workingSheet.join(shared_from_this());
			//DONE:send clientID- have increment counter
			//DONE::CREATE WORKING SHEET
			boost::asio::async_write(socket, clientNum + "",
				boost::bind(&connection::read, shared_from_this(),
					boost::asio::placeholders::error));
		}
	}








	/*
	* Waits for a new update from the client, then calls write method
	*/
	void read(const boost::system::error_code& error)
	{
		if (!error)
		{
			boost::asio::async_read_until(socket_,
				boost::asio::buffer(input_message_), "\n",
				boost::bind(&connection::write, shared_from_this(),
					boost::asio::placeholders::error));
			//read in the message
			std::istream is(&buffer(input_message_));
			std::string msg;

			//get the message and store it in msg
			std::getline(is, msg);

			//Parse Json
			nlohmann::json decodedMessage = nlohmann::json::parse(msg);
			std::string requestType = decodedMessage.value("requestType", "none");
			switch (requestType) {
				if (requestType == "editCell") {
					//Update cell in spreadsheet
					std::string cell = decodedMessage.value("cellName", "none");
					std::string update = decodedMessage.value("contents", "none");
					workingSheet->updateCell(cell, update);
					std::string message = "{\"messageType\":\"editCell\", \"cellName\": \"" + cell + "\",\"contents\":\"" + update + "\"}";
				}

				else if (requestType == "selectCell") {
					//Open and create spreadsheet
					std::string cell = decodedMessage.value("cellName", "none");
					//DONE: We need to get the selector id
					int selectorID = workingSheet->getID(shared_from_this());
					std::string highlightMessage = "{\"messageType\":\"cellSelected\", \"cellName\": \"" + cell + "\", \"selector\": \"" + selectorID + "\",\"selectorName\":\"" + name_ + "\"}";
					workingSheet->deliver(highlightMessage);
				}

				else if (requestType == "undo") {
					//Undo
					workingSheet->undo(shared_from_this());
				}

				else if (requestType == "revertCell") {
					//Revert
					workingSheet->revert(shared_from_this());
				}
				else {
					//Data is unreadable. Send a requestError message
				}
			}
		}
		else
		{
			workingSheet->leave(shared_from_this());
		}
	}









	/*
	* Deliver the message to the instance for distribution to all clients
	* Continues loop to wait for more client input
	*/
	void write(const boost::system::error_code& error)
	{
		//DONE: Remove client from list if send fails
		//TODO: LOCK EVERYTHING BEFORE SENDING
		if (!error)
		{
			to_write.pop_front();
			if (!to_write.empty())
			{
				boost::asio::async_write(socket_,
					boost::asio::buffer(to_write.front().data(),
						to_write.front().length()),
					boost::bind(&connection::write, shared_from_this(),
						boost::asio::placeholders::error));
			}
		}
		else
		{
			workingSheet->leave(shared_from_this());
		}
	}









	/*
	* Send messages out to clients
	*/
	void deliver(const std::string& msg)
	{
		bool write_in_progress = !to_write.empty();
		to_write.push_back(msg);
		if (!write_in_progress)
		{
			boost::asio::async_write(socket_,
				boost::asio::buffer(to_write.front().data(),
					to_write.front().length()),
				boost::bind(&connection::write, shared_from_this(),
					boost::asio::placeholders::error));
		}
	}

	void close()
	{
		socket_.shutdown(boost::asio::ip::tcp::socket::shutdown_receive);
		boost::asio::async_write(socket_,
			boost::asio::buffer(to_write.front().data(),
				to_write.front().length()),
			boost::bind(&connection::socket_close, shared_from_this(),
				boost::asio::placeholders::error));
	}

	void socket_close(const boost::system::error_code& error)
	{
		socket_.close();
	}

private:
	boost::asio::streambuf input_message_;
	std::string name_, filename_;
	spreadsheet_instance* workingSheet;
	std::deque<std::string> to_write;
	boost::asio::ip::tcp::socket socket_;

};
typedef boost::shared_ptr<connection> connection_ptr;
