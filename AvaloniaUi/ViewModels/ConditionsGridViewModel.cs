using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

using aDataLib;

using DynamicData;

using ReactiveUI;
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace aSql.ViewModels;

/// <summary>
///   ViewModel for the Conditions Grid - manages the WHERE clause condition rows.
/// </summary>
public sealed class ConditionsGridViewModel : ViewModelBase, IRefreshable
{
  private DatDef? _currentDatDef;

  public ConditionsGridViewModel()
  {
    ConditionRows = [];
    UpdateCondsPreviewText();
    DeleteSelectedRowCommand = ReactiveCommand.Create(DeleteSelectedRow, CanDeleteSelectedRow);
    TakeOverConditionsCommand = ReactiveCommand.Create(TakeOverConditions, CanTakeOverConditions);
    AddRowCommand = ReactiveCommand.Create(AddRow);
    ClearAllConditionsCommand = ReactiveCommand.Create(ClearAllConditions);
  }

  public IObservable<bool> CanTakeOverConditions => this.WhenAnyValue(
    x => x.ConditionRows,
    x => x.CurrentRowNumber,
    x => x.CurrentCondition!.Operator,
    x => x.CurrentCondition!.RightOperand,
    (conditionRows, _, _, _) => conditionRows.Any(x =>
      (x.LeftOperand?.Length == 0 || x.RightOperand?.Length == 0) && x.Operator != "IS NULL" &&
      x.Operator != "IS NOT NULL") is false
  );

  public IObservable<bool> CanDeleteSelectedRow => this.WhenAnyValue(
    x => x.CurrentCondition,
    x => x.CurrentRowNumber,
    (currentCondition, currentRowNumber) => currentCondition != null && currentRowNumber > 0
  );

  public ICommand ClearAllConditionsCommand { get; }

  public ICommand TakeOverConditionsCommand { get; }

  public ICommand DeleteSelectedRowCommand { get; }
  public ICommand AddRowCommand { get; }

  public ObservableCollection<ConditionRowViewModel> ConditionRows { get; }

  public string CondsPreviewText
  {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  } = "WHERE";

  public bool IsHaving
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int CurrentRowNumber
  {
    get;
    set
    {
      this.RaiseAndSetIfChanged(ref field, value);
      CurrentCondition = field > -1 ? ConditionRows[field] : null;
    }
  } = -1;

  // Aktuell: ColNumber = 3 ---> Linker Operand
  //          ColNumber = 6 ---> Rechter Operand
  public int CurrentColNumber
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = 0;

  public ConditionRowViewModel? CurrentCondition
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public event EventHandler? ConditionsChanged;

  private void OnConditionsChanged()
  {
    ConditionsChanged?.Invoke(this, EventArgs.Empty);
  }

  private void DeleteSelectedRow()
  {
    if (CurrentCondition == null) return;

    ConditionRows.Remove(CurrentCondition);

    UpdateCondsPreviewText();
  }

  private void ClearAllConditions()
  {
    if (_currentDatDef is null) return;

    ConditionRows.RemoveMany(ConditionRows.Where(x => x.RowNumber > 0));
    CurrentCondition = ConditionRows.First();
    CurrentCondition.OperateWithPrevious = "WHERE";
    CurrentCondition.OpenParenthese = string.Empty;
    CurrentCondition.LeftOperand = string.Empty;
    CurrentCondition.Operator = "=";
    CurrentCondition.RightOperand = string.Empty;
    CurrentCondition.CloseParenthese = string.Empty;
    _currentDatDef.CondsClear();
    OnConditionsChanged();
  }


  private void TakeOverConditions()
  {
    if (IsHaving)
      TakeOverConditionsHaving();
    else
      TakeOverConditionsWhere();
  }

  private void TakeOverConditionsHaving()
  {
    if (_currentDatDef is null) return;

    _currentDatDef.GroupCondsClear();

    foreach (var condition in ConditionRows)
    {
      var opWithPrev = condition.OperateWithPrevious switch
      {
        "HAVING" => OpTypes.Where,
        "HAVING NOT" => OpTypes.WhereNot,
        "AND" => OpTypes.And,
        "AND NOT" => OpTypes.AndNot,
        "OR" => OpTypes.Or,
        "OR NOT" => OpTypes.OrNot,
        _ => OpTypes.Nothing
      };

      var op = condition.Operator switch
      {
        "=" => CompTypes.Equal,
        "<>" => CompTypes.Unequal,
        ">" => CompTypes.Greater,
        "<" => CompTypes.Smaller,
        ">=" => CompTypes.GreaterEqual,
        "<=" => CompTypes.SmallerEqual,
        "LIKE" => CompTypes.Like,
        "IN" => CompTypes.In,
        "EXISTS" => CompTypes.Exists,
        "IS NULL" => CompTypes.IsNull,
        "IS NOT NULL" => CompTypes.IsNotNull,
        _ => CompTypes.Nothing
      };

      var tab = string.Empty;
      var field = string.Empty;
      var comptab = string.Empty;
      var compfield = string.Empty;
      var rightValue = string.Empty;


      if (condition.LeftOperand.Count('.') == 1)
      {
        tab = condition.LeftOperand[..condition.LeftOperand.IndexOf('.')];
        field = condition.LeftOperand[(condition.LeftOperand.IndexOf('.') + 1)..];

        if (_currentDatDef.LHtTables.ContainsKey(tab.ToLower()) is false)
        {
          tab = string.Empty;
          field = string.Empty;
        }
      }

      if (condition.RightOperand.Count('.') == 1)
      {
        comptab = condition.RightOperand[..condition.RightOperand.IndexOf('.')];
        compfield = condition.RightOperand[(condition.RightOperand.IndexOf('.') + 1)..];

        if (_currentDatDef.LHtTables.ContainsKey(comptab.ToLower()) is false)
        {
          rightValue = condition.RightOperand;
          comptab = string.Empty;
          compfield = string.Empty;
        }
      }
      else
      {
        rightValue = condition.RightOperand;
      }

      _currentDatDef.GroupCondsAdd(opWithPrev,
        condition.OpenParenthese,
        tab,
        field,
        condition.AggregateOption,
        op,
        rightValue,
        comptab,
        AggregateTypes.Nothing,
        compfield,
        condition.CloseParenthese);
    }

    OnConditionsChanged();
  }


  private void TakeOverConditionsWhere()
  {
    if (_currentDatDef is null) return;

    _currentDatDef.CondsClear();

    foreach (var condition in ConditionRows)
    {
      var opWithPrev = condition.OperateWithPrevious switch
      {
        "WHERE" => OpTypes.Where,
        "WHERE NOT" => OpTypes.WhereNot,
        "AND" => OpTypes.And,
        "AND NOT" => OpTypes.AndNot,
        "OR" => OpTypes.Or,
        "OR NOT" => OpTypes.OrNot,
        _ => OpTypes.Nothing
      };

      var op = condition.Operator switch
      {
        "=" => CompTypes.Equal,
        "<>" => CompTypes.Unequal,
        ">" => CompTypes.Greater,
        "<" => CompTypes.Smaller,
        ">=" => CompTypes.GreaterEqual,
        "<=" => CompTypes.SmallerEqual,
        "LIKE" => CompTypes.Like,
        "IN" => CompTypes.In,
        "EXISTS" => CompTypes.Exists,
        "IS NULL" => CompTypes.IsNull,
        "IS NOT NULL" => CompTypes.IsNotNull,
        _ => CompTypes.Nothing
      };

      var tab = string.Empty;
      var field = string.Empty;
      var leftValue = string.Empty;
      var comptab = string.Empty;
      var compfield = string.Empty;
      var rightValue = string.Empty;


      if (condition.LeftOperand.Count('.') == 1)
      {
        tab = condition.LeftOperand[..condition.LeftOperand.IndexOf('.')];
        field = condition.LeftOperand[(condition.LeftOperand.IndexOf('.') + 1)..];

        if (_currentDatDef.LHtTables.ContainsKey(tab.ToLower()) is false)
        {
          leftValue = condition.LeftOperand;
          tab = string.Empty;
          field = string.Empty;
        }
      }
      else
      {
        leftValue = condition.LeftOperand;
      }

      if (condition.RightOperand.Count('.') == 1)
      {
        comptab = condition.RightOperand[..condition.RightOperand.IndexOf('.')];
        compfield = condition.RightOperand[(condition.RightOperand.IndexOf('.') + 1)..];

        if (_currentDatDef.LHtTables.ContainsKey(comptab.ToLower()) is false)
        {
          rightValue = condition.RightOperand;
          comptab = string.Empty;
          compfield = string.Empty;
        }
      }
      else
      {
        rightValue = condition.RightOperand;
      }

      _currentDatDef.CondsAdd(opWithPrev,
        condition.OpenParenthese,
        tab,
        AggregateTypes.Nothing,
        field,
        leftValue,
        op,
        rightValue,
        comptab,
        AggregateTypes.Nothing,
        compfield,
        condition.CloseParenthese,
        false);
    }

    OnConditionsChanged();
  }


  private void AddRow()
  {
    var cond = new DatDefCond(OpTypes.And, string.Empty, string.Empty, string.Empty, string.Empty,
      AggregateTypes.Nothing,
      CompTypes.Equal, string.Empty, string.Empty, AggregateTypes.Nothing, string.Empty, string.Empty, 0,
      DbFieldType.Unknown);
    AddConditionGridRow(cond);
  }

  private void OnConditionRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    UpdateCondsPreviewText();
  }

  private void UpdateCondsPreviewText()
  {
    var activeRows = ConditionRows.Where(x => x.IsEnabled).ToList();
    if (activeRows.Count == 0)
    {
      CondsPreviewText = "WHERE";
      return;
    }

    var sb = new StringBuilder();
    foreach (var row in activeRows)
    {
      if (!string.Equals(row.OperateWithPrevious, "None", StringComparison.OrdinalIgnoreCase))
        sb.Append(' ').Append(row.OperateWithPrevious.ToUpperInvariant());
      sb.Append(' ').Append(row.OpenParenthese);
      var left = string.IsNullOrWhiteSpace(row.LeftOperand) ? string.Empty : row.LeftOperand;
      var op = string.IsNullOrWhiteSpace(row.Operator) ? "=" : row.Operator;
      var right = string.IsNullOrWhiteSpace(row.RightOperand) ? string.Empty : row.RightOperand;
      sb.Append(' ').Append($"{left} {op} {right}");
      sb.Append(' ').Append(row.CloseParenthese);
    }

    CondsPreviewText = sb.ToString();
  }

  /// <summary>
  ///   Populates the condition rows from DatDef conditions.
  /// </summary>
  public void SyncFromDatDef(DatDef datDef)
  {
    _currentDatDef = datDef;

    if (IsHaving)
      SyncFromDatDefHaving();
    else
      SyncFromDatDefWhere();
  }

  public void SyncFromDatDefHaving()
  {
    ConditionRows.Clear();

    if (_currentDatDef == null) return;

    if (_currentDatDef.GroupCondsCount == 0)
    {
      _currentDatDef.GroupCondsAdd(OpTypes.Where, string.Empty, string.Empty, string.Empty, AggregateTypes.Nothing,
        CompTypes.Equal, string.Empty, string.Empty, AggregateTypes.Nothing, string.Empty, string.Empty);
      for (var i = 0; i < _currentDatDef.GroupCondsCount; i++)
      {
        var cond = _currentDatDef.GroupConds(i);
        cond?.UseMe = false;
      }
    }

    for (var i = 0; i < _currentDatDef.GroupCondsCount; i++) AddConditionGridRow(_currentDatDef.GroupConds(i)!);

    UpdateCondsPreviewText();
  }

  public void SyncFromDatDefWhere()
  {
    ConditionRows.Clear();
    if (_currentDatDef == null) return;

    if (_currentDatDef.CondsCount == 0)
    {
      _currentDatDef.CondsAdd(OpTypes.Where, string.Empty, string.Empty, string.Empty, CompTypes.Equal, string.Empty,
        string.Empty);
      // TODO: unbedingt refaktorisieren....
      for (var i = 0; i < _currentDatDef.CondsCount; i++)
      {
        var cond = _currentDatDef.Conds(i);
        cond?.UseMe = false;
      }
    }

    for (var i = 0; i < _currentDatDef.CondsCount; i++) AddConditionGridRow(_currentDatDef.Conds(i)!);

    UpdateCondsPreviewText();
  }

  private void AddConditionGridRow(DatDefCond cond)
  {
    var row = new ConditionRowViewModel
    {
      IsEnabled = true,
      IsHaving = IsHaving,
      RowNumber = ConditionRows.Count,
      OperateWithPrevious = cond.OpType switch
      {
        OpTypes.Where => IsHaving ? "HAVING" : "WHERE",
        OpTypes.WhereNot => IsHaving ? "HAVING NOT" : "WHERE NOT",
        OpTypes.And => "AND",
        OpTypes.AndNot => "AND NOT",
        OpTypes.Or => "OR",
        OpTypes.OrNot => "OR NOT",
        _ => ""
      },
      OpenParenthese = cond.BracksOpen,
      AggregateOption = cond.FieldAgg,
      LeftOperand = cond.Table.Length > 0 ? $"{cond.Table}.{cond.Field}" : string.Empty,
      Operator = cond.CompType switch
      {
        CompTypes.Equal => "=",
        CompTypes.Unequal => "<>",
        CompTypes.Greater => ">",
        CompTypes.Smaller => "<",
        CompTypes.GreaterEqual => ">=",
        CompTypes.SmallerEqual => "<=",
        CompTypes.Like => "LIKE",
        CompTypes.In => "IN",
        CompTypes.Exists => "EXISTS",
        CompTypes.IsNull => "IS NULL",
        CompTypes.IsNotNull => "IS NOT NULL",
        _ => ""
      },

      RightOperand = cond.CompTable.Length > 0 ? $"{cond.CompTable}.{cond.CompField}" : cond.CompValue,

      RightOperandType = "Literal",
      CloseParenthese = cond.BracksClose,
      CurrentDatDef = _currentDatDef
    };

    row.PropertyChanged += OnConditionRowPropertyChanged;
    ConditionRows.Add(row);
  }

  public sealed class ConditionRowViewModel : ViewModelBase
  {
    private readonly List<string> _allOperators = [];
    private readonly List<string> _withPreviousOperators = [];
    private int _countCloseParenthese;
    private int _countOpenParenthese;
    private int _curOperator;
    private int _curPrevOperator;

    public ConditionRowViewModel()
    {
      DecreaseOpenParentheseCommand = ReactiveCommand.Create(DecreaseOpenParenthese);
      IncreaseOpenParentheseCommand = ReactiveCommand.Create(IncreaseOpenParenthese);

      DecreaseCloseParentheseCommand = ReactiveCommand.Create(DecreaseCloseParenthese);
      IncreaseCloseParentheseCommand = ReactiveCommand.Create(IncreaseCloseParenthese);

      DecreaseOperatorCommand = ReactiveCommand.Create(DecreaseOperator);
      IncreaseOperatorCommand = ReactiveCommand.Create(IncreaseOperator);
      DecreasePreviousOperatorCommand = ReactiveCommand.Create(DecreasePrevOperator);
      IncreasePreviousOperatorCommand = ReactiveCommand.Create(IncreasePrevOperator);
      _allOperators.AddRange("=", "<>", ">", "<", ">=", "<=", "LIKE", "IN", "EXISTS", "IS NULL", "IS NOT NULL");
      _withPreviousOperators.AddRange("WHERE", "WHERE NOT", "AND", "AND NOT", "OR", "OR NOT");
      AggregateOptions.AddRange(AggregateTypes.Count, AggregateTypes.Sum, AggregateTypes.Min, AggregateTypes.Max,
        AggregateTypes.Avg);
    }

    public ICommand DecreaseCloseParentheseCommand { get; }

    public ICommand IncreaseCloseParentheseCommand { get; }

    public ICommand DecreaseOpenParentheseCommand { get; }

    public ICommand IncreaseOpenParentheseCommand { get; }

    public ICommand DecreaseOperatorCommand { get; }

    public ICommand IncreaseOperatorCommand { get; }

    public ICommand DecreasePreviousOperatorCommand { get; }

    public ICommand IncreasePreviousOperatorCommand { get; }

    public bool IsHaving
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
        _withPreviousOperators.Clear();
        if (value)
          _withPreviousOperators.AddRange("HAVING", "HAVING NOT", "AND", "AND NOT", "OR", "OR NOT");
        else
          _withPreviousOperators.AddRange("WHERE", "WHERE NOT", "AND", "AND NOT", "OR", "OR NOT");
      }
    } = true;

    public bool IsEnabled
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = true;

    public string OperateWithPrevious
    {
      get;
      set
      {
        _curPrevOperator = _withPreviousOperators.IndexOf(value);
        if (_curPrevOperator < 0) _curPrevOperator = 0;

        this.RaiseAndSetIfChanged(ref field, _withPreviousOperators[_curPrevOperator]);
        this.RaisePropertyChanged();
      }
    } = string.Empty;

    public string OpenParenthese
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = string.Empty;

    public string OpenParentheseToolTip
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = string.Empty;

    public string CloseParentheseToolTip
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = string.Empty;

    public List<string> FieldList
    {
      get;
      set => this.RaiseAndSetIfChanged(ref field, value);
    } = [];

    public List<AggregateTypes> AggregateOptions
    {
      get;
      set => this.RaiseAndSetIfChanged(ref field, value);
    } = [];

    public AggregateTypes AggregateOption
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = AggregateTypes.Nothing;

    public string LeftOperand
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = string.Empty;

    public string Operator
    {
      get;
      set
      {
        _curOperator = _allOperators.IndexOf(value);
        if (_curOperator < 0) _curOperator = 0;

        this.RaiseAndSetIfChanged(ref field, _allOperators[_curOperator]);
        this.RaisePropertyChanged();
      }
    } = "=";

    public string RightOperandType
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = "Literal";

    public string RightOperand
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = string.Empty;

    public string CloseParenthese
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = string.Empty;

    public int RowNumber
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        JoinWithPreviousEnabled = field > 0;
        this.RaisePropertyChanged();
      }
    } = 0;

    public bool JoinWithPreviousEnabled
    {
      get;

      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
      }
    } = false;

    public DatDef? CurrentDatDef
    {
      get;
      set
      {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged();
        if (value != null) RefreshFields();
      }
    }

    private void DecreaseCloseParenthese()
    {
      if (_countCloseParenthese > 0) _countCloseParenthese--;
      CloseParenthese = new string(')', _countCloseParenthese);
      CloseParentheseToolTip = _countCloseParenthese + " schließende Klammern";
    }

    private void IncreaseCloseParenthese()
    {
      _countCloseParenthese++;
      CloseParenthese = new string(')', _countCloseParenthese);
      CloseParentheseToolTip = _countCloseParenthese + " schließende Klammern";
    }

    private void DecreaseOpenParenthese()
    {
      if (_countOpenParenthese > 0) _countOpenParenthese--;
      OpenParenthese = new string('(', _countOpenParenthese);
      OpenParentheseToolTip = _countOpenParenthese + " öffnende Klammern";
    }

    private void IncreaseOpenParenthese()
    {
      _countOpenParenthese++;
      OpenParenthese = new string('(', _countOpenParenthese);
      OpenParentheseToolTip = _countOpenParenthese + " öffnende Klammern";
    }

    private void DecreaseOperator()
    {
      if (_curOperator > 0)
        _curOperator--;
      else
        _curOperator = _allOperators.Count - 1;

      Operator = _allOperators[_curOperator];
    }

    private void IncreaseOperator()
    {
      if (_curOperator < _allOperators.Count - 1)
        _curOperator++;
      else
        _curOperator = 0;

      Operator = _allOperators[_curOperator];
    }

    private void DecreasePrevOperator()
    {
      if (_curPrevOperator > 0)
        _curPrevOperator--;
      else
        _curPrevOperator = _withPreviousOperators.Count - 1;

      OperateWithPrevious = _withPreviousOperators[_curPrevOperator];
    }

    private void IncreasePrevOperator()
    {
      if (_curPrevOperator < _withPreviousOperators.Count - 1)
        _curPrevOperator++;
      else
        _curPrevOperator = 0;

      OperateWithPrevious = _withPreviousOperators[_curPrevOperator];
    }

    private void RefreshFields()
    {
      var fieldListNew = new List<string>();

      var dict = CurrentDatDef!.DbConnect.DataDic;
      foreach (var tab in CurrentDatDef!.Tables)
      {
        var dictTable = dict.Tables.First(x => x.TableName == tab.Name);
        fieldListNew.AddRange(dictTable.AllColumns.Select(col => tab.Name + "." + col.ColName));
      }

      FieldList.Clear();
      FieldList.Add(fieldListNew.OrderBy(x => x));
    }
  }
}