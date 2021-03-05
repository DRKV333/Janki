using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LibAnkiCards.Importing
{
    internal class CardTypeComparer : IEqualityComparer<CardType>
    {
        public static CardTypeComparer Instance { get; } = new CardTypeComparer();

        private class SinglePropertyComparer<T, TProp> : IEqualityComparer<T>, IEqualityComparer
        {
            private readonly Func<T, TProp> property;

            public SinglePropertyComparer(Func<T, TProp> property) => this.property = property;

            public bool Equals(T x, T y) => property(x).Equals(property(y));

            public new bool Equals(object x, object y) => (x is T tx && y is T ty) && Equals(tx, ty);

            public int GetHashCode(T obj) => property(obj).GetHashCode();

            public int GetHashCode(object obj) => (obj is T tobj) ? GetHashCode(tobj) : obj.GetHashCode();
        }

        private static readonly SinglePropertyComparer<CardField, string> fieldComparer = new SinglePropertyComparer<CardField, string>(x => x.Name);
        private static readonly SinglePropertyComparer<CardVariant, string> variantComparer = new SinglePropertyComparer<CardVariant, string>(x => x.Name);

        public bool Equals(CardType x, CardType y) => (x == null) ? (y == null) :
                                                      x.Name == y.Name &&
                                                      x.Fields.SequenceEqual(y.Fields, fieldComparer) &&
                                                      x.Variants.SequenceEqual(y.Variants, variantComparer);

        public int GetHashCode(CardType obj) => HashCode.Combine(obj.Name, ListHash(obj.Fields, fieldComparer), ListHash(obj.Variants, variantComparer));

        private int ListHash<T>(IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            HashCode hash = new HashCode();
            foreach (var item in list)
            {
                hash.Add(item, comparer);
            }
            return hash.ToHashCode();
        }
    }
}