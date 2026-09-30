using System.Data;
using aDataLib;
using ReactiveUI;

namespace aSql.ViewModels;

public sealed class ResultsGridViewModel : ViewModelBase
{
  private IReadOnlyList<DatDefTable.DatDefTableField> _columnNames = [];

  public DataView Rows
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

  public void SetRows(DataView datView)
  {
    Rows = datView;
  }
}