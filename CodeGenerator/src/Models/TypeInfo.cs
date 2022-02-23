using System.Text;
using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record TypeInfo(
        UnionTypeInfo? Union,
        IntersectionTypeInfo? Intersection,
        TypeInfo? Parenthesized,
        SingleTypeInfo? Single,
        FunctionTypeInfo? Function,
        TypeInfo? Array)
    {
        public static readonly TypeInfo AnyType = new(
            null,
            null,
            null,
            new SingleTypeInfo(
                "any",
                null,
                null,
                null,
                ValueImmutableList.Create<TypeInfo>(),
                false),
            null,
            null);

        public string RenderDebugString()
        {
            if (this.Union != null)
            {
                return string.Join(" | ", this.Union.Types
                    .Select(t => t.RenderDebugString())
                    .ToValueImmutableList());
            }

            if (this.Array != null)
            {
                return this.Array.RenderDebugString() + "[]";
            }

            if (this.Function != null)
            {
                var functionStrBuilder = new StringBuilder();

                functionStrBuilder.Append('<');
                functionStrBuilder.Append(string.Join(", ", this.Function.ExtractTypeParametersResult.TypeParameters
                    .Select(p =>
                    {
                        var typeParameterStrBuilder = new StringBuilder();

                        typeParameterStrBuilder.Append(p.Name);

                        if (p.Default != null)
                        {
                            typeParameterStrBuilder.Append($" = {p.Default.RenderDebugString()}");
                        }

                        if (p.Constraint != null)
                        {
                            typeParameterStrBuilder.Append($" extends {p.Constraint.RenderDebugString()}");
                        }

                        return typeParameterStrBuilder.ToString();
                    })));
                functionStrBuilder.Append('>');

                functionStrBuilder.Append('(');
                functionStrBuilder.Append(string.Join(", ", this.Function.Parameters
                    .Select(p => $"{p.Type.RenderDebugString()} {p.Name}")));
                functionStrBuilder.Append(')');
                functionStrBuilder.Append(" => ");
                functionStrBuilder.Append(this.Function.ReturnType.RenderDebugString());
            }

            if (this.Intersection != null)
            {
                return string.Join(" & ", this.Intersection.Types
                    .Select(t => t.RenderDebugString())
                    .ToValueImmutableList());
            }

            if (this.Parenthesized != null)
            {
                return $"({this.Parenthesized.RenderDebugString()})";
            }

            if (this.Single != null)
            {
                if (this.Single.IsUnhandled)
                {
                    return "<unhandled>";
                }

                if (this.Single.StringLiteral != null)
                {
                    return $"\"{this.Single.StringLiteral.Value}\"";
                }

                if (this.Single.NumberLiteral != null)
                {
                    return $"{this.Single.NumberLiteral.Value}";
                }

                if (this.Single.BooleanLiteral != null)
                {
                    return this.Single.BooleanLiteral.Value ? "true" : "false";
                }

                if (this.Single.Name != null)
                {
                    var singleStringBuilder = new StringBuilder();

                    singleStringBuilder.Append(this.Single.Name);

                    if (this.Single.TypeArguments.Any())
                    {
                        singleStringBuilder.Append('<');
                        singleStringBuilder.Append(string.Join(", ", this.Single.TypeArguments
                            .Select(t => t.RenderDebugString())
                            .ToValueImmutableList()));
                        singleStringBuilder.Append('>');
                    }

                    return singleStringBuilder.ToString();
                }

                return "<unhandled single>";
            }

            return "<null>";
        }
    }
}
