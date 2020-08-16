using System;
using System.Linq;

namespace Qulaly.Syntax
{
    public partial class SelectorSyntaxReader
    {
        private readonly string _selector;
        private ProductionScope _productionScope;

        public int Index { get; private set; }
        public Production? CurrentProduction { get; private set; }

        public SelectorSyntaxReader(string selector)
        {
            _selector = selector;
            _productionScope = new ProductionScope();
            Index = 0;
            CurrentProduction = default;
        }

        public bool Production(ProductionKind kind, Func<bool> next)
        {
            var node = new Production(kind);
            var parent = CurrentProduction;
            var parentScope = _productionScope;

            _productionScope = new ProductionScope();
            CurrentProduction = node;
            try
            {
                var accepted = next();
                if (accepted)
                {
                    parentScope.AddProduction(node);
                }

                return accepted;
            }
            finally
            {
                node.Children.AddRange(_productionScope.GetProductions());
                node.Captures.AddRange(_productionScope.GetCaptures());
                CurrentProduction = parent;
                _productionScope = parentScope;
            }
        }

        public Func<bool> Capture(Func<bool> func)
        {
            return () =>
            {
                var origIndex = Index;
                var result = func();
                if (result)
                {
                    var capturedString = _selector.Substring(origIndex, Index - origIndex);
                    if (CurrentProduction == null)
                    {
                        throw new InvalidOperationException();
                    }

                    _productionScope.AddCapture(capturedString);
                    //CurrentProduction.Captures.Add(capturedString);
                }
                return result;
            };
        }

        public char ReadNext()
        {
            return _selector[Index++];
        }

        public char PeekNext()
        {
            return _selector[Index + 1];
        }

        public bool HasNext()
        {
            return Index <= _selector.Length - 1;
        }

        public Func<bool> Chars(string s)
        {
            return () =>
            {
                for (var i = 0; i < s.Length; i++)
                {
                    if (!HasNext())
                        return false;

                    var nextChar = char.ToLower(ReadNext());
                    if (s[i] != nextChar)
                        return false;
                }
                return true;
            };
        }

        public Func<bool> Char(char c, bool isNegative = false)
        {
            return () =>
            {
                if (!HasNext())
                    return false;

                var nextChar = char.ToLower(ReadNext());
                return isNegative
                            ? (c != nextChar)
                            : (c == nextChar);
            };
        }

        public Func<bool> Char(char[] c, bool isNegative = false)
        {
            return () =>
            {
                if (!HasNext())
                    return false;

                var nextChar = char.ToLower(ReadNext());
                return isNegative
                            ? (c.All(x => x != nextChar))
                            : (c.Any(x => x == nextChar));
            };
        }
        public Func<bool> CharRange(char start, char end, bool isNegative = false)
        {
            return () =>
            {
                if (!HasNext())
                    return false;

                var nextChar = char.ToLower(ReadNext());
                var isContain = (start <= nextChar && end >= nextChar);
                return isNegative
                            ? !isContain
                            : isContain;
            };
        }

        public bool Nmstart()
        {
            // nmstart   [_a-z]|{nonascii}|{escape}
            return Expect(Char('_'), CharRange('a', 'z'), NonAscii, Escape);
        }
        public bool Nmchar()
        {
            // nmchar    [_a-z0-9-]|{nonascii}|{escape}
            return Expect(Char('_'), CharRange('a', 'z'), Char('-'), Num, NonAscii, Escape);
        }
        public bool Num()
        {
            // num       [0-9]+|[0-9]*\.[0-9]+
            // TODO:
            return ExpectOneOrMore(CharRange('0', '9'));
        }
        public bool NonAscii()
        {
            // nonascii  [^\0-\177]
            return CharRange('\0', '\xB1', true)();
        }
        public bool Unicode()
        {
            // unicode   \\[0-9a-f]{1,6}(\r\n|[ \n\r\t\f])?
            return Expect(Char('\\')) && ExpectOneOrMore(() => Expect(CharRange('0', '9'), CharRange('a', 'f')), 6)
                && ExpectZeroOrOne(() => Expect(Chars("\r\n")) || Expect(Char(' '), Char('\n'), Char('\t'), Char('\f')));
        }

        public bool Escape()
        {
            // escape    {unicode}|\\[^\n\r\f0-9a-f]
            return Expect(Unicode, () => Expect(Char('\\')) && Expect(() =>
            {
                if (!HasNext())
                    return false;
                var nextChar = char.ToLower(ReadNext());
                return !(
                    nextChar == '\n'
                    || nextChar == '\r'
                    || nextChar == '\f'
                    || (nextChar >= '0' && nextChar <= '9')
                    || (nextChar >= 'a' && nextChar <= 'f')
                  );
            }));
        }

        public bool IdentToken()
        {
            // https://www.w3.org/TR/css-syntax-3/#ident-token-diagram
            return Expect(Chars("--"), () => ExpectZeroOrOne(Char('-')) && Expect(Char('_'), CharRange('a', 'z'), NonAscii, Escape))
                && ExpectZeroOrMore(() => Expect(Char('_'), CharRange('a', 'z'), CharRange('A', 'Z'), CharRange('0', '9'), NonAscii, Escape));
        }

        public bool Space()
        {
            // [ \t\r\n\f]+     return S;
            return ExpectOneOrMore(() => Expect(Char(' '), Char('\t'), Char('\n'), Char('\f')));
        }

        public bool W()
        {
            // w         [ \t\r\n\f]*
            return ExpectZeroOrMore(() => Expect(Char(' '), Char('\t'), Char('\n'), Char('\f')));
        }

        public bool Plus()
        {
            // {w}"+"           return PLUS;
            return W() && Expect(Char('+'));
        }

        public bool Greater()
        {
            // {w}">"           return GREATER;
            return W() && Expect(Char('>'));
        }

        public bool Tilde()
        {
            // {w}"~"           return TILDE;
            return W() && Expect(Char('~'));
        }

        public bool Comma()
        {
            // {w}","           return COMMA;
            return W() && Expect(Char(','));
        }

        public bool MultiComma(Func<bool> next)
        {
            // https://www.w3.org/TR/css-values-4/#mult-comma
            return Expect(next) &&
                   ExpectZeroOrMore(() => ExpectZeroOrMore(Space) && Expect(Char(',')) && ExpectZeroOrMore(Space) && Expect(next));
        }

        public bool HashToken()
        {
            // hash-token
            // https://www.w3.org/TR/css-syntax-3/#hash-token-diagram
            return Expect(Char('#'))
                && Expect(CharRange('a', 'z'), CharRange('A', 'Z'), CharRange('0', '9'), Char('_'), Char('-'), NonAscii, Escape);
        }

        public bool Name()
        {
            // name      {nmchar}+
            return ExpectOneOrMore(Nmchar);
        }

        public bool Dimension()
        {
            return false;
        }

        public bool Number()
        {
            // {num}            return NUMBER;
            return Num();
        }

        public bool String()
        {
            // string    {string1}|{string2}
            return Expect(String1, String2);
        }

        public bool String1()
        {
            // string1   \"([^\n\r\f\\"]|\\{nl}|{nonascii}|{escape})*\"
            return Expect(Char('"'))
                && ExpectZeroOrMore(() => Expect(
                        Char(new[] { '\n', '\r', '\f', '\\', '"' }, true),
                        () => Expect(Char('\\')) && Expect(Nl),
                        NonAscii,
                        Escape
                    ))
                && Expect(Char('"'));
        }

        public bool String2()
        {
            // string2   \'([^\n\r\f\\']|\\{nl}|{nonascii}|{escape})*\'
            return Expect(Char('\''))
                && ExpectZeroOrMore(() => Expect(
                        Char(new[] { '\n', '\r', '\f', '\\', '\'' }, true),
                        () => Expect(Char('\\')) && Expect(Nl),
                        NonAscii,
                        Escape
                    ))
                && Expect(Char('\''));
        }

        public bool Nl()
        {
            // nl        \n|\r\n|\r|\f
            return Expect(Char('\n'), () => Expect(Char('\r'), Char('\n')), Char('\r'), Char('\f'));
        }

        public bool FunctionToken()
        {
            // <function-token>
            // https://www.w3.org/TR/css-syntax-3/#function-token-diagram
            return Expect(Capture(IdentToken))
                && Expect(Char('('));
        }

        public bool D()
        {
            // D         d|\\0{0,4}(44|64)(\r\n|[ \t\r\n\f])?
            return Expect(Char('d'), Char('D'),
                          () => Char('\\')()
                                  && ExpectZeroOrMore(Char('0'), 4)
                                  && Expect(Chars("44"), Chars("64"))
                                  && ExpectZeroOrOne(() => Expect(Char(' '), Char('\r'), Char('\n'), Char('\r'), Char('\f'))));
        }
        public bool E()
        {
            // E         e|\\0{0,4}(45|65)(\r\n|[ \t\r\n\f])?
            return Expect(Char('e'), Char('E'),
                          () => Char('\\')()
                                  && ExpectZeroOrMore(Char('0'), 4)
                                  && Expect(Chars("45"), Chars("65"))
                                  && ExpectZeroOrOne(() => Expect(Char(' '), Char('\r'), Char('\n'), Char('\r'), Char('\f'))));
        }
        public bool N()
        {
            // N         n|\\0{0,4}(4e|6e)(\r\n|[ \t\r\n\f])?|\\n
            return Expect(Char('n'), Char('N'),
                          () => Char('\\')()
                                  && ExpectZeroOrMore(Char('0'), 4)
                                  && Expect(Chars("4e"), Chars("6e"))
                                  && ExpectZeroOrOne(() => Expect(Char(' '), Char('\r'), Char('\n'), Char('\r'), Char('\f'))),
                          (Chars("\\n")), Chars("\\N"));
        }
        public bool O()
        {
            // O         o|\\0{0,4}(4f|6f)(\r\n|[ \t\r\n\f])?|\\o
            return Expect(Char('o'), Char('O'),
                          () => Char('\\')()
                                  && ExpectZeroOrMore(Char('0'), 4)
                                  && Expect(Chars("4f"), Chars("6f"))
                                  && ExpectZeroOrOne(() => Expect(Char(' '), Char('\r'), Char('\n'), Char('\r'), Char('\f'))),
                          (Chars("\\o")), Chars("\\O"));
        }
        public bool T()
        {
            // T         t|\\0{0,4}(54|74)(\r\n|[ \t\r\n\f])?|\\t
            return Expect(Char('t'), Char('T'),
                          () => Char('\\')()
                                  && ExpectZeroOrMore(Char('0'), 4)
                                  && Expect(Chars("54"), Chars("74"))
                                  && ExpectZeroOrOne(() => Expect(Char(' '), Char('\r'), Char('\n'), Char('\r'), Char('\f'))),
                          (Chars("\\t")), Chars("\\T"));
        }
        public bool V()
        {
            // V         v|\\0{0,4}(58|78)(\r\n|[ \t\r\n\f])?|\\v
            return Expect(Char('v'), Char('V'),
                          () => Char('\\')()
                                  && ExpectZeroOrMore(Char('0'), 4)
                                  && Expect(Chars("58"), Chars("78"))
                                  && ExpectZeroOrOne(() => Expect(Char(' '), Char('\r'), Char('\n'), Char('\r'), Char('\f'))),
                          (Chars("\\v")), Chars("\\V"));
        }

        public bool Not()
        {
            // ":"{N}{O}{T}"("  return NOT;
            return Expect(Char(':')) && Expect(N) && Expect(O) && Expect(T) && Expect(Char('('));
        }

        public bool PrefixMatch()
        {
            // "^="             return PREFIXMATCH;
            return Expect(Chars("^="));
        }
        public bool SuffixMatch()
        {
            // "$="             return SUFFIXMATCH;
            return Expect(Chars("$="));
        }
        public bool SubstringMatch()
        {
            // "*="             return SUBSTRINGMATCH;
            return Expect(Chars("*="));
        }
        public bool Includes()
        {
            // "~="             return INCLUDES;
            return Expect(Chars("~="));
        }
        public bool DashMatch()
        {
            // "|="             return DASHMATCH;
            return Expect(Chars("|="));
        }
    }
}
