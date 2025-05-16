using System.Text.Json;

namespace DeskSwap
{
    public static class Commands
    {
        public static void CreateProfile(string name, string description, bool writeTips)
        {
            if (name == "default")
            {
                Console.WriteLine("\t\tSorry, but you can't create a profile with the name 'default'.");
                Console.WriteLine("\t\tIf you want to update the default profile, you can use `setup`.\n");
                return;
            }

            Utilities.GetDesktopItems(out var folders, out var files);
            var profile = new DeskSwapProfile(name, description, folders, files, new Dictionary<string, string>());
            foreach (var folder in folders)
            {
                profile.OriginalPaths.Add(folder, Path.GetDirectoryName(folder) ?? string.Empty);
            }

            foreach (var file in files)
            {
                profile.OriginalPaths.Add(file, Path.GetDirectoryName(file) ?? string.Empty);
            }

            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            var profilePath = Path.Combine(Paths.ProfilesPath, $"{name}.json");
            File.WriteAllText(profilePath, json);
            if (!writeTips) return;
            Console.WriteLine($"\t\tProfile {name} created successfully!");
            Console.WriteLine("\t\tYou can use `load` to load this profile later.");
            Console.WriteLine("\t\tYou can also use `edit` to edit this profile\n");
            Console.WriteLine($"Tip: The profile is saved in {Paths.DocumentsPath} as {name}.json\n\n");
        }

        public static void EditProfile(string name)
        {
            if (name == "default")
            {
                Console.WriteLine("\t\tSorry, but you can't edit the default profile.");
                Console.WriteLine("\t\tIf you want to update the default profile, you can use `setup`.\n");
                return;
            }

            var profilePath = Path.Combine(Paths.ProfilesPath, $"{name}.json");
            if (!File.Exists(profilePath))
            {
                Console.WriteLine($"\t\tAh sorry, but the profile {name} does not exist.");
                Console.WriteLine("\t\tMaybe you have misspelled the name or it was deleted?");
                Console.WriteLine("\t\tYou can use `list` to see all the profiles you have.\n\n");
                return;
            }

            var profileData = File.ReadAllText(profilePath);
            var profileObj = JsonSerializer.Deserialize<DeskSwapProfile>(profileData);
            string[] folders = profileObj?.Folders ?? [];
            string[] files = profileObj?.Files ?? [];
            string[] allItems = folders.Concat(files).ToArray();
            Console.WriteLine($"\t\t===Items in {name}===");
            for (int i = 0; i < allItems.Length; i++)
            {
                Console.WriteLine($"\t\t{i}- {allItems[i]}");
            }

            Console.WriteLine("\t\t===End of items===\n");
            Console.WriteLine("\t\tYou can use the following commands:");
            Console.WriteLine("\t\t- add <path> : Add a new item to the profile");
            Console.WriteLine("\t\t- remove <index> : Remove an item from the profile");
            Console.WriteLine("\t\t- quit : Quit the editor\n");
            while (true)
            {
                Console.Write("\t\t> ");
                var input = Console.ReadLine() ?? string.Empty;
                if (input == "quit")
                {
                    break;
                }

                if (input.StartsWith("remove"))
                {
                    var index = input.Split(" ")[1];
                    if (int.TryParse(index, out int indexInt))
                    {
                        if (indexInt < 0 || indexInt >= allItems.Length)
                        {
                            Console.WriteLine("\t\tInvalid index.");
                            continue;
                        }

                        var item = allItems[indexInt];
                        if (folders.Contains(item))
                        {
                            folders = folders.Where(x => x != item).ToArray();
                        }
                        else if (files.Contains(item))
                        {
                            files = files.Where(x => x != item).ToArray();
                        }
                        else
                        {
                            Console.WriteLine("\t\tInvalid item.");
                            continue;
                        }
                    }
                }

                if (input.StartsWith("add"))
                {
                    var path = input.Split(" ")[1];
                    if (!path.StartsWith(Paths.DesktopPath))
                    {
                        Console.WriteLine("\t\tInvalid path, it should be on the desktop.");
                        continue;
                    }

                    if (File.Exists(path))
                    {
                        files = files.Append(path).ToArray();
                    }
                    else if (Directory.Exists(path))
                    {
                        folders = folders.Append(path).ToArray();
                    }
                    else
                    {
                        Console.WriteLine("\t\tInvalid path, it should be a file or a folder.");
                    }
                }
            }

            var newProfile = new DeskSwapProfile(name, profileObj?.Description ?? "", folders, files,
                profileObj?.OriginalPaths ?? new Dictionary<string, string>());
            var newJson = JsonSerializer.Serialize(newProfile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(profilePath, newJson);
            Console.WriteLine("\t\tDone! Your profile has been updated.");
            Utilities.WriteHeader();
        }

        public static void DeleteProfile(string name)
        {
            var profilePath = Path.Combine(Paths.ProfilesPath, $"{name}.json");
            if (!File.Exists(profilePath))
            {
                Console.WriteLine($"\t\tAh sorry, but the profile {name} does not exist.");
                Console.WriteLine("\t\tMaybe you have misspelled the name or it was deleted?");
                Console.WriteLine("\t\tYou can use `list` to see all the profiles you have.\n\n");
                return;
            }

            File.Delete(profilePath);
            Console.WriteLine($"\t\tProfile {name} deleted successfully!");
        }

        public static void ListProfiles()
        {
            var profiles = Directory.GetFiles(Paths.ProfilesPath, "*.json");
            if (profiles.Length == 0)
            {
                Console.WriteLine("\t\tYou don't have any profiles yet.");
                Console.WriteLine("\t\tYou can create a new profile using `profile` command.\n");
                return;
            }

            Console.WriteLine("\t\tHere are your profiles:");
            foreach (var profile in profiles)
            {
                var profileData = File.ReadAllText(profile);
                var profileObj = JsonSerializer.Deserialize<DeskSwapProfile>(profileData);
                if (profileObj == null)
                {
                    Console.WriteLine(
                        $"\t\t- {Path.GetFileNameWithoutExtension(profile)} | [ERROR] Could not load profile.");
                    continue;
                }

                Console.WriteLine($"\t\t- {profileObj.Name} : {profileObj.Description}");

            }
            Console.WriteLine("\n\t\tTip : You can use `load` to load a profile.\n");
        }

        public static void CreateTempDump()
        {
            Utilities.GetDesktopItems(out var folders, out var files);
            if (!Directory.Exists(Paths.TempDumpPath))
            {
                Directory.CreateDirectory(Paths.TempDumpPath);
            }

            foreach (var folder in folders)
            {
                var folderName = Path.GetFileName(folder);
                var newFolderPath = Path.Combine(Paths.TempDumpPath, folderName);
                Directory.Move(folder, newFolderPath);
            }

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var newFilePath = Path.Combine(Paths.TempDumpPath, fileName);
                File.Move(file, newFilePath);
            }

            Console.WriteLine("\t\tDone! Your desktop is now empty.");
            Console.WriteLine("\t\tYou can use `restore` to restore your desktop state.\n");
        }

        public static void RestoreTempDump()
        {
            if (!Directory.Exists(Paths.TempDumpPath))
            {
                Console.WriteLine("\t\tYou don't have any temp dump yet.");
                Console.WriteLine("\t\tYou can create a new temp dump using `dump` command.\n");
                return;
            }

            var folders = Directory.GetDirectories(Paths.TempDumpPath);
            var files = Directory.GetFiles(Paths.TempDumpPath);
            foreach (var folder in folders)
            {
                var folderName = Path.GetFileName(folder);
                var newFolderPath = Path.Combine(Paths.DesktopPath, folderName);
                Directory.Move(folder, newFolderPath);

            }

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var newFilePath = Path.Combine(Paths.DesktopPath, fileName);
                File.Move(file, newFilePath);
            }

            Directory.Delete(Paths.TempDumpPath, true);
            Console.WriteLine("\t\tDone! Your desktop is now restored.");
        }

        public static void LoadProfile(string name)
        {
            var profilePath = Path.Combine(Paths.ProfilesPath, $"{name}.json");
            var defaultProfilePath = Path.Combine(Paths.ProfilesPath, "Default.json");

            if (!File.Exists(profilePath))
            {
                Console.WriteLine($"\t\tAh sorry, but the profile {name} does not exist.");
                Console.WriteLine("\t\tMaybe you have misspelled the name or it was deleted?");
                Console.WriteLine("\t\tYou can use `list` to see all the profiles you have.\n\n");
                return;
            }

            var profileData = File.ReadAllText(profilePath);
            var profileObj = JsonSerializer.Deserialize<DeskSwapProfile>(profileData);

            var defaultProfileData = File.ReadAllText(defaultProfilePath);
            var defaultProfileObj = JsonSerializer.Deserialize<DeskSwapProfile>(defaultProfileData);

            if (profileObj == null || defaultProfileObj == null)
            {
                Console.WriteLine("\t\t[ERROR] Profile is found but could not be loaded.");
                Console.WriteLine("\t\tMaybe the file is corrupted or not a valid profile.");
                return;
            }

            // Get current desktop items
            Utilities.GetDesktopItems(out var currentFolders, out var currentFiles);

            var foldersToRemove = currentFolders.Where(f => !profileObj.Folders.Contains(f)).ToArray();
            var filesToRemove = currentFiles.Where(f => !profileObj.Files.Contains(f)).ToArray();

            var foldersToAdd = profileObj.Folders.Where(f => !currentFolders.Contains(f)).ToArray();
            var filesToAdd = profileObj.Files.Where(f => !currentFiles.Contains(f)).ToArray();

            if (!Directory.Exists(Paths.DumpPath))
            {
                Directory.CreateDirectory(Paths.DumpPath);
            }

            foreach (var folder in foldersToAdd)
            {
                var folderName = Path.GetFileName(folder);
                var folderPath = Path.Combine(Paths.DumpPath, folderName);
                var newFolderPath = defaultProfileObj.OriginalPaths[folder];
                try
                {
                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.Move(folderPath, newFolderPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\t\t[ERROR] Could not move folder {folderName}: {ex.Message}");
                }
            }

            foreach (var file in filesToAdd)
            {
                var fileName = Path.GetFileName(file);
                Console.WriteLine("[DEBUG] File name: " + fileName);
                var filePath = Path.Combine(Paths.DumpPath, fileName);
                Console.WriteLine("[DEBUG] File path: " + filePath);
                var newFilePath = Path.Combine(defaultProfileObj.OriginalPaths[file], fileName);
                Console.WriteLine("[DEBUG] New file path: " + newFilePath);
                try
                {
                    if (!File.Exists(newFilePath))
                    {
                        File.Move(filePath, newFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\t\t[ERROR] Could not move file {fileName}: {ex.Message}");
                }
            }


            foreach (var folder in foldersToRemove)
            {
                var folderName = Path.GetFileName(folder);
                var newFolderPath = Path.Combine(Paths.DumpPath, folderName);
                try
                {
                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.Move(folder, newFolderPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\t\t[ERROR] Could not move folder {folderName}: {ex.Message}");
                }
            }

            foreach (var file in filesToRemove)
            {
                var fileName = Path.GetFileName(file);
                var newFilePath = Path.Combine(Paths.DumpPath, fileName);
                try
                {
                    if (!File.Exists(newFilePath))
                    {
                        File.Move(file, newFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\t\t[ERROR] Could not move file {fileName}: {ex.Message}");
                }
            }

        }

        public static void Setup()
        {
            Console.WriteLine("\t\t Before we setup your profile, we need to make sure you have everything set up.\n\n");
            Console.WriteLine("\t\t-Make sure you have all of your files, folders and shortcuts on your desktop.\n");
            Console.WriteLine("\t\tOnce that's done, press any key...\n\n");
            Console.ReadKey();
            CreateProfile("Default", "Default profile created by DeskSwap", false);
            Utilities.WriteHeader();
            Console.WriteLine("\t\tDone! Your default profile has been setup.");
        }
    }
}