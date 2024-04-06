using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


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
        private List<Node> nodes = new List<Node>();
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
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 10;
                ellipse.Height = 10;
                ellipse.Fill = Brushes.Red;

                Point position = e.GetPosition(canva);

                Canvas.SetLeft(ellipse, position.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, position.Y - ellipse.Height / 2);

                canva.Children.Add(ellipse);
                nodes.Add(new Node { ellipse = ellipse });
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
                        // Tworzymy nową linię
                        Line line = new Line();
                        line.Stroke = Brushes.Black; // Ustawiamy kolor linii na czarny
                        line.X1 = Canvas.GetLeft(selectedNodes[0]) + selectedNodes[0].Width / 2; // Punkt początkowy X
                        line.Y1 = Canvas.GetTop(selectedNodes[0]) + selectedNodes[0].Height / 2; // Punkt początkowy Y
                        line.X2 = Canvas.GetLeft(selectedNodes[1]) + selectedNodes[1].Width / 2; // Punkt końcowy X
                        line.Y2 = Canvas.GetTop(selectedNodes[1]) + selectedNodes[1].Height / 2; // Punkt końcowy Y
                        line.StrokeThickness = 2; // Grubość linii

                        // Dodajemy linię do kontrolki Canvas
                        canva.Children.Add(line);
                        foreach (var node in nodes)
                        {
                            if (node.ellipse == selectedNodes[0])
                            {
                                node.edgeEndpoints.Add(new EdgeEndpoint { line = line, isFirstPoint = true });
                            }
                            else if (node.ellipse == selectedNodes[1])
                            {
                                node.edgeEndpoints.Add(new EdgeEndpoint { line = line });
                            }
                        }
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
    }
}
