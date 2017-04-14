using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using log4net.Core;

namespace TiKaoTestOne
{
    public delegate void ShowMessageDelegateHander(String message);//显示消息
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public DispatcherTimer socketItemTime=null;//获取连接列表定时器

        private SocketTiKao socketTikao=null;//Socket对象

        public MainWindow()
        {
            InitializeComponent();
            SocketTiKao.ShowMessageEvent += new ShowMessageDelegateHander(ShowMessageInfo);//监听数据
        }

        ///
        ///开启服务器
        ///
        private void startServerBtn_Click(object sender, RoutedEventArgs e)
        {

            socketTikao = new SocketTiKao();
            socketTikao.startServer();//开始监听

            //socketItemTime = new System.Windows.Threading.DispatcherTimer();
           // socketItemTime.Tick += new EventHandler(OnSocketTimedEvent);

            //socketItemTime.Interval = new TimeSpan(60000);//设置获取列表刷新时间
            //socketItemTime.Start();//开始定时器

            //打开关闭服务器按钮
            closeServerBtn.IsEnabled = true;
            startServerBtn.IsEnabled = false;

        }

        //
        //获取客户端列表
        //
        private void OnSocketTimedEvent(object sender, EventArgs e)
        {
            if (socketTikao != null)
            {
                String data = socketTikao.GetClientItem();//获取当前连接用户
                ServerInfoList.Items.Clear();//请空
                ServerInfoList.Items.Add(data);
            }
        }

        ///
        ///关闭窗口之前处理关闭服务器
        ///
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(socketTikao!=null)
            { 
               socketTikao.CloseServer();
            }
            //this.Close();
        }

        //关闭服务器
        private void CloseServer_Click(object sender, RoutedEventArgs e)
        {
            if (socketTikao != null)
            {
                socketTikao.CloseServer();
                socketTikao = null;
                
               

                //打开关闭服务器按钮
                closeServerBtn.IsEnabled = false;
                startServerBtn.IsEnabled = true;
                System.GC.Collect();
            }

        }
        
        //显示消息
        private void ShowMessageInfo(String message)
        {
            try
            {
                ServerInfo.Items.Add(message + "\n");
                ServerInfo.ScrollIntoView(ServerInfo.Items[ServerInfo.Items.Count - 1]);
            }
            catch (Exception ex)
            {
                //错误处理
            }
        }

    }



}
