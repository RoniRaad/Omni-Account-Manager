using System.Collections.ObjectModel;

namespace AccountManager.Core.Static
{
    public static class ObservableCollectionExtensions
    {
        public static void RemoveAll<T>(this ObservableCollection<T> collection,
                                                            Func<T, bool> condition)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }
        public static void AddRange<T>(this ObservableCollection<T> collection,
                                                    IEnumerable<T> values)
        {
            var enumerator = values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                collection.Add(enumerator.Current);
            }
        }
    }
}
