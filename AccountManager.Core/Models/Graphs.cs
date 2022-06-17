namespace AccountManager.Core.Models
{
    public class Graphs
    {
        public List<LineGraph> LineGraphs { get; set; } = new List<LineGraph>();
        public List<PieChart> PieCharts { get; set; } = new List<PieChart>();
        public List<BarChart> BarCharts { get; set; } = new List<BarChart>();
    }
}
