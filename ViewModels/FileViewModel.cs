using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Reactive;
using File = FileApp.Models.File;
using FileApp.Models;
using System.IO;

namespace FileApp.ViewModels
{
    public class FileViewModel : ReactiveObject
    {
        private string _name = string.Empty;
        private string _path = string.Empty;
        private string _newPath = string.Empty;
        private string _newName = string.Empty;
        private FileCategory _category;
        private long _size;
        private File? _selectedFile;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string NewName
        {
            get => _newName;
            set => this.RaiseAndSetIfChanged(ref _newName, value);
        }

        public string NewPath
        {
            get => _newPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _newPath, value);
            }
        }

        public FileCategory Category
        {
            get => _category;
            set
            {
                this.RaiseAndSetIfChanged(ref _category, value);
                if (value == FileCategory.Folder) Size = 0; // Если это папка, размер должен быть 0
            }
        }

        public long Size
        {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
        }

        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public bool IsSizeEnabled => Category == FileCategory.File; // Размер доступен только для файлов
        public Array Categories => Enum.GetValues(typeof(FileCategory)); // Список категорий (файл/папка)

        public AvaloniaList<File> Files { get; } = new(); // Коллекция файлов

        public File? SelectedFile
        {
            get => _selectedFile;
            set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
        }

        public ReactiveCommand<Unit, Unit> AddFileCommand { get; } // Команда для добавления файла
        public ReactiveCommand<Unit, Unit> CopyCommand { get; } // Команда для копирования
        public ReactiveCommand<Unit, Unit> MoveCommand { get; } // Команда для перемещения
        public ReactiveCommand<Unit, Unit> RenameCommand { get; } // Команда для переименования

        public FileViewModel()
        {
            AddFileCommand = ReactiveCommand.Create(AddFile);
            CopyCommand = ReactiveCommand.Create(Copy);
            MoveCommand = ReactiveCommand.Create(Move);
            RenameCommand = ReactiveCommand.Create(Rename);
        }

        private void AddFile()
        {
            Console.WriteLine($"AddFile called! Name: '{Name}', Path: '{Path}'");

            if (string.IsNullOrWhiteSpace(Name))
                Console.WriteLine("Name is empty!");

            if (string.IsNullOrWhiteSpace(Path))
                Console.WriteLine("Path is empty!");

            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Path))
            {
                Console.WriteLine("Validation failed!");
                return;
            }

            try
            {
                string fullPath = System.IO.Path.Combine(Path, Name);

                if (Category == FileCategory.Folder)
                {
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                        Console.WriteLine($"Folder created: {fullPath}");
                    }
                }
                else
                {
                    if (!System.IO.File.Exists(fullPath))
                    {
                        using (FileStream fs = System.IO.File.Create(fullPath))
                        {
                            fs.SetLength(Size);
                        }
                    }
                }

                var newFile = new FileApp.Models.File(Name, Category, Size, fullPath);
                Files.Add(newFile);
                Console.WriteLine($"File added to list: {newFile.Name} | {newFile.Path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }

            Name = string.Empty;
            Path = string.Empty;
            Size = 0;
        }

        private void Copy()
        {
            if (SelectedFile == null || string.IsNullOrWhiteSpace(NewPath))
            {
                Console.WriteLine("Copy failed: No file selected or new path is empty.");
                return;
            }

            string destinationPath = System.IO.Path.Combine(NewPath, SelectedFile.Name);

            try
            {
                if (SelectedFile.Category == FileCategory.Folder)
                {
                    CopyDirectory(SelectedFile.Path, destinationPath);
                }
                else
                {
                    System.IO.File.Copy(SelectedFile.Path, destinationPath);
                }

                var copiedFile = new FileApp.Models.File(SelectedFile.Name, SelectedFile.Category, SelectedFile.Size, destinationPath);
                Files.Add(copiedFile);

                Console.WriteLine($"Copied: {SelectedFile.Path} → {destinationPath}");
                NewPath = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Copy failed: {ex.Message}");
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = System.IO.Path.Combine(destinationDir, System.IO.Path.GetFileName(file));
                System.IO.File.Copy(file, destFile);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string destDir = System.IO.Path.Combine(destinationDir, System.IO.Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }


        private void Move()
        {
            if (SelectedFile == null || string.IsNullOrWhiteSpace(NewPath))
            {
                Console.WriteLine("Move failed: No file selected or new path is empty.");
                return;
            }

            string destinationPath = System.IO.Path.Combine(NewPath, SelectedFile.Name);

            try
            {
                if (SelectedFile.Category == FileCategory.Folder)
                {
                    Directory.Move(SelectedFile.Path, destinationPath);
                }
                else
                {
                    System.IO.File.Move(SelectedFile.Path, destinationPath);
                }

                var movedFile = new FileApp.Models.File(SelectedFile.Name, SelectedFile.Category, SelectedFile.Size, destinationPath);
                Files.Add(movedFile);
                Files.Remove(SelectedFile);

                Console.WriteLine($"Moved: {SelectedFile.Path} → {destinationPath}");
                NewPath = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Move failed: {ex.Message}");
            }
        }


        private void Rename()
        {
            if (SelectedFile == null || string.IsNullOrWhiteSpace(NewName))
            {
                Console.WriteLine("Rename failed: No file selected or new name is empty.");
                return;
            }

            string newFullPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(SelectedFile.Path)!, NewName);

            try
            {
                if (SelectedFile.Category == FileCategory.Folder)
                {
                    Directory.Move(SelectedFile.Path, newFullPath);
                }
                else
                {
                    System.IO.File.Move(SelectedFile.Path, newFullPath);
                }

                var renamedFile = new FileApp.Models.File(NewName, SelectedFile.Category, SelectedFile.Size, newFullPath);
                Files.Add(renamedFile);
                Files.Remove(SelectedFile);

                Console.WriteLine($"Renamed: {SelectedFile.Path} → {newFullPath}");
                NewName = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rename failed: {ex.Message}");
            }
        }
    }
}