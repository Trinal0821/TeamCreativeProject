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
private:
	std::set<client_ptr> clients_;
	enum { max_recent_msgs = 100 };
	message_queue messages_;
};

typedef std::deque<chat_message> message_queue;