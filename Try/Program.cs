using System.CommandLine;
using System.IO;

// Define options with their explanation
var outputOption = new Option<FileInfo?>("--output", "Output file path and name");
var languageOption = new Option<string>("--language", "Which type of file to bundle - mandatory");
var noteOption = new Option<bool>("--note", "Addition full path and name file as a note");
var sortOption = new Option<bool>("--sort", "Sort the text in file by file name or by file ending");
var removeEmptyLinesOption = new Option<bool>("--removeEmptyLines", "remove the empty lines on the file before the copy to bundle file");
var authorOption = new Option<string>("--author", "add name author of the new file");
//Define alias for the options
outputOption.AddAlias("-o");
languageOption.AddAlias("-l");
noteOption.AddAlias("-n");
sortOption.AddAlias("-s");
removeEmptyLinesOption.AddAlias("-rel");
authorOption.AddAlias("-a");

// Define supported file types for correctness
string[] types = new string[] { "c", "cs", "cpp", "css", "html", "py", "java", "js", "json", "jsx", "txt", "php", "sql", "all" };
string[] ends = new string[] { ".c", ".cs", ".cpp", ".css", ".html", ".py", ".java", ".js", ".json", ".jsx", ".txt", ".php", ".sql", "all" };

// Define commeand
var bundleCmd = new Command("bundle", "Bundle all files into a single file.");
// Add to command the options
bundleCmd.AddOption(outputOption);
bundleCmd.AddOption(languageOption);
bundleCmd.AddOption(noteOption);
bundleCmd.AddOption(sortOption);
bundleCmd.AddOption(removeEmptyLinesOption);
bundleCmd.AddOption(authorOption);

// On runing bundle command:
// The command receive all the params from the options that the command can receive.
bundleCmd.SetHandler((FileInfo output, string language, bool note = false, bool sort = false,
    bool removeEmptyLines = false, string author = "") =>
{
    // Define defult values if don't was received
    output ??= new FileInfo("default_output.txt");
    author ??= "";
    if (language != null)
    {
        if (types.Contains(language))
        {
            //Change it to extension file
            int index = Array.IndexOf(types, language);
            language = ends[index];
        }
        else
        {
            // print an exception
            Console.WriteLine($"this language:{language} is invald");
        }
        try
        {
            // Create file
            using (StreamWriter writer = new StreamWriter(output.FullName))
            {
                // If what auther
                if (author != "")
                    // Write auther name
                    writer.WriteLine($"--------------{author}-------------\n");
                // Full path from new file
                string fullPath = output.FullName;
                // Full path the directory from new file
                string directoryPath = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
                // Arrey with all paths from files in directory
                string[] files = Directory.GetFiles(directoryPath);
                string extensionFile;
                // Whated sort
                if (sort == true)
                {
                    // Sort the arrey files by extension path
                    var sortedFilePaths = files.OrderBy(path => Path.GetExtension(path)).ToArray();
                    for (int i = 0; i < sortedFilePaths.Length; i++)
                        files[i] = Path.GetFullPath(Path.GetFileName(sortedFilePaths[i]));
                }
                // Past of all files
                foreach (string file in files)
                {
                    // If file extension equal to the language requst and the file not himself.
                    if ((language.Equals("all") || Path.GetExtension(file) == language) && file != fullPath)
                    {
                        // If the requst is note 
                        if (note != false)
                        {
                            // According the type of file, is the type of the note
                            extensionFile = Path.GetExtension(file);
                            if (extensionFile.Equals(".c") || extensionFile.Equals(".cpp") || extensionFile.Equals(".cs")
                            || extensionFile.Equals(".java") || extensionFile.Equals(".js") || extensionFile.Equals(".json")
                            || extensionFile.Equals(".jsx"))
                                writer.WriteLine("//" + Path.GetFullPath(file));
                            else if (extensionFile.Equals(".py") || extensionFile.Equals(".php"))
                                writer.WriteLine("#" + Path.GetFullPath(file));
                            else if (extensionFile.Equals(".sql"))
                                writer.WriteLine("--" + Path.GetFullPath(file));
                            else if (extensionFile.Equals(".css"))
                                writer.WriteLine("/*" + Path.GetFullPath(file) + "*/");
                            else if (extensionFile.Equals(".html"))
                                writer.WriteLine("<!--" + Path.GetFullPath(file) + "-->");

                        }
                        // If the requst is remove empty lines
                        if (removeEmptyLines == true)
                        {
                            string line;
                            //open the file
                            using (StreamReader reader = new StreamReader(file))
                                // past of all lines in file
                                while ((line = reader.ReadLine()) != null)
                                    // check if is not empty and wirte to the new file
                                    if (!string.IsNullOrWhiteSpace(line))
                                        writer.WriteLine(line);
                        }
                        // If the requst is not remove empty lines
                        else
                            // wirte the file to the new file
                            writer.Write("\n" + File.ReadAllText(file) + "\n");
                    }
                }
            }
            // print the full path from the new file 
            Console.WriteLine($"Finished writing to file: {output.FullName}");
        }
        // else catch the exception and print it.
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    else
        Console.WriteLine("Error: you mast to choose a type of languae");

    // Dfine all Options of bundle
}, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

var createRspCommand = new Command("create-rsp", "create a file with that have all params and commands and option to on the bundle command like what the user want");

createRspCommand.SetHandler(() =>
{
    string nameRSPFile = "response_file.rsp";
    using (StreamWriter writer = new StreamWriter(nameRSPFile))
    {
        writer.Write(bundleCmd.Name + " ");
        Console.WriteLine("you want choose the name of the bundle file? (enter: y-yes, n-no):");
        string want = Console.ReadLine();
        if (want == "y")
        {
            Console.WriteLine("enter the name of the bundle file (default: default_output.txt):");
            string nameBundleFile = Console.ReadLine();
            if (nameBundleFile != "")
                if (nameBundleFile.Contains('.'))
                    writer.Write(" --" + outputOption.Name + " " + nameBundleFile);
                else
                {
                    Console.WriteLine("the name don't contains type of file\nenter type of file");
                    string type = Console.ReadLine();
                    if (types.Contains(type) && type != "all" || ends.Contains(type) && type != "all")
                    {
                        if (type.Contains('.'))
                        {
                            nameBundleFile = nameBundleFile + type;
                        }
                        else
                            nameBundleFile = nameBundleFile + "." + type;

                        writer.Write(" --" + outputOption.Name + " " + nameBundleFile);
                    }
                    else
                    {
                        nameBundleFile = nameBundleFile + ".txt";
                        writer.Write(" --" + outputOption.Name + " " + nameBundleFile);
                        Console.WriteLine("the type of file is invalid the program useing default type -'txt' ");
                    }
                }
            else
                Console.WriteLine("the name of the file is invalid the program useing default file name");
        }
        else if (want != "n")
            Console.WriteLine("your answer is invalid the program use the default value without this option");
        else
            Console.WriteLine("the program use the default value");
        Console.WriteLine("enter the name language to bundle for all the language enter 'all':");
        string languaeName = Console.ReadLine();
        if (types.Contains(languaeName) || ends.Contains(languaeName))
        {
            if (ends.Contains(languaeName) && languaeName != "all")
                languaeName = languaeName.Substring(1);
            writer.Write(" --" + languageOption.Name + " " + languaeName);
            Console.WriteLine("enter true/false if you want note in the bundle file (default: false):");
            string isNote = Console.ReadLine();
            if (isNote == "true" || isNote == "false")
            {
                bool note = bool.Parse(isNote);
                writer.Write(" --" + noteOption.Name + " " + note);
            }
            else
                Console.WriteLine("the answer of the note is invalid the program useing default value: false");
            Console.WriteLine("enter true/false if you want sort by types file on the bundle file (default: false):");
            string isSort = Console.ReadLine();
            if (isSort == "true" || isSort == "false")
            {
                bool note = bool.Parse(isSort);
                writer.Write(" --" + sortOption.Name + " " + isSort);
            }
            else
                Console.WriteLine("the answer of the sort is invalid the program useing default value: false");
            Console.WriteLine("enter true/false if you want remove empty lines on the bundle file (default: false):");
            string isRemoveEmptyLines = Console.ReadLine();
            if (isRemoveEmptyLines == "true" || isRemoveEmptyLines == "false")
            {
                bool note = bool.Parse(isRemoveEmptyLines);
                writer.Write(" --" + removeEmptyLinesOption.Name + " " + isRemoveEmptyLines);
            }
            else
                Console.WriteLine("the answer of the remove empty list is invalid the program useing default value: false");
            Console.WriteLine("you want choose the a name of auther to the bundle file? (enter: y-yes, n-no):");
            want = Console.ReadLine();
            if (want == "y")
            {
                Console.WriteLine("enter the author name to the bundle file (default: ' '):");
                string authorName = Console.ReadLine();
                if (authorName != "")
                    writer.Write(" --" + authorOption.Name + " " + authorName);
                else
                    Console.WriteLine("you don't choose the auther name the program useing default -' ' ");
            }
            else if (want != "n")
                Console.WriteLine("your answer is invalid the program use the default value without this option");
            else
                Console.WriteLine("the program use the default value");
            Console.WriteLine("the response file is ready to running\nrun this command to start the bundle: fib @response_file.rsp");
            writer.Close();
        }
        else if (languaeName == null || languaeName == "")
        {
            Console.WriteLine("Error: you mast to choose a type of languae");
            writer.Close();
            File.Delete(nameRSPFile);
            Console.WriteLine("the response file create failed.\nbecouse you mast input type of language to bundle.");
        }
        else
        {
            Console.WriteLine("the name language of the file is invalid");
            writer.Close();
            File.Delete(nameRSPFile);
            Console.WriteLine("the response file create failed.\nbecouse you mast input type of language to bundle.");
        }
    }
});

// Add command to root
var rootCommand = new RootCommand("Root Command for this project");
rootCommand.AddCommand(bundleCmd);
rootCommand.AddCommand(createRspCommand);

await rootCommand.InvokeAsync(args);
