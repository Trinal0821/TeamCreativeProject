/*********************************
Actions for the spreadsheet instances
********************************/
#include<string>


class ActionNode{
  public:
    std::string cell;
    std::string contents;

  private:
  ActionNode(std::string cell, std::string contents){
      this->cell = cell;
      this->contents = contents;
    }
};
