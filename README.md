# Qulaly
**Qu**ery **la**nguage for Ros**ly**n. Qulaly is a library that queries Roslyn's C# syntax tree with CSS selector-like syntax. Inspired by [esquery](https://github.com/estools/esquery) in ECMAScript ecosystem.

[![NuGet version](https://badge.fury.io/nu/Qulaly.svg)](https://www.nuget.org/packages/Qulaly)
[![Build-Development](https://github.com/mayuki/Qulaly/workflows/Build-Development/badge.svg)](https://github.com/mayuki/Qulaly/actions?query=workflow%3ABuild-Development)

## Example
The following code shows how to query the `async` method.

```csharp
using Qulaly;

var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Class1
{
    public static async ValueTask<T> FooAsync<T>(int a, string b, T c) => throw new NotImplementedException();
    public async Task BarAsync<T>() => throw new NotImplementedException();
    public object MethodA(int arg1) => throw new NotImplementedException();
    public object MethodB(int arg1, string arg2) => throw new NotImplementedException();
}
");

// Enumerate SyntaxNodes by calling `QuerySelectorAll` extension method for SyntaxNode/SyntaxTree.
foreach (var methodNode in syntaxTree.QuerySelectorAll(":method[Modifiers ~= 'async']"))
{
    Console.WriteLine(((MethodDeclarationSyntax)methodNode).Identifier.ToFullString());
}
```
### Output
```
FooAsync
BarAsync
```

## Methods
- `QuerySelectorAll`
- `QuerySelector`

## Install
Install NuGet package from NuGet.org

```bash
$ dotnet add package Qulaly
```

```powershell
PS> Install-Package Qulaly
```

## Supported Selectors
Qulaly supports a subset of [CSS selector level 4](https://www.w3.org/TR/selectors-4/). The selector engine also supports Qulaly-specific extensions to the selector.

- SyntaxNode Type: `MethodDeclaration`, `ClassDeclaration` ... 
    - See also [SyntaxKind enum](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.syntaxkind?view=roslyn-dotnet)
- SyntaxNode Univarsal: `*`
- SyntaxNode pseudo-classes (for short-hand)
    - `:method`
    - `:class`
    - `:interface`
    - `:lambda`
- Combinators
    - [Descendant](https://www.w3.org/TR/selectors-4/#descendant-combinators): `node descendant`
    - [Child](https://www.w3.org/TR/selectors-4/#child-combinators): `node > child`
    - [Next-sibling](https://www.w3.org/TR/selectors-4/#adjacent-sibling-combinators): `node + next`
    - [Subsequent-sibling](https://www.w3.org/TR/selectors-4/#general-sibling-combinators): `node ~ sibling`
- Pseudo-class
    - [Negation](https://www.w3.org/TR/selectors-4/#negation): `:not(...)`
    - [Matches-any](https://www.w3.org/TR/selectors-4/#matches): `:is(...)`
    - [Relational](https://www.w3.org/TR/selectors-4/#relational): `:has(...)`
    - [`:first-child`](https://www.w3.org/TR/selectors-4/#the-first-child-pseudo)
    - [`:last-child`](https://www.w3.org/TR/selectors-4/#the-last-child-pseudo)
- Attributes (Properties)
    - `[PropName]` (existance)
    - `[PropName = 'Exact']`
    - `[PropName ^= 'StartsWith']`
    - `[PropName $= 'EndsWith']`
    - `[PropName *= 'Contains']`
    - `[PropName ~= 'Item']` (ex. `[Modifiers ~= 'async']`)
- Qulaly Extensions
    - `[Name = 'MethodName']`: Name special property
        - `Name` is a special property for convenience that can be used in `MethodDeclaration`, `ClassDeclaration` ... etc
    - `[TypeParameters.Count > 0]`: Conditions
        - `Parameters.Count`
        - `TypeParameters.Count`

## License
MIT License
```
Copyright © 2020-present Mayuki Sawatari <mayuki@misuzilla.org>
```