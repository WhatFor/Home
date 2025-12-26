// Variables
const string POSTS_DIR = "./src/posts/";
const string POST_TEMPLATE = "./src/_templates/posts.html";
const string POSTS_DIR_OUTPUT = "./dist/posts";

const string PAGES_DIR = "./src/";
const string PAGES_DIR_OUTPUT = "./dist/";
const string MAIN_TEMPLATE = "./src/_templates/main.html";

const string TEMPLATES_DIR = "./src/_templates/";

const string ASSETS_DIR = "./src/assets/";
const string ASSETS_DIR_OUTPUT = "./dist/assets";

const string POSTS_PLACEHOLDER = "<!--#POSTS#-->";
const string BODY_PLACEHOLDER = "<!--#CONTENT#-->";

// Helpers
bool IsTemplate(string path) => Path.GetFileName(path).StartsWith('_');

string ParseAndSanitisePath(string path) => Path.GetFullPath(path).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

bool IsBuildPath(string path)
{
    var p = ParseAndSanitisePath(path);
    var posts = ParseAndSanitisePath(POSTS_DIR);
    var assets = ParseAndSanitisePath(ASSETS_DIR);
    var templates = ParseAndSanitisePath(TEMPLATES_DIR);

    return p.StartsWith(posts) || p.StartsWith(assets) || p.StartsWith(templates);
}

string ReplaceLeadingPathSegment(string path, string segment, string replacement)
{
    var dir = Path.GetDirectoryName(path)! + "\\";
    var trimmedDir = dir.Replace(segment, replacement);

    var fileName = Path.GetFileName(path);

    return Path.Combine(trimmedDir, fileName);
}

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

// Output pages;
// Includes both top-level html files,
// and also any nested files (preserving the path).
// Excludes /_templates/, /assets/, /posts/, which are handled elsewhere.
var pagesHtml = Directory
    .GetFiles(PAGES_DIR, "*.html", new EnumerationOptions
    {
        RecurseSubdirectories = true
    })
    .Where(f => !IsBuildPath(f))
    .Select(f => new
    {
        Name = Path.GetFileName(f),
        Content = File.ReadAllText(f),
        OutputDir = ReplaceLeadingPathSegment(f, ".\\src\\", ".\\"),
    })
    .ToList();

foreach (var page in pagesHtml)
{
    if (IsTemplate(page.Name))
        continue;

    Console.WriteLine($"Page: {page.Name}. Dir: {page.OutputDir}.");
    var pageOutput = mainTemplate.Replace(BODY_PLACEHOLDER, page.Content);

    // Special handling for the posts.html listing page
    if (page.Name == "posts.html")
    {
        var listing = string.Join(
            "\r\n",
            postsHtml.Select(p => $"<a href='posts/{p.Name}'>{Path.GetFileNameWithoutExtension(p.Name)}</a>"));

        pageOutput = pageOutput.Replace(POSTS_PLACEHOLDER, listing);
    }

    var outputFile = Path.Combine(PAGES_DIR_OUTPUT, page.OutputDir);

    // Ensure dir exists
    Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);

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
