/*Boost library organization code based off of: https://www.boost.org/doc/libs/1_63_0/doc/html/boost_asio/example/cpp03/chat/chat_server.cpp */
/*Working version*/
#include <algorithm>
#include <cstdlib>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>

using boost::asio::ip::tcp;
class client
{
public:
	virtual ~client() {}
	virtual void deliver(const std::string msg) = 0;
};

typedef boost::shared_ptr<client> client_ptr;