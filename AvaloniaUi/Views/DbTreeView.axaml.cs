using aDataLib;

using aSql.ViewModels;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

using AControl = Avalonia.Controls.Control;

namespace aSql.Views;

public partial class DbTreeView : UserControl
{
  public DbTreeView()
  {
    InitializeComponent();
  }

  private void OnNodeDoubleTapped(object? sender, TappedEventArgs e)
  {
    HandleOnNodeDoubleTapped(sender, e);
  }

  private async void HandleOnNodeDoubleTapped(object? sender, TappedEventArgs e)
  {
    try
    {
      if (!TryGetViewModel(out var vm) || sender is not AControl source ||
          source.DataContext is not DataTreeNode node) return;
      e.Handled = true;
      if (vm == null) return;
      await HandleNodeActionAsync(vm, node);
    }
    catch (Exception ex)
    {
      await MessageBoxManager.GetMessageBoxStandard("Fehler", ex.Message, ButtonEnum.Ok, Icon.Error)
        .ShowWindowDialogAsync(null!);
    }
  }

  private bool TryGetViewModel(out MainWindowViewModel? vm)
  {
    vm = null;
    if (TopLevel.GetTopLevel(this) is not Window { DataContext: MainWindowViewModel viewModel }) return false;
    vm = viewModel;
    return true;
  }

  private static async Task HandleNodeActionAsync(MainWindowViewModel vm, DataTreeNode node)
  {
    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
    switch (node.Kind)
    {
      case DataTreeNodeKind.Table:
        vm.SqlEditor.AddTableToDefinition(node, true, true);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);
        vm.SqlEditor.ExecuteCommand.Execute(null);
        break;
      case DataTreeNodeKind.Column:
        HandleColumnNode(vm, node);
        break;
      case DataTreeNodeKind.Index:
        HandleIndexNode(vm, node);
        break;
      case DataTreeNodeKind.RelationsGroup:
        await HandleRelationsGroupNodeAsync(vm, node);
        break;
      case DataTreeNodeKind.Relation:
      case DataTreeNodeKind.RelationColumn:
        HandleRelationNode(vm, node);
        break;
    }
  }

  private static void HandleColumnNode(MainWindowViewModel vm, DataTreeNode node)
  {
    if (node.Tag is not DictTable.DictColumn col) return;
    var tableName = col.MyMotherTable?.TableName ?? string.Empty;
    var columnName = col.ColName;
    if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(columnName)) return;
    vm.SqlEditor.AddColumnToDefinition(tableName, columnName, node.Tag);
  }

  private static void HandleIndexNode(MainWindowViewModel vm, DataTreeNode node)
  {
    if (node.Tag is not DictIndex idx) return;
    var indexTableName = idx.MyMotherTable?.TableName ?? string.Empty;
    if (string.IsNullOrWhiteSpace(indexTableName)) return;
    for (var i = 0; i < idx.ColumnsCount; i++)
    {
      var column = idx[i];
      if (column != null && !string.IsNullOrWhiteSpace(column.ColName))
        vm.SqlEditor.AddSortToDefinition(indexTableName, column.ColName);
    }
  }

  private static Task HandleRelationsGroupNodeAsync(MainWindowViewModel vm, DataTreeNode node)
  {
    try
    {
      foreach (var subNode in node.Children)
      {
        if (subNode.Tag is not DictRelation rel) continue;
        var isForeignKeys = node.Name == "ForeignKeys";
        vm.SqlEditor.AddJoinFromRelation(JoinTypes.LeftJoin, rel, isForeignKeys);
      }

      return Task.CompletedTask;
    }
    catch (Exception exception)
    {
      return Task.FromException(exception);
    }
  }

  private static void HandleRelationNode(MainWindowViewModel vm, DataTreeNode node)
  {
    switch (node.Tag)
    {
      case DictRelation rel:
        var isForeignKeys = node.Parent != null && node.Parent.Name.Contains("ForeignKeys");
        vm.SqlEditor.AddJoinFromRelation(JoinTypes.LeftJoin, rel, isForeignKeys);
        break;
      case DictTable dictTable:
      {
        var relation = FindRelationByNodeName(dictTable, node.Name);
        if (relation != null) vm.SqlEditor.AddJoinFromRelation(JoinTypes.LeftJoin, relation);

        break;
      }
      case DictRelation.DictRelationColumn { MyMotherRelation: not null } relCol:
        vm.SqlEditor.AddJoinFromRelation(JoinTypes.LeftJoin, relCol.MyMotherRelation);
        break;
    }
  }

  private static DictRelation? FindRelationByNodeName(DictTable table, string nodeName)
  {
    var relations = new List<DictRelation>();
    for (var i = 0; i < table.ForeignKeysCount; i++)
    {
      var rel = table.ForeignKeys(i);
      if (rel != null) relations.Add(rel);
    }

    for (var i = 0; i < table.ReferencedByCount; i++)
    {
      var rel = table.ReferencedBy(i);
      if (rel != null) relations.Add(rel);
    }

    if (relations.Count == 0) return null;

    if (string.IsNullOrWhiteSpace(nodeName)) return relations[0];
    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
    foreach (var relation in relations)
      if (string.Equals(BuildRelationDisplayName(relation), nodeName, StringComparison.OrdinalIgnoreCase) ||
          string.Equals(relation.RelationName, nodeName, StringComparison.OrdinalIgnoreCase))
        return relation;

    return relations[0];
  }

  private static string BuildRelationDisplayName(DictRelation relation)
  {
    var suffix = string.Empty;
    if (relation is { OnUpDateCascade: true, OnDeleteCascade: true })
      suffix = "{du}";
    else if (relation.OnDeleteCascade)
      suffix = "{d}";
    else if (relation.OnUpDateCascade) suffix = "{u}";

    return relation.RelationName + suffix;
  }

  private void DataTree_KeyDown(object? sender, KeyEventArgs e)
  {
    if (e.KeyModifiers != KeyModifiers.None || e.Key < Key.A || e.Key > Key.Z)
      return;

    var searchChar = e.Key.ToString().ToLower();

    var rootNode = DataTree.ItemsSource?.Cast<DataTreeNode>().FirstOrDefault();

    var items = rootNode?.Children.ToList();
    if (items == null || !items.Any()) return;

    foreach (var node in items.Where(node => node.Name.StartsWith(searchChar, StringComparison.OrdinalIgnoreCase)))
    {
      DataTree.SelectedItem = node;
      e.Handled = true; 
      break;
    }
  }
}