using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Puelloc
{
    public class HttpClient
    {
        private readonly Setting _sets;
        public HttpClient(Setting setting, params Pipe[] routes)
        {
            _sets = setting;
            ResponseMessage.BasePath = _sets.BasePath;
            _patterns = new List<Pipe>(routes);
        }

        private readonly List<Pipe> _patterns;
        private Socket _socket;
        private Thread _acceptThread = null;

        public bool IsRunning { get; private set; } = false;

        public IPEndPoint LocalEndPoint => _sets.BindIP;

        public void Listen()
        {
            if (_sets.BindIP.Port != 0)
            {
                var ipinf = IPGlobalProperties.GetIPGlobalProperties();
                foreach (var endpoint in ipinf.GetActiveTcpListeners())
                {
                    if (endpoint.Port == _sets.BindIP.Port)
                    {
                        if (_sets.IsAutoChangePortIfHasUsed)
                        {
                            _sets.BindIP.Port = 0;
                        }
                        else
                        {
                            Log($"Port {_sets.BindIP.Port} has been used.");
                            return;
                        }
                    }
                }
            }

            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Bind(_sets.BindIP);
            }
            catch (SocketException e)
            {
                Log($"Bind Fail {e.Message}");
                return;
            }

            if (((IPEndPoint)_socket.LocalEndPoint).Port != _sets.BindIP.Port)
            {
                _sets.BindIP.Port = ((IPEndPoint)_socket.LocalEndPoint).Port;
                Log($"Port has been auto changed to {_sets.BindIP.Port}");
            }

            _socket.ReceiveTimeout = _sets.ReceiveTimeOut;
            _socket.SendTimeout = _sets.SendTimeOut;
            _socket.Listen(_sets.BackLog);
            _acceptThread = new Thread(() =>
            {
                while (IsRunning)
                {
                    try
                    {
                        Socket a = _socket.Accept();
                        Task.Run(() => Accept(a));
                    }
                    catch (SocketException e)
                    {
                        Log(e.Message);
                    }
                }
            })
            { IsBackground = true };
            _acceptThread.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException) { }

            _socket.Close();
            while (AcceptConnectCounts > 0)
            {
            }
        }

        public void AddRoutes(params Pipe[] routes)
        {
            _patterns.AddRange(routes);
        }

        public int AcceptConnectCounts { get; private set; }

        private void Accept(Socket acceptSocket)
        {
            int aSid = AcceptConnectCounts++;
            Log($"New Accept {aSid}");
            try
            {
                while (true)
                {
                    StringBuilder request = new StringBuilder();
                    byte[] receivedByte = new byte[4096];
                    int lengthOfReceivedByte = acceptSocket.Receive(receivedByte, receivedByte.Length, SocketFlags.None);
                    request.Append(Encoding.UTF8.GetString(receivedByte, 0, lengthOfReceivedByte).Replace("\0", ""));
                    while (lengthOfReceivedByte == receivedByte.Length)
                    {
                        lengthOfReceivedByte = acceptSocket.Receive(receivedByte, receivedByte.Length, SocketFlags.None);
                        request.Append(Encoding.UTF8.GetString(receivedByte, 0, lengthOfReceivedByte).Replace("\0", ""));
                    }
                    RequsetMessage req = Message.Parse(request.ToString()) as RequsetMessage;
                    if (req == null)
                    {
                        continue;
                    }

                    Log($"{aSid}:Receive {req.Method} {req.Url}");
                    ResponseMessage res = GetResponse(req);
                    if (res != null)
                    {
                        acceptSocket.Send(res.ToBytes());
                    }

                    Log($"{aSid}:Send {req.Method} {req.Url}");
                }
            }
            catch (SocketException se)
            {
                Log($"Socket:{se.Message}");
            }
            catch (Exception e)
            {
                Log(e.Message);
                throw;
            }
            finally
            {
                acceptSocket.Close();
                Log($"Close Accept {aSid}");
                AcceptConnectCounts--;
            }
        }

        private ResponseMessage GetResponse(RequsetMessage message)
        {
            if (message == null)
            {
                return new ResponseMessage("CANNOT PARSE THE MESSAGE", 400);
            }
            Pipe p = _patterns.Find(x => x.RouteMatch(message.Method, message.Url));
            if (p != null)
            {
                return p.Proc(message);
            }
            else
            {
                string url = message.Url.StartsWith('/') ? message.Url.TrimStart('/') : message.Url;
                if (url == "")
                {
                    url = _sets.DefaultPage;
                }
                if (message.Header.ContainsKey("Range"))
                {
                    return ResponseMessage.TryGetRangeFileResponse(url, message.Header["Range"]);
                }
                else
                {
                    return ResponseMessage.TryGetFileResponse(url);
                }
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
