/************************
A holder for client information when they connect to the server
************************/


class ClientObject{
	private:
		int clientSocket;
		
	public:
		ClientObject(int socket){
			this.clientSocket=socket;
		}
		
		//This will hold the name of the client
		std::string clientName;
		//This will tell which spreadsheet the client is using
		std::string clientSpreadsheet;
}