using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiKaoTestOne
{
    class SqlSocketProtocol : BaseSocketProtocol
    {

       public SqlSocketProtocol(AsyncSocketServer asyncSocketServer, AsyncSocketUserToken asyncSocketUserToken)
          :base(asyncSocketServer, asyncSocketUserToken)
       {
           m_socketFlag = "SQL";

       }

       public override void Close()
       {
           base.Close();
       }

        //
       //处理分完包的数据，子类从这个方法继承
        //
       public override bool ProcessCommand(byte[] buffer, int offset, int count) 
       {
           SQLSocketCommand command = StrToCommand(m_incomingDataParser.Command);
           m_outgoingDataAssembler.Clear();
           m_outgoingDataAssembler.AddResponse();
           m_outgoingDataAssembler.AddCommand(m_incomingDataParser.Command);

           if (!CheckLogined(command)) //检测登录
           {
               m_outgoingDataAssembler.AddFailure(ProtocolCode.UserHasLogined, "");
               return DoSendResult();
           }
           if (command == SQLSocketCommand.Login)
               return DoLogin();
           else if (command == SQLSocketCommand.Active)
               return DoActive();
           else if (command == SQLSocketCommand.SQLOpen)
               return DoSQLOpen(buffer,offset,count);
           else if (command == SQLSocketCommand.SQLExec)
               return DoSQLExec();
           else if (command == SQLSocketCommand.RollbackTrans)
               return DoRollbackTrans();
           else if (command == SQLSocketCommand.CommitTrans)
               return DoCommitTrans();
           else if (command == SQLSocketCommand.BeginTrans)
               return DoBeginTrans();

           else
           {
               SocketTiKao.Logger.Error("没有命令: " + m_incomingDataParser.Command);
               return false;
           }
       }


       public bool DoSQLOpen(byte[] buffer,int offset,int count){


          try
          {
              string fatas=null;
              m_incomingDataParser.GetValue("comm", ref fatas);

              m_outgoingDataAssembler.AddSuccess();
              string data = System.Text.Encoding.UTF8.GetString(buffer, offset, count);//获取数据

              m_outgoingDataAssembler.AddValue(ProtocolKey.Item, data);//发送数据给客户端
          }
          catch (Exception E)
          {
              SocketTiKao.Logger.ErrorFormat("命令解析失败重新上传: {0}", E.Message);
              SocketTiKao.Logger.Error(E.StackTrace);
          }

          return DoSendResult();

      }
      public bool DoSQLExec()
      {
          try
          {
              //m_asyncSocketServer.AsyncSocketUserTokenList.CopyList(ref userTokenArray);
              string fatas=null;
              m_incomingDataParser.GetValue("name", ref fatas);
              m_incomingDataParser.GetValue("idCard", ref fatas);
              
              m_outgoingDataAssembler.AddSuccess();
              string name = null;

              
              m_outgoingDataAssembler.AddValue(ProtocolKey.Item, name);//发送数据给客户端
          }
          catch (Exception E)
          {
              SocketTiKao.Logger.ErrorFormat("命令解析失败重新上传: {0}", E.Message);
              SocketTiKao.Logger.Error(E.StackTrace);
          }

          return DoSendResult();
      }

      public bool DoRollbackTrans()
      {
          return true;
      }
      public bool DoCommitTrans()
      {
          return true;
      }

      public bool DoBeginTrans()
      {
          return true;
      }

       public SQLSocketCommand StrToCommand(string command)
       {
           if (command.Equals(ProtocolKey.Active, StringComparison.CurrentCultureIgnoreCase))
               return SQLSocketCommand.Active;
           else if (command.Equals(ProtocolKey.Login, StringComparison.CurrentCultureIgnoreCase))
               return SQLSocketCommand.Login;
           else if (command.Equals(ProtocolKey.SQLOpen, StringComparison.CurrentCultureIgnoreCase))
               return SQLSocketCommand.SQLOpen;
           else if (command.Equals(ProtocolKey.SQLExec, StringComparison.CurrentCultureIgnoreCase))
               return SQLSocketCommand.SQLExec;
           else if (command.Equals(ProtocolKey.RollbackTrans, StringComparison.CurrentCultureIgnoreCase))
               return SQLSocketCommand.RollbackTrans;
           else if (command.Equals(ProtocolKey.Login, StringComparison.CurrentCultureIgnoreCase))
               return SQLSocketCommand.BeginTrans;
           else if (command.Equals(ProtocolKey.SQLOpen, StringComparison.CurrentCultureIgnoreCase))
               return SQLSocketCommand.CommitTrans;
           else
               return SQLSocketCommand.None;
       }
       public bool CheckLogined(SQLSocketCommand command)
       {
           if ((command == SQLSocketCommand.Login) | (command == SQLSocketCommand.Active))
               return true;
           else
               return m_logined;
       }
    }
}
