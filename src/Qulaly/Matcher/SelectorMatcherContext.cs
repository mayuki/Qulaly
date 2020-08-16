using System;
using Microsoft.CodeAnalysis;

namespace Qulaly.Matcher
{
    public readonly struct SelectorMatcherContext
    {
        public SyntaxNode Node { get; }
        public SemanticModel? SemanticModel { get; }

        public SelectorMatcherContext(SyntaxNode node, SemanticModel? semanticModel)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            SemanticModel = semanticModel;
        }

        public SelectorMatcherContext WithSyntaxNode(SyntaxNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            return new SelectorMatcherContext(node, SemanticModel);
        }
    }
}