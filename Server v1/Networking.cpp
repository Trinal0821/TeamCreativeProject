#include <iostream>
#include <boost/asio.hpp>
#include "networking.h"

using namespace boost::asio;

std::vector<spreadsheet> spreadsheets;

class networking : public boost::enable_shared_from_this(networking)
{
	static pointer create(boost::asio::io_service & io_service)
	{
		return pointer(new con_handler(io_service));
	}

	tcp::socket& socket()
	{
		return socket;
	}

	void start(std::vector<spreadsheet> given_spreadsheets)
	{
		//copy passed in spreadsheets
		spreadsheets = given_spreadsheets;
		//begin by reading from the buffer and seeing if the name has been sent
		boost::asio::async_read_until(socket, boost::asio::buffer(message_), "\n\n"
			boost::bind(&networking::process_name, shared_from_this()));

	}
	void process_name()
	{
		std::string name, serverlist;
		std::istream is(&data_);
		//pull the name out to save
		is >> name;
		//create a string list of all available spreadsheets
		for (int i = 0; i < spreadsheets.size(); i++)
		{
			spreadlist += spreadsheets.at(i) + "\n";
		}
		spreadlist += "\n";
		//go to next part of handshake (reading the filename)
		boost::asio::async_write(socket,
			boost::asio::buffer(spreadlist)
			boost::bind(&networking::read_filename, shared_from_this()));
	}

	void read_filename()
	{
		//next call to read the incoming filename selection
		boost::asio::async_read_until(socket, boost::asio::buffer(message_), "\n\n"
			boost::bind(&networking::process_filename, shared_from_this()));
	}

	void process_filename()
	{
		std::string filename;
		std::istream is(&data_);
		is >> filename;

	}

	void read()
	{
		std::cout << "In read" << std::endl;
	}

	void write()
	{
		std::cout << "In write" << std::endl;
	}
}
