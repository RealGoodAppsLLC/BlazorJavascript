using System.Collections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator;

public static class ValueImmutableListExtensions
{
    public static IEnumerable<T> DistinctSafeSlow<T>(
        this IEnumerable<T> self)
    {
        var newList = new List<T>();

        foreach (var item in self)
        {
            if (newList.Contains(item))
            {
                continue;
            }

            newList.Add(item);
        }

        return newList;
    }

    public static IEnumerable<ISafeSlowGrouping<TKey?, TSource>> GroupBySafeSlow<TSource, TKey>(
        this IEnumerable<TSource> self,
        Func<TSource, TKey?> keySelector)
    {
        var newList = new List<SafeSlowGrouping<TKey?, TSource>>();

        foreach (var item in self)
        {
            var keyValue = keySelector(item);

            var elementAddedToExistingGrouping = false;

            foreach (var grouping in newList)
            {
                if (keyValue == null && grouping.Key == null)
                {
                    grouping.AddElement(item);
                    elementAddedToExistingGrouping = true;
                    break;
                }

                if (keyValue != null && keyValue.Equals(grouping.Key))
                {
                    grouping.AddElement(item);
                    elementAddedToExistingGrouping = true;
                    break;
                }
            }

            if (elementAddedToExistingGrouping)
            {
                continue;
            }

            var newGrouping = new SafeSlowGrouping<TKey?, TSource>(keyValue);
            newGrouping.AddElement(item);
            newList.Add(newGrouping);
        }

        return newList;
    }

    public interface ISafeSlowGrouping<out TKey, out TElement> : IEnumerable<TElement>
    {
        TKey? Key { get; }
    }

    private sealed class SafeSlowGrouping<TKey, TElement> : ISafeSlowGrouping<TKey, TElement>
    {
        private readonly List<TElement> elements;

        public SafeSlowGrouping(TKey key)
        {
            this.Key = key;
            this.elements = new List<TElement>();
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        public void AddElement(TElement element)
        {
            this.elements.Add(element);
        }

        public TKey? Key { get; }
    }
}
