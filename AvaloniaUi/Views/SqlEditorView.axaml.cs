using System.ComponentModel;
using aSql.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace aSql.Views;

public partial class SqlEditorView : UserControl
{
  private const double FontZoomStep = 0.5;
  private const double DefaultEditorFontSize = 13.0;
  private const double MinEditorFontSize = 6.0;
  private bool _isEditorSubscribed;
  private bool _isSynchronizing;
  private RegistryOptions? _registryOptions;
  private TextEditor? _sqlEditor;
  private TextMate.Installation? _textMateInstallation;
  private SqlEditorViewModel? _viewModel;

  public SqlEditorView()
  {
    InitializeComponent();
    EnsureEditorInitialized();
    Loaded += OnLoaded;
    this.GetObservable(DataContextProperty).Subscribe(_ =>
    {
      SyncViewModel();
      SyncEditorFromViewModel();
    });
  }

  private void InitializeTextZoom()
  {
    if (this.FindControl<TextEditor>("SqlEditor") is not { } tedt) return;

    tedt.AddHandler(
      PointerWheelChangedEvent,
      OnSqltextPointerWheelChanged,
      RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
      true);
  }

  private void OnSqltextPointerWheelChanged(object? sender, PointerWheelEventArgs e)
  {
    if (sender is not TextEditor || e.KeyModifiers.HasFlag(KeyModifiers.Control) is false) return;

    switch (e.Delta.Y)
    {
      case > 0:
        _sqlEditor?.FontSize += FontZoomStep;
        e.Handled = true;
        break;
      case < 0:
        _sqlEditor?.FontSize = Math.Max(MinEditorFontSize, _sqlEditor.FontSize - FontZoomStep);
        e.Handled = true;
        break;
    }
  }

  private void OnLoaded(object? sender, RoutedEventArgs e)
  {
    EnsureEditorInitialized();
    SyncViewModel();
    SyncEditorFromViewModel();
  }

  private void EnsureEditorInitialized()
  {
    _sqlEditor ??= this.FindControl<TextEditor>("SqlEditor");

    if (_sqlEditor == null || _isEditorSubscribed) return;
    _sqlEditor.TextChanged += OnEditorTextChanged;
    InitializeTextZoom();
    _isEditorSubscribed = true;
    EnableSqlSyntaxHighlighting(_sqlEditor);
  }

  private void StandardSettingsRestoreButton_OnClick(object? sender, RoutedEventArgs e)
  {
    _sqlEditor?.FontSize = DefaultEditorFontSize;
  }

  private async void CopySqlTextButton_OnClick(object? sender, RoutedEventArgs e)
  {
    try
    {
      EnsureEditorInitialized();

      if (_sqlEditor == null) return;

      var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
      if (clipboard is null) return;

      await clipboard.SetTextAsync(_sqlEditor.Text ?? string.Empty);
    }
    catch
    {
      // Ignored
    }
  }

  private void PasteSqlTextButton_OnClick(object? sender, RoutedEventArgs e)
  {
    EnsureEditorInitialized();

    if (_sqlEditor == null) return;

    _ = DoPasteAsync();
  }

  private async Task DoPasteAsync()
  {
    try
    {
      var topLevel = TopLevel.GetTopLevel(this);
      if (topLevel?.Clipboard == null) return;

      var clipboardText = await topLevel.Clipboard.TryGetTextAsync();

      if (!string.IsNullOrEmpty(clipboardText))
      {
        _isSynchronizing = true;
        try
        {
          _sqlEditor!.Text = clipboardText;
        }
        finally
        {
          _isSynchronizing = false;
        }
      }
    }
    catch
    {
      // Ignored
    }
  }

  private void EnableSqlSyntaxHighlighting(TextEditor editor)
  {
    if (_textMateInstallation != null) return;

    _registryOptions = new RegistryOptions(ThemeName.DarkPlus);
    _textMateInstallation = editor.InstallTextMate(_registryOptions);
    _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("sql"));
  }

  private void SyncViewModel()
  {
    if (_viewModel != null)
    {
      _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
      _viewModel.SqlTreeView.PropertyChanged -= OnSqlTreeViewPropertyChanged;
    }

    _viewModel = DataContext as SqlEditorViewModel;

    if (_viewModel == null) return;
    _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    _viewModel.SqlTreeView.PropertyChanged += OnSqlTreeViewPropertyChanged;
    SelectedSqlEditorTab();
  }

  private void OnSqlTreeViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == nameof(SqlTreeViewModel.SelectedNode)) SelectedSqlEditorTab();
  }

  private void SelectedSqlEditorTab()
  {
    var sqlEditor = this.FindControl<TextEditor>("SqlEditor");

    if (sqlEditor == null || _viewModel?.SqlTreeView.SelectedNode == null) return;

    MainTab.SelectedIndex = _viewModel.SqlTreeView.SelectedNode.Kind switch
    {
      DataTreeNodeKind.Database => 0,
      DataTreeNodeKind.Table => 2,
      DataTreeNodeKind.ColumnsGroup => 2,
      DataTreeNodeKind.Column => 2,
      DataTreeNodeKind.IndexesGroup => 6,
      DataTreeNodeKind.Index => 6,
      DataTreeNodeKind.IndexColumn => 6,
      DataTreeNodeKind.RelationsGroup => 3,
      DataTreeNodeKind.Relation => 3,
      DataTreeNodeKind.RelationColumn => 3,
      DataTreeNodeKind.Select => 0,
      DataTreeNodeKind.SqlTablesGroup => 1,
      DataTreeNodeKind.SqlJoinsGroup => 3,
      DataTreeNodeKind.SqlWhereGroup => 4,
      DataTreeNodeKind.SqlHavingGroup => 5,
      DataTreeNodeKind.SqlOrderByGroup => 6,
      DataTreeNodeKind.SqlJoin => 3,
      DataTreeNodeKind.SqlCondition => 4,
      DataTreeNodeKind.SqlGrouping => 5,
      DataTreeNodeKind.SqlOrdering => 6,
      _ => 0
    };
  }

  private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == nameof(SqlEditorViewModel.SqlText)) SyncEditorFromViewModel();
  }

  private void OnEditorTextChanged(object? sender, EventArgs e)
  {
    EnsureEditorInitialized();

    if (_isSynchronizing || _viewModel == null || _sqlEditor == null) return;

    _viewModel.SqlText = _sqlEditor.Text ?? string.Empty;
  }

  private void SyncEditorFromViewModel()
  {
    EnsureEditorInitialized();

    if (_sqlEditor == null || _viewModel == null) return;

    var targetText = _viewModel.SqlText;
    if (string.Equals(_sqlEditor.Text, targetText, StringComparison.Ordinal)) return;

    _isSynchronizing = true;
    try
    {
      _sqlEditor.Text = targetText;
    }
    finally
    {
      _isSynchronizing = false;
    }
  }

  private void MainTab_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
  {
    if (e.Source == null || e.Source.Equals(MainTab) is false) return;

    _viewModel?.RefreshCurrentTabByMaintab(MainTab.SelectedIndex);
  }
}