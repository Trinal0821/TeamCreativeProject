#include <iostream>
#include <boost/asio.hpp>

using namespace boost::asio;

class networking : boost::enable_shared_from_this<networking>
{
  private:
  tcp::socket socket;
  char buffer = char[4096];

public:
  typedef boost::shared_ptr<networking> pointer;
  networking(boost::asio::io_service& io_service);
  static pointer create(boost::asio::io_service& io_service);
  tcp::socket& socket();
  void start();
  void read();
  void write();
}
