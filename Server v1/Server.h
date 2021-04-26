#include <iostream>
#include <boost/asio.hpp>

using namespace boost::asio;

class server
{
 private:
  void start_accept();
  void handle_accept(tcp_connection::pointer new_connection,
		     const boost::system::error_code& error);

 public:
  server(boost::asio::io_service& io_service)
    : acceptor_(io_service, tcp::endpoint(tcp::v4(), 1100));

}
