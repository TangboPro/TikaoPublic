using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TiKaoTestOne
{
    class DaemonThread : Object//服务器线程开启
    {
        private Thread m_thread;
        private AsyncSocketServer m_asyncSocketServer;

        public DaemonThread(AsyncSocketServer asyncSocketServer)
        {
            m_asyncSocketServer = asyncSocketServer;
            m_thread = new Thread(DaemonThreadStart);
            m_thread.Start();
        }

        public void DaemonThreadStart()
        {
            while (m_thread.IsAlive)
            {
                AsyncSocketUserToken[] userTokenArray = null;
                m_asyncSocketServer.AsyncSocketUserTokenList.CopyList(ref userTokenArray);
                for (int i = 0; i < userTokenArray.Length; i++)
                {
                    if (!m_thread.IsAlive)
                        break;
                    try
                    {
                        if ((DateTime.Now - userTokenArray[i].ActiveDateTime).Milliseconds > m_asyncSocketServer.SocketTimeOutMS) //超时Socket断开
                        {
                            lock (userTokenArray[i])
                            {
                                m_asyncSocketServer.CloseClientSocket(userTokenArray[i]);
                            }
                        }
                    }                    
                    catch (Exception E)
                    {
                        SocketTiKao.Logger.ErrorFormat("守护线程检查超时套接字错误,出错信息: {0}", E.Message);
                        SocketTiKao.Logger.Error(E.StackTrace);
                    }
                }

                for (int i = 0; i < 60*1000/10; i++) //每分钟检测一次
                {
                    if (!m_thread.IsAlive)
                        break;
                    Thread.Sleep(10);
                }
            }
        }

        public void Close()
        {
            m_thread.Abort();
        }
    }
}
