using System.Windows.Controls;

namespace TRock.Music.Client
{
    public partial class SearchResultsView : UserControl
    {
        public SearchResultsView(SearchResultsViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
