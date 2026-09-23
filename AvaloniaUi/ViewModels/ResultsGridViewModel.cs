using ReactiveUI;

namespace aSql.ViewModels;

public sealed class ResultsGridViewModel : ViewModelBase
{
  private IReadOnlyList<string> _columnNames = [];

  public IReadOnlyList<Dictionary<string, object?>> Rows
  {
    get;
    private set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged(nameof(RowCountDisplayText));
    }
  } = [];

  public string RowCountDisplayText => $"Zeilen im Grid: {Rows.Count}";

  public IReadOnlyList<string>? ColumnNames
  {
    get => _columnNames;
    set => this.RaiseAndSetIfChanged(ref _columnNames, value ?? []);
  }

  public void SetRows(IEnumerable<Dictionary<string, object?>> rows)
  {
    Rows = rows as IReadOnlyList<Dictionary<string, object?>> ?? [.. rows];
  }
}