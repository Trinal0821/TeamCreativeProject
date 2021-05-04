#include <algorithm>
#include <cstdlib>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include <list>

using boost::asio::ip::tcp;

class server
{
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
			boost::bind(&connection::handle_accept, this, new_connection));
	}
	/*
	* Method to handle the acception of clients
	* Creates acception loop with start_accept
	*/
	void handle_accept(connection_ptr connection)
	{
		connection->start();
		start_accept();
	}

private:
	boost::asio::io_service& io_service_;
	tcp::acceptor acceptor_;
};

typedef boost::shared_ptr<server> server_ptr;
typedef std::list<server> server_list;

//TODO: Add in server close