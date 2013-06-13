using System;
using System.Collections.Generic;
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
using StatApp.ModelView;
namespace StatApp.Controles
{
    /// <summary>
    /// Logique d'interaction pour IndivDataUserControl.xaml
    /// </summary>
    public partial class IndivDataUserControl : UserControl
    {
        public IndivDataUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Object obj = this.DataContext;
            if ((obj != null) && (obj is OrdModelView))
            {
                OrdModelView model = obj as OrdModelView;
                model.NotifyChange("Individus");
            }
        }
    }
}
