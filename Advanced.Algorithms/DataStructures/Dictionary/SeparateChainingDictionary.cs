﻿using System;
using System.Collections;
using System.Collections.Generic;


namespace Advanced.Algorithms.DataStructures
{
    /// <summary>
    /// A hash table implementation (key value dictionary) with separate chaining
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal class SeparateChainingDictionary<K, V> : IDictionary<K, V>  
    {
        private const double tolerance = 0.1;

        private DoublyLinkedList<DictionaryNode<K, V>>[] hashArray;
        private int bucketSize => hashArray.Length;
        private readonly int initialBucketSize;
        private int filledBuckets;

        public int Count { get; private set; }

        //init with an expected size (the larger the size lesser the collission, but memory matters!)
        public SeparateChainingDictionary(int initialBucketSize = 3)
        {
            this.initialBucketSize = initialBucketSize;
            hashArray = new DoublyLinkedList<DictionaryNode<K, V>>[initialBucketSize];
        }

        public V this[K key]
        {
            get => getValue(key);
            set => setValue(key, value);
        }
        //O(1) time complexity; worst case O(n)
        public bool ContainsKey(K key)
        {
            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                return false;
            }
            else
            {
                var current = hashArray[index].Head;

                while (current != null)
                {
                    if (current.Data.Key.Equals(key))
                    {
                        return true;
                    }

                    current = current.Next;
                }
            }

            return false;
        }

        //O(1) time complexity; worst case O(n)
        //add an item to this hash table
        public void Add(K key, V value)
        {
            grow();

            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                hashArray[index] = new DoublyLinkedList<DictionaryNode<K, V>>();
                hashArray[index].InsertFirst(new DictionaryNode<K, V>(key, value));
                filledBuckets++;
            }
            else
            {
                var current = hashArray[index].Head;

                while (current != null)
                {
                    if (current.Data.Key.Equals(key))
                    {
                        throw new Exception("Duplicate key");
                    }

                    current = current.Next;
                }

                hashArray[index].InsertFirst(new DictionaryNode<K, V>(key, value));
            }

            Count++;
        }

        //O(1) time complexity; worst case O(n)
        public void Remove(K key)
        {
            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                throw new Exception("No such item for given key");
            }
            else
            {
                var current = hashArray[index].Head;

                //VODO merge both search and remove to a single loop here!
                DoublyLinkedListNode<DictionaryNode<K, V>> item = null;
                while (current != null)
                {
                    if (current.Data.Key.Equals(key))
                    {
                        item = current;
                        break;
                    }

                    current = current.Next;
                }

                //remove
                if (item == null)
                {
                    throw new Exception("No such item for given key");
                }
                else
                {
                    hashArray[index].Delete(item);

                    //if list is empty mark bucket as null
                    if (hashArray[index].Head == null)
                    {
                        hashArray[index] = null;
                        filledBuckets--;
                    }
                }

            }

            Count--;

            shrink();

        }

        /// <summary>
        /// clear hash table
        /// </summary>
        public void Clear()
        {
            hashArray = new DoublyLinkedList<DictionaryNode<K, V>>[initialBucketSize];
            Count = 0;
            filledBuckets = 0;
        }


        private void setValue(K key, V value)
        {
            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                throw new Exception("Item not found");
            }
            else
            {
                var current = hashArray[index].Head;

                while (current != null)
                {
                    if (current.Data.Key.Equals(key))
                    {
                        Remove(key);
                        Add(key, value);
                        return;
                    }

                    current = current.Next;
                }
            }

            throw new Exception("Item not found");
        }

        private V getValue(K key)
        {
            var index = Math.Abs(key.GetHashCode()) % bucketSize;

            if (hashArray[index] == null)
            {
                throw new Exception("Item not found");
            }
            else
            {
                var current = hashArray[index].Head;

                while (current != null)
                {
                    if (current.Data.Key.Equals(key))
                    {
                        return current.Data.Value;
                    }

                    current = current.Next;
                }
            }

            throw new Exception("Item not found");
        }
        /// <summary>
        /// Grow array if needed
        /// </summary>
        private void grow()
        {
            if (filledBuckets >= bucketSize * 0.7)
            {
                filledBuckets = 0;
                //increase array size exponentially on demand
                var newBucketSize = bucketSize * 2;

                var biggerArray = new DoublyLinkedList<DictionaryNode<K, V>>[newBucketSize];

                for (int i = 0; i < bucketSize; i++)
                {
                    var item = hashArray[i];

                    //hashcode changes when bucket size changes
                    if (item != null)
                    {
                        if (item.Head != null)
                        {
                            var current = item.Head;

                            //find new location for each item
                            while (current != null)
                            {
                                var next = current.Next;

                                var newIndex = Math.Abs(current.Data.Key.GetHashCode()) % newBucketSize;

                                if (biggerArray[newIndex] == null)
                                {
                                    filledBuckets++;
                                    biggerArray[newIndex] = new DoublyLinkedList<DictionaryNode<K, V>>();
                                }

                                biggerArray[newIndex].InsertFirst(current);

                                current = next;
                            }
                        }

                    }


                }

                hashArray = biggerArray;
            }
        }

        /// <summary>
        /// Shrink if needed
        /// </summary>
        private void shrink()
        {
            if (Math.Abs(filledBuckets - bucketSize * 0.3) < tolerance && bucketSize / 2 > initialBucketSize)
            {
                filledBuckets = 0;
                //reduce array by half 
                var newBucketSize = bucketSize / 2;

                var smallerArray = new DoublyLinkedList<DictionaryNode<K, V>>[newBucketSize];

                for (var i = 0; i < bucketSize; i++)
                {
                    var item = hashArray[i];

                    //hashcode changes when bucket size changes
                    if (item?.Head != null)
                    {
                        var current = item.Head;

                        //find new location for each item
                        while (current != null)
                        {
                            var next = current.Next;

                            var newIndex = Math.Abs(current.Data.Key.GetHashCode()) % newBucketSize;

                            if (smallerArray[newIndex] == null)
                            {
                                filledBuckets++;
                                smallerArray[newIndex] = new DoublyLinkedList<DictionaryNode<K, V>>();
                            }

                            smallerArray[newIndex].InsertFirst(current);

                            current = next;
                        }
                    }
                }

                hashArray = smallerArray;
            }
        }

        //Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<DictionaryNode<K, V>> GetEnumerator()
        {
            return new SeparateChainingDictionaryEnumerator<K, V>(hashArray, bucketSize);
        }

    }

    //  implement IEnumerator.
    public class SeparateChainingDictionaryEnumerator<TK, TV> : IEnumerator<DictionaryNode<TK, TV>> 
    {
        internal DoublyLinkedList<DictionaryNode<TK, TV>>[] HashList;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;
        DoublyLinkedListNode<DictionaryNode<TK, TV>> currentNode = null;

        int length;

        internal SeparateChainingDictionaryEnumerator(DoublyLinkedList<DictionaryNode<TK, TV>>[] hashList, int length)
        {
            this.length = length;
            this.HashList = hashList;
        }

        public bool MoveNext()
        {
            if (currentNode?.Next != null)
            {
                currentNode = currentNode.Next;
                return true;
            }

            while (currentNode?.Next == null)
            {
                position++;

                if (position < length)
                {
                    if (HashList[position] == null)
                        continue;

                    currentNode = HashList[position].Head;

                    if (currentNode == null)
                        continue;

                    return true;
                }
                else
                {
                    break;
                }
            }

            return false;

        }

        public void Reset()
        {
            position = -1;
            currentNode = null;
        }


        object IEnumerator.Current => Current;

        public DictionaryNode<TK, TV> Current
        {
            get
            {
                try
                {
                    return currentNode.Data;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public void Dispose()
        {
        }

    }
}
