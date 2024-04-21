using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.Json;


namespace Grafy_serwer.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy GrapheditorPage.xaml
    /// </summary>
    public partial class GraphEditorPage : Page
    {
        private Ellipse currentlyClickedNode;
        private Point startClickPoint;
        private bool isNodeAdding = false;
        private bool isEdgeAdding = false;
        private List<Ellipse> selectedNodes = new List<Ellipse>();
        public static List<Node> nodes = new List<Node>();
        private TabControl tabControl;
        public GraphEditorPage()
        {
            InitializeComponent();
            canva.MouseLeftButtonDown += Canva_On_Click_Left;
            canva.MouseRightButtonDown += Canva_On_Click_Right;
            canva.MouseMove += Canva_On_Mouse_Move;
            canva.MouseRightButtonUp += Canva_On_Release_Right;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                tabControl = mainWindow.mainTabControl;
            }
        }
        private void Canva_On_Click_Left(object sender, MouseButtonEventArgs e)
        {
            if (isNodeAdding)
            {
                Point position = e.GetPosition(canva);
                createNodeOnWorkspace(position);
            }
            else if (isEdgeAdding)
            {
                Point position = e.GetPosition(canva);
                HitTestResult hitTestResult = VisualTreeHelper.HitTest(canva, position);
                if (hitTestResult != null && hitTestResult.VisualHit is Ellipse)
                {
                    Ellipse tmpElipse = (Ellipse)hitTestResult.VisualHit;
                    tmpElipse.Fill = Brushes.Blue;
                    // Poprawiamy wywołanie metody selectedNodes, usuwając rzutowanie do Ellipse
                    selectedNodes.Add((Ellipse)hitTestResult.VisualHit);
                    if (selectedNodes.Count == 2)
                    {
                        createEdgeOnWorkspace(selectedNodes[0], selectedNodes[1]);
                     
                        foreach (var node in selectedNodes)
                        {
                            node.Fill = Brushes.Red;
                        }
                        selectedNodes.Clear();

                    }
                }
            }

        }
        private void Canva_On_Click_Right(object sender, MouseButtonEventArgs e)
        {
            // Sprawdzamy, czy kliknięto na elipsę
            Point position = e.GetPosition(canva);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(canva, position);
            if (hitTestResult != null && hitTestResult.VisualHit is Ellipse)
            {
                currentlyClickedNode = hitTestResult.VisualHit as Ellipse;
                startClickPoint = position;
            }
        }
        private void Canva_On_Mouse_Move(object sender, MouseEventArgs e)
        {
            if (currentlyClickedNode != null && e.RightButton == MouseButtonState.Pressed)
            {
                var node = nodes.Find(e => e.ellipse == currentlyClickedNode);
                Point newPosition = e.GetPosition(canva);
                double offsetX = newPosition.X - startClickPoint.X;
                double offsetY = newPosition.Y - startClickPoint.Y;

                foreach (var endpoint in node.edgeEndpoints)
                {
                    if (endpoint.isFirstPoint)
                    {
                        endpoint.line.X1 = Canvas.GetLeft(node.ellipse) + node.ellipse.Width / 2;
                        endpoint.line.Y1 = Canvas.GetTop(node.ellipse) + node.ellipse.Height / 2;
                    }
                    else
                    {
                        endpoint.line.X2 = Canvas.GetLeft(node.ellipse) + node.ellipse.Width / 2;
                        endpoint.line.Y2 = Canvas.GetTop(node.ellipse) + node.ellipse.Height / 2;
                    }
                }

                // Aktualizacja pozycji elipsy
                Canvas.SetLeft(currentlyClickedNode, Canvas.GetLeft(currentlyClickedNode) + offsetX);
                Canvas.SetTop(currentlyClickedNode, Canvas.GetTop(currentlyClickedNode) + offsetY);



                startClickPoint = newPosition;
            }
        }
        private void Canva_On_Release_Right(object sender, MouseButtonEventArgs e)
        {
            currentlyClickedNode = null;
        }

        private void Button_Click_Add_Enge(object sender, RoutedEventArgs e)
        {
            isEdgeAdding = true;
            isNodeAdding = false;
            addNode.Background = (Brush)new BrushConverter().ConvertFrom("#657AAF");
            addEdge.Background = (Brush)new BrushConverter().ConvertFrom("#2e69ff");
        }
        private void Button_Click_Add_Node(object sender, RoutedEventArgs e)
        {
            isEdgeAdding = false;
            isNodeAdding = true;

            addNode.Background = (Brush)new BrushConverter().ConvertFrom("#2e69ff");
            addEdge.Background = (Brush)new BrushConverter().ConvertFrom("#657AAF");
        }

        private void Start_Calculations(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 3;
        }

        private void Bact(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
        }
        public static List<Node> getGrapfNodes()
        {
            return nodes;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Pliki JSON (*.json)|*.json|Wszystkie pliki (*.*)|*.*";
                Nullable<bool> result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // Serializuj listę nodes do JSON
                    //string json = JsonConvert.SerializeObject(nodes);
                    string json = JsonSerializer.Serialize(Generate_Saveable_Graph());
                    // Zapisz JSON do pliku
                    File.WriteAllText(filePath, json);

                    MessageBox.Show("Dane zostały zapisane pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas zapisywania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Obsługa zdarzenia kliknięcia przycisku "Wczytaj"
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Pliki JSON (*.json)|*.json|Wszystkie pliki (*.*)|*.*";
                Nullable<bool> result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    string filePath = openFileDialog.FileName;
                    string json = File.ReadAllText(filePath);
                    GraphStructure structure = JsonSerializer.Deserialize<GraphStructure>(json);

                    foreach(var nodePos in structure.nodePositions)
                    {
                        createNodeOnWorkspace(nodePos);
                    }
                    foreach(var edgeIndexes in structure.edgeEndpoints)
                    {
                        Ellipse first = nodes[edgeIndexes.Item1].ellipse;
                        Ellipse second = nodes[edgeIndexes.Item2].ellipse;
                        createEdgeOnWorkspace(first, second);
                    }
                    MessageBox.Show("Dane zostały wczytane pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas wczytywania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private GraphStructure Generate_Saveable_Graph()
        {
            GraphStructure graphStructure = new GraphStructure();
            int i = 0;
            foreach (Node node in nodes)
            {
                graphStructure.nodePositions.Add(node.position);
                foreach(int index in node.forignNodeIndexes)
                {
                    graphStructure.edgeEndpoints.Add(Tuple.Create(i, index));
                }
                i++;
            }
            return graphStructure;
        }
        private void createNodeOnWorkspace(Point position)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 10;
            ellipse.Height = 10;
            ellipse.Fill = Brushes.Red;

            Canvas.SetLeft(ellipse, position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, position.Y - ellipse.Height / 2);

            canva.Children.Add(ellipse);
            nodes.Add(new Node { ellipse = ellipse, position = position });
        }
        private void createEdgeOnWorkspace(Ellipse firstNode, Ellipse secondNonde)
        {
            Line line = new Line();
            line.Stroke = Brushes.Black; // Ustawiamy kolor linii na czarny
            line.X1 = Canvas.GetLeft(firstNode) + firstNode.Width / 2; // Punkt początkowy X
            line.Y1 = Canvas.GetTop(firstNode) + firstNode.Height / 2; // Punkt początkowy Y
            line.X2 = Canvas.GetLeft(secondNonde) + secondNonde.Width / 2; // Punkt końcowy X
            line.Y2 = Canvas.GetTop(secondNonde) + secondNonde.Height / 2; // Punkt końcowy Y
            line.StrokeThickness = 2; // Grubość linii

            // Dodajemy linię do kontrolki Canvas
            canva.Children.Add(line);

            //Zapisanie indeksów krawędzi
            int firstNodeIndex = -1, secondNodeIndex = -1;
            int i = 0;
            foreach (var node in nodes)
            {
                if (node.ellipse == firstNode)
                {
                    firstNodeIndex = i;
                    node.edgeEndpoints.Add(new EdgeEndpoint { line = line, isFirstPoint = true });
                }
                else if (node.ellipse == secondNonde)
                {
                    secondNodeIndex = i;
                    node.edgeEndpoints.Add(new EdgeEndpoint { line = line });
                }
                i++;
            }
            nodes[firstNodeIndex].forignNodeIndexes.Add(secondNodeIndex);
        }
    }
}
