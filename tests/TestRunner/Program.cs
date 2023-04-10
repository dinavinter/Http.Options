// See https://aka.ms/new-console-template for more information

using System.Reflection;

Console.WriteLine("Hello, World!");

var currentAssembly = typeof(Program).Assembly.Location;
var assemblyDir = Path.GetDirectoryName(currentAssembly);
Directory.SetCurrentDirectory(assemblyDir);

var argsWithPath = args.Concat(new[] {currentAssembly}).ToArray();
var asm = Assembly.LoadFrom(Path.Combine(assemblyDir, @"vstest.console.dll"));

var program = asm.GetTypes().First(t => t.Name == "Program");
var main = program.GetMethods().First(m => m.Name == "Main");

return (int) main.Invoke(null, new object[] {argsWithPath})!;