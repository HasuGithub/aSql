using System.Collections;

namespace aDataLib;

public class FathersAndSons : IDisposable
{
  private readonly SortedList _lSlMain = new();

  public FathersAndSons? this[string fathersName]
  {
    get
    {
      FathersAndSons? result;
      try
      {
        result = _lSlMain[fathersName.ToLower()] as FathersAndSons;
      }
      catch
      {
        result = null;
      }

      return result;
    }
  }

  public FathersAndSons? this[int index]
  {
    get
    {
      FathersAndSons? result;
      try
      {
        result = _lSlMain.GetByIndex(index) as FathersAndSons;
      }
      catch
      {
        result = null;
      }

      return result;
    }
  }

  public void Dispose()
  {
    Clear();
  }

  ~FathersAndSons()
  {
    Dispose();
  }

  public void Clear()
  {
    for (var i = 0; i < _lSlMain.Count; i++)
    {
      var fathersAndSons = _lSlMain.GetByIndex(i) as FathersAndSons;
      fathersAndSons?.Dispose();
    }

    _lSlMain.Clear();
    _lSlMain.TrimToSize();
  }

  public FathersAndSons Add(string father, string son)
  {
    var fathersAndSons = this[father.ToLower()];
    if (fathersAndSons == null)
    {
      _lSlMain.Add(father.ToLower(), new FathersAndSons());
      fathersAndSons = _lSlMain[father.ToLower()] as FathersAndSons;
    }

    if (son.Length > 0) fathersAndSons?.Add(son, "");
    return fathersAndSons ?? new FathersAndSons();
  }
}