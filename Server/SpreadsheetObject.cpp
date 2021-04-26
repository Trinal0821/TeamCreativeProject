/************************
A holder for Spreadsheet Information
************************/


class SpreadsheetObject{
	private:
		std::string spreadsheetName;
		list<std::string> actionStack;
		list<std::string> undoStack;
		list<int> workingUsers;
		
	public:
		SpreadsheetObject(name){
			this.spreadsheetName=name;
		}
		
		
		std::string undo(){
			
		}
		
		
		std::string redo(){
			
		}
		
		void addCell(std::string cell, std::string value){
			
		}
		
		
		void addClient(int client){
			workingUsers.push_back(client);
		}
		
		
		std::string broadcastToAll(std::string message){
			
		}
		
		
		std::string getData(){
			std::string spreadsheetData = "";
			ifstream spreadsheetFile("Spreadsheet/"+this.spreadsheetName+".xls");
			std::string temp;
			while (getline (spreadsheetFile, temp)) {
				spreadsheetData+=temp;
			}
			return spreadsheetData;
		}
}