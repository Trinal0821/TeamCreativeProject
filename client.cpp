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