using System.Collections.ObjectModel;

using aDataLib;

using ReactiveUI;

namespace aSql.ViewModels;

public sealed class TablesGridViewModel : ViewModelBase, IRefreshable
{
  public ObservableCollection<TablesRowViewModel> TableRows { get; } = [];

  public DatDef? SqlDefinition
  {
    get;
    internal set
    {
      field = value;
      Refresh();
    }
  }

  public event EventHandler? TableChanged;

  private void OnChanged()
  {
    TableChanged?.Invoke(this, EventArgs.Empty);
  }

  public void Refresh()
  {
    TableRows.Clear();

    if (SqlDefinition?.Tables == null) return;
    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
    foreach (var tab in SqlDefinition!.Tables)
    {
      var tabRow = new TablesRowViewModel
        { TableName = tab.Name, TableAlias = tab.Alias, Take = true, MyDatDefTable = tab };
      tabRow.TableRowChanged += TabRow_Changed;
      TableRows.Add(tabRow);
    }

    this.RaisePropertyChanged(nameof(TableRows));
  }

  private void TabRow_Changed(object? sender, EventArgs e)
  {
    OnChanged();
  }
}

public sealed class TablesRowViewModel : ViewModelBase
{
  public string TableName
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = string.Empty;

  public bool Take
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = true;

  public string TableAlias
  {
    get;
    set
    {
      if (field != value)
      {
        MyDatDefTable?.Alias = value;
        OnChanged();
      }

      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = string.Empty;

  public DatDefTable? MyDatDefTable
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  }

  public event EventHandler? TableRowChanged;

  private void OnChanged()
  {
    TableRowChanged?.Invoke(this, EventArgs.Empty);
  }
}