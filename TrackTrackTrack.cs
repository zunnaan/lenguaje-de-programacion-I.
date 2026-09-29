public static class Languages
{
    public static List<string> NewList()
    {
        return new List<string>();
    }

    public static List<string> GetExistingLanguages()
    {
        return new List<string> { "C#", "Clojure", "Elm" };
    }

    public static List<string> AddLanguage(List<string> languages, string language)
    {
        languages.Add(language);
        return languages;
    }

    public static int CountLanguages(List<string> languages)
    {
        return languages.Count;
    }

    public static bool HasLanguage(List<string> languages, string language)
    {
        return languages.Contains(language);
    }

    public static List<string> ReverseList(List<string> languages)
    {
        languages.Reverse();
        return languages;
    }

    public static bool IsExciting(List<string> languages)
    {
        if (languages.Count == 0) return false;
        if (languages[0] == "C#") return true;

        return languages.Count >= 2 && languages.Count <= 3 && languages[1] == "C#";
    }

    public static List<string> RemoveLanguage(List<string> languages, string language)
    {
        languages.Remove(language);
        return languages;
    }

    public static bool IsUnique(List<string> languages)
    {
        return languages.Distinct().Count() == languages.Count;
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine(Languages.NewList().Count); 
        Console.WriteLine(string.Join(", ", Languages.GetExistingLanguages()));          
        Console.WriteLine(string.Join(", ", Languages.AddLanguage(Languages.GetExistingLanguages(), "VBA")));
        Console.WriteLine(Languages.CountLanguages(Languages.GetExistingLanguages()));     
        Console.WriteLine(Languages.HasLanguage(Languages.GetExistingLanguages(), "Elm"));  
        Console.WriteLine(string.Join(", ", Languages.ReverseList(Languages.GetExistingLanguages()))); 
        Console.WriteLine(Languages.IsExciting(Languages.GetExistingLanguages()));              
        Console.WriteLine(string.Join(", ", Languages.RemoveLanguage(Languages.GetExistingLanguages(), "Clojure"))); 
        Console.WriteLine(Languages.IsUnique(Languages.GetExistingLanguages()));               
    }
}
