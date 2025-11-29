// Variables
const string POSTS_DIR = "./src/posts/";
const string POST_TEMPLATE = "./src/_templates/posts.html";
const string POSTS_DIR_OUTPUT = "./dist/posts";

const string PAGES_DIR = "./src/";
const string PAGES_DIR_OUTPUT = "./dist/";

const string ASSETS_DIR = "./src/assets/";
const string ASSETS_DIR_OUTPUT = "./dist/assets";

const string BODY_PLACEHOLDER = "<!--#CONTENT#-->";

// Helpers
bool IsTemplate(string path) => Path.GetFileName(path).StartsWith('_');

// Setup
Directory.CreateDirectory(POSTS_DIR_OUTPUT);

if (!File.Exists(POST_TEMPLATE))
{
    throw new InvalidOperationException(
        $"File {POST_TEMPLATE} does not exist.");
}

var templateHtml = File.ReadAllText(POST_TEMPLATE);

if (!templateHtml.Contains(BODY_PLACEHOLDER))
{
    throw new InvalidOperationException(
        $"{POST_TEMPLATE} does not contain body placeholder '{BODY_PLACEHOLDER}'.");
}

// Build posts
var postsHtml = Directory
    .GetFiles(POSTS_DIR, "*.html")
    .Select(f => new { Name = Path.GetFileName(f), Content = File.ReadAllText(f) });

foreach (var post in postsHtml)
{
    Console.WriteLine(post.Name);
    var postOutput = templateHtml.Replace(BODY_PLACEHOLDER, post.Content);
    var outputFile = Path.Combine(POSTS_DIR_OUTPUT, post.Name);

    File.WriteAllText(outputFile, postOutput);
}

// Output pages
foreach (var pageFile in Directory.GetFiles(PAGES_DIR, "*.html"))
{
    if (IsTemplate(pageFile))
        continue;

    var fileName = Path.GetFileName(pageFile);
    var outputFile = Path.Combine(PAGES_DIR_OUTPUT, fileName);
    File.Copy(pageFile, outputFile, overwrite: true);
}

// Output assets
Directory.CreateDirectory(ASSETS_DIR_OUTPUT);

foreach (var assetFile in Directory.GetFiles(ASSETS_DIR, "*", new EnumerationOptions { RecurseSubdirectories = true }))
{
    var filePath = assetFile.Replace("./src/assets/", "");
    var outputFile = Path.Combine(ASSETS_DIR_OUTPUT, filePath);

    Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
    File.Copy(assetFile, outputFile, overwrite: true);
}
