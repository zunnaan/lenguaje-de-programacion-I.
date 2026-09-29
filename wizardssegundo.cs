static class GameMaster
{
    public static string Describe(Character character)
    {
        return $"You're a level {character.Level} {character.Class} with {character.HitPoints} hit points.";
    }

    public static string Describe(Destination destination)
    {
        return $"You've arrived at {destination.Name}, which has {destination.Inhabitants} inhabitants.";
    }

    public static string Describe(TravelMethod travelMethod)
    {
        string method = travelMethod == TravelMethod.Horseback ? "on horseback" : "by walking";
        return $"You're traveling to your destination {method}.";
    }

    public static string Describe(Character character, Destination destination, TravelMethod travelMethod)
    {
        return $"{Describe(character)} {Describe(travelMethod)} {Describe(destination)}";
    }

    public static string Describe(Character character, Destination destination)
    {
        return Describe(character, destination, TravelMethod.Walking);
    }
}

class Character
{
    public string Class { get; set; } = "";
    public int Level { get; set; }
    public int HitPoints { get; set; }
}

class Destination
{
    public string Name { get; set; } = "";
    public int Inhabitants { get; set; }
}

enum TravelMethod
{
    Walking,
    Horseback
}

class Program
{
    static void Main()
    {
        var character = new Character { Class = "Wizard", Level = 4, HitPoints = 28 };
        var destination = new Destination { Name = "Muros", Inhabitants = 732 };

        Console.WriteLine(GameMaster.Describe(character));
        Console.WriteLine(GameMaster.Describe(destination));
        Console.WriteLine(GameMaster.Describe(TravelMethod.Horseback));
        Console.WriteLine(GameMaster.Describe(character, destination, TravelMethod.Horseback));
        Console.WriteLine(GameMaster.Describe(character, destination));
    }
}