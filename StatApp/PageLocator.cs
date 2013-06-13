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
///////////////////////////////////////
using StatApp.Pages;
///////////////////////////////////////////
namespace StatApp
{
    public class PageLocator : IPageLocator
    {
        public static readonly String DATASETS_PAGE = "datasets";
        public static readonly String GRAPH_PAGE = "graphs";
        public static readonly String PHOTOS_PAGE = "photos";
        public static readonly String CLASSIFICATION_PAGE = "classification";
        public static readonly String EIGEN_PAGE = "eigen";
        //
        private static  PageLocator m_locator = new PageLocator();

        private Dictionary<String, Page> m_dict = new Dictionary<String, Page>();
        private PageLocator()
        {
        }
        public static PageLocator GetInstance()
        {
            return m_locator;
        }
        public Page GetPage(string pageName)
        {
            if (String.IsNullOrWhiteSpace(pageName)){
                return null;
            }
            String ss = pageName.Trim().ToLower();
            if (m_dict.ContainsKey(ss))
            {
                return m_dict[ss];
            }
            if (ss == DATASETS_PAGE)
            {
                m_dict[ss] = new DataSetsPage();
            }
            else if (ss == GRAPH_PAGE)
            {
                m_dict[ss] = new GraphsPage();
            }
            else if (ss == PHOTOS_PAGE)
            {
                m_dict[ss] = new PhotosPage();
            }
            else if (ss == CLASSIFICATION_PAGE)
            {
                m_dict[ss] = new ClassificationPage();
            }
            else if (ss == EIGEN_PAGE)
            {
                m_dict[ss] = new EigenPage();
            }
            else
            {
                return null;
            }
            return m_dict[ss];
        }
    }
}
