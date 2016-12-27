using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BookClient.Data
{
	// NOTE: This class contains all the typical CRUD operations
  public class BookManager
  {
		const string Url = "http://xam150.azurewebsites.net/api/books/";
		private string authorizationKey;

		private async Task<HttpClient> GetClient()
		{
			HttpClient client = new HttpClient();

			//NOTE: This is C#'s version of optional unwrapping (although C# doesn't actually have the built-in safety of optionals)
			if (string.IsNullOrEmpty(authorizationKey))
			{
				//NOTE: Conversion from a JSON string based on a URL login request response to a deserialized human-readable string
				authorizationKey = await client.GetStringAsync(Url + "login");
				authorizationKey = JsonConvert.DeserializeObject<string>(authorizationKey);
			}

			//NOTE: This is how you add headers to your client request
			client.DefaultRequestHeaders.Add("Authorization", authorizationKey);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
			return client;
		}

// CREATE
		public async Task<Book> Add(string title, string author, string genre)
		{
			//NOTE: Constructor is defined and called here explicitly, rather than as part of the class
			Book book = new Book()
			{
				Title = title,
				Authors = new List<string>(new[] { author }),
				ISBN = string.Empty,
				Genre = genre,
				PublishDate = DateTime.Now.Date,
			};

			var client = await GetClient();

			//NOTE: Converts the book object to JSON and uploads it to the API as a StringContent object
			String jsonPost = JsonConvert.SerializeObject(book);
			StringContent content = new StringContent(jsonPost, Encoding.UTF8, "application/json");
			var jsonResponse = await client.PostAsync(Url, content);

			//NOTE: You must convert the jsonResponse content back to a jsonString and then a Book to return it.
			return JsonConvert.DeserializeObject<Book>(await jsonResponse.Content.ReadAsStringAsync());

		}

// READ
		public async Task<IEnumerable<Book>> GetAll()
		{
			//NOTE: Uses method above to get a logged in client.
			var client = await GetClient();

			//NOTE: Conversion from a JSON string based on a URL request response to a deserialized list of Book objects
			string result = await client.GetStringAsync(Url);
			return JsonConvert.DeserializeObject<IEnumerable<Book>>(result);
		}

// UPDATE
		public async Task Update(Book book)
		{
			var client = await GetClient();

			//NOTE: The following takes the parameter provided book and:
			// 1. generates the URL (ISBN-based)
			// 2. Serializes the book into JSON content
			// 3. Posts (or 'puts') the book to the API

			String updateURL = Url + "/" + book.ISBN;
			StringContent content = new StringContent(JsonConvert.SerializeObject(book), Encoding.UTF8, "application/json");
		  await client.PutAsync(updateURL, content);
		}

// DELETE
		public async Task Delete(string isbn)
		{
			var client = await GetClient();
			String deletionURL = Url + "/" + isbn;
			await client.DeleteAsync(deletionURL);
		}
	}
}

