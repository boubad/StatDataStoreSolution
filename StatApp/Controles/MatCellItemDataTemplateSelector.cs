using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
namespace StatApp.Controles
{
    public class DisplayItemDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Window window = Application.Current.MainWindow;
            if ((item != null) && (item is DisplayItem))
            {
                DisplayItem cell = item as DisplayItem;
                DisplayItemType type = cell.DisplayType;
                if (type == DisplayItemType.eDisplayNumber)
                {
                    return window.FindResource("display_double_template") as DataTemplate;
                }
                else if (type == DisplayItemType.eDisplayIndex)
                {
                    return window.FindResource("display_index_template") as DataTemplate;
                }
                else if (type == DisplayItemType.eDisplayText)
                {
                    return window.FindResource("display_text_template") as DataTemplate;
                }
                else if (type == DisplayItemType.eDisplayTitle)
                {
                    return window.FindResource("display_title_template") as DataTemplate;
                }
                else if (type ==DisplayItemType.eDisplayImage)
                {
                        return window.FindResource("display_image_template") as DataTemplate;
                }
            }
            return window.FindResource("display_default_template") as DataTemplate;
        }
    }// class DisplayItemDataTemplateSelector
    public class MatCellItemDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Window window = Application.Current.MainWindow;
            if ((item != null) && (item is MatCellItem))
            {
                MatCellItem cell = item as MatCellItem;
                MatDisplayMode mode = cell.DisplayMode;
                MatItemType type = cell.CellType;
                if (type == MatItemType.typeInd)
                {
                    return window.FindResource("cell_indiv_template") as DataTemplate;
                }
                else if (type == MatItemType.typeVar)
                {
                    return window.FindResource("cell_var_template") as DataTemplate;
                }
                else if (type == MatItemType.typeSummary)
                {
                    return window.FindResource("cell_sum_template") as DataTemplate;
                }
                else if (mode == MatDisplayMode.modeGrayscale)
                {
                    if ((type == MatItemType.typeValInf) || (type == MatItemType.typeValSup))
                    {
                        return window.FindResource("cell_grayscale_template") as DataTemplate;
                    }
                }
                else if ((type == MatItemType.typeValInf) || (type == MatItemType.typeValSup))
                {
                    return window.FindResource("cell_inf_template") as DataTemplate;
                }
            }
            return window.FindResource("cell_default_template") as DataTemplate;
        }
    }// class MatCellItemDataTemplateSelector
}
