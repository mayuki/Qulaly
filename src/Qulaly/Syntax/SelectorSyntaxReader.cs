namespace Qulaly.Syntax
{
    public partial class SelectorSyntaxReader
    {
        public Production? GetRoot()
        {
            if (Production(ProductionKind.Root, () => ExpectZeroOrMore(Space) && Expect(ComplexSelectorList) && ExpectZeroOrMore(Space)))
            {
                return _productionScope.GetProductions()[0];
            }

            return default;
        }

        public bool ComplexSelectorList()
        {
            // <complex-selector-list> = <complex-selector>#
            return Production(ProductionKind.ComplexSelectorList, () =>
            {
                return Expect(() => MultiComma(ComplexSelector));
            });
        }

        public bool ComplexSelector()
        {
            // <complex-selector> = <compound-selector> [ <combinator>? <compound-selector> ]*
            return Production(ProductionKind.ComplexSelector, () =>
            {
                return Expect(CompoundSelector) && ExpectZeroOrMore(() => ExpectZeroOrOne(Combinator) && Expect(CompoundSelector));
            });
        }

        public bool CompoundSelector()
        {
            // <compound-selector> = [ <type-selector>? <subclass-selector>*
            //                       [ <pseudo-element-selector> <pseudo-class-selector>* ]* ]!
            return Production(ProductionKind.CompoundSelector, () =>
            {
                return ExpectOneOrMore(() =>
                {
                    return ExpectZeroOrOne(TypeSelector, out var firstIsZero) && ExpectZeroOrMore(SubclassSelector, out var secondIsZero) &&
                           ExpectZeroOrMore(() => Expect(PseudoElementSelector) && ExpectZeroOrMore(PseudoClassSelector), out var thirdIsZero) &&
                           !(firstIsZero && secondIsZero && thirdIsZero); // one or more
                });
            });
        }

        public bool SimpleSelectorList()
        {
            // <simple-selector-list> = <simple-selector>#
            return Production(ProductionKind.SimpleSelectorList, () =>
            {
                return MultiComma(SimpleSelector);
            });
        }

        public bool SimpleSelector()
        {
            // <simple-selector> = <type-selector> | <subclass-selector>
            // https://www.w3.org/TR/selectors-4/#typedef-simple-selector
            return Production(ProductionKind.SimpleSelector, () =>
            {
                return Expect(TypeSelector, SubclassSelector);
            });
        }

        public bool SubclassSelector()
        {
            // <subclass-selector> = <id-selector> | <class-selector> |
            //                       <attribute-selector> | <pseudo-class-selector>
            // https://www.w3.org/TR/selectors-4/#typedef-subclass-selector
            return Production(ProductionKind.SubclassSelector, () =>
            {
                return Expect(IdSelector, ClassSelector, AttributeSelectorQulalyExtensionNumber, AttributeSelector, PseudoClassSelector);
            });
        }

        public bool PseudoElementSelector()
        {
            // <pseudo-element-selector> = ':' <pseudo-class-selector>
            return Production(ProductionKind.PseudoElementSelector, () =>
            {
                return Expect(() => Expect(Char(':')) && Expect(PseudoClassSelector));
            });
        }

        public bool PseudoClassSelector()
        {
            // <pseudo-class-selector> = ':' <ident-token> |
            //                           ':' <function-token> <any-value> ')' |
            //                           ':' <not-pseudo-class-selector>
            //                           ':' <is-pseudo-class-selector>
            //                           ':' <has-pseudo-class-selector>
            return Production(ProductionKind.PseudoClassSelector, () =>
            {
                return Expect(
                    () => Expect(Char(':')) && Expect(IsPseudoClassSelector),
                    () => Expect(Char(':')) && Expect(HasPseudoClassSelector),
                    () => Expect(Char(':')) && Expect(NotPseudoClassSelector),
                    () => Expect(Char(':')) && Expect(FunctionToken) && Expect(Expression) && Expect(Char(')')),
                    () => Expect(Char(':')) && Expect(Capture(IdentToken))
                );
            });
        }

        public bool NotPseudoClassSelector()
        {
            // <not-pseudo-class-selector> = 'not' '(' <not-pseudo-class-selector-value> ')'
            return Production(ProductionKind.NotPseudoClassSelector, () =>
            {
                return Expect(Chars("not"))
                       && ExpectZeroOrMore(Space)
                       && Expect(Char('('))
                       && ExpectZeroOrMore(Space)
                       && Expect(NotPseudoClassSelectorValue)
                       && ExpectZeroOrMore(Space)
                       && Expect(Char(')'));
            });
        }

        public bool NotPseudoClassSelectorValue()
        {
            return Production(ProductionKind.NotPseudoClassSelectorValue, () =>
            {
                return Expect(CompoundSelector);
            });
        }

        public bool IsPseudoClassSelector()
        {
            // <is-pseudo-class-selector> = 'is' '(' <is-pseudo-class-selector-value> ')'
            return Production(ProductionKind.IsPseudoClassSelector, () =>
            {
                return Expect(Chars("is"))
                       && ExpectZeroOrMore(Space)
                       && Expect(Char('('))
                       && ExpectZeroOrMore(Space)
                       && Expect(IsPseudoClassSelectorValue)
                       && ExpectZeroOrMore(Space)
                       && Expect(Char(')'));
            });
        }

        public bool IsPseudoClassSelectorValue()
        {
            return Production(ProductionKind.IsPseudoClassSelectorValue, () =>
            {
                return Expect(ComplexSelectorList);
            });
        }

        public bool HasPseudoClassSelector()
        {
            // <has-pseudo-class-selector> = 'has' '(' <has-pseudo-class-selector-value> ')'
            return Production(ProductionKind.HasPseudoClassSelector, () =>
            {
                return Expect(Chars("has"))
                       && ExpectZeroOrMore(Space)
                       && Expect(Char('('))
                       && ExpectZeroOrMore(Space)
                       && Expect(HasPseudoClassSelectorValue)
                       && ExpectZeroOrMore(Space)
                       && Expect(Char(')'));
            });
        }

        public bool HasPseudoClassSelectorValue()
        {
            return Production(ProductionKind.HasPseudoClassSelectorValue, () =>
            {
                return Expect(ComplexSelectorList);
            });
        }

        public bool IdSelector()
        {
            // <id-selector> = <hash-token>
            return Production(ProductionKind.IdSelector, () =>
            {
                return Expect(Capture(HashToken));
            });
        }

        public bool Combinator()
        {
            // <combinator> = '>' | '+' | '~'
            return Production(ProductionKind.Combinator, () =>
            {
                return Expect(
                    () => Expect(() => ExpectZeroOrMore(Space) && Expect(Capture(() => Expect(Char('>'), Char('+'), Char('~')))) && ExpectZeroOrMore(Space)),
                    () => Expect(Capture(() => ExpectOneOrMore(Space)))
                );
            });
        }

        public bool TypeSelector()
        {
            // <type-selector> = <wq-name> | <ns-prefix>? '*'
            return Production(ProductionKind.TypeSelector, () =>
            {
                return Expect(WqName, () => ExpectZeroOrOne(NsPrefix) && Expect(Capture(Char('*'))));
            });
        }

        public bool NsPrefix()
        {
            // <ns-prefix> = [ <ident-token> | '*' ]? '|'
            return Production(ProductionKind.NsPrefix, () =>
            {
                return ExpectZeroOrOne(Capture(() => Expect(IdentToken, Char('*'))))
                    && Expect(Char('|'));
            });
        }

        public bool ClassSelector()
        {
            // <class-selector> = '.' <ident-token>
            return Production(ProductionKind.ClassSelector, () =>
            {
                return Expect(Char('.'))
                    && Expect(Capture(IdentToken));
            });
        }

        public bool AttributeSelector()
        {
            // <attribute-selector> = '[' <wq-name> ']' |
            //                        '[' <wq-name> <attr-matcher> [ <string-token> | <ident-token> ] <attr-modifier>? ']'
            return Production(ProductionKind.AttributeSelector, () =>
            {
                return Expect(() => Expect(Char('[')) && ExpectZeroOrMore(Space) && Expect(WqName) && ExpectZeroOrMore(Space) && Expect(Char(']')),
                    () => Expect(Char('['))
                          && ExpectZeroOrMore(Space)
                          && Expect(WqName)
                          && ExpectZeroOrMore(Space)
                          && Expect(() =>
                              Capture(AttrMatcher)()
                              && ExpectZeroOrMore(Space)
                              && Expect(Capture(() => Expect(IdentToken, String)))
                              && ExpectZeroOrMore(() => ExpectZeroOrMore(Space) && Expect(Capture(() => Expect(Char('i'))))) // attr-modifier
                              && ExpectZeroOrMore(Space)
                          )
                          && Expect(Char(']'))
                );
            });
        }

        public bool AttributeSelectorQulalyExtensionNumber()
        {
            return Production(ProductionKind.AttributeSelectorQulalyExtensionNumber, () =>
            {
                // Qulaly Extensions: [Name > 1]
                return Expect(() => Expect(Char('['))
                          && ExpectZeroOrMore(Space)
                          && Expect(PropertyNameChainQulalyExtension)
                          && ExpectZeroOrMore(Space)
                          && Expect(() =>
                              Capture(AttrMatcherQulalyExtension)()
                              && ExpectZeroOrMore(Space)
                              && Expect(Capture(() => Expect(Number)))
                              && ExpectZeroOrMore(Space)
                          )
                          && Expect(Char(']'))
                );
            });
        }

        public bool AttrMatcher()
        {
            // <attr-matcher> = [ '~' | '|' | '^' | '$' | '*' ]? '='
            return Expect(() => ExpectZeroOrOne(() => Expect(Char('~'), Char('|'), Char('^'), Char('$'), Char('*'))) && Expect(Char('=')));
        }
        public bool AttrMatcherQulalyExtension()
        {
            // Qulaly Extension = ['<' | '<=' | '>' | '>=' ]
            return Expect(Chars("<="), Chars(">="), Chars("<"), Chars(">"), Chars("="));
        }

        public bool WqName()
        {
            return Production(ProductionKind.WqName, () =>
            {
                return ExpectZeroOrOne(NsPrefix) && Expect(Capture(IdentToken));
            });
        }

        public bool PropertyNameChainQulalyExtension()
        {
            return Production(ProductionKind.PropertyNameChainQulalyExtension, () =>
            {
                // <ident-token> [ '.' <ident-token> ]*
                return Expect(Capture(() => Expect(IdentToken) && ExpectZeroOrMore(() => Expect(Char('.')) && Expect(IdentToken))));
            });
        }

        public bool Expression()
        {
            // expression
            //   /* In CSS3, the expressions are identifiers, strings, */
            //   /* or of the form "an+b" */
            //   : [ [ PLUS | '-' | DIMENSION | NUMBER | STRING | IDENT ] S* ]+
            //   ;
            return Production(ProductionKind.Expression, () =>
            {
                return ExpectOneOrMore(Capture(() => Expect(Plus, Char('-'), Dimension, Number, String, IdentToken) && ExpectZeroOrMore(Space)));
            });
        }

        public bool Nth()
        {
            // nth
            //   : S* [ ['-'|'+']? INTEGER? {N} [ S* ['-'|'+'] S* INTEGER ]? |
            //          ['-'|'+']? INTEGER | {O}{D}{D} | {E}{V}{E}{N} ] S*
            //   ;
            return Production(ProductionKind.Nth, () =>
            {
                return ExpectZeroOrMore(Space)
                    && Expect(
                        () => ExpectZeroOrOne(Char(new[] { '-', '+' }))
                           && ExpectOneOrMore(CharRange('0', '9'))
                           && Expect(N)
                           && ExpectZeroOrOne(() => ExpectZeroOrMore(Space) && Char(new[] { '-', '+' })() && ExpectZeroOrMore(Space) && ExpectOneOrMore(CharRange('0', '9'))),
                        () => ExpectZeroOrOne(Char(new[] { '-', '+' }))
                           && ExpectOneOrMore(CharRange('0', '9')),
                        () => Expect(O) && Expect(D) && Expect(D),
                        () => Expect(E) && Expect(V) && Expect(E) && Expect(N)
                    )
                    && ExpectZeroOrMore(Space);
            });
        }
    }
}
