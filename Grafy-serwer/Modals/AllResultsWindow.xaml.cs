using Grafy_serwer.Pages;
using Grafy_serwer.RecordsTemplates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Grafy_serwer.Modals
{
    /// <summary>
    /// Logika interakcji dla klasy AllResultsWindow.xaml
    /// </summary>
    public partial class AllResultsWindow : Window
    {
        private List<PathOnGraph> pathOnGraphsList = new List<PathOnGraph>();
        private PathOnGraph selectetPathOnGraph = null;
        private Brush lastSelectetPathOnGraphColor = null;
        private Ellipse lastClickedNode = null;
        private Ellipse startNode = null;
        public ObservableCollection<ResultPathRecord> ResultsRecords { get; set; }
        public ObservableCollection<PathRecord> ResultRecordPath { get; set; }
        public List<Ellipse> localNodes = new List<Ellipse>();
        public List<ReturnObject> returnObjectsList { get; set; } = new List<ReturnObject>();
        public AllResultsWindow()
        {
            InitializeComponent();
            canva.MouseLeftButtonDown += Canva_On_Click_Left;

            ResultsRecords = new ObservableCollection<ResultPathRecord>();
            allResultsListView.ItemsSource = ResultsRecords;

            ResultRecordPath = new ObservableCollection<PathRecord>();
            selectedResult.ItemsSource = ResultRecordPath;
        }
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Pobierz skalę Canvas
            ScaleTransform scaleTransform = canva.LayoutTransform as ScaleTransform;
            if (scaleTransform == null)
            {
                scaleTransform = new ScaleTransform(1, 1);
                canva.LayoutTransform = scaleTransform;
            }

            // Określ czynnik zmiany przybliżenia/oddalenia
            double zoomFactor = 1.2;
            if (e.Delta > 0)
            {
                // Przybliżenie
                scaleTransform.ScaleX *= zoomFactor;
                scaleTransform.ScaleY *= zoomFactor;
            }
            else
            {
                // Oddalenie
                scaleTransform.ScaleX /= zoomFactor;
                scaleTransform.ScaleY /= zoomFactor;
            }

            // Zablokuj propagację zdarzenia
            e.Handled = true;
        }
        private void ModalWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(var returnObject in returnObjectsList)
            {
                foreach(var results in returnObject.results)
                {
                    var endNode = new List<int>();
                    foreach(var path in results.paths)
                    {
                        if(path.Count!=0)
                            endNode.Add(path[path.Count-1]);
                        else
                            endNode.Add(-1);
                    }
                    ResultsRecords.Add(new ResultPathRecord
                    {
                        Clients = returnObject.client,
                        RecieveDate = returnObject.receiveTime.ToString(),
                        BegineDate = results.beginCalculation.ToString(),
                        EndDate = results.endCalculations.ToString(),
                        Path = results.PathToString(),
                        Paths = results.PathToList(),
                        Distance = results.sum.ToString(),
                        IndexPaths = results.paths,
                        StartNode = results.startNode.ToString(),
                        EndNode = endNode
                    });
                    
                    
                }
                
            }
            ResultsRecords = new ObservableCollection<ResultPathRecord>(ResultsRecords.OrderBy(r => r.Distance));
            Generate_Graph();
        }
        private void Generate_Graph()
        {
            
            foreach(var node in GraphEditorPage.nodes)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 10;
                ellipse.Height = 10;
                ellipse.Fill = Brushes.Red;

                Point position = node.position;

                Canvas.SetLeft(ellipse, position.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, position.Y - ellipse.Height / 2);

                canva.Children.Add(ellipse);
                localNodes.Add(ellipse);
               // nodes.Add(new Node { ellipse = ellipse });
            }
            
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            pathOnGraphsList.Clear();
            if (allResultsListView.SelectedItem != null)
            {
                for (int i = canva.Children.Count - 1; i >= 0; i--)
                {
                    if (canva.Children[i] is Line)
                    {
                        canva.Children.RemoveAt(i);
                    }
                }
                // Pobranie obiektu powiązanego z wybranym rekordem
                ResultPathRecord selectedObject = (ResultPathRecord)allResultsListView.SelectedItem;
                ResultRecordPath.Clear();
                for(int i = 0;i<selectedObject.Paths.Count;i++)
                {
                    var newPathRecord = new PathRecord { path = selectedObject.Paths[i], endNode = selectedObject.EndNode[i] };
                    ResultRecordPath.Add(newPathRecord);
                }
                if(startNode!=null)
                    startNode.Fill = Brushes.Red;
                startNode = localNodes[int.Parse(selectedObject.StartNode)];
                startNode.Fill = Brushes.Black;
                foreach (var path in selectedObject.IndexPaths)
                {
                    PathOnGraph pathOnGraph = new PathOnGraph();
                    if(path.Count>0)
                        pathOnGraph.destinationNode = localNodes[path[path.Count-1]];
                    for(int i=0;i<path.Count-1;i++)
                    {


                        Ellipse elNum1 = localNodes[path[i]];
                        Ellipse elNum2 = localNodes[path[i+1]];

                        pathOnGraph.pathParts.Add(drawLine(elNum1, elNum2));

                    }
                    if(path.Count>0)
                    {
                        Ellipse elNum1 = localNodes[int.Parse(selectedObject.StartNode)];
                        Ellipse elNum2 = localNodes[path[0]];
                        pathOnGraph.pathParts.Add(drawLine(elNum1, elNum2));
                    }
                    pathOnGraphsList.Add(pathOnGraph);
                }
                // Tutaj możesz użyć selectedObject do dalszej pracy
            }
        }
        private void ListView_SelectionChanged_Path(object sender, SelectionChangedEventArgs e)
        {
            PathRecord selectedObject = (PathRecord)selectedResult.SelectedItem;
            clearPathToGraph();
            if (selectedObject.endNode == -1)
                return;
            var selectedNode = localNodes[selectedObject.endNode];

            drawPathToNode(selectedNode);

            if (lastClickedNode != null)
            {
                lastClickedNode.Fill = Brushes.Red;
            }
            lastClickedNode = selectedNode;

        }
        private Line drawLine(Ellipse elNum1, Ellipse elNum2)
        {
            Line line = new Line();
            Random random = new Random();
            Color randomColor = Color.FromArgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            SolidColorBrush brush = new SolidColorBrush(randomColor);
            line.Stroke = brush;
            line.Stroke = Brushes.Black;



            line.X1 = Canvas.GetLeft(elNum1) + elNum1.Width / 2; // Punkt początkowy X
            line.Y1 = Canvas.GetTop(elNum1) + elNum1.Height / 2; // Punkt początkowy Y
            line.X2 = Canvas.GetLeft(elNum2) + elNum2.Width / 2; // Punkt końcowy X
            line.Y2 = Canvas.GetTop(elNum2) + elNum2.Height / 2; // Punkt końcowy Y
            line.StrokeThickness = 2;

            canva.Children.Add(line);
            return line;
        }
        private void clearPathToGraph()
        {
            //przywracanie orginalnego koloru ścieżki do poprzednio zaznaczonego węzła
            if (selectetPathOnGraph != null)
            {
                foreach (var pathPart in selectetPathOnGraph.pathParts)
                {
                    pathPart.StrokeThickness = 2;
                    pathPart.Stroke = lastSelectetPathOnGraphColor;
                }
            }
        }
        private void drawPathToNode(Ellipse node)
        {
            if (node == startNode)
                return;
            node.Fill = Brushes.Blue;

            //zaznaczanie ścieżki do wybranego węzła
            selectetPathOnGraph = pathOnGraphsList.Find(p => p.destinationNode == node);
            if (selectetPathOnGraph != null)
            {
                foreach (var pathPart in selectetPathOnGraph.pathParts)
                {
                    pathPart.StrokeThickness = 4;
                    lastSelectetPathOnGraphColor = pathPart.Stroke;
                    pathPart.Stroke = Brushes.Green;
                }
            }
        }
        private void Canva_On_Click_Left(object sender, MouseButtonEventArgs e)
        {
            clearPathToGraph();

            Ellipse tmpElipse = null;
            Point position = e.GetPosition(canva);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(canva, position);
            if (hitTestResult != null && hitTestResult.VisualHit is Ellipse)
            {
                tmpElipse = (Ellipse)hitTestResult.VisualHit;
                drawPathToNode(tmpElipse);
            }
            if(lastClickedNode!=null)
            {
                lastClickedNode.Fill = Brushes.Red;
            }
            lastClickedNode = tmpElipse;
        }

    }
}
