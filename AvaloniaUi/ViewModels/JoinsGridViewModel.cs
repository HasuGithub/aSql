using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;

using aDataLib;

using ReactiveUI;

namespace aSql.ViewModels;

public sealed class JoinsGridViewModel : ViewModelBase, IRefreshable
{
  public JoinsGridViewModel()
  {
    ClearAllJoinsCommand = ReactiveCommand.Create(ClearAllJoins);
  }

  public ICommand ClearAllJoinsCommand { get; }

  public ObservableCollection<JoinsRowViewModel> JoinRows { get; } = [];

  public DataDictionary? DataDict { get; set; }

  public bool IsKeyCtrlDown { get; set; }

  public DatDef? SqlDefinition { get; internal set; }
  public event EventHandler? JoinsChanged;

  private void OnChanged()
  {
    JoinsChanged?.Invoke(this, EventArgs.Empty);
  }

  private void ClearAllJoins()
  {
    JoinRows.Clear();

    if (SqlDefinition is null) return;

    SqlDefinition.JoinsClear();
    OnChanged();
  }

  public void Refresh()
  {
    JoinRows.Clear();

    if (SqlDefinition != null)
      foreach (var joinRow in SqlDefinition.JoinsAll.Select(join => new JoinsRowViewModel(join)
               {
                 FromTable = join.From,
                 JoinType = join.JoinType switch
                 {
                   JoinTypes.LeftJoin => "Left Join",
                   JoinTypes.NaturalJoin => "Inner Join",
                   JoinTypes.RightJoin => "Right Join",
                   _ => ""
                 },
                 ToTable = join.To
               }))
      {
        joinRow.JoinRowChanged += Join_Changed;
        JoinRows.Add(joinRow);
      }

    this.RaisePropertyChanged(nameof(JoinRows));
  }

  private void Join_Changed(object? sender, EventArgs e)
  {
    var curRow = sender as JoinsRowViewModel;
    Contract.Assert(curRow != null);

    var curJoinType = curRow.JoinType switch
    {
      "Left Join" => JoinTypes.LeftJoin,
      "Inner Join" => JoinTypes.NaturalJoin,
      "Right Join" => JoinTypes.RightJoin,
      _ => JoinTypes.LeftJoin
    };

    if (IsKeyCtrlDown)
    {
      foreach (var row in JoinRows)
      {
        var aktJoin = row.MyDatDefJoin;

        var aktDefJoin = SqlDefinition?.JoinsAll.FirstOrDefault(x => x.Key == aktJoin.Key);

        aktDefJoin?.JoinType = curJoinType;
      }

      Refresh();
    }
    else
    {
      var aktDefJoin = SqlDefinition?.JoinsAll.FirstOrDefault(x => x.Key == curRow.MyDatDefJoin.Key);
      aktDefJoin?.JoinType = curJoinType;
    }

    OnChanged();
  }
}

public sealed class JoinsRowViewModel : ViewModelBase
{
  private readonly List<string> _allJoinTypes = [];
  private int _curJoinType;

  public JoinsRowViewModel(DatDefJoin myDatDefJoin)
  {
    MyDatDefJoin = myDatDefJoin;
    DecreaseJoinTypeCommand = ReactiveCommand.Create(DecreaseJoinType);
    IncreaseJoinTypeCommand = ReactiveCommand.Create(IncreaseJoinType);
    _allJoinTypes.AddRange("Left Join", "Inner Join", "Right Join");
  }

  public ICommand DecreaseJoinTypeCommand { get; }

  public ICommand IncreaseJoinTypeCommand { get; }

  public string FromTable
  {
    get;
    internal set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = string.Empty;

  public string JoinType
  {
    get;
    internal set
    {
      _curJoinType = _allJoinTypes.IndexOf(value);
      if (_curJoinType < 0) _curJoinType = 0;

      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = string.Empty;

  public string ToTable
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  } = string.Empty;

  public DatDefJoin MyDatDefJoin
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RaisePropertyChanged();
    }
  }

  public event EventHandler? JoinRowChanged;

  private void DecreaseJoinType()
  {
    if (_curJoinType > 0)
      _curJoinType--;
    else
      _curJoinType = _allJoinTypes.Count - 1;

    JoinType = _allJoinTypes[_curJoinType];
    OnChanged();
  }

  private void IncreaseJoinType()
  {
    if (_curJoinType < _allJoinTypes.Count - 1)
      _curJoinType++;
    else
      _curJoinType = 0;

    JoinType = _allJoinTypes[_curJoinType];
    OnChanged();
  }


  private void OnChanged()
  {
    JoinRowChanged?.Invoke(this, EventArgs.Empty);
  }
}