// Variables
const string POSTS_DIR = "./src/posts/";
const string POST_TEMPLATE = "./src/_templates/posts.html";
const string POSTS_DIR_OUTPUT = "./dist/posts";

const string PAGES_DIR = "./src/";
const string PAGES_DIR_OUTPUT = "./dist/";
const string MAIN_TEMPLATE = "./src/_templates/main.html";

const string ASSETS_DIR = "./src/assets/";
const string ASSETS_DIR_OUTPUT = "./dist/assets";

const string BODY_PLACEHOLDER = "<!--#CONTENT#-->";

// Helpers
bool IsTemplate(string path) => Path.GetFileName(path).StartsWith('_');

string ReadTemplate(string path)
{
    if (!File.Exists(path))
    {
        throw new InvalidOperationException(
            $"File {path} does not exist.");
    }

    var templateHtml = File.ReadAllText(path);

    if (!templateHtml.Contains(BODY_PLACEHOLDER))
    {
        throw new InvalidOperationException(
            $"{path} does not contain body placeholder '{BODY_PLACEHOLDER}'.");
    }

    return templateHtml;
}

// Setup
Directory.CreateDirectory(POSTS_DIR_OUTPUT);

var postTemplate = ReadTemplate(POST_TEMPLATE);
var mainTemplate = ReadTemplate(MAIN_TEMPLATE);

// Build posts
var postsHtml = Directory
    .GetFiles(POSTS_DIR, "*.html")
    .Select(f => new { Name = Path.GetFileName(f), Content = File.ReadAllText(f) });

foreach (var post in postsHtml)
{
    Console.WriteLine($"Post: {post.Name}");
    var postOutput = postTemplate.Replace(BODY_PLACEHOLDER, post.Content);
    var outputFile = Path.Combine(POSTS_DIR_OUTPUT, post.Name);

    File.WriteAllText(outputFile, postOutput);
}

// Output pages
var pagesHtml = Directory
    .GetFiles(PAGES_DIR, "*.html")
    .Select(f => new { Name = Path.GetFileName(f), Content = File.ReadAllText(f) });

foreach (var page in pagesHtml)
{
    if (IsTemplate(page.Name))
        continue;

    Console.WriteLine($"Page: {page.Name}");
    var pageOutput = mainTemplate.Replace(BODY_PLACEHOLDER, page.Content);
    var outputFile = Path.Combine(PAGES_DIR_OUTPUT, page.Name);

    File.WriteAllText(outputFile, pageOutput);
}

// Output assets
Directory.CreateDirectory(ASSETS_DIR_OUTPUT);

foreach (var assetFile in Directory.GetFiles(ASSETS_DIR, "*", new EnumerationOptions { RecurseSubdirectories = true }))
{
    var filePath = assetFile.Replace("./src/assets/", "");
    var outputFile = Path.Combine(ASSETS_DIR_OUTPUT, filePath);

    Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
    File.Copy(assetFile, outputFile, overwrite: true);
}

// Output robots.txt
File.Copy(Path.Combine(PAGES_DIR, "robots.txt"), Path.Combine(PAGES_DIR_OUTPUT, "robots.txt"), overwrite: true);
