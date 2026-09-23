namespace aDataLib;

public class DatDefCond
{
  public DatDefCond(OpTypes opType, string bracksOpen, string table, string field, string leftCompValue,
    AggregateTypes fieldAgg, CompTypes compType, string compValue, string compTable, AggregateTypes compFieldAgg,
    string compField, string bracksClose, int myIndex, DbFieldType fieldType)
  {
    OpType = opType;
    BracksOpen = bracksOpen;
    Table = table;
    Field = field;
    LeftCompValue = leftCompValue;
    FieldAgg = fieldAgg;
    CompType = compType;
    CompValue = compValue;
    CompTable = compTable;
    CompFieldAgg = compFieldAgg;
    CompField = compField;
    BracksClose = bracksClose;
    MyIndex = myIndex;
    FieldType = fieldType;
    if (CompType == CompTypes.In) CompValueList = [];
    UseMe = true;
  }

  public bool UseMe { get; set; }
  public List<object>? CompValueList { get; set; } = [];
  public OpTypes OpType { get; }
  public DbFieldType FieldType { get; }
  public string BracksOpen { get; }
  public string Table { get; }
  public AggregateTypes FieldAgg { get; }
  public string Field { get; }
  public string LeftCompValue { get; set; }
  public CompTypes CompType { get; }
  public string CompValue { get; set; }
  public string CompTable { get; }
  public AggregateTypes CompFieldAgg { get; }
  public string CompField { get; }
  public string BracksClose { get; }
  public int MyIndex { get; }
}