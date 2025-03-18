using System;

namespace FileApp.Models
{
    public enum FileCategory { File, Folder }

    public class File
    {
        public string Name { get; }
        public FileCategory Category { get; }
        public long Size { get; }
        public string Path { get; }

        public File(string name, FileCategory category, long size, string path)
        {
            Name = name;
            Category = category;
            Size = category == FileCategory.Folder ? 0 : size;
            Path = path;
        }

        // Копирование файла
        public File Copy(string newPath) => new(Name, Category, Size, newPath);

        // Перемещение файла
        public File Move(string newPath) => new(Name, Category, Size, newPath);

        // Переименование файла
        public File Rename(string newName) => new(newName, Category, Size, Path);
    }
}