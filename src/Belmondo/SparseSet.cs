using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Belmondo;

public class SparseSetBuilder
{
    public static SparseSet<T> Create<T>(ReadOnlySpan<T> values)
    {
        var sparseSet = new SparseSet<T>();

        foreach (T value in values)
        {
            sparseSet.Add(value);
        }

        return sparseSet;
    }
}

// [TODO]: this shit don't work
[CollectionBuilder(typeof(SparseSetBuilder), nameof(SparseSetBuilder.Create))]
public partial class SparseSet<T>
{
    // literally just for type safety
    public readonly struct SparseIndex(int value)
    {
        public readonly int Value = value;
    }

    // ditto
    public readonly struct DenseIndex(int value)
    {
        public readonly int Value = value;
    }

    public struct DenseElement(SparseIndex sparseIndex, T value) : IEquatable<T>
    {
        public SparseIndex SparseIndex = sparseIndex;
        public T Value = value;

        public readonly bool Equals(T? other) => other?.Equals(Value) ?? false;
    }

    private readonly List<DenseIndex> _sparse = [];
    private readonly List<DenseElement> _dense = [];

    public int Count { get; private set; } = 0;

    public Span<DenseElement> Span => CollectionsMarshal.AsSpan(_dense);

    public DenseElement this[SparseIndex sparseIndex]
    {
        get => _dense[_sparse[sparseIndex.Value].Value];
        set => _dense[_sparse[sparseIndex.Value].Value] = value;
    }

    public SparseIndex Add(T item)
    {
        DenseIndex denseIndex = new(Count);

        Count += 1;

        // try to reuse last freed index
        if (denseIndex.Value < _dense.Count)
        {
            var denseElement = _dense[denseIndex.Value];

            denseElement.Value = item;
            _dense[denseIndex.Value] = denseElement;

            return denseElement.SparseIndex;
        }

        // allocate new index
        SparseIndex sparseIndex = new(_sparse.Count);

        _dense.Add(new(sparseIndex, item));
        _sparse.Add(denseIndex);

        return sparseIndex;
    }

    public bool Remove(SparseIndex sparseIndex)
    {
        if (!Contains(sparseIndex))
        {
            return false;
        }

        Count -= 1;

        var denseIndex = _sparse[sparseIndex.Value];
        DenseIndex endDenseIndex = new(Count);

        // swap remove
        var endElement = _dense[endDenseIndex.Value];

        _dense[denseIndex.Value] = endElement;
        _sparse[endElement.SparseIndex.Value] = denseIndex;

        // update end dense index for reuse
        var denseElement = _dense[endDenseIndex.Value];

        denseElement.SparseIndex = sparseIndex;
        _dense[endDenseIndex.Value] = denseElement;

        _sparse[sparseIndex.Value] = endDenseIndex;

        return true;
    }

    public bool Contains(SparseIndex sparseIndex)
    {
        if (sparseIndex.Value >= _sparse.Count)
        {
            return false;
        }

        var denseIndex = _sparse[sparseIndex.Value];
        var currentIndex = _dense[denseIndex.Value].SparseIndex;

        return denseIndex.Value < Count && sparseIndex.Value == currentIndex.Value;
    }

    public void Clear()
    {
        _dense.Clear();
        _sparse.Clear();
    }
}

public partial class SparseSet<T> : IReadOnlyCollection<SparseSet<T>.DenseElement>
{
    public IEnumerator<DenseElement> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            // to prevent out of range exceptions if iterating while collection
            // was modified
            if (i >= _dense.Count)
            {
                break;
            }

            yield return _dense[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public partial class SparseSet<T>
{
    private readonly List<SparseIndex> _toRemove = [];

    public void RemoveAll(Predicate<DenseElement> predicate)
    {
        _toRemove.Clear();

        foreach (ref var denseElement in Span)
        {
            if (predicate(denseElement))
            {
                _toRemove.Add(denseElement.SparseIndex);
            }
        }

        foreach (var sparseIndex in _toRemove)
        {
            Remove(sparseIndex);
        }
    }
}
