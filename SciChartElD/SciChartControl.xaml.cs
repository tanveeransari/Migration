using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.ChartModifiers;
using Abt.Controls.SciChart.Visuals;
using Abt.Controls.SciChart.Visuals.Annotations;
using Abt.Controls.SciChart.Visuals.RenderableSeries;

namespace SciChartElD
{
    public partial class SciChartControl : UserControl
    {
        public SciChartControl()
        {
            InitializeComponent();
            this.InitializeHereNotInViewModel();

            //this.chartSurface.MouseDoubleClick += (s, e) =>
            //{
            //    Point mousePosSurface = Mouse.GetPosition(this.chartSurface);
            //    Debug.WriteLine("Surface mouse double click X:0 Y:1 from surface", mousePosSurface.X, mousePosSurface.Y);

            //    IInputElement ctrl = Mouse.DirectlyOver;
            //    Debug.WriteLine("Mouse directly over " + ctrl);

            //    Debug.Assert(ctrl is FrameworkElement, "Mouse was over control that isn't framework element");
            //    FrameworkElement fwe = ctrl as FrameworkElement;
            //    while (fwe.Parent != null && !(fwe is Abt.Controls.SciChart.Visuals.Axes.NumericAxis))
            //    {
            //        fwe = fwe.Parent as FrameworkElement;
            //    }
            //    if (fwe.Parent != null) Debug.WriteLine("Found the Numeric axis");
            //};

        }

        private void InitializeHereNotInViewModel()
        {
            //SciChartGroup.SetVerticalChartGroup(this.priceChart, this.Name + "myGroup");
        }

        internal void HandleKeyDown(object sender, KeyEventArgs e)
        {
            var chartVM = this.DataContext as ViewModel.ChartViewModel;
            Debug.Assert(chartVM != null, "Unable to find chartVM from Control's data context");

            if (e.Key == Key.Delete)
            {
                Debug.WriteLine("Chart Delete Key Pressed");
                if (chartSurface.SelectedRenderableSeries.Count > 0)
                {
                    //TODO: Remove tech indicators
                }
                var selectedAnnotations = chartSurface.Annotations.Where(annotation => annotation.IsSelected).ToList();

                foreach (var selectedAnnotation in selectedAnnotations)
                {
                    chartSurface.Annotations.Remove(selectedAnnotation);
                }
            }
            else if (e.Key == Key.Space)
            {
                Debug.WriteLine("Chart Space Key Pressed");
                //chartVM.QueueResetChartRange();
            }
        }

        private void SeriesSelectionModifier_SelectionChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("SeriesSelectionModifier_SelectionChanged");
            bool hasSelection = chartSurface.SelectedRenderableSeries.Any();
            if (hasSelection)
            {
                foreach (var seri in chartSurface.SelectedRenderableSeries)
                {
                    Debug.WriteLine(seri.GetType());
                    //if (seri is HorizontalLineAnnotation)
                    //{ 
                        
                    //}
                }
            }
        }

        private void CategoryDateTimeAxis_VisibleRangeChanged(object sender, Abt.Controls.SciChart.VisibleRangeChangedEventArgs e)
        {
            Debug.WriteLine("{0} from {1}:{2} to {3}:{4}", "CategoryDateTimeAxis_VisibleRangeChanged ", e.OldVisibleRange.Min, e.OldVisibleRange.Max,
                e.NewVisibleRange.Min, e.NewVisibleRange.Max);
        }

        private void OnAnnotationCreated(object sender, EventArgs e)
        {
            var newAnnotation = (annotationCreation.Annotation as AnnotationBase);
            if (newAnnotation != null)
            {
                newAnnotation.IsEditable = true;
                newAnnotation.CanEditText = true;
            }
            //pointerButton.IsChecked = true;
        }

        private void OnAnnotationTypeChanged(object sender, RoutedEventArgs e)
        {
            //string annotationType = "HorizontalLineAnnotation";
            var type = typeof(HorizontalLineAnnotation);
            var resourceName = String.Format("{0}Style", type.Name);
            if (Resources.Contains(resourceName))
            {
                annotationCreation.AnnotationStyle = (Style)Resources[resourceName];
            }
            annotationCreation.AnnotationType = type;
        }
    }
}
