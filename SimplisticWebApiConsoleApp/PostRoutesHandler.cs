public class PostRoutesHandler : ISimplisticRoutesHandler {
    
    public void RegisterRoutes(RouteMap routeMap) {
        Console.WriteLine("+++ PostRoutesHandler.RegisterRoutes() called.");
        
        // TODO: Has to compile without any changes !!!
        routeMap.Map("/services/PostCode/getDataAsJson", GetPostCodesByCityAndStreet);
    }

    public IReadOnlyList<PostCodeItem> GetPostCodesByCityAndStreet(string cityOrPart, string nameStreet) {
        return cityOrPart switch {
            "Praha" => nameStreet switch {
                "Patkova" => new[] {   
                        new PostCodeItem("Praha", "Pátkova", "Praha 82", "18200", "2136/3"),
                    },
                "Malostranske namesti" => new[] {
                        new PostCodeItem("Praha", "Malostranské náměstí", "Praha 011", "11800", "2/25"),
                        new PostCodeItem("Praha", "Malostranské náměstí", "Praha 011", "11800", "203/14")
                    },
                _ => Array.Empty<PostCodeItem>()
            },
            "Sokolov" => nameStreet switch {
                "Karla Hynka Machy" => new[] {
                    new PostCodeItem("Sokolov", "Karla Hynka Máchy", "Sokolov 1", "35601", "1100"),
                    new PostCodeItem("Sokolov", "Karla Hynka Máchy", "Sokolov 1", "35601", "2230"),
                    new PostCodeItem("Sokolov", "Karla Hynka Máchy", "Sokolov 1", "35601", "2256")
                },
                _ => Array.Empty<PostCodeItem>()
            },
            _ => Array.Empty<PostCodeItem>()
        };
    }
}

public record PostCodeItem(string NameCity, string NameStreet, string Name, string PostCode, string Number);

public record TestItem(string testName1, string testName2, string testName3);
