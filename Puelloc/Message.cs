using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Puelloc
{
    public class Pipe
    {
        public Func<string, string, bool> RouteMatch { get; }
        public Func<RequsetMessage, ResponseMessage> Proc { get; }

        public Pipe(Func<string, string, bool> route, Func<RequsetMessage, ResponseMessage> proc)
        {
            RouteMatch = route;
            Proc = proc;
        }

        public Pipe(string method, string urlStart, Func<RequsetMessage, ResponseMessage> proc)
        {
            RouteMatch = (requsetmethod, requseturl) => requsetmethod == method && requseturl.StartsWith(urlStart);
            Proc = proc;
        }
    }

    public abstract class Message
    {
        public byte[] Content { get; protected internal set; }

        public string Text
        {
            get => Encoding.UTF8.GetString(Content);
            set => Content = Encoding.UTF8.GetBytes(value);
        }

        public string ProtocolVersion { get; protected internal set; } = "HTTP/1.1";
        public HttpHeader Header { get; protected internal set; } = new HttpHeader();

        public byte[] ToBytes()
        {
            string firstLine = this switch
            {
                ResponseMessage response =>
                $"{response.ProtocolVersion} {response.Status} {response.StatusMessage}\r\n",
                RequsetMessage requset => $"{requset.Method} {requset.Url} {requset.ProtocolVersion}\r\n",
                _ => throw new Exception()
            };
            List<byte> bres = new List<byte>(Encoding.UTF8.GetBytes(firstLine));
            if (Content != null)
            {
                Header.AddHeader("Content-Length", Content.Length.ToString());
                bres.AddRange(Encoding.UTF8.GetBytes(Header.ToString()));
                bres.AddRange(Content);
            }
            else
            {
                bres.AddRange(Encoding.UTF8.GetBytes(Header.ToString()));
            }

            return bres.ToArray();
        }

        public static Message Parse(string message)
        {
            List<string> method = new List<string>
                {"GET", "POST", "PUT", "HEAD", "DELETE", "OPTIONS", "TRACE", "CONNECT"};
            string[] headerBody = message.Split(new[] { "\r\n\r\n" }, 2, StringSplitOptions.RemoveEmptyEntries);
            string[] messageLines = headerBody[0].Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] firstLine = messageLines[0].Split(' ');
            if (firstLine.Length != 3)
            {
                return null;
            }

            if (firstLine[0].StartsWith("HTTP"))
            {
                ResponseMessage response = new ResponseMessage()
                {
                    ProtocolVersion = firstLine[0],
                    Status = int.Parse(firstLine[1]),
                };
                if (headerBody.Length == 2)
                {
                    response.Text = headerBody[1];
                }

                for (int i = 1; i < messageLines.Length; i++)
                {
                    response.Header.AddHeader(messageLines[i]);
                }

                return response;
            }

            if (!method.Contains(firstLine[0])) return null;
            {
                RequsetMessage requset = new RequsetMessage
                {
                    Method = firstLine[0],
                    Url = WebUtility.UrlDecode(firstLine[1]),
                    ProtocolVersion = firstLine[2]
                };
                if (headerBody.Length == 2)
                {
                    requset.Text = headerBody[1];
                }

                for (int i = 1; i < messageLines.Length; i++)
                {
                    requset.Header.AddHeader(messageLines[i]);
                }

                return requset;
            }
        }
    }

    public class RequsetMessage : Message
    {
        public string Method { get; protected internal set; }
        public string Url { get; protected internal set; }

        public Dictionary<string, string> UrlQuerys
        {
            get
            {
                string[] temp = Url.Split('?');
                if (temp.Length == 2)
                {
                    string query = temp[1];
                    return query.Split('&').Select(pair => pair.Split('='))
                        .ToDictionary(keyvalue => keyvalue[0], keyvalue => keyvalue[1]);
                }
                else
                {
                    return new Dictionary<string, string>();
                }
            }
        }
    }

    public class ResponseMessage : Message
    {
        public int Status
        {
            get;
            protected internal set;
        }

        public string StatusMessage => HttpResponseStatusCodes.GetName(Status);

        protected internal ResponseMessage() { }
        public ResponseMessage(int status)
        {
            Status = status;
        }
        public ResponseMessage(string text, int status = 200)
        {
            Status = status;
            Text = text;
            Header.AddHeader("Content-Type", "text/plain; charset=utf-8");
        }
        internal static string BasePath { get; set; }
        public static ResponseMessage TryGetFileResponse(string filePath)
        {
            if (!Path.IsPathFullyQualified(filePath))
            {
                filePath = BasePath == null ? Path.GetFullPath(filePath) : Path.GetFullPath(filePath, BasePath);
            }
            if (!File.Exists(filePath))
            {
                return new ResponseMessage($"NOT FOUND {filePath}", 404);
            }
            else
            {
                string extension = Path.GetExtension(filePath);
                return new ResponseMessage(filePath, MIMEs.TryParse(extension));
            }
        }
        public ResponseMessage(string absoluteFilePath, MIME mime, int status = 200)
        {
            if (!File.Exists(absoluteFilePath))
            {
                Status = 404;
                Text = $"NOT FOUND {absoluteFilePath}";
                Header.AddHeader("Content-Type", "text/plain; charset=utf-8");
                return;
            }
            Status = status;
            using (FileStream stream = new FileInfo(absoluteFilePath).OpenRead())
            {
                Content = new byte[stream.Length];
                stream.Read(Content, 0, Convert.ToInt32(stream.Length));
                stream.Close();
            }
            Header.AddHeader("Content-Type", mime.ToString());
        }
        public static ResponseMessage TryGetRangeFileResponse(string filePath, int start, int end)
        {
            if (!Path.IsPathFullyQualified(filePath))
            {
                filePath = BasePath == null ? Path.GetFullPath(filePath) : Path.GetFullPath(filePath, BasePath);
            }
            if (!File.Exists(filePath))
            {
                return new ResponseMessage($"NOT FOUND {filePath}", 404);
            }
            else
            {
                string extension = Path.GetExtension(filePath);
                return new ResponseMessage(filePath, start, end, MIMEs.TryParse(extension));
            }
        }
        public ResponseMessage(string absoluteFilePath, int rangeStart, int rangeEnd, MIME mime, int status = 206)
        {
            if (!File.Exists(absoluteFilePath))
            {
                Status = 404;
                Text = $"NOT FOUND {absoluteFilePath}";
                Header.AddHeader("Content-Type", "text/plain; charset=utf-8");
                return;
            }
            Status = status;
            using FileStream stream = new FileInfo(absoluteFilePath).OpenRead();
            byte[] buffer = new byte[stream.Length];
            int len = (int)stream.Length;
            rangeStart = rangeStart == -1 ? 0 : rangeStart;
            rangeEnd = rangeEnd == -1 ? len - 1 : rangeEnd;
            if (rangeStart <= rangeEnd && rangeEnd < len)
            {
                stream.Read(buffer, rangeStart, rangeEnd - rangeStart);
                Content = buffer;
                Header.AddHeader("Content-Type", mime.ToString());
                Header.AddHeader("Content-Length", Content.Length.ToString());
                Header.AddHeader("Content-Range", $"bytes {rangeStart}-{rangeEnd}/{len}");
            }
            else
            {
                Header.AddHeader("Content-Range", $"*/{len}");
                Status = 416;
            }
        }
    }
}
