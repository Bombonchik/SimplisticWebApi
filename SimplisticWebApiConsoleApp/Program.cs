using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

/*
    I spent a lot of time on this task :(
 */

class Program {
    static RequestProcessor requestProcessor = new RequestProcessor();

    static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        requestProcessor.RegisterAllRoutes();

        app.Run(ProcessRequest);
        app.UseHttpsRedirection();

        app.Run();
    }

    static Task ProcessRequest(HttpContext context) {
        var path = context.Request.Path.Value ?? "";
        var query = context.Request.QueryString.Value ?? "";
        var result = requestProcessor.HandleRequest(path, query);
        return context.Response.WriteAsync(result);
    }
}

class RequestProcessor {

    private RouteMap routes = new RouteMap();
    public void RegisterAllRoutes() {
        Assembly assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ISimplisticRoutesHandler)));
        foreach (var type in types)
        {
            var currentRoutesHandlerInstance = Activator.CreateInstance(type);
            ((ISimplisticRoutesHandler)currentRoutesHandlerInstance).RegisterRoutes(routes);
        }
        //find all implementations of interface ISimplisticRoutesHandler using reflection
        //create instance of that type using Activator.CreateInstance(Type) and call on it RegisterRoutes
    }

    public string HandleRequest(string path, string query) {
        Console.WriteLine($"+++ Thread #{Thread.CurrentThread.ManagedThreadId} processing request:");
        Console.WriteLine($"    Path =\"{path}\"");
        Console.WriteLine($"    Query=\"{query}\" ...");

        if (path == "/")
            return "Home page";

        if (routes.HasRoute(path))
        {
            var queryDictionary = QueryHelpers.ParseQuery(query);
            string[] args = new string[queryDictionary.Count];
            var values = queryDictionary.Values.ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                args[i] = values[i].ToString();
            }
            
            var result = routes.HandleRoute(path, args);
            return JsonSerializer.Serialize(result);
        }
        else
            Console.WriteLine($"!!! Route not found !!!");
        return "{}";
    }
}

public class RouteMap {
    Dictionary<string, Delegate> routeDelegate = new Dictionary<string, Delegate>();
    Dictionary<Type, object> objectInstances = new Dictionary<Type, object>();
    //for different args count, I couldn't do it another way.
    public void Map(string route, Func<IReadOnlyList<object>> handler)
    {
        var info = handler.GetMethodInfo();
        var declaringType = info.DeclaringType;
        if (!objectInstances.ContainsKey(declaringType))
            objectInstances[declaringType] = Activator.CreateInstance(info.DeclaringType);
        routeDelegate[route] = Delegate.CreateDelegate(typeof(Func<IReadOnlyList<object>>), objectInstances[declaringType], info);
    }
    public void Map(string route, Func<string, IReadOnlyList<object>> handler)
    {
        var info = handler.GetMethodInfo();
        var declaringType = info.DeclaringType;
        if (!objectInstances.ContainsKey(declaringType))
            objectInstances[declaringType] = Activator.CreateInstance(info.DeclaringType);
        routeDelegate[route] = Delegate.CreateDelegate(typeof(Func<string, IReadOnlyList<object>>), objectInstances[declaringType], info);
    }
    public void Map(string route, Func<string, string, IReadOnlyList<object>> handler)
    {
        var info = handler.GetMethodInfo();
        var declaringType = info.DeclaringType;
        if (!objectInstances.ContainsKey(declaringType))
            objectInstances[declaringType] = Activator.CreateInstance(info.DeclaringType);
        routeDelegate[route] = Delegate.CreateDelegate(typeof(Func<string, string, IReadOnlyList<object>>), objectInstances[declaringType], info);
    }
    public void Map(string route, Func<string, string, string, IReadOnlyList<object>> handler)
    {
        var info = handler.GetMethodInfo();
        var declaringType = info.DeclaringType;
        if (!objectInstances.ContainsKey(declaringType))
            objectInstances[declaringType] = Activator.CreateInstance(info.DeclaringType);
        routeDelegate[route] = Delegate.CreateDelegate(typeof(Func<string, string, string, IReadOnlyList<object>>), objectInstances[declaringType], info);
    }
    // ...
    public bool HasRoute(string route)
    {
        return routeDelegate.ContainsKey(route);
    }
    public IReadOnlyList<object> HandleRoute(string route, params string[] args)
    {
        return (IReadOnlyList<object>)routeDelegate[route].DynamicInvoke(args);
    }

}

public interface ISimplisticRoutesHandler {
    public void RegisterRoutes(RouteMap routeMap);
} 
