using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Qulaly.Tests
{
    public class MatcherTests
    {
        [Fact]
        public void Complex_1()
        {
            var selector = QulalySelector.Parse(":class SwitchSection ObjectCreationExpression > :is(PredefinedType, GenericName, IdentifierName):not([Name^='List']) ");
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp22
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
        }

        public static async ValueTask<T> Test<T>(int a, string b, T c)
        {
            return default;
        }

        object Bar(int key)
        {
            switch (key)
            {
                case 0: return new object();
                case 1: return new List<int>();
                case 2: return new Program();
            }
            return null;
        }

        object Foo(int key)
        {
            switch (key)
            {
                case 0: return new int();
                case 1: return new List<string>();
                case 2: return new Exception();
            }
            return null;
        }
    }

    public readonly struct AStruct
    {}

    [MyNantoka]
    public class BClass<T>
        where T: class
    {}

    public class MyNantokaAttribute : Attribute { }
}
");

            var compilation = CSharpCompilation.Create("Test")
                    .AddSyntaxTrees(syntaxTree);

            var root = syntaxTree.GetCompilationUnitRoot();
            var matches = root.QuerySelectorAll(selector, compilation).ToArray();
            matches.Should().HaveCount(4); // object, Program, int, Exception
            matches.Select(x => x.ToString()).Should().ContainInOrder("object", "Program", "int", "Exception");
        }

        [Fact]
        public void Method()
        {
            var selector = QulalySelector.Parse(":method");
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp22
{
    public class Program
    {
        static void Main(string[] args)
        {
        }

        public static async ValueTask<T> Test<T>(int a, string b, T c)
        {
        }

        object Bar(int key)
        {
        }

        object Foo(int key)
        {
        }
    }
    public class Class1
    {
        object Bar(int key) => throw new NotImplementedException();

        object Foo(int key) => throw new NotImplementedException();
    }
}
");

            var compilation = CSharpCompilation.Create("Test")
                .AddSyntaxTrees(syntaxTree);

            var root = syntaxTree.GetCompilationUnitRoot();
            var matches = root.QuerySelectorAll(selector, compilation).ToArray();
            matches.Should().HaveCount(6);
            matches.OfType<MethodDeclarationSyntax>().Select(x => x.Identifier.ToString()).Should().ContainInOrder("Main", "Test", "Bar", "Foo", "Bar", "Foo");
        }

        [Fact]
        public void Class()
        {
            var selector = QulalySelector.Parse(":class");
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp22
{
    public class Program
    {
        static void Main(string[] args)
        {
        }

        public static async ValueTask<T> Test<T>(int a, string b, T c)
        {
        }

        object Bar(int key) => throw new NotImplementedException();
        object Foo(int key) => throw new NotImplementedException();
    }
    public class Class1
    {
        object Bar(int key) => throw new NotImplementedException();
        object Foo(int key) => throw new NotImplementedException();
    }
}
");

            var compilation = CSharpCompilation.Create("Test")
                .AddSyntaxTrees(syntaxTree);

            var root = syntaxTree.GetCompilationUnitRoot();
            var matches = root.QuerySelectorAll(selector, compilation).ToArray();
            matches.Should().HaveCount(2);
            matches.OfType<ClassDeclarationSyntax>().Select(x => x.Identifier.ToString()).Should().ContainInOrder("Program", "Class1");
        }

        [Fact]
        public void PropertyCount()
        {
            var selector = QulalySelector.Parse(":method:has(ParameterList[Count > 1])"); // the method has two or more parameters.
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp22
{
    public class Program
    {
        static void Main(string[] args)
        {
        }

        public static async ValueTask<T> Test<T>(int a, string b, T c)
        {
        }

        object Bar(int key)
        {
        }

        object Foo(int key)
        {
        }
    }
    public class Class1
    {
        object MethodA(int arg1) => throw new NotImplementedException();
        object MethodB(int arg1, string arg2) => throw new NotImplementedException();
    }
}
");

            var compilation = CSharpCompilation.Create("Test")
                .AddSyntaxTrees(syntaxTree);

            var root = syntaxTree.GetCompilationUnitRoot();
            var matches = root.QuerySelectorAll(selector, compilation).ToArray();
            matches.Should().HaveCount(2);
            matches.OfType<MethodDeclarationSyntax>().Select(x => x.Identifier.ToString()).Should().ContainInOrder("Test", "MethodB");
        }

        [Fact]
        public void PropertyCount_TypeParameter_Count()
        {
            var selector = QulalySelector.Parse(":method[TypeParameters.Count > 0]"); // the method has one or more type-parameters.
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp22
{
    public class Program
    {
        static void Main(string[] args)
        {
        }

        public static async ValueTask<T> Test<T>(int a, string b, T c)
        {
        }

        object Bar(int key)
        {
        }

        object Foo(int key)
        {
        }
    }
    public class Class1
    {
        object MethodA(int arg1) => throw new NotImplementedException();
        object MethodB(int arg1, string arg2) => throw new NotImplementedException();
    }
}
");

            var compilation = CSharpCompilation.Create("Test")
                .AddSyntaxTrees(syntaxTree);

            var root = syntaxTree.GetCompilationUnitRoot();
            var matches = root.QuerySelectorAll(selector, compilation).ToArray();
            matches.Should().HaveCount(1);
            matches.OfType<MethodDeclarationSyntax>().Select(x => x.Identifier.ToString()).Should().ContainInOrder("Test");
        }

        [Fact]
        public void PseudoClass_FirstChild()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp22
{
    public class Program
    {
        static void Main(string[] args) => throw new NotImplementedException();
        public static async ValueTask<T> Test<T>(int a, string b, T c) => throw new NotImplementedException();
        object Bar(int key) => throw new NotImplementedException();
        object Foo(int key) => throw new NotImplementedException();
    }
    public class Class1
    {
        object MethodA(int arg1) => throw new NotImplementedException();
        object MethodB(int arg1, string arg2) => throw new NotImplementedException();
    }
}
");

            var firsts = syntaxTree.QuerySelectorAll(":method:first-child").ToArray();
            firsts.Should().HaveCount(2);
            firsts.OfType<MethodDeclarationSyntax>().Select(x => x.Identifier.ToFullString()).Should().ContainInOrder("Main", "MethodA");
        }

        [Fact]
        public void PseudoClass_LastChild()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp22
{
    public class Program
    {
        static void Main(string[] args) => throw new NotImplementedException();
        public static async ValueTask<T> Test<T>(int a, string b, T c) => throw new NotImplementedException();
        object Bar(int key) => throw new NotImplementedException();
        object Foo(int key) => throw new NotImplementedException();
    }
    public class Class1
    {
        object MethodA(int arg1) => throw new NotImplementedException();
        object MethodB(int arg1, string arg2) => throw new NotImplementedException();
    }
}
");

            var lasts = syntaxTree.QuerySelectorAll(":method:last-child").ToArray();
            lasts.Should().HaveCount(2);
            lasts.OfType<MethodDeclarationSyntax>().Select(x => x.Identifier.ToFullString()).Should().ContainInOrder("Foo", "MethodB");
        }

    }
}
