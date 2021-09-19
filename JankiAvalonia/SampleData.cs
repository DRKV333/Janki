using System.Collections.Generic;

namespace JankiAvalonia
{
    public class TreeLevel1
    {
        public string? Text { get; set; }
        public List<TreeLevel2>? Children { get; set; }
    }

    public class TreeLevel2
    {
        public string? Name { get; set; }
    }

    public static class SampleData
    {
        public static readonly IReadOnlyList<string> ListOfStrings = new List<string>()
        {
            "Item1",
            "This is long item, to test how well the ui can deal with those.",
            "Item2",
            "Item3",
            "Item4",
            "Item5"
        
        };

        public static readonly IReadOnlyList<TreeLevel1> ListOfTreeItems = new List<TreeLevel1>()
        {
            new()
            {
                Text = "First",
                Children = new()
                {
                    new() { Name = "Inner1" },
                    new() { Name = "Inner2" },
                    new() { Name = "Inner3" }
                }
            },
            new()
            {
                Text = "No Inner",
                Children = new() { }
            },
            new()
            {
                Text = "Second",
                Children = new()
                {
                    new() { Name = "Inner1" },
                    new() { Name = "Inner2" }
                }
            }
        };
    }
}