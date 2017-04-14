using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NETUploadClient.SyncSocketProtocolCore;

namespace NETUploadClient.SyncSocketProtocol
{
    class ClientSQLSocket : ClientBaseSocket
    {
         public ClientSQLSocket()
            :base()
        {
            m_protocolFlag = AsyncSocketServer.ProtocolFlag.SQL;
        }

        public string DoSQLOpen(string QueryState){
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(AsyncSocketServer.ProtocolKey.SQLOpen);
                m_outgoingDataAssembler.AddValue("comm", "sds");
                byte[] data = System.Text.Encoding.UTF8.GetBytes(QueryState);


                SendCommand(data, 0, QueryState.Length);
 
                byte[] buffer;
                int offset;
                int size;
                bool bSuccess = RecvCommand(out buffer, out offset, out size);//获取服务器发来的数据
                string datas = System.Text.Encoding.UTF8.GetString(buffer,0,offset);

                string strData = null;
                m_incomingDataParser.GetValue("item", ref strData);//获得的数据

                return strData;//返回的用户信息

            }
            catch (Exception E)
            {
                //记录日志
                m_errorString = E.Message;
                return null;
            }
        }

        public bool DoSQLExec()
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(AsyncSocketServer.ProtocolKey.SQLExec);
                m_outgoingDataAssembler.AddValue("comm", "tangbo");
                m_outgoingDataAssembler.AddValue("name", "tangboname");
                m_outgoingDataAssembler.AddValue("idCard", "tangboiDcard");

                SendCommand();

                byte[] buffer;
                int offset;
                int size;
                bool bSuccess = RecvCommand(out buffer, out offset, out size);//获取服务器发来的数据
                string datas = System.Text.Encoding.UTF8.GetString(buffer, 0, offset);

                string strData = null;
                m_incomingDataParser.GetValue("item", ref strData);//获得的数据

                return true;

            }
            catch (Exception E)
            {
                //记录日志
                m_errorString = E.Message;
                return false;
            }
        }

        public bool DoBeginTrans(){
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(AsyncSocketServer.ProtocolKey.BeginTrans);
                SendCommand();
                bool bSuccess = RecvCommand();

                if (bSuccess)
                {
                    return CheckErrorCode();
                }

                else
                    return false;
            }
            catch (Exception E)
            {
                //记录日志
                m_errorString = E.Message;
                return false;
            }
        }

        public bool DoRollbackTrans()
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(AsyncSocketServer.ProtocolKey.RollbackTrans);
                SendCommand();
                bool bSuccess = RecvCommand();

                if (bSuccess)
                {
                    return CheckErrorCode();
                }

                else
                    return false;
            }
            catch (Exception E)
            {
                //记录日志
                m_errorString = E.Message;
                return false;
            }
        }
   
    }
}
