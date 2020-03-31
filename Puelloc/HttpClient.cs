using System;
using System.Collections.Generic;
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
        private bool _running = false;
        private Thread _acceptThread = null;
        public void Listen()
        {
            _running = true;
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(_sets.BindIP);
            _socket.ReceiveTimeout = _sets.ReceiveTimeOut;
            _socket.SendTimeout = _sets.SendTimeOut;
            _socket.Listen(_sets.BackLog);
            _acceptThread = new Thread(() =>
            {
                while (_running)
                {
                    try
                    {
                        Socket a = _socket.Accept();
                        Task.Run(() => Accept(a));
                    }
                    catch(Exception e)
                    {
                        Log(e.Message);
                    }
                }
            }) {IsBackground = true};
            _acceptThread.Start();
        }

        public void Stop()
        {
            _running = false;
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch(Exception e)
            {
                Log(e.Message);
            }

            _socket.Close();
            while (_asPoolNum > 0)
            {
            }
        }

        private int _asPoolNum;

        private void Accept(Socket acceptSocket)
        {
            int aSid = _asPoolNum++;
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
            catch(SocketException se)
            {
                Log($"Socket:{se.Message}");
            }
            catch(Exception e)
            {
                Log(e.Message);
            }
            finally
            {
                acceptSocket.Close();
                Log($"Close Accept {aSid}");
                _asPoolNum--;
            }
        }

        private ResponseMessage GetResponse(RequsetMessage message)
        {
            if (message == null)
            {
                return new ResponseMessage("CANNOT PARSE THE MESSAGE",400);
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
                    url = "index.html";
                }
                return ResponseMessage.TryGetFileResponse(url);
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
