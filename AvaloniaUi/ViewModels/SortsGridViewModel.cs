using System.Collections.ObjectModel;
using System.Windows.Input;

using aDataLib;

using DynamicData;

using ReactiveUI;

namespace aSql.ViewModels;

public sealed class SortsGridViewModel : ViewModelBase, IRefreshable
{
  public SortsGridViewModel()
  {
    ClearAllMarksCommand = ReactiveCommand.Create(ClearAllMarks);
    MarkAllMarksCommand = ReactiveCommand.Create(MarkAllMarks);
  }


  public ICommand ClearAllMarksCommand { get; }
  public ICommand MarkAllMarksCommand { get; }

  public ObservableCollection<SortsRowViewModel> SortRows { get; } = [];

  public DatDef? SqlDefinition
  {
    get;
    internal set
    {
      field = value;
      Refresh();
    }
  }

  public event EventHandler? FieldChanged;

  private void OnChanged()
  {
    FieldChanged?.Invoke(this, EventArgs.Empty);
  }

  private void MarkAllMarks()
  {
    foreach (var row in SortRows) row.Take = true;
  }


  private void ClearAllMarks()
  {
    foreach (var row in SortRows) row.Take = false;
  }

  public void Refresh()
  {
    var newSorts = new List<SortsRowViewModel>();

    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
    foreach (var tab in SqlDefinition!.Tables)
    {
      var dictTab = SqlDefinition.DbConnect.DataDic.Tables.First(x => x.TableName == tab.Name);

      foreach (var sortRow in from field in dictTab.AllColumns
               let takeIt = SqlDefinition.SortsList.Any(x => x.Table == tab.Name && x.Field == field.ColName)
               select new SortsRowViewModel
               {
                 FieldName = tab.Name + "." + field.ColName,
                 TabName = tab.Name,
                 ColName = field.ColName,
                 Take = takeIt,
                 IsDescanding = takeIt &&
                                SqlDefinition.SortsList.First(x => x.Table == tab.Name && x.Field == field.ColName)
                                  .Type is SortTypes.Desc,
                 AggregateOption =
                   takeIt
                     ? SqlDefinition.SortsList.First(x => x.Table == tab.Name && x.Field == field.ColName).FieldAgg
                     : AggregateTypes.Nothing,
                 Position = takeIt
                   ? SqlDefinition.SortsList.First(x => x.Table == tab.Name && x.Field == field.ColName).MyIndex + 1
                   : 0
               })
      {
        sortRow.SortRowChanged += SortRow_Changed;
        newSorts.Add(sortRow);
      }
    }

    SortRows.Clear();
    SortRows.Add(newSorts.OrderBy(x => x.FieldName));

    this.RaisePropertyChanged(nameof(SortRows));
  }

  private void SortRow_Changed(object? sender, EventArgs e)
  {
    SqlDefinition!.SortsClear();

    foreach (var row in SortRows.Where(x => x.Take).OrderBy(x => x.Position))
      SqlDefinition.SortsAdd(string.Empty, row.TabName, row.AggregateOption,
        row.ColName, row.IsDescanding ? SortTypes.Desc : SortTypes.Asc);
    OnChanged();
  }
}

public sealed class SortsRowViewModel : ViewModelBase
{
  public string TabName
  {
    get;
    internal set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = string.Empty;

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

  public bool IsDescanding
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
      OnChanged();
    }
  }

  public int Position
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
      OnChanged();
    }
  }

  public event EventHandler? SortRowChanged;

  private void OnChanged()
  {
    SortRowChanged?.Invoke(this, EventArgs.Empty);
  }
}