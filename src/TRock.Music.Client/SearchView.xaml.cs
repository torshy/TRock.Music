using System.Windows.Controls;

namespace TRock.Music.Client
{
    public partial class SearchView : UserControl
    {
        public SearchView(SearchViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
