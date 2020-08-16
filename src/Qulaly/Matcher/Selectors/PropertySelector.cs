using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Qulaly.Matcher.Selectors
{
    public abstract class PropertySelector : Selector
    {
        public string PropertyName { get; }

        public PropertySelector(string propertyName)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        }

        protected string? GetFriendlyName(in SelectorMatcherContext ctx)
        {
            return ctx.Node switch
            {
                AttributeSyntax attrSyntax => attrSyntax.Name.ToString(),
                MethodDeclarationSyntax methodDeclSyntax => methodDeclSyntax.Identifier.ToString(),
                TypeDeclarationSyntax typeDeclSyntax => typeDeclSyntax.Identifier.ToString(),
                ParameterSyntax paramSyntax => paramSyntax.Identifier.ToString(),
                NameSyntax nameSyntax => nameSyntax.ToString(),
                _ => default,
            };
        }

        protected string? GetPropertyValue(in SelectorMatcherContext ctx)
        {
            if (PropertyName == "Name")
            {
                return GetFriendlyName(ctx);
            }

            var prop = ctx.Node.GetType().GetProperty(PropertyName);
            return prop?.GetValue(ctx.Node)?.ToString();
        }
    }


    public class PropertyNameSelector : PropertySelector
    {
        public PropertyNameSelector(string propertyName)
            : base(propertyName)
        {
        }

        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) =>
            {
                var prop = ctx.Node.GetType().GetProperty(PropertyName);
                return prop != null;
            };
        }

        public override string ToSelectorString()
        {
            return $"[{PropertyName}]";
        }
    }

    public class PropertyExactMatchSelector : PropertySelector
    {
        public string Value { get; }

        public PropertyExactMatchSelector(string propertyName, string value)
            : base(propertyName)
        {
            Value = value;
        }

        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) =>
            {
                return GetPropertyValue(ctx)?.Equals(Value) ?? false;
            };
        }

        public override string ToSelectorString()
        {
            return $"[{PropertyName}='{Value}']";
        }
    }

    public class PropertyPrefixMatchSelector : PropertySelector
    {
        public string Value { get; }

        public PropertyPrefixMatchSelector(string propertyName, string value)
            : base(propertyName)
        {
            Value = value;
        }

        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) =>
            {
                return GetPropertyValue(ctx)?.StartsWith(Value) ?? false;
            };
        }

        public override string ToSelectorString()
        {
            return $"[{PropertyName}^='{Value}']";
        }
    }

    public class PropertySuffixMatchSelector : PropertySelector
    {
        public string Value { get; }

        public PropertySuffixMatchSelector(string propertyName, string value)
            : base(propertyName)
        {
            Value = value;
        }

        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) =>
            {
                return GetPropertyValue(ctx)?.EndsWith(Value) ?? false;
            };
        }

        public override string ToSelectorString()
        {
            return $"[{PropertyName}$='{Value}']";
        }
    }

    public class PropertySubstringMatchSelector : PropertySelector
    {
        public string Value { get; }

        public PropertySubstringMatchSelector(string propertyName, string value)
            : base(propertyName)
        {
            Value = value;
        }

        public override SelectorMatcher GetMatcher()
        {
            return (in SelectorMatcherContext ctx) =>
            {
                return GetPropertyValue(ctx)?.Contains(Value) ?? false;
            };
        }

        public override string ToSelectorString()
        {
            return $"[{PropertyName}*='{Value}']";
        }
    }

    public class PropertyItemContainsMatchSelector : PropertySelector
    {
        public string Value { get; }

        public PropertyItemContainsMatchSelector(string propertyName, string value)
            : base(propertyName)
        {
            Value = value;
        }

        public override SelectorMatcher GetMatcher()
        {
            if (Value == string.Empty) return (in SelectorMatcherContext ctx) => false;

            return (in SelectorMatcherContext ctx) =>
            {
                if (PropertyName == "Modifiers" && ctx.Node is MemberDeclarationSyntax memberDeclarationSyntax)
                {
                    return memberDeclarationSyntax.Modifiers.Any(x => x.ValueText == Value);
                }

                return GetPropertyValue(ctx)?.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(Value) ?? false;
            };
        }

        public override string ToSelectorString()
        {
            return $"[{PropertyName}~='{Value}']";
        }
    }
}