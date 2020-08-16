using System.Collections.Generic;
using System.Diagnostics;

namespace Qulaly.Syntax
{
    [DebuggerDisplay("Production: {Kind,nq}; Captures={Captures.Count,nq}; Children={Children.Count,nq}")]
    public class Production
    {
        public ProductionKind Kind { get; }

        public List<string> Captures { get; }
        public List<Production> Children { get; }

        public Production(ProductionKind kind)
        {
            Kind = kind;
            Captures = new List<string>();
            Children = new List<Production>();
        }
    }

    public enum ProductionKind
    {
        Root,
        WqName,
        ComplexSelectorList,
        ComplexSelector,
        Combinator,
        CompoundSelector,
        SimpleSelector,
        SimpleSelectorList,
        SubclassSelector,
        NotPseudoClassSelector,
        NotPseudoClassSelectorValue,
        IsPseudoClassSelector,
        IsPseudoClassSelectorValue,
        HasPseudoClassSelector,
        HasPseudoClassSelectorValue,
        PseudoElementSelector,
        PseudoClassSelector,
        Expression,
        Nth,
        IdSelector,
        TypeSelector,
        NsPrefix,
        ClassSelector,
        AttributeSelector,
        AttributeSelectorQulalyExtensionNumber,
    }
}
