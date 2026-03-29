namespace SpellCheckingTool.Presentation;

public static class LicensePrinter
{
    public static void Print()
    {
        Console.WriteLine("This application uses the UK Advanced Cryptics Dictionary for a predefined word list under the following license:");
        Console.WriteLine("Copyright © J Ross Beresford 1993-1999. All Rights Reserved.");
        Console.WriteLine("Visit the 'UK Advanced Cryptics Dictionary' project at: https://diginoodles.com/projects/eowl");
    }
}