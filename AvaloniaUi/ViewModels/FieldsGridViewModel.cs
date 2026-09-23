using System.Collections.ObjectModel;
using System.Windows.Input;

using aDataLib;

using DynamicData;

using ReactiveUI;

namespace aSql.ViewModels;

public sealed class FieldsGridViewModel : ViewModelBase, IRefreshable
{
  public FieldsGridViewModel()
  {
    ClearAllMarksCommand = ReactiveCommand.Create(ClearAllMarks);
    MarkAllMarksCommand = ReactiveCommand.Create(MarkAllMarks);
  }

  public ICommand ClearAllMarksCommand { get; }
  public ICommand MarkAllMarksCommand { get; }


  public ObservableCollection<FieldsRowViewModel> FieldRows { get; } = [];

  public DataDictionary? DataDict { get; set; }

  public DatDef? SqlDefinition { get; internal set; }

  public event EventHandler? FieldChanged;

  private void OnChanged()
  {
    FieldChanged?.Invoke(this, EventArgs.Empty);
  }


  private void MarkAllMarks()
  {
    foreach (var row in FieldRows) row.Take = true;
  }


  private void ClearAllMarks()
  {
    foreach (var row in FieldRows) row.Take = false;
  }

  public void Refresh()
  {
    List<FieldsRowViewModel> newFieldRows = [];

    if (SqlDefinition != null)
      foreach (var tab in SqlDefinition.Tables)
      {
        var dictTab = DataDict?.Tables.FirstOrDefault(x => x.TableName == tab.Name);
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        if (dictTab?.AllColumns == null) continue;
        foreach (var col in dictTab.AllColumns)
        {
          var field = tab.Fields.FirstOrDefault(x => x.Name == col.ColName);
          var alias = field is { Alias.Length: > 0 } ? field.Alias : string.Empty;
          var groupby = field is { IsInGroupBy: true };
          var aggType = AggregateTypes.Nothing;
          if (field != null) aggType = field.FieldAgg;

          var fieldRow = new FieldsRowViewModel(tab)
          {
            MyDatDefTableField = field,
            FieldName = tab.Name + "." + col.ColName,
            ColName = col.ColName,
            FieldAlias = alias,
            IsGroupBy = groupby,
            Take = field != null,
            AggregateOption = aggType
          };
          fieldRow.FieldRowChanged += Field_Changed;
          newFieldRows.Add(fieldRow);
        }
      }

    FieldRows.Clear();
    FieldRows.Add(newFieldRows.OrderBy(x => x.FieldName));
    this.RaisePropertyChanged(nameof(FieldRows));
  }

  private void Field_Changed(object? sender, EventArgs e)
  {
    SqlDefinition!.GroupsClear();

    foreach (var row in FieldRows)
    {
      var aktTable = row.MyDatDefTable;
      if (row.Take)
      {
        if (aktTable.Fields.Any(x => x.Name == row.ColName) is false)
        {
          var field = aktTable.FieldsAdd(row.AggregateOption, row.ColName, row.FieldAlias);
          field!.IsInGroupBy = row.IsGroupBy;
          row.MyDatDefTableField = field;
        }
        else
        {
          var field = aktTable.Fields.First(x => x.Name == row.ColName);
          field.IsInGroupBy = row.IsGroupBy;
          field.FieldAgg = row.AggregateOption;
          row.MyDatDefTableField = field;
        }
      }
      else
      {
        if (aktTable.Fields.Any(x => x.Name == row.ColName)) aktTable.FieldsDelete(row.ColName);
      }

      foreach (var field in aktTable.Fields.Where(field => field.IsInGroupBy))
        SqlDefinition.GroupsAdd(aktTable.Name, field.Name);
    }

    OnChanged();
  }
}

public sealed class FieldsRowViewModel : ViewModelBase
{
  public FieldsRowViewModel(DatDefTable myDatDefTable)
  {
    MyDatDefTable = myDatDefTable;
  }

  public string ColName
  {
    get;
    internal set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = string.Empty;

  public string FieldName
  {
    get;
    internal set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = string.Empty;

  public AggregateTypes AggregateOption
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
      if (FieldRowChanged == null || FieldAlias.Length != 0 || field == AggregateTypes.Nothing) return;
      var tabName = MyDatDefTable.Alias.Length > 0
        ? MyDatDefTable.Alias
        : MyDatDefTable.Name;
      FieldAlias = value + "_" + tabName + "_" + FieldName;
    }
  } = AggregateTypes.Nothing;

  public bool Take
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
      OnChanged();
    }
  }

  public bool IsGroupBy
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
      OnChanged();
    }
  }

  public string FieldAlias
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
      OnChanged();
    }
  } = string.Empty;

  public DatDefTable.DatDefTableField? MyDatDefTableField
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  }

  public DatDefTable MyDatDefTable
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  }

  public event EventHandler? FieldRowChanged;

  private void OnChanged()
  {
    FieldRowChanged?.Invoke(this, EventArgs.Empty);
  }
}