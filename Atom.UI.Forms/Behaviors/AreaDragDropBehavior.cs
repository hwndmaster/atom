using System.Windows.Controls;
using Genius.Atom.UI.Forms.Wpf;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;

namespace Genius.Atom.UI.Forms;

/// <summary>
///   Makes the selected element ready for drag-n-drop operations with multiple areas,
///   bound by <see cref="DropAreas"/>. It is expected that the aimed control has Grid as a parent element,
///   which is used as a container for the drop overlay.
/// </summary>
public sealed class AreaDragDropBehavior : Behavior<UIElement>
{
    #region IsDraggingProperty
    public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.RegisterAttached(
        "IsDragging", typeof(bool), typeof(AreaDragDropBehavior), new PropertyMetadata(default(bool)));
    public static void SetIsDragging(DependencyObject element, bool value)
        => element.NotNull().SetValue(IsDraggingProperty, value);
    public static bool GetIsDragging(DependencyObject element)
        => (bool)element.NotNull().GetValue(IsDraggingProperty);
    #endregion

    #region DropAreasProperty
    public static readonly DependencyProperty DropAreasProperty =
        DependencyProperty.Register(nameof(DropAreas), typeof(ICollection<DropAreaViewModel>), typeof(AreaDragDropBehavior), new PropertyMetadata(OnDropAreasChanged));
    public ICollection<DropAreaViewModel> DropAreas
    {
        get { return (ICollection<DropAreaViewModel>)GetValue(DropAreasProperty); }
        set { SetValue(DropAreasProperty, value); }
    }
    #endregion

    #region DataFormatProperty
    public static readonly DependencyProperty DataFormatProperty =
        DependencyProperty.Register(nameof(DataFormat), typeof(string), typeof(AreaDragDropBehavior));
    public string DataFormat
    {
        get { return (string)GetValue(DataFormatProperty); }
        set { SetValue(DataFormatProperty, value); }
    }
    #endregion

    private AreaDropOverlay? _dropOverlay;
    private bool _itemsContainerInitialized;

    protected override void OnAttached()
    {
        AssociatedObject.AllowDrop = true;
        AssociatedObject.DragEnter += OnDragEnter;

        _dropOverlay = new AreaDropOverlay
        {
            Visibility = Visibility.Collapsed,
            DataContext = null,
        };
        _dropOverlay.DragEnter += OnDropOverlayDragEnter;
        _dropOverlay.DragLeave += OnDropOverlayDragLeave;

        var grid = AssociatedObject.FindVisualParent<Grid>().NotNull();
        StretchToGrid(_dropOverlay, grid);
        grid.Children.Add(_dropOverlay);

        _itemsContainerInitialized = false;
        _dropOverlay.ItemsControl.Loaded += OnDropOverlayItemsControlLoaded;

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.DragEnter -= OnDragEnter;

        var grid = AssociatedObject.FindVisualParent<Grid>().NotNull();
        grid.Children.Remove(_dropOverlay);
        _dropOverlay = null;

        base.OnDetaching();
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (_dropOverlay is null
            || !e.Data.GetDataPresent(DataFormat))
        {
            return;
        }

        _dropOverlay.Visibility = Visibility.Visible;
    }

    private void OnDropOverlayDragEnter(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormat))
        {
            return;
        }

        e.Effects = DragDropEffects.Link;
        e.Handled = true;
    }

    private void OnDropOverlayDragLeave(object sender, DragEventArgs e)
    {
        if (_dropOverlay is null)
            return;

        _dropOverlay.Visibility = Visibility.Collapsed;
    }

    private void OnDropOverlayItemsControlLoaded(object sender, RoutedEventArgs e)
    {
        var dropAreas = DropAreas;
        var itemsControl = (ItemsControl)sender;

        if (_itemsContainerInitialized
            || itemsControl.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated
            || itemsControl.ItemContainerGenerator.Items.Count != dropAreas.Count)
        {
            return;
        }

        for (var i = 0; i < dropAreas.Count; i++)
        {
            var areaCtrlContainer = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            var areaCtrl = areaCtrlContainer.FindChild<Border>();
            var dropAreaVm = dropAreas.ElementAt(i);
            areaCtrl.AllowDrop = true;
            areaCtrl.Drop += delegate (object sender, DragEventArgs e)
            {
                if (_dropOverlay is null)
                    return;

                var droppedObject = e.Data.GetData(DataFormat);
                if (droppedObject is null)
                {
                    return;
                }

                dropAreaVm.DropAction.Execute(droppedObject);

                _dropOverlay.Visibility = Visibility.Collapsed;
            };
            areaCtrl.DragEnter += (_, __) => SetIsDragging(areaCtrl, true);
            areaCtrl.DragLeave += (_, __) => SetIsDragging(areaCtrl, false);
        }

        _itemsContainerInitialized = true;
    }

    private static void OnDropAreasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (AreaDragDropBehavior)d;
        behavior.SetupItems();
    }

    private void SetupItems()
    {
        if (_dropOverlay is null)
            return;

        _itemsContainerInitialized = false;
        _dropOverlay.DataContext = this;
    }

    private static void StretchToGrid(UIElement element, Grid grid)
    {
        element.SetValue(Grid.RowProperty, 0);
        element.SetValue(Grid.RowSpanProperty, Math.Max(1, grid.RowDefinitions.Count));
        element.SetValue(Grid.ColumnProperty, 0);
        element.SetValue(Grid.ColumnSpanProperty, Math.Max(1, grid.ColumnDefinitions.Count));
    }
}
