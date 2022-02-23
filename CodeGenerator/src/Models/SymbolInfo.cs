using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record SymbolParent(
        InterfaceInfo OwnerInterface,
        ValueImmutableList<TypeInfo>? TypeArguments,
        SymbolParent? Parent)
    {
        public static SymbolParent Root(InterfaceInfo ownerInterface)
        {
            return new SymbolParent(
                ownerInterface,
                null,
                null);
        }
    }

    public sealed record SymbolInfo(
        SymbolParent Parent,
        MethodInfo? MethodInfo,
        ConstructorInfo? ConstructorInfo,
        PropertyInfo? PropertyInfo,
        GetAccessorInfo? GetAccessorInfo,
        SetAccessorInfo? SetAccessorInfo,
        IndexerInfo? IndexerInfo)
    {
        public static SymbolInfo From(
            SymbolParent parent,
            MethodInfo methodInfo)
        {
            return new SymbolInfo(
                parent,
                methodInfo,
                null,
                null,
                null,
                null,
                null);
        }

        public static SymbolInfo From(
            SymbolParent parent,
            ConstructorInfo constructorInfo)
        {
            return new SymbolInfo(
                parent,
                null,
                constructorInfo,
                null,
                null,
                null,
                null);
        }

        public static SymbolInfo From(
            SymbolParent parent,
            PropertyInfo propertyInfo)
        {
            return new SymbolInfo(
                parent,
                null,
                null,
                propertyInfo,
                null,
                null,
                null);
        }

        public static SymbolInfo From(
            SymbolParent parent,
            GetAccessorInfo getAccessorInfo)
        {
            return new SymbolInfo(
                parent,
                null,
                null,
                null,
                getAccessorInfo,
                null,
                null);
        }

        public static SymbolInfo From(
            SymbolParent parent,
            SetAccessorInfo setAccessorInfo)
        {
            return new SymbolInfo(
                parent,
                null,
                null,
                null,
                null,
                setAccessorInfo,
                null);
        }

        public static SymbolInfo From(
            SymbolParent parent,
            IndexerInfo indexerInfo)
        {
            return new SymbolInfo(
                parent,
                null,
                null,
                null,
                null,
                null,
                indexerInfo);
        }
    }
}
