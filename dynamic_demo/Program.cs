// See https://aka.ms/new-console-template for more information
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;


Action<string> Write = Console.WriteLine;


var jsonString = File.ReadAllText("demo.json");
var jObject = JObject.Parse(jsonString);

Dictionary<string, string> classDicts = new Dictionary<string, string>();
classDicts.Add("Root", GetClassDefinion(jObject));
foreach (var item in jObject.Properties())
{
    classDicts.Add(item.Name, GetClassDefinion(item.Value));
    GetClasses(item.Value, classDicts);
}
StringBuilder sb = new StringBuilder(1024);
sb.AppendLine("using System;");
sb.AppendLine("using System.Collections.Generic;");
sb.AppendLine("namespace RoslynCompileSample");
sb.AppendLine("{");
foreach (var item in classDicts)
{
    sb.Append($"public class {item.Key}" + Environment.NewLine);
    sb.Append("{" + Environment.NewLine);
    sb.Append(item.Value);
    sb.Append("}" + Environment.NewLine);
}
sb.AppendLine("}");
Write(sb.ToString());

Write("Let's compile!");
Write("Parsing the code into the SyntaxTree");
SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sb.ToString());

string assemblyName = Path.GetRandomFileName();
var refPaths = new[] {
                typeof(object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")
            };
MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

Write("Adding the following references");
foreach (var r in refPaths)
    Write(r);

Write("Compiling ...");
CSharpCompilation compilation = CSharpCompilation.Create(
    assemblyName,
    syntaxTrees: new[] { syntaxTree },
    references: references,
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

using (var ms = new MemoryStream())
{
    EmitResult result = compilation.Emit(ms);

    if (!result.Success)
    {
        Write("Compilation failed!");
        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error);

        foreach (Diagnostic diagnostic in failures)
        {
            Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
        }
    }
    else
    {
        Write("Compilation successful! Now instantiating and executing the code ...");
        ms.Seek(0, SeekOrigin.Begin);

        Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
        var type = assembly.GetType("RoslynCompileSample.Root");
        var instance = assembly.CreateInstance("RoslynCompileSample.Root");

        //var meth = type.GetMember("Write").First() as MethodInfo;
        //meth.Invoke(instance, new[] { "joel" });

        var deserializeObject = typeof(JsonConvert).GetGenericMethod("DeserializeObject", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(JsonSerializerSettings) });
        var genericDeserializeObject = deserializeObject.MakeGenericMethod(type);

        var root = genericDeserializeObject.Invoke(null, new object[] { jsonString, null });
        Console.WriteLine(JsonConvert.SerializeObject(root, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
    }
}

void GetClasses(JToken jToken, Dictionary<string, string> classes)
{
    if (jToken is JValue)
    {
        return;
    }
    var childToken = jToken.First;
    while (childToken != null)
    {
        if (childToken.Type == JTokenType.Property)
        {
            var p = (JProperty)childToken;
            var valueType = p.Value.Type;

            if (valueType == JTokenType.Object)
            {
                classes.Add(p.Name, GetClassDefinion(p.Value));
                GetClasses(p.Value, classes);
            }
            else if (valueType == JTokenType.Array)
            {
                foreach (var item in (JArray)p.Value)
                {
                    if (item.Type == JTokenType.Object)
                    {
                        if (!classes.ContainsKey(p.Name))
                        {
                            classes.Add(p.Name, GetClassDefinion(item));
                        }

                        GetClasses(item, classes);
                    }
                }
            }
        }

        childToken = childToken.Next;
    }
}

string GetClassDefinion(JToken jToken)
{
    StringBuilder sb = new(256);
    var subValueToken = jToken.First();
    while (subValueToken != null)
    {
        if (subValueToken.Type == JTokenType.Property)
        {
            var p = (JProperty)subValueToken;
            var valueType = p.Value.Type;
            if (valueType == JTokenType.Object)
            {
                sb.Append("public " + p.Name + " " + p.Name + " {get;set;}" + Environment.NewLine);
            }
            else if (valueType == JTokenType.Array)
            {
                var arr = (JArray)p.Value;
                //a.First

                switch (arr.First().Type)
                {
                    case JTokenType.Object:
                        sb.Append($"public List<{p.Name}> " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    case JTokenType.Integer:
                        sb.Append($"public List<int> " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    case JTokenType.Float:
                        sb.Append($"public List<float> " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    case JTokenType.String:
                        sb.Append($"public List<string> " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    case JTokenType.Boolean:
                        sb.Append($"public List<bool> " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (valueType)
                {
                    case JTokenType.Integer:
                        sb.Append($"public int " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    case JTokenType.Float:
                        sb.Append($"public float " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    case JTokenType.String:
                        sb.Append($"public string " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    case JTokenType.Boolean:
                        sb.Append($"public bool " + p.Name + " {get;set;}" + Environment.NewLine);
                        break;
                    default:
                        break;
                }
            }
        }

        subValueToken = subValueToken.Next;
    }

    return sb.ToString();
}

public static class Extension
{
    public static MethodInfo GetGenericMethod(this Type targetType, string name, BindingFlags flags, params Type[] parameterTypes)
    {
        var methods = targetType.GetMethods(flags).Where(m => m.Name == name && m.IsGenericMethod);
        var flag = false;
        foreach (MethodInfo method in methods)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != parameterTypes.Length)
                continue;

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != parameterTypes[i])
                {
                    break;
                }
                if (i == parameters.Length - 1)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                return method;
            }
        }
        return null;
    }
}