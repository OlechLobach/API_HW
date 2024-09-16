using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GutenbergApp
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        // Завдання 1: Завантаження тексту "Гамлета"
        private async void DownloadHamlet_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://openlibrary.org/works/OL362173W.json"; // URL "Гамлета" через Open Library
            string jsonResponse = await GetApiResponse(url);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                JObject data = JObject.Parse(jsonResponse);
                string description = data["description"]?.ToString() ?? "No description available";
                HamletTextBox.Text = description;
            }
            else
            {
                MessageBox.Show("Unable to download Hamlet.");
            }
        }

        // Завдання 2 і 4: Завантаження топ-100 книг і обкладинок
        private async void LoadTop100Books_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://openlibrary.org/search.json?q=subject:popular"; // Пошук за популярністю
            string jsonResponse = await GetApiResponse(url);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                DisplayBooksWithCovers(jsonResponse);
            }
            else
            {
                MessageBox.Show("No books found.");
            }
        }

        private void DisplayBooksWithCovers(string jsonResponse)
        {
            var data = JObject.Parse(jsonResponse);
            var books = data["docs"];
            BooksListBox.Items.Clear();

            foreach (var book in books)
            {
                string title = book["title"]?.ToString() ?? "Unknown title";
                BooksListBox.Items.Add(title);
            }
        }

        private async void BooksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksListBox.SelectedItem != null)
            {
                string selectedBook = BooksListBox.SelectedItem.ToString();
                string url = $"https://openlibrary.org/search.json?title={selectedBook}";
                string jsonResponse = await GetApiResponse(url);

                var data = JObject.Parse(jsonResponse);
                var book = data["docs"]?[0];

                if (book != null)
                {
                    SelectedBookTextBlock.Text = book["first_sentence"]?.ToString() ?? "No description available";

                    string coverId = book["cover_i"]?.ToString();
                    if (!string.IsNullOrEmpty(coverId))
                    {
                        BookCoverImage.Source = new BitmapImage(new Uri($"https://covers.openlibrary.org/b/id/{coverId}-L.jpg"));
                    }
                }
            }
        }

        // Завдання 3: Пошук книг
        private async void SearchBooks_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text;
            string url = $"https://openlibrary.org/search.json?q={searchText}";
            string jsonResponse = await GetApiResponse(url);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                DisplaySearchResults(jsonResponse);
            }
            else
            {
                MessageBox.Show("No search results found.");
            }
        }

        private void DisplaySearchResults(string jsonResponse)
        {
            var data = JObject.Parse(jsonResponse);
            var books = data["docs"];
            SearchResultsListBox.Items.Clear();

            foreach (var book in books)
            {
                string title = book["title"]?.ToString() ?? "Unknown title";
                SearchResultsListBox.Items.Add(title);
            }
        }

        private async void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem != null)
            {
                string selectedBook = SearchResultsListBox.SelectedItem.ToString();
                string url = $"https://openlibrary.org/search.json?title={selectedBook}";
                string jsonResponse = await GetApiResponse(url);

                var data = JObject.Parse(jsonResponse);
                var book = data["docs"]?[0];

                if (book != null)
                {
                    SearchSelectedBookTextBlock.Text = book["first_sentence"]?.ToString() ?? "No description available";
                }
            }
        }

        // Завдання 5: Завантаження книг автора
        private async void DownloadAuthorBooks_Click(object sender, RoutedEventArgs e)
        {
            string author = AuthorTextBox.Text;
            string url = $"https://openlibrary.org/search.json?author={author}";
            string jsonResponse = await GetApiResponse(url);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                await DownloadBooks(jsonResponse);
            }
            else
            {
                MessageBox.Show("No books found for this author.");
            }
        }

        private async Task DownloadBooks(string jsonResponse)
        {
            var data = JObject.Parse(jsonResponse);
            var books = data["docs"];
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AuthorBooks");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var book in books)
            {
                string title = book["title"]?.ToString() ?? "Unknown title";
                string bookFilePath = Path.Combine(folderPath, $"{title}.txt");

                await File.WriteAllTextAsync(bookFilePath, book["first_sentence"]?.ToString() ?? "No content available");
            }

            DownloadStatusTextBlock.Text = $"Books by author downloaded to {folderPath}";
        }

        // Метод для завантаження JSON даних з API
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

        // Методи для обробки фокусу в текстових полях
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Enter search term" || textBox.Text == "Enter author name")
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Name == "SearchTextBox" ? "Enter search term" : "Enter author name";
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}