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

using boost::asio::ip::tcp;

class connection
	: public client,
	  public boost::enable_shared_from_this<chat_session>
{

public:
	/*
	* Constructor; instant assign to remove chance for incomplete connection object
	*/
	connection(boost::asio::io_service& io_service)
		: socket_(io_service),
		instance_(instance)
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
		boost::asio::async_read_until(socket_, message_, "\n\n",
			boost::bind(&connection::process_name, shared_from_this()));
	}

	/*
	* First step of the handshake.
	* After reading, the name is recorded, to be used for spreadsheet details.
	*/
	void process_name()
	{
		std::string name, serverlist;
		std::istream is(&data_);
		//pull the name out to save
		is >> name;
		name_ = name;
		//create a string list of all available spreadsheets
		for (int i = 0; i < spreadsheets.size(); i++)
		{
			spreadlist += spreadsheets.at(i) + "\n";
		}
		spreadlist += "\n";
		//send list of spreadsheets to client, move onto the next part of the handshake
		boost::asio::async_write(socket,
			boost::asio::buffer(spreadlist)
			boost::bind(&connection::read_filename, shared_from_this()));
	}

	/*
	* Intermediate call to get the desired spreadsheet name from the client
	*/
	void read_filename()
	{
		//next call to read the incoming filename selection
		boost::asio::async_read_until(socket, message_, "\n\n",
			boost::bind(&connection::process_filename, shared_from_this()));
	}

	/*
	* Retreives the filename from client input.
	* Begins the loop to start reading and writing to the client
	* upon spreadsheet update.
	* 
	* TODO: spreadsheet part not yet implemented
	*/
	void process_filename()
	{
		std::string filename;
		std::istream is(&data_);
		is >> filename;
		filename_ = filename;
		boost::asio::async_read_until(socket_,
			boost::asio::buffer(message_), "\n",
			boost::bind(&connection::write, shared_from_this()));
		instance_.join(shared_from_this(), );

	}
	/*
	* Waits for a new update from the client, then calls write method
	*/
	void read()
	{
		boost::asio::async_read_until(socket_,
			boost::asio::buffer(message_), "\n",
			boost::bind(&connection::write, shared_from_this()));
		
		//read in the message
		std:: istream is(&buffer(message_));
		std:: string msg;

		//get the message and store it in msg
		std:: getline(is, msg);
	}

	/*
	* IN PROGRESS
	* Deliver the message to the instance for distribution to all clients
	* Continues loop to wait for more client input
	*/
	void write()
	{
		instance_.deliver(message_);
		read();
	}

	/*
	* IN PROGRESS
	* Send messages out to clients
	*/
	void deliver(const std::string& msg)
	{
		/*bool write_in_progress = !write_msgs_.empty();
		write_msgs_.push_back(msg);
		if (!write_in_progress)
		{
			boost::asio::async_write(socket_,
				boost::asio::buffer(write_msgs_.front().data(),
					write_msgs_.front().length()),
				boost::bind(&chat_session::handle_write, shared_from_this(),
					boost::asio::placeholders::error));
		}*/
	}

private:
	boost::asio::streambuf message_;
	std::string name_, filename_;
};

typedef boost::shared_ptr<connection> session_ptr;
