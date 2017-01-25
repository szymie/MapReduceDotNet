using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;


namespace Server.Services
{
    [Route ("/api/hello")]
    [Route ("/api/hello/{Name}")]
    public class Hello : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }

    public class HelloService : Service
    {
        public object Any (Hello request)
        {
            return new HelloResponse {Result = "Hello, " + request.Name};
        }
    }
}