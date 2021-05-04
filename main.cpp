#include "client.hpp"
#include "connection.hpp"
#include "server.hpp"
#include "spreadsheet_instance.hpp"
#include <cstdlib>
#include <iostream>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include <list>

using boost::asio::ip::tcp;

int main()
{
    int main(int argc, char* argv[])
    {
        try
        {
            if (argc < 2)
            {
                std::cerr << "Usage: main <port> [<port> ...]\n";
                return 1;
            }

            boost::asio::io_service io_service;

            server_list servers;
            for (int i = 1; i < argc; ++i)
            {
                tcp::endpoint endpoint(tcp::v4(), std::atoi(argv[i]));
                server_ptr server(new server(io_service, endpoint));
                servers.push_back(server);
            }

            io_service.run();
        }
        catch (std::exception& e)
        {
            std::cerr << "Exception: " << e.what() << "\n";
        }

        return 0;
    }
}
