using System.ComponentModel;
using System.Dynamic;

namespace aSql.ViewModels;

public sealed class DynamicRowWrapper : INotifyPropertyChanged
{
  public event PropertyChangedEventHandler? PropertyChanged;

  public DynamicRowWrapper(ExpandoObject expando)
  {
    Data = expando;
    if (expando is INotifyPropertyChanged notifyExpando)
    {
      notifyExpando.PropertyChanged += (_, e) =>
      {
        if (!string.IsNullOrEmpty(e.PropertyName))
        {
          OnIndexerChanged(e.PropertyName);
        }
      };
    }
  }

  public object? this[string key]
  {
    get => Data.TryGetValue(key, out var val) ? val : null;
    set
    {
      if (Data.TryGetValue(key, out var oldVal) && Equals(oldVal, value))
        return;

      Data[key] = value;

      OnIndexerChanged(key);
    }
  }

  public IDictionary<string, object?> Data { get; }

  private void OnIndexerChanged(string key)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
    // Alternativ (Falls Avalonia in manchen Versionen den exakten Key ignoriert):
    // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
  }
}
