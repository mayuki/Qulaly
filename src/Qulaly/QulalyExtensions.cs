using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Qulaly.Matcher;

namespace Qulaly
{
    public static class QulalyExtensions
    {
        public static IEnumerable<SyntaxNode> QuerySelectorAll(this SyntaxNode node, string selector, Compilation? compilation = default)
        {
            var parsedSelector = QulalySelector.Parse(selector);
            return EnumerableMatcher.GetEnumerable(node, parsedSelector, compilation?.GetSemanticModel(node.SyntaxTree));
        }

        public static IEnumerable<SyntaxNode> QuerySelectorAll(this SyntaxNode node, QulalySelector selector, Compilation? compilation = default)
        {
            return EnumerableMatcher.GetEnumerable(node, selector, compilation?.GetSemanticModel(node.SyntaxTree));
        }

        public static SyntaxNode? QuerySelector(this SyntaxNode node, string selector, Compilation? compilation = default)
        {
            var parsedSelector = QulalySelector.Parse(selector);
            return EnumerableMatcher.GetEnumerable(node, parsedSelector, compilation?.GetSemanticModel(node.SyntaxTree)).FirstOrDefault();
        }

        public static SyntaxNode? QuerySelector(this SyntaxNode node, QulalySelector selector, Compilation? compilation = default)
        {
            return EnumerableMatcher.GetEnumerable(node, selector, compilation?.GetSemanticModel(node.SyntaxTree)).FirstOrDefault();
        }
    }
}