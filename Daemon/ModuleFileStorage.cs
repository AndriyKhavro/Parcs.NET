using Parcs;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;

namespace DaemonPr
{
    internal class ModuleFileStorage
    {
        private static readonly object _syncRoot = new object();

        public string SaveFile(byte[] file)
        {
            string fileName = GetHashedFileName(file);

            using (IsolatedStorageFile userFile = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (!userFile.FileExists(fileName))
                {
                    lock (_syncRoot)
                    {
                        if (!userFile.FileExists(fileName))
                        {
                            using (IsolatedStorageFileStream isolatedStream = new IsolatedStorageFileStream(fileName, FileMode.Create, userFile))
                            {
                                using (BinaryWriter writer = new BinaryWriter(isolatedStream))
                                {
                                    writer.Write(file);
                                    writer.Close();
                                    isolatedStream.Close();
                                }
                            }
                        }
                    }
                }
            }

            return fileName;
        }

        public byte[] ReadFile(string fileName)
        {
            using (IsolatedStorageFile userFile = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (!userFile.FileExists(fileName))
                {
                    throw new ParcsException($"File is not found: {fileName}");
                }

                using (IsolatedStorageFileStream stream = userFile.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                {
                    if (stream.Length > int.MaxValue)
                    {
                        throw new ParcsException($"File is too large: {fileName}");
                    }

                    var file = new byte[stream.Length];

                    stream.Read(file, 0, (int)stream.Length);

                    stream.Close();

                    return file;
                }
            }
        }

        public void DeleteFileIfExists(string fileName)
        {
            using (IsolatedStorageFile userFile = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (userFile.FileExists(fileName))
                {
                    userFile.DeleteFile(fileName);
                }
            }                
        }

        private string GetHashedFileName(byte[] file)
        {
            return ByteArrayToString(GetHash(file));
        }

        private byte[] GetHash(byte[] data)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                return sha1.ComputeHash(data);
            }
        }

        private string ByteArrayToString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}
