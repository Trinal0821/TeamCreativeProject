/*Boost library organization code based off of: https://www.boost.org/doc/libs/1_63_0/doc/html/boost_asio/example/cpp03/chat/chat_server.cpp */
#include "server.hpp"
#include <cstdlib>
#include <iostream>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include <list>
#include <csignal>

using boost::asio::ip::tcp;


server_ptr server_;

/*
* Catch server close and push it to a handler that sends the closing message
*/

void server_close(int signum)
{
	server_->close();
}

int main(int argc, char* argv[])
{
	//catch close of server
	signal(SIGINT, server_close);
	try
	{
		if (argc < 2)
		{
			std::cerr << "Usage: main <port> \n";
			return 1;
		}

		//create a server object and run
		boost::asio::io_service io_service;

		tcp::endpoint endpoint(tcp::v4(), std::atoi(argv[1]));
		server_ptr running_server(new server(io_service, endpoint));

		server_ = running_server;

		io_service.run();

	}
	//catch any errors that might happen upon server startup
	catch (std::exception& e)
	{
		std::cerr << "Exception: " << e.what() << "\n";
	}

	return 0;
}
