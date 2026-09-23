using System.Globalization;

using aDataLib;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace aSql.ViewModels;

public sealed class DataTreeKindToBrushConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return value switch
    {
      DataTreeNode { Kind: DataTreeNodeKind.RelationsGroup } node => node.Name switch
      {
        "ForeignKeys" => new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x80)), // Teal
        "ReferencedBy" => new SolidColorBrush(Color.FromRgb(0x80, 0x00, 0x80)), // Purple
        _ => new SolidColorBrush(Color.FromRgb(0xB2, 0x22,
          0x22)) // Firebrick (default relations group)
      },
      DataTreeNode node => node.Kind switch
      {
        DataTreeNodeKind.Database =>
          new SolidColorBrush(Color.FromRgb(0x1E, 0x90, 0xFF)), // DodgerBlue
        DataTreeNodeKind.Table => new SolidColorBrush(Color.FromRgb(0x22, 0x8B, 0x22)), // ForestGreen
        DataTreeNodeKind.SqlTablesGroup =>
          new SolidColorBrush(Color.FromRgb(0x22, 0x8B, 0x22)), // ForestGreen (table)
        DataTreeNodeKind.ColumnsGroup =>
          new SolidColorBrush(Color.FromRgb(0x8A, 0x2B, 0xE2)), // BlueViolet
        DataTreeNodeKind.Column =>
          new SolidColorBrush(Color.FromRgb(0x48, 0x3D, 0x8B)), // DarkSlateBlue
        DataTreeNodeKind.IndexesGroup =>
          new SolidColorBrush(Color.FromRgb(0xFF, 0x8C, 0x00)), // DarkOrange
        DataTreeNodeKind.SqlOrderByGroup =>
          new SolidColorBrush(Color.FromRgb(0xFF, 0x8C, 0x00)), // DarkOrange (indexes)
        DataTreeNodeKind.Index => new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)), // Orange
        DataTreeNodeKind.IndexColumn => new SolidColorBrush(Color.FromRgb(0xCD, 0x85, 0x3F)), // Peru
        DataTreeNodeKind.Select => new SolidColorBrush(Color.FromRgb(0x1E, 0x90, 0xFF)), // DodgerBlue
        DataTreeNodeKind.SqlJoinsGroup =>
          new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x80)), // Teal (foreign keys)
        DataTreeNodeKind.SqlJoin =>
          new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x80)), // Teal (foreign key join)
        DataTreeNodeKind.SqlWhereGroup =>
          new SolidColorBrush(Color.FromRgb(0xDC, 0x14, 0x3C)), // Crimson (WHERE conditions)
        DataTreeNodeKind.SqlGroupByGroup =>
          new SolidColorBrush(Color.FromRgb(0x32, 0xCD, 0x32)), // LimeGreen (GROUP BY)
        DataTreeNodeKind.SqlHavingGroup =>
          new SolidColorBrush(Color.FromRgb(0x80, 0x00, 0x80)), // Purple (HAVING conditions)
        DataTreeNodeKind.Relation => node.Parent?.Name switch
        {
          "ForeignKeys" => new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x80)), // Teal
          "ReferencedBy" => new SolidColorBrush(Color.FromRgb(0x80, 0x00, 0x80)), // Purple
          _ => new SolidColorBrush(Color.FromRgb(0xB2, 0x22, 0x22)) // Firebrick (default relations group)
        },
        DataTreeNodeKind.RelationColumn =>
          node.Tag is DictRelation.DictRelationColumn { MyMotherRelation: not null } column
            ? column.MyMotherRelation.RelationType switch
            {
              RelationTypes.FKey => new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x80)), // Teal
              RelationTypes.RefBy => new SolidColorBrush(Color.FromRgb(0x80, 0x00, 0x80)), // Purple
              _ => new SolidColorBrush(Color.FromRgb(0xB2, 0x22, 0x22)) // Firebrick (default relations group)
            }
            : Brushes.Gray,
        _ => Brushes.Gray
      },
      _ => Brushes.Transparent
    };
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotSupportedException();
  }
}