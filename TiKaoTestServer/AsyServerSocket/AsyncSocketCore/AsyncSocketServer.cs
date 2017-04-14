using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TiKaoTestOne
{
    public class AsyncSocketServer//开启服务、用户管理、发送数据、接收数据、协议选择、异步
    {
        private Socket listenSocket;
        
        private int m_numConnections; //最大支持连接个数
        private int m_receiveBufferSize; //每个连接接收缓存大小
        private Semaphore m_maxNumberAcceptedClients; //限制访问接收连接的线程数，用来控制最大并发数

        private int m_socketTimeOutMS; //Socket最大超时时间，单位为MS
        public int SocketTimeOutMS { get { return m_socketTimeOutMS; } set { m_socketTimeOutMS = value; } }
        //     
        private AsyncSocketUserTokenPool m_asyncSocketUserTokenPool;//空闲token
        private AsyncSocketUserTokenList m_asyncSocketUserTokenList;//运行token

        
        //
        public AsyncSocketUserTokenList AsyncSocketUserTokenList { get { return m_asyncSocketUserTokenList; } }

        private LogOutputSocketProtocolMgr m_logOutputSocketProtocolMgr;/////////////
        public LogOutputSocketProtocolMgr LogOutputSocketProtocolMgr { get { return m_logOutputSocketProtocolMgr; } }

        private DaemonThread m_daemonThread;//////////////////

        public AsyncSocketServer(int numConnections)
        {
            m_numConnections = numConnections;
            m_receiveBufferSize = ProtocolConst.ReceiveBufferSize;

            m_asyncSocketUserTokenPool = new AsyncSocketUserTokenPool(numConnections);
            m_asyncSocketUserTokenList = new AsyncSocketUserTokenList();
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);

            m_logOutputSocketProtocolMgr = new LogOutputSocketProtocolMgr();

        }

        public void Init()
        {
            AsyncSocketUserToken userToken;
            for (int i = 0; i < m_numConnections; i++) //按照连接数建立读写对象
            {
                userToken = new AsyncSocketUserToken(m_receiveBufferSize);//创建后却无法释放掉
                userToken.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                m_asyncSocketUserTokenPool.Push(userToken);
                
            }
        }

        public void Start(IPEndPoint localEndPoint)
        {
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(m_numConnections);
            SocketTiKao.Logger.InfoFormat("服务器开启 {0} 成功", localEndPoint.ToString());
            
            
            StartAccept(null);
            m_daemonThread = new DaemonThread(this);
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)//初始化
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);//发送请求来时调用
            }
            else
            {
                acceptEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
            }

            m_maxNumberAcceptedClients.WaitOne(); //递增线程数
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArgs);//绑定
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);//？
            }
        }

        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs acceptEventArgs)
        {

            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (Exception E)
            {
                //SocketTiKao.Logger.ErrorFormat("客户端连接 {0} 失败, 出错信息: {1}", acceptEventArgs.AcceptSocket, E.Message);
                //SocketTiKao.Logger.Error(E.StackTrace);  
                
            }
            

        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
          //  SocketTiKao.Logger.InfoFormat("客户端连接成功. 客户端地址: {0}, 远程地址: {1}",
           //     acceptEventArgs.AcceptSocket.LocalEndPoint, acceptEventArgs.AcceptSocket.RemoteEndPoint);

            AsyncSocketUserToken userToken = m_asyncSocketUserTokenPool.Pop();
            m_asyncSocketUserTokenList.Add(userToken); //添加到正在连接列表
            userToken.ConnectSocket = acceptEventArgs.AcceptSocket;//绑定连接用户
            userToken.ConnectDateTime = DateTime.Now;//登录时间

            try
            {
                bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                if (!willRaiseEvent)
                {
                    lock (userToken)
                    {
                        ProcessReceive(userToken.ReceiveEventArgs);
                    }
                }                    
            }
            catch (Exception E)
            {
                //SocketTiKao.Logger.ErrorFormat("客户端连接 {0} 失败, 出错信息: {1}", userToken.ConnectSocket, E.Message);
                //SocketTiKao.Logger.Error(E.StackTrace);                
            }            

            StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接
        }
        //AsyncSocketUserToken包含一个接收异步事件m_receiveEventArgs，一个发送异步事件m_sendEventArgs，
        //接收数据缓冲区m_receiveBuffer，发送数据缓冲区m_sendBuffer，协议逻辑调用对象m_asyncSocketInvokeElement，
        //建立服务对象后，需要实现接收和发送的事件响应函数：
        void IO_Completed(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            AsyncSocketUserToken userToken = asyncEventArgs.UserToken as AsyncSocketUserToken;
            userToken.ActiveDateTime = DateTime.Now;//取得消息时间
            try
            {                
                lock (userToken)
                {
                    if (asyncEventArgs.LastOperation == SocketAsyncOperation.Receive)
                        ProcessReceive(asyncEventArgs);//发送处理
                    else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Send)
                        ProcessSend(asyncEventArgs);//接收处理
                    else
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }   
            }
            catch (Exception E)
            {
                SocketTiKao.Logger.ErrorFormat("IO_Completed {0} error, message: {1}", userToken.ConnectSocket, E.Message);
                SocketTiKao.Logger.Error(E.StackTrace);
            }                     
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            AsyncSocketUserToken userToken = receiveEventArgs.UserToken as AsyncSocketUserToken;
            if (userToken.ConnectSocket == null)
                return;
            userToken.ActiveDateTime = DateTime.Now;
            if (userToken.ReceiveEventArgs.BytesTransferred > 0 && userToken.ReceiveEventArgs.SocketError == SocketError.Success)
            {
                int offset = userToken.ReceiveEventArgs.Offset;//传输数据的偏移量
                int count = userToken.ReceiveEventArgs.BytesTransferred;//传输时套体字的大小
                if ((userToken.AsyncSocketInvokeElement == null) & (userToken.ConnectSocket != null)) //存在Socket对象，并且没有绑定协议对象，则进行协议对象绑定
                {
                    BuildingSocketInvokeElement(userToken);
                    offset = offset + 1;//偏移量加一
                    count = count - 1;//
                }
                if (userToken.AsyncSocketInvokeElement == null) //如果没有解析对象，提示非法连接并关闭连接
                {
                    SocketTiKao.Logger.WarnFormat("非法连接. 客户端地址: {0}, 远程地址: {1}", userToken.ConnectSocket.LocalEndPoint, 
                        userToken.ConnectSocket.RemoteEndPoint);
                    CloseClientSocket(userToken);//关闭当前连接
                }
                else
                {
                    if (count > 0) //处理接收数据
                    {
                        if (!userToken.AsyncSocketInvokeElement.ProcessReceive(userToken.ReceiveEventArgs.Buffer, offset, count))
                        { //如果处理数据返回失败，则断开连接
                            CloseClientSocket(userToken);
                        }
                        else //否则投递下次介绍数据请求
                        {
                            bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                            if (!willRaiseEvent)
                                ProcessReceive(userToken.ReceiveEventArgs);
                        }
                    }
                    else
                    {
                        bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                        if (!willRaiseEvent)
                            ProcessReceive(userToken.ReceiveEventArgs);
                    }
                }
            }
            else
            {
                CloseClientSocket(userToken);
            }
        }

        private void BuildingSocketInvokeElement(AsyncSocketUserToken userToken)
        {
            byte flag = userToken.ReceiveEventArgs.Buffer[userToken.ReceiveEventArgs.Offset];
            if (flag == (byte)ProtocolFlag.LogOutput)
                userToken.AsyncSocketInvokeElement = new LogOutputSocketProtocol(this, userToken);
            else if (flag == (byte)ProtocolFlag.SQL)
                userToken.AsyncSocketInvokeElement = new SqlSocketProtocol(this, userToken);

            if (userToken.AsyncSocketInvokeElement != null)
            {
                SocketTiKao.Logger.InfoFormat("构建套接字调用元素 {0}.客户端地址: {1}, 远程地址: {2}",
                    userToken.AsyncSocketInvokeElement, userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
            } 
        }

        private bool ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            AsyncSocketUserToken userToken = sendEventArgs.UserToken as AsyncSocketUserToken;
            if (userToken.AsyncSocketInvokeElement == null)
                return false;
            userToken.ActiveDateTime = DateTime.Now;
            if (sendEventArgs.SocketError == SocketError.Success)
                return userToken.AsyncSocketInvokeElement.SendCompleted(); //调用子类回调函数
            else
            {
                CloseClientSocket(userToken);//关闭
                return false;
            }
        }

        //持续发送回调函数
        public bool SendAsyncEvent(Socket connectSocket, SocketAsyncEventArgs sendEventArgs, byte[] buffer, int offset, int count)
        {
            if (connectSocket == null)
                return false;
            sendEventArgs.SetBuffer(buffer, offset, count);
            bool willRaiseEvent = connectSocket.SendAsync(sendEventArgs);//发送数据
            if (!willRaiseEvent)
            {
                return ProcessSend(sendEventArgs);//回调开始
            }
            else
                return true;
        }

        public void CloseClientSocket(AsyncSocketUserToken userToken)
        {
            if (userToken.ConnectSocket == null)
                return;
            string socketInfo = string.Format("客户端地址: {0} 远程地址: {1}", userToken.ConnectSocket.LocalEndPoint,
                userToken.ConnectSocket.RemoteEndPoint);
            SocketTiKao.Logger.InfoFormat("客户端断开连接. {0}", socketInfo);

            try
            {
                userToken.ConnectSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception E) 
            {
               // SocketTiKao.Logger.ErrorFormat("关闭客户端 {0} 出错,出错信息: {1}", socketInfo, E.Message);
            }

            userToken.ConnectSocket.Close();
            userToken.ConnectSocket = null; //释放引用，并清理缓存，包括释放协议对象等资源

            m_maxNumberAcceptedClients.Release();
            m_asyncSocketUserTokenPool.Push(userToken);
            m_asyncSocketUserTokenList.Remove(userToken);
        }

        public void Close(){//关闭连接



            m_asyncSocketUserTokenPool.ClearPool();

            m_daemonThread.Close();
            listenSocket.Dispose();
            listenSocket.Close();

            SocketTiKao.Logger.InfoFormat("服务器已关闭");
        }
    }
}
