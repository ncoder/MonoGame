using System.Collections.Generic;

/// <summary>
/// This improved version of the List<> doesn't release the buffer on Clear(), resulting in better performance and less garbage collection.
/// </summary>

namespace System.Collections.Generic
{

    public class BetterList<T>
    {
        
         /// <summary>
         /// Direct access to the buffer. Note that you should not use its 'Length' parameter, but instead use BetterList.Length.
         /// </summary>
        
         public T[] buffer;
        
         /// <summary>
         /// Direct access to the buffer's size. Note that it's only public for speed and efficiency. You shouldn't modify it.
         /// </summary>
        
         private int size = 0;
        
         public int Count
        {   
            get { return size; }
        }
        
        
        // called when the buffer gets re-allocated.
        public Action resizeCallback;
         /// <summary>
         /// For 'foreach' functionality.
         /// </summary>
        
         public IEnumerator<T> GetEnumerator ()
         {
             if (buffer != null)
             {
                 for (int i = 0; i < size; ++i)
                 {
                     yield return buffer[i];
                 }
             }
         }
         
         /// <summary>
         /// Convenience function. I recommend using .buffer instead.
         /// </summary>
         
         public T this[int i]
         {
             get { return buffer[i]; }
             set { buffer[i] = value; }
         }
        
         /// <summary>
         /// Helper function that expands the size of the array, maintaining the content.
         /// </summary>
        
         void AllocateMore (int num = 1)
         {
             T[] newList = (buffer != null) ? new T[Math.Max((buffer.Length+num-1) << 1, 32)] : new T[32];
             if (buffer != null && size > 0) buffer.CopyTo(newList, 0);
             buffer = newList;
            if(resizeCallback != null)
            {
                resizeCallback();
            }
         }
        
         /// <summary>
         /// Trim the unnecessary memory, resizing the buffer to be of 'Length' size.
         /// Call this function only if you are sure that the buffer won't need to resize anytime soon.
         /// </summary>
        
         void Trim ()
         {
             if (size > 0)
             {
                 if (size < buffer.Length)
                 {
                     T[] newList = new T[size];
                     for (int i = 0; i < size; ++i) newList[i] = buffer[i];
                     buffer = newList;
                    if(resizeCallback != null)
                    {
                        resizeCallback();
                    }
                    
                 }
             }
             else 
            {
                buffer = null;
                if(resizeCallback != null)
                {
                    resizeCallback();
                }
            }
         }
        
         /// <summary>
         /// Clear the array by resetting its size to zero. Note that the memory is not actually released.
         /// </summary>
        
         public void Clear () { size = 0; }
        
         /// <summary>
         /// Clear the array and release the used memory.
         /// </summary>
        
         public void Release () 
        { 
            size = 0; buffer = null; 
            if(resizeCallback != null)
            {
                resizeCallback();
            }
            
        }
        
         /// <summary>
         /// Add the specified item to the end of the list.
         /// </summary>
        
         public void Add (T item)
         {
             if (buffer == null || size == buffer.Length) AllocateMore();
             buffer[size++] = item;
         }
        
        // add N more values, unititialized data..
         public void AddN (int number)
         {
             if (buffer == null || size+number > buffer.Length) 
                AllocateMore(number);
            size += number;
         }
        
         /// <summary>
         /// Remove the specified item from the list. Note that RemoveAt() is faster and is advisable if you already know the index.
         /// </summary>
        
         public void Remove (T item)
         {
             if (buffer != null)
             {
                 EqualityComparer<T> comp = EqualityComparer<T>.Default;
        
                 for (int i = 0; i < size; ++i)
                 {
                     if (comp.Equals(buffer[i], item))
                     {
                         --size;
                         buffer[i] = default(T);
                         for (int b = i; b < size; ++b) buffer[b] = buffer[b + 1];
                         return;
                     }
                 }
             }
         }
        
         /// <summary>
         /// Remove an item at the specified index.
         /// </summary>
        
         public void RemoveAt (int index)
         {
             if (buffer != null && index < size)
             {
                 --size;
                 buffer[index] = default(T);
                 for (int b = index; b < size; ++b) buffer[b] = buffer[b + 1];
             }
         }
        
         /// <summary>
         /// Mimic List's ToArray() functionality, except that in this case the list is resized to match the current size.
         /// </summary>
        
         public T[] ToArray () { Trim(); return buffer; }
         
         public bool Contains(T item)
         {
             if (buffer != null)
             {
                 EqualityComparer<T> comp = EqualityComparer<T>.Default;
        
                 for (int i = 0; i < size; ++i)
                 {
                     if (comp.Equals(buffer[i], item))
                     {
                         return true;
                     }
                 }
             }
             return false;
         }
    }
    
}