using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Qulaly.Matcher.Selectors;
using Qulaly.Matcher.Selectors.Combinators;
using Qulaly.Syntax;
using Xunit;

namespace Qulaly.Tests
{
    public class SelectorSyntaxParserTests
    {
        [Fact]
        public void Invalid()
        {
            Assert.Throws<QulalyParseException>(() => new SelectorSyntaxParser().Parse(">"));
        }

        [Fact]
        public void IgnoreAroundWhitespaces()
        {
            var selector = new SelectorSyntaxParser().Parse("     ClassDeclaration         ");
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new TypeSelector(SyntaxKind.ClassDeclaration))).ToSelectorString());
        }

        [Fact]
        public void Complex_1()
        {
            var selector = new SelectorSyntaxParser().Parse(":class :method:not([Name='Foo'])  SwitchSection ObjectCreationExpression    > :is(PredefinedType, GenericName)[Name^='List'] ");
            var expected = new ComplexSelectorList(new ComplexSelector(
                new CompoundSelector(new ClassPseudoClassSelector()),
                new DescendantCombinator(),
                new CompoundSelector(new MethodPseudoClassSelector(), new NotPseudoClassSelector(new PropertyExactMatchSelector("Name", "Foo"))),
                new DescendantCombinator(),
                new CompoundSelector(new TypeSelector(SyntaxKind.SwitchSection)),
                new DescendantCombinator(),
                new CompoundSelector(new TypeSelector(SyntaxKind.ObjectCreationExpression)),
                new ChildCombinator(),
                new CompoundSelector(
                    new IsPseudoClassSelector(
                        new TypeSelector(SyntaxKind.PredefinedType),
                        new TypeSelector(SyntaxKind.GenericName)
                    ),
                    new PropertyPrefixMatchSelector("Name", "List")
                )
            ));

            selector.ToSelectorString().Should().Be(expected.ToSelectorString());
        }

        [Fact]
        public void PseudoNot()
        {
            var selector = new SelectorSyntaxParser().Parse(":not([Name='Foo'])");
            var expected = new ComplexSelectorList(new ComplexSelector(
                new NotPseudoClassSelector(new PropertyExactMatchSelector("Name", "Foo"))
            ));

            selector.ToSelectorString().Should().Be(expected.ToSelectorString());
        }

        [Fact]
        public void PseudoIs()
        {
            var selector = new SelectorSyntaxParser().Parse(":is(PredefinedType, GenericName, :not([Name='Foo']))");
            var expected = new ComplexSelectorList(new ComplexSelector(
                new IsPseudoClassSelector(
                    new TypeSelector(SyntaxKind.PredefinedType),
                    new TypeSelector(SyntaxKind.GenericName),
                    new NotPseudoClassSelector(new PropertyExactMatchSelector("Name", "Foo"))
                )
            ));

            selector.ToSelectorString().Should().Be(expected.ToSelectorString());
        }

        [Fact]
        public void Property_NameOnly()
        {
            var selector = new SelectorSyntaxParser().Parse("[Name]");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new PropertyNameSelector("Name"))).ToSelectorString());
        }

        [Fact]
        public void Property_Exact()
        {
            var selector = new SelectorSyntaxParser().Parse("[Name=A]");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new PropertyExactMatchSelector("Name", "A"))).ToSelectorString());
        }

        [Fact]
        public void Property_Exact_1()
        {
            var selector = new SelectorSyntaxParser().Parse("[Name='A']");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new PropertyExactMatchSelector("Name", "A"))).ToSelectorString());
        }

        [Fact]
        public void Property_ItemContainsMatchSelector()
        {
            var selector = new SelectorSyntaxParser().Parse("[Name ~= 'A']");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new PropertyItemContainsMatchSelector("Name", "A"))).ToSelectorString());
        }

        [Fact]
        public void PseudoClass_Class()
        {
            var selector = new SelectorSyntaxParser().Parse(":class");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new ClassPseudoClassSelector())).ToSelectorString());
        }

        [Fact]
        public void PseudoClass_Method()
        {
            var selector = new SelectorSyntaxParser().Parse(":method");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new MethodPseudoClassSelector())).ToSelectorString());
        }

        [Fact]
        public void PseudoClass_Interface()
        {
            var selector = new SelectorSyntaxParser().Parse(":interface");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new InterfacePseudoClassSelector())).ToSelectorString());
        }

        [Fact]
        public void PseudoClass_Lambda()
        {
            var selector = new SelectorSyntaxParser().Parse(":lambda");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new LambdaPseudoClassSelector())).ToSelectorString());
        }

        [Fact]
        public void TypeSelector_SyntaxKind_Invalid()
        {
            Assert.Throws<QulalyParseException>(() => new SelectorSyntaxParser().Parse("Unknown"));
        }

        [Fact]
        public void TypeSelector_SyntaxKind()
        {
            var selector = new SelectorSyntaxParser().Parse("ClassDeclaration");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(new ComplexSelectorList(new ComplexSelector(new TypeSelector(SyntaxKind.ClassDeclaration))).ToSelectorString());
        }

        [Fact]
        public void DescendantCombinator()
        {
            var selector = new SelectorSyntaxParser().Parse("ClassDeclaration MethodDeclaration");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(
                new ComplexSelectorList(
                    new ComplexSelector(new TypeSelector(SyntaxKind.ClassDeclaration), new DescendantCombinator(), new TypeSelector(SyntaxKind.MethodDeclaration))
                ).ToSelectorString());
        }

        [Fact]
        public void ChildCombinator()
        {
            var selector = new SelectorSyntaxParser().Parse("ClassDeclaration > MethodDeclaration");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(
                new ComplexSelectorList(
                    new ComplexSelector(new TypeSelector(SyntaxKind.ClassDeclaration), new ChildCombinator(), new TypeSelector(SyntaxKind.MethodDeclaration))
                ).ToSelectorString());
        }

        [Fact]
        public void NextSiblingCombinator()
        {
            var selector = new SelectorSyntaxParser().Parse("ClassDeclaration + MethodDeclaration");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(
                new ComplexSelectorList(
                    new ComplexSelector(new TypeSelector(SyntaxKind.ClassDeclaration), new NextSiblingCombinator(), new TypeSelector(SyntaxKind.MethodDeclaration))
                ).ToSelectorString());
        }

        [Fact]
        public void SubsequentSiblingCombinator()
        {
            var selector = new SelectorSyntaxParser().Parse("ClassDeclaration ~ MethodDeclaration");
            selector.Should().BeOfType<ComplexSelectorList>();
            selector.ToSelectorString().Should().Be(
                new ComplexSelectorList(
                    new ComplexSelector(new TypeSelector(SyntaxKind.ClassDeclaration), new SubsequentSiblingCombinator(), new TypeSelector(SyntaxKind.MethodDeclaration))
                ).ToSelectorString());
        }
    }
}
