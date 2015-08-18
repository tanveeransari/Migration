//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Input;
//using Abt.Controls.SciChart;
//using Abt.Controls.SciChart.ChartModifiers;
//using Abt.Controls.SciChart.Visuals;
//using System.Windows.Data;

//namespace SciChartElD.CustomModifiers
//{
//    public class KeyDownChartModifier : ChartModifierBase
//    {
//        public override void OnAttached()
//        {
//            base.OnAttached();
//            var sciChart = (SciChartSurface)ParentSurface;
//            //sciChart.PreviewKeyDown -= sciChart_PreviewKeyDown;
//            //sciChart.PreviewKeyDown += sciChart_PreviewKeyDown;
//            var mainWindow = FindLogicalParent(sciChart);
//            mainWindow.PreviewKeyDown -= mainWindow_PreviewKeyDown;
//            mainWindow.PreviewKeyDown += mainWindow_PreviewKeyDown;
//        }

//        void sciChart_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
//        {
//            Debug.WriteLine("Chart surface preview key down");
//            this.handlePreviewKeyDown(e);
//        }

//        private void mainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
//        {
//            Debug.WriteLine("Main Window preview key down");

//            this.handlePreviewKeyDown(e);
//        }

//        private void handlePreviewKeyDown(System.Windows.Input.KeyEventArgs e)
//        {
//            if (e.Key == System.Windows.Input.Key.Delete)
//            {
//                var sciChart = (SciChartSurface)ParentSurface;
//                var selectedAnnotations = sciChart.Annotations.Where(annotation => annotation.IsSelected).ToList();

//                foreach (var selectedAnnotation in selectedAnnotations)
//                {
//                    sciChart.Annotations.Remove(selectedAnnotation);
//                }
//            }
//        }

//        private FrameworkElement FindLogicalParent(SciChartSurface scichart)// where T:class
//        {
//            var parent = (FrameworkElement)scichart.Parent;
//            while (parent != null)
//            {
//                var candidate = parent as SciChartControl;
//                if (candidate != null) return candidate;

//                parent = (FrameworkElement)parent.Parent;
//            }

//            return null;
//        }
//    }
//}
