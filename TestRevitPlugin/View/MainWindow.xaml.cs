using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TestRevitAlbum.View.Common;
using TestRevitAlbum.View.Models;
using TestRevitPlugin.View.Models;

using Wpf.Ui.Appearance;

namespace TestRevitPlugin
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        List<KeyValuePair<GroupAlbum, ListView>> groupPosInView = new List<KeyValuePair<GroupAlbum, ListView>>();
        public UIDocument uiDocument { get; }
        public Document document { get; }
        private List<Album> albums = new List<Album>();
        private List<GroupAlbum> groups = new List<GroupAlbum>();
        public MainWindow(UIDocument UiDocument)
        {
            this.uiDocument = UiDocument;
            this.document = UiDocument.Document;
            ColorsProject.initColors();
            InitializeComponent();
            ApplicationThemeManager.Apply(this);
            LoadAlbums();

        }

        private void LoadAlbums()
        {
            try
            {
               
                GetSheetsWithPnkAlbum(document);

                // Обновляем UI только в нужных местах
                UpdateListView();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Ошибка при загрузке альбомов: {ex.Message}");
            }
        }
        private void UpdateListView()
        {
            if (CheckAccess())
            {
                ListAlbum.ItemsSource = albums;
            }
            else
            {
                Dispatcher.Invoke(() => ListAlbum.ItemsSource = albums);
            }
        }
        private void GetSheetsWithPnkAlbum(Document doc)
        {

            var collector = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).OfCategory(BuiltInCategory.OST_Sheets).ToElements();
            foreach (ViewSheet sheet in collector)
            {
                var album_param = sheet.LookupParameter("PNK Альбом").AsValueString();
                if (album_param != null && album_param != "Информация о проекте" && album_param.Length < 6)
                {

                    var res = albums.FirstOrDefault(x => x.AlbumName == album_param);
                    if (res != null) albums.Where(x => x.AlbumName == album_param).First().AlbumSheets.Add(sheet);
                    else
                    {
                        Album album = new Album(album_param);
                        album.AlbumSheets.Add(sheet);
                        albums.Add(album);
                    }

                }
            }
            if (albums.Count > 0)
            {

                albums = albums.OrderBy(x => x.AlbumName).ToList();
            }
            else
            {
                TaskDialog.Show("Ошибка", "Нет нужных листов");
            }
        }

        private void GroupSelectedItems_Click(object sender, RoutedEventArgs e)
        {
            var selectedAlbums = ListAlbum.SelectedItems.Cast<Album>().ToList();
            if (!selectedAlbums.Any()) return;

            GroupAlbum groupAlbum = new GroupAlbum
            {
                name = "Группа " + (groups.Count + 1).ToString(),
                albums = new List<Album>(selectedAlbums)
            };

            foreach (Album album in ListAlbum.SelectedItems)
            {
                albums.Remove(album);
            }
            groups.Add(groupAlbum);
            try
            {
                CreatGroupListView(groupAlbum);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Ошибка при создании группы: {ex.Message}");
            }
            RefreshViewLists();
        
        }

        private void CreatGroupListView(GroupAlbum group)
        {
            string getColor = ColorsProject.getColor();
            if (getColor == "")
            {
                TaskDialog.Show("Ерр", "Достигнут лимит групп");
                return;
            }
            else
            {
                ColorsProject.usedColorGroup(getColor);
                var listView = new ListView
                {
                    SelectionMode = SelectionMode.Extended,
                    AllowDrop = true,
                    Background = new BrushConverter().ConvertFromString(getColor) as SolidColorBrush,
                    ItemTemplate = Resources["AlbumCardTemplate"] as DataTemplate,
                };
                listView.ItemsSource = group.albums;
                listView.MouseMove += ListView_MouseMove;
                listView.Drop += ListView_Drop;


                groupPosInView.Add(new KeyValuePair<GroupAlbum, ListView>(group, listView));
                Dispatcher.Invoke(() => { GroupsStackPanel.Children.Add(listView); });
            }
            
        }

        
        /// <summary>
        /// Создание контекстного меню для одиночного объекта в группе
        /// </summary>
        /// <param name="album"></param>
        /// <returns></returns>
        private ContextMenu CreateAlbumContextMenu(Album album)
        {
            ContextMenu albumContextMenu = new ContextMenu();

            MenuItem deleteAlbumItem = new MenuItem { Header = "Исключить из группы" };
            deleteAlbumItem.Click += (s, e) => RemoveAlbumFromGroup(album);


            albumContextMenu.Items.Add(deleteAlbumItem);

            return albumContextMenu;
        }

        private void RemoveAlbumFromGroup(Album album)
        {
            var group = groupPosInView.Where(x => x.Key.albums.Contains(album)).FirstOrDefault();
            if (group.Key.albums.Count() > 1)
            {
                group.Key.albums.Remove(album);
            }
            else
            {
                GroupsStackPanel.Children.RemoveAt(groupPosInView.IndexOf(group));
                var clr = group.Value.Background as SolidColorBrush;
                ColorsProject.removeFromUsed(ColorsProject.ColorToHex(clr.Color));
                groupPosInView.Remove(group);
                groups.Remove(group.Key);
                foreach (GroupAlbum gr in groups)
                {
                    gr.name = "Группа " + (groups.IndexOf(gr)+1).ToString();
                }
            }
            albums.Add(album);
            albums = albums.OrderBy(x => x.AlbumName).ToList();
            RefreshViewLists();
        }

        private GroupAlbum _draggedFromGroup;

        /// <summary>
        /// Метод для начала работа функции Drag and Drop внутри группы  в момент захвата объекта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ListView listView && listView.SelectedItem is Album album)
            {
                _draggedFromGroup = groupPosInView.FirstOrDefault(g => g.Key.albums.Contains(album)).Key;

                if (_draggedFromGroup != null)
                {
                    DragDrop.DoDragDrop(listView, album, DragDropEffects.Move);
                }
            }
        }

        /// <summary>
        /// Метод для работы функции Drag and Drop внутри группы 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_Drop(object sender, DragEventArgs e)
        {
            
            if (sender is ListView targetListView && e.Data.GetData(typeof(Album)) is Album album)
            {
                
                var targetGroup = groupPosInView.FirstOrDefault(g => g.Value == targetListView).Key;

                
                if (_draggedFromGroup != null && targetGroup != null && _draggedFromGroup == targetGroup)
                {
                    _draggedFromGroup.albums.Remove(album);
                    var targetIndex = GetInsertionIndex(targetListView, e.GetPosition(targetListView));
                    _draggedFromGroup.albums.Insert(targetIndex, album);
                    RefreshViewLists();
                }
            }
        }

        /// <summary>
        /// Метод для поиска позиции при вставке, для функции Drag and Drop
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="dropPosition"></param>
        /// <returns></returns>
        private int GetInsertionIndex(ListView listView, System.Windows.Point dropPosition)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                var item = listView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (item != null)
                {
                    var itemBounds = VisualTreeHelper.GetDescendantBounds(item);
                    var itemPosition = item.TransformToAncestor(listView).Transform(new System.Windows.Point(0, 0));

                    if (dropPosition.Y < itemPosition.Y + itemBounds.Height / 2)
                    {
                        return i;
                    }
                }
            }
            return listView.Items.Count;
        }

        /// <summary>
        /// Функция обновления списков
        /// </summary>
        private void RefreshViewLists()
        {
            Dispatcher.Invoke(() =>
            {
                ListAlbum.ItemsSource = null;
                ListAlbum.ItemsSource = albums;
                if (groupPosInView.Count > 0)
                {

                    foreach (KeyValuePair<GroupAlbum, ListView> kvp in groupPosInView)
                    {

                        kvp.Value.ItemsSource = null;
                        kvp.Value.ItemsSource = kvp.Key.albums;
                    }
                }
            });
        }



        private void OnListViewItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Trace.WriteLine("Preview MouseRightButtonDown");

            e.Handled = true;
        }

        /// <summary>
        /// Функция разгруппировки всех элементов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    foreach (GroupAlbum groupAlbum in groups)
                    {
                        foreach (Album album in groupAlbum.albums)
                        {
                            albums.Add(album);
                        }
                    }
                    groups.Clear();
                    ColorsProject.initColors();
                    albums = albums.OrderBy(x => x.AlbumName).ToList();
                    //ColorsProject.initColors();
                    RefreshViewLists();
                    GroupsStackPanel.Children.Clear();
                
                });
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", ex.Message);
            }

        }

        private void ListAlbum_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ListAlbum.SelectedItems.Count == 1)
            {
                var singleAlbumContextMenu = this.Resources["SingleAlbumContextMenu"] as ContextMenu;
                if (singleAlbumContextMenu != null)
                {
                    var addToGroupMenuItem = singleAlbumContextMenu.Items.OfType<MenuItem>().FirstOrDefault(item => item.Name == "AddToGroupMenuItem");
                    if (groups.Count > 0)
                    {
                        addToGroupMenuItem.Visibility = System.Windows.Visibility.Visible;
                        addToGroupMenuItem.Items.Clear();
                        foreach (var group in groups)
                        {
                            var groupMenuItem = new MenuItem { Header = group.name };
                            groupMenuItem.Click += (s, args) => AddAlbumToGroup(ListAlbum.SelectedItem as Album, group);
                            addToGroupMenuItem.Items.Add(groupMenuItem);
                        }
                    }
                    else
                    {
                        
                        addToGroupMenuItem.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    singleAlbumContextMenu.PlacementTarget = sender as UIElement;
                    singleAlbumContextMenu.IsOpen = true;
                }
            }
            else if (ListAlbum.SelectedItems.Count > 1)
            {
                var multipleAlbumsContextMenu = this.Resources["MultipleAlbumsContextMenu"] as ContextMenu;
                if (multipleAlbumsContextMenu != null)
                {
                    multipleAlbumsContextMenu.PlacementTarget = sender as UIElement;
                    multipleAlbumsContextMenu.IsOpen = true;
                }
            }
            e.Handled = true;
        }

        private void AddAlbumToGroup(Album album, GroupAlbum group)
        {
            if (album != null && group != null)
            {
                group.albums.Add(album);
                albums.Remove(album);
                RefreshViewLists();
            }
        }

        private void TextBlock_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is Album album)
            {
                ContextMenu albumContextMenu = CreateAlbumContextMenu(album);
                albumContextMenu.PlacementTarget = textBlock;
                albumContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void Delete_Group_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuItem menuItem && menuItem.DataContext is Album album)
                {
                    var group = groupPosInView.Where(x => x.Key.albums.Contains(album)).FirstOrDefault();
                    GroupsStackPanel.Children.RemoveAt(groupPosInView.IndexOf(group));
                    foreach(Album al in group.Key.albums) albums.Add(al);
                    var clr = group.Value.Background as SolidColorBrush;
                    ColorsProject.removeFromUsed(ColorsProject.ColorToHex(clr.Color));
                    groupPosInView.Remove(group);
                    groups.Remove(group.Key);
                    foreach (GroupAlbum gr in groups)
                    {
                        gr.name = "Группа " + (groups.IndexOf(gr) + 1).ToString();
                    }
                    albums = albums.OrderBy(x => x.AlbumName).ToList();
                    
                    RefreshViewLists();
                }
            }
            catch (Exception ex) 
            {
                TaskDialog.Show("error", ex.Message);
            }
        }
    }
}
