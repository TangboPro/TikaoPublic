using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NETUploadClient.SyncSocketProtocolCore;
using NETUploadClient.SyncSocketProtocol;

namespace TikaoTestThree
{
    class clientConServer
    {
        public static event ShowMessageDelegateHander ShowMessageEvent;//委托显示消息

        private ClientSQLSocket sqlSocket;
        public clientConServer()
        {
            byte[] by = System.Text.Encoding.ASCII.GetBytes("tangbo");
            sqlSocket = new ClientSQLSocket();
        }

        public bool clientConnect(string ip, int port)
        {
            try
            {
                sqlSocket.Connect(ip, port);
                ShowMessageEvent("服务器连接成功");
                return true;
               
            }
            catch (Exception e)
            {
                ShowMessageEvent("服务器连接失败");
                return false;
            }
        }
        public string uploadSqlCom(string sqlCom){

            sqlSocket.DoActive();
            sqlSocket.DoLogin("admin", "admin");

            return sqlSocket.DoSQLOpen(sqlCom);
        }

        public bool SqlSearch()
        {
            sqlSocket.DoActive();
            sqlSocket.DoLogin("admin", "admin");
            return sqlSocket.DoSQLExec();
        }
        public void close()
        {
            sqlSocket.Disconnect();//断开连接

            ShowMessageEvent("断开服务器连接");
        }
    }
}
