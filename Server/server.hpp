/*Boost library organization code based off of: https://www.boost.org/doc/libs/1_63_0/doc/html/boost_asio/example/cpp03/chat/chat_server.cpp */
/*Working version 1.1*/
#include <algorithm>
#include <cstdlib>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include <list>
#include "connection.hpp"

using boost::asio::ip::tcp;

class server
{
public:
	/*
	* Constructor that automatically starts the connection process upon build
	*/
	server(boost::asio::io_service& io_service,
		const tcp::endpoint& endpoint)
		: io_service_(io_service),
		acceptor_(io_service, endpoint)
	{
		start_accept();
	}

	/*
	* Starts the acception process from server creation in main
	*/
	void start_accept()
	{
		connection_ptr new_connection(new connection(io_service_));
		acceptor_.async_accept(new_connection->socket(),
			boost::bind(&server::handle_accept, this, new_connection));
	}
	/*
	* Method to handle the acception of clients
	* Creates acception loop with start_accept
	*/
	void handle_accept(connection_ptr connection)
	{
		connection->start();
		connections_.insert(connection);
		start_accept();
	}

	/*
	* Closes all sockets
	* Used for server close
	*/
	void close()
	{
		std::set<connection_ptr>::iterator it;
		for (it = connections_.begin(); it != connections_.end(); ++it) {
			it->close();
		}
	}

private:
	boost::asio::io_service& io_service_;
	tcp::acceptor acceptor_;
	std::set<connection_ptr> connections_;
};

typedef boost::shared_ptr<server> server_ptr;
typedef std::list<server> server_list;
