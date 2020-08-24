using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp;
using Qulaly;

namespace Qulaly.Web.Playground.Pages
{
    public partial class Index
    {
        private CancellationTokenSource _pendingQueryCancellation;
        private string sourceCode = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Class1
{
    public static async ValueTask<T> FooAsync<T>(int a, string b, T c) => throw new NotImplementedException();
    public async Task BarAsync<T>() => throw new NotImplementedException();
}

public class Class2
{
    public object MethodA(int arg1) => throw new NotImplementedException();
    public object MethodB(int arg1, string arg2) => throw new NotImplementedException();
}
        ".Trim();
        private string query = ":class :method:has(Parameter[Name^='a'])";
        private string output;
        private Microsoft.CodeAnalysis.SyntaxTree syntaxTree;
        private HashSet<Microsoft.CodeAnalysis.SyntaxNode> syntaxNodeMatches = new HashSet<Microsoft.CodeAnalysis.SyntaxNode>();

        private RenderFragment<Microsoft.CodeAnalysis.SyntaxTree> RenderSyntaxTree;
        private RenderFragment<Microsoft.CodeAnalysis.SyntaxNode> RenderSyntaxNode;
        private RenderFragment<Microsoft.CodeAnalysis.SyntaxToken> RenderSyntaxToken;

        private void UpdateSource(ChangeEventArgs e)
        {
            try
            {
                sourceCode = e.Value?.ToString();

                ParseSource();
                ExecuteQuery();
            }
            catch (Exception ex)
            {
                output = ex.ToString();
            }
        }

        private void UpdateQuery(ChangeEventArgs e)
        {
            _pendingQueryCancellation?.Cancel();

            query = e.Value?.ToString();
            if (syntaxTree == null) return;

            var currentCancellation = _pendingQueryCancellation = new CancellationTokenSource();
            if (currentCancellation.IsCancellationRequested) return;

            ExecuteQuery();
        }

        private void ParseSource()
        {
            if (string.IsNullOrWhiteSpace(sourceCode)) return;
            syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        }

        private void ExecuteQuery()
        {
            output = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    var matches = syntaxTree.QuerySelectorAll(query).ToArray();
                    syntaxNodeMatches.Clear();
                    syntaxNodeMatches.UnionWith(matches);
                }
            }
            catch (QulalyParseException ex)
            {
                output = ex.Message;
            }
            catch (Exception ex)
            {
                output = ex.ToString();
            }
        }

        private string GetNodePath(Microsoft.CodeAnalysis.SyntaxNode node)
        {
            if (node == null) return "";
            var parent = GetNodePath(node.Parent);

            return (string.IsNullOrEmpty(parent) ? "" : $"{parent} → ") + node.Kind();
        }

        private string GetTokenPath(Microsoft.CodeAnalysis.SyntaxToken token)
        {
            var parent = GetNodePath(token.Parent);
            return (string.IsNullOrEmpty(parent) ? "" : $"{parent} → ") + token.Kind();
        }
    }
}
