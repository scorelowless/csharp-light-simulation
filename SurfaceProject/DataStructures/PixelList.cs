using System;
using System.Collections;
using System.Collections.Generic;
using SurfaceProject.Bezier;

namespace SurfaceProject.DataStructures;

public class PixelList : IEnumerable<(int x, int y, Triangle t)>
{
    private readonly (int x, int y, Triangle t)[] _array;
    private int _count;

    public PixelList(int size)
    {
        _array = new (int x, int y, Triangle t)[size];
    }
    
    public void Add(int x, int y, Triangle t)
    {
        if (_count >= _array.Length)
            throw new InvalidOperationException("ListWrapper is full.");
        _array[_count++] = (x, y, t);
    }

    public void Reset()
    {
        _count = 0;
    }

    public IEnumerator<(int x, int y, Triangle t)> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            yield return _array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}