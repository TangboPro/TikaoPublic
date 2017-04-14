using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace TiKaoTestOne
{

   public class SocketTiKao
    {
        public static ILog Logger;
        public static AsyncSocketServer AsyncSocketSvr;
        public static string FileDirectory;
        public static event ShowMessageDelegateHander ShowMessageEvent;
       public bool startServer()
       {
           DateTime currentTime = DateTime.Now;//服务时间

           //日志管理
           log4net.GlobalContext.Properties["LogDir"] = currentTime.ToString("yyyyMM");
           log4net.GlobalContext.Properties["LogFileName"] = "_SocketAsyncServer" + currentTime.ToString("yyyyMMdd");
           Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

           
           //
           Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
           FileDirectory = config.AppSettings.Settings["FileDirectory"].Value;

           //创建文件夹
           if (FileDirectory == "")
               FileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files");//合成路径

           if (!Directory.Exists(FileDirectory))//当前路径不存在创建文件夹
               Directory.CreateDirectory(FileDirectory);

           //创建端口
           int port = 0;
           if (!(int.TryParse(config.AppSettings.Settings["Port"].Value, out port)))
               port = 9999;

           //连接数
           int parallelNum = 0;
           if (!(int.TryParse(config.AppSettings.Settings["ParallelNum"].Value, out parallelNum)))
               parallelNum = 400;

           //Socket最大超时时间，单位为MS
           int socketTimeOutMS = 0;
           if (!(int.TryParse(config.AppSettings.Settings["SocketTimeOutMS"].Value, out socketTimeOutMS)))
               socketTimeOutMS = 5 * 60 * 1000;


           AsyncSocketSvr = new AsyncSocketServer(parallelNum);
           AsyncSocketSvr.SocketTimeOutMS = socketTimeOutMS;
           AsyncSocketSvr.Init();//初始化
           IPEndPoint listenPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);//添加地址
           AsyncSocketSvr.Start(listenPoint);//开始服务

           return true;
       }


       //自定义函数
       //
       //获取客户端连接
       //
       public String GetClientItem()
       {
           AsyncSocketUserToken[] userTokenArray = null;

           AsyncSocketSvr.AsyncSocketUserTokenList.CopyList(ref userTokenArray);

           String socketText = null;

           for (int i = 0; i < userTokenArray.Length; i++)
           {
               try
               {
                   socketText =socketText+userTokenArray[i].ConnectSocket.LocalEndPoint.ToString() + "\t"
                       + userTokenArray[i].ConnectSocket.RemoteEndPoint.ToString() + "\t"
                       + (userTokenArray[i].AsyncSocketInvokeElement as BaseSocketProtocol).SocketFlag + "\t"
                       + (userTokenArray[i].AsyncSocketInvokeElement as BaseSocketProtocol).UserName + "\t"
                       + userTokenArray[i].AsyncSocketInvokeElement.ConnectDT.ToString() + "\t"
                       + userTokenArray[i].AsyncSocketInvokeElement.ActiveDT.ToString() + "\n";
               }
               catch (Exception E)
               {
                   SocketTiKao.Logger.ErrorFormat("客户端列表获取失败,Message:{0}", E.Message);
                   SocketTiKao.Logger.Error(E.StackTrace);
               }
           }

           return socketText;
       }
   
       //
       //显示消息
       //
       public static Boolean ShowMessage(String message)
       {
           if (ShowMessageEvent != null)
           {
               ShowMessageEvent(message);
               return true;
           }
           else return false;
       } 


       public bool CloseServer()
       {
           AsyncSocketSvr.Close();
           AsyncSocketSvr = null;
           return true;
       }
   }
}
