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
//////////////////////////////
using StatApp.ModelView;
using StatApp.Pages;
////////////////////////////////////
namespace StatApp
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        private MainModelView m_model;
        //
        public MainWindow()
        {
            InitializeComponent();
            m_model = new MainModelView(ServiceLocator.GetInstance(),PageLocator.GetInstance(),this.NavigationService);
            this.DataContext = m_model;
        }

        private void NavigationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Object obj = this.DataContext;
            if ((obj != null) && (obj is MainModelView))
            {
                MainModelView model = obj as MainModelView;
                model.RefreshDataSets();
                model.NavigateToPage(PageLocator.DATASETS_PAGE);
            }
        }
    }
}
