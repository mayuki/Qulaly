using Microsoft.CodeAnalysis.CSharp;

namespace Qulaly.Matcher.Selectors
{
    public class TypeSelector : Selector
    {
        public SyntaxKind Kind { get; }

        public TypeSelector(SyntaxKind kind)
        {
            Kind = kind;
        }

        // Type Selector
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node.Kind() == Kind;
        }

        public override string ToSelectorString()
        {
            return Kind.ToString();
        }
    }

    public class UniversalTypeSelector : Selector
    {

        public UniversalTypeSelector()
        {
        }

        // Type Selector
        public override SelectorMatcher GetMatcher()
        {
            // Wildcard selector
            return (in SelectorMatcherContext ctx) => true;
        }

        public override string ToSelectorString()
        {
            return "*";
        }
    }
}
