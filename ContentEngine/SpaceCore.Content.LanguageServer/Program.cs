using SpaceCore.Content.LanguageServer;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
try
{
    using var input = Console.OpenStandardInput();
    using var output = Console.OpenStandardOutput();
    var app = new App(input, output);
    app.Listen().Wait();
}
catch (Exception e)
{
    Console.Error.WriteLine("Exception: " + e);
}
