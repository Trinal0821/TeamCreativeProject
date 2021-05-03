/*Boost library organization code based off of: https://www.boost.org/doc/libs/1_63_0/doc/html/boost_asio/example/cpp03/chat/chat_server.cpp */
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

server_ptr server_;

int main()
{
    int main(int argc, char* argv[])
    {
        catch_sigterm();
        try
        {
            if (argc < 2)
            {
                std::cerr << "Usage: main <port> \n";
                return 1;
            }

            boost::asio::io_service io_service;

            tcp::endpoint endpoint(tcp::v4(), std::atoi(argv[1]));
            server_ptr server(new server(io_service, endpoint));

            server_ = server;

            io_service.run();
            
        }
        catch (std::exception& e)
        {
            std::cerr << "Exception: " << e.what() << "\n";
        }

        return 0;
    }
}
/*
* Catch server close and push it to a handler that sends the closing message
*/
void catch_sigterm()
{
    static struct sigaction _sigact;

    memset(&_sigact, 0, sizeof(_sigact));
    _sigact.sa_sigaction = server_close;
    _sigact.sa_flags = SA_SIGINFO;

    sigaction(SIGTERM, &_sigact, NULL);
}

void server_close(int signum, siginfo_t* info, void* ptr)
{
    server_.close();
}
