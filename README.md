# Puelloc

一个简陋级的HTTP服务端

## 快速上手

### 简单的例子

```csharp
class Program
{
    static void Main(string[] args)
    {
        Setting setting = new Setting();
        //默认绑定到自身网络环路的80端口
        HttpClient httpClient = new HttpClient(setting);
        httpClient.Listen();
        //注意:Listen方法是非阻塞的
        while(flag);
        httpClient.Stop();
    }
}
```

### 处理请求

```csharp
class Program
{
    static ResponseMessage Hello(RequsetMessage message){
        return new ResponseMessage("Hello World");
    }
    static void Main(string[] args)
    {
        Pipe hello = new Pipe((method,url)=>method=="GET"&&url=="/",Hello);
        //第一个参数是判断条件，第二参数是处理方法
        Setting setting = new Setting();
        HttpClient httpClient = new HttpClient(setting,hello);
        httpClient.Listen();
        while(flag);
        httpClient.Stop();
    }
}
```

使用正则：

```csharp
class Program
{
    static ResponseMessage Hello(RequsetMessage message){
        return new ResponseMessage("Hello World");
    }
    static void Main(string[] args)
    {
        Pipe hello = new Pipe("GET", new Regex(@"^/$", RegexOptions.Compiled), Hello);
        Setting setting = new Setting();
        HttpClient httpClient = new HttpClient(setting,hello);
        httpClient.Listen();
        while(flag);
        httpClient.Stop();
    }
}
```
