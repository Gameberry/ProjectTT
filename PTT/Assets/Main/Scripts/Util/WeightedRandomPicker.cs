using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;

public class WeightedRandomPicker<T>
{
    private readonly LinkedList<(T item, ObscuredDouble weight)> _items;
    private readonly System.Random _random;
    private ObscuredDouble totalWeight = 0.0;

    public int Count { get { return _items.Count; } }

    public WeightedRandomPicker()
    {
        _items = new LinkedList<(T, ObscuredDouble)>();
        _random = new System.Random();
    }

    public void Add(T item, ObscuredDouble weight)
    {
        _items.AddLast((item, weight));
        totalWeight += weight;
    }

    public void Remove(T item, ObscuredDouble weight)
    {
        if (_items.Remove((item, weight)) == true)
            totalWeight -= weight;
    }

    public T Pick()
    {
        ObscuredDouble randomNumber = _random.NextDouble() * totalWeight;

        foreach ((T item, ObscuredDouble weight) in _items)
        {
            if (randomNumber < weight)
            {
                return item;
            }
            randomNumber -= weight;
        }

        return default;
    }

    public double GetTotalWeight()
    {
        return totalWeight;
    }

    public void RefreshTotalWeight()
    {
        totalWeight = 0;

        foreach ((T item, ObscuredDouble weight) in _items)
        {
            totalWeight += weight;
        }
    }

    public void Clear()
    {
        _items.Clear();
        totalWeight = 0;
    }
}
