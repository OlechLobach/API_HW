using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace SearchApp
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string GoogleApiKey = "AIzaSyARsI5oFhU4r3Hv7_sT78iEHzxDcKX3ngM"; // Ваш API ключ Google
        private const string CustomSearchEngineId = "945e0edd5a54b468c"; // Ваш ID Custom Search Engine

        public MainWindow()
        {
            InitializeComponent();
        }

        // Завдання 1: Пошук інформації в Google
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchTextBox.Text;
            await SearchGoogle(query, false);
        }

        // Завдання 2: Пошук в кількох системах (поки що лише Google)
        private async void MultiSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = MultiSearchTextBox.Text;
            await SearchGoogle(query, true);
        }

        // Завдання 3: Пошук зображень в Google
        private async void ImageSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = ImageSearchTextBox.Text;
            await SearchGoogleImages(query, false);
        }

        // Завдання 4: Пошук зображень в кількох системах (поки що лише Google)
        private async void MultiImageSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = MultiImageSearchTextBox.Text;
            await SearchGoogleImages(query, true);
        }

        private async Task SearchGoogle(string query, bool multiSearch)
        {
            string url = $"https://www.googleapis.com/customsearch/v1?q={query}&key={GoogleApiKey}&cx={CustomSearchEngineId}";
            string jsonResponse = await GetApiResponse(url);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                DisplaySearchResults(jsonResponse, multiSearch);
            }
            else
            {
                MessageBox.Show("No search results found.");
            }
        }

        private async Task SearchGoogleImages(string query, bool multiSearch)
        {
            string url = $"https://www.googleapis.com/customsearch/v1?q={query}&key={GoogleApiKey}&cx={CustomSearchEngineId}&searchType=image";
            string jsonResponse = await GetApiResponse(url);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                DisplayImageResults(jsonResponse, multiSearch);
            }
            else
            {
                MessageBox.Show("No image results found.");
            }
        }

        private void DisplaySearchResults(string jsonResponse, bool multiSearch)
        {
            var data = JObject.Parse(jsonResponse);
            var items = data["items"];
            ListBox targetListBox = multiSearch ? MultiSearchResultsListBox : SearchResultsListBox;
            targetListBox.Items.Clear();

            foreach (var item in items)
            {
                string title = item["title"]?.ToString() ?? "Unknown title";
                string link = item["link"]?.ToString() ?? "No link available";
                targetListBox.Items.Add(new { Title = title, Link = link });
            }
        }

        private void DisplayImageResults(string jsonResponse, bool multiSearch)
        {
            var data = JObject.Parse(jsonResponse);
            var items = data["items"];
            ListBox targetListBox = multiSearch ? MultiImageSearchResultsListBox : ImageResultsListBox;
            targetListBox.Items.Clear();

            foreach (var item in items)
            {
                string title = item["title"]?.ToString() ?? "Unknown title";
                string imageUrl = item["link"]?.ToString() ?? "No image URL available";
                targetListBox.Items.Add(new { Title = title, ImageUrl = imageUrl });
            }
        }

        private async Task<string> GetApiResponse(string url)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // Методи для обробки подій фокусу
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Enter search term")
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Enter search term";
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}