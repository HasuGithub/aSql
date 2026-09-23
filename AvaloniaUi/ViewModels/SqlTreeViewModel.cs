using System.Collections.ObjectModel;
using System.Windows.Input;

using aDataLib;

using ReactiveUI;

namespace aSql.ViewModels;

public sealed class SqlTreeViewModel : ViewModelBase
{
  private readonly Action? _sqlChangedCallback;
  private DatDef? _sqlDefinition;

  public SqlTreeViewModel()
  {
    ClearCommand = ReactiveCommand.Create(Clear);
    InitializeBaseNodes();
  }

  public SqlTreeViewModel(DatDef? sqlDefinition, Action? sqlChangedCallback = null)
  {
    _sqlDefinition = sqlDefinition;
    _sqlChangedCallback = sqlChangedCallback;
    ClearCommand = ReactiveCommand.Create(Clear);
    InitializeBaseNodes();
  }

  public ObservableCollection<DataTreeNode>? Root
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = [];

  public DataTreeNode? SelectedNode
  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public ICommand ClearCommand { get; set; }

  public event EventHandler? SqlTreeViewChanged;

  public void Clear()
  {
    _sqlDefinition?.Clear();
    InitializeBaseNodes();
    OnChanged();
  }

  private void OnChanged()
  {
    SqlTreeViewChanged?.Invoke(this, EventArgs.Empty);
  }

  private void InitializeBaseNodes()
  {
    Root?.Clear();
    Root?.Add(new DataTreeNode("select", DataTreeNodeKind.Select, "BaseNode") { IsExpanded = true });

    var firstNode = Root?.First()!;

    firstNode.Children.Add(
      new DataTreeNode("Tables", DataTreeNodeKind.SqlTablesGroup, "BaseNode") { IsExpanded = true });
    firstNode.Children.Add(new DataTreeNode("Joins", DataTreeNodeKind.SqlJoinsGroup, "BaseNode") { IsExpanded = true });
    firstNode.Children.Add(new DataTreeNode("Where", DataTreeNodeKind.SqlWhereGroup, "BaseNode")
      { IsExpanded = false });
    firstNode.Children.Add(new DataTreeNode("Groups", DataTreeNodeKind.SqlGroupByGroup, "BaseNode")
      { IsExpanded = true });
    firstNode.Children.Add(new DataTreeNode("Having", DataTreeNodeKind.SqlHavingGroup, "BaseNode")
      { IsExpanded = false });
    firstNode.Children.Add(
      new DataTreeNode("Sorts", DataTreeNodeKind.SqlOrderByGroup, "BaseNode") { IsExpanded = true });
  }

  public void SyncFromDatDef(DatDef? datDef)
  {
    if (datDef == null) return;

    _sqlDefinition = datDef;

    if (Root != null)
      foreach (var baseNode in Root.First().Children)
        baseNode.Children.Clear();

    var tablesGroup = Root?.First().Children.FirstOrDefault(r => r.Kind == DataTreeNodeKind.SqlTablesGroup);
    if (tablesGroup != null && datDef.TablesCount > 0)
      for (var i = 0; i < datDef.TablesCount; i++)
      {
        var table = datDef[i];
        if (table == null) continue;
        var tableNode = table.Alias.Length == 0
          ? new DataTreeNode(table.Key, DataTreeNodeKind.Table, table.Key) { IsExpanded = true }
          : new DataTreeNode(table.Name + "( " + table.Alias + " )", DataTreeNodeKind.Table, table.Key)
            { IsExpanded = true };

        if (table.FieldsCount > 0)
          for (var f = 0; f < table.FieldsCount; f++)
          {
            var field = table[f];
            if (field == null || string.IsNullOrWhiteSpace(field.Name)) continue;
            tableNode.Children.Add(field.Alias.Length > 0
              ? new DataTreeNode(field.Name + " ( " + field.Alias + " )", DataTreeNodeKind.Column, field.Key)
              : new DataTreeNode(field.Name, DataTreeNodeKind.Column, field.Key));
          }

        tablesGroup.Children.Add(tableNode);
      }

    var joinsGroup = Root?.First().Children.FirstOrDefault(r => r.Kind == DataTreeNodeKind.SqlJoinsGroup);
    if (joinsGroup != null && datDef.JoinsCount > 0)
      for (var i = 0; i < datDef.JoinsCount; i++)
      {
        var join = datDef.Joins(i);
        if (join == null) continue;
        var joinNode = new DataTreeNode(join.Key, DataTreeNodeKind.SqlJoin, join.Tag) { IsExpanded = true };
        joinsGroup.Children.Add(joinNode);
      }

    // TODO: Schwierig hier anzuzeigen ---> mal sehen...
    // Sync Conditions (WHERE)
    //var condsGroup = Roots.FirstOrDefault(r => r.Kind == DataTreeNodeKind.SqlWhereGroup);
    //if (condsGroup != null && datDef.CondsCount > 0)
    //{
    //  for (int i = 0; i < datDef.CondsCount; i++)
    //  {
    //    var cond = datDef.Conds(i);
    //    if (cond != null && !string.IsNullOrWhiteSpace(cond.Table))
    //    {
    //      condsGroup.Children.Add(new DataTreeNode(cond.Table, DataTreeNodeKind.SqlCondition, cond.Tag));
    //    }
    //  }
    //}

    var groupsGroup = Root?.First().Children.FirstOrDefault(r => r.Kind == DataTreeNodeKind.SqlGroupByGroup);
    if (groupsGroup != null && datDef.GroupsCount > 0)
      for (var i = 0; i < datDef.GroupsCount; i++)
      {
        var grp = datDef.Groups(i);
        if (grp != null && !string.IsNullOrWhiteSpace(grp.Key))
          groupsGroup.Children.Add(new DataTreeNode(grp.Key, DataTreeNodeKind.SqlGrouping, grp.Tag));
      }

    // TODO: Schwierig hier anzuzeigen ---> mal sehen...
    // Sync GroupConditions (HAVING)
    //var groupCondsGroup = Roots.FirstOrDefault(r => r.Kind == DataTreeNodeKind.SqlHavingGroup);
    //if (groupCondsGroup != null && datDef.GroupCondsCount > 0)
    //{
    //  for (int i = 0; i < datDef.GroupCondsCount; i++)
    //  {
    //    var cond = datDef.GroupConds(i);
    //    if (cond != null && !string.IsNullOrWhiteSpace(cond.Table))
    //    {
    //      groupCondsGroup.Children.Add(new DataTreeNode(cond.Table, DataTreeNodeKind.SqlCondition, cond.Tag));
    //    }
    //  }
    //}

    // Sync Sorts (ORDER BY)
    var sortsGroup = Root?.First().Children.FirstOrDefault(r => r.Kind == DataTreeNodeKind.SqlOrderByGroup);
    if (sortsGroup == null || datDef.SortsCount <= 0) return;
    {
      for (var i = 0; i < datDef.SortsCount; i++)
      {
        var sort = datDef.Sorts(i);
        if (sort != null && !string.IsNullOrWhiteSpace(sort.Key))
          sortsGroup.Children.Add(new DataTreeNode(sort.Key, DataTreeNodeKind.SqlOrdering, sort.Tag));
      }
    }
  }

  public void RegenerateSqlText()
  {
    _sqlChangedCallback?.Invoke();
  }
}