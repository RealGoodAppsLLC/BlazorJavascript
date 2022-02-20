using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public static class TypeIdentifiers
    {
        public enum TypeIdentifier
        {
            Null = 0,
            Undefined,
            Number,
            String,
            Object,
            Function,
            Boolean,
            Array,
        }

        public static int ToInteger(
            this TypeIdentifier typeIdentifier)
        {
            return (int)typeIdentifier;
        }

        public static ImmutableList<TypeIdentifier> GetPredefinedTypeIdentifiers()
        {
            var typeIdentifiers = Enum.GetValues(typeof(TypeIdentifier)).Cast<TypeIdentifier>();
            return typeIdentifiers.ToImmutableList();
        }
    }
}
