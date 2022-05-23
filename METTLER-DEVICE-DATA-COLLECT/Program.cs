using Fleck;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace METTLER_DEVICE_DATA_COLLECT
{
    class Program
    {
        #region 声明变量+对象
        private static string readWeight = "0";
        private static Dictionary<string, AsyncTcpSession> pairs = new Dictionary<string, AsyncTcpSession>();
        private static Dictionary<string, List<IWebSocketConnection>> dicList = new Dictionary<string, List<IWebSocketConnection>>();
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("**************Hello, Welcome!**************");
            CsWebsocket();
            Console.ReadKey();
        }

        #region Websocket        

        public static void CsWebsocket()
        {
            try
            {
                // FleckLog.Level = LogLevel.Debug; // 收集debug模式下的日志
                var server = new WebSocketServer("ws://172.16.0.16:8998/device/data/endpoint");
                server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        socket.Send("握手...");
                    };
                    socket.OnClose = () =>
                    {
                        socket.Send("关闭...");
                        string device = socket.ConnectionInfo.Path.Replace("/", "");
                        //关闭Tcp
                        if (pairs.ContainsKey(device))
                        {
                            pairs[device].Close();
                            pairs.Remove(device);
                        }
                        //移除socket
                        if (dicList.ContainsKey(device))
                        {
                            dicList[device].Remove(socket);
                            dicList.Remove(device);
                        }
                    };
                    socket.OnMessage = message =>
                    {
                        string device = GetIpByDeviceName(message);
                        //关闭Tcp
                        if (pairs.ContainsKey(device))
                        {
                            pairs[device].Close();
                            pairs.Remove(device);
                        }
                        //移除socket
                        if (dicList.ContainsKey(device))
                        {
                            dicList[device].Remove(socket);
                            dicList.Remove(device);
                        }
                        List<IWebSocketConnection> list = new List<IWebSocketConnection>();
                        list.Add(socket);
                        dicList.Add(device, list);
                        ConfigurationParameter(device);
                    };

                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("CsWebsocket Error:" + ex.Message.ToString());
            }
        }
        #endregion

        #region IP

        public static void ConfigurationParameter(string serverIP)
        {
            int port = 4196;//端口号
            if (!pairs.ContainsKey(serverIP))
            {
                AsyncTcpSession async = InitializeTcpClient(serverIP, port);
                pairs.Add(serverIP, async);
            }
        }

        private static AsyncTcpSession InitializeTcpClient(string serverIP, int port)
        {
            AsyncTcpSession client = new AsyncTcpSession();
            client.Closed += client_Closed;// 连接断开事件
            client.DataReceived += client_DataReceived;// 收到服务器数据事件
            client.Connected += client_Connected;// 连接到服务器事件
            client.Error += client_Error;// 发生错误的处理
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
            client.Connect(endPoint);
            return client;
        }

        private static void client_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("client_Error Error:" + e.Exception.Message.ToString());
        }

        private static void client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("client_Connected:Connect Success!");
        }

        private static void client_DataReceived(object sender, DataEventArgs e)
        {
            string hostName = ((TcpClientSession)sender).HostName;
            string szData = Encoding.Default.GetString(e.Data);
            ReadWeight(szData, hostName);
        }

        private static void client_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("client_Closed:Connect Break!");
        }

        /// <summary>
        /// 读取称重数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hostName"></param>
        private static void ReadWeight(string data, string hostName)
        {
            double weight = 0;
            try
            {
                if (hostName.Trim().Equals("192.168.5.213"))
                {
                    string[] list = data.Split("\r".ToCharArray());
                    if (list.Length > 0)
                    {
                        string dataLine = null;
                        if (!list[0].Trim().Contains("\0") && !string.IsNullOrEmpty(list[0].Trim()))
                        {
                            dataLine = list[0].Replace("\u0002", "").Replace("   ", ",").Replace("  ", ",");
                        }
                        if (!list[1].Trim().Contains("\0") && !string.IsNullOrEmpty(list[1].Trim()))
                        {
                            dataLine = list[1].Replace("\u0002", "").Replace("   ", ",").Replace("  ", ",");
                        }

                        string[] list1 = dataLine.Split(",".ToCharArray());
                        if (list1.Length > 0)
                            if (list1[0].Trim().Substring(0, 1) == "4")
                            {
                                weight = double.Parse(list1[1].Trim()) / 100.00;
                                readWeight = weight.ToString("f2");
                            }
                        if (list1[0].Trim().Substring(0, 1) == "5")
                        {
                            weight = double.Parse(list1[1].Trim()) / 1000.00;
                            readWeight = weight.ToString("f3");
                        }
                    }
                }

                if (hostName.Trim().Equals("192.168.5.214"))
                {
                    string[] list = data.Split("\r".ToCharArray());
                    if (list.Length > 0)
                    {
                        string dataLine = null;
                        if (!list[0].Trim().Contains("\0") && !string.IsNullOrEmpty(list[0].Trim()))
                        {
                            dataLine = list[0].Replace("\u0002", "").Replace("   ", ",").Replace("  ", ",");
                        }
                        if (!list[1].Trim().Contains("\0") && !string.IsNullOrEmpty(list[1].Trim()))
                        {
                            dataLine = list[1].Replace("\u0002", "").Replace("   ", ",").Replace("  ", ",");
                        }

                        string[] list1 = dataLine.Split(",".ToCharArray());
                        if (list1.Length > 0)
                            if (list1[0].Trim() != "" && list1[0].Trim().Substring(0, 1) == "4")
                            {
                                weight = double.Parse(list1[1].Trim()) / 100.00;
                                readWeight = weight.ToString("f2");
                            }
                        if (list1[0].Trim().Substring(0, 1) == "5")
                        {
                            weight = double.Parse(list1[1].Trim()) / 1000.00;
                            readWeight = weight.ToString("f3");
                        }
                    }
                }

                if (hostName.Trim().Equals("192.168.5.209"))
                {
                    string[] list = data.Split(" ".ToCharArray());
                    if (list.Length > 0)
                    {
                        string[] list1 = list[1].Split("\r".ToCharArray());
                        readWeight = list1[0].ToUpper().Replace(" ", "").Replace("SS", "").Replace("SD", "").Replace("KG", "").Replace(Environment.NewLine, "");
                        readWeight = readWeight.Substring(0, 6);
                        weight = double.Parse(readWeight) / 10.0;
                        readWeight = weight.ToString("f1");
                    }
                }

                if (hostName.Trim().Equals("192.168.5.210"))
                {
                    string[] list = data.Split(" ".ToCharArray());
                    if (list.Length > 0)
                    {
                        string[] list1 = list[1].Split("\r".ToCharArray());
                        readWeight = list1[0].ToUpper().Replace(" ", "").Replace("SS", "").Replace("SD", "").Replace("KG", "").Replace(Environment.NewLine, "");
                        readWeight = readWeight.Substring(0, 6);
                        weight = double.Parse(readWeight) / 10.0;
                        readWeight = weight.ToString("f1");
                    }
                }

                if (hostName.Trim().Equals("192.168.3.82"))
                {
                    string[] list = data.Split(" ".ToCharArray());
                    if (list.Length > 0)
                    {
                        string[] list1 = null;
                        if (!string.IsNullOrEmpty(list[1].Trim()))
                        {
                            list1 = list[1].Split("\r".ToCharArray());
                        }
                        if (!string.IsNullOrEmpty(list[2].Trim()))
                        {
                            list1 = list[2].Split("\r".ToCharArray());
                        }
                        if (!string.IsNullOrEmpty(list[4].Trim()))
                        {
                            list1 = list[4].Split("\r".ToCharArray());
                        }
                        if (!string.IsNullOrEmpty(list[5].Trim()))
                        {
                            list1 = list[5].Split("\r".ToCharArray());
                        }

                        readWeight = list1[0].ToUpper().Replace(" ", "").Replace("SS", "").Replace("SD", "").Replace("KG", "").Replace(Environment.NewLine, "");
                        weight = double.Parse(readWeight) / 10.0;
                        readWeight = weight.ToString("f1");
                    }

                }
                // 实验室ME天平
                if (hostName.Trim().Equals("192.168.5.14"))
                {
                    string[] list = data.Split(" ".ToCharArray());
                    if (list.Length > 10)
                    {
                        string strData = "";
                        for (int i = 0; i < 10; i++)
                        {
                            strData += list[i];
                        }
                        list = null;
                        readWeight = strData.ToUpper().Replace(" ", "").Replace("S", "").Replace("D", "").Replace("G", "").Replace(Environment.NewLine, "");
                        weight = double.Parse(readWeight);
                        readWeight = weight.ToString("f2");//保留两位小数
                    }
                }

                dicList[hostName].ForEach(s => s.Send("【" + hostName + "】" + readWeight));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadWeight Error:【" + hostName + "】" + ex.Message);
            }
        }

        #endregion

        /// <summary>
        /// 根据设备型号获取ip地址
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static string GetIpByDeviceName(string deviceName)
        {
            string serverIP = "";
            switch (deviceName)
            {
                case "QDZY-10006":
                    serverIP = "192.168.5.214";
                    break;
                case "QDZY-10007":
                    serverIP = "192.168.5.213";
                    break;
                case "QDZY-10008":
                    serverIP = "192.168.5.209";
                    break;
                case "QDZY-10094":
                    serverIP = "192.168.5.210";
                    break;
                case "QDZY-30001":
                    serverIP = "192.168.3.82";
                    break;
                case "QDZY-40001":
                    serverIP = "192.168.5.14";
                    break;
                default:
                    serverIP = deviceName;
                    break;
            }
            return serverIP;
        }
    }
}
