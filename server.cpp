#include <algorithm>
#include <cstdlib>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>

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
		session_ptr new_session(new chat_session(io_service_, instance_));
		acceptor_.async_accept(new_session->socket(),
			boost::bind(&chat_server::handle_accept, this, new_session));
	}
	/*
	* Method to handle the acception of clients
	* Creates acception loop with start_accept
	*/
	void handle_accept(chat_session_ptr session)
	{
		session->start();
		start_accept();
	}

private:
	boost::asio::io_service& io_service_;
	tcp::acceptor acceptor_;
	spreadsheet_instance instaance_;
};