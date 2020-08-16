using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Qulaly.Matcher;

namespace Qulaly
{
    public static class QulalyExtensions
    {
        public static IEnumerable<SyntaxNode> QuerySelectorAll(this SyntaxTree syntaxTree, string selector, Compilation? compilation = default)
        {
            return QuerySelectorAll(syntaxTree.GetRoot(), selector, compilation);
        }

        public static IEnumerable<SyntaxNode> QuerySelectorAll(this SyntaxNode node, string selector, Compilation? compilation = default)
        {
            return QuerySelectorAll(node, QulalySelector.Parse(selector), compilation);
        }

        public static IEnumerable<SyntaxNode> QuerySelectorAll(this SyntaxTree syntaxTree, QulalySelector selector, Compilation? compilation = default)
        {
            return QuerySelectorAll(syntaxTree.GetRoot(), selector, compilation);
        }

        public static IEnumerable<SyntaxNode> QuerySelectorAll(this SyntaxNode node, QulalySelector selector, Compilation? compilation = default)
        {
            return EnumerableMatcher.GetEnumerable(node, selector, compilation?.GetSemanticModel(node.SyntaxTree));
        }

        public static SyntaxNode? QuerySelector(this SyntaxTree syntaxTree, string selector, Compilation? compilation = default)
        {
            return QuerySelector(syntaxTree.GetRoot(), selector, compilation);
        }

        public static SyntaxNode? QuerySelector(this SyntaxNode node, string selector, Compilation? compilation = default)
        {
            return QuerySelector(node, QulalySelector.Parse(selector), compilation);
        }

        public static SyntaxNode? QuerySelector(this SyntaxTree syntaxTree, QulalySelector selector, Compilation? compilation = default)
        {
            return QuerySelector(syntaxTree.GetRoot(), selector, compilation);
        }

        public static SyntaxNode? QuerySelector(this SyntaxNode node, QulalySelector selector, Compilation? compilation = default)
        {
            return EnumerableMatcher.GetEnumerable(node, selector, compilation?.GetSemanticModel(node.SyntaxTree)).FirstOrDefault();
        }
    }
}
