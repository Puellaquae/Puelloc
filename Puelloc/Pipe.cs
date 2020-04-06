using System;
using System.Text.RegularExpressions;

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
            RouteMatch = (requsetMethod, requsetUrl) => requsetMethod == method && requsetUrl.StartsWith(urlStart);
            Proc = proc;
        }

        public Pipe(string method, Regex urlRegaxPattern, Func<RequsetMessage,ResponseMessage> proc)
        {
            RouteMatch = (requsetMethod, requsetUrl) => requsetMethod == method && urlRegaxPattern.IsMatch(requsetUrl);
            Proc = proc;
        }
    }
}
