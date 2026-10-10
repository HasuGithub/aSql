using System.Collections.ObjectModel;
using aDataLib;
using ReactiveUI;

namespace aSql.ViewModels;

public sealed class ResultsGridViewModel : ViewModelBase
{
  private IReadOnlyList<DatDefTable.DatDefTableField> _columnNames = [];
  private DatDef? _currDatDef;
  private SqlEditorViewModel? _sqlEditorViewModel;

  public ObservableCollection<DynamicRowWrapper> Rows
  {
    get;
    private set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(RowCountDisplayText));
    }
  } = [];

  public string RowCountDisplayText => $"Zeilen im Grid: {Rows.Count}";

  public IReadOnlyList<DatDefTable.DatDefTableField>? ColumnNames
  {
    get => _columnNames;
    set => this.RaiseAndSetIfChanged(ref _columnNames, value ?? []);
  }

  public string[]? ReaderColumnNames
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value ?? []);
  }

  public DatDef? ReaderDatDef
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }


  public DynamicRowWrapper? SelectedItem
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      if (field is null) return;
      SynchronizeRowAndDatDef(field);
    }
  }

  private void SynchronizeRowAndDatDef(DynamicRowWrapper dRow)
  {
    if (SelectedItem is null || _currDatDef is null) return;
    foreach (var col in dRow.Data.Keys)
    {
      var field = _currDatDef.AllFields.Values.FirstOrDefault(f =>
        string.Equals(f.Name, col, StringComparison.CurrentCultureIgnoreCase));
      if (field == null) continue;
      field.Value = dRow[col];
      field.ValueOld = field.Value;
    }
  }

  public bool UpdateCell(string fieldName, string newValue, DynamicRowWrapper dRow)
  {
    if (SelectedItem is null || ReaderDatDef is null || _sqlEditorViewModel is null) return false;
    var field = ReaderDatDef.AllFields.Values.FirstOrDefault(f =>
      string.Equals(f.Name, fieldName, StringComparison.CurrentCultureIgnoreCase));
    if (field is null) return false;
    if (field.Value?.ToString() == newValue) return false;
    field.Value = newValue;
    var sql = field.MyTable.GetUpDateSqlIntern(CondTypes.None, true);
    var ret = _sqlEditorViewModel.ExecuteNonQuery(sql);
    return ret;
  }

  public void SetRows(ObservableCollection<DynamicRowWrapper> rows)
  {
    Rows = rows;
  }

  public void SetCurrDatDef(DatDef? datDef, SqlEditorViewModel sqlEditorViewModel)
  {
    _currDatDef = datDef;
    _sqlEditorViewModel = sqlEditorViewModel;
  }
}