///----------- ----------- ----------- ----------- ----------- -----------
/// <copyright file="DictionaryValueCollection.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>                               
///
/// <owner>gpaperin</owner>
///----------- ----------- ----------- ----------- ----------- -----------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;


namespace System.Runtime.InteropServices.WindowsRuntime {
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class DictionaryValueCollection<TKey, TValue> : ICollection<TValue>
    {
        private readonly IDictionary<TKey, TValue> dictionary;

        public DictionaryValueCollection(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            this.dictionary = dictionary;
        }

        public void CopyTo(TValue[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (array.Length <= index && this.Count > 0)
                throw new ArgumentException(Environment.GetResourceString("Arg_IndexOutOfRangeException"));
            if (array.Length - index < dictionary.Count)
                throw new ArgumentException(Environment.GetResourceString("Argument_InsufficientSpaceToCopyCollection"));

            int i = index;
            foreach (KeyValuePair<TKey, TValue> mapping in dictionary)
            {
                array[i++] = mapping.Value;
            }
        }

        public int Count {
            get { return dictionary.Count; }
        }

        bool ICollection<TValue>.IsReadOnly {
            get { return true; }
        }

        void ICollection<TValue>.Add(TValue item)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_ValueCollectionSet"));
        }

        void ICollection<TValue>.Clear()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_ValueCollectionSet"));
        }

        public bool Contains(TValue item)
        {
            EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
            foreach (TValue value in this)
                if (comparer.Equals(item, value))
                    return true;
            return false;
        }

        bool ICollection<TValue>.Remove(TValue item)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_ValueCollectionSet"));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TValue>)this).GetEnumerator();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return new DictionaryValueEnumerator<TKey, TValue>(dictionary);
        }
    }  // public class DictionaryValueCollection<TKey, TValue>


    [Serializable]
    internal sealed class DictionaryValueEnumerator<TKey, TValue> : IEnumerator<TValue>
    {
        private readonly IDictionary<TKey, TValue> dictionary;
        private IEnumerator<KeyValuePair<TKey, TValue>> enumeration;

        public DictionaryValueEnumerator(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            this.dictionary = dictionary;
            this.enumeration = dictionary.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
            enumeration.Dispose();
        }

        public bool MoveNext()
        {
            return enumeration.MoveNext();
        }

        Object IEnumerator.Current {
            get { return ((IEnumerator<TValue>)this).Current; }
        }

        public TValue Current {
            get { return enumeration.Current.Value; }
        }

        public void Reset()
        {
            enumeration = dictionary.GetEnumerator();
        }
    }  // class DictionaryValueEnumerator<TKey, TValue>
}

// DictionaryValueCollection.cs
