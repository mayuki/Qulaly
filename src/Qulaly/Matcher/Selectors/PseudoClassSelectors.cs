using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Qulaly.Matcher.Selectors
{
    public abstract class PseudoClassSelector : Selector
    { }

    /// <summary>
    /// 4.3. The Negation (Matches-None) Pseudo-class: :not()
    /// </summary>
    public class NotPseudoClassSelector : PseudoClassSelector
    {
        private readonly Selector _selector;

        public NotPseudoClassSelector(Selector selector)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        public override SelectorMatcher GetMatcher()
        {
            var matcher = _selector.GetMatcher();

            return (in SelectorMatcherContext ctx) =>
            {
                return !matcher(ctx);
            };
        }

        public override string ToSelectorString()
        {
            return $":not({_selector.ToSelectorString()})";
        }
    }

    /// <summary>
    /// 4.5. The Relational Pseudo-class: :has()
    /// </summary>
    public class HasPseudoClassSelector : PseudoClassSelector
    {
        private readonly Selector[] _relativeSelectors;

        public HasPseudoClassSelector(params Selector[] relativeSelectors)
        {
            _relativeSelectors = relativeSelectors ?? throw new ArgumentNullException(nameof(relativeSelectors));
        }

        public override SelectorMatcher GetMatcher()
        {
            SelectorMatcher matcher = (in SelectorMatcherContext _) => false;
            foreach (var selector in _relativeSelectors)
            {
                matcher = SelectorCompilerHelper.ComposeOr(matcher, selector.GetMatcher());
            }

            var query = new QulalySelector(matcher, this);

            return (in SelectorMatcherContext ctx) =>
            {
                return ctx.Node.QuerySelector(query) != null;
            };
        }

        public override string ToSelectorString()
        {
            return $":has({string.Join(",", _relativeSelectors.Select(x => x.ToSelectorString()))})";
        }
    }

    /// <summary>
    /// 4.2. The Matches-Any Pseudo-class: :is()
    /// </summary>
    public class IsPseudoClassSelector : PseudoClassSelector
    {
        private readonly Selector[] _selectors;

        public IsPseudoClassSelector(params Selector[] selectors)
        {
            _selectors = selectors ?? throw new ArgumentNullException(nameof(selectors));
        }

        public override SelectorMatcher GetMatcher()
        {
            SelectorMatcher matcher = (in SelectorMatcherContext _) => false;
            foreach (var selector in _selectors)
            {
                matcher = SelectorCompilerHelper.ComposeOr(matcher, selector.GetMatcher());
            }

            return matcher;
        }

        public override string ToSelectorString()
        {
            return $":is({string.Join(",", _selectors.Select(x => x.ToSelectorString()))})";
        }
    }


    public class MethodPseudoClassSelector : PseudoClassSelector
    {
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node.IsKind(SyntaxKind.MethodDeclaration);
        }

        public override string ToSelectorString()
        {
            return ":method";
        }
    }

    public class StatementPseudoClassSelector : PseudoClassSelector
    {
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node is StatementSyntax;
        }

        public override string ToSelectorString()
        {
            return ":statement";
        }
    }

    public class LambdaPseudoClassSelector : PseudoClassSelector
    {
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node is LambdaExpressionSyntax;
        }

        public override string ToSelectorString()
        {
            return ":lambda";
        }
    }

    public class StructPseudoClassSelector : PseudoClassSelector
    {
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node is StructDeclarationSyntax;
        }

        public override string ToSelectorString()
        {
            return ":struct";
        }
    }

    public class ClassPseudoClassSelector : PseudoClassSelector
    {
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node is ClassDeclarationSyntax;
        }

        public override string ToSelectorString()
        {
            return ":class";
        }
    }

    public class InterfacePseudoClassSelector : PseudoClassSelector
    {
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node is InterfaceDeclarationSyntax;
        }

        public override string ToSelectorString()
        {
            return ":interface";
        }
    }

    public class FirstChildPseudoClassSelector : PseudoClassSelector
    {
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node == ctx.Node.Parent.ChildNodes().First();
        }

        public override string ToSelectorString()
        {
            return ":first-child";
        }
    }

    public class LastChildPseudoClassSelector : PseudoClassSelector
    {
        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) => ctx.Node == ctx.Node.Parent.ChildNodes().Last();
        }

        public override string ToSelectorString()
        {
            return ":last-child";
        }
    }
}
