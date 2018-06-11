using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FastCGINet
{
    class Program
    {
        static Dictionary<string, string> cached;
        static string rootPath;
        static string basePath;
        static void Main(string[] args)
        {
            cached = new Dictionary<string, string>();
            basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(basePath + "/cache"))
                Directory.CreateDirectory(basePath + "/cache");

            if (args != null && args.Length > 0)
            {
                rootPath = args[0];
                Reader();
                Console.ReadKey();
            }
            else
            {
                rootPath = "";
                Reader();
                Console.ReadKey();
            }
        }

        static void Reader()
        {
            do
            {
                string cmd = Console.ReadLine();
                if (cmd.Contains(".cs"))
                {
                    int index = cmd.IndexOf(".cs");
                    string path = rootPath + cmd.Substring(0, index + 3);
                    if (File.Exists(path))
                    {
                        try
                        {
                            string[] args = null;
                            if (cmd.Length > index + 3)
                            {
                                string argLine = cmd.Substring(index + 3);
                                args = argLine.Split(' ');
                            }

                            if (!TryCached(path, args))
                            {
                                string sourceCode = File.ReadAllText(path);

                                if (!Executor(path, sourceCode, args))
                                    Console.WriteLine("Couldn't excute script!");
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Error during script execution: " + ex.Message);
                        }
                        Console.WriteLine("<EOF>");
                    }
                    else
                    {
                        Console.WriteLine("Script not found " + path);
                        Console.WriteLine("<EOF>");
                    }
                }
            } while (true);
        }

        static Dictionary<string, string> providerOptions = new Dictionary<string, string>
                {
                    {"CompilerVersion", "v3.5"}
                };

        static bool TryCached(string path, string[] args = null)
        {
            if(cached.ContainsKey(path))
            {
                if (File.Exists(cached[path]))
                {
                    Assembly asm = Assembly.LoadFile(cached[path]);
                    asm.EntryPoint.Invoke(null, new object[] { args });
                    return true;
                }
            }
            return false;
        }

        static bool Executor(string fullPath, string sourceCode, string[] args = null)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);
            string path = basePath + "/cache/" + Path.GetFileNameWithoutExtension(fullPath) + ".compiled";
            CompilerParameters compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = true,
                OutputAssembly = path,
            };

            CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, sourceCode);
            if (results.Errors.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    sb.Append("[ScriptError] " + results.Errors[i].ErrorText + " Line:" + results.Errors[i].Line.ToString() + ";");
                }
                Console.WriteLine(sb.ToString());
                return false;
            }

            if(!cached.ContainsKey(fullPath))
                cached.Add(fullPath, path);
            results.CompiledAssembly.EntryPoint.Invoke(null, new object[] { args });
            return true;
        }
    }
}
